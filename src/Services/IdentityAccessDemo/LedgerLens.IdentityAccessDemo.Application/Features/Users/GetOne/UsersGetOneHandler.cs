using LedgerLens.Application.Abstractions;

namespace LedgerLens.IdentityAccessDemo.Application.Features.Users.GetOne;

public sealed class UsersGetOneHandler(IUsersGetOneStore store) : IQueryHandler<UsersGetOneQuery, UserView?>
{
    public async Task<UserView?> HandleAsync(UsersGetOneQuery query, CancellationToken cancellationToken)
    {
        var user = await store.UsersGetOneAsync(query.UserId, cancellationToken);
        return user is null ? null : UserView.From(user);
    }
}
