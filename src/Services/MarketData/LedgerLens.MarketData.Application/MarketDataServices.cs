using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Application;

public interface IMarketDataProvider
{
    Task<IReadOnlyList<MarketInstrument>> SearchAsync(string query, string market, CancellationToken cancellationToken);
    Task<IReadOnlyList<QuoteSnapshot>> GetLatestQuotesAsync(IReadOnlyList<MarketInstrument> instruments, CancellationToken cancellationToken);
    Task<CandleSeries?> GetCandlesAsync(Guid instrumentId, string interval, DateOnly from, DateOnly to, CancellationToken cancellationToken);
    Task<IReadOnlyList<FxSnapshot>> GetFxAsync(string baseCurrency, string quoteCurrency, CancellationToken cancellationToken);
}

public sealed class MarketDataUnavailableException(string message) : Exception(message);

public sealed class SearchInstrumentsHandler(IMarketDataProvider provider)
{
    public Task<IReadOnlyList<MarketInstrument>> HandleAsync(string query, string market, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length > 64) throw new ArgumentException("Query must contain 1 to 64 characters.", nameof(query));
        if (string.IsNullOrWhiteSpace(market) || market.Trim().Length > 16) throw new ArgumentException("Market must contain 1 to 16 characters.", nameof(market));
        return provider.SearchAsync(query.Trim(), market.Trim(), cancellationToken);
    }
}

public sealed class GetLatestQuotesHandler(IMarketDataProvider provider)
{
    public Task<IReadOnlyList<QuoteSnapshot>> HandleAsync(IReadOnlyList<MarketInstrument> instruments, CancellationToken cancellationToken)
    {
        if (instruments.Count is < 1 or > 25) throw new ArgumentException("Request must contain 1 to 25 instruments.", nameof(instruments));
        return provider.GetLatestQuotesAsync(instruments.Select(instrument => instrument.Normalize()).ToArray(), cancellationToken);
    }
}

public sealed class GetCandlesHandler(IMarketDataProvider provider)
{
    public Task<CandleSeries?> HandleAsync(Guid instrumentId, string interval, DateOnly from, DateOnly to, CancellationToken cancellationToken)
    {
        if (instrumentId == Guid.Empty || instrumentId.Version != 7) throw new ArgumentException("Instrument ID must be UUIDv7.", nameof(instrumentId));
        if (!string.Equals(interval, "1day", StringComparison.Ordinal)) throw new ArgumentException("Only the 1day interval is supported.", nameof(interval));
        if (to < from || to.DayNumber - from.DayNumber > 3660) throw new ArgumentException("Candle range must be ordered and no longer than ten years.");
        return provider.GetCandlesAsync(instrumentId, interval, from, to, cancellationToken);
    }
}

public sealed class GetFxHandler(IMarketDataProvider provider)
{
    public Task<IReadOnlyList<FxSnapshot>> HandleAsync(string baseCurrency, string quoteCurrency, CancellationToken cancellationToken)
    {
        if (baseCurrency.Length != 3 || quoteCurrency.Length != 3) throw new ArgumentException("FX currencies must be ISO three-letter codes.");
        return provider.GetFxAsync(baseCurrency.ToUpperInvariant(), quoteCurrency.ToUpperInvariant(), cancellationToken);
    }
}
