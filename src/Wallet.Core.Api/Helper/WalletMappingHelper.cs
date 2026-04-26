using Wallet.Core.Model.Entities;
using Wallet.Core.Model.Enum;
using Wallet.Core.Model.Request;

namespace Wallet.Core.Api.Helper;

/// <summary>نگاشت DTO → Entity (الگوی MappingHelper نمونه).</summary>
public static class WalletMappingHelper
{
    /// <summary>درخواست تسویهٔ ورودی را به موجودیت قبل از Insert نگاشت می‌کند.</summary>
    public static SettlementRecord ToPendingSettlementRecord(this SettlementSubmitRequest request, Guid userId) =>
        new()
        {
            UserId = userId,
            Amount = request.Amount,
            BankAccountId = request.BankAccountId,
            RequestStatus = (short)SettlementRequestStatus.Pending,
            InvoiceUrl = string.IsNullOrWhiteSpace(request.InvoiceUrl) ? null : request.InvoiceUrl.Trim(),
            PaymentSlipUrl = string.IsNullOrWhiteSpace(request.PaymentSlipUrl) ? null : request.PaymentSlipUrl.Trim(),
            LockedAmount = request.Amount
        };
}
