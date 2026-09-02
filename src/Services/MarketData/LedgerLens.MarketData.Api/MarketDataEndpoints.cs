using LedgerLens.MarketData.Application;
using LedgerLens.MarketData.Domain;

namespace LedgerLens.MarketData.Api;

public static class MarketDataEndpoints
{
    public static IEndpointRouteBuilder MapMarketDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/v1");
        group.MapGet("/instruments/search", async (string query, string? market, SearchInstrumentsHandler handler, CancellationToken cancellationToken) =>
            await ExecuteAsync(() => handler.HandleAsync(query, market ?? "US", cancellationToken)));
        group.MapPost("/quotes/latest", async (LatestQuotesRequest request, GetLatestQuotesHandler handler, CancellationToken cancellationToken) =>
            await ExecuteAsync(() => handler.HandleAsync(request.Instruments, cancellationToken)));
        group.MapGet("/instruments/{instrumentId:guid}/candles", async (Guid instrumentId, string? interval, DateOnly from, DateOnly to,
            GetCandlesHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await ExecuteAsync(() => handler.HandleAsync(instrumentId, interval ?? "1day", from, to, cancellationToken));
            return result;
        });
        group.MapPost("/fx/latest", async (FxRequest request, GetFxHandler handler, CancellationToken cancellationToken) =>
            await ExecuteAsync(() => handler.HandleAsync(request.BaseCurrency, request.QuoteCurrency, cancellationToken)));
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

public sealed record LatestQuotesRequest(IReadOnlyList<MarketInstrument> Instruments);
public sealed record FxRequest(string BaseCurrency, string QuoteCurrency);
