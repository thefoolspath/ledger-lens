using LedgerLens.PortfolioCore.Api.Features.CashLedgerEntries.Correct;
using LedgerLens.PortfolioCore.Api.Features.CashLedgerEntries.Create;
using LedgerLens.PortfolioCore.Api.Features.InvestmentAccounts.Create;
using LedgerLens.PortfolioCore.Api.Features.Portfolios.Create;
using LedgerLens.PortfolioCore.Api.Features.Portfolios.GetList;
using LedgerLens.PortfolioCore.Api.Features.Portfolios.GetOne;
using LedgerLens.PortfolioCore.Api.Features.SimulationAccounts.Create;
using LedgerLens.PortfolioCore.Api.Features.SimulationAccounts.GetList;
using LedgerLens.PortfolioCore.Api.Features.SimulationAccounts.GetOne;
using LedgerLens.PortfolioCore.Api.Features.SimulationTradeDrafts.Confirm;
using LedgerLens.PortfolioCore.Api.Features.SimulationTradeDrafts.Create;
using LedgerLens.PortfolioCore.Api.Features.SimulationTrades.Correct;
using LedgerLens.PortfolioCore.Api.Features.SimulationValuations.CalculateSeriesList;
using LedgerLens.PortfolioCore.Api.Features.SimulationValuations.RecordList;

namespace LedgerLens.PortfolioCore.Api;

public static class PortfolioCoreEndpoints
{
    public static IEndpointRouteBuilder MapPortfolioCoreEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/v1");
        group.MapPortfoliosGetList();
        group.MapPortfoliosGetOne();
        group.MapPortfoliosCreate();
        group.MapInvestmentAccountsCreate();
        group.MapCashLedgerEntriesCreate();
        group.MapCashLedgerEntriesCorrect();
        group.MapSimulationAccountsGetList();
        group.MapSimulationAccountsCreate();
        group.MapSimulationAccountsGetOne();
        group.MapSimulationTradeDraftsCreate();
        group.MapSimulationTradeDraftsConfirm();
        group.MapSimulationTradesCorrect();
        group.MapSimulationValuationsRecordList();
        group.MapSimulationValuationsCalculateSeriesList();
        return endpoints;
    }
}
