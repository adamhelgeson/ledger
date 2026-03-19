using Anthropic;
using Ledger.Core.Interfaces;
using Ledger.Infrastructure.Data;
using Ledger.Infrastructure.Repositories;
using Ledger.Infrastructure.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ledger.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<LedgerDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IImportBatchRepository, ImportBatchRepository>();
        services.AddScoped<IBalanceSnapshotRepository, BalanceSnapshotRepository>();
        services.AddScoped<IHoldingRepository, HoldingRepository>();

        services.AddScoped<DatabaseSeeder>();

        services.AddSingleton<AnthropicClient>(sp =>
        {
            IConfiguration config = sp.GetRequiredService<IConfiguration>();
            string? apiKey = config["Heimdall:ApiKey"];
            return string.IsNullOrWhiteSpace(apiKey)
                ? new AnthropicClient()      // falls back to ANTHROPIC_API_KEY env var
                : new AnthropicClient { ApiKey = apiKey };
        });

        return services;
    }
}
