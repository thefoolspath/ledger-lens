using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record SimulationDraftView(Guid Id, Guid SimulationAccountId, string Side, string InputMode,
    decimal? RequestedQuantity, decimal? RequestedAmount, decimal Quantity, decimal GrossAmount, decimal UnusedAmount,
    decimal AssumedFee, decimal AssumedTax, decimal NetCash, decimal? FxRate, DateTimeOffset EffectiveAt,
    DateTimeOffset ExpiresAt, SimulationMarketEvidence Quote, Guid? ConfirmedTradeId);
