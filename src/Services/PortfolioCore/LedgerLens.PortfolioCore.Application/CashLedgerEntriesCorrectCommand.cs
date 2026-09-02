using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record CashLedgerEntriesCorrectCommand(Guid EntryId, CashLedgerEntryType Type, decimal Amount, string Currency,
    DateTimeOffset EffectiveAt, string? Note, string? InstrumentSymbol, decimal? Quantity, decimal? UnitPrice, string Reason);
