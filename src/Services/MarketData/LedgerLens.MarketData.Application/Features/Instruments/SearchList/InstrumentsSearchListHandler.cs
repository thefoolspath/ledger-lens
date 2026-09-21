using LedgerLens.Application.Abstractions;
using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Application.Features.Instruments.SearchList;

public sealed class InstrumentsSearchListHandler(IMarketDataProvider provider)
    : IQueryHandler<InstrumentsSearchListQuery, IReadOnlyList<MarketInstrument>>
{
    public Task<IReadOnlyList<MarketInstrument>> HandleAsync(
        InstrumentsSearchListQuery query,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Query) || query.Query.Length > 64)
            throw new ArgumentException("Query must contain 1 to 64 characters.", nameof(query));
        if (string.IsNullOrWhiteSpace(query.Market) || query.Market.Length > 16)
            throw new ArgumentException("Market must contain 1 to 16 characters.", nameof(query));
        return provider.InstrumentsSearchListAsync(query.Query, query.Market, cancellationToken);
    }
}
