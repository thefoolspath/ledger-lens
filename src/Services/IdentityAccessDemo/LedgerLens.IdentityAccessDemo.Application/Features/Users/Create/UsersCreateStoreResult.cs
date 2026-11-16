using LedgerLens.IdentityAccessDemo.Domain;

namespace LedgerLens.IdentityAccessDemo.Application.Features.Users.Create;

public sealed record UsersCreateStoreResult(User? User, UsersCreateStoreFailure? Failure);
