using LedgerLens.ApiContracts;
using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Application.Features.Portfolios.Create;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api.Features.Portfolios.Create;

public static class PortfoliosCreateEndpoint
{
    public static RouteHandlerBuilder MapPortfoliosCreate(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/portfolios/create", async (
            PortfoliosCreateRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            ICommandHandler<PortfoliosCreateCommand, PortfolioListItem> handler,
            CancellationToken cancellationToken) =>
        {
            try
            {
                var portfolio = await handler.HandleAsync(
                    new(request.Name, request.BaseCurrency, request.ReportingCurrency), cancellationToken);
                return ApiResults.Created(httpContext, $"/v1/portfolios/get-one/{portfolio.Id}", portfolio);
            }
            catch (ArgumentException)
            {
                return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest);
            }
        }).WithName("PortfoliosCreate").AddEndpointFilter<LocalMutationFilter>();
}
