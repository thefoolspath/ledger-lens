using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public interface IPortfolioCoreStore
{
    Task PortfoliosCreateAsync(UserProfile userProfile, Portfolio portfolio, CancellationToken cancellationToken);
    Task<bool> PortfolioIsOwnedByAsync(Guid portfolioId, Guid ownerUserId, CancellationToken cancellationToken);
    Task InvestmentAccountsCreateAsync(InvestmentAccount account, CancellationToken cancellationToken);
    Task<AccountIdentity?> FindOwnedAccountAsync(Guid accountId, Guid ownerUserId, CancellationToken cancellationToken);
    Task CashLedgerEntriesCreateAsync(CashLedgerEntry entry, CancellationToken cancellationToken);
    Task<CashLedgerEntry?> FindOwnedEntryAsync(Guid entryId, Guid ownerUserId, CancellationToken cancellationToken);
    Task<bool> CashLedgerEntriesCorrectAsync(CashLedgerEntry correctedEntry, CashLedgerEntry reversal, CashLedgerEntry replacement, CancellationToken cancellationToken);
    Task<PortfolioOverview?> PortfoliosGetOneAsync(Guid portfolioId, Guid ownerUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<PortfolioListItem>> PortfoliosGetListAsync(Guid ownerUserId, CancellationToken cancellationToken);
}
