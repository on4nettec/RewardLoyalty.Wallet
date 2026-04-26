using Wallet.Core.Model.Entities;

namespace Wallet.Core.Data.Repositories;

public interface ISettlementRequestRepository
{
    Task<SettlementRecord> InsertAsync(SettlementRecord row, CancellationToken cancellationToken = default);

    Task<SettlementRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SettlementRecord>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> TryUpdateRequestStatusAsync(
        Guid id,
        short newStatus,
        short expectedPreviousStatus,
        string modifiedBy,
        CancellationToken cancellationToken = default);
}
