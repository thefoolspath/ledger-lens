using LedgerLens.ApiContracts;
using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Domain;
using LedgerLens.ServiceDefaults;

namespace LedgerLens.PortfolioCore.Api;

public sealed class LocalMutationFilter(ApiProblemFactory problems) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.HttpContext.Request;
        if (!request.HasJsonContentType())
        {
            return problems.UnsupportedMediaType(context.HttpContext);
        }

        if (!request.Headers.TryGetValue("X-LedgerLens-Request", out var marker) || marker != "1")
        {
            return problems.BadRequest(context.HttpContext, ApiErrorCodes.MutationMarkerMissing);
        }

        var source = request.Headers.Origin.FirstOrDefault() ?? request.Headers.Referer.FirstOrDefault();
        if (!Uri.TryCreate(source, UriKind.Absolute, out var sourceUri) || !sourceUri.IsLoopback)
        {
            return problems.Forbidden(context.HttpContext, ApiErrorCodes.LoopbackOriginRequired);
        }

        return await next(context);
    }
}
