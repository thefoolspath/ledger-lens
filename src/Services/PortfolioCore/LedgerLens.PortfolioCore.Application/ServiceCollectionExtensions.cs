using Microsoft.Extensions.DependencyInjection;

namespace LedgerLens.PortfolioCore.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPortfolioCoreApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<PortfoliosCreateHandler>();
        services.AddScoped<InvestmentAccountsCreateHandler>();
        services.AddScoped<CashLedgerEntriesCreateHandler>();
        services.AddScoped<CashLedgerEntriesCorrectHandler>();
        services.AddScoped<PortfoliosGetOneHandler>();
        services.AddScoped<PortfoliosGetListHandler>();
        services.AddScoped<SimulationAccountsCreateHandler>();
        services.AddScoped<SimulationAccountsGetListHandler>();
        services.AddScoped<SimulationTradeDraftsCreateHandler>();
        services.AddScoped<SimulationTradeDraftsConfirmHandler>();
        services.AddScoped<SimulationTradesCorrectHandler>();
        services.AddScoped<SimulationAccountsGetOneHandler>();
        services.AddScoped<SimulationValuationsRecordListHandler>();
        services.AddScoped<SimulationValuationsCalculateSeriesListHandler>();
        return services;
    }
}
