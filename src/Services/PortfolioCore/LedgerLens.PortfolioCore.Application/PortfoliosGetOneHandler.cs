using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed class PortfoliosGetOneHandler(ICurrentUser currentUser, IPortfolioCoreStore store)
{
    public Task<PortfolioOverview?> HandleAsync(Guid portfolioId, CancellationToken cancellationToken) =>
        store.PortfoliosGetOneAsync(portfolioId, currentUser.UserId, cancellationToken);
}
