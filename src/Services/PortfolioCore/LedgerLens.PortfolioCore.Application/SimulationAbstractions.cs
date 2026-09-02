using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public interface ISimulationStore
{
    Task AddSimulationAccountAsync(SimulationAccount account, CancellationToken cancellationToken);
    Task<SimulationAccount?> FindOwnedSimulationAccountAsync(Guid accountId, Guid ownerUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SimulationAccountListItem>> ListSimulationAccountsAsync(Guid portfolioId, Guid ownerUserId, CancellationToken cancellationToken);
    Task AddSimulationDraftAsync(SimulationTradeDraft draft, CancellationToken cancellationToken);
    Task<SimulationTradeDraft?> FindOwnedSimulationDraftAsync(Guid draftId, Guid ownerUserId, CancellationToken cancellationToken);
    Task<SimulationTradeEntry?> ConfirmSimulationDraftAsync(SimulationTradeDraft draft, SimulationTradeEntry trade, Guid ownerUserId, CancellationToken cancellationToken);
    Task<SimulationTradeEntry?> FindOwnedSimulationTradeAsync(Guid tradeId, Guid ownerUserId, CancellationToken cancellationToken);
    Task<bool> CorrectSimulationTradeAsync(SimulationTradeEntry corrected, SimulationTradeEntry reversal,
        SimulationTradeEntry replacement, Guid replacementDraftId, Guid ownerUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SimulationTradeEntry>> GetSimulationEntriesAsync(Guid accountId, Guid ownerUserId, CancellationToken cancellationToken);
    Task AddSimulationValuationSnapshotsAsync(IReadOnlyList<SimulationValuationSnapshotData> snapshots, CancellationToken cancellationToken);
}

public sealed record SimulationAccountListItem(Guid Id, Guid PortfolioId, string Name, DateTimeOffset CreatedAt);
public sealed record SimulationTradeView(Guid Id, string Side, decimal Quantity, decimal UnitPrice, decimal GrossAmount,
    decimal AssumedFee, decimal AssumedTax, decimal? FxRate, decimal NetCash, DateTimeOffset EffectiveAt,
    DateTimeOffset RecordedAt, string Symbol, string Provider, string Feed, DateTimeOffset PriceAsOf,
    string Freshness, string Role, Guid? CorrectsEntryId, string? CorrectionReason);
public sealed record SimulationDraftView(Guid Id, Guid SimulationAccountId, string Side, string InputMode,
    decimal? RequestedQuantity, decimal? RequestedAmount, decimal Quantity, decimal GrossAmount, decimal UnusedAmount,
    decimal AssumedFee, decimal AssumedTax, decimal NetCash, decimal? FxRate, DateTimeOffset EffectiveAt,
    DateTimeOffset ExpiresAt, SimulationMarketEvidence Quote, Guid? ConfirmedTradeId);
public sealed record SimulationPositionView(string Symbol, decimal Quantity, decimal RemainingCostUsd,
    decimal? AverageCostUsd, decimal RealizedUsd, decimal InvestedCapitalUsd, decimal? RemainingCostThb, decimal? RealizedThb);
public sealed record SimulationAccountOverview(SimulationAccountListItem Account,
    IReadOnlyList<SimulationPositionView> Positions, IReadOnlyList<SimulationTradeView> Trades);
public sealed record SimulationValuationView(string Symbol, decimal Quantity, decimal RemainingCostUsd, decimal? AverageCostUsd,
    decimal RealizedUsd, decimal InvestedCapitalUsd, decimal? CurrentPrice, decimal? CurrentValueUsd, decimal? UnrealizedUsd,
    decimal? TotalPlUsd, decimal? ReturnPercent,
    decimal? CurrentFxRate, decimal? CurrentValueThb, decimal? UnrealizedThb, decimal? TotalPlThb,
    decimal? StockEffectThb, decimal? FxEffectThb, bool IsComplete, IReadOnlyList<string> MissingReasons,
    SimulationMarketEvidence? Quote);
public sealed record SimulationValuationSnapshotData(Guid Id, Guid SimulationAccountId, SimulationValuationView Value,
    DateTimeOffset RecordedAt);
public sealed record SimulationSeriesInput(DateOnly Date, decimal Price, decimal? FxRate);
public sealed record SimulationSeriesPoint(DateOnly Date, string Symbol, decimal Quantity, decimal CostBasisUsd,
    decimal MarketValueUsd, decimal UnrealizedUsd, decimal? MarketValueThb, bool IsComplete);
