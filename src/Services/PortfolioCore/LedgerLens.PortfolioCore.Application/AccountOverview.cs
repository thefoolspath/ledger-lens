using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record AccountOverview(Guid Id, string Name, string Broker, string Currency, decimal CashBalance,
    IReadOnlyList<LedgerEntryView> Entries);
