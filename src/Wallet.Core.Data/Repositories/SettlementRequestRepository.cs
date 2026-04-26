using On4Net.Extensions.Data.DataManager;
using On4Net.Extensions.Data.DataManager.Infostruct;
using On4Net.Extensions.Data.Model.Entity;
using Wallet.Core.Model.Entities;

namespace Wallet.Core.Data.Repositories;

public class SettlementRequestRepository : ExecutionMethods, ISettlementRequestRepository
{
    private const string SchemaName = "\"public\"";
    private const string TableName = "\"settlement_requests\"";

    public SettlementRequestRepository(IOutboxTransactionManager outboxTransactionManager, Func<DateTime> dateTimeProvider)
        : base(outboxTransactionManager, dateTimeProvider)
    {
    }

    public async Task<SettlementRecord> InsertAsync(SettlementRecord row, CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider();
        row.Id = Guid.NewGuid();
        row.Version = 1;
        row.Status = Status.Active;
        row.CreatedAt = now;
        row.ModifiedAt = now;
        row.CreatedBy ??= "SYSTEM";
        row.ModifiedBy ??= "SYSTEM";

        var query = $@"
            INSERT INTO {SchemaName}.{TableName}
            (""id"", ""user_id"", ""amount"", ""bank_account_id"", ""request_status"", ""invoice_url"", ""payment_slip_url"",
             ""locked_amount"", ""completed_at"", ""version"", ""status"", ""created_at"", ""created_by"", ""modified_at"", ""modified_by"")
            VALUES
            (@Id, @UserId, @Amount, @BankAccountId, @RequestStatus, @InvoiceUrl, @PaymentSlipUrl,
             @LockedAmount, @CompletedAt, @Version, @Status, @CreatedAt, @CreatedBy, @ModifiedAt, @ModifiedBy)";

        await ExecuteAsync(query, row, cancellationToken);
        return row;
    }

    public async Task<SettlementRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = $@"SELECT * FROM {SchemaName}.{TableName}
                       WHERE ""id"" = @Id AND ""status"" = @Status LIMIT 1";
        return await ExecuteAndGetFirstOrDefaultAsync<SettlementRecord?>(
            query,
            new { Id = id, Status = Status.Active },
            cancellationToken);
    }

    public async Task<IReadOnlyList<SettlementRecord>> ListByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var query = $@"SELECT * FROM {SchemaName}.{TableName}
                       WHERE ""user_id"" = @UserId AND ""status"" = @Status
                       ORDER BY ""created_at"" DESC";
        var rows = await ExecuteAndGetListAsync<SettlementRecord>(
            query,
            new { UserId = userId, Status = Status.Active },
            cancellationToken);
        return rows.ToList();
    }

    public async Task<bool> TryUpdateRequestStatusAsync(
        Guid id,
        short newStatus,
        short expectedPreviousStatus,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider();
        var query = $@"
            UPDATE {SchemaName}.{TableName}
            SET ""request_status"" = @NewStatus,
                ""version"" = ""version"" + 1,
                ""modified_at"" = @ModifiedAt,
                ""modified_by"" = @ModifiedBy
            WHERE ""id"" = @Id
              AND ""request_status"" = @ExpectedPrevious
              AND ""status"" = @RowStatus
            RETURNING ""id""";

        var returned = await ExecuteAndGetFirstOrDefaultAsync<Guid?>(
            query,
            new
            {
                Id = id,
                NewStatus = newStatus,
                ExpectedPrevious = expectedPreviousStatus,
                RowStatus = Status.Active,
                ModifiedAt = now,
                ModifiedBy = modifiedBy
            },
            cancellationToken);

        return returned != null;
    }
}
