using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed class CashLedgerEntriesCorrectHandler(ICurrentUser currentUser, IPortfolioCoreStore store, TimeProvider timeProvider)
{
    public async Task<CorrectionView?> HandleAsync(CashLedgerEntriesCorrectCommand command, CancellationToken cancellationToken)
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
        if (!await store.CashLedgerEntriesCorrectAsync(corrected, reversal, replacement, cancellationToken))
            throw new InvalidOperationException("This entry has already been corrected.");
        return new(CashLedgerEntriesCreateHandler.ToView(reversal), CashLedgerEntriesCreateHandler.ToView(replacement));
    }
}
