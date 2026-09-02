namespace LedgerLens.PortfolioCore.Database.Models;

public sealed class SimulationTradeDraft
{
    public Guid Id { get; set; }
    public Guid SimulationAccountId { get; set; }
    public string Side { get; set; } = null!;
    public string InputMode { get; set; } = null!;
    public decimal? RequestedQuantity { get; set; }
    public decimal? RequestedAmount { get; set; }
    public decimal Quantity { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal UnusedAmount { get; set; }
    public decimal AssumedFee { get; set; }
    public decimal AssumedTax { get; set; }
    public decimal? AcquisitionFxRate { get; set; }
    public DateTimeOffset EffectiveAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public Guid MarketInstrumentId { get; set; }
    public string Symbol { get; set; } = null!;
    public string Exchange { get; set; } = null!;
    public string Mic { get; set; } = null!;
    public string Currency { get; set; } = null!;
    public decimal Price { get; set; }
    public string PriceKind { get; set; } = null!;
    public string Provider { get; set; } = null!;
    public string Feed { get; set; } = null!;
    public DateTimeOffset PriceAsOf { get; set; }
    public DateTimeOffset RetrievedAt { get; set; }
    public string Freshness { get; set; } = null!;
    public int? DelaySeconds { get; set; }
    public string RequestId { get; set; } = null!;
    public string RetentionPolicyKey { get; set; } = null!;
    public Guid? ConfirmedTradeId { get; set; }
}
