using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record SimulationValuationsRecordListCommand(Guid AccountId, IReadOnlyList<SimulationMarketEvidence> Quotes,
    decimal? CurrentFxRate);
