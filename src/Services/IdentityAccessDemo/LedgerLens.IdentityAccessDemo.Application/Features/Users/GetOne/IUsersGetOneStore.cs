using LedgerLens.IdentityAccessDemo.Domain;

namespace LedgerLens.IdentityAccessDemo.Application.Features.Users.GetOne;

public interface IUsersGetOneStore
{
    Task<User?> UsersGetOneAsync(Guid userId, CancellationToken cancellationToken);
}
