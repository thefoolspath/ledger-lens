using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Application;

public sealed class InstrumentCandlesGetOneHandler(IMarketDataProvider provider)
{
    public Task<CandleSeries?> HandleAsync(Guid instrumentId, string interval, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        if (instrumentId == Guid.Empty || instrumentId.Version != 7) throw new ArgumentException("Instrument ID must be UUIDv7.", nameof(instrumentId));
        if (!string.Equals(interval, "1day", StringComparison.Ordinal)) throw new ArgumentException("Only the 1day interval is supported.", nameof(interval));
        if (to < from || to.DayNumber - from.DayNumber > 3660) throw new ArgumentException("Candle range must be ordered and no longer than ten years.");
        return provider.InstrumentCandlesGetOneAsync(instrumentId, interval, from, to, cancellationToken);
    }
}
