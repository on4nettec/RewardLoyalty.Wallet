using Wallet.Core.Test.IntegrationTests.Base.Factories;

namespace Wallet.Core.Test.IntegrationTests.Base.Fixtures;

public class WalletAppsFixture : IDisposable
{
    private readonly WalletApiFactory _apiApplicationFactory;

    public WalletAppsFixture()
    {
        _apiApplicationFactory = new WalletApiFactory();
    }

    public HttpClient CreateClient() => _apiApplicationFactory.CreateDefaultClient();

    public void Dispose()
    {
        _apiApplicationFactory.Dispose();
        GC.SuppressFinalize(this);
    }
}
