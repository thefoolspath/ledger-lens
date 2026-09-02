namespace LedgerLens.MarketData.Domain;

public sealed record QuoteSnapshot(
    MarketInstrument Instrument,
    decimal Price,
    string PriceKind,
    ObservationProvenance Provenance);
