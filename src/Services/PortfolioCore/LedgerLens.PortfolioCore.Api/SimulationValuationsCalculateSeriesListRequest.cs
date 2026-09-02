using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Api;

public sealed record SimulationValuationsCalculateSeriesListRequest(Guid AccountId, string Symbol, IReadOnlyList<SimulationSeriesInput> Observations);
