namespace Wallet.Core.Model.Enum;

/// <summary>وضعیت درخواست تسویه (request_status).</summary>
public enum SettlementRequestStatus : short
{
    Pending = 1,
    Approved = 2,
    Completed = 3,
    Rejected = 4
}
