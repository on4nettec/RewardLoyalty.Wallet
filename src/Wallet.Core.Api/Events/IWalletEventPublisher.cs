using Wallet.Core.Model.Entities;

namespace Wallet.Core.Api.Events;

/// <summary>انتشار رویدادهای دامنه؛ پیاده‌سازی پیش‌فرض NoOp است تا بعداً RabbitMQ و غیره جایگزین شود.</summary>
public interface IWalletEventPublisher
{
    Task PublishWalletDepositedAsync(Guid userId, WalletTransaction transaction, CancellationToken cancellationToken = default);

    Task PublishWalletWithdrawnAsync(Guid userId, WalletTransaction transaction, CancellationToken cancellationToken = default);

    Task PublishSettlementRequestedAsync(Guid userId, SettlementRecord record, CancellationToken cancellationToken = default);

    Task PublishSettlementApprovedAsync(Guid userId, SettlementRecord record, CancellationToken cancellationToken = default);
}
