namespace LedgerLens.PortfolioCore.Api.Features.SimulationTrades.Correct;

public sealed record SimulationTradesCorrectRequest(Guid ReplacementDraftId, string Reason);
