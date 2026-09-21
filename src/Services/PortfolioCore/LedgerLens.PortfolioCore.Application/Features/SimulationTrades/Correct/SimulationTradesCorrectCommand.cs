namespace LedgerLens.PortfolioCore.Application.Features.SimulationTrades.Correct;

public sealed record SimulationTradesCorrectCommand(Guid TradeId, Guid ReplacementDraftId, string Reason);
