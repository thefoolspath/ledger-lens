using LedgerLens.Application.Abstractions;
using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Application.Features.ForeignExchangeRates.GetLatestList;

public sealed class ForeignExchangeRatesGetLatestListHandler(IMarketDataProvider provider)
    : IQueryHandler<ForeignExchangeRatesGetLatestListQuery, IReadOnlyList<FxSnapshot>>
{
    public Task<IReadOnlyList<FxSnapshot>> HandleAsync(
        ForeignExchangeRatesGetLatestListQuery query,
        CancellationToken cancellationToken)
    {
        if (query.BaseCurrency.Length != 3 || query.QuoteCurrency.Length != 3)
            throw new ArgumentException("FX currencies must be ISO three-letter codes.", nameof(query));
        return provider.ForeignExchangeRatesGetLatestListAsync(
            query.BaseCurrency.ToUpperInvariant(), query.QuoteCurrency.ToUpperInvariant(), cancellationToken);
    }
}
