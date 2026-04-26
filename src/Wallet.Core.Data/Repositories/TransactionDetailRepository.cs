using On4Net.Extensions.Data.DataManager;
using On4Net.Extensions.Data.DataManager.Infostruct;
using On4Net.Extensions.Data.Model.Entity;
using Wallet.Core.Model.Entities;

namespace Wallet.Core.Data.Repositories;

public class TransactionDetailRepository : ExecutionMethods, ITransactionDetailRepository
{
    private const string SchemaName = "\"public\"";
    private const string TableName = "\"trans_details\"";

    public TransactionDetailRepository(IOutboxTransactionManager outboxTransactionManager, Func<DateTime> dateTimeProvider)
        : base(outboxTransactionManager, dateTimeProvider)
    {
    }

    public async Task InsertAsync(TransactionDetail row, CancellationToken cancellationToken = default)
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
            (""id"", ""transaction_id"", ""key"", ""value"", ""version"", ""status"", ""created_at"", ""created_by"", ""modified_at"", ""modified_by"")
            VALUES
            (@Id, @TransactionId, @Key, @Value, @Version, @Status, @CreatedAt, @CreatedBy, @ModifiedAt, @ModifiedBy)";

        await ExecuteAsync(query, row, cancellationToken);
    }

    public async Task<IReadOnlyList<TransactionDetail>> ListByTransactionIdAsync(
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        var query = $@"SELECT * FROM {SchemaName}.{TableName}
                       WHERE ""transaction_id"" = @TransactionId AND ""status"" = @Status
                       ORDER BY ""created_at""";
        var rows = await ExecuteAndGetListAsync<TransactionDetail>(
            query,
            new { TransactionId = transactionId, Status = Status.Active },
            cancellationToken);
        return rows.ToList();
    }
}
