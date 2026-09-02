using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using LedgerLens.MarketData.Application;
using LedgerLens.MarketData.Domain;
using Microsoft.Extensions.Options;

namespace LedgerLens.MarketData.Infrastructure;

public sealed class MarketDataProviderOptions
{
    public const string SectionName = "LedgerLens:MarketData";
    public string Provider { get; set; } = "Disabled";
    public bool TermsAccepted { get; set; }
    public string? ApiKey { get; set; }
}

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

    public Task<IReadOnlyList<MarketInstrument>> SearchAsync(string query, string market, CancellationToken cancellationToken) => Selected.SearchAsync(query, market, cancellationToken);
    public Task<IReadOnlyList<QuoteSnapshot>> GetLatestQuotesAsync(IReadOnlyList<MarketInstrument> instruments, CancellationToken cancellationToken) => Selected.GetLatestQuotesAsync(instruments, cancellationToken);
    public Task<CandleSeries?> GetCandlesAsync(Guid instrumentId, string interval, DateOnly from, DateOnly to, CancellationToken cancellationToken) => Selected.GetCandlesAsync(instrumentId, interval, from, to, cancellationToken);
    public Task<IReadOnlyList<FxSnapshot>> GetFxAsync(string baseCurrency, string quoteCurrency, CancellationToken cancellationToken) => Selected.GetFxAsync(baseCurrency, quoteCurrency, cancellationToken);
}

public sealed class SyntheticMarketDataProvider(TimeProvider timeProvider) : IMarketDataProvider
{
    private static readonly MarketInstrument[] Instruments =
    [
        new(Guid.Parse("019a0f10-0000-7000-8000-000000000001"), "NVDA", "NVIDIA Corporation", "Nasdaq", "XNAS", "USD"),
        new(Guid.Parse("019a0f10-0000-7000-8000-000000000002"), "AAPL", "Apple Inc.", "Nasdaq", "XNAS", "USD"),
        new(Guid.Parse("019a0f10-0000-7000-8000-000000000003"), "MSFT", "Microsoft Corporation", "Nasdaq", "XNAS", "USD"),
    ];
    private static readonly IReadOnlyDictionary<string, decimal> Prices = new Dictionary<string, decimal>(StringComparer.Ordinal)
    {
        ["NVDA"] = 180.25m,
        ["AAPL"] = 242.50m,
        ["MSFT"] = 515.75m,
    };

    public Task<IReadOnlyList<MarketInstrument>> SearchAsync(string query, string market, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = Instruments.Where(instrument =>
                instrument.Symbol.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                instrument.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(10).ToArray();
        return Task.FromResult<IReadOnlyList<MarketInstrument>>(result);
    }

    public Task<IReadOnlyList<QuoteSnapshot>> GetLatestQuotesAsync(IReadOnlyList<MarketInstrument> instruments, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var now = timeProvider.GetUtcNow();
        var result = instruments.Select(instrument => new QuoteSnapshot(
            instrument.Normalize(),
            Prices.GetValueOrDefault(instrument.Symbol, 100m),
            "LastTrade",
            Provenance(now, $"synthetic-quote-{instrument.Symbol}"))).ToArray();
        return Task.FromResult<IReadOnlyList<QuoteSnapshot>>(result);
    }

    public Task<CandleSeries?> GetCandlesAsync(Guid instrumentId, string interval, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var instrument = Instruments.SingleOrDefault(candidate => candidate.Id == instrumentId);
        if (instrument is null) return Task.FromResult<CandleSeries?>(null);
        var target = Prices[instrument.Symbol];
        var dates = Enumerable.Range(0, to.DayNumber - from.DayNumber + 1)
            .Select(offset => from.AddDays(offset))
            .Where(date => date.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            .ToArray();
        var candles = dates.Select((date, index) =>
        {
            var close = decimal.Round(target * (0.88m + (0.12m * (index + 1) / Math.Max(1, dates.Length))), 4);
            return new PriceCandle(date, close - 1.25m, close + 2m, close - 2m, close, 1_000_000m + index * 10_000m);
        }).ToArray();
        var now = timeProvider.GetUtcNow();
        return Task.FromResult<CandleSeries?>(new(instrument, interval, candles, Provenance(now, $"synthetic-candles-{instrument.Symbol}")));
    }

    public Task<IReadOnlyList<FxSnapshot>> GetFxAsync(string baseCurrency, string quoteCurrency, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var now = timeProvider.GetUtcNow();
        var result = new[]
        {
            new FxSnapshot(baseCurrency, quoteCurrency, 32.50m, Provenance(now, "synthetic-fx-primary"), false),
            new FxSnapshot(baseCurrency, quoteCurrency, 32.45m, Provenance(now, "synthetic-fx-bot"), true),
        };
        return Task.FromResult<IReadOnlyList<FxSnapshot>>(result);
    }

    private static ObservationProvenance Provenance(DateTimeOffset now, string requestId) =>
        new("Synthetic", "SyntheticFixture", now, now, FreshnessClass.Synthetic, 0, requestId, "synthetic-test-only");
}

public sealed class TwelveDataMarketDataProvider(
    HttpClient httpClient,
    IOptions<MarketDataProviderOptions> options,
    TimeProvider timeProvider) : IMarketDataProvider
{
    private readonly ConcurrentDictionary<Guid, MarketInstrument> instruments = new();

    public async Task<IReadOnlyList<MarketInstrument>> SearchAsync(string query, string market, CancellationToken cancellationToken)
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

    public async Task<IReadOnlyList<QuoteSnapshot>> GetLatestQuotesAsync(IReadOnlyList<MarketInstrument> requested, CancellationToken cancellationToken)
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

    public async Task<CandleSeries?> GetCandlesAsync(Guid instrumentId, string interval, DateOnly from, DateOnly to, CancellationToken cancellationToken)
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

    public async Task<IReadOnlyList<FxSnapshot>> GetFxAsync(string baseCurrency, string quoteCurrency, CancellationToken cancellationToken)
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
