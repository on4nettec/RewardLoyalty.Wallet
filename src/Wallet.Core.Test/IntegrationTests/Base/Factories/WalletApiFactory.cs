using On4Net.Extensions.Data.Migration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Wallet.Core.Test.IntegrationTests.Base.Factories;

public class WalletApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"test_wallet_{Guid.NewGuid():N}";
    private readonly PostgreSqlContainer? _container;
    private bool _disposed;

    public WalletApiFactory()
    {
        var useExternal = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WALLET_TEST_PG_CONNECTION_STRING"));
        if (!useExternal)
        {
            _container = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .Build();
            _container.StartAsync().GetAwaiter().GetResult();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, conf) =>
        {
            conf.AddJsonFile("appsettings.Testing.json", optional: true);
            conf.AddInMemoryCollection(BuildDbConfiguration());
        });
    }

    private IEnumerable<KeyValuePair<string, string?>> BuildDbConfiguration()
    {
        var external = Environment.GetEnvironmentVariable("WALLET_TEST_PG_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(external))
        {
            var b = new NpgsqlConnectionStringBuilder(external);
            return new Dictionary<string, string?>
            {
                ["db:Name"] = _databaseName,
                ["db:Address"] = b.Host,
                ["db:Port"] = (b.Port == 0 ? 5432 : b.Port).ToString(),
                ["db:UserName"] = b.Username,
                ["db:Password"] = b.Password ?? string.Empty,
                ["db:JournalTable"] = "schema_versions",
                ["RunMigrations"] = "true",
            };
        }

        var cb = new NpgsqlConnectionStringBuilder(_container!.GetConnectionString());
        return new Dictionary<string, string?>
        {
            ["db:Name"] = _databaseName,
            ["db:Address"] = cb.Host,
            ["db:Port"] = (cb.Port == 0 ? 5432 : cb.Port).ToString(),
            ["db:UserName"] = cb.Username,
            ["db:Password"] = cb.Password ?? string.Empty,
            ["db:JournalTable"] = "schema_versions",
            ["RunMigrations"] = "true",
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            if (_container != null)
            {
                try
                {
                    _container.StopAsync().GetAwaiter().GetResult();
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                try
                {
                    using var scope = Services.CreateScope();
                    scope.ServiceProvider.GetService<DataSchemaMigrator>()?.DropDatabase();
                }
                catch
                {
                    // ignored
                }
            }

            _disposed = true;
        }

        base.Dispose(disposing);
    }
}
