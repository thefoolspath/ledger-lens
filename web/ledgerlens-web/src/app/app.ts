import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';
import { MarketCandle, SimulationChart, SimulationSeriesPoint } from './simulation-chart';

type ConnectivityState = 'checking' | 'connected' | 'unavailable';

interface ServiceStatus {
  readonly name: string;
  readonly endpoint: string;
  readonly state: ConnectivityState;
}

interface PortfolioListItem {
  readonly id: string;
  readonly name: string;
  readonly baseCurrency: string;
  readonly reportingCurrency: string;
}

interface LedgerEntry {
  readonly id: string;
  readonly type: string;
  readonly role: string;
  readonly amount: number;
  readonly signedAmount: number;
  readonly currency: string;
  readonly effectiveAt: string;
  readonly note?: string;
  readonly instrumentSymbol?: string;
  readonly quantity?: number;
  readonly signedQuantity?: number;
  readonly unitPrice?: number;
  readonly correctsEntryId?: string;
  readonly correctionReason?: string;
}

interface AccountOverview {
  readonly id: string;
  readonly name: string;
  readonly broker: string;
  readonly currency: string;
  readonly cashBalance: number;
  readonly entries: readonly LedgerEntry[];
}

interface PortfolioOverview extends PortfolioListItem {
  readonly accounts: readonly AccountOverview[];
}

interface MarketInstrument {
  readonly id: string;
  readonly symbol: string;
  readonly name: string;
  readonly exchange: string;
  readonly mic: string;
  readonly currency: string;
}

interface Provenance {
  readonly provider: string;
  readonly feed: string;
  readonly asOf: string;
  readonly retrievedAt: string;
  readonly freshness: string;
  readonly delaySeconds?: number;
  readonly requestId: string;
  readonly retentionPolicyKey: string;
}

interface MarketQuote {
  readonly instrument: MarketInstrument;
  readonly price: number;
  readonly priceKind: string;
  readonly provenance: Provenance;
}

interface FxSnapshot {
  readonly baseCurrency: string;
  readonly quoteCurrency: string;
  readonly rate: number;
  readonly provenance: Provenance;
  readonly isBenchmark: boolean;
}

interface CandleSeries {
  readonly instrument: MarketInstrument;
  readonly interval: string;
  readonly candles: readonly MarketCandle[];
  readonly provenance: Provenance;
}

interface SimulationAccount {
  readonly id: string;
  readonly portfolioId: string;
  readonly name: string;
  readonly createdAt: string;
}

interface SimulationTrade {
  readonly id: string;
  readonly side: string;
  readonly quantity: number;
  readonly unitPrice: number;
  readonly grossAmount: number;
  readonly assumedFee: number;
  readonly assumedTax: number;
  readonly fxRate?: number;
  readonly netCash: number;
  readonly effectiveAt: string;
  readonly symbol: string;
  readonly provider: string;
  readonly feed: string;
  readonly priceAsOf: string;
  readonly freshness: string;
  readonly role: string;
  readonly correctsEntryId?: string;
  readonly correctionReason?: string;
}

interface SimulationPosition {
  readonly symbol: string;
  readonly quantity: number;
  readonly remainingCostUsd: number;
  readonly averageCostUsd?: number;
  readonly realizedUsd: number;
  readonly investedCapitalUsd: number;
}

interface SimulationOverview {
  readonly account: SimulationAccount;
  readonly positions: readonly SimulationPosition[];
  readonly trades: readonly SimulationTrade[];
}

interface SimulationDraft {
  readonly id: string;
  readonly side: string;
  readonly inputMode: string;
  readonly quantity: number;
  readonly grossAmount: number;
  readonly unusedAmount: number;
  readonly assumedFee: number;
  readonly assumedTax: number;
  readonly netCash: number;
  readonly expiresAt: string;
}

interface SimulationValuation {
  readonly symbol: string;
  readonly quantity: number;
  readonly remainingCostUsd: number;
  readonly averageCostUsd?: number;
  readonly realizedUsd: number;
  readonly investedCapitalUsd: number;
  readonly currentPrice?: number;
  readonly currentValueUsd?: number;
  readonly unrealizedUsd?: number;
  readonly totalPlUsd?: number;
  readonly returnPercent?: number;
  readonly currentFxRate?: number;
  readonly currentValueThb?: number;
  readonly unrealizedThb?: number;
  readonly totalPlThb?: number;
  readonly stockEffectThb?: number;
  readonly fxEffectThb?: number;
  readonly isComplete: boolean;
  readonly missingReasons: readonly string[];
}

interface SimulationMarketEvidence {
  readonly marketInstrumentId: string;
  readonly symbol: string;
  readonly exchange: string;
  readonly mic: string;
  readonly currency: string;
  readonly price: number;
  readonly priceKind: string;
  readonly provider: string;
  readonly feed: string;
  readonly asOf: string;
  readonly retrievedAt: string;
  readonly freshness: string;
  readonly delaySeconds?: number;
  readonly requestId: string;
  readonly retentionPolicyKey: string;
}

@Component({
  selector: 'app-root',
  imports: [FormsModule, SimulationChart],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  private readonly http = inject(HttpClient);
  private readonly mutationHeaders = new HttpHeaders({ 'X-LedgerLens-Request': '1' });
  protected readonly services = signal<readonly ServiceStatus[]>([
    { name: 'Portfolio Core', endpoint: '/api/portfolio/internal/ping', state: 'checking' },
    { name: 'Market Data', endpoint: '/api/market/internal/ping', state: 'checking' },
    { name: 'Slip Import', endpoint: '/api/slips/internal/ping', state: 'checking' },
    { name: 'Research', endpoint: '/api/research/internal/ping', state: 'checking' },
    { name: 'Operations', endpoint: '/api/operations/internal/ping', state: 'checking' }
  ]);
  protected readonly portfolios = signal<readonly PortfolioListItem[]>([]);
  protected readonly selectedPortfolio = signal<PortfolioOverview | null>(null);
  protected readonly busy = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly mode = signal<'real' | 'simulation'>('real');
  protected readonly simulationAccounts = signal<readonly SimulationAccount[]>([]);
  protected readonly selectedSimulation = signal<SimulationOverview | null>(null);
  protected readonly instrumentResults = signal<readonly MarketInstrument[]>([]);
  protected readonly selectedInstrument = signal<MarketInstrument | null>(null);
  protected readonly latestQuote = signal<MarketQuote | null>(null);
  protected readonly fxRates = signal<readonly FxSnapshot[]>([]);
  protected readonly candles = signal<readonly MarketCandle[]>([]);
  protected readonly valuations = signal<readonly SimulationValuation[]>([]);
  protected readonly seriesPoints = signal<readonly SimulationSeriesPoint[]>([]);
  protected readonly pendingDraft = signal<SimulationDraft | null>(null);

  protected portfolioName = '';
  protected accountName = '';
  protected broker = '';
  protected accountCurrency = 'USD';
  protected entryType = 'Deposit';
  protected entryAmount: number | null = null;
  protected entryNote = '';
  protected instrumentSymbol = '';
  protected quantity: number | null = null;
  protected unitPrice: number | null = null;
  protected correctionTarget: LedgerEntry | null = null;
  protected correctionReason = '';
  protected simulationAccountName = '';
  protected instrumentQuery = 'NVDA';
  protected simulationSide = 'Buy';
  protected simulationInputMode = 'ByAmount';
  protected simulationAmount: number | null = 1000;
  protected simulationQuantity: number | null = null;
  protected simulationFee = 0;
  protected simulationTax = 0;
  protected simulationCorrectionTarget: SimulationTrade | null = null;
  protected simulationCorrectionReason = '';

  public constructor() {
    this.refreshConnectivity();
    this.loadPortfolios();
  }

  protected refreshConnectivity(): void {
    this.services.update(services => services.map(service => ({ ...service, state: 'checking' })));
    for (const service of this.services()) {
      this.http.get(service.endpoint).subscribe({
        next: () => this.setState(service.endpoint, 'connected'),
        error: () => this.setState(service.endpoint, 'unavailable')
      });
    }
  }

  protected switchMode(mode: 'real' | 'simulation'): void {
    this.mode.set(mode);
    this.error.set(null);
    const portfolio = this.selectedPortfolio();
    if (mode === 'simulation' && portfolio) this.loadSimulationAccounts(portfolio.id);
  }

  protected createSimulationAccount(): void {
    const portfolio = this.selectedPortfolio();
    if (!portfolio || !this.simulationAccountName.trim()) return;
    this.startRequest();
    this.http.post<SimulationAccount>(`/api/portfolio/v1/portfolios/${portfolio.id}/simulation-accounts`,
      { name: this.simulationAccountName }, { headers: this.mutationHeaders }).subscribe({
      next: account => {
        this.simulationAccountName = '';
        this.simulationAccounts.update(items => [...items, account]);
        this.selectSimulationAccount(account.id);
      },
      error: () => this.failRequest('Could not create the simulation account.')
    });
  }

  protected selectSimulationAccount(id: string): void {
    this.startRequest();
    this.http.get<SimulationOverview>(`/api/portfolio/v1/simulation-accounts/${id}`).subscribe({
      next: overview => {
        this.selectedSimulation.set(overview);
        this.pendingDraft.set(null);
        this.finishRequest();
        if (this.latestQuote()) this.refreshSimulationOutputs();
      },
      error: () => this.failRequest('Could not load the simulation account.')
    });
  }

  protected searchInstruments(): void {
    if (!this.instrumentQuery.trim()) return;
    this.startRequest();
    this.http.get<readonly MarketInstrument[]>(`/api/market/v1/instruments/search?query=${encodeURIComponent(this.instrumentQuery)}&market=US`).subscribe({
      next: instruments => {
        this.instrumentResults.set(instruments);
        this.finishRequest();
      },
      error: () => this.failRequest('Market-data search is unavailable. Check the configured provider.')
    });
  }

  protected chooseInstrument(instrument: MarketInstrument): void {
    this.selectedInstrument.set(instrument);
    this.instrumentResults.set([]);
    this.fetchMarketData();
  }

  protected fetchMarketData(): void {
    const instrument = this.selectedInstrument();
    if (!instrument) return;
    this.startRequest();
    const today = new Date();
    const from = new Date(today);
    from.setUTCFullYear(from.getUTCFullYear() - 1);
    forkJoin({
      quotes: this.http.post<readonly MarketQuote[]>('/api/market/v1/quotes/latest', { instruments: [instrument] }),
      fx: this.http.post<readonly FxSnapshot[]>('/api/market/v1/fx/latest', { baseCurrency: 'USD', quoteCurrency: 'THB' }),
      candles: this.http.get<CandleSeries>(`/api/market/v1/instruments/${instrument.id}/candles?interval=1day&from=${this.isoDate(from)}&to=${this.isoDate(today)}`)
    }).subscribe({
      next: result => {
        this.latestQuote.set(result.quotes[0] ?? null);
        this.fxRates.set(result.fx);
        this.candles.set(result.candles.candles);
        this.finishRequest();
        this.refreshSimulationOutputs();
      },
      error: () => this.failRequest('Could not refresh quote, candles, or USD/THB FX.')
    });
  }

  protected previewSimulationTrade(): void {
    const simulation = this.selectedSimulation();
    const quote = this.latestQuote();
    if (!simulation || !quote) return;
    const byAmount = this.simulationInputMode === 'ByAmount';
    if ((byAmount && (!this.simulationAmount || this.simulationAmount <= 0)) ||
        (!byAmount && (!this.simulationQuantity || this.simulationQuantity <= 0))) return;
    this.startRequest();
    this.http.post<SimulationDraft>(`/api/portfolio/v1/simulation-accounts/${simulation.account.id}/trade-drafts`, {
      side: this.simulationSide,
      inputMode: this.simulationInputMode,
      requestedQuantity: byAmount ? null : this.simulationQuantity,
      requestedAmount: byAmount ? this.simulationAmount : null,
      assumedFee: this.simulationFee,
      assumedTax: this.simulationTax,
      fxRate: this.primaryFx()?.rate ?? null,
      effectiveAt: new Date().toISOString(),
      quote: this.toEvidence(quote)
    }, { headers: this.mutationHeaders }).subscribe({
      next: draft => {
        this.pendingDraft.set(draft);
        this.finishRequest();
      },
      error: () => this.failRequest('Could not preview the simulated trade. A sell may exceed the simulated holding.')
    });
  }

  protected confirmSimulationTrade(): void {
    const simulation = this.selectedSimulation();
    const draft = this.pendingDraft();
    if (!simulation || !draft) return;
    this.startRequest();
    const request = this.simulationCorrectionTarget
      ? this.http.post(`/api/portfolio/v1/simulation-trades/${this.simulationCorrectionTarget.id}/corrections`, {
          replacementDraftId: draft.id,
          reason: this.simulationCorrectionReason
        }, { headers: this.mutationHeaders })
      : this.http.post(`/api/portfolio/v1/simulation-accounts/${simulation.account.id}/trade-drafts/${draft.id}/confirm`, {},
          { headers: this.mutationHeaders });
    request.subscribe({
      next: () => {
        this.pendingDraft.set(null);
        this.cancelSimulationCorrection();
        this.selectSimulationAccount(simulation.account.id);
      },
      error: () => this.failRequest('Could not confirm or correct the simulation trade; refresh the quote and try again.')
    });
  }

  protected beginSimulationCorrection(trade: SimulationTrade): void {
    this.simulationCorrectionTarget = trade;
    this.simulationCorrectionReason = '';
    this.simulationSide = trade.side;
    this.simulationInputMode = 'ByQuantity';
    this.simulationQuantity = trade.quantity;
    this.simulationFee = trade.assumedFee;
    this.simulationTax = trade.assumedTax;
    this.pendingDraft.set(null);
  }

  protected cancelSimulationCorrection(): void {
    this.simulationCorrectionTarget = null;
    this.simulationCorrectionReason = '';
  }

  protected canCorrectSimulation(simulation: SimulationOverview, trade: SimulationTrade): boolean {
    return trade.role !== 'Reversal' && !simulation.trades.some(candidate =>
      candidate.role === 'Reversal' && candidate.correctsEntryId === trade.id);
  }

  protected primaryFx(): FxSnapshot | undefined {
    return this.fxRates().find(rate => !rate.isBenchmark);
  }

  protected benchmarkFx(): FxSnapshot | undefined {
    return this.fxRates().find(rate => rate.isBenchmark);
  }

  protected valuationFor(symbol: string): SimulationValuation | undefined {
    return this.valuations().find(value => value.symbol === symbol);
  }

  protected createPortfolio(): void {
    if (!this.portfolioName.trim()) return;
    this.startRequest();
    this.http.post<PortfolioListItem>('/api/portfolio/v1/portfolios', {
      name: this.portfolioName,
      baseCurrency: 'USD',
      reportingCurrency: 'THB'
    }, { headers: this.mutationHeaders }).subscribe({
      next: portfolio => {
        this.portfolioName = '';
        this.portfolios.update(items => [...items, portfolio]);
        this.selectPortfolio(portfolio.id);
      },
      error: () => this.failRequest('Could not create the portfolio. Check fixed-user configuration and try again.')
    });
  }

  protected createAccount(): void {
    const portfolio = this.selectedPortfolio();
    if (!portfolio || !this.accountName.trim() || !this.broker.trim()) return;
    this.startRequest();
    this.http.post(`/api/portfolio/v1/portfolios/${portfolio.id}/accounts`, {
      name: this.accountName,
      broker: this.broker,
      currency: this.accountCurrency
    }, { headers: this.mutationHeaders }).subscribe({
      next: () => {
        this.accountName = '';
        this.broker = '';
        this.selectPortfolio(portfolio.id);
      },
      error: () => this.failRequest('Could not create the investment account.')
    });
  }

  protected recordLedgerEntry(account: AccountOverview): void {
    const amount = this.requestedAmount();
    if (amount === null || amount <= 0) return;
    const portfolioId = this.selectedPortfolio()!.id;
    this.startRequest();
    const body = {
      type: this.entryType,
      amount,
      currency: account.currency,
      effectiveAt: new Date().toISOString(),
      note: this.entryNote || null,
      instrumentSymbol: this.isTrade() ? this.instrumentSymbol : null,
      quantity: this.isTrade() ? this.quantity : null,
      unitPrice: this.isTrade() ? this.unitPrice : null
    };
    const request = this.correctionTarget
      ? this.http.post(`/api/portfolio/v1/ledger-entries/${this.correctionTarget.id}/corrections`,
          { ...body, reason: this.correctionReason }, { headers: this.mutationHeaders })
      : this.http.post(`/api/portfolio/v1/accounts/${account.id}/ledger-entries`, body, { headers: this.mutationHeaders });
    request.subscribe({
      next: () => {
        this.resetEntryForm();
        this.selectPortfolio(portfolioId);
      },
      error: () => this.failRequest('Could not record the ledger entry. Check its values or correction status.')
    });
  }

  protected beginCorrection(entry: LedgerEntry): void {
    this.correctionTarget = entry;
    this.entryType = entry.type;
    this.entryAmount = entry.amount;
    this.entryNote = entry.note ?? '';
    this.instrumentSymbol = entry.instrumentSymbol ?? '';
    this.quantity = entry.quantity ?? null;
    this.unitPrice = entry.unitPrice ?? null;
    this.correctionReason = '';
  }

  protected isTrade(): boolean {
    return this.entryType === 'Buy' || this.entryType === 'Sell';
  }

  protected requestedAmount(): number | null {
    return this.isTrade() && this.quantity !== null && this.unitPrice !== null
      ? this.quantity * this.unitPrice
      : this.entryAmount;
  }

  protected canCorrect(account: AccountOverview, entry: LedgerEntry): boolean {
    return entry.role !== 'Reversal' && !account.entries.some(candidate =>
      candidate.role === 'Reversal' && candidate.correctsEntryId === entry.id);
  }

  protected resetEntryForm(): void {
    this.entryAmount = null;
    this.entryNote = '';
    this.instrumentSymbol = '';
    this.quantity = null;
    this.unitPrice = null;
    this.correctionTarget = null;
    this.correctionReason = '';
  }

  protected selectPortfolio(id: string): void {
    this.startRequest();
    this.http.get<PortfolioOverview>(`/api/portfolio/v1/portfolios/${id}`).subscribe({
      next: portfolio => {
        this.selectedPortfolio.set(portfolio);
        this.finishRequest();
        if (this.mode() === 'simulation') this.loadSimulationAccounts(portfolio.id);
      },
      error: () => this.failRequest('Could not load the selected portfolio.')
    });
  }

  private loadPortfolios(): void {
    this.http.get<readonly PortfolioListItem[]>('/api/portfolio/v1/portfolios').subscribe({
      next: portfolios => {
        this.portfolios.set(portfolios);
        if (portfolios.length > 0) this.selectPortfolio(portfolios[0].id);
      },
      error: () => this.error.set('Portfolio Core is not ready. Verify migration and fixed-user configuration.')
    });
  }

  private loadSimulationAccounts(portfolioId: string): void {
    this.http.get<readonly SimulationAccount[]>(`/api/portfolio/v1/portfolios/${portfolioId}/simulation-accounts`).subscribe({
      next: accounts => {
        this.simulationAccounts.set(accounts);
        if (accounts.length > 0) this.selectSimulationAccount(accounts[0].id);
        else this.selectedSimulation.set(null);
      },
      error: () => this.failRequest('Could not load simulation accounts.')
    });
  }

  private refreshSimulationOutputs(): void {
    const simulation = this.selectedSimulation();
    const quote = this.latestQuote();
    const instrument = this.selectedInstrument();
    if (!simulation || !quote || !instrument || this.candles().length === 0) return;
    forkJoin({
      valuations: this.http.post<readonly SimulationValuation[]>(
        `/api/portfolio/v1/simulation-accounts/${simulation.account.id}/valuations`,
        { quotes: [this.toEvidence(quote)], currentFxRate: this.primaryFx()?.rate ?? null },
        { headers: this.mutationHeaders }),
      series: this.http.post<readonly SimulationSeriesPoint[]>(
        `/api/portfolio/v1/simulation-accounts/${simulation.account.id}/valuation-series`,
        { symbol: instrument.symbol, observations: this.candles().map(candle => ({ date: candle.date, price: candle.close, fxRate: null })) },
        { headers: this.mutationHeaders })
    }).subscribe({
      next: result => {
        this.valuations.set(result.valuations);
        this.seriesPoints.set(result.series);
      },
      error: () => this.error.set('Market data loaded, but simulation valuation could not be calculated.')
    });
  }

  private toEvidence(quote: MarketQuote): SimulationMarketEvidence {
    return {
      marketInstrumentId: quote.instrument.id,
      symbol: quote.instrument.symbol,
      exchange: quote.instrument.exchange,
      mic: quote.instrument.mic,
      currency: quote.instrument.currency,
      price: quote.price,
      priceKind: quote.priceKind,
      provider: quote.provenance.provider,
      feed: quote.provenance.feed,
      asOf: quote.provenance.asOf,
      retrievedAt: quote.provenance.retrievedAt,
      freshness: quote.provenance.freshness,
      delaySeconds: quote.provenance.delaySeconds,
      requestId: quote.provenance.requestId,
      retentionPolicyKey: quote.provenance.retentionPolicyKey
    };
  }

  private isoDate(value: Date): string {
    return value.toISOString().slice(0, 10);
  }

  private startRequest(): void {
    this.busy.set(true);
    this.error.set(null);
  }

  private finishRequest(): void {
    this.busy.set(false);
    this.error.set(null);
  }

  private failRequest(message: string): void {
    this.busy.set(false);
    this.error.set(message);
  }

  private setState(endpoint: string, state: ConnectivityState): void {
    this.services.update(services => services.map(service =>
      service.endpoint === endpoint ? { ...service, state } : service));
  }
}
