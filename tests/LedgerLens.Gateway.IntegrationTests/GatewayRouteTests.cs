using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace LedgerLens.Gateway.IntegrationTests;

public sealed class GatewayRouteTests
{
    [Fact]
    public void Browser_routes_target_only_logical_service_names()
    {
        var expected = new Dictionary<string, string>
        {
            ["/api/portfolio/{**catch-all}"] = "https+http://portfolio-api",
            ["/api/market/{**catch-all}"] = "https+http://market-api",
            ["/api/slips/{**catch-all}"] = "https+http://slip-api",
            ["/api/research/{**catch-all}"] = "https+http://research-api",
            ["/api/operations/{**catch-all}"] = "https+http://operations-api",
        };

        Assert.Equal(expected.Count, GatewayRoutes.Routes.Count);
        foreach (var route in GatewayRoutes.Routes)
        {
            var destination = Assert.Single(GatewayRoutes.Clusters
                .Single(cluster => cluster.ClusterId == route.ClusterId)
                .Destinations!);
            Assert.Equal(expected[route.Match.Path!], destination.Value.Address);
        }
    }

    [Fact]
    public async Task Gateway_resolves_every_service_on_dynamic_ports()
    {
        var services = new Dictionary<string, WebApplication>();
        try
        {
            foreach (var serviceName in new[] { "portfolio-api", "market-api", "slip-api", "research-api", "operations-api" })
            {
                services[serviceName] = await StartPingServiceAsync(serviceName);
            }

            await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(webHost =>
                webHost.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                    services.ToDictionary(
                        pair => $"services:{pair.Key}:http:0",
                        pair => pair.Value.Urls.Single())!)));
            using var client = factory.CreateClient();

            var paths = new[]
            {
                "/api/portfolio/internal/ping",
                "/api/market/internal/ping",
                "/api/slips/internal/ping",
                "/api/research/internal/ping",
                "/api/operations/internal/ping",
            };
            foreach (var path in paths)
            {
                using var response = await client.GetAsync(path);
                response.EnsureSuccessStatusCode();
            }
        }
        finally
        {
            foreach (var service in services.Values)
            {
                await service.DisposeAsync();
            }
        }
    }

    private static async Task<WebApplication> StartPingServiceAsync(string serviceName)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        var app = builder.Build();
        app.MapGet("/internal/ping", () => new { service = serviceName });
        await app.StartAsync();
        return app;
    }
}
