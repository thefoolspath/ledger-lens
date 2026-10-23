using LedgerLens.Application.Abstractions;
using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Application.Features.InstrumentCandles.GetOne;

public sealed class InstrumentCandlesGetOneHandler(IMarketDataProvider provider)
    : IQueryHandler<InstrumentCandlesGetOneQuery, CandleSeries?>
{
    public Task<CandleSeries?> HandleAsync(
        InstrumentCandlesGetOneQuery query,
        CancellationToken cancellationToken)
    {
        if (query.InstrumentId == Guid.Empty || query.InstrumentId.Version != 7)
            throw new ArgumentException("Instrument ID must be UUIDv7.", nameof(query));
        if (!string.Equals(query.Interval, "1day", StringComparison.Ordinal))
            throw new ArgumentException("Only the 1day interval is supported.", nameof(query));
        if (query.To < query.From || query.To.DayNumber - query.From.DayNumber > 3660)
            throw new ArgumentException("Candle range must be ordered and no longer than ten years.", nameof(query));
        return provider.InstrumentCandlesGetOneAsync(
            query.InstrumentId, query.Interval, query.From, query.To, cancellationToken);
    }
}
