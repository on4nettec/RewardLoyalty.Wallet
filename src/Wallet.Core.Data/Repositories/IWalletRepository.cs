using Wallet.Core.Model.Entities;

namespace Wallet.Core.Data.Repositories;

public interface IWalletRepository
{
    Task<ConsumerWallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ConsumerWallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ConsumerWallet> InsertAsync(ConsumerWallet wallet, CancellationToken cancellationToken = default);

    /// <summary>به‌روزرسانی خوش‌بینانه؛ در صورت عدم تطابق نسخه false برمی‌گرداند.</summary>
    Task<bool> TryUpdateAsync(ConsumerWallet wallet, int expectedVersion, CancellationToken cancellationToken = default);
}
