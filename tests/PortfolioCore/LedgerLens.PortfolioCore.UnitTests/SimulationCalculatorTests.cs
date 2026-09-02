using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.UnitTests;

public sealed class SimulationCalculatorTests
{
    [Fact]
    public void Amount_mode_rounds_quantity_down_and_preserves_remainder()
    {
        var draft = SimulationTradeDraft.Create(Id(1), Id(2), SimulationTradeSide.Buy, SimulationInputMode.ByAmount,
            null, 100m, 1m, 0.5m, 32m, Instant(1), Instant(1), Evidence(30m));

        Assert.Equal(3.333333333333m, draft.Quantity);
        Assert.Equal(99.9999999999m, draft.GrossAmount);
        Assert.Equal(0.0000000001m, draft.UnusedAmount);
        Assert.Equal(-101.4999999999m, draft.NetCash);
    }

    [Fact]
    public void Multiple_buys_and_partial_sell_use_fifo_and_reconcile_usd_and_thb()
    {
        var entries = new[]
        {
            Trade(1, SimulationTradeSide.Buy, 2m, 100m, 2m, 0m, 30m, 1),
            Trade(2, SimulationTradeSide.Buy, 1m, 120m, 0m, 0m, 31m, 2),
            Trade(3, SimulationTradeSide.Sell, 1.5m, 150m, 1m, 0m, 32m, 3),
        };

        var position = Assert.Single(SimulationCalculator.Rebuild(entries));
        var valuation = SimulationCalculator.Value(position, 160m, 33m);

        Assert.Equal(1.5m, position.Quantity);
        Assert.Equal(170.5m, position.RemainingCostUsd);
        Assert.Equal(113.66666666666666666666666667m, position.AverageCostUsd);
        Assert.Equal(72.5m, position.RealizedUsd);
        Assert.Equal(322m, position.InvestedCapitalUsd);
        Assert.Equal(240m, valuation.CurrentValueUsd);
        Assert.Equal(69.5m, valuation.UnrealizedUsd);
        Assert.Equal(142m, valuation.TotalPlUsd);
        Assert.Equal(valuation.TotalPlUsd / position.InvestedCapitalUsd * 100m, valuation.ReturnPercent);
        Assert.True(valuation.IsComplete);
        Assert.Equal(valuation.TotalPlThb, valuation.UnrealizedThb + position.RealizedThb);
    }

    [Fact]
    public void Oversell_is_rejected_and_missing_fx_is_incomplete()
    {
        var oversell = new[]
        {
            Trade(1, SimulationTradeSide.Buy, 1m, 100m, 0m, 0m, null, 1),
            Trade(2, SimulationTradeSide.Sell, 2m, 110m, 0m, 0m, null, 2),
        };
        Assert.Throws<InvalidOperationException>(() => SimulationCalculator.Rebuild(oversell));

        var position = Assert.Single(SimulationCalculator.Rebuild([oversell[0]]));
        var valuation = SimulationCalculator.Value(position, 120m, null);
        Assert.False(valuation.IsComplete);
        Assert.Null(valuation.TotalPlThb);
        Assert.NotEmpty(valuation.MissingReasons);
    }

    [Fact]
    public void Reversal_and_replacement_rebuild_only_the_corrected_history()
    {
        var original = Trade(1, SimulationTradeSide.Buy, 1m, 100m, 0m, 0m, 30m, 1);
        var replacementDraft = SimulationTradeDraft.Create(Id(2), Id(100), SimulationTradeSide.Buy,
            SimulationInputMode.ByQuantity, 2m, null, 1m, 0m, 31m, Instant(1), Instant(2), Evidence(110m));
        var reversal = SimulationTradeEntry.Reversal(Id(3), original, "Synthetic correction", Instant(2));
        var replacement = SimulationTradeEntry.Replacement(Id(4), replacementDraft, original.Id,
            "Synthetic correction", Instant(2));

        var position = Assert.Single(SimulationCalculator.Rebuild([original, reversal, replacement]));

        Assert.Equal(2m, position.Quantity);
        Assert.Equal(221m, position.RemainingCostUsd);
        Assert.Equal(221m, position.InvestedCapitalUsd);
    }

    private static SimulationTradeEntry Trade(int id, SimulationTradeSide side, decimal quantity, decimal price,
        decimal fee, decimal tax, decimal? fx, int day) => new(Id(id), Id(100), side, quantity, price,
        decimal.Floor(quantity * price * 10_000_000_000m) / 10_000_000_000m, fee, tax, fx, Instant(day), Instant(day), Evidence(price));

    private static SimulationMarketEvidence Evidence(decimal price) => new(Id(200), "NVDA", "Nasdaq", "XNAS", "USD",
        price, "LastTrade", "Synthetic", "Fixture", Instant(1), Instant(1), "Synthetic", 0, "request", "synthetic-test-only");

    private static Guid Id(int suffix) => Guid.Parse($"019a0f20-0000-7000-8000-{suffix:000000000000}");
    private static DateTimeOffset Instant(int day) => new(2026, 8, day, 12, 0, 0, TimeSpan.Zero);
}
