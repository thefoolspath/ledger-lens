using LedgerLens.PortfolioCore.Domain;

using LedgerLens.Application.Abstractions;

namespace LedgerLens.PortfolioCore.Application.Features.Portfolios.GetOne;

public sealed class PortfoliosGetOneHandler(ICurrentUser currentUser, IPortfolioCoreStore store)
    : IQueryHandler<PortfoliosGetOneQuery, PortfolioOverview?>
{
    public Task<PortfolioOverview?> HandleAsync(PortfoliosGetOneQuery query, CancellationToken cancellationToken) =>
        store.PortfoliosGetOneAsync(query.PortfolioId, currentUser.UserId, cancellationToken);
}
