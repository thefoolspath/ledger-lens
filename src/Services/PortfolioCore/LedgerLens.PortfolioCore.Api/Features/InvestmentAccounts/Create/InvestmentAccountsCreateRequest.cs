namespace LedgerLens.PortfolioCore.Api.Features.InvestmentAccounts.Create;

public sealed record InvestmentAccountsCreateRequest(Guid PortfolioId, string Name, string Broker, string Currency);
