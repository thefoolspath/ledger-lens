namespace LedgerLens.PortfolioCore.Application.Features.SimulationAccounts.Create;

public sealed record SimulationAccountsCreateCommand(Guid PortfolioId, string Name);
