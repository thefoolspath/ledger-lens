using LedgerLens.Application.Abstractions;
using LedgerLens.PortfolioCore.Application.Features.CashLedgerEntries.Correct;
using LedgerLens.PortfolioCore.Application.Features.CashLedgerEntries.Create;
using LedgerLens.PortfolioCore.Application.Features.InvestmentAccounts.Create;
using LedgerLens.PortfolioCore.Application.Features.Portfolios.Create;
using LedgerLens.PortfolioCore.Application.Features.Portfolios.GetList;
using LedgerLens.PortfolioCore.Application.Features.Portfolios.GetOne;
using LedgerLens.PortfolioCore.Application.Features.SimulationAccounts.Create;
using LedgerLens.PortfolioCore.Application.Features.SimulationAccounts.GetList;
using LedgerLens.PortfolioCore.Application.Features.SimulationAccounts.GetOne;
using LedgerLens.PortfolioCore.Application.Features.SimulationTradeDrafts.Confirm;
using LedgerLens.PortfolioCore.Application.Features.SimulationTradeDrafts.Create;
using LedgerLens.PortfolioCore.Application.Features.SimulationTrades.Correct;
using LedgerLens.PortfolioCore.Application.Features.SimulationValuations.CalculateSeriesList;
using LedgerLens.PortfolioCore.Application.Features.SimulationValuations.RecordList;
using Microsoft.Extensions.DependencyInjection;

namespace LedgerLens.PortfolioCore.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPortfolioCoreApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<ICommandHandler<PortfoliosCreateCommand, PortfolioListItem>, PortfoliosCreateHandler>();
        services.AddScoped<IQueryHandler<PortfoliosGetListQuery, IReadOnlyList<PortfolioListItem>>, PortfoliosGetListHandler>();
        services.AddScoped<IQueryHandler<PortfoliosGetOneQuery, PortfolioOverview?>, PortfoliosGetOneHandler>();
        services.AddScoped<ICommandHandler<InvestmentAccountsCreateCommand, AccountIdentity?>, InvestmentAccountsCreateHandler>();
        services.AddScoped<ICommandHandler<CashLedgerEntriesCreateCommand, LedgerEntryView?>, CashLedgerEntriesCreateHandler>();
        services.AddScoped<ICommandHandler<CashLedgerEntriesCorrectCommand, CorrectionView?>, CashLedgerEntriesCorrectHandler>();
        services.AddScoped<ICommandHandler<SimulationAccountsCreateCommand, SimulationAccountListItem?>, SimulationAccountsCreateHandler>();
        services.AddScoped<IQueryHandler<SimulationAccountsGetListQuery, IReadOnlyList<SimulationAccountListItem>>, SimulationAccountsGetListHandler>();
        services.AddScoped<IQueryHandler<SimulationAccountsGetOneQuery, SimulationAccountOverview?>, SimulationAccountsGetOneHandler>();
        services.AddScoped<ICommandHandler<SimulationTradeDraftsCreateCommand, SimulationDraftView?>, SimulationTradeDraftsCreateHandler>();
        services.AddScoped<ICommandHandler<SimulationTradeDraftsConfirmCommand, SimulationTradeView?>, SimulationTradeDraftsConfirmHandler>();
        services.AddScoped<ICommandHandler<SimulationTradesCorrectCommand, IReadOnlyList<SimulationTradeView>?>, SimulationTradesCorrectHandler>();
        services.AddScoped<ICommandHandler<SimulationValuationsRecordListCommand, IReadOnlyList<SimulationValuationView>?>, SimulationValuationsRecordListHandler>();
        services.AddScoped<IQueryHandler<SimulationValuationsCalculateSeriesListQuery, IReadOnlyList<SimulationSeriesPoint>?>, SimulationValuationsCalculateSeriesListHandler>();
        return services;
    }
}
