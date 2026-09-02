using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace LedgerLens.PortfolioCore.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static TBuilder AddPortfolioCoreInfrastructure<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddNpgsqlDbContext<PortfolioCoreDbContext>("portfolio-db");
        builder.AddNatsClient("messaging");
        builder.Services.AddOptions<FixedLocalUserOptions>()
            .BindConfiguration(FixedLocalUserOptions.SectionName)
            .ValidateOnStart();
        builder.Services.AddSingleton<IValidateOptions<FixedLocalUserOptions>, FixedLocalUserOptionsValidator>();
        builder.Services.AddSingleton<ICurrentUser, FixedLocalCurrentUser>();
        builder.Services.AddScoped<IPortfolioCoreStore, PortfolioCoreStore>();
        builder.Services.AddScoped<ISimulationStore>(services => services.GetRequiredService<IPortfolioCoreStore>() as PortfolioCoreStore
            ?? throw new InvalidOperationException("PortfolioCoreStore registration is invalid."));
        return builder;
    }
}
