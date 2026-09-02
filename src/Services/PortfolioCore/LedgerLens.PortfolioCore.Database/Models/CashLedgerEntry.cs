namespace LedgerLens.PortfolioCore.Database.Models;

public partial class CashLedgerEntry
{
    public Guid Id { get; set; }

    public Guid AccountId { get; set; }

    public string EntryType { get; set; } = null!;

    public string EntryRole { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public DateTimeOffset EffectiveAt { get; set; }

    public string? Note { get; set; }

    public DateTimeOffset RecordedAt { get; set; }

    public string? InstrumentSymbol { get; set; }

    public decimal? Quantity { get; set; }

    public decimal? UnitPrice { get; set; }

    public Guid? CorrectsEntryId { get; set; }

    public string? CorrectionReason { get; set; }
}
