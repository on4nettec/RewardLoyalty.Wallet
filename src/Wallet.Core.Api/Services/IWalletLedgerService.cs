using Wallet.Core.Model.Request;
using Wallet.Core.Model.Response;

namespace Wallet.Core.Api.Services;

public interface IWalletLedgerService
{
    Task<WalletBalanceResponse> GetBalanceAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<DepositResponse> DepositAsync(Guid userId, DepositRequest request, CancellationToken cancellationToken = default);

    Task<WithdrawResponse> WithdrawAsync(Guid userId, WithdrawRequest request, CancellationToken cancellationToken = default);

    Task<PagedTransactionsResult> ListTransactionsAsync(
        Guid userId,
        TransactionSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<TransactionDetailResponse> GetTransactionAsync(Guid userId, Guid transactionId, CancellationToken cancellationToken = default);

    Task<WalletSummaryResponse> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<SettlementSubmitResponse> RequestSettlementAsync(
        Guid userId,
        SettlementSubmitRequest request,
        CancellationToken cancellationToken = default);

    Task<SettlementRequestResponse> ApproveSettlementAsync(
        Guid adminUserId,
        Guid settlementId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SettlementRequestResponse>> ListSettlementStatusAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
