namespace Wallet.Core.Model.Constants;

/// <summary>کدهای transaction_type مطابق technical-database.</summary>
public static class TransactionTypeCodes
{
    public const short MainWalletDeposit = 1;
    public const short MainWalletWithdraw = 2;
    public const short TransferToUser = 3;
    public const short CashbackReceived = 4;
    public const short CashbackSpent = 5;
    public const short CashbackExpired = 6;
    public const short ReferralReward = 7;
    public const short Penalty = 8;
    public const short GiftReceived = 9;
    public const short BankSettlement = 10;
}
