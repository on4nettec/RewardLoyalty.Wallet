using Microsoft.Extensions.DependencyInjection;
using Wallet.Core.Data.Repositories;

namespace Wallet.Core.Data;

public static class Configuration
{
    public static IServiceCollection ConfigureWalletDataServices(this IServiceCollection services)
    {
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
        services.AddScoped<ITransactionDetailRepository, TransactionDetailRepository>();
        services.AddScoped<ISettlementRequestRepository, SettlementRequestRepository>();
        services.AddSingleton(() => DateTime.UtcNow);
        return services;
    }
}
