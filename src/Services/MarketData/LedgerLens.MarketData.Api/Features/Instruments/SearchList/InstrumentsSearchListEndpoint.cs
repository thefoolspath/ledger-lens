using LedgerLens.Application.Abstractions;
using LedgerLens.MarketData.Application.Features.Instruments.SearchList;
using LedgerLens.MarketData.Domain;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.MarketData.Api.Features.Instruments.SearchList;

public static class InstrumentsSearchListEndpoint
{
    public static RouteHandlerBuilder MapInstrumentsSearchList(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/instruments/search-list", async (
            string query,
            string? market,
            HttpContext httpContext,
            ApiProblemFactory problems,
            IQueryHandler<InstrumentsSearchListQuery, IReadOnlyList<MarketInstrument>> handler,
            CancellationToken cancellationToken) =>
            await MarketDataEndpointExecution.ExecuteAsync(httpContext, problems,
                () => handler.HandleAsync(new(query, market ?? "US"), cancellationToken)))
            .WithName("InstrumentsSearchList");
}
