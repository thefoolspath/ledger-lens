using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record SimulationTradeDraftsCreateCommand(Guid AccountId, SimulationTradeSide Side, SimulationInputMode InputMode,
    decimal? RequestedQuantity, decimal? RequestedAmount, decimal AssumedFee, decimal AssumedTax, decimal? FxRate,
    DateTimeOffset EffectiveAt, SimulationMarketEvidence Quote);
