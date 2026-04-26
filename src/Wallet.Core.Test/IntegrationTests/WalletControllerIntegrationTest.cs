using System.Net;
using Wallet.Core.Model.Request;
using Wallet.Core.Model.Response;
using Wallet.Core.Test.Helper;
using Wallet.Core.Test.IntegrationTests.Base;
using FluentAssertions;

namespace Wallet.Core.Test.IntegrationTests;

public class WalletControllerIntegrationTest : WalletTestBase
{
    [Fact]
    public async Task Wallet_Health_ReturnsOk()
    {
        using var response = await GetAsync("/api/wallet/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Wallet_Balance_WithoutToken_ReturnsUnauthorized()
    {
        using var response = await GetAsync("/api/wallet/balance");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Wallet_Deposit_Then_Balance_ReturnsMainBalance()
    {
        var userId = Guid.NewGuid();
        var token = JwtTestHelper.CreateToken(userId);

        using var deposit = await PostJsonAsync(
            "/api/wallet/deposit",
            new DepositRequest { Amount = 75.50m, Description = "تست واریز" },
            token);
        deposit.StatusCode.Should().Be(HttpStatusCode.OK);
        var depBody = await deposit.ReadAsJsonAsync<DepositResponse>();
        depBody.Should().NotBeNull();
        depBody!.IdempotentReplay.Should().BeFalse();
        depBody.Wallet.MainBalance.Should().Be(75.50m);

        using var balance = await GetAsync("/api/wallet/balance", token);
        balance.StatusCode.Should().Be(HttpStatusCode.OK);
        var bal = await balance.ReadAsJsonAsync<WalletBalanceResponse>();
        bal!.MainBalance.Should().Be(75.50m);
    }

    [Fact]
    public async Task Wallet_Deposit_Idempotent_ByReference()
    {
        var userId = Guid.NewGuid();
        var token = JwtTestHelper.CreateToken(userId);
        var refId = Guid.NewGuid();

        using var first = await PostJsonAsync(
            "/api/wallet/deposit",
            new DepositRequest
            {
                Amount = 10m,
                ReferenceType = "TestRef",
                ReferenceId = refId
            },
            token);
        first.StatusCode.Should().Be(HttpStatusCode.OK);

        using var second = await PostJsonAsync(
            "/api/wallet/deposit",
            new DepositRequest
            {
                Amount = 10m,
                ReferenceType = "TestRef",
                ReferenceId = refId
            },
            token);
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await second.ReadAsJsonAsync<DepositResponse>();
        body!.IdempotentReplay.Should().BeTrue();

        using var balance = await GetAsync("/api/wallet/balance", token);
        var bal2 = await balance.ReadAsJsonAsync<WalletBalanceResponse>();
        bal2!.MainBalance.Should().Be(10m);
    }

    [Fact]
    public async Task Wallet_SettlementRequest_Then_AdminApprove()
    {
        var userId = Guid.NewGuid();
        var userToken = JwtTestHelper.CreateToken(userId);
        var adminToken = JwtTestHelper.CreateToken(Guid.NewGuid(), userType: "1");

        using var dep = await PostJsonAsync(
            "/api/wallet/deposit",
            new DepositRequest { Amount = 500m },
            userToken);
        dep.StatusCode.Should().Be(HttpStatusCode.OK);

        var bankId = Guid.NewGuid();
        using var req = await PostJsonAsync(
            "/api/wallet/settlement/request",
            new SettlementSubmitRequest { Amount = 100m, BankAccountId = bankId },
            userToken);
        req.StatusCode.Should().Be(HttpStatusCode.OK);
        var submit = await req.ReadAsJsonAsync<SettlementSubmitResponse>();
        submit!.Wallet.MainBalance.Should().Be(400m);

        var settlementId = submit.SettlementRequestId;
        using var approve = await PutAsync($"/api/wallet/settlement/{settlementId}/approve", adminToken);
        approve.StatusCode.Should().Be(HttpStatusCode.OK);
        var approved = await approve.ReadAsJsonAsync<SettlementRequestResponse>();
        approved!.RequestStatus.Should().Be(2);
    }
}
