namespace LedgerLens.MarketData.Domain;

public sealed record FxSnapshot(
    string BaseCurrency,
    string QuoteCurrency,
    decimal Rate,
    ObservationProvenance Provenance,
    bool IsBenchmark);
