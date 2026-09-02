using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record CreatePortfolioCommand(string Name, string BaseCurrency, string ReportingCurrency);
public sealed class CreatePortfolioHandler(ICurrentUser currentUser, IPortfolioCoreStore store, TimeProvider timeProvider)
{
    public async Task<PortfolioListItem> HandleAsync(CreatePortfolioCommand command, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var user = new UserProfile(currentUser.UserId, currentUser.Email, now);
        var portfolio = new Portfolio(DomainId.New(), currentUser.UserId, command.Name, command.BaseCurrency, command.ReportingCurrency, now);
        await store.CreatePortfolioAsync(user, portfolio, cancellationToken);
        return new(portfolio.Id, portfolio.Name, portfolio.BaseCurrency, portfolio.ReportingCurrency);
    }
}

public sealed record CreateAccountCommand(Guid PortfolioId, string Name, string Broker, string Currency);
public sealed class CreateAccountHandler(ICurrentUser currentUser, IPortfolioCoreStore store, TimeProvider timeProvider)
{
    public async Task<AccountIdentity?> HandleAsync(CreateAccountCommand command, CancellationToken cancellationToken)
    {
        if (!await store.PortfolioIsOwnedByAsync(command.PortfolioId, currentUser.UserId, cancellationToken)) return null;
        var account = new InvestmentAccount(DomainId.New(), command.PortfolioId, command.Name, command.Broker, command.Currency, timeProvider.GetUtcNow());
        await store.AddAccountAsync(account, cancellationToken);
        return new(account.Id, account.PortfolioId, account.Currency);
    }
}

public sealed record RecordLedgerEntryCommand(Guid AccountId, CashLedgerEntryType Type, decimal Amount, string Currency,
    DateTimeOffset EffectiveAt, string? Note, string? InstrumentSymbol, decimal? Quantity, decimal? UnitPrice);

public sealed class RecordLedgerEntryHandler(ICurrentUser currentUser, IPortfolioCoreStore store, TimeProvider timeProvider)
{
    public async Task<LedgerEntryView?> HandleAsync(RecordLedgerEntryCommand command, CancellationToken cancellationToken)
    {
        var account = await store.FindOwnedAccountAsync(command.AccountId, currentUser.UserId, cancellationToken);
        if (account is null) return null;
        var currency = CurrencyCode.Normalize(command.Currency);
        if (!string.Equals(account.Currency, currency, StringComparison.Ordinal))
            throw new ArgumentException("Ledger entry currency must match the investment account currency.", nameof(command));
        var entry = new CashLedgerEntry(DomainId.New(), command.AccountId, command.Type, command.Amount, currency,
            command.EffectiveAt, command.Note, timeProvider.GetUtcNow(), command.InstrumentSymbol, command.Quantity, command.UnitPrice);
        await store.AddCashEntryAsync(entry, cancellationToken);
        return ToView(entry);
    }

    internal static LedgerEntryView ToView(CashLedgerEntry entry) => new(entry.Id, entry.Type.ToString(), entry.Role.ToString(),
        entry.Amount, entry.SignedAmount, entry.Currency, entry.EffectiveAt, entry.Note, entry.InstrumentSymbol,
        entry.Quantity, entry.SignedQuantity, entry.UnitPrice, entry.CorrectsEntryId, entry.CorrectionReason);
}

public sealed record CorrectLedgerEntryCommand(Guid EntryId, CashLedgerEntryType Type, decimal Amount, string Currency,
    DateTimeOffset EffectiveAt, string? Note, string? InstrumentSymbol, decimal? Quantity, decimal? UnitPrice, string Reason);

public sealed class CorrectLedgerEntryHandler(ICurrentUser currentUser, IPortfolioCoreStore store, TimeProvider timeProvider)
{
    public async Task<CorrectionView?> HandleAsync(CorrectLedgerEntryCommand command, CancellationToken cancellationToken)
    {
        var corrected = await store.FindOwnedEntryAsync(command.EntryId, currentUser.UserId, cancellationToken);
        if (corrected is null) return null;
        if (corrected.Role == LedgerEntryRole.Reversal) throw new InvalidOperationException("Reversal entries cannot be corrected.");
        var currency = CurrencyCode.Normalize(command.Currency);
        if (!string.Equals(corrected.Currency, currency, StringComparison.Ordinal))
            throw new ArgumentException("Replacement currency must match the corrected entry currency.", nameof(command));
        var now = timeProvider.GetUtcNow();
        var reversal = CashLedgerEntry.CreateReversal(corrected, DomainId.New(), command.Reason, now);
        var replacement = new CashLedgerEntry(DomainId.New(), corrected.AccountId, command.Type, command.Amount, currency,
            command.EffectiveAt, command.Note, now, command.InstrumentSymbol, command.Quantity, command.UnitPrice,
            LedgerEntryRole.Replacement, corrected.Id, command.Reason);
        if (!await store.CorrectEntryAsync(corrected, reversal, replacement, cancellationToken))
            throw new InvalidOperationException("This entry has already been corrected.");
        return new(RecordLedgerEntryHandler.ToView(reversal), RecordLedgerEntryHandler.ToView(replacement));
    }
}

public sealed class GetPortfolioOverviewHandler(ICurrentUser currentUser, IPortfolioCoreStore store)
{
    public Task<PortfolioOverview?> HandleAsync(Guid portfolioId, CancellationToken cancellationToken) =>
        store.GetPortfolioOverviewAsync(portfolioId, currentUser.UserId, cancellationToken);
}
public sealed class ListPortfoliosHandler(ICurrentUser currentUser, IPortfolioCoreStore store)
{
    public Task<IReadOnlyList<PortfolioListItem>> HandleAsync(CancellationToken cancellationToken) =>
        store.ListPortfoliosAsync(currentUser.UserId, cancellationToken);
}
