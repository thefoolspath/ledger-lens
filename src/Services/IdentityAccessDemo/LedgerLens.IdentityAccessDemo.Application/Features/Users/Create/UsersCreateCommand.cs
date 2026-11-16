namespace LedgerLens.IdentityAccessDemo.Application.Features.Users.Create;

public sealed record UsersCreateCommand(Guid RequestId, string Email, string DisplayName);
