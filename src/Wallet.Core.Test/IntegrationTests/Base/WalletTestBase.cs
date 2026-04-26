using Wallet.Core.Test.Helper;
using Wallet.Core.Test.IntegrationTests.Base.Fixtures;
using FluentAssertions;

namespace Wallet.Core.Test.IntegrationTests.Base;

public abstract class WalletTestBase : IDisposable
{
    protected WalletAppsFixture Fixture { get; }
    protected HttpClient Client { get; }

    protected WalletTestBase()
    {
        Fixture = new WalletAppsFixture();
        Client = Fixture.CreateClient();
    }

    protected async Task<HttpResponseMessage> GetAsync(
        string uri,
        string? bearerToken = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.ConfigureBearer(bearerToken);
        return await Client.SendAsync(request, cancellationToken);
    }

    protected async Task<HttpResponseMessage> PostJsonAsync(
        string uri,
        object body,
        string? bearerToken = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, uri);
        request.ConfigureBearer(bearerToken);
        request.WithJsonBody(body);
        return await Client.SendAsync(request, cancellationToken);
    }

    protected async Task<HttpResponseMessage> PutAsync(
        string uri,
        string? bearerToken = null,
        object? body = null,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, uri);
        request.ConfigureBearer(bearerToken);
        if (body != null)
        {
            request.WithJsonBody(body);
        }

        return await Client.SendAsync(request, cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
        {
            return;
        }

        Client.Dispose();
        Fixture.Dispose();
    }
}
