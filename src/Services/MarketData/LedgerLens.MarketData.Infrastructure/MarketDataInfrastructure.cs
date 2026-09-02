using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using LedgerLens.MarketData.Application;

namespace LedgerLens.MarketData.Infrastructure;

public static class MarketDataInfrastructure
{
    public static TBuilder AddMarketDataInfrastructure<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.AddNpgsqlDbContext<MarketDataDbContext>("market-db");
        builder.AddNatsClient("messaging");
        builder.Services.AddOptions<MarketDataProviderOptions>()
            .BindConfiguration(MarketDataProviderOptions.SectionName);
        builder.Services.AddSingleton<SyntheticMarketDataProvider>();
        builder.Services.AddHttpClient<TwelveDataMarketDataProvider>(client =>
        {
            client.BaseAddress = new Uri("https://api.twelvedata.com/");
            client.Timeout = TimeSpan.FromSeconds(15);
        });
        builder.Services.AddScoped<IMarketDataProvider, ConfiguredMarketDataProvider>();
        return builder;
    }
}
