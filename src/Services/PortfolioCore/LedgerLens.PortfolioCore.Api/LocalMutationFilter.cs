using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Api;

public sealed class LocalMutationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.HttpContext.Request;
        if (!request.HasJsonContentType())
        {
            return Results.StatusCode(StatusCodes.Status415UnsupportedMediaType);
        }

        if (!request.Headers.TryGetValue("X-LedgerLens-Request", out var marker) || marker != "1")
        {
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "Missing local mutation marker.");
        }

        var source = request.Headers.Origin.FirstOrDefault() ?? request.Headers.Referer.FirstOrDefault();
        if (!Uri.TryCreate(source, UriKind.Absolute, out var sourceUri) || !sourceUri.IsLoopback)
        {
            return Results.Problem(statusCode: StatusCodes.Status403Forbidden, detail: "Mutation origin must be loopback.");
        }

        return await next(context);
    }
}
