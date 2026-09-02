using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record AccountIdentity(Guid Id, Guid PortfolioId, string Currency);
