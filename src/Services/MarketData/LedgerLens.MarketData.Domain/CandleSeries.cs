namespace LedgerLens.MarketData.Domain;

public sealed record CandleSeries(
    MarketInstrument Instrument,
    string Interval,
    IReadOnlyList<PriceCandle> Candles,
    ObservationProvenance Provenance);
