using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed class SimulationTradeDraftsCreateHandler(ICurrentUser currentUser, ISimulationStore store, TimeProvider timeProvider)
{
    public async Task<SimulationDraftView?> HandleAsync(SimulationTradeDraftsCreateCommand command, CancellationToken cancellationToken)
    {
        if (await store.SimulationAccountsGetOneAsync(command.AccountId, currentUser.UserId, cancellationToken) is null) return null;
        var now = timeProvider.GetUtcNow();
        var draft = SimulationTradeDraft.Create(DomainId.New(), command.AccountId, command.Side, command.InputMode,
            command.RequestedQuantity, command.RequestedAmount, command.AssumedFee, command.AssumedTax, command.FxRate,
            command.EffectiveAt, now, command.Quote);
        if (draft.Side == SimulationTradeSide.Sell)
        {
            var positions = SimulationCalculator.Rebuild(await store.GetSimulationEntriesAsync(command.AccountId, currentUser.UserId, cancellationToken));
            var available = positions.SingleOrDefault(position => position.Symbol == draft.Evidence.Symbol)?.Quantity ?? 0m;
            if (draft.Quantity > available) throw new InvalidOperationException($"Simulation sell exceeds the available {available} shares.");
        }
        await store.SimulationTradeDraftsCreateAsync(draft, cancellationToken);
        return ToView(draft);
    }

    internal static SimulationDraftView ToView(SimulationTradeDraft draft) => new(draft.Id, draft.SimulationAccountId,
        draft.Side.ToString(), draft.InputMode.ToString(), draft.RequestedQuantity, draft.RequestedAmount, draft.Quantity,
        draft.GrossAmount, draft.UnusedAmount, draft.AssumedFee, draft.AssumedTax, draft.NetCash, draft.AcquisitionFxRate,
        draft.EffectiveAt, draft.ExpiresAt, draft.Evidence, draft.ConfirmedTradeId);
}
