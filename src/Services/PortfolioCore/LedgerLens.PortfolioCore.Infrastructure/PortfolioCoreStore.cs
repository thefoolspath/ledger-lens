using LedgerLens.PortfolioCore.Application;
using LedgerLens.PortfolioCore.Database;
using LedgerLens.PortfolioCore.Domain;
using Microsoft.EntityFrameworkCore;
using DatabaseModels = LedgerLens.PortfolioCore.Database.Models;

namespace LedgerLens.PortfolioCore.Infrastructure;

public sealed class PortfolioCoreStore(PortfolioCoreDbContext dbContext) : IPortfolioCoreStore, ISimulationStore
{
    private static readonly string DepositType = CashLedgerEntryType.Deposit.ToString();
    private static readonly string SellType = CashLedgerEntryType.Sell.ToString();
    private static readonly string DividendType = CashLedgerEntryType.Dividend.ToString();
    private static readonly string ReversalRole = LedgerEntryRole.Reversal.ToString();

    public async Task PortfoliosCreateAsync(
        UserProfile userProfile,
        Portfolio portfolio,
        CancellationToken cancellationToken)
    {
        var executionStrategy = dbContext.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var existingUser = await dbContext.UserProfiles.SingleOrDefaultAsync(
                user => user.Id == userProfile.Id,
                cancellationToken);
            if (existingUser is null)
            {
                dbContext.UserProfiles.Add(ToDatabase(userProfile));
            }
            else
            {
                existingUser.Email = userProfile.Email;
            }

            dbContext.Portfolios.Add(ToDatabase(portfolio));
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        });
    }

    public Task<bool> PortfolioIsOwnedByAsync(
        Guid portfolioId,
        Guid ownerUserId,
        CancellationToken cancellationToken) =>
        dbContext.Portfolios.AnyAsync(
            portfolio => portfolio.Id == portfolioId && portfolio.OwnerUserId == ownerUserId,
            cancellationToken);

    public async Task InvestmentAccountsCreateAsync(InvestmentAccount account, CancellationToken cancellationToken)
    {
        dbContext.InvestmentAccounts.Add(ToDatabase(account));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<AccountIdentity?> FindOwnedAccountAsync(
        Guid accountId,
        Guid ownerUserId,
        CancellationToken cancellationToken) =>
        (from account in dbContext.InvestmentAccounts.AsNoTracking()
         join portfolio in dbContext.Portfolios.AsNoTracking() on account.PortfolioId equals portfolio.Id
         where account.Id == accountId && portfolio.OwnerUserId == ownerUserId
         select new AccountIdentity(account.Id, account.PortfolioId, account.Currency))
        .SingleOrDefaultAsync(cancellationToken);

    public async Task CashLedgerEntriesCreateAsync(CashLedgerEntry entry, CancellationToken cancellationToken)
    {
        dbContext.CashLedgerEntries.Add(ToDatabase(entry));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<CashLedgerEntry?> FindOwnedEntryAsync(
        Guid entryId,
        Guid ownerUserId,
        CancellationToken cancellationToken)
    {
        var entry = await (
            from candidate in dbContext.CashLedgerEntries.AsNoTracking()
            join account in dbContext.InvestmentAccounts.AsNoTracking() on candidate.AccountId equals account.Id
            join portfolio in dbContext.Portfolios.AsNoTracking() on account.PortfolioId equals portfolio.Id
            where candidate.Id == entryId && portfolio.OwnerUserId == ownerUserId
            select candidate)
            .SingleOrDefaultAsync(cancellationToken);
        return entry is null ? null : ToDomain(entry);
    }

    public async Task<bool> CashLedgerEntriesCorrectAsync(
        CashLedgerEntry correctedEntry,
        CashLedgerEntry reversal,
        CashLedgerEntry replacement,
        CancellationToken cancellationToken)
    {
        var corrected = false;
        var executionStrategy = dbContext.Database.CreateExecutionStrategy();
        await executionStrategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            var exists = await dbContext.CashLedgerEntries.AnyAsync(
                entry => entry.Id == correctedEntry.Id,
                cancellationToken);
            var alreadyCorrected = await dbContext.CashLedgerEntries.AnyAsync(
                entry => entry.CorrectsEntryId == correctedEntry.Id && entry.EntryRole == ReversalRole,
                cancellationToken);
            if (!exists || alreadyCorrected)
            {
                await transaction.RollbackAsync(cancellationToken);
                return;
            }

            dbContext.CashLedgerEntries.AddRange(ToDatabase(reversal), ToDatabase(replacement));
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            corrected = true;
        });
        return corrected;
    }

    public async Task<PortfolioOverview?> PortfoliosGetOneAsync(
        Guid portfolioId,
        Guid ownerUserId,
        CancellationToken cancellationToken)
    {
        var portfolio = await dbContext.Portfolios.AsNoTracking()
            .Where(candidate => candidate.Id == portfolioId && candidate.OwnerUserId == ownerUserId)
            .Select(candidate => new PortfolioListItem(
                candidate.Id,
                candidate.Name,
                candidate.BaseCurrency,
                candidate.ReportingCurrency))
            .SingleOrDefaultAsync(cancellationToken);
        if (portfolio is null)
        {
            return null;
        }

        var accounts = await dbContext.InvestmentAccounts.AsNoTracking()
            .Where(account => account.PortfolioId == portfolioId)
            .OrderBy(account => account.CreatedAt)
            .Take(50)
            .ToListAsync(cancellationToken);
        var accountIds = accounts.Select(account => account.Id).ToArray();
        var balances = await dbContext.CashLedgerEntries.AsNoTracking()
            .Where(entry => accountIds.Contains(entry.AccountId))
            .GroupBy(entry => entry.AccountId)
            .Select(group => new
            {
                AccountId = group.Key,
                Balance = group.Sum(entry =>
                    (entry.EntryType == DepositType ||
                     entry.EntryType == SellType ||
                     entry.EntryType == DividendType ? entry.Amount : -entry.Amount) *
                    (entry.EntryRole == ReversalRole ? -1 : 1)),
            })
            .ToDictionaryAsync(item => item.AccountId, item => item.Balance, cancellationToken);
        var entries = await dbContext.CashLedgerEntries.AsNoTracking()
            .Where(entry => accountIds.Contains(entry.AccountId))
            .OrderByDescending(entry => entry.EffectiveAt)
            .ThenByDescending(entry => entry.Id)
            .Take(500)
            .ToListAsync(cancellationToken);

        var accountViews = accounts.Select(account =>
        {
            var accountEntries = entries
                .Where(entry => entry.AccountId == account.Id)
                .Select(ToView)
                .ToArray();
            return new AccountOverview(
                account.Id,
                account.Name,
                account.Broker,
                account.Currency,
                balances.GetValueOrDefault(account.Id),
                accountEntries);
        }).ToArray();

        return new PortfolioOverview(
            portfolio.Id,
            portfolio.Name,
            portfolio.BaseCurrency,
            portfolio.ReportingCurrency,
            accountViews);
    }

    public async Task<IReadOnlyList<PortfolioListItem>> PortfoliosGetListAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken) =>
        await dbContext.Portfolios.AsNoTracking()
            .Where(portfolio => portfolio.OwnerUserId == ownerUserId)
            .OrderBy(portfolio => portfolio.Name)
            .Take(100)
            .Select(portfolio => new PortfolioListItem(
                portfolio.Id,
                portfolio.Name,
                portfolio.BaseCurrency,
                portfolio.ReportingCurrency))
            .ToListAsync(cancellationToken);

    public async Task SimulationAccountsCreateAsync(SimulationAccount account, CancellationToken cancellationToken)
    {
        dbContext.SimulationAccounts.Add(new DatabaseModels.SimulationAccount
        {
            Id = account.Id,
            PortfolioId = account.PortfolioId,
            Name = account.Name,
            CreatedAt = account.CreatedAt,
        });
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<SimulationAccount?> SimulationAccountsGetOneAsync(Guid accountId, Guid ownerUserId,
        CancellationToken cancellationToken)
    {
        var value = await (from account in dbContext.SimulationAccounts.AsNoTracking()
            join portfolio in dbContext.Portfolios.AsNoTracking() on account.PortfolioId equals portfolio.Id
            where account.Id == accountId && portfolio.OwnerUserId == ownerUserId
            select account).SingleOrDefaultAsync(cancellationToken);
        return value is null ? null : new SimulationAccount(value.Id, value.PortfolioId, value.Name, value.CreatedAt);
    }

    public async Task<IReadOnlyList<SimulationAccountListItem>> SimulationAccountsGetListAsync(Guid portfolioId,
        Guid ownerUserId, CancellationToken cancellationToken) =>
        await (from account in dbContext.SimulationAccounts.AsNoTracking()
            join portfolio in dbContext.Portfolios.AsNoTracking() on account.PortfolioId equals portfolio.Id
            where account.PortfolioId == portfolioId && portfolio.OwnerUserId == ownerUserId
            orderby account.CreatedAt
            select new SimulationAccountListItem(account.Id, account.PortfolioId, account.Name, account.CreatedAt))
        .Take(50).ToListAsync(cancellationToken);

    public async Task SimulationTradeDraftsCreateAsync(SimulationTradeDraft draft, CancellationToken cancellationToken)
    {
        dbContext.SimulationTradeDrafts.Add(ToDatabase(draft));
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<SimulationTradeDraft?> FindOwnedSimulationDraftAsync(Guid draftId, Guid ownerUserId,
        CancellationToken cancellationToken)
    {
        var value = await (from draft in dbContext.SimulationTradeDrafts.AsNoTracking()
            join account in dbContext.SimulationAccounts.AsNoTracking() on draft.SimulationAccountId equals account.Id
            join portfolio in dbContext.Portfolios.AsNoTracking() on account.PortfolioId equals portfolio.Id
            where draft.Id == draftId && portfolio.OwnerUserId == ownerUserId
            select draft).SingleOrDefaultAsync(cancellationToken);
        return value is null ? null : ToDomain(value);
    }

    public async Task<SimulationTradeEntry?> SimulationTradeDraftsConfirmAsync(SimulationTradeDraft draft,
        SimulationTradeEntry trade, Guid ownerUserId, CancellationToken cancellationToken)
    {
        SimulationTradeEntry? result = null;
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var storedDraft = await (from candidate in dbContext.SimulationTradeDrafts
                join account in dbContext.SimulationAccounts on candidate.SimulationAccountId equals account.Id
                join portfolio in dbContext.Portfolios on account.PortfolioId equals portfolio.Id
                where candidate.Id == draft.Id && portfolio.OwnerUserId == ownerUserId
                select candidate).SingleOrDefaultAsync(cancellationToken);
            if (storedDraft is null) return;
            if (storedDraft.ConfirmedTradeId is not null)
            {
                var existing = await dbContext.SimulationTradeEntries.AsNoTracking()
                    .SingleAsync(item => item.Id == storedDraft.ConfirmedTradeId.Value, cancellationToken);
                result = ToDomain(existing);
                await transaction.CommitAsync(cancellationToken);
                return;
            }
            storedDraft.ConfirmedTradeId = trade.Id;
            dbContext.SimulationTradeEntries.Add(ToDatabase(trade));
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            result = trade;
        });
        return result;
    }

    public async Task<SimulationTradeEntry?> FindOwnedSimulationTradeAsync(Guid tradeId, Guid ownerUserId,
        CancellationToken cancellationToken)
    {
        var value = await (from trade in dbContext.SimulationTradeEntries.AsNoTracking()
            join account in dbContext.SimulationAccounts.AsNoTracking() on trade.SimulationAccountId equals account.Id
            join portfolio in dbContext.Portfolios.AsNoTracking() on account.PortfolioId equals portfolio.Id
            where trade.Id == tradeId && portfolio.OwnerUserId == ownerUserId
            select trade).SingleOrDefaultAsync(cancellationToken);
        return value is null ? null : ToDomain(value);
    }

    public async Task<bool> SimulationTradesCorrectAsync(SimulationTradeEntry corrected, SimulationTradeEntry reversal,
        SimulationTradeEntry replacement, Guid replacementDraftId, Guid ownerUserId, CancellationToken cancellationToken)
    {
        var result = false;
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(
                System.Data.IsolationLevel.Serializable, cancellationToken);
            var isOwned = await (from trade in dbContext.SimulationTradeEntries
                join account in dbContext.SimulationAccounts on trade.SimulationAccountId equals account.Id
                join portfolio in dbContext.Portfolios on account.PortfolioId equals portfolio.Id
                where trade.Id == corrected.Id && portfolio.OwnerUserId == ownerUserId
                select trade.Id).AnyAsync(cancellationToken);
            var alreadyCorrected = await dbContext.SimulationTradeEntries.AnyAsync(item =>
                item.CorrectsEntryId == corrected.Id && item.EntryRole == SimulationEntryRole.Reversal.ToString(), cancellationToken);
            if (!isOwned || alreadyCorrected) return;
            var draft = await dbContext.SimulationTradeDrafts.SingleOrDefaultAsync(item =>
                item.Id == replacementDraftId && item.SimulationAccountId == corrected.SimulationAccountId &&
                item.ConfirmedTradeId == null, cancellationToken);
            if (draft is null) return;
            dbContext.SimulationTradeEntries.AddRange(ToDatabase(reversal), ToDatabase(replacement));
            draft.ConfirmedTradeId = replacement.Id;
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            result = true;
        });
        return result;
    }

    public async Task<IReadOnlyList<SimulationTradeEntry>> GetSimulationEntriesAsync(Guid accountId, Guid ownerUserId,
        CancellationToken cancellationToken)
    {
        var values = await (from trade in dbContext.SimulationTradeEntries.AsNoTracking()
            join account in dbContext.SimulationAccounts.AsNoTracking() on trade.SimulationAccountId equals account.Id
            join portfolio in dbContext.Portfolios.AsNoTracking() on account.PortfolioId equals portfolio.Id
            where account.Id == accountId && portfolio.OwnerUserId == ownerUserId
            orderby trade.EffectiveAt, trade.RecordedAt, trade.Id
            select trade).Take(5000).ToListAsync(cancellationToken);
        return values.Select(ToDomain).ToArray();
    }

    public Task<IReadOnlyList<SimulationTradeEntry>> SimulationValuationsCalculateSeriesListAsync(
        Guid accountId, Guid ownerUserId, CancellationToken cancellationToken) =>
        GetSimulationEntriesAsync(accountId, ownerUserId, cancellationToken);

    public async Task SimulationValuationsRecordListAsync(IReadOnlyList<SimulationValuationSnapshotData> snapshots,
        CancellationToken cancellationToken)
    {
        foreach (var snapshot in snapshots)
        {
            var value = snapshot.Value;
            dbContext.SimulationValuationSnapshots.Add(new DatabaseModels.SimulationValuationSnapshot
            {
                Id = snapshot.Id,
                SimulationAccountId = snapshot.SimulationAccountId,
                Symbol = value.Symbol,
                Quantity = value.Quantity,
                CurrentPrice = value.CurrentPrice!.Value,
                CurrentValueUsd = value.CurrentValueUsd!.Value,
                RemainingCostUsd = value.RemainingCostUsd,
                RealizedUsd = value.RealizedUsd,
                UnrealizedUsd = value.UnrealizedUsd!.Value,
                CurrentFxRate = value.CurrentFxRate,
                CurrentValueThb = value.CurrentValueThb,
                TotalPlThb = value.TotalPlThb,
                IsComplete = value.IsComplete,
                MissingReasons = string.Join(" | ", value.MissingReasons),
                Provider = value.Quote!.Provider,
                Feed = value.Quote.Feed,
                PriceAsOf = value.Quote.AsOf,
                RetrievedAt = value.Quote.RetrievedAt,
                Freshness = value.Quote.Freshness,
                RequestId = value.Quote.RequestId,
                RecordedAt = snapshot.RecordedAt,
            });
        }
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static DatabaseModels.UserProfile ToDatabase(UserProfile source) => new()
    {
        Id = source.Id,
        Email = source.Email,
        CreatedAt = source.CreatedAt,
    };

    private static DatabaseModels.Portfolio ToDatabase(Portfolio source) => new()
    {
        Id = source.Id,
        OwnerUserId = source.OwnerUserId,
        Name = source.Name,
        BaseCurrency = source.BaseCurrency,
        ReportingCurrency = source.ReportingCurrency,
        CreatedAt = source.CreatedAt,
    };

    private static DatabaseModels.InvestmentAccount ToDatabase(InvestmentAccount source) => new()
    {
        Id = source.Id,
        PortfolioId = source.PortfolioId,
        Name = source.Name,
        Broker = source.Broker,
        Currency = source.Currency,
        CreatedAt = source.CreatedAt,
    };

    private static DatabaseModels.CashLedgerEntry ToDatabase(CashLedgerEntry source) => new()
    {
        Id = source.Id,
        AccountId = source.AccountId,
        EntryType = source.Type.ToString(),
        EntryRole = source.Role.ToString(),
        Amount = source.Amount,
        Currency = source.Currency,
        EffectiveAt = source.EffectiveAt,
        Note = source.Note,
        RecordedAt = source.RecordedAt,
        InstrumentSymbol = source.InstrumentSymbol,
        Quantity = source.Quantity,
        UnitPrice = source.UnitPrice,
        CorrectsEntryId = source.CorrectsEntryId,
        CorrectionReason = source.CorrectionReason,
    };

    private static DatabaseModels.SimulationTradeDraft ToDatabase(SimulationTradeDraft source)
    {
        var evidence = source.Evidence;
        return new()
        {
            Id = source.Id, SimulationAccountId = source.SimulationAccountId, Side = source.Side.ToString(),
            InputMode = source.InputMode.ToString(), RequestedQuantity = source.RequestedQuantity,
            RequestedAmount = source.RequestedAmount, Quantity = source.Quantity, GrossAmount = source.GrossAmount,
            UnusedAmount = source.UnusedAmount, AssumedFee = source.AssumedFee, AssumedTax = source.AssumedTax,
            AcquisitionFxRate = source.AcquisitionFxRate, EffectiveAt = source.EffectiveAt, CreatedAt = source.CreatedAt,
            ExpiresAt = source.ExpiresAt, MarketInstrumentId = evidence.MarketInstrumentId, Symbol = evidence.Symbol,
            Exchange = evidence.Exchange, Mic = evidence.Mic, Currency = evidence.Currency, Price = evidence.Price,
            PriceKind = evidence.PriceKind, Provider = evidence.Provider, Feed = evidence.Feed, PriceAsOf = evidence.AsOf,
            RetrievedAt = evidence.RetrievedAt, Freshness = evidence.Freshness, DelaySeconds = evidence.DelaySeconds,
            RequestId = evidence.RequestId, RetentionPolicyKey = evidence.RetentionPolicyKey,
            ConfirmedTradeId = source.ConfirmedTradeId,
        };
    }

    private static DatabaseModels.SimulationTradeEntry ToDatabase(SimulationTradeEntry source)
    {
        var evidence = source.Evidence;
        return new()
        {
            Id = source.Id, SimulationAccountId = source.SimulationAccountId, Side = source.Side.ToString(),
            Quantity = source.Quantity, UnitPrice = source.UnitPrice, GrossAmount = source.GrossAmount,
            AssumedFee = source.AssumedFee, AssumedTax = source.AssumedTax, FxRate = source.FxRate,
            EffectiveAt = source.EffectiveAt, RecordedAt = source.RecordedAt,
            MarketInstrumentId = evidence.MarketInstrumentId, Symbol = evidence.Symbol, Exchange = evidence.Exchange,
            Mic = evidence.Mic, Currency = evidence.Currency, Price = evidence.Price, PriceKind = evidence.PriceKind,
            Provider = evidence.Provider, Feed = evidence.Feed, PriceAsOf = evidence.AsOf, RetrievedAt = evidence.RetrievedAt,
            Freshness = evidence.Freshness, DelaySeconds = evidence.DelaySeconds, RequestId = evidence.RequestId,
            RetentionPolicyKey = evidence.RetentionPolicyKey, EntryRole = source.Role.ToString(),
            CorrectsEntryId = source.CorrectsEntryId, CorrectionReason = source.CorrectionReason,
        };
    }

    private static SimulationTradeDraft ToDomain(DatabaseModels.SimulationTradeDraft source) => new(source.Id,
        source.SimulationAccountId, ParseEnum<SimulationTradeSide>(source.Side, nameof(source.Side)),
        ParseEnum<SimulationInputMode>(source.InputMode, nameof(source.InputMode)), source.RequestedQuantity,
        source.RequestedAmount, source.Quantity, source.GrossAmount, source.UnusedAmount, source.AssumedFee,
        source.AssumedTax, source.AcquisitionFxRate, source.EffectiveAt, source.CreatedAt, source.ExpiresAt,
        Evidence(source), source.ConfirmedTradeId);

    private static SimulationTradeEntry ToDomain(DatabaseModels.SimulationTradeEntry source) => new(source.Id,
        source.SimulationAccountId, ParseEnum<SimulationTradeSide>(source.Side, nameof(source.Side)), source.Quantity,
        source.UnitPrice, source.GrossAmount, source.AssumedFee, source.AssumedTax, source.FxRate, source.EffectiveAt,
        source.RecordedAt, Evidence(source), ParseEnum<SimulationEntryRole>(source.EntryRole, nameof(source.EntryRole)),
        source.CorrectsEntryId, source.CorrectionReason);

    private static SimulationMarketEvidence Evidence(DatabaseModels.SimulationTradeDraft source) => new(
        source.MarketInstrumentId, source.Symbol, source.Exchange, source.Mic, source.Currency, source.Price,
        source.PriceKind, source.Provider, source.Feed, source.PriceAsOf, source.RetrievedAt, source.Freshness,
        source.DelaySeconds, source.RequestId, source.RetentionPolicyKey);

    private static SimulationMarketEvidence Evidence(DatabaseModels.SimulationTradeEntry source) => new(
        source.MarketInstrumentId, source.Symbol, source.Exchange, source.Mic, source.Currency, source.Price,
        source.PriceKind, source.Provider, source.Feed, source.PriceAsOf, source.RetrievedAt, source.Freshness,
        source.DelaySeconds, source.RequestId, source.RetentionPolicyKey);

    private static CashLedgerEntry ToDomain(DatabaseModels.CashLedgerEntry source) => new(
        source.Id,
        source.AccountId,
        ParseEnum<CashLedgerEntryType>(source.EntryType, nameof(source.EntryType)),
        source.Amount,
        source.Currency,
        source.EffectiveAt,
        source.Note,
        source.RecordedAt,
        source.InstrumentSymbol,
        source.Quantity,
        source.UnitPrice,
        ParseEnum<LedgerEntryRole>(source.EntryRole, nameof(source.EntryRole)),
        source.CorrectsEntryId,
        source.CorrectionReason);

    private static LedgerEntryView ToView(DatabaseModels.CashLedgerEntry entry)
    {
        var type = ParseEnum<CashLedgerEntryType>(entry.EntryType, nameof(entry.EntryType));
        var role = ParseEnum<LedgerEntryRole>(entry.EntryRole, nameof(entry.EntryRole));
        var roleDirection = role == LedgerEntryRole.Reversal ? -1 : 1;
        var amountDirection = type is CashLedgerEntryType.Deposit or CashLedgerEntryType.Sell or CashLedgerEntryType.Dividend
            ? 1
            : -1;
        decimal? signedQuantity = entry.Quantity is null
            ? null
            : entry.Quantity.Value * (type == CashLedgerEntryType.Buy ? 1 : -1) * roleDirection;
        return new LedgerEntryView(
            entry.Id,
            entry.EntryType,
            entry.EntryRole,
            entry.Amount,
            amountDirection * entry.Amount * roleDirection,
            entry.Currency,
            entry.EffectiveAt,
            entry.Note,
            entry.InstrumentSymbol,
            entry.Quantity,
            signedQuantity,
            entry.UnitPrice,
            entry.CorrectsEntryId,
            entry.CorrectionReason);
    }

    private static TEnum ParseEnum<TEnum>(string value, string fieldName)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(value, ignoreCase: false, out var parsed) && Enum.IsDefined(parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException($"Database field {fieldName} contains unsupported value '{value}'.");
    }
}
