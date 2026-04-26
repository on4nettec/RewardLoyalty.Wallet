namespace Wallet.Core.Model.Request;

public class TransactionSearchRequest
{
    public int PageNo { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public short? WalletType { get; set; }
    public short? TransactionType { get; set; }
}
