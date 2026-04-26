namespace Wallet.Core.Model.Response;

public class PagedTransactionsResult
{
    public int TotalCountRecords { get; set; }
    public IReadOnlyList<TransactionListRow> Items { get; set; } = Array.Empty<TransactionListRow>();
}
