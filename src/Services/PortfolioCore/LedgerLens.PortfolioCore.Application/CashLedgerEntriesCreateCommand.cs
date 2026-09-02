using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record CashLedgerEntriesCreateCommand(Guid AccountId, CashLedgerEntryType Type, decimal Amount, string Currency,
    DateTimeOffset EffectiveAt, string? Note, string? InstrumentSymbol, decimal? Quantity, decimal? UnitPrice);
