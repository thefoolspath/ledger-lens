using LedgerLens.Application.Abstractions;
using LedgerLens.MarketData.Application.Features.Quotes.GetLatestList;
using LedgerLens.MarketData.Domain;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.MarketData.Api.Features.Quotes.GetLatestList;

public static class QuotesGetLatestListEndpoint
{
    public static RouteHandlerBuilder MapQuotesGetLatestList(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/quotes/get-latest-list", async (
            QuotesGetLatestListRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            IQueryHandler<QuotesGetLatestListQuery, IReadOnlyList<QuoteSnapshot>> handler,
            CancellationToken cancellationToken) =>
            await MarketDataEndpointExecution.ExecuteAsync(httpContext, problems,
                () => handler.HandleAsync(new(request.Instruments), cancellationToken)))
            .WithName("QuotesGetLatestList");
}
