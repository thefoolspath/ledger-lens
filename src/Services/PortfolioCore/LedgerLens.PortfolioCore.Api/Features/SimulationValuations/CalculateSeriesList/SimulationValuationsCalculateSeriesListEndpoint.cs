using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.SimulationValuations.CalculateSeriesList;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api.Features.SimulationValuations.CalculateSeriesList;

public static class SimulationValuationsCalculateSeriesListEndpoint
{
    public static RouteHandlerBuilder MapSimulationValuationsCalculateSeriesList(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/simulation-valuations/calculate-series-list", async (
            SimulationValuationsCalculateSeriesListRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            IQueryHandler<SimulationValuationsCalculateSeriesListQuery, IReadOnlyList<SimulationSeriesPoint>?> handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.HandleAsync(
                    new(request.AccountId, request.Symbol, request.Observations), cancellationToken);
                return result is null ? problems.NotFound(httpContext) : ApiResults.Ok(httpContext, result);
            }
            catch (ArgumentException)
            {
                return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest);
            }
            catch (InvalidOperationException)
            {
                return problems.Conflict(httpContext);
            }
        }).WithName("SimulationValuationsCalculateSeriesList").AddEndpointFilter<LocalMutationFilter>();
}
