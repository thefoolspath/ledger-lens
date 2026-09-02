namespace LedgerLens.PortfolioCore.Domain;

public sealed record SimulationValuation(
    SimulationPosition Position,
    decimal CurrentPrice,
    decimal CurrentValueUsd,
    decimal UnrealizedUsd,
    decimal TotalPlUsd,
    decimal? ReturnPercent,
    decimal? CurrentFxRate,
    decimal? CurrentValueThb,
    decimal? UnrealizedThb,
    decimal? TotalPlThb,
    decimal? StockEffectThb,
    decimal? FxEffectThb,
    bool IsComplete,
    IReadOnlyList<string> MissingReasons);
