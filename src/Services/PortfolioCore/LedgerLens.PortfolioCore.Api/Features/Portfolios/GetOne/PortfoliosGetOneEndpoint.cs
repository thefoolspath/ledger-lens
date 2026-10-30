using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.Portfolios.GetOne;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api.Features.Portfolios.GetOne;

public static class PortfoliosGetOneEndpoint
{
    public static RouteHandlerBuilder MapPortfoliosGetOne(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/portfolios/get-one/{portfolioId:guid}", async (
            Guid portfolioId,
            HttpContext httpContext,
            ApiProblemFactory problems,
            IQueryHandler<PortfoliosGetOneQuery, PortfolioOverview?> handler,
            CancellationToken cancellationToken) =>
        {
            if (!PortfolioCoreEndpointValidation.IsVersion7(portfolioId))
                return problems.Validation(httpContext, ApiErrorCodes.InvalidIdentifier, "portfolioId");
            var portfolio = await handler.HandleAsync(new(portfolioId), cancellationToken);
            return portfolio is null ? problems.NotFound(httpContext) : ApiResults.Ok(httpContext, portfolio);
        }).WithName("PortfoliosGetOne");
}
