using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed class PortfoliosGetListHandler(ICurrentUser currentUser, IPortfolioCoreStore store)
{
    public Task<IReadOnlyList<PortfolioListItem>> HandleAsync(CancellationToken cancellationToken) =>
        store.PortfoliosGetListAsync(currentUser.UserId, cancellationToken);
}
