using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Domain;
using Microsoft.Extensions.Options;

namespace LedgerLens.PortfolioCore.Infrastructure;

public sealed class FixedLocalCurrentUser(IOptions<FixedLocalUserOptions> options) : ICurrentUser
{
    private readonly FixedLocalUserOptions value = options.Value;

    public Guid UserId => value.Id;

    public string Email => value.Email;
}
