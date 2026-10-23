using System.Net;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Yarp.ReverseProxy.Transforms;

public sealed class PortfolioLocationResponseTransform(string publicPrefix) : ResponseTransform
{
    public override ValueTask ApplyAsync(ResponseTransformContext context)
    {
        if (context.ProxyResponse?.StatusCode != HttpStatusCode.Created)
        {
            return ValueTask.CompletedTask;
        }

        var locations = TakeHeader(context, HeaderNames.Location);
        if (StringValues.IsNullOrEmpty(locations))
        {
            return ValueTask.CompletedTask;
        }

        if (locations.Count == 1 && TryCreatePublicLocation(locations[0], out var publicLocation))
        {
            SetHeader(context, HeaderNames.Location, publicLocation);
            return ValueTask.CompletedTask;
        }

        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<PortfolioLocationResponseTransform>>();
        logger.LogWarning(
            "Removed invalid upstream Location header from a created Portfolio response. RouteId: {RouteId}",
            GatewayRoutes.PortfolioRouteId);
        return ValueTask.CompletedTask;
    }

    private bool TryCreatePublicLocation(string? location, out string publicLocation)
    {
        publicLocation = string.Empty;
        if (string.IsNullOrWhiteSpace(location) ||
            !location.StartsWith("/v1/", StringComparison.Ordinal) ||
            location.Contains('\\') ||
            location.Contains('%') ||
            location.Contains('?') ||
            location.Contains('#') ||
            location.Any(char.IsControl) ||
            location.Any(char.IsWhiteSpace) ||
            !Uri.TryCreate(location, UriKind.Relative, out _))
        {
            return false;
        }

        var segments = location.Split('/');
        if (segments.Length < 4 || segments.Skip(1).Any(segment => segment.Length == 0 || segment is "." or ".."))
        {
            return false;
        }

        publicLocation = $"{publicPrefix}{location}";
        return true;
    }
}
