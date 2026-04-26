using On4Net.Extensions.Data.Model.Entity;

namespace Wallet.Core.Model.Entities;

/// <summary>ردیف جدول settlement_requests.</summary>
public class SettlementRecord : BaseStatusEntity
{
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public Guid BankAccountId { get; set; }
    public short RequestStatus { get; set; }
    public string? InvoiceUrl { get; set; }
    public string? PaymentSlipUrl { get; set; }
    public decimal LockedAmount { get; set; }
    public DateTime? CompletedAt { get; set; }
}
