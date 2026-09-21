using LedgerLens.Application.Abstractions;
using LedgerLens.MarketData.Application.Features.ForeignExchangeRates.GetLatestList;
using LedgerLens.MarketData.Application.Features.InstrumentCandles.GetOne;
using LedgerLens.MarketData.Application.Features.Instruments.SearchList;
using LedgerLens.MarketData.Application.Features.Quotes.GetLatestList;
using LedgerLens.MarketData.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerLens.MarketData.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMarketDataApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IQueryHandler<InstrumentsSearchListQuery, IReadOnlyList<MarketInstrument>>, InstrumentsSearchListHandler>();
        services.AddScoped<IQueryHandler<QuotesGetLatestListQuery, IReadOnlyList<QuoteSnapshot>>, QuotesGetLatestListHandler>();
        services.AddScoped<IQueryHandler<InstrumentCandlesGetOneQuery, CandleSeries?>, InstrumentCandlesGetOneHandler>();
        services.AddScoped<IQueryHandler<ForeignExchangeRatesGetLatestListQuery, IReadOnlyList<FxSnapshot>>, ForeignExchangeRatesGetLatestListHandler>();
        return services;
    }
}
