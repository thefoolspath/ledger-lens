using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using LedgerLens.MarketData.Application;
using LedgerLens.MarketData.Domain;
using Microsoft.Extensions.Options;

namespace LedgerLens.MarketData.Infrastructure;

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

    public Task<IReadOnlyList<MarketInstrument>> InstrumentsSearchListAsync(string query, string market, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = Instruments.Where(instrument =>
                instrument.Symbol.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                instrument.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(10).ToArray();
        return Task.FromResult<IReadOnlyList<MarketInstrument>>(result);
    }

    public Task<IReadOnlyList<QuoteSnapshot>> QuotesGetLatestListAsync(IReadOnlyList<MarketInstrument> instruments, CancellationToken cancellationToken)
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

    public Task<CandleSeries?> InstrumentCandlesGetOneAsync(Guid instrumentId, string interval, DateOnly from, DateOnly to, CancellationToken cancellationToken)
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

    public Task<IReadOnlyList<FxSnapshot>> ForeignExchangeRatesGetLatestListAsync(string baseCurrency, string quoteCurrency, CancellationToken cancellationToken)
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
