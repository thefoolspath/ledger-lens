export const LedgerLensApiRoutes = {
  PortfoliosCreate: '/api/portfolio/v1/portfolios/create',
  PortfoliosGetList: '/api/portfolio/v1/portfolios/get-list',
  PortfoliosGetOne: (portfolioId: string) => `/api/portfolio/v1/portfolios/get-one/${portfolioId}`,
  InvestmentAccountsCreate: '/api/portfolio/v1/investment-accounts/create',
  CashLedgerEntriesCreate: '/api/portfolio/v1/cash-ledger-entries/create',
  CashLedgerEntriesCorrect: (entryId: string) => `/api/portfolio/v1/cash-ledger-entries/correct/${entryId}`,
  SimulationAccountsCreate: '/api/portfolio/v1/simulation-accounts/create',
  SimulationAccountsGetList: (portfolioId: string) => `/api/portfolio/v1/simulation-accounts/get-list?portfolioId=${portfolioId}`,
  SimulationAccountsGetOne: (accountId: string) => `/api/portfolio/v1/simulation-accounts/get-one/${accountId}`,
  SimulationTradeDraftsCreate: '/api/portfolio/v1/simulation-trade-drafts/create',
  SimulationTradeDraftsConfirm: (draftId: string) => `/api/portfolio/v1/simulation-trade-drafts/confirm/${draftId}`,
  SimulationTradesCorrect: (tradeId: string) => `/api/portfolio/v1/simulation-trades/correct/${tradeId}`,
  SimulationValuationsRecordList: '/api/portfolio/v1/simulation-valuations/record-list',
  SimulationValuationsCalculateSeriesList: '/api/portfolio/v1/simulation-valuations/calculate-series-list',
  InstrumentsSearchList: (query: string) => `/api/market/v1/instruments/search-list?query=${encodeURIComponent(query)}&market=US`,
  QuotesGetLatestList: '/api/market/v1/quotes/get-latest-list',
  InstrumentCandlesGetOne: (instrumentId: string, from: string, to: string) =>
    `/api/market/v1/instrument-candles/get-one/${instrumentId}?interval=1day&from=${from}&to=${to}`,
  ForeignExchangeRatesGetLatestList: '/api/market/v1/foreign-exchange-rates/get-latest-list'
} as const;
