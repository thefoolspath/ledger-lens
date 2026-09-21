using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application.Features.SimulationValuations.CalculateSeriesList;

public sealed class SimulationValuationsCalculateSeriesListHandler(ICurrentUser currentUser, ISimulationStore store)
    : IQueryHandler<SimulationValuationsCalculateSeriesListQuery, IReadOnlyList<SimulationSeriesPoint>?>
{
    public async Task<IReadOnlyList<SimulationSeriesPoint>?> HandleAsync(
        SimulationValuationsCalculateSeriesListQuery query,
        CancellationToken cancellationToken)
    {
        if (await store.SimulationAccountsGetOneAsync(query.AccountId, currentUser.UserId, cancellationToken) is null) return null;
        if (query.Observations.Count is < 1 or > 3660)
            throw new ArgumentException("Series requires 1 to 3660 daily observations.", nameof(query));
        var normalizedSymbol = SimulationMarketEvidence.SymbolValue(query.Symbol);
        var entries = await store.SimulationValuationsCalculateSeriesListAsync(
            query.AccountId, currentUser.UserId, cancellationToken);
        var result = new List<SimulationSeriesPoint>();
        foreach (var observation in query.Observations.OrderBy(item => item.Date))
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
