namespace LedgerLens.PortfolioCore.Domain;

public enum CashLedgerEntryType { Deposit = 1, Withdrawal, Buy, Sell, Fee, Tax, Dividend }
public enum LedgerEntryRole { Original = 1, Reversal, Replacement }

public sealed class CashLedgerEntry
{
    private CashLedgerEntry() { }

    public CashLedgerEntry(Guid id, Guid accountId, CashLedgerEntryType type, decimal amount, string currency,
        DateTimeOffset effectiveAt, string? note, DateTimeOffset recordedAt, string? instrumentSymbol = null,
        decimal? quantity = null, decimal? unitPrice = null, LedgerEntryRole role = LedgerEntryRole.Original,
        Guid? correctsEntryId = null, string? correctionReason = null)
    {
        DomainId.RequireVersion7(id, nameof(id));
        DomainId.RequireVersion7(accountId, nameof(accountId));
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        if (!Enum.IsDefined(role)) throw new ArgumentOutOfRangeException(nameof(role));
        RequirePositiveScale(amount, 10, nameof(amount));

        var isTrade = type is CashLedgerEntryType.Buy or CashLedgerEntryType.Sell;
        if (isTrade)
        {
            InstrumentSymbol = NormalizeSymbol(instrumentSymbol);
            if (quantity is null || unitPrice is null)
                throw new ArgumentException("Buy and sell entries require quantity and unit price.");
            RequirePositiveScale(quantity.Value, 12, nameof(quantity));
            RequirePositiveScale(unitPrice.Value, 10, nameof(unitPrice));
            if (quantity.Value * unitPrice.Value != amount)
                throw new ArgumentException("Trade amount must equal quantity multiplied by unit price.", nameof(amount));
            Quantity = quantity;
            UnitPrice = unitPrice;
        }
        else if (instrumentSymbol is not null || quantity is not null || unitPrice is not null)
        {
            throw new ArgumentException("Instrument, quantity, and unit price are allowed only for buy and sell entries.");
        }

        if (role == LedgerEntryRole.Original)
        {
            if (correctsEntryId is not null || correctionReason is not null)
                throw new ArgumentException("Original entries cannot reference a correction.");
        }
        else
        {
            if (correctsEntryId is null)
                throw new ArgumentException("Correction entries must reference the corrected entry.", nameof(correctsEntryId));
            DomainId.RequireVersion7(correctsEntryId.Value, nameof(correctsEntryId));
            CorrectionReason = NormalizeRequiredText(correctionReason, 500, nameof(correctionReason));
        }

        Id = id;
        AccountId = accountId;
        Type = type;
        Role = role;
        Amount = amount;
        Currency = CurrencyCode.Normalize(currency);
        EffectiveAt = effectiveAt.ToUniversalTime();
        Note = NormalizeOptionalText(note, 500, nameof(note));
        RecordedAt = recordedAt.ToUniversalTime();
        CorrectsEntryId = correctsEntryId;
    }

    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public CashLedgerEntryType Type { get; private set; }
    public LedgerEntryRole Role { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTimeOffset EffectiveAt { get; private set; }
    public string? Note { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public string? InstrumentSymbol { get; private set; }
    public decimal? Quantity { get; private set; }
    public decimal? UnitPrice { get; private set; }
    public Guid? CorrectsEntryId { get; private set; }
    public string? CorrectionReason { get; private set; }

    public decimal SignedAmount => Direction * Amount * (Role == LedgerEntryRole.Reversal ? -1 : 1);
    public decimal? SignedQuantity => Quantity is null ? null
        : Quantity.Value * (Type == CashLedgerEntryType.Buy ? 1 : -1) * (Role == LedgerEntryRole.Reversal ? -1 : 1);

    public static CashLedgerEntry CreateReversal(CashLedgerEntry correctedEntry, Guid id, string reason, DateTimeOffset recordedAt) =>
        new(id, correctedEntry.AccountId, correctedEntry.Type, correctedEntry.Amount, correctedEntry.Currency,
            correctedEntry.EffectiveAt, correctedEntry.Note, recordedAt, correctedEntry.InstrumentSymbol,
            correctedEntry.Quantity, correctedEntry.UnitPrice, LedgerEntryRole.Reversal, correctedEntry.Id, reason);

    private int Direction => Type is CashLedgerEntryType.Deposit or CashLedgerEntryType.Sell or CashLedgerEntryType.Dividend ? 1 : -1;

    private static void RequirePositiveScale(decimal value, int scale, string parameterName)
    {
        if (value <= 0 || decimal.Round(value, scale) != value)
            throw new ArgumentOutOfRangeException(parameterName, $"Value must be positive with at most {scale} decimal places.");
    }

    private static string NormalizeSymbol(string? symbol)
    {
        var normalized = NormalizeRequiredText(symbol, 32, nameof(symbol)).ToUpperInvariant();
        if (normalized.Any(character => !char.IsLetterOrDigit(character) && character is not '.' and not '-' and not '_'))
            throw new ArgumentException("Instrument symbol contains unsupported characters.", nameof(symbol));
        return normalized;
    }

    private static string NormalizeRequiredText(string? value, int maxLength, string parameterName) =>
        NormalizeOptionalText(value, maxLength, parameterName) ?? throw new ArgumentException("Value is required.", parameterName);

    private static string? NormalizeOptionalText(string? value, int maxLength, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var normalized = value.Trim();
        if (normalized.Length > maxLength)
            throw new ArgumentException($"Value cannot exceed {maxLength} characters.", parameterName);
        return normalized;
    }
}
