using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Api;

public sealed record SimulationTradesCorrectRequest(Guid ReplacementDraftId, string Reason);
