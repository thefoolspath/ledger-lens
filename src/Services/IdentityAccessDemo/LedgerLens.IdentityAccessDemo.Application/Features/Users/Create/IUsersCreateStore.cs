using LedgerLens.IdentityAccessDemo.Domain;

namespace LedgerLens.IdentityAccessDemo.Application.Features.Users.Create;

public interface IUsersCreateStore
{
    Task<UsersCreateStoreResult> UsersCreateAsync(User user, Guid requestId, CancellationToken cancellationToken);
}
