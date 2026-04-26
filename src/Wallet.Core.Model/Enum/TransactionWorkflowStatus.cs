namespace Wallet.Core.Model.Enum;

/// <summary>وضعیت گردش تراکنش (transaction_status).</summary>
public enum TransactionWorkflowStatus : short
{
    Pending = 1,
    Completed = 2,
    Error = 3,
    Cancelled = 4
}
