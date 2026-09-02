using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record SimulationSeriesInput(DateOnly Date, decimal Price, decimal? FxRate);
