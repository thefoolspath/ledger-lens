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
    public async Task Created_portfolio_location_is_rewritten_and_resolves_through_the_Gateway()
    {
        var resourceId = Guid.CreateVersion7();
        await using var service = await StartPortfolioLocationServiceAsync(
            $"/v1/portfolios/get-one/{resourceId}", resourceId);
        await using var factory = CreateGatewayFactory(service);
        using var client = factory.CreateClient();

        using var createResponse = await client.PostAsync("/api/portfolio/v1/portfolios/create", null);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.Equal($"/api/portfolio/v1/portfolios/get-one/{resourceId}",
            createResponse.Headers.Location?.OriginalString);
        using var getResponse = await client.GetAsync(createResponse.Headers.Location);
        getResponse.EnsureSuccessStatusCode();
        var retrieved = await getResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(resourceId, retrieved.GetProperty("id").GetGuid());
    }

    [Theory]
    [InlineData("https://malicious.example/v1/portfolios/get-one/1")]
    [InlineData("//malicious.example/v1/portfolios/get-one/1")]
    [InlineData("v1/portfolios/get-one/1")]
    [InlineData("/internal/portfolios/get-one/1")]
    [InlineData("/v1/../internal/secrets")]
    [InlineData("/v1/%2e%2e/internal/secrets")]
    [InlineData("/v1//portfolios/get-one/1")]
    [InlineData("/v1/portfolios/get-one/1?redirect=https://malicious.example")]
    public async Task Invalid_created_portfolio_location_is_removed_without_changing_the_committed_response(
        string upstreamLocation)
    {
        var resourceId = Guid.CreateVersion7();
        await using var service = await StartPortfolioLocationServiceAsync(upstreamLocation, resourceId);
        await using var factory = CreateGatewayFactory(service);
        using var client = factory.CreateClient();

        using var response = await client.PostAsync("/api/portfolio/v1/portfolios/create", null);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Null(response.Headers.Location);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(resourceId, body.GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task Created_response_without_location_keeps_its_envelope_and_status()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        var app = builder.Build();
        app.MapPost("/created", (HttpContext context) => ApiResults.Created(context, new { id = "created" }));
        await app.StartAsync();
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri(app.Urls.Single()) };
            using var response = await client.PostAsync("/created", null);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Null(response.Headers.Location);
            var body = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.Equal("created", body.GetProperty("data").GetProperty("id").GetString());
            Assert.True(body.GetProperty("meta").TryGetProperty("correlationId", out _));
        }
        finally
        {
            await app.DisposeAsync();
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
            problems.Validation(context, ApiErrorCodes.InvalidRequest, "resourceId"));
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
            Assert.Equal(ApiErrorCodes.InvalidRequest, problem.GetProperty("code").GetString());
            Assert.Equal("contract-correlation", problem.GetProperty("correlationId").GetString());
            Assert.Equal("คำขอไม่ถูกต้อง", problem.GetProperty("title").GetString());
            Assert.Equal("คำขอไม่ผ่านกฎการตรวจสอบของการดำเนินการนี้",
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

    private static WebApplicationFactory<Program> CreateGatewayFactory(WebApplication portfolioService) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(webHost =>
            webHost.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["services:portfolio-api:http:0"] = portfolioService.Urls.Single(),
                })));

    private static async Task<WebApplication> StartPortfolioLocationServiceAsync(
        string location,
        Guid resourceId)
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        var app = builder.Build();
        app.MapPost("/v1/portfolios/create", (HttpContext context) =>
        {
            context.Response.Headers.Location = location;
            return Results.Json(new { id = resourceId }, statusCode: StatusCodes.Status201Created);
        });
        app.MapGet($"/v1/portfolios/get-one/{resourceId}", () => new { id = resourceId });
        await app.StartAsync();
        return app;
    }
}
