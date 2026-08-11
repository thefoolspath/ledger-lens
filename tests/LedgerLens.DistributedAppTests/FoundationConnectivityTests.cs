using Aspire.Hosting.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LedgerLens.DistributedAppTests;

public sealed class FoundationConnectivityTests
{
    [Fact]
    public async Task AppHost_resource_model_can_be_created()
    {
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.LedgerLens_AppHost>();
        Assert.NotNull(builder);
    }

    [Fact]
    public async Task Gateway_reaches_every_service_when_distributed_tests_are_enabled()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LEDGERLENS_RUN_DISTRIBUTED_TESTS"), "1", StringComparison.Ordinal))
        {
            return;
        }

        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.LedgerLens_AppHost>(timeout.Token);
        builder.Services.AddLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
        await using var application = await builder.BuildAsync(timeout.Token);
        await application.StartAsync(timeout.Token);
        await application.ResourceNotifications.WaitForResourceHealthyAsync("gateway", timeout.Token);

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
    }
}
