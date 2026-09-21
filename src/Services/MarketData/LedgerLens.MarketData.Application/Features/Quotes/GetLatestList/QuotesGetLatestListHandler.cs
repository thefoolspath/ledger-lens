using LedgerLens.Application.Abstractions;
using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Application.Features.Quotes.GetLatestList;

public sealed class QuotesGetLatestListHandler(IMarketDataProvider provider)
    : IQueryHandler<QuotesGetLatestListQuery, IReadOnlyList<QuoteSnapshot>>
{
    public Task<IReadOnlyList<QuoteSnapshot>> HandleAsync(
        QuotesGetLatestListQuery query,
        CancellationToken cancellationToken)
    {
        if (query.Instruments.Count is < 1 or > 25)
            throw new ArgumentException("Request must contain 1 to 25 instruments.", nameof(query));
        return provider.QuotesGetLatestListAsync(
            query.Instruments.Select(instrument => instrument.Normalize()).ToArray(),
            cancellationToken);
    }
}
