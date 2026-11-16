namespace LedgerLens.IdentityAccessDemo.Api.Features.Users.Create;

public sealed record UsersCreateRequest(Guid RequestId, string Email, string DisplayName);
