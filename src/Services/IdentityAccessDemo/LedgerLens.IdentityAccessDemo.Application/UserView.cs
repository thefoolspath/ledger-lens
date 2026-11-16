using LedgerLens.IdentityAccessDemo.Domain;

namespace LedgerLens.IdentityAccessDemo.Application;

public sealed record UserView(Guid Id, string Email, string DisplayName, DateTimeOffset CreatedAt)
{
    public static UserView From(User user) => new(user.Id, user.Email, user.DisplayName, user.CreatedAt);
}
