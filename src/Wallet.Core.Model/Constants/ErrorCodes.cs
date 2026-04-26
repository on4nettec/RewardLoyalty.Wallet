namespace Wallet.Core.Model.Constants;

/// <summary>شناسهٔ خطا برای i18n و استثناهای پکیج On4Net.Extensions.Exception.</summary>
public static class ErrorCodes
{
    public const string ValidationError = "WALLET_VALIDATION_ERROR";

    public const string WalletAmountInvalid = "WALLET_AMOUNT_INVALID";
    public const string WalletInsufficientFunds = "WALLET_INSUFFICIENT_FUNDS";
    public const string WalletLocked = "WALLET_LOCKED";
    public const string WalletTransactionNotFound = "WALLET_TRANSACTION_NOT_FOUND";
    public const string WalletSettlementNotFound = "WALLET_SETTLEMENT_NOT_FOUND";
    public const string WalletSettlementInvalidState = "WALLET_SETTLEMENT_INVALID_STATE";
    public const string WalletConcurrentUpdate = "WALLET_CONCURRENT_UPDATE";

    /// <summary>توکن بدون sub معتبر؛ با Cashback یکسان برای i18n مشترک.</summary>
    public const string AuthTokenNotValid = "AUTH_TOKEN_NOT_VALID";
}

