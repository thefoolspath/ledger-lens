using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using LedgerLens.MarketData.Application;
using LedgerLens.MarketData.Domain;
using Microsoft.Extensions.Options;

namespace LedgerLens.MarketData.Infrastructure;

public sealed class ConfiguredMarketDataProvider(
    IOptions<MarketDataProviderOptions> options,
    SyntheticMarketDataProvider synthetic,
    TwelveDataMarketDataProvider twelveData) : IMarketDataProvider
{
    private IMarketDataProvider Selected => options.Value.Provider.ToUpperInvariant() switch
    {
        "SYNTHETIC" => synthetic,
        "TWELVEDATA" when options.Value.TermsAccepted && !string.IsNullOrWhiteSpace(options.Value.ApiKey) => twelveData,
        "TWELVEDATA" => throw new MarketDataUnavailableException("Twelve Data requires an API key and explicit terms acceptance in backend configuration."),
        _ => throw new MarketDataUnavailableException("Market data provider is disabled. Configure an explicitly admitted backend provider."),
    };

    public Task<IReadOnlyList<MarketInstrument>> InstrumentsSearchListAsync(string query, string market, CancellationToken cancellationToken) => Selected.InstrumentsSearchListAsync(query, market, cancellationToken);
    public Task<IReadOnlyList<QuoteSnapshot>> QuotesGetLatestListAsync(IReadOnlyList<MarketInstrument> instruments, CancellationToken cancellationToken) => Selected.QuotesGetLatestListAsync(instruments, cancellationToken);
    public Task<CandleSeries?> InstrumentCandlesGetOneAsync(Guid instrumentId, string interval, DateOnly from, DateOnly to, CancellationToken cancellationToken) => Selected.InstrumentCandlesGetOneAsync(instrumentId, interval, from, to, cancellationToken);
    public Task<IReadOnlyList<FxSnapshot>> ForeignExchangeRatesGetLatestListAsync(string baseCurrency, string quoteCurrency, CancellationToken cancellationToken) => Selected.ForeignExchangeRatesGetLatestListAsync(baseCurrency, quoteCurrency, cancellationToken);
}
