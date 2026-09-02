namespace LedgerLens.PortfolioCore.Domain;

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
