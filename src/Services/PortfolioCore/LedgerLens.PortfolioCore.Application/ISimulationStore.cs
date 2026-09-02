using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public interface ISimulationStore
{
    Task SimulationAccountsCreateAsync(SimulationAccount account, CancellationToken cancellationToken);
    Task<SimulationAccount?> SimulationAccountsGetOneAsync(Guid accountId, Guid ownerUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SimulationAccountListItem>> SimulationAccountsGetListAsync(Guid portfolioId, Guid ownerUserId, CancellationToken cancellationToken);
    Task SimulationTradeDraftsCreateAsync(SimulationTradeDraft draft, CancellationToken cancellationToken);
    Task<SimulationTradeDraft?> FindOwnedSimulationDraftAsync(Guid draftId, Guid ownerUserId, CancellationToken cancellationToken);
    Task<SimulationTradeEntry?> SimulationTradeDraftsConfirmAsync(SimulationTradeDraft draft, SimulationTradeEntry trade, Guid ownerUserId, CancellationToken cancellationToken);
    Task<SimulationTradeEntry?> FindOwnedSimulationTradeAsync(Guid tradeId, Guid ownerUserId, CancellationToken cancellationToken);
    Task<bool> SimulationTradesCorrectAsync(SimulationTradeEntry corrected, SimulationTradeEntry reversal,
        SimulationTradeEntry replacement, Guid replacementDraftId, Guid ownerUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SimulationTradeEntry>> GetSimulationEntriesAsync(Guid accountId, Guid ownerUserId, CancellationToken cancellationToken);
    Task<IReadOnlyList<SimulationTradeEntry>> SimulationValuationsCalculateSeriesListAsync(Guid accountId, Guid ownerUserId,
        CancellationToken cancellationToken);
    Task SimulationValuationsRecordListAsync(IReadOnlyList<SimulationValuationSnapshotData> snapshots, CancellationToken cancellationToken);
}
