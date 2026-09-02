namespace LedgerLens.PortfolioCore.Domain;

public enum SimulationTradeSide { Buy = 1, Sell }
public enum SimulationInputMode { ByQuantity = 1, ByAmount }
public enum SimulationEntryRole { Original = 1, Reversal, Replacement }

public sealed record SimulationMarketEvidence(
    Guid MarketInstrumentId,
    string Symbol,
    string Exchange,
    string Mic,
    string Currency,
    decimal Price,
    string PriceKind,
    string Provider,
    string Feed,
    DateTimeOffset AsOf,
    DateTimeOffset RetrievedAt,
    string Freshness,
    int? DelaySeconds,
    string RequestId,
    string RetentionPolicyKey)
{
    public SimulationMarketEvidence Normalize()
    {
        DomainId.RequireVersion7(MarketInstrumentId, nameof(MarketInstrumentId));
        RequirePositive(Price, 10, nameof(Price));
        if (!string.Equals(PriceKind, "LastTrade", StringComparison.Ordinal))
            throw new ArgumentException("Simulation trades require a LastTrade observation.", nameof(PriceKind));
        return this with
        {
            Symbol = SymbolValue(Symbol),
            Exchange = SimulationAccount.NormalizeText(Exchange, 80, nameof(Exchange)),
            Mic = SimulationAccount.NormalizeText(Mic, 8, nameof(Mic)).ToUpperInvariant(),
            Currency = CurrencyCode.Normalize(Currency),
            Provider = SimulationAccount.NormalizeText(Provider, 40, nameof(Provider)),
            Feed = SimulationAccount.NormalizeText(Feed, 80, nameof(Feed)),
            Freshness = SimulationAccount.NormalizeText(Freshness, 32, nameof(Freshness)),
            RequestId = SimulationAccount.NormalizeText(RequestId, 100, nameof(RequestId)),
            RetentionPolicyKey = SimulationAccount.NormalizeText(RetentionPolicyKey, 80, nameof(RetentionPolicyKey)),
            AsOf = AsOf.ToUniversalTime(),
            RetrievedAt = RetrievedAt.ToUniversalTime(),
        };
    }

    public static string SymbolValue(string? value)
    {
        var symbol = SimulationAccount.NormalizeText(value, 32, nameof(Symbol)).ToUpperInvariant();
        if (symbol.Any(character => !char.IsLetterOrDigit(character) && character is not '.' and not '-' and not '_'))
            throw new ArgumentException("Instrument symbol contains unsupported characters.", nameof(Symbol));
        return symbol;
    }

    internal static void RequirePositive(decimal value, int scale, string name)
    {
        if (value <= 0 || decimal.Round(value, scale) != value)
            throw new ArgumentOutOfRangeException(name, $"Value must be positive with at most {scale} decimal places.");
    }

    internal static void RequireNonNegative(decimal value, int scale, string name)
    {
        if (value < 0 || decimal.Round(value, scale) != value)
            throw new ArgumentOutOfRangeException(name, $"Value must be non-negative with at most {scale} decimal places.");
    }
}

public sealed class SimulationTradeDraft
{
    private SimulationTradeDraft() { }

    public SimulationTradeDraft(Guid id, Guid simulationAccountId, SimulationTradeSide side, SimulationInputMode inputMode,
        decimal? requestedQuantity, decimal? requestedAmount, decimal quantity, decimal grossAmount, decimal unusedAmount,
        decimal assumedFee, decimal assumedTax, decimal? acquisitionFxRate, DateTimeOffset effectiveAt,
        DateTimeOffset createdAt, DateTimeOffset expiresAt, SimulationMarketEvidence evidence, Guid? confirmedTradeId = null)
    {
        DomainId.RequireVersion7(id, nameof(id));
        DomainId.RequireVersion7(simulationAccountId, nameof(simulationAccountId));
        if (!Enum.IsDefined(side)) throw new ArgumentOutOfRangeException(nameof(side));
        if (!Enum.IsDefined(inputMode)) throw new ArgumentOutOfRangeException(nameof(inputMode));
        SimulationMarketEvidence.RequirePositive(quantity, 12, nameof(quantity));
        SimulationMarketEvidence.RequirePositive(grossAmount, 10, nameof(grossAmount));
        SimulationMarketEvidence.RequireNonNegative(unusedAmount, 10, nameof(unusedAmount));
        SimulationMarketEvidence.RequireNonNegative(assumedFee, 10, nameof(assumedFee));
        SimulationMarketEvidence.RequireNonNegative(assumedTax, 10, nameof(assumedTax));
        if (acquisitionFxRate is not null) SimulationMarketEvidence.RequirePositive(acquisitionFxRate.Value, 12, nameof(acquisitionFxRate));
        if (confirmedTradeId is not null) DomainId.RequireVersion7(confirmedTradeId.Value, nameof(confirmedTradeId));
        Id = id;
        SimulationAccountId = simulationAccountId;
        Side = side;
        InputMode = inputMode;
        RequestedQuantity = requestedQuantity;
        RequestedAmount = requestedAmount;
        Quantity = quantity;
        GrossAmount = grossAmount;
        UnusedAmount = unusedAmount;
        AssumedFee = assumedFee;
        AssumedTax = assumedTax;
        AcquisitionFxRate = acquisitionFxRate;
        EffectiveAt = effectiveAt.ToUniversalTime();
        CreatedAt = createdAt.ToUniversalTime();
        ExpiresAt = expiresAt.ToUniversalTime();
        Evidence = evidence.Normalize();
        ConfirmedTradeId = confirmedTradeId;
    }

    public Guid Id { get; private set; }
    public Guid SimulationAccountId { get; private set; }
    public SimulationTradeSide Side { get; private set; }
    public SimulationInputMode InputMode { get; private set; }
    public decimal? RequestedQuantity { get; private set; }
    public decimal? RequestedAmount { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal GrossAmount { get; private set; }
    public decimal UnusedAmount { get; private set; }
    public decimal AssumedFee { get; private set; }
    public decimal AssumedTax { get; private set; }
    public decimal? AcquisitionFxRate { get; private set; }
    public DateTimeOffset EffectiveAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public SimulationMarketEvidence Evidence { get; private set; } = null!;
    public Guid? ConfirmedTradeId { get; private set; }
    public decimal NetCash => Side == SimulationTradeSide.Buy
        ? -(GrossAmount + AssumedFee + AssumedTax)
        : GrossAmount - AssumedFee - AssumedTax;

    public static SimulationTradeDraft Create(Guid id, Guid accountId, SimulationTradeSide side, SimulationInputMode inputMode,
        decimal? requestedQuantity, decimal? requestedAmount, decimal fee, decimal tax, decimal? fxRate,
        DateTimeOffset effectiveAt, DateTimeOffset createdAt, SimulationMarketEvidence evidence)
    {
        var normalizedEvidence = evidence.Normalize();
        decimal quantity;
        decimal gross;
        decimal unused;
        if (inputMode == SimulationInputMode.ByQuantity)
        {
            if (requestedQuantity is null || requestedAmount is not null) throw new ArgumentException("ByQuantity requires only requestedQuantity.");
            SimulationMarketEvidence.RequirePositive(requestedQuantity.Value, 12, nameof(requestedQuantity));
            quantity = requestedQuantity.Value;
            gross = FloorMoney(quantity * normalizedEvidence.Price);
            unused = 0m;
        }
        else
        {
            if (requestedAmount is null || requestedQuantity is not null) throw new ArgumentException("ByAmount requires only requestedAmount.");
            SimulationMarketEvidence.RequirePositive(requestedAmount.Value, 10, nameof(requestedAmount));
            quantity = decimal.Floor((requestedAmount.Value / normalizedEvidence.Price) * 1_000_000_000_000m) / 1_000_000_000_000m;
            SimulationMarketEvidence.RequirePositive(quantity, 12, nameof(requestedAmount));
            gross = FloorMoney(quantity * normalizedEvidence.Price);
            unused = requestedAmount.Value - gross;
        }
        return new(id, accountId, side, inputMode, requestedQuantity, requestedAmount, quantity, gross, unused,
            fee, tax, fxRate, effectiveAt, createdAt, createdAt.AddMinutes(15), normalizedEvidence);
    }

    internal static decimal FloorMoney(decimal value) => decimal.Floor(value * 10_000_000_000m) / 10_000_000_000m;

    public void MarkConfirmed(Guid tradeId)
    {
        DomainId.RequireVersion7(tradeId, nameof(tradeId));
        ConfirmedTradeId ??= tradeId;
        if (ConfirmedTradeId != tradeId) throw new InvalidOperationException("Draft was already confirmed as another trade.");
    }
}

public sealed class SimulationTradeEntry
{
    private SimulationTradeEntry() { }

    public SimulationTradeEntry(Guid id, Guid simulationAccountId, SimulationTradeSide side, decimal quantity,
        decimal unitPrice, decimal grossAmount, decimal assumedFee, decimal assumedTax, decimal? fxRate,
        DateTimeOffset effectiveAt, DateTimeOffset recordedAt, SimulationMarketEvidence evidence,
        SimulationEntryRole role = SimulationEntryRole.Original, Guid? correctsEntryId = null, string? correctionReason = null)
    {
        DomainId.RequireVersion7(id, nameof(id));
        DomainId.RequireVersion7(simulationAccountId, nameof(simulationAccountId));
        if (!Enum.IsDefined(side)) throw new ArgumentOutOfRangeException(nameof(side));
        if (!Enum.IsDefined(role)) throw new ArgumentOutOfRangeException(nameof(role));
        SimulationMarketEvidence.RequirePositive(quantity, 12, nameof(quantity));
        SimulationMarketEvidence.RequirePositive(unitPrice, 10, nameof(unitPrice));
        SimulationMarketEvidence.RequirePositive(grossAmount, 10, nameof(grossAmount));
        SimulationMarketEvidence.RequireNonNegative(assumedFee, 10, nameof(assumedFee));
        SimulationMarketEvidence.RequireNonNegative(assumedTax, 10, nameof(assumedTax));
        if (SimulationTradeDraft.FloorMoney(quantity * unitPrice) != grossAmount)
            throw new ArgumentException("Gross amount must equal rounded quantity multiplied by unit price.", nameof(grossAmount));
        if (side == SimulationTradeSide.Sell && grossAmount <= assumedFee + assumedTax)
            throw new ArgumentException("Sell proceeds must remain positive after assumptions.");
        if (fxRate is not null) SimulationMarketEvidence.RequirePositive(fxRate.Value, 12, nameof(fxRate));
        if (role == SimulationEntryRole.Original && (correctsEntryId is not null || correctionReason is not null))
            throw new ArgumentException("Original entries cannot reference corrections.");
        if (role != SimulationEntryRole.Original)
        {
            if (correctsEntryId is null) throw new ArgumentException("Correction entry requires a target.", nameof(correctsEntryId));
            DomainId.RequireVersion7(correctsEntryId.Value, nameof(correctsEntryId));
            CorrectionReason = SimulationAccount.NormalizeText(correctionReason, 500, nameof(correctionReason));
        }
        Id = id;
        SimulationAccountId = simulationAccountId;
        Side = side;
        Quantity = quantity;
        UnitPrice = unitPrice;
        GrossAmount = grossAmount;
        AssumedFee = assumedFee;
        AssumedTax = assumedTax;
        FxRate = fxRate;
        EffectiveAt = effectiveAt.ToUniversalTime();
        RecordedAt = recordedAt.ToUniversalTime();
        Evidence = evidence.Normalize();
        Role = role;
        CorrectsEntryId = correctsEntryId;
    }

    public Guid Id { get; private set; }
    public Guid SimulationAccountId { get; private set; }
    public SimulationTradeSide Side { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal GrossAmount { get; private set; }
    public decimal AssumedFee { get; private set; }
    public decimal AssumedTax { get; private set; }
    public decimal? FxRate { get; private set; }
    public DateTimeOffset EffectiveAt { get; private set; }
    public DateTimeOffset RecordedAt { get; private set; }
    public SimulationMarketEvidence Evidence { get; private set; } = null!;
    public SimulationEntryRole Role { get; private set; }
    public Guid? CorrectsEntryId { get; private set; }
    public string? CorrectionReason { get; private set; }
    public decimal NetCash => Side == SimulationTradeSide.Buy ? -(GrossAmount + AssumedFee + AssumedTax) : GrossAmount - AssumedFee - AssumedTax;

    public static SimulationTradeEntry FromDraft(Guid id, SimulationTradeDraft draft, DateTimeOffset recordedAt) =>
        new(id, draft.SimulationAccountId, draft.Side, draft.Quantity, draft.Evidence.Price, draft.GrossAmount,
            draft.AssumedFee, draft.AssumedTax, draft.AcquisitionFxRate, draft.EffectiveAt, recordedAt, draft.Evidence);

    public static SimulationTradeEntry Reversal(Guid id, SimulationTradeEntry corrected, string reason, DateTimeOffset recordedAt) =>
        new(id, corrected.SimulationAccountId, corrected.Side, corrected.Quantity, corrected.UnitPrice, corrected.GrossAmount,
            corrected.AssumedFee, corrected.AssumedTax, corrected.FxRate, corrected.EffectiveAt, recordedAt, corrected.Evidence,
            SimulationEntryRole.Reversal, corrected.Id, reason);

    public static SimulationTradeEntry Replacement(Guid id, SimulationTradeDraft draft, Guid correctedEntryId,
        string reason, DateTimeOffset recordedAt) =>
        new(id, draft.SimulationAccountId, draft.Side, draft.Quantity, draft.Evidence.Price, draft.GrossAmount,
            draft.AssumedFee, draft.AssumedTax, draft.AcquisitionFxRate, draft.EffectiveAt, recordedAt, draft.Evidence,
            SimulationEntryRole.Replacement, correctedEntryId, reason);
}
