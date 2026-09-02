using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record LedgerEntryView(Guid Id, string Type, string Role, decimal Amount, decimal SignedAmount,
    string Currency, DateTimeOffset EffectiveAt, string? Note, string? InstrumentSymbol, decimal? Quantity,
    decimal? SignedQuantity, decimal? UnitPrice, Guid? CorrectsEntryId, string? CorrectionReason);
