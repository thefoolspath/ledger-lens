using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record PortfolioListItem(Guid Id, string Name, string BaseCurrency, string ReportingCurrency);
