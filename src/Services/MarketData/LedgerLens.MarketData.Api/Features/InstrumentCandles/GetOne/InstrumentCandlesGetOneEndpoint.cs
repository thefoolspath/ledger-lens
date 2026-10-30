using LedgerLens.Application.Abstractions;
using LedgerLens.MarketData.Application.Features.InstrumentCandles.GetOne;
using LedgerLens.MarketData.Domain;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.MarketData.Api.Features.InstrumentCandles.GetOne;

public static class InstrumentCandlesGetOneEndpoint
{
    public static RouteHandlerBuilder MapInstrumentCandlesGetOne(this IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/instrument-candles/get-one/{instrumentId:guid}", async (
            Guid instrumentId,
            string? interval,
            DateOnly from,
            DateOnly to,
            HttpContext httpContext,
            ApiProblemFactory problems,
            IQueryHandler<InstrumentCandlesGetOneQuery, CandleSeries?> handler,
            CancellationToken cancellationToken) =>
            await MarketDataEndpointExecution.ExecuteAsync(httpContext, problems,
                () => handler.HandleAsync(new(instrumentId, interval ?? "1day", from, to), cancellationToken)))
            .WithName("InstrumentCandlesGetOne");
}
