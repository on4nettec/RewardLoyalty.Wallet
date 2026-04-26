using Wallet.Core.Model.Entities;

namespace Wallet.Core.Data.Repositories;

public interface ITransactionDetailRepository
{
    Task InsertAsync(TransactionDetail row, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TransactionDetail>> ListByTransactionIdAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default);
}
