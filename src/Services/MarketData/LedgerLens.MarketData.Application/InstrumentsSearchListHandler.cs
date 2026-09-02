using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Application;

public sealed class InstrumentsSearchListHandler(IMarketDataProvider provider)
{
    public Task<IReadOnlyList<MarketInstrument>> HandleAsync(string query, string market, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length > 64) throw new ArgumentException("Query must contain 1 to 64 characters.", nameof(query));
        if (string.IsNullOrWhiteSpace(market) || market.Trim().Length > 16) throw new ArgumentException("Market must contain 1 to 16 characters.", nameof(market));
        return provider.InstrumentsSearchListAsync(query.Trim(), market.Trim(), cancellationToken);
    }
}
