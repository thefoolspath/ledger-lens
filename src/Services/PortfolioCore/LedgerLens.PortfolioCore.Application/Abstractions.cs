using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public interface ICurrentUser { Guid UserId { get; } string Email { get; } }

public interface IPortfolioCoreStore
{
    Task CreatePortfolioAsync(UserProfile userProfile, Portfolio portfolio, CancellationToken cancellationToken);
    Task<bool> PortfolioIsOwnedByAsync(Guid portfolioId, Guid ownerUserId, CancellationToken cancellationToken);
    Task AddAccountAsync(InvestmentAccount account, CancellationToken cancellationToken);
    Task<AccountIdentity?> FindOwnedAccountAsync(Guid accountId, Guid ownerUserId, CancellationToken cancellationToken);
    Task AddCashEntryAsync(CashLedgerEntry entry, CancellationToken cancellationToken);
    Task<CashLedgerEntry?> FindOwnedEntryAsync(Guid entryId, Guid ownerUserId, CancellationToken cancellationToken);
    Task<bool> CorrectEntryAsync(CashLedgerEntry correctedEntry, CashLedgerEntry reversal, CashLedgerEntry replacement, CancellationToken cancellationToken);
    Task<PortfolioOverview?> GetPortfolioOverviewAsync(Guid portfolioId, Guid ownerUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PortfolioListItem>> ListPortfoliosAsync(Guid ownerUserId, CancellationToken cancellationToken);
}

public sealed record AccountIdentity(Guid Id, Guid PortfolioId, string Currency);
public sealed record PortfolioListItem(Guid Id, string Name, string BaseCurrency, string ReportingCurrency);
public sealed record LedgerEntryView(Guid Id, string Type, string Role, decimal Amount, decimal SignedAmount,
    string Currency, DateTimeOffset EffectiveAt, string? Note, string? InstrumentSymbol, decimal? Quantity,
    decimal? SignedQuantity, decimal? UnitPrice, Guid? CorrectsEntryId, string? CorrectionReason);
public sealed record CorrectionView(LedgerEntryView Reversal, LedgerEntryView Replacement);
public sealed record AccountOverview(Guid Id, string Name, string Broker, string Currency, decimal CashBalance,
    IReadOnlyList<LedgerEntryView> Entries);
public sealed record PortfolioOverview(Guid Id, string Name, string BaseCurrency, string ReportingCurrency,
    IReadOnlyList<AccountOverview> Accounts);
