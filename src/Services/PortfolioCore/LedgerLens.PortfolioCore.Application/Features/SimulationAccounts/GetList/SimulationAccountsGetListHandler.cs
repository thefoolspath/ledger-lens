using LedgerLens.PortfolioCore.Domain;

using LedgerLens.Application.Abstractions;

namespace LedgerLens.PortfolioCore.Application.Features.SimulationAccounts.GetList;

public sealed class SimulationAccountsGetListHandler(ICurrentUser currentUser, ISimulationStore store)
    : IQueryHandler<SimulationAccountsGetListQuery, IReadOnlyList<SimulationAccountListItem>>
{
    public Task<IReadOnlyList<SimulationAccountListItem>> HandleAsync(
        SimulationAccountsGetListQuery query,
        CancellationToken cancellationToken) =>
        store.SimulationAccountsGetListAsync(query.PortfolioId, currentUser.UserId, cancellationToken);
}
