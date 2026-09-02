using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Application;

public sealed class ForeignExchangeRatesGetLatestListHandler(IMarketDataProvider provider)
{
    public Task<IReadOnlyList<FxSnapshot>> HandleAsync(string baseCurrency, string quoteCurrency, CancellationToken cancellationToken)
    {
        if (baseCurrency.Length != 3 || quoteCurrency.Length != 3) throw new ArgumentException("FX currencies must be ISO three-letter codes.");
        return provider.ForeignExchangeRatesGetLatestListAsync(baseCurrency.ToUpperInvariant(), quoteCurrency.ToUpperInvariant(), cancellationToken);
    }
}
