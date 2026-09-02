using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record SimulationSeriesPoint(DateOnly Date, string Symbol, decimal Quantity, decimal CostBasisUsd,
    decimal MarketValueUsd, decimal UnrealizedUsd, decimal? MarketValueThb, bool IsComplete);
