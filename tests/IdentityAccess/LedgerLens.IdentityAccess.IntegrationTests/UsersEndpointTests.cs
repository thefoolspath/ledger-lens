using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using LedgerLens.IdentityAccess.Application.Features.Users.Create;
using LedgerLens.IdentityAccess.Application.Features.Users.GetOne;
using LedgerLens.IdentityAccess.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LedgerLens.IdentityAccess.IntegrationTests;

public sealed class UsersEndpointTests
{
    [Fact]
    public async Task Create_and_get_one_use_shared_contract_without_public_location()
    {
        var store = new MemoryStore();
        await using var factory = CreateFactory(store);
        using var client = factory.CreateClient();
        var requestId = Guid.CreateVersion7();

        using var created = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId, email = "synthetic@example.test", displayName = "Synthetic User" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.Null(created.Headers.Location);
        using var createdBody = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var userId = createdBody.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        Assert.Equal(7, userId.Version);
        Assert.True(createdBody.RootElement.TryGetProperty("meta", out _));

        using var fetched = await client.GetAsync($"/v1/users/get-one/{userId}");
        Assert.Equal(HttpStatusCode.OK, fetched.StatusCode);
        using var fetchedBody = JsonDocument.Parse(await fetched.Content.ReadAsStringAsync());
        Assert.Equal(userId, fetchedBody.RootElement.GetProperty("data").GetProperty("id").GetGuid());
    }

    [Fact]
    public async Task Expected_failures_are_problem_details_and_unknown_user_is_not_found()
    {
        var store = new MemoryStore();
        await using var factory = CreateFactory(store);
        using var client = factory.CreateClient();
        using var invalid = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId = Guid.Empty, email = "", displayName = "" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal("application/problem+json", invalid.Content.Headers.ContentType?.MediaType);

        using var created = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId = Guid.CreateVersion7(), email = "synthetic@example.test", displayName = "Synthetic User" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var conflict = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId = Guid.CreateVersion7(), email = "synthetic@example.test", displayName = "Other" });
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Equal("application/problem+json", conflict.Content.Headers.ContentType?.MediaType);

        using var missing = await client.GetAsync($"/v1/users/get-one/{Guid.CreateVersion7()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
    }

    [Fact]
    public async Task Unexpected_failure_returns_generic_problem_without_exception_text()
    {
        await using var factory = CreateFactory(new ThrowingStore());
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId = Guid.CreateVersion7(), email = "synthetic@example.test", displayName = "Synthetic User" });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.DoesNotContain("sensitive exception detail", body, StringComparison.Ordinal);
    }

    private static WebApplicationFactory<Program> CreateFactory(object store) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(webHost =>
        {
            webHost.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:identity-db"] = "Host=localhost;Database=synthetic;Username=synthetic;Password=synthetic",
                }));
            webHost.ConfigureTestServices(services =>
            {
                services.RemoveAll<IUsersCreateStore>();
                services.RemoveAll<IUsersGetOneStore>();
                services.AddSingleton((IUsersCreateStore)store);
                if (store is IUsersGetOneStore readStore)
                {
                    services.AddSingleton(readStore);
                }
                else
                {
                    services.AddSingleton<IUsersGetOneStore>(new MemoryStore());
                }
            });
        });

    private sealed class MemoryStore : IUsersCreateStore, IUsersGetOneStore
    {
        private readonly Dictionary<Guid, User> users = [];

        public Task<UsersCreateStoreResult> UsersCreateAsync(User user, Guid requestId, CancellationToken cancellationToken)
        {
            if (users.Values.Any(existing => string.Equals(existing.Email, user.Email, StringComparison.OrdinalIgnoreCase)))
                return Task.FromResult(new UsersCreateStoreResult(null, UsersCreateStoreFailure.EmailAlreadyExists));
            users.Add(user.Id, user);
            return Task.FromResult(new UsersCreateStoreResult(user, null));
        }

        public Task<User?> UsersGetOneAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult(users.GetValueOrDefault(userId));
    }

    private sealed class ThrowingStore : IUsersCreateStore
    {
        public Task<UsersCreateStoreResult> UsersCreateAsync(User user, Guid requestId, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("sensitive exception detail");
    }
}
