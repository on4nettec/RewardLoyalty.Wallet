namespace Wallet.Core.Model.Request;

public class SettlementSubmitRequest
{
    public decimal Amount { get; set; }
    public Guid BankAccountId { get; set; }
    public string? InvoiceUrl { get; set; }
    public string? PaymentSlipUrl { get; set; }
}
