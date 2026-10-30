namespace LedgerLens.PortfolioCore.Application.Features.InvestmentAccounts.Create;

public sealed record InvestmentAccountsCreateCommand(Guid PortfolioId, string Name, string Broker, string Currency);
