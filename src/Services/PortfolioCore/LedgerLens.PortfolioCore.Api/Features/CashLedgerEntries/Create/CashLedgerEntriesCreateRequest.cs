namespace LedgerLens.PortfolioCore.Api.Features.CashLedgerEntries.Create;

public sealed record CashLedgerEntriesCreateRequest(
    Guid AccountId,
    string Type,
    decimal Amount,
    string Currency,
    DateTimeOffset EffectiveAt,
    string? Note,
    string? InstrumentSymbol,
    decimal? Quantity,
    decimal? UnitPrice);
