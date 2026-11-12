namespace LedgerLens.IdentityAccess.Application.Features.Users.Create;

public sealed record UsersCreateCommand(Guid RequestId, string Email, string DisplayName);
