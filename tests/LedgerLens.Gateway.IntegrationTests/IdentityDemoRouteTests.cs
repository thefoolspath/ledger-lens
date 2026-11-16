using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace LedgerLens.Gateway.IntegrationTests;

public sealed class IdentityDemoRouteTests
{
    [Fact]
    public void Gateway_exposes_only_the_named_identity_demo_prefix()
    {
        var route = Assert.Single(GatewayRoutes.Routes,
            candidate => candidate.RouteId == "identity-demo-route");
        Assert.Equal("/api/identity-demo/{**catch-all}", route.Match.Path);
        Assert.DoesNotContain(GatewayRoutes.Routes,
            candidate => candidate.Match.Path == "/api/identity/{**catch-all}");
    }

    [Fact]
    public async Task Gateway_preserves_identity_demo_unexpected_problem()
    {
        var builder = WebApplication.CreateSlimBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        await using var service = builder.Build();
        service.MapPost("/v1/users/create", (HttpContext context) =>
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].ToString();
            context.Response.Headers["X-Correlation-ID"] = correlationId;
            return Results.Json(new
            {
                status = 500,
                code = "common.internal.unexpected",
                correlationId,
            }, statusCode: 500, contentType: "application/problem+json");
        });
        await service.StartAsync();

        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(webHost =>
            webHost.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["services:identity-demo-api:http:0"] = service.Urls.Single(),
                })));
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/identity-demo/v1/users/create");
        request.Headers.Add("X-Correlation-ID", "demo-correlation");
        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("common.internal.unexpected", body.RootElement.GetProperty("code").GetString());
        Assert.Equal("demo-correlation", body.RootElement.GetProperty("correlationId").GetString());
        Assert.All(response.Headers.GetValues("X-Correlation-ID"),
            value => Assert.Equal("demo-correlation", value));
    }
}
