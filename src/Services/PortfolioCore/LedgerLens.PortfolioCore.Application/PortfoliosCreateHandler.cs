using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed class PortfoliosCreateHandler(ICurrentUser currentUser, IPortfolioCoreStore store, TimeProvider timeProvider)
{
    public async Task<PortfolioListItem> HandleAsync(PortfoliosCreateCommand command, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var user = new UserProfile(currentUser.UserId, currentUser.Email, now);
        var portfolio = new Portfolio(DomainId.New(), currentUser.UserId, command.Name, command.BaseCurrency, command.ReportingCurrency, now);
        await store.PortfoliosCreateAsync(user, portfolio, cancellationToken);
        return new(portfolio.Id, portfolio.Name, portfolio.BaseCurrency, portfolio.ReportingCurrency);
    }
}
