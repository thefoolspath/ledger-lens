namespace LedgerLens.PortfolioCore.Database.Models;

public sealed class SimulationValuationSnapshot
{
    public Guid Id { get; set; }
    public Guid SimulationAccountId { get; set; }
    public string Symbol { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal CurrentValueUsd { get; set; }
    public decimal RemainingCostUsd { get; set; }
    public decimal RealizedUsd { get; set; }
    public decimal UnrealizedUsd { get; set; }
    public decimal? CurrentFxRate { get; set; }
    public decimal? CurrentValueThb { get; set; }
    public decimal? TotalPlThb { get; set; }
    public bool IsComplete { get; set; }
    public string MissingReasons { get; set; } = null!;
    public string Provider { get; set; } = null!;
    public string Feed { get; set; } = null!;
    public DateTimeOffset PriceAsOf { get; set; }
    public DateTimeOffset RetrievedAt { get; set; }
    public string Freshness { get; set; } = null!;
    public string RequestId { get; set; } = null!;
    public DateTimeOffset RecordedAt { get; set; }
}
