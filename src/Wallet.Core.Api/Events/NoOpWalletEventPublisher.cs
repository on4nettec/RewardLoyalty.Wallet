using Wallet.Core.Model.Entities;

namespace Wallet.Core.Api.Events;

public class NoOpWalletEventPublisher : IWalletEventPublisher
{
    public Task PublishWalletDepositedAsync(Guid userId, WalletTransaction transaction, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task PublishWalletWithdrawnAsync(Guid userId, WalletTransaction transaction, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task PublishSettlementRequestedAsync(Guid userId, SettlementRecord record, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public Task PublishSettlementApprovedAsync(Guid userId, SettlementRecord record, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;
}
