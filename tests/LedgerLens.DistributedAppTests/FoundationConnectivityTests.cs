using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LedgerLens.DistributedAppTests;

public sealed class FoundationConnectivityTests
{
    private static readonly string[] TestAppHostArguments =
    [
        "--LedgerLens:Runtime:UsePersistentVolumes=false",
    ];

    [Fact]
    public async Task AppHost_resource_model_can_be_created()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.LedgerLens_AppHost>(TestAppHostArguments);
        Assert.NotNull(builder);
    }

    [Fact]
    public async Task Complete_graph_becomes_healthy_and_gateway_reaches_every_service_when_distributed_tests_are_enabled()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LEDGERLENS_RUN_DISTRIBUTED_TESTS"), "1", StringComparison.Ordinal))
        {
            return;
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.LedgerLens_AppHost>(TestAppHostArguments, timeout.Token);
        builder.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
        await using var application = await builder.BuildAsync(timeout.Token);
        await application.StartAsync(timeout.Token);

        var resourceNames = new[]
        {
            "postgres",
            "portfolio-db",
            "market-db",
            "slip-db",
            "research-db",
            "operations-db",
            "messaging",
            "portfolio-api",
            "market-api",
            "slip-api",
            "research-api",
            "operations-api",
            "slip-worker",
            "gateway",
            "web",
        };
        foreach (var resourceName in resourceNames)
        {
            await application.ResourceNotifications.WaitForResourceHealthyAsync(resourceName, timeout.Token);
        }

        using var client = application.CreateHttpClient("gateway");
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
            using var response = await client.GetAsync(path, timeout.Token);
            response.EnsureSuccessStatusCode();
        }

        using var webClient = application.CreateHttpClient("web");
        using var webResponse = await webClient.GetAsync("/api/portfolio/internal/ping", timeout.Token);
        webResponse.EnsureSuccessStatusCode();
        Assert.Contains("portfolio-api", await webResponse.Content.ReadAsStringAsync(timeout.Token));
    }
}
