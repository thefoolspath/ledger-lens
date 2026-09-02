namespace LedgerLens.PortfolioCore.Domain;

public sealed record SimulationPosition(
    string Symbol,
    decimal Quantity,
    decimal RemainingCostUsd,
    decimal? AverageCostUsd,
    decimal RealizedUsd,
    decimal InvestedCapitalUsd,
    decimal? RemainingCostThb,
    decimal? RealizedThb);

public sealed record SimulationValuation(
    SimulationPosition Position,
    decimal CurrentPrice,
    decimal CurrentValueUsd,
    decimal UnrealizedUsd,
    decimal TotalPlUsd,
    decimal? ReturnPercent,
    decimal? CurrentFxRate,
    decimal? CurrentValueThb,
    decimal? UnrealizedThb,
    decimal? TotalPlThb,
    decimal? StockEffectThb,
    decimal? FxEffectThb,
    bool IsComplete,
    IReadOnlyList<string> MissingReasons);

public static class SimulationCalculator
{
    public static IReadOnlyList<SimulationPosition> Rebuild(IReadOnlyList<SimulationTradeEntry> entries)
    {
        var correctedIds = entries.Where(entry => entry.Role == SimulationEntryRole.Reversal && entry.CorrectsEntryId is not null)
            .Select(entry => entry.CorrectsEntryId!.Value).ToHashSet();
        var active = entries.Where(entry => entry.Role != SimulationEntryRole.Reversal && !correctedIds.Contains(entry.Id))
            .OrderBy(entry => entry.EffectiveAt).ThenBy(entry => entry.RecordedAt).ThenBy(entry => entry.Id);
        var results = new List<SimulationPosition>();
        foreach (var group in active.GroupBy(entry => entry.Evidence.Symbol, StringComparer.Ordinal))
        {
            var lots = new Queue<Lot>();
            decimal realizedUsd = 0m;
            decimal investedCapitalUsd = 0m;
            decimal? realizedThb = 0m;
            foreach (var entry in group)
            {
                if (entry.Side == SimulationTradeSide.Buy)
                {
                    var cost = entry.GrossAmount + entry.AssumedFee + entry.AssumedTax;
                    investedCapitalUsd += cost;
                    lots.Enqueue(new Lot(entry.Quantity, cost, entry.FxRate));
                    continue;
                }

                var remainingSale = entry.Quantity;
                decimal allocatedCost = 0m;
                decimal? allocatedCostThb = 0m;
                while (remainingSale > 0m)
                {
                    if (!lots.TryPeek(out var lot)) throw new InvalidOperationException($"Simulation sell exceeds holding for {group.Key}.");
                    var used = Math.Min(remainingSale, lot.Quantity);
                    var cost = lot.CostUsd * used / lot.Quantity;
                    allocatedCost += cost;
                    if (lot.FxRate is null) allocatedCostThb = null;
                    else if (allocatedCostThb is not null) allocatedCostThb += cost * lot.FxRate.Value;
                    lot.Quantity -= used;
                    lot.CostUsd -= cost;
                    remainingSale -= used;
                    if (lot.Quantity == 0m) lots.Dequeue();
                }
                var proceeds = entry.GrossAmount - entry.AssumedFee - entry.AssumedTax;
                realizedUsd += proceeds - allocatedCost;
                if (entry.FxRate is null || allocatedCostThb is null) realizedThb = null;
                else if (realizedThb is not null) realizedThb += proceeds * entry.FxRate.Value - allocatedCostThb.Value;
            }

            var quantity = lots.Sum(lot => lot.Quantity);
            var costUsd = lots.Sum(lot => lot.CostUsd);
            decimal? costThb = lots.All(lot => lot.FxRate is not null)
                ? lots.Sum(lot => lot.CostUsd * lot.FxRate!.Value)
                : null;
            results.Add(new(group.Key, quantity, costUsd, quantity == 0m ? null : costUsd / quantity,
                realizedUsd, investedCapitalUsd, costThb, realizedThb));
        }
        return results;
    }

    public static SimulationValuation Value(SimulationPosition position, decimal currentPrice, decimal? currentFxRate)
    {
        SimulationMarketEvidence.RequirePositive(currentPrice, 10, nameof(currentPrice));
        if (currentFxRate is not null) SimulationMarketEvidence.RequirePositive(currentFxRate.Value, 12, nameof(currentFxRate));
        var currentUsd = position.Quantity * currentPrice;
        var unrealizedUsd = currentUsd - position.RemainingCostUsd;
        var missing = new List<string>();
        if (currentFxRate is null) missing.Add("Current USD/THB FX is missing.");
        if (position.RemainingCostThb is null) missing.Add("One or more acquisition FX rates are missing.");
        decimal? currentThb = currentFxRate is null ? null : currentUsd * currentFxRate.Value;
        decimal? unrealizedThb = currentThb is null || position.RemainingCostThb is null ? null : currentThb - position.RemainingCostThb;
        decimal? stockEffect = null;
        decimal? fxEffect = null;
        if (position.RemainingCostThb is not null && position.RemainingCostUsd > 0m && currentFxRate is not null)
        {
            var weightedAcquisitionFx = position.RemainingCostThb.Value / position.RemainingCostUsd;
            stockEffect = (currentUsd - position.RemainingCostUsd) * weightedAcquisitionFx;
            fxEffect = currentUsd * (currentFxRate.Value - weightedAcquisitionFx);
        }
        var totalThb = unrealizedThb is null || position.RealizedThb is null ? null : unrealizedThb + position.RealizedThb;
        var totalPlUsd = unrealizedUsd + position.RealizedUsd;
        decimal? returnPercent = position.InvestedCapitalUsd == 0m ? null : totalPlUsd / position.InvestedCapitalUsd * 100m;
        return new(position, currentPrice, currentUsd, unrealizedUsd, unrealizedUsd + position.RealizedUsd,
            returnPercent, currentFxRate, currentThb, unrealizedThb, totalThb, stockEffect, fxEffect, missing.Count == 0, missing);
    }

    private sealed class Lot(decimal quantity, decimal costUsd, decimal? fxRate)
    {
        public decimal Quantity { get; set; } = quantity;
        public decimal CostUsd { get; set; } = costUsd;
        public decimal? FxRate { get; } = fxRate;
    }
}
