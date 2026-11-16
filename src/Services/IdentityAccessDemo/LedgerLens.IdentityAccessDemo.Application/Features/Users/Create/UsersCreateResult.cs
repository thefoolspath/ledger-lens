namespace LedgerLens.IdentityAccessDemo.Application.Features.Users.Create;

public sealed record UsersCreateResult(UserView? User, UsersCreateFailure? Failure)
{
    public static UsersCreateResult Succeeded(UserView user) => new(user, null);
    public static UsersCreateResult Failed(UsersCreateFailure failure) => new(null, failure);
}
