namespace Wallet.Core.Model.Request;

public class WithdrawRequest
{
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}
