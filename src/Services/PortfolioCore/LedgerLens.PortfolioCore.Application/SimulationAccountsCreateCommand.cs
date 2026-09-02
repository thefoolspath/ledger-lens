using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record SimulationAccountsCreateCommand(Guid PortfolioId, string Name);
