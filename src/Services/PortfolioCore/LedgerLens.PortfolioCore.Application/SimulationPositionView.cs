using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record SimulationPositionView(string Symbol, decimal Quantity, decimal RemainingCostUsd,
    decimal? AverageCostUsd, decimal RealizedUsd, decimal InvestedCapitalUsd, decimal? RemainingCostThb, decimal? RealizedThb);
