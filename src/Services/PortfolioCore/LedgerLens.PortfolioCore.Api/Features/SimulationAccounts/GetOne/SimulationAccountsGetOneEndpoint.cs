using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.SimulationAccounts.GetOne;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api.Features.SimulationAccounts.GetOne;

public static class SimulationAccountsGetOneEndpoint
{
    public static RouteHandlerBuilder MapSimulationAccountsGetOne(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/simulation-accounts/get-one/{accountId:guid}", async (
            Guid accountId,
            HttpContext httpContext,
            ApiProblemFactory problems,
            IQueryHandler<SimulationAccountsGetOneQuery, SimulationAccountOverview?> handler,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new(accountId), cancellationToken);
            return result is null ? problems.NotFound(httpContext) : ApiResults.Ok(httpContext, result);
        }).WithName("SimulationAccountsGetOne");
}
