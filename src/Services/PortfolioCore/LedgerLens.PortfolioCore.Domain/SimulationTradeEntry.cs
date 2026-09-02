namespace LedgerLens.PortfolioCore.Domain;

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
