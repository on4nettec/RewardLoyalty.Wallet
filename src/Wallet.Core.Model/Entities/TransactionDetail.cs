using On4Net.Extensions.Data.Model.Entity;

namespace Wallet.Core.Model.Entities;

/// <summary>ردیف جدول trans_details.</summary>
public class TransactionDetail : BaseStatusEntity
{
    public Guid TransactionId { get; set; }
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
}
