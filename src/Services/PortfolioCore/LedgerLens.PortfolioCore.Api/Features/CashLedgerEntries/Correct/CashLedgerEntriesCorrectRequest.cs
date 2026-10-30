namespace LedgerLens.PortfolioCore.Api.Features.CashLedgerEntries.Correct;

public sealed record CashLedgerEntriesCorrectRequest(
    string Type,
    decimal Amount,
    string Currency,
    DateTimeOffset EffectiveAt,
    string? Note,
    string? InstrumentSymbol,
    decimal? Quantity,
    decimal? UnitPrice,
    string Reason);
