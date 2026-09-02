using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Api;

public sealed record InvestmentAccountsCreateRequest(Guid PortfolioId, string Name, string Broker, string Currency);
