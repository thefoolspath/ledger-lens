using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed class SimulationAccountsGetOneHandler(ICurrentUser currentUser, ISimulationStore store)
{
    public async Task<SimulationAccountOverview?> HandleAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var account = await store.SimulationAccountsGetOneAsync(accountId, currentUser.UserId, cancellationToken);
        if (account is null) return null;
        var entries = await store.GetSimulationEntriesAsync(accountId, currentUser.UserId, cancellationToken);
        return new(SimulationAccountsCreateHandler.ToListItem(account),
            SimulationCalculator.Rebuild(entries).Select(ToPosition).ToArray(),
            entries.OrderByDescending(entry => entry.EffectiveAt).ThenByDescending(entry => entry.Id)
                .Select(SimulationTradeDraftsConfirmHandler.ToView).ToArray());
    }

    internal static SimulationPositionView ToPosition(SimulationPosition position) => new(position.Symbol, position.Quantity,
        position.RemainingCostUsd, position.AverageCostUsd, position.RealizedUsd, position.InvestedCapitalUsd,
        position.RemainingCostThb, position.RealizedThb);
}
