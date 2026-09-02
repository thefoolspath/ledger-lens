using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using LedgerLens.MarketData.Application;
using LedgerLens.MarketData.Domain;
using Microsoft.Extensions.Options;

namespace LedgerLens.MarketData.Infrastructure;

public sealed class TwelveDataMarketDataProvider(
    HttpClient httpClient,
    IOptions<MarketDataProviderOptions> options,
    TimeProvider timeProvider) : IMarketDataProvider
{
    private readonly ConcurrentDictionary<Guid, MarketInstrument> instruments = new();

    public async Task<IReadOnlyList<MarketInstrument>> InstrumentsSearchListAsync(string query, string market, CancellationToken cancellationToken)
    {
        using var document = await GetAsync($"symbol_search?symbol={Uri.EscapeDataString(query)}&outputsize=10", cancellationToken);
        var result = new List<MarketInstrument>();
        if (!document.RootElement.TryGetProperty("data", out var data) || data.ValueKind != JsonValueKind.Array) return result;
        foreach (var item in data.EnumerateArray())
        {
            var symbol = Text(item, "symbol");
            var exchange = Text(item, "exchange");
            var currency = Text(item, "currency");
            if (string.IsNullOrWhiteSpace(symbol) || !string.Equals(currency, "USD", StringComparison.OrdinalIgnoreCase)) continue;
            var instrument = new MarketInstrument(Guid.CreateVersion7(), symbol, Text(item, "instrument_name") ?? symbol,
                exchange ?? "Unknown", Text(item, "mic_code") ?? "XNAS", currency!).Normalize();
            instruments[instrument.Id] = instrument;
            result.Add(instrument);
        }
        return result;
    }

    public async Task<IReadOnlyList<QuoteSnapshot>> QuotesGetLatestListAsync(IReadOnlyList<MarketInstrument> requested, CancellationToken cancellationToken)
    {
        var result = new List<QuoteSnapshot>();
        foreach (var instrument in requested)
        {
            using var document = await GetAsync($"quote?symbol={Uri.EscapeDataString(instrument.Symbol)}&exchange={Uri.EscapeDataString(instrument.Exchange)}", cancellationToken);
            ThrowProviderError(document.RootElement);
            var price = Decimal(document.RootElement, "close");
            var now = timeProvider.GetUtcNow();
            var timestamp = Long(document.RootElement, "timestamp");
            var asOf = timestamp is null ? now : DateTimeOffset.FromUnixTimeSeconds(timestamp.Value);
            result.Add(new QuoteSnapshot(instrument, price, "LastTrade", Provenance(asOf, now, $"twelve-quote-{instrument.Symbol}")));
            instruments[instrument.Id] = instrument;
        }
        return result;
    }

    public async Task<CandleSeries?> InstrumentCandlesGetOneAsync(Guid instrumentId, string interval, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        if (!instruments.TryGetValue(instrumentId, out var instrument)) return null;
        using var document = await GetAsync($"time_series?symbol={Uri.EscapeDataString(instrument.Symbol)}&interval=1day&start_date={from:yyyy-MM-dd}&end_date={to:yyyy-MM-dd}&order=ASC&outputsize=5000", cancellationToken);
        ThrowProviderError(document.RootElement);
        var candles = new List<PriceCandle>();
        if (document.RootElement.TryGetProperty("values", out var values))
        {
            foreach (var item in values.EnumerateArray())
            {
                candles.Add(new PriceCandle(DateOnly.Parse(Text(item, "datetime")!, CultureInfo.InvariantCulture), Decimal(item, "open"),
                    Decimal(item, "high"), Decimal(item, "low"), Decimal(item, "close"), Decimal(item, "volume")));
            }
        }
        var now = timeProvider.GetUtcNow();
        return new CandleSeries(instrument, interval, candles, Provenance(now, now, $"twelve-candles-{instrument.Symbol}"));
    }

    public async Task<IReadOnlyList<FxSnapshot>> ForeignExchangeRatesGetLatestListAsync(string baseCurrency, string quoteCurrency, CancellationToken cancellationToken)
    {
        using var document = await GetAsync($"exchange_rate?symbol={baseCurrency}/{quoteCurrency}", cancellationToken);
        ThrowProviderError(document.RootElement);
        var now = timeProvider.GetUtcNow();
        return [new FxSnapshot(baseCurrency, quoteCurrency, Decimal(document.RootElement, "rate"), Provenance(now, now, "twelve-fx"), false)];
    }

    private async Task<JsonDocument> GetAsync(string path, CancellationToken cancellationToken)
    {
        var separator = path.Contains('?', StringComparison.Ordinal) ? '&' : '?';
        using var response = await httpClient.GetAsync($"{path}{separator}apikey={Uri.EscapeDataString(options.Value.ApiKey!)}", cancellationToken);
        if (!response.IsSuccessStatusCode) throw new MarketDataUnavailableException($"Market provider returned HTTP {(int)response.StatusCode}.");
        return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(cancellationToken), cancellationToken: cancellationToken);
    }

    private static void ThrowProviderError(JsonElement element)
    {
        if (element.TryGetProperty("status", out var status) && string.Equals(status.GetString(), "error", StringComparison.OrdinalIgnoreCase))
            throw new MarketDataUnavailableException(Text(element, "message") ?? "Market provider returned an error.");
    }

    private static ObservationProvenance Provenance(DateTimeOffset asOf, DateTimeOffset retrievedAt, string requestId) =>
        new("TwelveData", "EntitledAccountFeed", asOf, retrievedAt, FreshnessClass.LatestAvailable,
            (int)Math.Max(0, (retrievedAt - asOf).TotalSeconds), requestId, "provider-terms-controlled");
    private static string? Text(JsonElement element, string name) => element.TryGetProperty(name, out var value) ? value.GetString() : null;
    private static decimal Decimal(JsonElement element, string name) => decimal.Parse(Text(element, name) ?? throw new MarketDataUnavailableException($"Provider omitted {name}."), CultureInfo.InvariantCulture);
    private static long? Long(JsonElement element, string name) => element.TryGetProperty(name, out var value) && value.TryGetInt64(out var result) ? result : null;
}
