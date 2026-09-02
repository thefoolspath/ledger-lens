using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed class CashLedgerEntriesCreateHandler(ICurrentUser currentUser, IPortfolioCoreStore store, TimeProvider timeProvider)
{
    public async Task<LedgerEntryView?> HandleAsync(CashLedgerEntriesCreateCommand command, CancellationToken cancellationToken)
    {
        var account = await store.FindOwnedAccountAsync(command.AccountId, currentUser.UserId, cancellationToken);
        if (account is null) return null;
        var currency = CurrencyCode.Normalize(command.Currency);
        if (!string.Equals(account.Currency, currency, StringComparison.Ordinal))
            throw new ArgumentException("Ledger entry currency must match the investment account currency.", nameof(command));
        var entry = new CashLedgerEntry(DomainId.New(), command.AccountId, command.Type, command.Amount, currency,
            command.EffectiveAt, command.Note, timeProvider.GetUtcNow(), command.InstrumentSymbol, command.Quantity, command.UnitPrice);
        await store.CashLedgerEntriesCreateAsync(entry, cancellationToken);
        return ToView(entry);
    }

    internal static LedgerEntryView ToView(CashLedgerEntry entry) => new(entry.Id, entry.Type.ToString(), entry.Role.ToString(),
        entry.Amount, entry.SignedAmount, entry.Currency, entry.EffectiveAt, entry.Note, entry.InstrumentSymbol,
        entry.Quantity, entry.SignedQuantity, entry.UnitPrice, entry.CorrectsEntryId, entry.CorrectionReason);
}
