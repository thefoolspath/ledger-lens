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
