using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using LedgerLens.Application.Abstractions;
using LedgerLens.IdentityAccessDemo.Application;
using LedgerLens.IdentityAccessDemo.Application.Features.Users.Create;
using LedgerLens.IdentityAccessDemo.Application.Features.Users.GetOne;
using LedgerLens.IdentityAccessDemo.Api;
using LedgerLens.IdentityAccessDemo.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace LedgerLens.IdentityAccessDemo.IntegrationTests;

public sealed class UsersEndpointTests
{
    [Fact]
    public async Task Create_and_get_one_use_shared_contract_with_service_local_location()
    {
        var store = new MemoryStore();
        await using var factory = CreateFactory(store);
        using var client = factory.CreateClient();
        var requestId = Guid.CreateVersion7();

        using var created = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId, email = "synthetic@example.test", displayName = "Synthetic User" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Assert.NotNull(created.Headers.Location);
        using var createdBody = JsonDocument.Parse(await created.Content.ReadAsStringAsync());
        var userId = createdBody.RootElement.GetProperty("data").GetProperty("id").GetGuid();
        Assert.Equal($"/v1/users/get-one/{userId}", created.Headers.Location.OriginalString);
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
        using var logger = new CaptureLoggerProvider();
        await using var factory = CreateFactory(store, logger);
        using var client = factory.CreateClient();
        using var invalid = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId = Guid.Empty, email = "", displayName = "" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        Assert.Equal("application/problem+json", invalid.Content.Headers.ContentType?.MediaType);
        using var invalidBody = JsonDocument.Parse(await invalid.Content.ReadAsStringAsync());
        Assert.Equal("common.validation.invalid_request", invalidBody.RootElement.GetProperty("code").GetString());

        using var created = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId = Guid.CreateVersion7(), email = "synthetic@example.test", displayName = "Synthetic User" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        using var conflict = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId = Guid.CreateVersion7(), email = "synthetic@example.test", displayName = "Other" });
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.Equal("application/problem+json", conflict.Content.Headers.ContentType?.MediaType);
        using var conflictBody = JsonDocument.Parse(await conflict.Content.ReadAsStringAsync());
        Assert.Equal("identity_demo.users.email_already_exists",
            conflictBody.RootElement.GetProperty("code").GetString());

        using var missing = await client.GetAsync($"/v1/users/get-one/{Guid.CreateVersion7()}");
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        using var missingBody = JsonDocument.Parse(await missing.Content.ReadAsStringAsync());
        Assert.Equal("common.resource.not_found", missingBody.RootElement.GetProperty("code").GetString());
        Assert.DoesNotContain(logger.Entries, entry => entry.Level == LogLevel.Error);
    }

    [Fact]
    public async Task Reused_request_id_with_changed_input_has_distinct_conflict_code()
    {
        await using var factory = CreateFactory(new RequestIdConflictStore());
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId = Guid.CreateVersion7(), email = "synthetic@example.test", displayName = "Synthetic User" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("identity_demo.users.request_id_conflict",
            body.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Conflict_uses_thai_demo_text_when_requested()
    {
        await using var factory = CreateFactory(new RequestIdConflictStore());
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/users/create")
        {
            Content = JsonContent.Create(new
            {
                requestId = Guid.CreateVersion7(), email = "synthetic@example.test", displayName = "Synthetic User",
            }),
        };
        request.Headers.AcceptLanguage.ParseAdd("th-TH");
        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("รหัสคำขอขัดแย้ง", body.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Unsupported_language_falls_back_to_english_demo_text()
    {
        await using var factory = CreateFactory(new RequestIdConflictStore());
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/users/create")
        {
            Content = JsonContent.Create(new
            {
                requestId = Guid.CreateVersion7(), email = "synthetic@example.test", displayName = "Synthetic User",
            }),
        };
        request.Headers.AcceptLanguage.ParseAdd("fr-FR");
        using var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Request ID conflict", body.RootElement.GetProperty("title").GetString());
    }

    [Fact]
    public async Task Malformed_json_returns_safe_validation_problem()
    {
        using var logger = new CaptureLoggerProvider();
        await using var factory = CreateFactory(new MemoryStore(), logger);
        using var client = factory.CreateClient();
        using var response = await client.PostAsync("/v1/users/create",
            new StringContent("{\"email\":", Encoding.UTF8, "application/json"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("common.validation.invalid_request", body.RootElement.GetProperty("code").GetString());
        Assert.True(body.RootElement.GetProperty("errors").TryGetProperty("request", out _));
        Assert.False(body.RootElement.GetProperty("detail").GetString()?.Contains("JSON", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(logger.Entries, entry => entry.Level == LogLevel.Error);
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
        Assert.DoesNotContain("secret-like marker", body, StringComparison.Ordinal);
        using var problem = JsonDocument.Parse(body);
        Assert.Equal("common.internal.unexpected", problem.RootElement.GetProperty("code").GetString());
        Assert.Equal(response.Headers.GetValues("X-Correlation-ID").Single(),
            problem.RootElement.GetProperty("correlationId").GetString());
        Assert.False(string.IsNullOrWhiteSpace(problem.RootElement.GetProperty("traceId").GetString()));
    }

    [Fact]
    public async Task Unexpected_failure_logs_safe_exception_types_once()
    {
        using var logger = new CaptureLoggerProvider();
        await using var factory = CreateFactory(new ThrowingStore(), logger);
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId = Guid.CreateVersion7(), email = "synthetic@example.test", displayName = "Synthetic User" });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var errors = logger.Entries.Where(entry => entry.Level == LogLevel.Error).ToArray();
        Assert.Single(errors);
        Assert.EndsWith("UnexpectedIdentityExceptionHandler", errors[0].Category, StringComparison.Ordinal);
        Assert.Contains("System.InvalidOperationException", errors[0].Message, StringComparison.Ordinal);
        Assert.Contains("System.ArgumentException", errors[0].Message, StringComparison.Ordinal);
        Assert.Contains("UsersCreate", errors[0].Message, StringComparison.Ordinal);
        Assert.DoesNotContain("sensitive exception detail", errors[0].Message, StringComparison.Ordinal);
        Assert.DoesNotContain("synthetic@example.test", errors[0].Message, StringComparison.Ordinal);
        Assert.DoesNotContain("secret-like marker", errors[0].Message, StringComparison.Ordinal);
        Assert.Null(errors[0].Exception);
    }

    [Fact]
    public async Task Handler_result_with_both_user_and_failure_returns_generic_500()
    {
        await using var factory = CreateFactory(new MemoryStore(), createHandler: new InvalidResultHandler());
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId = Guid.CreateVersion7(), email = "synthetic@example.test", displayName = "Synthetic User" });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("common.internal.unexpected", body.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Unexpected_log_uses_allowlisted_operation_name()
    {
        using var provider = new CaptureLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        var context = new DefaultHttpContext();
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(),
            "secret-like marker"));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        context.RequestAborted = cancellation.Token;
        var handler = new UnexpectedIdentityExceptionHandler(
            loggerFactory.CreateLogger<UnexpectedIdentityExceptionHandler>(), null!);

        Assert.True(await handler.TryHandleAsync(context, new InvalidOperationException("raw marker"), cancellation.Token));
        var error = Assert.Single(provider.Entries, entry => entry.Level == LogLevel.Error);
        Assert.Contains("Operation=Unknown", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("secret-like marker", error.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("raw marker", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Unrelated_argument_exception_is_an_unexpected_failure()
    {
        await using var factory = CreateFactory(new ArgumentThrowingStore());
        using var client = factory.CreateClient();
        using var response = await client.PostAsJsonAsync("/v1/users/create",
            new { requestId = Guid.CreateVersion7(), email = "synthetic@example.test", displayName = "Synthetic User" });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("common.internal.unexpected", body.RootElement.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Request_aborted_cancellation_writes_no_body_or_error_log()
    {
        using var provider = new CaptureLoggerProvider();
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(provider));
        var context = new DefaultHttpContext();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        context.RequestAborted = cancellation.Token;
        context.Response.Body = new MemoryStream();
        var handler = new UnexpectedIdentityExceptionHandler(
            loggerFactory.CreateLogger<UnexpectedIdentityExceptionHandler>(), null!);

        Assert.True(await handler.TryHandleAsync(context, new OperationCanceledException(), cancellation.Token));
        Assert.Equal(499, context.Response.StatusCode);
        Assert.Equal(0, context.Response.Body.Length);
        Assert.DoesNotContain(provider.Entries, entry => entry.Level == LogLevel.Error);
    }

    private static WebApplicationFactory<Program> CreateFactory(object store, ILoggerProvider? provider = null,
        ICommandHandler<UsersCreateCommand, UsersCreateResult>? createHandler = null) =>
        new WebApplicationFactory<Program>().WithWebHostBuilder(webHost =>
        {
            if (provider is not null)
                webHost.ConfigureLogging(logging => logging.AddProvider(provider));
            webHost.ConfigureAppConfiguration((_, configuration) =>
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:identity-demo-db"] = "Host=localhost;Database=synthetic;Username=synthetic;Password=synthetic",
                }));
            webHost.ConfigureTestServices(services =>
            {
                services.RemoveAll<IUsersCreateStore>();
                services.RemoveAll<IUsersGetOneStore>();
                if (createHandler is not null)
                {
                    services.RemoveAll<ICommandHandler<UsersCreateCommand, UsersCreateResult>>();
                    services.AddSingleton(createHandler);
                }
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
            throw new InvalidOperationException("sensitive exception detail synthetic@example.test",
                new ArgumentException("secret-like marker"));
    }

    private sealed class CaptureLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentQueue<(string Category, LogLevel Level, string Message, Exception? Exception)> entries = new();
        public IEnumerable<(string Category, LogLevel Level, string Message, Exception? Exception)> Entries => entries;
        public ILogger CreateLogger(string categoryName) => new CaptureLogger(categoryName, entries);
        public void Dispose() { }

        private sealed class CaptureLogger(string category,
            ConcurrentQueue<(string Category, LogLevel Level, string Message, Exception? Exception)> entries) : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state,
                Exception? exception, Func<TState, Exception?, string> formatter) =>
                entries.Enqueue((category, logLevel, formatter(state, exception), exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }

    private sealed class RequestIdConflictStore : IUsersCreateStore
    {
        public Task<UsersCreateStoreResult> UsersCreateAsync(User user, Guid requestId, CancellationToken cancellationToken) =>
            Task.FromResult(new UsersCreateStoreResult(null, UsersCreateStoreFailure.RequestIdConflict));
    }

    private sealed class ArgumentThrowingStore : IUsersCreateStore
    {
        public Task<UsersCreateStoreResult> UsersCreateAsync(User user, Guid requestId, CancellationToken cancellationToken) =>
            throw new ArgumentException("not a client validation failure");
    }

    private sealed class InvalidResultHandler : ICommandHandler<UsersCreateCommand, UsersCreateResult>
    {
        public Task<UsersCreateResult> HandleAsync(UsersCreateCommand command, CancellationToken cancellationToken) =>
            Task.FromResult(new UsersCreateResult(
                new UserView(Guid.CreateVersion7(), command.Email, command.DisplayName, DateTimeOffset.UtcNow),
                UsersCreateFailure.EmailAlreadyExists));
    }
}
