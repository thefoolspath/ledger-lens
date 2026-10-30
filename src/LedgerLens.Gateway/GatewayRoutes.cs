using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;

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

    public static string PortfolioPrefix => DefinitionFor("portfolio").Prefix;

    public static string PortfolioRouteId => RouteIdFor(DefinitionFor("portfolio"));

    public static IReadOnlyList<RouteConfig> Routes { get; } = Definitions
        .Select(definition => new RouteConfig
        {
            RouteId = RouteIdFor(definition),
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

    private static (string Prefix, string Cluster, string Service) DefinitionFor(string cluster) =>
        Definitions.Single(definition => string.Equals(definition.Cluster, cluster, StringComparison.Ordinal));

    private static string RouteIdFor((string Prefix, string Cluster, string Service) definition) =>
        $"{definition.Cluster}-route";
}
