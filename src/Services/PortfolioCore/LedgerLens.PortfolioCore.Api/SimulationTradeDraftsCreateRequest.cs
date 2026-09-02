using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Api;

public sealed record SimulationTradeDraftsCreateRequest(Guid AccountId, string Side, string InputMode, decimal? RequestedQuantity,
    decimal? RequestedAmount, decimal AssumedFee, decimal AssumedTax, decimal? FxRate, DateTimeOffset EffectiveAt,
    SimulationMarketEvidence Quote);
