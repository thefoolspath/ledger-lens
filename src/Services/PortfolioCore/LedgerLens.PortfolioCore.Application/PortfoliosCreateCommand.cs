using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record PortfoliosCreateCommand(string Name, string BaseCurrency, string ReportingCurrency);
