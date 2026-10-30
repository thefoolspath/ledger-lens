namespace LedgerLens.PortfolioCore.Api.Features.Portfolios.Create;

public sealed record PortfoliosCreateRequest(string Name, string BaseCurrency, string ReportingCurrency);
