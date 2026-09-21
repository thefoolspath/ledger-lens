using LedgerLens.Application.Abstractions;
using LedgerLens.MarketData.Application.Features.ForeignExchangeRates.GetLatestList;
using LedgerLens.MarketData.Domain;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.MarketData.Api.Features.ForeignExchangeRates.GetLatestList;

public static class ForeignExchangeRatesGetLatestListEndpoint
{
    public static RouteHandlerBuilder MapForeignExchangeRatesGetLatestList(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/foreign-exchange-rates/get-latest-list", async (
            ForeignExchangeRatesGetLatestListRequest request,
            HttpContext httpContext,
            ApiProblemFactory problems,
            IQueryHandler<ForeignExchangeRatesGetLatestListQuery, IReadOnlyList<FxSnapshot>> handler,
            CancellationToken cancellationToken) =>
            await MarketDataEndpointExecution.ExecuteAsync(httpContext, problems,
                () => handler.HandleAsync(new(request.BaseCurrency, request.QuoteCurrency), cancellationToken)))
            .WithName("ForeignExchangeRatesGetLatestList");
}
