namespace LedgerLens.PortfolioCore.Api.Features.SimulationAccounts.Create;

public sealed record SimulationAccountsCreateRequest(Guid PortfolioId, string Name);
