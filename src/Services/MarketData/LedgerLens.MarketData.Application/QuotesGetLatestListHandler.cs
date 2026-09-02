using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Application;

public sealed class QuotesGetLatestListHandler(IMarketDataProvider provider)
{
    public Task<IReadOnlyList<QuoteSnapshot>> HandleAsync(IReadOnlyList<MarketInstrument> instruments, CancellationToken cancellationToken)
    {
        if (instruments.Count is < 1 or > 25) throw new ArgumentException("Request must contain 1 to 25 instruments.", nameof(instruments));
        return provider.QuotesGetLatestListAsync(instruments.Select(instrument => instrument.Normalize()).ToArray(), cancellationToken);
    }
}
