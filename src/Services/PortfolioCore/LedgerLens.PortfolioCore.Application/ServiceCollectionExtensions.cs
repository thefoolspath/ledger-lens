using Microsoft.Extensions.DependencyInjection;

namespace LedgerLens.PortfolioCore.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPortfolioCoreApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<CreatePortfolioHandler>();
        services.AddScoped<CreateAccountHandler>();
        services.AddScoped<RecordLedgerEntryHandler>();
        services.AddScoped<CorrectLedgerEntryHandler>();
        services.AddScoped<GetPortfolioOverviewHandler>();
        services.AddScoped<ListPortfoliosHandler>();
        services.AddScoped<CreateSimulationAccountHandler>();
        services.AddScoped<ListSimulationAccountsHandler>();
        services.AddScoped<CreateSimulationDraftHandler>();
        services.AddScoped<ConfirmSimulationDraftHandler>();
        services.AddScoped<CorrectSimulationTradeHandler>();
        services.AddScoped<GetSimulationOverviewHandler>();
        services.AddScoped<RecordSimulationValuationHandler>();
        services.AddScoped<GetSimulationValuationSeriesHandler>();
        return services;
    }
}
