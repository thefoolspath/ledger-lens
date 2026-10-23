namespace LedgerLens.PortfolioCore.Application.Features.Portfolios.Create;

public sealed record PortfoliosCreateCommand(string Name, string BaseCurrency, string ReportingCurrency);
