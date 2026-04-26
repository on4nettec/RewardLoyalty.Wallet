using On4Net.Extensions.Exception;
using Wallet.Core.Api.Events;
using Wallet.Core.Api.Helper;
using Wallet.Core.Data.Repositories;
using Wallet.Core.Model.Constants;
using Wallet.Core.Model.Entities;
using Wallet.Core.Model.Enum;
using Wallet.Core.Model.Request;
using Wallet.Core.Model.Response;

namespace Wallet.Core.Api.Services;

public class WalletLedgerService : IWalletLedgerService
{
    private const int MaxOptimisticRetries = 8;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _transactionRepository;
    private readonly ITransactionDetailRepository _detailRepository;
    private readonly ISettlementRequestRepository _settlementRepository;
    private readonly IWalletEventPublisher _eventPublisher;
    private readonly Func<DateTime> _utcNow;

    public WalletLedgerService(
        IWalletRepository walletRepository,
        IWalletTransactionRepository transactionRepository,
        ITransactionDetailRepository detailRepository,
        ISettlementRequestRepository settlementRepository,
        IWalletEventPublisher eventPublisher,
        Func<DateTime> utcNow)
    {
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _detailRepository = detailRepository;
        _settlementRepository = settlementRepository;
        _eventPublisher = eventPublisher;
        _utcNow = utcNow;
    }

    public async Task<WalletBalanceResponse> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wallet = await EnsureWalletAsync(userId, cancellationToken);
        return MapBalance(wallet);
    }

    public async Task<DepositResponse> DepositAsync(Guid userId, DepositRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new DataValidationException(ErrorCodes.WalletAmountInvalid);
        }

        if (!string.IsNullOrWhiteSpace(request.ReferenceType) && request.ReferenceId.HasValue)
        {
            var existing = await _transactionRepository.FindCompletedMainDepositByReferenceAsync(
                userId,
                request.ReferenceType.Trim(),
                request.ReferenceId.Value,
                cancellationToken);
            if (existing != null)
            {
                var w = await EnsureWalletAsync(userId, cancellationToken);
                return new DepositResponse
                {
                    TransactionId = existing.Id,
                    Wallet = MapBalance(w),
                    IdempotentReplay = true
                };
            }
        }

        var description = string.IsNullOrWhiteSpace(request.Description)
            ? "واریز به کیف پول اصلی"
            : request.Description.Trim();

        for (var attempt = 0; attempt < MaxOptimisticRetries; attempt++)
        {
            var wallet = await EnsureWalletAsync(userId, cancellationToken);
            ThrowIfWalletLocked(wallet);

            var balanceBefore = wallet.MainBalance;
            var balanceAfter = balanceBefore + request.Amount;

            var tx = new WalletTransaction
            {
                UserId = userId,
                WalletType = (short)WalletType.Main,
                TransactionType = TransactionTypeCodes.MainWalletDeposit,
                ReferenceId = request.ReferenceId,
                ReferenceType = request.ReferenceType?.Trim(),
                Debit = 0,
                Credit = request.Amount,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                Description = description,
                TransactionStatus = (short)TransactionWorkflowStatus.Completed,
                CompletedAt = _utcNow(),
                IsExpired = false
            };

            await _transactionRepository.InsertAsync(tx, cancellationToken);

            if (request.Details is { Count: > 0 })
            {
                foreach (var d in request.Details)
                {
                    await _detailRepository.InsertAsync(
                        new TransactionDetail
                        {
                            TransactionId = tx.Id,
                            Key = d.Key,
                            Value = d.Value
                        },
                        cancellationToken);
                }
            }

            wallet.MainBalance = balanceAfter;
            wallet.TotalMainDeposited += request.Amount;

            var updated = await _walletRepository.TryUpdateAsync(wallet, wallet.Version, cancellationToken);
            if (updated)
            {
                await _eventPublisher.PublishWalletDepositedAsync(userId, tx, cancellationToken);
                return new DepositResponse
                {
                    TransactionId = tx.Id,
                    Wallet = MapBalance(await _walletRepository.GetByUserIdAsync(userId, cancellationToken) ?? wallet),
                    IdempotentReplay = false
                };
            }
        }

        throw new DataValidationException(ErrorCodes.WalletConcurrentUpdate);
    }

    public async Task<WithdrawResponse> WithdrawAsync(Guid userId, WithdrawRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new DataValidationException(ErrorCodes.WalletAmountInvalid);
        }

        var description = string.IsNullOrWhiteSpace(request.Description)
            ? "برداشت از کیف پول اصلی"
            : request.Description.Trim();

        for (var attempt = 0; attempt < MaxOptimisticRetries; attempt++)
        {
            var wallet = await EnsureWalletAsync(userId, cancellationToken);
            ThrowIfWalletLocked(wallet);

            if (wallet.MainBalance < request.Amount)
            {
                throw new DataValidationException(ErrorCodes.WalletInsufficientFunds);
            }

            var balanceBefore = wallet.MainBalance;
            var balanceAfter = balanceBefore - request.Amount;

            var tx = new WalletTransaction
            {
                UserId = userId,
                WalletType = (short)WalletType.Main,
                TransactionType = TransactionTypeCodes.MainWalletWithdraw,
                Debit = request.Amount,
                Credit = 0,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                Description = description,
                TransactionStatus = (short)TransactionWorkflowStatus.Completed,
                CompletedAt = _utcNow(),
                IsExpired = false
            };

            await _transactionRepository.InsertAsync(tx, cancellationToken);

            wallet.MainBalance = balanceAfter;
            wallet.TotalMainWithdrawn += request.Amount;

            var updated = await _walletRepository.TryUpdateAsync(wallet, wallet.Version, cancellationToken);
            if (updated)
            {
                await _eventPublisher.PublishWalletWithdrawnAsync(userId, tx, cancellationToken);
                return new WithdrawResponse
                {
                    TransactionId = tx.Id,
                    Wallet = MapBalance(await _walletRepository.GetByUserIdAsync(userId, cancellationToken) ?? wallet)
                };
            }
        }

        throw new DataValidationException(ErrorCodes.WalletConcurrentUpdate);
    }

    public Task<PagedTransactionsResult> ListTransactionsAsync(
        Guid userId,
        TransactionSearchRequest request,
        CancellationToken cancellationToken = default) =>
        _transactionRepository.SearchAsync(userId, request, cancellationToken);

    public async Task<TransactionDetailResponse> GetTransactionAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        var tx = await _transactionRepository.GetByIdForUserAsync(userId, transactionId, cancellationToken);
        if (tx == null)
        {
            throw new NotFoundException(ErrorCodes.WalletTransactionNotFound);
        }

        var details = await _detailRepository.ListByTransactionIdAsync(tx.Id, cancellationToken);
        return new TransactionDetailResponse
        {
            Id = tx.Id,
            WalletType = tx.WalletType,
            TransactionType = tx.TransactionType,
            ReferenceId = tx.ReferenceId,
            ReferenceType = tx.ReferenceType,
            Debit = tx.Debit,
            Credit = tx.Credit,
            BalanceBefore = tx.BalanceBefore,
            BalanceAfter = tx.BalanceAfter,
            Description = tx.Description,
            TransactionStatus = tx.TransactionStatus,
            ExpiresAt = tx.ExpiresAt,
            IsExpired = tx.IsExpired,
            CompletedAt = tx.CompletedAt,
            CreatedAt = tx.CreatedAt,
            Details = details.Select(d => new TransactionDetailItemResponse { Key = d.Key, Value = d.Value }).ToList()
        };
    }

    public async Task<WalletSummaryResponse> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var wallet = await EnsureWalletAsync(userId, cancellationToken);
        var totals = await _transactionRepository.GetCompletedTotalsForUserAsync(userId, cancellationToken);
        return new WalletSummaryResponse
        {
            MainBalance = wallet.MainBalance,
            CashbackBalance = wallet.CashbackBalance,
            SumCompletedCredits = totals.SumCredit,
            SumCompletedDebits = totals.SumDebit,
            CompletedTransactionCount = totals.Count
        };
    }

    public async Task<SettlementSubmitResponse> RequestSettlementAsync(
        Guid userId,
        SettlementSubmitRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new DataValidationException(ErrorCodes.WalletAmountInvalid);
        }

        for (var attempt = 0; attempt < MaxOptimisticRetries; attempt++)
        {
            var wallet = await EnsureWalletAsync(userId, cancellationToken);
            ThrowIfWalletLocked(wallet);

            if (wallet.MainBalance < request.Amount)
            {
                throw new DataValidationException(ErrorCodes.WalletInsufficientFunds);
            }

            var balanceBefore = wallet.MainBalance;
            var balanceAfter = balanceBefore - request.Amount;

            var settlement = request.ToPendingSettlementRecord(userId);

            await _settlementRepository.InsertAsync(settlement, cancellationToken);

            var tx = new WalletTransaction
            {
                UserId = userId,
                WalletType = (short)WalletType.Main,
                TransactionType = TransactionTypeCodes.BankSettlement,
                ReferenceId = settlement.Id,
                ReferenceType = "SettlementRequest",
                Debit = request.Amount,
                Credit = 0,
                BalanceBefore = balanceBefore,
                BalanceAfter = balanceAfter,
                Description = "درخواست تسویه به بانک",
                TransactionStatus = (short)TransactionWorkflowStatus.Completed,
                CompletedAt = _utcNow(),
                IsExpired = false
            };

            await _transactionRepository.InsertAsync(tx, cancellationToken);

            wallet.MainBalance = balanceAfter;
            wallet.TotalMainWithdrawn += request.Amount;

            var updated = await _walletRepository.TryUpdateAsync(wallet, wallet.Version, cancellationToken);
            if (updated)
            {
                await _eventPublisher.PublishSettlementRequestedAsync(userId, settlement, cancellationToken);
                return new SettlementSubmitResponse
                {
                    SettlementRequestId = settlement.Id,
                    TransactionId = tx.Id,
                    Wallet = MapBalance(await _walletRepository.GetByUserIdAsync(userId, cancellationToken) ?? wallet)
                };
            }
        }

        throw new DataValidationException(ErrorCodes.WalletConcurrentUpdate);
    }

    public async Task<SettlementRequestResponse> ApproveSettlementAsync(
        Guid adminUserId,
        Guid settlementId,
        CancellationToken cancellationToken = default)
    {
        var row = await _settlementRepository.GetByIdAsync(settlementId, cancellationToken);
        if (row == null)
        {
            throw new NotFoundException(ErrorCodes.WalletSettlementNotFound);
        }

        if (row.RequestStatus != (short)SettlementRequestStatus.Pending)
        {
            throw new DataValidationException(ErrorCodes.WalletSettlementInvalidState);
        }

        var ok = await _settlementRepository.TryUpdateRequestStatusAsync(
            settlementId,
            (short)SettlementRequestStatus.Approved,
            (short)SettlementRequestStatus.Pending,
            adminUserId.ToString(),
            cancellationToken);

        if (!ok)
        {
            throw new DataValidationException(ErrorCodes.WalletConcurrentUpdate);
        }

        var updated = await _settlementRepository.GetByIdAsync(settlementId, cancellationToken);
        if (updated != null)
        {
            await _eventPublisher.PublishSettlementApprovedAsync(updated.UserId, updated, cancellationToken);
        }

        return MapSettlement(updated!);
    }

    public async Task<IReadOnlyList<SettlementRequestResponse>> ListSettlementStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var rows = await _settlementRepository.ListByUserIdAsync(userId, cancellationToken);
        return rows.Select(MapSettlement).ToList();
    }

    private async Task<ConsumerWallet> EnsureWalletAsync(Guid userId, CancellationToken cancellationToken)
    {
        var existing = await _walletRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existing != null)
        {
            return existing;
        }

        return await _walletRepository.InsertAsync(
            new ConsumerWallet
            {
                UserId = userId,
                MainBalance = 0,
                CashbackBalance = 0,
                TotalMainDeposited = 0,
                TotalMainWithdrawn = 0,
                TotalCashbackReceived = 0,
                TotalCashbackSpent = 0,
                IsLocked = false,
                CreatedBy = "SYSTEM",
                ModifiedBy = "SYSTEM"
            },
            cancellationToken);
    }

    private static void ThrowIfWalletLocked(ConsumerWallet wallet)
    {
        if (!wallet.IsLocked)
        {
            return;
        }

        if (wallet.LockedUntil == null || wallet.LockedUntil > DateTime.UtcNow)
        {
            throw new DataValidationException(ErrorCodes.WalletLocked);
        }
    }

    private static WalletBalanceResponse MapBalance(ConsumerWallet w) =>
        new()
        {
            WalletId = w.Id,
            MainBalance = w.MainBalance,
            CashbackBalance = w.CashbackBalance,
            TotalMainDeposited = w.TotalMainDeposited,
            TotalMainWithdrawn = w.TotalMainWithdrawn,
            TotalCashbackReceived = w.TotalCashbackReceived,
            TotalCashbackSpent = w.TotalCashbackSpent,
            IsLocked = w.IsLocked,
            LockedUntil = w.LockedUntil
        };

    private static SettlementRequestResponse MapSettlement(SettlementRecord r) =>
        new()
        {
            Id = r.Id,
            Amount = r.Amount,
            BankAccountId = r.BankAccountId,
            RequestStatus = r.RequestStatus,
            InvoiceUrl = r.InvoiceUrl,
            PaymentSlipUrl = r.PaymentSlipUrl,
            LockedAmount = r.LockedAmount,
            CompletedAt = r.CompletedAt,
            CreatedAt = r.CreatedAt
        };
}
