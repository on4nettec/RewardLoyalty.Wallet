namespace Wallet.Core.Model.Response;

/// <summary>ردیف لیست تراکنش همراه با TotalCountRecords برای صفحه‌بندی.</summary>
public class TransactionListRow
{
    public int TotalCountRecords { get; set; }
    public Guid Id { get; set; }
    public short WalletType { get; set; }
    public short TransactionType { get; set; }
    public Guid? ReferenceId { get; set; }
    public string? ReferenceType { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal BalanceBefore { get; set; }
    public decimal BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public short TransactionStatus { get; set; }
    public DateTime CreatedAt { get; set; }
}
