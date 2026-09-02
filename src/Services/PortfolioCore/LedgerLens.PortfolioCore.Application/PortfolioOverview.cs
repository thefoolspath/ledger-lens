using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record PortfolioOverview(Guid Id, string Name, string BaseCurrency, string ReportingCurrency,
    IReadOnlyList<AccountOverview> Accounts);
