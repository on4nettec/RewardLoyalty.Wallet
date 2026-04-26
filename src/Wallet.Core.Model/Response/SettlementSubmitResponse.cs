namespace Wallet.Core.Model.Response;

public class SettlementSubmitResponse
{
    public Guid SettlementRequestId { get; set; }
    public Guid TransactionId { get; set; }
    public WalletBalanceResponse Wallet { get; set; } = null!;
}
