namespace Wallet.Core.Model.Response;

public class WithdrawResponse
{
    public Guid TransactionId { get; set; }
    public WalletBalanceResponse Wallet { get; set; } = null!;
}
