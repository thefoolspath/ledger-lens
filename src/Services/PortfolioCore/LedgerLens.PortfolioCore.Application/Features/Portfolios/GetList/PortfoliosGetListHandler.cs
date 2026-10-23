using LedgerLens.PortfolioCore.Domain;

using LedgerLens.Application.Abstractions;

namespace LedgerLens.PortfolioCore.Application.Features.Portfolios.GetList;

public sealed class PortfoliosGetListHandler(ICurrentUser currentUser, IPortfolioCoreStore store)
    : IQueryHandler<PortfoliosGetListQuery, IReadOnlyList<PortfolioListItem>>
{
    public Task<IReadOnlyList<PortfolioListItem>> HandleAsync(
        PortfoliosGetListQuery query,
        CancellationToken cancellationToken) =>
        store.PortfoliosGetListAsync(currentUser.UserId, cancellationToken);
}
