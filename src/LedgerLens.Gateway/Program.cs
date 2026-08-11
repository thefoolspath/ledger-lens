using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddReverseProxy()
    .LoadFromMemory(GatewayRoutes.Routes, GatewayRoutes.Clusters)
    .AddServiceDiscoveryDestinationResolver();

var app = builder.Build();
app.UseExceptionHandler();
app.MapDefaultEndpoints("gateway");
app.MapReverseProxy();
app.Run();

public static class GatewayRoutes
{
    private static readonly (string Prefix, string Cluster, string Service)[] Definitions =
    [
        ("/api/portfolio", "portfolio", "portfolio-api"),
        ("/api/market", "market", "market-api"),
        ("/api/slips", "slips", "slip-api"),
        ("/api/research", "research", "research-api"),
        ("/api/operations", "operations", "operations-api"),
    ];

    public static IReadOnlyList<RouteConfig> Routes { get; } = Definitions
        .Select(definition => new RouteConfig
        {
            RouteId = $"{definition.Cluster}-route",
            ClusterId = definition.Cluster,
            Match = new RouteMatch { Path = $"{definition.Prefix}/{{**catch-all}}" },
        }.WithTransformPathRemovePrefix(definition.Prefix))
        .ToArray();

    public static IReadOnlyList<ClusterConfig> Clusters { get; } = Definitions
        .Select(definition => new ClusterConfig
        {
            ClusterId = definition.Cluster,
            Destinations = new Dictionary<string, DestinationConfig>
            {
                [definition.Service] = new() { Address = $"https+http://{definition.Service}" },
            },
        })
        .ToArray();
}

public partial class Program;
