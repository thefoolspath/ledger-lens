using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record SimulationValuationSnapshotData(Guid Id, Guid SimulationAccountId, SimulationValuationView Value,
    DateTimeOffset RecordedAt);
