import { LedgerLensApiRoutes } from './api-routes';

describe('LedgerLensApiRoutes', () => {
  it('maps every OperationKey to its canonical Gateway route', () => {
    expect(LedgerLensApiRoutes.PortfoliosCreate).toBe('/api/portfolio/v1/portfolios/create');
    expect(LedgerLensApiRoutes.PortfoliosGetList).toBe('/api/portfolio/v1/portfolios/get-list');
    expect(LedgerLensApiRoutes.PortfoliosGetOne('portfolio-id')).toBe('/api/portfolio/v1/portfolios/get-one/portfolio-id');
    expect(LedgerLensApiRoutes.InvestmentAccountsCreate).toBe('/api/portfolio/v1/investment-accounts/create');
    expect(LedgerLensApiRoutes.CashLedgerEntriesCreate).toBe('/api/portfolio/v1/cash-ledger-entries/create');
    expect(LedgerLensApiRoutes.CashLedgerEntriesCorrect('entry-id')).toBe('/api/portfolio/v1/cash-ledger-entries/correct/entry-id');
    expect(LedgerLensApiRoutes.SimulationAccountsCreate).toBe('/api/portfolio/v1/simulation-accounts/create');
    expect(LedgerLensApiRoutes.SimulationAccountsGetList('portfolio-id')).toBe('/api/portfolio/v1/simulation-accounts/get-list?portfolioId=portfolio-id');
    expect(LedgerLensApiRoutes.SimulationAccountsGetOne('account-id')).toBe('/api/portfolio/v1/simulation-accounts/get-one/account-id');
    expect(LedgerLensApiRoutes.SimulationTradeDraftsCreate).toBe('/api/portfolio/v1/simulation-trade-drafts/create');
    expect(LedgerLensApiRoutes.SimulationTradeDraftsConfirm('draft-id')).toBe('/api/portfolio/v1/simulation-trade-drafts/confirm/draft-id');
    expect(LedgerLensApiRoutes.SimulationTradesCorrect('trade-id')).toBe('/api/portfolio/v1/simulation-trades/correct/trade-id');
    expect(LedgerLensApiRoutes.SimulationValuationsRecordList).toBe('/api/portfolio/v1/simulation-valuations/record-list');
    expect(LedgerLensApiRoutes.SimulationValuationsCalculateSeriesList).toBe('/api/portfolio/v1/simulation-valuations/calculate-series-list');
    expect(LedgerLensApiRoutes.InstrumentsSearchList('NVDA')).toBe('/api/market/v1/instruments/search-list?query=NVDA&market=US');
    expect(LedgerLensApiRoutes.QuotesGetLatestList).toBe('/api/market/v1/quotes/get-latest-list');
    expect(LedgerLensApiRoutes.InstrumentCandlesGetOne('instrument-id', '2026-01-01', '2026-01-31'))
      .toBe('/api/market/v1/instrument-candles/get-one/instrument-id?interval=1day&from=2026-01-01&to=2026-01-31');
    expect(LedgerLensApiRoutes.ForeignExchangeRatesGetLatestList)
      .toBe('/api/market/v1/foreign-exchange-rates/get-latest-list');
  });
});
