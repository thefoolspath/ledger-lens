using LedgerLens.IdentityAccessDemo.Application.Features.Users.Create;
using LedgerLens.IdentityAccessDemo.Application.Features.Users.GetOne;
using LedgerLens.IdentityAccessDemo.Domain;

namespace LedgerLens.IdentityAccessDemo.UnitTests;

public sealed class UsersHandlersTests
{
    [Fact]
    public async Task Create_returns_uuidv7_user_and_forwards_cancellation()
    {
        var store = new StubStore();
        using var source = new CancellationTokenSource();
        var result = await new UsersCreateHandler(store, TimeProvider.System)
            .HandleAsync(new(Guid.CreateVersion7(), "synthetic@example.test", "Synthetic User"), source.Token);

        Assert.Null(result.Failure);
        Assert.NotNull(result.User);
        Assert.Equal(7, result.User.Id.Version);
        Assert.Equal(source.Token, store.LastToken);
    }

    [Fact]
    public async Task Create_returns_typed_failures_without_throwing()
    {
        var store = new StubStore { Failure = UsersCreateStoreFailure.EmailAlreadyExists };
        var handler = new UsersCreateHandler(store, TimeProvider.System);
        var invalid = await handler.HandleAsync(new(Guid.Empty, "", ""), CancellationToken.None);
        var duplicate = await handler.HandleAsync(new(Guid.CreateVersion7(), "synthetic@example.test", "Synthetic User"), CancellationToken.None);

        Assert.Equal(UsersCreateFailure.InvalidRequest, invalid.Failure);
        Assert.Equal(UsersCreateFailure.EmailAlreadyExists, duplicate.Failure);
        Assert.Equal(1, store.CreateCalls);
    }

    [Fact]
    public async Task Create_rejects_email_outside_demo_domain_before_store_call()
    {
        var store = new StubStore();
        var handler = new UsersCreateHandler(store, TimeProvider.System);

        var result = await handler.HandleAsync(
            new(Guid.CreateVersion7(), "person@example.com", "Synthetic User"), CancellationToken.None);

        Assert.Equal(UsersCreateFailure.InvalidRequest, result.Failure);
        Assert.Equal(0, store.CreateCalls);
    }

    [Fact]
    public async Task GetOne_maps_single_user_and_forwards_cancellation()
    {
        var user = new User(Guid.CreateVersion7(), "synthetic@example.test", "Synthetic User", DateTimeOffset.UtcNow);
        var store = new StubStore { Existing = user };
        using var source = new CancellationTokenSource();
        var result = await new UsersGetOneHandler(store)
            .HandleAsync(new(user.Id), source.Token);

        Assert.Equal(user.Id, result?.Id);
        Assert.Equal(source.Token, store.LastToken);
    }

    [Fact]
    public void User_rejects_non_uuidv7_identity()
    {
        Assert.Throws<ArgumentException>(() =>
            new User(Guid.NewGuid(), "synthetic@example.test", "Synthetic User", DateTimeOffset.UtcNow));
    }

    private sealed class StubStore : IUsersCreateStore, IUsersGetOneStore
    {
        public UsersCreateStoreFailure? Failure { get; set; }
        public int CreateCalls { get; private set; }
        public User? Existing { get; set; }
        public CancellationToken LastToken { get; private set; }

        public Task<UsersCreateStoreResult> UsersCreateAsync(User user, Guid requestId, CancellationToken cancellationToken)
        {
            CreateCalls++;
            LastToken = cancellationToken;
            return Task.FromResult(Failure is null
                ? new UsersCreateStoreResult(user, null)
                : new UsersCreateStoreResult(null, Failure));
        }

        public Task<User?> UsersGetOneAsync(Guid userId, CancellationToken cancellationToken)
        {
            LastToken = cancellationToken;
            return Task.FromResult(Existing?.Id == userId ? Existing : null);
        }
    }
}
