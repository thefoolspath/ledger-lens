using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Api;

public sealed record PortfoliosCreateRequest(string Name, string BaseCurrency, string ReportingCurrency);
