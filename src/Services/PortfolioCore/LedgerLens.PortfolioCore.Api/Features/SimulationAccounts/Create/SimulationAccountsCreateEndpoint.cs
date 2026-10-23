using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.SimulationAccounts.Create;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api.Features.SimulationAccounts.Create;

public static class SimulationAccountsCreateEndpoint
{
    public static RouteHandlerBuilder MapSimulationAccountsCreate(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/simulation-accounts/create", async (
            SimulationAccountsCreateRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            ICommandHandler<SimulationAccountsCreateCommand, SimulationAccountListItem?> handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var account = await handler.HandleAsync(new(request.PortfolioId, request.Name), cancellationToken);
                return account is null
                    ? problems.NotFound(httpContext)
                    : ApiResults.Created(httpContext, $"/v1/simulation-accounts/get-one/{account.Id}", account);
            }
            catch (ArgumentException)
            {
                return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest);
            }
        }).WithName("SimulationAccountsCreate").AddEndpointFilter<LocalMutationFilter>();
}
