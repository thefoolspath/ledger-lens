namespace LedgerLens.MarketData.Domain;

public sealed record PriceCandle(
    DateOnly Date,
    decimal Open,
    decimal High,
    decimal Low,
    decimal Close,
    decimal Volume);
