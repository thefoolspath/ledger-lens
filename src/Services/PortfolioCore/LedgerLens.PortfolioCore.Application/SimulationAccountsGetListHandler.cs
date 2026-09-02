using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed class SimulationAccountsGetListHandler(ICurrentUser currentUser, ISimulationStore store)
{
    public Task<IReadOnlyList<SimulationAccountListItem>> HandleAsync(Guid portfolioId, CancellationToken cancellationToken) =>
        store.SimulationAccountsGetListAsync(portfolioId, currentUser.UserId, cancellationToken);
}
