using Microsoft.Extensions.DependencyInjection;

namespace LedgerLens.MarketData.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMarketDataApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<InstrumentsSearchListHandler>();
        services.AddScoped<QuotesGetLatestListHandler>();
        services.AddScoped<InstrumentCandlesGetOneHandler>();
        services.AddScoped<ForeignExchangeRatesGetLatestListHandler>();
        return services;
    }
}
