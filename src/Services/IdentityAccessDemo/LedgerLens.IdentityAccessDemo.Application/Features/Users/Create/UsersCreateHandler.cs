using LedgerLens.Application.Abstractions;
using LedgerLens.IdentityAccessDemo.Domain;

namespace LedgerLens.IdentityAccessDemo.Application.Features.Users.Create;

public sealed class UsersCreateHandler(IUsersCreateStore store, TimeProvider clock)
    : ICommandHandler<UsersCreateCommand, UsersCreateResult>
{
    public async Task<UsersCreateResult> HandleAsync(UsersCreateCommand command, CancellationToken cancellationToken)
    {
        if (command.RequestId == Guid.Empty || command.RequestId.Version != 7 ||
            string.IsNullOrWhiteSpace(command.Email) || command.Email.Length > 320 ||
            !System.Net.Mail.MailAddress.TryCreate(command.Email, out var address) ||
            !string.Equals(address.Address, command.Email, StringComparison.Ordinal) ||
            !string.Equals(address.Host, "example.test", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(command.DisplayName) || command.DisplayName.Length > 200)
            return UsersCreateResult.Failed(UsersCreateFailure.InvalidRequest);

        var user = new User(Guid.CreateVersion7(), command.Email, command.DisplayName, clock.GetUtcNow());
        var stored = await store.UsersCreateAsync(user, command.RequestId, cancellationToken);
        return stored.Failure switch
        {
            UsersCreateStoreFailure.EmailAlreadyExists => UsersCreateResult.Failed(UsersCreateFailure.EmailAlreadyExists),
            UsersCreateStoreFailure.RequestIdConflict => UsersCreateResult.Failed(UsersCreateFailure.RequestIdConflict),
            null when stored.User is not null => UsersCreateResult.Succeeded(UserView.From(stored.User)),
            _ => throw new InvalidOperationException("UsersCreate store returned an invalid result."),
        };
    }
}
