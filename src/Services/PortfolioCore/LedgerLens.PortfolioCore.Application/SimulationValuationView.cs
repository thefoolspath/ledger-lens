using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record SimulationValuationView(string Symbol, decimal Quantity, decimal RemainingCostUsd, decimal? AverageCostUsd,
    decimal RealizedUsd, decimal InvestedCapitalUsd, decimal? CurrentPrice, decimal? CurrentValueUsd, decimal? UnrealizedUsd,
    decimal? TotalPlUsd, decimal? ReturnPercent,
    decimal? CurrentFxRate, decimal? CurrentValueThb, decimal? UnrealizedThb, decimal? TotalPlThb,
    decimal? StockEffectThb, decimal? FxEffectThb, bool IsComplete, IReadOnlyList<string> MissingReasons,
    SimulationMarketEvidence? Quote);
