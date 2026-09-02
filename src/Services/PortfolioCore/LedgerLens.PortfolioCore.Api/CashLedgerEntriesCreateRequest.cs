using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Api;

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
