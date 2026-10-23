namespace LedgerLens.PortfolioCore.Application.Features.SimulationTradeDrafts.Confirm;

public sealed record SimulationTradeDraftsConfirmCommand(Guid AccountId, Guid DraftId);
