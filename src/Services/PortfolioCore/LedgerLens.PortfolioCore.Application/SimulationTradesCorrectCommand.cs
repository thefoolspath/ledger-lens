using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record SimulationTradesCorrectCommand(Guid TradeId, Guid ReplacementDraftId, string Reason);
