using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed class SimulationTradesCorrectHandler(ICurrentUser currentUser, ISimulationStore store, TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<SimulationTradeView>?> HandleAsync(SimulationTradesCorrectCommand command, CancellationToken cancellationToken)
    {
        var corrected = await store.FindOwnedSimulationTradeAsync(command.TradeId, currentUser.UserId, cancellationToken);
        var draft = await store.FindOwnedSimulationDraftAsync(command.ReplacementDraftId, currentUser.UserId, cancellationToken);
        if (corrected is null || draft is null) return null;
        if (corrected.Role == SimulationEntryRole.Reversal) throw new InvalidOperationException("A reversal cannot be corrected.");
        if (draft.SimulationAccountId != corrected.SimulationAccountId) throw new ArgumentException("Replacement draft must belong to the same simulation account.");
        if (draft.ConfirmedTradeId is not null) throw new InvalidOperationException("Replacement draft is already confirmed.");
        var now = timeProvider.GetUtcNow();
        if (now > draft.ExpiresAt) throw new InvalidOperationException("Replacement draft has expired.");
        var reversal = SimulationTradeEntry.Reversal(DomainId.New(), corrected, command.Reason, now);
        var replacement = SimulationTradeEntry.Replacement(DomainId.New(), draft, corrected.Id, command.Reason, now);
        var existing = await store.GetSimulationEntriesAsync(corrected.SimulationAccountId, currentUser.UserId, cancellationToken);
        _ = SimulationCalculator.Rebuild([.. existing, reversal, replacement]);
        if (!await store.SimulationTradesCorrectAsync(corrected, reversal, replacement, draft.Id, currentUser.UserId, cancellationToken))
            throw new InvalidOperationException("Simulation trade has already been corrected.");
        return [SimulationTradeDraftsConfirmHandler.ToView(reversal), SimulationTradeDraftsConfirmHandler.ToView(replacement)];
    }
}
