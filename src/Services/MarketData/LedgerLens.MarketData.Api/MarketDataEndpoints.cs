using LedgerLens.ApiContracts;
using LedgerLens.MarketData.Application;
using LedgerLens.MarketData.Domain;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.MarketData.Api;

public static class MarketDataEndpoints
{
    public static IEndpointRouteBuilder MapMarketDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/v1");
        group.MapGet("/instruments/search-list", async (string query, string? market, HttpContext httpContext,
            ApiProblemFactory problems, InstrumentsSearchListHandler handler, CancellationToken cancellationToken) =>
            await ExecuteAsync(httpContext, problems, () => handler.HandleAsync(query, market ?? "US", cancellationToken)))
            .WithName("InstrumentsSearchList");
        group.MapPost("/quotes/get-latest-list", async (QuotesGetLatestListRequest request, HttpContext httpContext,
            ApiProblemFactory problems, QuotesGetLatestListHandler handler, CancellationToken cancellationToken) =>
            await ExecuteAsync(httpContext, problems, () => handler.HandleAsync(request.Instruments, cancellationToken)))
            .WithName("QuotesGetLatestList");
        group.MapGet("/instrument-candles/get-one/{instrumentId:guid}", async (Guid instrumentId, string? interval, DateOnly from, DateOnly to,
            HttpContext httpContext, ApiProblemFactory problems, InstrumentCandlesGetOneHandler handler, CancellationToken cancellationToken) =>
        {
            var result = await ExecuteAsync(httpContext, problems,
                () => handler.HandleAsync(instrumentId, interval ?? "1day", from, to, cancellationToken));
            return result;
        }).WithName("InstrumentCandlesGetOne");
        group.MapPost("/foreign-exchange-rates/get-latest-list", async (ForeignExchangeRatesGetLatestListRequest request,
            HttpContext httpContext, ApiProblemFactory problems, ForeignExchangeRatesGetLatestListHandler handler,
            CancellationToken cancellationToken) =>
            await ExecuteAsync(httpContext, problems,
                () => handler.HandleAsync(request.BaseCurrency, request.QuoteCurrency, cancellationToken)))
            .WithName("ForeignExchangeRatesGetLatestList");
        return endpoints;
    }

    private static async Task<IResult> ExecuteAsync<T>(
        HttpContext httpContext,
        ApiProblemFactory problems,
        Func<Task<T>> action)
    {
        try
        {
            var value = await action();
            return value is null ? problems.NotFound(httpContext) : ApiResults.Ok(httpContext, value);
        }
        catch (ArgumentException)
        {
            return problems.Validation(httpContext, ApiErrorCodes.InvalidRequest);
        }
        catch (MarketDataUnavailableException)
        {
            return problems.ServiceUnavailable(httpContext, ApiErrorCodes.MarketDataUnavailable);
        }
    }
}
