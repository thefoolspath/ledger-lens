using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record SimulationAccountListItem(Guid Id, Guid PortfolioId, string Name, DateTimeOffset CreatedAt);
