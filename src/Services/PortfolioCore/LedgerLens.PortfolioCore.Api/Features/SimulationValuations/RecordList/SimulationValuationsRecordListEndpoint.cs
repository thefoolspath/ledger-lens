using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.SimulationValuations.RecordList;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api.Features.SimulationValuations.RecordList;

public static class SimulationValuationsRecordListEndpoint
{
    public static RouteHandlerBuilder MapSimulationValuationsRecordList(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/simulation-valuations/record-list", async (
            SimulationValuationsRecordListRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            ICommandHandler<SimulationValuationsRecordListCommand, IReadOnlyList<SimulationValuationView>?> handler,
            CancellationToken cancellationToken) =>
        {
            if (!PortfolioCoreEndpointValidation.IsVersion7(request.AccountId))
                return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "accountId");
            try
            {
                var result = await handler.HandleAsync(
                    new(request.AccountId, request.Quotes, request.CurrentFxRate), cancellationToken);
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
        }).WithName("SimulationValuationsRecordList").AddEndpointFilter<LocalMutationFilter>();
}
