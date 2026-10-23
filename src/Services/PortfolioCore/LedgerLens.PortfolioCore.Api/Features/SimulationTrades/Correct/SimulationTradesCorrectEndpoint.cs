using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.SimulationTrades.Correct;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api.Features.SimulationTrades.Correct;

public static class SimulationTradesCorrectEndpoint
{
    public static RouteHandlerBuilder MapSimulationTradesCorrect(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/simulation-trades/correct/{tradeId:guid}", async (
            Guid tradeId,
            SimulationTradesCorrectRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            ICommandHandler<SimulationTradesCorrectCommand, IReadOnlyList<SimulationTradeView>?> handler,
            CancellationToken cancellationToken) =>
        {
            if (!PortfolioCoreEndpointValidation.IsVersion7(tradeId) ||
                !PortfolioCoreEndpointValidation.IsVersion7(request.ReplacementDraftId))
                return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier);
            try
            {
                var result = await handler.HandleAsync(
                    new(tradeId, request.ReplacementDraftId, request.Reason), cancellationToken);
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
        }).WithName("SimulationTradesCorrect").AddEndpointFilter<LocalMutationFilter>();
}
