using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed class SimulationValuationsRecordListHandler(ICurrentUser currentUser, ISimulationStore store, TimeProvider timeProvider)
{
    public async Task<IReadOnlyList<SimulationValuationView>?> HandleAsync(SimulationValuationsRecordListCommand command, CancellationToken cancellationToken)
    {
        if (await store.SimulationAccountsGetOneAsync(command.AccountId, currentUser.UserId, cancellationToken) is null) return null;
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
        await store.SimulationValuationsRecordListAsync(values.Where(value => value.Quote is not null).Select(value =>
            new SimulationValuationSnapshotData(DomainId.New(), command.AccountId, value, now)).ToArray(), cancellationToken);
        return values;
    }
}
