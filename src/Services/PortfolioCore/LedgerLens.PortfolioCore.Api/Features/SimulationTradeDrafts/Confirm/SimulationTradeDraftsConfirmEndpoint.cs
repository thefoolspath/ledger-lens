using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.SimulationTradeDrafts.Confirm;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api.Features.SimulationTradeDrafts.Confirm;

public static class SimulationTradeDraftsConfirmEndpoint
{
    public static RouteHandlerBuilder MapSimulationTradeDraftsConfirm(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/simulation-trade-drafts/confirm/{draftId:guid}", async (
            Guid draftId,
            SimulationTradeDraftsConfirmRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            ICommandHandler<SimulationTradeDraftsConfirmCommand, SimulationTradeView?> handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var result = await handler.HandleAsync(new(request.AccountId, draftId), cancellationToken);
                return result is null ? problems.NotFound(httpContext) : ApiResults.Ok(httpContext, result);
            }
            catch (InvalidOperationException)
            {
                return problems.Conflict(httpContext);
            }
        }).WithName("SimulationTradeDraftsConfirm").AddEndpointFilter<LocalMutationFilter>();
}
