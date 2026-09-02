using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Api;

public sealed record SimulationValuationsRecordListRequest(Guid AccountId, IReadOnlyList<SimulationMarketEvidence> Quotes, decimal? CurrentFxRate);
