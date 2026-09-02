using LedgerLens.PortfolioCore.Domain;
using Microsoft.Extensions.Options;

namespace LedgerLens.PortfolioCore.Infrastructure;

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
