namespace LedgerLens.PortfolioCore.Application.Features.SimulationValuations.CalculateSeriesList;

public sealed record SimulationValuationsCalculateSeriesListQuery(
    Guid AccountId,
    string Symbol,
    IReadOnlyList<SimulationSeriesInput> Observations);
