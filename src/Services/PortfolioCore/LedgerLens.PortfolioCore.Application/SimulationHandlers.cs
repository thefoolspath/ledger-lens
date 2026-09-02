using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record CreateSimulationAccountCommand(Guid PortfolioId, string Name);
public sealed class CreateSimulationAccountHandler(ICurrentUser currentUser, IPortfolioCoreStore portfolioStore,
    ISimulationStore simulationStore, TimeProvider timeProvider)
{
    public async Task<SimulationAccountListItem?> HandleAsync(CreateSimulationAccountCommand command, CancellationToken cancellationToken)
    {
        if (!await portfolioStore.PortfolioIsOwnedByAsync(command.PortfolioId, currentUser.UserId, cancellationToken)) return null;
        var account = new SimulationAccount(DomainId.New(), command.PortfolioId, command.Name, timeProvider.GetUtcNow());
        await simulationStore.AddSimulationAccountAsync(account, cancellationToken);
        return ToListItem(account);
    }

    internal static SimulationAccountListItem ToListItem(SimulationAccount account) =>
        new(account.Id, account.PortfolioId, account.Name, account.CreatedAt);
}

public sealed class ListSimulationAccountsHandler(ICurrentUser currentUser, ISimulationStore store)
{
    public Task<IReadOnlyList<SimulationAccountListItem>> HandleAsync(Guid portfolioId, CancellationToken cancellationToken) =>
        store.ListSimulationAccountsAsync(portfolioId, currentUser.UserId, cancellationToken);
}

public sealed record CreateSimulationDraftCommand(Guid AccountId, SimulationTradeSide Side, SimulationInputMode InputMode,
    decimal? RequestedQuantity, decimal? RequestedAmount, decimal AssumedFee, decimal AssumedTax, decimal? FxRate,
    DateTimeOffset EffectiveAt, SimulationMarketEvidence Quote);

public sealed class CreateSimulationDraftHandler(ICurrentUser currentUser, ISimulationStore store, TimeProvider timeProvider)
{
    public async Task<SimulationDraftView?> HandleAsync(CreateSimulationDraftCommand command, CancellationToken cancellationToken)
    {
        if (await store.FindOwnedSimulationAccountAsync(command.AccountId, currentUser.UserId, cancellationToken) is null) return null;
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
        await store.AddSimulationDraftAsync(draft, cancellationToken);
        return ToView(draft);
    }

    internal static SimulationDraftView ToView(SimulationTradeDraft draft) => new(draft.Id, draft.SimulationAccountId,
        draft.Side.ToString(), draft.InputMode.ToString(), draft.RequestedQuantity, draft.RequestedAmount, draft.Quantity,
        draft.GrossAmount, draft.UnusedAmount, draft.AssumedFee, draft.AssumedTax, draft.NetCash, draft.AcquisitionFxRate,
        draft.EffectiveAt, draft.ExpiresAt, draft.Evidence, draft.ConfirmedTradeId);
}

public sealed class ConfirmSimulationDraftHandler(ICurrentUser currentUser, ISimulationStore store, TimeProvider timeProvider)
{
    public async Task<SimulationTradeView?> HandleAsync(Guid accountId, Guid draftId, CancellationToken cancellationToken)
    {
        var draft = await store.FindOwnedSimulationDraftAsync(draftId, currentUser.UserId, cancellationToken);
        if (draft is null) return null;
        if (draft.SimulationAccountId != accountId) return null;
        var now = timeProvider.GetUtcNow();
        if (draft.ConfirmedTradeId is null && now > draft.ExpiresAt) throw new InvalidOperationException("Simulation trade draft has expired; fetch a new quote.");
        var trade = SimulationTradeEntry.FromDraft(DomainId.New(), draft, now);
        var confirmed = await store.ConfirmSimulationDraftAsync(draft, trade, currentUser.UserId, cancellationToken);
        return confirmed is null ? null : ToView(confirmed);
    }

    internal static SimulationTradeView ToView(SimulationTradeEntry entry) => new(entry.Id, entry.Side.ToString(), entry.Quantity,
        entry.UnitPrice, entry.GrossAmount, entry.AssumedFee, entry.AssumedTax, entry.FxRate, entry.NetCash,
        entry.EffectiveAt, entry.RecordedAt, entry.Evidence.Symbol, entry.Evidence.Provider, entry.Evidence.Feed,
        entry.Evidence.AsOf, entry.Evidence.Freshness, entry.Role.ToString(), entry.CorrectsEntryId, entry.CorrectionReason);
}

public sealed record CorrectSimulationTradeCommand(Guid TradeId, Guid ReplacementDraftId, string Reason);
public sealed class CorrectSimulationTradeHandler(ICurrentUser currentUser, ISimulationStore store, TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<SimulationTradeView>?> HandleAsync(CorrectSimulationTradeCommand command, CancellationToken cancellationToken)
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
        if (!await store.CorrectSimulationTradeAsync(corrected, reversal, replacement, draft.Id, currentUser.UserId, cancellationToken))
            throw new InvalidOperationException("Simulation trade has already been corrected.");
        return [ConfirmSimulationDraftHandler.ToView(reversal), ConfirmSimulationDraftHandler.ToView(replacement)];
    }
}

public sealed class GetSimulationOverviewHandler(ICurrentUser currentUser, ISimulationStore store)
{
    public async Task<SimulationAccountOverview?> HandleAsync(Guid accountId, CancellationToken cancellationToken)
    {
        var account = await store.FindOwnedSimulationAccountAsync(accountId, currentUser.UserId, cancellationToken);
        if (account is null) return null;
        var entries = await store.GetSimulationEntriesAsync(accountId, currentUser.UserId, cancellationToken);
        return new(CreateSimulationAccountHandler.ToListItem(account),
            SimulationCalculator.Rebuild(entries).Select(ToPosition).ToArray(),
            entries.OrderByDescending(entry => entry.EffectiveAt).ThenByDescending(entry => entry.Id)
                .Select(ConfirmSimulationDraftHandler.ToView).ToArray());
    }

    internal static SimulationPositionView ToPosition(SimulationPosition position) => new(position.Symbol, position.Quantity,
        position.RemainingCostUsd, position.AverageCostUsd, position.RealizedUsd, position.InvestedCapitalUsd,
        position.RemainingCostThb, position.RealizedThb);
}

public sealed record RecordSimulationValuationCommand(Guid AccountId, IReadOnlyList<SimulationMarketEvidence> Quotes,
    decimal? CurrentFxRate);
public sealed class RecordSimulationValuationHandler(ICurrentUser currentUser, ISimulationStore store, TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<SimulationValuationView>?> HandleAsync(RecordSimulationValuationCommand command, CancellationToken cancellationToken)
    {
        if (await store.FindOwnedSimulationAccountAsync(command.AccountId, currentUser.UserId, cancellationToken) is null) return null;
        if (command.Quotes.Count is < 1 or > 25) throw new ArgumentException("Valuation requires 1 to 25 quotes.");
        var quotes = command.Quotes.Select(quote => quote.Normalize()).ToDictionary(quote => quote.Symbol, StringComparer.Ordinal);
        var positions = SimulationCalculator.Rebuild(await store.GetSimulationEntriesAsync(command.AccountId, currentUser.UserId, cancellationToken));
        var values = new List<SimulationValuationView>();
        foreach (var position in positions.Where(position => position.Quantity > 0m))
        {
            if (!quotes.TryGetValue(position.Symbol, out var quote))
            {
                values.Add(new(position.Symbol, position.Quantity, position.RemainingCostUsd, position.AverageCostUsd,
                    position.RealizedUsd, position.InvestedCapitalUsd, null, null, null, null, null,
                    command.CurrentFxRate, null, null, null, null, null, false,
                    ["Latest LastTrade observation is missing."], null));
                continue;
            }
            var value = SimulationCalculator.Value(position, quote.Price, command.CurrentFxRate);
            values.Add(new(value.Position.Symbol, value.Position.Quantity, value.Position.RemainingCostUsd,
                value.Position.AverageCostUsd, value.Position.RealizedUsd, value.Position.InvestedCapitalUsd,
                value.CurrentPrice, value.CurrentValueUsd, value.UnrealizedUsd, value.TotalPlUsd, value.ReturnPercent,
                value.CurrentFxRate, value.CurrentValueThb, value.UnrealizedThb,
                value.TotalPlThb, value.StockEffectThb, value.FxEffectThb, value.IsComplete, value.MissingReasons, quote));
        }
        var now = timeProvider.GetUtcNow();
        await store.AddSimulationValuationSnapshotsAsync(values.Where(value => value.Quote is not null).Select(value =>
            new SimulationValuationSnapshotData(DomainId.New(), command.AccountId, value, now)).ToArray(), cancellationToken);
        return values;
    }
}

public sealed class GetSimulationValuationSeriesHandler(ICurrentUser currentUser, ISimulationStore store)
{
    public async Task<IReadOnlyList<SimulationSeriesPoint>?> HandleAsync(Guid accountId, string symbol,
        IReadOnlyList<SimulationSeriesInput> observations, CancellationToken cancellationToken)
    {
        if (await store.FindOwnedSimulationAccountAsync(accountId, currentUser.UserId, cancellationToken) is null) return null;
        if (observations.Count is < 1 or > 3660) throw new ArgumentException("Series requires 1 to 3660 daily observations.");
        var normalizedSymbol = SimulationMarketEvidence.SymbolValue(symbol);
        var entries = await store.GetSimulationEntriesAsync(accountId, currentUser.UserId, cancellationToken);
        var result = new List<SimulationSeriesPoint>();
        foreach (var observation in observations.OrderBy(item => item.Date))
        {
            var cutoff = new DateTimeOffset(observation.Date.ToDateTime(TimeOnly.MaxValue), TimeSpan.Zero);
            var position = SimulationCalculator.Rebuild(entries.Where(entry => entry.EffectiveAt <= cutoff).ToArray())
                .SingleOrDefault(item => item.Symbol == normalizedSymbol);
            if (position is null) continue;
            var valuation = SimulationCalculator.Value(position, observation.Price, observation.FxRate);
            result.Add(new(observation.Date, normalizedSymbol, position.Quantity, position.RemainingCostUsd,
                valuation.CurrentValueUsd, valuation.UnrealizedUsd, valuation.CurrentValueThb, valuation.IsComplete));
        }
        return result;
    }
}
