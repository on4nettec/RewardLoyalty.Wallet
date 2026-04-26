namespace Wallet.Core.Model.Response;

public class SettlementRequestResponse
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public Guid BankAccountId { get; set; }
    public short RequestStatus { get; set; }
    public string? InvoiceUrl { get; set; }
    public string? PaymentSlipUrl { get; set; }
    public decimal LockedAmount { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
