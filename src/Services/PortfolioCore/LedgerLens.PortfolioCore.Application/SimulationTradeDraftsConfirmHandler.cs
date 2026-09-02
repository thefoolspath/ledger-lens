using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed class SimulationTradeDraftsConfirmHandler(ICurrentUser currentUser, ISimulationStore store, TimeProvider timeProvider)
{
    public async Task<SimulationTradeView?> HandleAsync(Guid accountId, Guid draftId, CancellationToken cancellationToken)
    {
        var draft = await store.FindOwnedSimulationDraftAsync(draftId, currentUser.UserId, cancellationToken);
        if (draft is null) return null;
        if (draft.SimulationAccountId != accountId) return null;
        var now = timeProvider.GetUtcNow();
        if (draft.ConfirmedTradeId is null && now > draft.ExpiresAt) throw new InvalidOperationException("Simulation trade draft has expired; fetch a new quote.");
        var trade = SimulationTradeEntry.FromDraft(DomainId.New(), draft, now);
        var confirmed = await store.SimulationTradeDraftsConfirmAsync(draft, trade, currentUser.UserId, cancellationToken);
        return confirmed is null ? null : ToView(confirmed);
    }

    internal static SimulationTradeView ToView(SimulationTradeEntry entry) => new(entry.Id, entry.Side.ToString(), entry.Quantity,
        entry.UnitPrice, entry.GrossAmount, entry.AssumedFee, entry.AssumedTax, entry.FxRate, entry.NetCash,
        entry.EffectiveAt, entry.RecordedAt, entry.Evidence.Symbol, entry.Evidence.Provider, entry.Evidence.Feed,
        entry.Evidence.AsOf, entry.Evidence.Freshness, entry.Role.ToString(), entry.CorrectsEntryId, entry.CorrectionReason);
}
