using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LedgerLens.ApiContracts;
using LedgerLens.ServiceDefaults;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

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

    [Fact]
    public async Task Shared_API_contract_wraps_success_and_localizes_Problem_Details()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.AddLedgerLensApiResponses();
        var app = builder.Build();
        app.UseLedgerLensApiResponses();
        app.Use(async (context, next) =>
        {
            context.TraceIdentifier = "contract-correlation";
            context.Response.Headers["X-Correlation-ID"] = context.TraceIdentifier;
            await next(context);
        });
        app.MapGet("/success", (HttpContext context) => ApiResults.Ok(context, Array.Empty<object>()));
        app.MapGet("/invalid", (HttpContext context, ApiProblemFactory problems) =>
            problems.Validation(context, ApiErrorCodes.InvalidIdentifier, "resourceId"));
        await app.StartAsync();

        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
            var success = await client.GetFromJsonAsync<ApiResponse<object[]>>("/success");
            Assert.NotNull(success);
            Assert.Empty(success.Data);
            Assert.Equal("contract-correlation", success.Meta.CorrelationId);
            Assert.Null(success.Meta.Pagination);

            using var request = new HttpRequestMessage(HttpMethod.Get, "/invalid");
            request.Headers.AcceptLanguage.ParseAdd("th-TH");
            using var response = await client.SendAsync(request);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal(ApiErrorCodes.InvalidIdentifier, problem.GetProperty("code").GetString());
            Assert.Equal("contract-correlation", problem.GetProperty("correlationId").GetString());
            Assert.Equal("รหัสไม่ถูกต้อง", problem.GetProperty("title").GetString());
            Assert.Equal("รหัสทรัพยากร LedgerLens ต้องเป็น UUIDv7",
                problem.GetProperty("errors").GetProperty("resourceId")[0].GetString());
        }
        finally
        {
            await app.DisposeAsync();
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
