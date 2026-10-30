using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application.Features.SimulationValuations.RecordList;

public sealed record SimulationValuationsRecordListCommand(Guid AccountId, IReadOnlyList<SimulationMarketEvidence> Quotes,
    decimal? CurrentFxRate);
