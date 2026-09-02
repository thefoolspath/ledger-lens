using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record SimulationTradeView(Guid Id, string Side, decimal Quantity, decimal UnitPrice, decimal GrossAmount,
    decimal AssumedFee, decimal AssumedTax, decimal? FxRate, decimal NetCash, DateTimeOffset EffectiveAt,
    DateTimeOffset RecordedAt, string Symbol, string Provider, string Feed, DateTimeOffset PriceAsOf,
    string Freshness, string Role, Guid? CorrectsEntryId, string? CorrectionReason);
