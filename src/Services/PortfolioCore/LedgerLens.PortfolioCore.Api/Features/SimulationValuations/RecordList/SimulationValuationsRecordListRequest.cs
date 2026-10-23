using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Api.Features.SimulationValuations.RecordList;

public sealed record SimulationValuationsRecordListRequest(Guid AccountId, IReadOnlyList<SimulationMarketEvidence> Quotes, decimal? CurrentFxRate);
