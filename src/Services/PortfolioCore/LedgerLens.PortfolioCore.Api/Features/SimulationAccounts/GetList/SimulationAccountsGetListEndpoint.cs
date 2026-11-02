using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.SimulationAccounts.GetList;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api.Features.SimulationAccounts.GetList;

public static class SimulationAccountsGetListEndpoint
{
    public static RouteHandlerBuilder MapSimulationAccountsGetList(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/simulation-accounts/get-list", async (
            Guid portfolioId,
            HttpContext httpContext,
            ApiProblemFactory problems,
            IQueryHandler<SimulationAccountsGetListQuery, IReadOnlyList<SimulationAccountListItem>> handler,
            CancellationToken cancellationToken) =>
        {
            return ApiResults.Ok(httpContext, await handler.HandleAsync(new(portfolioId), cancellationToken));
        }).WithName("SimulationAccountsGetList");
}
