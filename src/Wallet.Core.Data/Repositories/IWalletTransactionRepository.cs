using Wallet.Core.Model.Entities;
using Wallet.Core.Model.Request;
using Wallet.Core.Model.Response;

namespace Wallet.Core.Data.Repositories;

public interface IWalletTransactionRepository
{
    Task<WalletTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<WalletTransaction?> GetByIdForUserAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    Task<WalletTransaction?> FindCompletedMainDepositByReferenceAsync(
        Guid userId,
        string referenceType,
        Guid referenceId,
        CancellationToken cancellationToken = default);

    Task<WalletTransaction> InsertAsync(WalletTransaction row, CancellationToken cancellationToken = default);

    Task<PagedTransactionsResult> SearchAsync(
        Guid userId,
        TransactionSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<(decimal SumCredit, decimal SumDebit, int Count)> GetCompletedTotalsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
