namespace LedgerLens.MarketData.Domain;

public enum FreshnessClass { RealTime = 1, Delayed, EndOfDay, LatestAvailable, Synthetic }

public sealed record MarketInstrument(
    Guid Id,
    string Symbol,
    string Name,
    string Exchange,
    string Mic,
    string Currency)
{
    public MarketInstrument Normalize()
    {
        if (Id == Guid.Empty || Id.Version != 7) throw new ArgumentException("Instrument ID must be UUIDv7.", nameof(Id));
        return this with
        {
            Symbol = Required(Symbol, 32, nameof(Symbol)).ToUpperInvariant(),
            Name = Required(Name, 200, nameof(Name)),
            Exchange = Required(Exchange, 80, nameof(Exchange)),
            Mic = Required(Mic, 8, nameof(Mic)).ToUpperInvariant(),
            Currency = Required(Currency, 3, nameof(Currency)).ToUpperInvariant(),
        };
    }

    private static string Required(string value, int maximumLength, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Value is required.", name);
        var normalized = value.Trim();
        if (normalized.Length > maximumLength) throw new ArgumentException($"Value cannot exceed {maximumLength} characters.", name);
        return normalized;
    }
}

public sealed record ObservationProvenance(
    string Provider,
    string Feed,
    DateTimeOffset AsOf,
    DateTimeOffset RetrievedAt,
    FreshnessClass Freshness,
    int? DelaySeconds,
    string RequestId,
    string RetentionPolicyKey);

public sealed record QuoteSnapshot(
    MarketInstrument Instrument,
    decimal Price,
    string PriceKind,
    ObservationProvenance Provenance);

public sealed record PriceCandle(
    DateOnly Date,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume);

public sealed record CandleSeries(
    MarketInstrument Instrument,
    string Interval,
    IReadOnlyList<PriceCandle> Candles,
    ObservationProvenance Provenance);

public sealed record FxSnapshot(
    string BaseCurrency,
    string QuoteCurrency,
    decimal Rate,
    ObservationProvenance Provenance,
    bool IsBenchmark);
