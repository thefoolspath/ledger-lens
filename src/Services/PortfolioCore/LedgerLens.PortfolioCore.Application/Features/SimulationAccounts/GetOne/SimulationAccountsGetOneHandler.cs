using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application.Features.SimulationAccounts.Create;
using LedgerLens.PortfolioCore.Application.Features.SimulationTradeDrafts.Confirm;
using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application.Features.SimulationAccounts.GetOne;

public sealed class SimulationAccountsGetOneHandler(ICurrentUser currentUser, ISimulationStore store)
    : IQueryHandler<SimulationAccountsGetOneQuery, SimulationAccountOverview?>
{
    public async Task<SimulationAccountOverview?> HandleAsync(
        SimulationAccountsGetOneQuery query,
        CancellationToken cancellationToken)
    {
        var account = await store.SimulationAccountsGetOneAsync(query.AccountId, currentUser.UserId, cancellationToken);
        if (account is null) return null;
        var entries = await store.GetSimulationEntriesAsync(query.AccountId, currentUser.UserId, cancellationToken);
        return new(SimulationAccountsCreateHandler.ToListItem(account),
            SimulationCalculator.Rebuild(entries).Select(ToPosition).ToArray(),
            entries.OrderByDescending(entry => entry.EffectiveAt).ThenByDescending(entry => entry.Id)
                .Select(SimulationTradeDraftsConfirmHandler.ToView).ToArray());
    }

    internal static SimulationPositionView ToPosition(SimulationPosition position) => new(position.Symbol, position.Quantity,
        position.RemainingCostUsd, position.AverageCostUsd, position.RealizedUsd, position.InvestedCapitalUsd,
        position.RemainingCostThb, position.RealizedThb);
}
