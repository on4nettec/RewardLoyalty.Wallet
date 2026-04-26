namespace Wallet.Core.Model.Request;

public class DepositRequest
{
    public decimal Amount { get; set; }
    public string? Description { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
    public List<TransactionDetailPair>? Details { get; set; }
}

public class TransactionDetailPair
{
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}
