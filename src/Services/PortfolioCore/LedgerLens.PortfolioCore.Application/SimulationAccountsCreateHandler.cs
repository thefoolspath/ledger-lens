using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed class SimulationAccountsCreateHandler(ICurrentUser currentUser, IPortfolioCoreStore portfolioStore,
    ISimulationStore simulationStore, TimeProvider timeProvider)
{
    public async Task<SimulationAccountListItem?> HandleAsync(SimulationAccountsCreateCommand command, CancellationToken cancellationToken)
    {
        if (!await portfolioStore.PortfolioIsOwnedByAsync(command.PortfolioId, currentUser.UserId, cancellationToken)) return null;
        var account = new SimulationAccount(DomainId.New(), command.PortfolioId, command.Name, timeProvider.GetUtcNow());
        await simulationStore.SimulationAccountsCreateAsync(account, cancellationToken);
        return ToListItem(account);
    }

    internal static SimulationAccountListItem ToListItem(SimulationAccount account) =>
        new(account.Id, account.PortfolioId, account.Name, account.CreatedAt);
}
