using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.SimulationTradeDrafts.Create;
using LedgerLens.PortfolioCore.Domain;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api.Features.SimulationTradeDrafts.Create;

public static class SimulationTradeDraftsCreateEndpoint
{
    public static RouteHandlerBuilder MapSimulationTradeDraftsCreate(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/simulation-trade-drafts/create", async (
            SimulationTradeDraftsCreateRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            ICommandHandler<SimulationTradeDraftsCreateCommand, SimulationDraftView?> handler,
            CancellationToken cancellationToken) =>
        {
            if (!PortfolioCoreEndpointValidation.IsVersion7(request.AccountId))
                return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "accountId");
            if (!Enum.TryParse<SimulationTradeSide>(request.Side, true, out var side) ||
                !Enum.TryParse<SimulationInputMode>(request.InputMode, true, out var inputMode))
                return problems.Validation(httpContext, ApiErrorCodes.InvalidSimulationTradeInput);
            try
            {
                var result = await handler.HandleAsync(new(request.AccountId, side, inputMode, request.RequestedQuantity,
                    request.RequestedAmount, request.AssumedFee, request.AssumedTax, request.FxRate,
                    request.EffectiveAt, request.Quote), cancellationToken);
                return result is null
                    ? problems.NotFound(httpContext)
                    : ApiResults.Created(httpContext, $"/v1/simulation-trade-drafts/{result.Id}", result);
            }
            catch (ArgumentException)
            {
                return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest);
            }
            catch (InvalidOperationException)
            {
                return problems.Conflict(httpContext);
            }
        }).WithName("SimulationTradeDraftsCreate").AddEndpointFilter<LocalMutationFilter>();
}
