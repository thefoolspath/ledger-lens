# ADR-0018: Apache ECharts for Financial Visualization

## Status

Accepted by owner direction on 2026-09-01, subject to exact-package audit at installation.

## Context

Manual simulations need a responsive value-versus-cost view and daily OHLCV candlesticks with zoom, tooltips, and transaction markers. The dependency must be free for the intended local/open-source use and must not become authoritative for financial calculations.

## Decision

Use the directly imported `echarts` package, initially pinned to 6.1.0, without an Angular wrapper. Import only required renderers, charts, and components; manage lifecycle with `ResizeObserver` and explicit disposal. Enable ECharts ARIA and provide a semantic summary and tabular fallback. All canonical values are calculated by Portfolio Core with `decimal`; charts receive presentation projections only.

## Alternatives considered

TradingView Lightweight Charts, Chart.js plus financial extensions, handwritten SVG/canvas, and server-rendered images.

## Evidence

Apache ECharts documents line, candlestick, zoom, tooltip, and accessibility support and distributes the project under Apache-2.0. Lightweight Charts is finance-focused but requires TradingView attribution. Exact package contents and transitive dependencies are rechecked during admission.

## Consequences

One library covers both required views and future decomposition charts without a wrapper dependency. The application owns Angular integration, chart cleanup, responsive behavior, accessible alternatives, and upgrade testing.

## Revisit conditions

Revisit if bundle, accessibility, maintenance, or security measurements fail the project budgets.
