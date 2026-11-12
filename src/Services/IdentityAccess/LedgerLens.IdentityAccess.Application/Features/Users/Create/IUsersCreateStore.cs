using LedgerLens.IdentityAccess.Domain;

namespace LedgerLens.IdentityAccess.Application.Features.Users.Create;

public interface IUsersCreateStore
{
    Task<UsersCreateStoreResult> UsersCreateAsync(User user, Guid requestId, CancellationToken cancellationToken);
}
