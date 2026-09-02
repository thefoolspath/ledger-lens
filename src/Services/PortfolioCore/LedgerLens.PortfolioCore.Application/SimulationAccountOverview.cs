using LedgerLens.PortfolioCore.Domain;

namespace LedgerLens.PortfolioCore.Application;

public sealed record SimulationAccountOverview(SimulationAccountListItem Account,
    IReadOnlyList<SimulationPositionView> Positions, IReadOnlyList<SimulationTradeView> Trades);
