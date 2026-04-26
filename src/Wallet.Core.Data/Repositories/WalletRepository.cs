using On4Net.Extensions.Data.DataManager;
using On4Net.Extensions.Data.DataManager.Infostruct;
using On4Net.Extensions.Data.Model.Entity;
using Wallet.Core.Model.Entities;

namespace Wallet.Core.Data.Repositories;

public class WalletRepository : ExecutionMethods, IWalletRepository
{
    private const string SchemaName = "\"public\"";
    private const string TableName = "\"wallets\"";

    public WalletRepository(IOutboxTransactionManager outboxTransactionManager, Func<DateTime> dateTimeProvider)
        : base(outboxTransactionManager, dateTimeProvider)
    {
    }

    public async Task<ConsumerWallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var query = $@"SELECT * FROM {SchemaName}.{TableName}
                       WHERE ""user_id"" = @UserId AND ""status"" = @Status
                       LIMIT 1";
        return await ExecuteAndGetFirstOrDefaultAsync<ConsumerWallet?>(
            query,
            new { UserId = userId, Status = Status.Active },
            cancellationToken);
    }

    public async Task<ConsumerWallet?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = $@"SELECT * FROM {SchemaName}.{TableName}
                       WHERE ""id"" = @Id AND ""status"" = @Status
                       LIMIT 1";
        return await ExecuteAndGetFirstOrDefaultAsync<ConsumerWallet?>(
            query,
            new { Id = id, Status = Status.Active },
            cancellationToken);
    }

    public async Task<ConsumerWallet> InsertAsync(ConsumerWallet wallet, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider();
        wallet.Id = Guid.NewGuid();
        wallet.Version = 1;
        wallet.Status = Status.Active;
        wallet.CreatedAt = now;
        wallet.ModifiedAt = now;
        wallet.CreatedBy ??= "SYSTEM";
        wallet.ModifiedBy ??= "SYSTEM";

        var query = $@"
            INSERT INTO {SchemaName}.{TableName}
            (""id"", ""user_id"", ""main_balance"", ""cashback_balance"", ""total_main_deposited"", ""total_main_withdrawn"",
             ""total_cashback_received"", ""total_cashback_spent"", ""is_locked"", ""locked_until"",
             ""version"", ""status"", ""created_at"", ""created_by"", ""modified_at"", ""modified_by"")
            VALUES
            (@Id, @UserId, @MainBalance, @CashbackBalance, @TotalMainDeposited, @TotalMainWithdrawn,
             @TotalCashbackReceived, @TotalCashbackSpent, @IsLocked, @LockedUntil,
             @Version, @Status, @CreatedAt, @CreatedBy, @ModifiedAt, @ModifiedBy)";

        await ExecuteAsync(query, wallet, cancellationToken);
        return wallet;
    }

    public async Task<bool> TryUpdateAsync(
        ConsumerWallet wallet,
        int expectedVersion,
        CancellationToken cancellationToken = default)
    {
        wallet.ModifiedAt = _dateTimeProvider();
        wallet.ModifiedBy ??= "SYSTEM";
        var newVersion = expectedVersion + 1;

        var query = $@"
            UPDATE {SchemaName}.{TableName}
            SET ""main_balance"" = @MainBalance,
                ""cashback_balance"" = @CashbackBalance,
                ""total_main_deposited"" = @TotalMainDeposited,
                ""total_main_withdrawn"" = @TotalMainWithdrawn,
                ""total_cashback_received"" = @TotalCashbackReceived,
                ""total_cashback_spent"" = @TotalCashbackSpent,
                ""is_locked"" = @IsLocked,
                ""locked_until"" = @LockedUntil,
                ""version"" = @NewVersion,
                ""modified_at"" = @ModifiedAt,
                ""modified_by"" = @ModifiedBy
            WHERE ""id"" = @Id AND ""version"" = @ExpectedVersion AND ""status"" = @Status
            RETURNING ""id""";

        var id = await ExecuteAndGetFirstOrDefaultAsync<Guid?>(
            query,
            new
            {
                wallet.Id,
                wallet.MainBalance,
                wallet.CashbackBalance,
                wallet.TotalMainDeposited,
                wallet.TotalMainWithdrawn,
                wallet.TotalCashbackReceived,
                wallet.TotalCashbackSpent,
                wallet.IsLocked,
                wallet.LockedUntil,
                NewVersion = newVersion,
                wallet.ModifiedAt,
                wallet.ModifiedBy,
                ExpectedVersion = expectedVersion,
                Status = Status.Active
            },
            cancellationToken);

        if (id != null)
        {
            wallet.Version = newVersion;
        }

        return id != null;
    }
}
