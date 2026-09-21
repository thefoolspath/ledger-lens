using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.Portfolios.GetList;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api.Features.Portfolios.GetList;

public static class PortfoliosGetListEndpoint
{
    public static RouteHandlerBuilder MapPortfoliosGetList(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/portfolios/get-list", async (
            HttpContext httpContext,
            IQueryHandler<PortfoliosGetListQuery, IReadOnlyList<PortfolioListItem>> handler,
            CancellationToken cancellationToken) =>
            ApiResults.Ok(httpContext, await handler.HandleAsync(new(), cancellationToken)))
        .WithName("PortfoliosGetList");
}
