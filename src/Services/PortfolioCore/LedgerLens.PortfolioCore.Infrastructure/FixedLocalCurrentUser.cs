using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Domain;
using Microsoft.Extensions.Options;

namespace LedgerLens.PortfolioCore.Infrastructure;

public sealed class FixedLocalUserOptions
{
    public const string SectionName = "LedgerLens:FixedUser";

    public Guid Id { get; init; }

    public string Email { get; init; } = string.Empty;
}

public sealed class FixedLocalCurrentUser(IOptions<FixedLocalUserOptions> options) : ICurrentUser
{
    private readonly FixedLocalUserOptions value = options.Value;

    public Guid UserId => value.Id;

    public string Email => value.Email;
}

internal sealed class FixedLocalUserOptionsValidator : IValidateOptions<FixedLocalUserOptions>
{
    public ValidateOptionsResult Validate(string? name, FixedLocalUserOptions options)
    {
        try
        {
            DomainId.RequireVersion7(options.Id, nameof(options.Id));
            _ = new UserProfile(options.Id, options.Email, DateTimeOffset.UnixEpoch);
            return ValidateOptionsResult.Success;
        }
        catch (ArgumentException exception)
        {
            return ValidateOptionsResult.Fail(exception.Message);
        }
    }
}
