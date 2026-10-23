using LedgerLens.MarketData.Api.Features.ForeignExchangeRates.GetLatestList;
using LedgerLens.MarketData.Api.Features.InstrumentCandles.GetOne;
using LedgerLens.MarketData.Api.Features.Instruments.SearchList;
using LedgerLens.MarketData.Api.Features.Quotes.GetLatestList;

namespace LedgerLens.MarketData.Api;

public static class MarketDataEndpoints
{
    public static IEndpointRouteBuilder MapMarketDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/v1");
        group.MapInstrumentsSearchList();
        group.MapQuotesGetLatestList();
        group.MapInstrumentCandlesGetOne();
        group.MapForeignExchangeRatesGetLatestList();
        return endpoints;
    }
}
