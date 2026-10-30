using LedgerLens.PortfolioCore.Application;

namespace LedgerLens.PortfolioCore.Api.Features.SimulationValuations.CalculateSeriesList;

public sealed record SimulationValuationsCalculateSeriesListRequest(Guid AccountId, string Symbol, IReadOnlyList<SimulationSeriesInput> Observations);
