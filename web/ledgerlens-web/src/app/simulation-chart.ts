import { AfterViewInit, Component, ElementRef, Input, OnChanges, OnDestroy, ViewChild } from '@angular/core';
import type { EChartsCoreOption, EChartsType } from 'echarts/core';

export interface SimulationSeriesPoint {
  readonly date: string;
  readonly symbol: string;
  readonly quantity: number;
  readonly costBasisUsd: number;
  readonly marketValueUsd: number;
  readonly unrealizedUsd: number;
  readonly marketValueThb?: number;
  readonly isComplete: boolean;
}

export interface MarketCandle {
  readonly date: string;
  readonly open: number;
  readonly high: number;
  readonly low: number;
  readonly close: number;
  readonly volume: number;
}

export interface TradeMarker {
  readonly side: string;
  readonly effectiveAt: string;
  readonly unitPrice: number;
}

@Component({
  selector: 'app-simulation-chart',
  template: `<div #chart class="chart-canvas" role="img" [attr.aria-label]="description"></div>`,
  styles: [`.chart-canvas { width: 100%; height: 22rem; }`]
})
export class SimulationChart implements AfterViewInit, OnChanges, OnDestroy {
  @ViewChild('chart') private chartElement?: ElementRef<HTMLDivElement>;
  @Input() public kind: 'value' | 'price' = 'value';
  @Input() public points: readonly SimulationSeriesPoint[] = [];
  @Input() public candles: readonly MarketCandle[] = [];
  @Input() public trades: readonly TradeMarker[] = [];
  @Input() public description = 'Simulation chart';

  private chart?: EChartsType;
  private resizeObserver?: ResizeObserver;
  private destroyed = false;

  public ngAfterViewInit(): void {
    this.render();
  }

  public ngOnChanges(): void {
    this.render();
  }

  public ngOnDestroy(): void {
    this.destroyed = true;
    this.resizeObserver?.disconnect();
    this.chart?.dispose();
  }

  private async render(): Promise<void> {
    const element = this.chartElement?.nativeElement;
    if (!element || (this.kind === 'value' ? this.points.length === 0 : this.candles.length === 0)) return;
    const [charts, components, core, renderers] = await Promise.all([
      import('echarts/charts'), import('echarts/components'), import('echarts/core'), import('echarts/renderers')
    ]);
    if (this.destroyed || !element.isConnected) return;
    core.use([charts.LineChart, charts.CandlestickChart, charts.BarChart, components.AriaComponent,
      components.DataZoomComponent, components.GridComponent, components.LegendComponent,
      components.MarkPointComponent, components.TooltipComponent, renderers.CanvasRenderer]);
    this.chart ??= core.init(element, undefined, { renderer: 'canvas' });
    if (!this.resizeObserver && typeof ResizeObserver !== 'undefined') {
      this.resizeObserver = new ResizeObserver(() => this.chart?.resize());
      this.resizeObserver.observe(element);
    }
    this.chart.setOption(this.kind === 'value' ? this.valueOptions() : this.priceOptions(), true);
  }

  private valueOptions(): EChartsCoreOption {
    return {
      aria: { enabled: true, description: this.description },
      backgroundColor: 'transparent',
      textStyle: { color: '#c9d7e8' },
      tooltip: { trigger: 'axis' },
      legend: { data: ['Market value', 'Cost basis'], textStyle: { color: '#c9d7e8' } },
      grid: { left: 62, right: 24, top: 48, bottom: 64 },
      dataZoom: [{ type: 'inside' }, { type: 'slider', bottom: 14 }],
      xAxis: { type: 'category', data: this.points.map(point => point.date), axisLabel: { color: '#9fb1c8' } },
      yAxis: { type: 'value', name: 'USD', axisLabel: { color: '#9fb1c8' }, splitLine: { lineStyle: { color: '#1b3046' } } },
      series: [
        { name: 'Market value', type: 'line', smooth: true, showSymbol: false, areaStyle: { opacity: 0.12 }, data: this.points.map(point => point.marketValueUsd) },
        { name: 'Cost basis', type: 'line', showSymbol: false, lineStyle: { type: 'dashed' }, data: this.points.map(point => point.costBasisUsd) }
      ]
    };
  }

  private priceOptions(): EChartsCoreOption {
    const markers = this.trades.map(trade => ({
      name: trade.side,
      coord: [trade.effectiveAt.slice(0, 10), trade.unitPrice],
      value: trade.side,
      itemStyle: { color: trade.side === 'Buy' ? '#54d3a8' : '#f27676' }
    }));
    return {
      aria: { enabled: true, description: this.description },
      backgroundColor: 'transparent',
      textStyle: { color: '#c9d7e8' },
      tooltip: { trigger: 'axis' },
      grid: [{ left: 62, right: 24, top: 28, height: '58%' }, { left: 62, right: 24, top: '74%', height: '12%' }],
      dataZoom: [{ type: 'inside', xAxisIndex: [0, 1] }, { type: 'slider', xAxisIndex: [0, 1], bottom: 8 }],
      xAxis: [
        { type: 'category', data: this.candles.map(candle => candle.date), axisLabel: { color: '#9fb1c8' } },
        { type: 'category', gridIndex: 1, data: this.candles.map(candle => candle.date), axisLabel: { show: false } }
      ],
      yAxis: [
        { scale: true, axisLabel: { color: '#9fb1c8' }, splitLine: { lineStyle: { color: '#1b3046' } } },
        { gridIndex: 1, axisLabel: { color: '#9fb1c8' }, splitLine: { show: false } }
      ],
      series: [
        {
          name: 'OHLC', type: 'candlestick',
          data: this.candles.map(candle => [candle.open, candle.close, candle.low, candle.high]),
          itemStyle: { color: '#54d3a8', color0: '#f27676', borderColor: '#54d3a8', borderColor0: '#f27676' },
          markPoint: { data: markers }
        },
        { name: 'Volume', type: 'bar', xAxisIndex: 1, yAxisIndex: 1, data: this.candles.map(candle => candle.volume), itemStyle: { color: '#4f7ca8' } }
      ]
    };
  }
}
