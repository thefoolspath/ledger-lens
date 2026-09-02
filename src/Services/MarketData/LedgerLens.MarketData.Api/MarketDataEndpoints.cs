using LedgerLens.MarketData.Application;
using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Api;

public static class MarketDataEndpoints
{
    public static IEndpointRouteBuilder MapMarketDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/v1");
        group.MapGet("/instruments/search-list", async (string query, string? market, InstrumentsSearchListHandler handler, CancellationToken cancellationToken) =>
            await ExecuteAsync(() => handler.HandleAsync(query, market ?? "US", cancellationToken)))
            .WithName("InstrumentsSearchList");
        group.MapPost("/quotes/get-latest-list", async (QuotesGetLatestListRequest request, QuotesGetLatestListHandler handler, CancellationToken cancellationToken) =>
            await ExecuteAsync(() => handler.HandleAsync(request.Instruments, cancellationToken)))
            .WithName("QuotesGetLatestList");
        group.MapGet("/instrument-candles/get-one/{instrumentId:guid}", async (Guid instrumentId, string? interval, DateOnly from, DateOnly to,
            InstrumentCandlesGetOneHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await ExecuteAsync(() => handler.HandleAsync(instrumentId, interval ?? "1day", from, to, cancellationToken));
            return result;
        }).WithName("InstrumentCandlesGetOne");
        group.MapPost("/foreign-exchange-rates/get-latest-list", async (ForeignExchangeRatesGetLatestListRequest request, ForeignExchangeRatesGetLatestListHandler handler, CancellationToken cancellationToken) =>
            await ExecuteAsync(() => handler.HandleAsync(request.BaseCurrency, request.QuoteCurrency, cancellationToken)))
            .WithName("ForeignExchangeRatesGetLatestList");
        return endpoints;
    }

    private static async Task<IResult> ExecuteAsync<T>(Func<Task<T>> action)
    {
        try
        {
            var value = await action();
            return value is null ? Results.NotFound() : Results.Ok(value);
        }
        catch (ArgumentException exception)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]> { ["request"] = [exception.Message] });
        }
        catch (MarketDataUnavailableException exception)
        {
            return Results.Problem(statusCode: StatusCodes.Status503ServiceUnavailable, title: "Market data unavailable", detail: exception.Message);
        }
    }
}
