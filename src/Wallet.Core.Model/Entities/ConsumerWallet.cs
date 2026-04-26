using On4Net.Extensions.Data.Model.Entity;

namespace Wallet.Core.Model.Entities;

/// <summary>ردیف جدول wallets (یک ردیف به‌ازای هر کاربر؛ مطابق technical-database).</summary>
public class ConsumerWallet : BaseStatusEntity
{
    public Guid UserId { get; set; }
    public decimal MainBalance { get; set; }
    public decimal CashbackBalance { get; set; }
    public decimal TotalMainDeposited { get; set; }
    public decimal TotalMainWithdrawn { get; set; }
    public decimal TotalCashbackReceived { get; set; }
    public decimal TotalCashbackSpent { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedUntil { get; set; }
}
