namespace Wallet.Core.Model.Response;

public class TransactionDetailResponse
{
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
    public DateTime? ExpiresAt { get; set; }
    public bool IsExpired { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TransactionDetailItemResponse> Details { get; set; } = new();
}

public class TransactionDetailItemResponse
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}
