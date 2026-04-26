using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Wallet.Core.Api.Services;
using Wallet.Core.Model.Constants;
using Wallet.Core.Model.Request;

namespace Wallet.Core.Api.Controllers;

[ApiController]
[Route("api/wallet")]
[Authorize]
public class WalletController : ControllerBase
{
    private const string AdminUserTypeClaim = "1";
    private readonly IWalletLedgerService _ledger;

    public WalletController(IWalletLedgerService ledger)
    {
        _ledger = ledger;
    }

    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health() => Ok(new { service = "wallet", status = "ok" });

    [HttpGet("balance")]
    public async Task<IActionResult> Balance(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        return Ok(await _ledger.GetBalanceAsync(userId, cancellationToken));
    }

    [HttpPost("deposit")]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        return Ok(await _ledger.DepositAsync(userId, request, cancellationToken));
    }

    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        return Ok(await _ledger.WithdrawAsync(userId, request, cancellationToken));
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> Transactions([FromQuery] TransactionSearchRequest request, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        return Ok(await _ledger.ListTransactionsAsync(userId, request, cancellationToken));
    }

    [HttpGet("transactions/{id:guid}")]
    public async Task<IActionResult> TransactionById(Guid id, CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        return Ok(await _ledger.GetTransactionAsync(userId, id, cancellationToken));
    }

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        return Ok(await _ledger.GetSummaryAsync(userId, cancellationToken));
    }

    [HttpPost("settlement/request")]
    public async Task<IActionResult> SettlementRequest(
        [FromBody] SettlementSubmitRequest request,
        CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        return Ok(await _ledger.RequestSettlementAsync(userId, request, cancellationToken));
    }

    [HttpPut("settlement/{id:guid}/approve")]
    public async Task<IActionResult> ApproveSettlement(Guid id, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var adminId = RequireUserId();
        return Ok(await _ledger.ApproveSettlementAsync(adminId, id, cancellationToken));
    }

    [HttpGet("settlement/status")]
    public async Task<IActionResult> SettlementStatus(CancellationToken cancellationToken)
    {
        var userId = RequireUserId();
        return Ok(await _ledger.ListSettlementStatusAsync(userId, cancellationToken));
    }

    private Guid RequireUserId()
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (string.IsNullOrEmpty(sub) || !Guid.TryParse(sub, out var userId))
        {
            throw new On4Net.Extensions.Exception.ApplicationException(
                ErrorCodes.AuthTokenNotValid,
                ErrorCodes.AuthTokenNotValid,
                statusCode: 401);
        }

        return userId;
    }

    private bool IsAdmin() =>
        User.FindFirst("userType")?.Value == AdminUserTypeClaim;
}
