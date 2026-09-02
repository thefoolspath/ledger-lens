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
