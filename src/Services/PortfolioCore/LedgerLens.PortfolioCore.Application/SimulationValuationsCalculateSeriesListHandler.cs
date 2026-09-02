using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed class SimulationValuationsCalculateSeriesListHandler(ICurrentUser currentUser, ISimulationStore store)
{
    public async Task<IReadOnlyList<SimulationSeriesPoint>?> HandleAsync(Guid accountId, string symbol,
        IReadOnlyList<SimulationSeriesInput> observations, CancellationToken cancellationToken)
    {
        if (await store.SimulationAccountsGetOneAsync(accountId, currentUser.UserId, cancellationToken) is null) return null;
        if (observations.Count is < 1 or > 3660) throw new ArgumentException("Series requires 1 to 3660 daily observations.");
        var normalizedSymbol = SimulationMarketEvidence.SymbolValue(symbol);
        var entries = await store.SimulationValuationsCalculateSeriesListAsync(
            accountId, currentUser.UserId, cancellationToken);
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
