using System.Text;
using On4Net.Extensions.Data.DataManager;
using On4Net.Extensions.Data.DataManager.Infostruct;
using On4Net.Extensions.Data.Model.Entity;
using Wallet.Core.Model.Entities;
using Wallet.Core.Model.Request;
using Wallet.Core.Model.Response;

namespace Wallet.Core.Data.Repositories;

public class WalletTransactionRepository : ExecutionMethods, IWalletTransactionRepository
{
    private const string SchemaName = "\"public\"";
    private const string TableName = "\"transactions\"";

    public WalletTransactionRepository(IOutboxTransactionManager outboxTransactionManager, Func<DateTime> dateTimeProvider)
        : base(outboxTransactionManager, dateTimeProvider)
    {
    }

    public async Task<WalletTransaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var query = $@"SELECT * FROM {SchemaName}.{TableName}
                       WHERE ""id"" = @Id AND ""status"" = @Status LIMIT 1";
        return await ExecuteAndGetFirstOrDefaultAsync<WalletTransaction?>(
            query,
            new { Id = id, Status = Status.Active },
            cancellationToken);
    }

    public async Task<WalletTransaction?> GetByIdForUserAsync(
        Guid userId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = $@"SELECT * FROM {SchemaName}.{TableName}
                       WHERE ""id"" = @Id AND ""user_id"" = @UserId AND ""status"" = @Status
                       LIMIT 1";
        return await ExecuteAndGetFirstOrDefaultAsync<WalletTransaction?>(
            query,
            new { Id = id, UserId = userId, Status = Status.Active },
            cancellationToken);
    }

    public async Task<WalletTransaction?> FindCompletedMainDepositByReferenceAsync(
        Guid userId,
        string referenceType,
        Guid referenceId,
        CancellationToken cancellationToken = default)
    {
        var query = $@"SELECT * FROM {SchemaName}.{TableName}
                       WHERE ""user_id"" = @UserId
                         AND ""transaction_type"" = @TxType
                         AND ""wallet_type"" = @WalletType
                         AND ""transaction_status"" = @TxStatus
                         AND ""reference_type"" = @ReferenceType
                         AND ""reference_id"" = @ReferenceId
                         AND ""status"" = @Status
                       LIMIT 1";
        return await ExecuteAndGetFirstOrDefaultAsync<WalletTransaction?>(
            query,
            new
            {
                UserId = userId,
                TxType = Wallet.Core.Model.Constants.TransactionTypeCodes.MainWalletDeposit,
                WalletType = (short)Wallet.Core.Model.Enum.WalletType.Main,
                TxStatus = (short)Wallet.Core.Model.Enum.TransactionWorkflowStatus.Completed,
                ReferenceType = referenceType,
                ReferenceId = referenceId,
                Status = Status.Active
            },
            cancellationToken);
    }

    public async Task<WalletTransaction> InsertAsync(WalletTransaction row, CancellationToken cancellationToken = default)
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
            (""id"", ""user_id"", ""wallet_type"", ""transaction_type"", ""reference_id"", ""reference_type"",
             ""debit"", ""credit"", ""balance_before"", ""balance_after"", ""description"", ""transaction_status"",
             ""expires_at"", ""is_expired"", ""completed_at"",
             ""version"", ""status"", ""created_at"", ""created_by"", ""modified_at"", ""modified_by"")
            VALUES
            (@Id, @UserId, @WalletType, @TransactionType, @ReferenceId, @ReferenceType,
             @Debit, @Credit, @BalanceBefore, @BalanceAfter, @Description, @TransactionStatus,
             @ExpiresAt, @IsExpired, @CompletedAt,
             @Version, @Status, @CreatedAt, @CreatedBy, @ModifiedAt, @ModifiedBy)";

        await ExecuteAsync(query, row, cancellationToken);
        return row;
    }

    public async Task<PagedTransactionsResult> SearchAsync(
        Guid userId,
        TransactionSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var where = new StringBuilder(@"""user_id"" = @UserId AND ""status"" = @Status");
        var parameters = new Dictionary<string, object?>
        {
            ["UserId"] = userId,
            ["Status"] = Status.Active
        };

        if (request.FromDate.HasValue)
        {
            where.Append(@" AND ""created_at"" >= @FromDate");
            parameters["FromDate"] = request.FromDate.Value;
        }

        if (request.ToDate.HasValue)
        {
            where.Append(@" AND ""created_at"" <= @ToDate");
            parameters["ToDate"] = request.ToDate.Value;
        }

        if (request.WalletType.HasValue)
        {
            where.Append(@" AND ""wallet_type"" = @WalletType");
            parameters["WalletType"] = request.WalletType.Value;
        }

        if (request.TransactionType.HasValue)
        {
            where.Append(@" AND ""transaction_type"" = @TransactionType");
            parameters["TransactionType"] = request.TransactionType.Value;
        }

        var pageNo = Math.Max(1, request.PageNo);
        var pageSize = Math.Clamp(request.PageSize, 1, 200);
        var offset = (pageNo - 1) * pageSize;
        parameters["PageSize"] = pageSize;
        parameters["Offset"] = offset;

        var query = $@"
            SELECT COUNT(*) OVER() AS ""TotalCountRecords"",
                   ""id"",
                   ""wallet_type"" AS ""WalletType"",
                   ""transaction_type"" AS ""TransactionType"",
                   ""reference_id"" AS ""ReferenceId"",
                   ""reference_type"" AS ""ReferenceType"",
                   ""debit"" AS ""Debit"",
                   ""credit"" AS ""Credit"",
                   ""balance_before"" AS ""BalanceBefore"",
                   ""balance_after"" AS ""BalanceAfter"",
                   ""description"" AS ""Description"",
                   ""transaction_status"" AS ""TransactionStatus"",
                   ""created_at"" AS ""CreatedAt""
            FROM {SchemaName}.{TableName}
            WHERE {where}
            ORDER BY ""created_at"" DESC
            LIMIT @PageSize OFFSET @Offset";

        var list = await ExecuteAndGetListAsync<TransactionListRow>(query, parameters, cancellationToken);
        var items = list.ToList();
        var total = items.FirstOrDefault()?.TotalCountRecords ?? 0;
        return new PagedTransactionsResult { Items = items, TotalCountRecords = total };
    }

    public async Task<(decimal SumCredit, decimal SumDebit, int Count)> GetCompletedTotalsForUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var query = $@"
            SELECT COALESCE(SUM(""credit""), 0) AS ""SumCredit"",
                   COALESCE(SUM(""debit""), 0) AS ""SumDebit"",
                   COUNT(*)::int AS ""Cnt""
            FROM {SchemaName}.{TableName}
            WHERE ""user_id"" = @UserId
              AND ""status"" = @Status
              AND ""transaction_status"" = @TxStatus";

        var row = await ExecuteAndGetFirstOrDefaultAsync<TotalsRow?>(
            query,
            new
            {
                UserId = userId,
                Status = Status.Active,
                TxStatus = (short)Wallet.Core.Model.Enum.TransactionWorkflowStatus.Completed
            },
            cancellationToken);

        return row == null ? (0, 0, 0) : (row.SumCredit, row.SumDebit, row.Cnt);
    }

    private sealed class TotalsRow
    {
        public decimal SumCredit { get; set; }
        public decimal SumDebit { get; set; }
        public int Cnt { get; set; }
    }
}
