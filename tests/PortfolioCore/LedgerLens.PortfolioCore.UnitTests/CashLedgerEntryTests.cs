using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.UnitTests;

public sealed class CashLedgerEntryTests
{
    [Fact]
    public void All_entry_types_have_deterministic_signed_amounts()
    {
        var accountId = Guid.CreateVersion7();
        var occurredAt = new DateTimeOffset(2026, 8, 26, 10, 0, 0, TimeSpan.Zero);

        var deposit = new CashLedgerEntry(
            Guid.CreateVersion7(), accountId, CashLedgerEntryType.Deposit, 125.25m, "usd", occurredAt, null, occurredAt);
        var withdrawal = new CashLedgerEntry(
            Guid.CreateVersion7(), accountId, CashLedgerEntryType.Withdrawal, 25.10m, "USD", occurredAt, null, occurredAt);

        var buy = Trade(CashLedgerEntryType.Buy, 2m, 10m, occurredAt);
        var sell = Trade(CashLedgerEntryType.Sell, 3m, 10m, occurredAt);
        var fee = Entry(CashLedgerEntryType.Fee, 1m, occurredAt);
        var tax = Entry(CashLedgerEntryType.Tax, 2m, occurredAt);
        var dividend = Entry(CashLedgerEntryType.Dividend, 5m, occurredAt);

        Assert.Equal(new[] { 125.25m, -25.10m, -20m, 30m, -1m, -2m, 5m },
            new[] { deposit.SignedAmount, withdrawal.SignedAmount, buy.SignedAmount, sell.SignedAmount,
                fee.SignedAmount, tax.SignedAmount, dividend.SignedAmount });
        Assert.Equal(112.15m, deposit.SignedAmount + withdrawal.SignedAmount + buy.SignedAmount +
            sell.SignedAmount + fee.SignedAmount + tax.SignedAmount + dividend.SignedAmount);
    }

    [Fact]
    public void Trade_requires_exact_gross_amount_and_normalizes_symbol()
    {
        var trade = Trade(CashLedgerEntryType.Buy, 1.25m, 8m, DateTimeOffset.UnixEpoch);
        Assert.Equal("SYNTH", trade.InstrumentSymbol);
        Assert.Equal(1.25m, trade.SignedQuantity);
        Assert.Equal(10m, trade.Amount);

        Assert.Throws<ArgumentException>(() => new CashLedgerEntry(Guid.CreateVersion7(), Guid.CreateVersion7(),
            CashLedgerEntryType.Buy, 9m, "USD", DateTimeOffset.UnixEpoch, null, DateTimeOffset.UnixEpoch,
            "SYNTH", 1.25m, 8m));
    }

    [Fact]
    public void Reversal_and_replacement_are_append_only_and_reconcile_to_replacement()
    {
        var time = DateTimeOffset.UnixEpoch;
        var original = Entry(CashLedgerEntryType.Deposit, 100m, time);
        var reversal = CashLedgerEntry.CreateReversal(original, Guid.CreateVersion7(), "Synthetic correction", time.AddMinutes(1));
        var replacement = new CashLedgerEntry(Guid.CreateVersion7(), original.AccountId, CashLedgerEntryType.Deposit,
            125m, "USD", time, "Replacement", time.AddMinutes(1), role: LedgerEntryRole.Replacement,
            correctsEntryId: original.Id, correctionReason: "Synthetic correction");

        Assert.Equal(-100m, reversal.SignedAmount);
        Assert.Equal(125m, original.SignedAmount + reversal.SignedAmount + replacement.SignedAmount);
        Assert.Equal(original.Id, reversal.CorrectsEntryId);
        Assert.Equal(original.Id, replacement.CorrectsEntryId);
    }

    [Fact]
    public void Non_trade_rejects_trade_fields_and_quantity_scale_is_bounded()
    {
        Assert.Throws<ArgumentException>(() => new CashLedgerEntry(Guid.CreateVersion7(), Guid.CreateVersion7(),
            CashLedgerEntryType.Dividend, 1m, "USD", DateTimeOffset.UnixEpoch, null, DateTimeOffset.UnixEpoch,
            "SYNTH", 1m, 1m));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CashLedgerEntry(Guid.CreateVersion7(), Guid.CreateVersion7(),
            CashLedgerEntryType.Buy, 1m, "USD", DateTimeOffset.UnixEpoch, null, DateTimeOffset.UnixEpoch,
            "SYNTH", 0.0000000000001m, 10000000000000m));
    }

    [Fact]
    public void Amount_rejects_more_than_ten_decimal_places()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => new CashLedgerEntry(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            CashLedgerEntryType.Deposit,
            0.00000000001m,
            "USD",
            DateTimeOffset.UnixEpoch,
            null,
            DateTimeOffset.UnixEpoch));

        Assert.Equal("amount", exception.ParamName);
    }

    [Fact]
    public void LedgerLens_identifiers_reject_uuid_version_four()
    {
        Assert.Throws<ArgumentException>(() => new Portfolio(
            Guid.NewGuid(),
            Guid.CreateVersion7(),
            "Synthetic portfolio",
            "USD",
            "THB",
            DateTimeOffset.UnixEpoch));
    }

    private static CashLedgerEntry Entry(CashLedgerEntryType type, decimal amount, DateTimeOffset time) =>
        new(Guid.CreateVersion7(), Guid.CreateVersion7(), type, amount, "USD", time, null, time);

    private static CashLedgerEntry Trade(CashLedgerEntryType type, decimal quantity, decimal price, DateTimeOffset time) =>
        new(Guid.CreateVersion7(), Guid.CreateVersion7(), type, quantity * price, "USD", time, null, time,
            "synth", quantity, price);
}
