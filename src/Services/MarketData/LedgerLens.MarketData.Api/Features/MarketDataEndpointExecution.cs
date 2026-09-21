using LedgerLens.ApiContracts;
using LedgerLens.MarketData.Application;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.MarketData.Api.Features;

internal static class MarketDataEndpointExecution
{
    internal static async Task<IResult> ExecuteAsync<T>(
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
