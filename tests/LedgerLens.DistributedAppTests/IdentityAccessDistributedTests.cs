using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Aspire.Hosting.Testing;

namespace LedgerLens.DistributedAppTests;

public sealed class IdentityAccessDistributedTests
{
    [Fact]
    public async Task IdentityAccess_migration_and_two_operations_work_against_ephemeral_PostgreSQL()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("LEDGERLENS_RUN_DISTRIBUTED_TESTS"), "1", StringComparison.Ordinal))
            return;

        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(4));
        var arguments = new[]
        {
            "--LedgerLens:Runtime:UsePersistentVolumes=false",
            "--Parameters:fixed-user-id=0198f0aa-0000-7000-8000-000000000001",
            "--Parameters:fixed-user-email=owner@example.invalid",
        };
        var builder = await DistributedApplicationTestingBuilder.CreateAsync<Projects.LedgerLens_AppHost>(
            arguments, timeout.Token);
        await using var application = await builder.BuildAsync(timeout.Token);
        await application.StartAsync(timeout.Token);
        await application.ResourceNotifications.WaitForResourceHealthyAsync("identity-api", timeout.Token);
        using var client = application.CreateHttpClient("identity-api");

        var requestId = Guid.CreateVersion7();
        var request = new { requestId, email = "synthetic@example.test", displayName = "Synthetic User" };
        using var created = await client.PostAsJsonAsync("/v1/users/create", request, timeout.Token);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Null(created.Headers.Location);
        using var createdBody = JsonDocument.Parse(await created.Content.ReadAsStringAsync(timeout.Token));
        var userId = createdBody.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        Assert.Equal(7, userId.Version);

        using var retry = await client.PostAsJsonAsync("/v1/users/create", request, timeout.Token);
        Assert.Equal(HttpStatusCode.Created, retry.StatusCode);
        using var retryBody = JsonDocument.Parse(await retry.Content.ReadAsStringAsync(timeout.Token));
        Assert.Equal(userId, retryBody.RootElement.GetProperty("data").GetProperty("id").GetGuid());

        using var getOne = await client.GetAsync($"/v1/users/get-one/{userId}", timeout.Token);
        Assert.Equal(HttpStatusCode.OK, getOne.StatusCode);
        using var getOneBody = JsonDocument.Parse(await getOne.Content.ReadAsStringAsync(timeout.Token));
        Assert.Equal(userId, getOneBody.RootElement.GetProperty("data").GetProperty("id").GetGuid());

        using var emailConflict = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId = Guid.CreateVersion7(), email = "SYNTHETIC@example.test", displayName = "Other User" },
            timeout.Token);
        Assert.Equal(HttpStatusCode.Conflict, emailConflict.StatusCode);

        using var requestConflict = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId, email = "other@example.test", displayName = "Other User" }, timeout.Token);
        Assert.Equal(HttpStatusCode.Conflict, requestConflict.StatusCode);
    }
}
