namespace LedgerLens.MarketData.Application.Features.InstrumentCandles.GetOne;

public sealed record InstrumentCandlesGetOneQuery(Guid InstrumentId, string Interval, DateOnly From, DateOnly To);
