using LedgerLens.IdentityAccess.Domain;

namespace LedgerLens.IdentityAccess.Application.Features.Users.Create;

public sealed record UsersCreateStoreResult(User? User, UsersCreateStoreFailure? Failure);
