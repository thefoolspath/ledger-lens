using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record InvestmentAccountsCreateCommand(Guid PortfolioId, string Name, string Broker, string Currency);
