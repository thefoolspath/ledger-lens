using LedgerLens.IdentityAccess.Domain;

namespace LedgerLens.IdentityAccess.Application.Features.Users.GetOne;

public interface IUsersGetOneStore
{
    Task<User?> UsersGetOneAsync(Guid userId, CancellationToken cancellationToken);
}
