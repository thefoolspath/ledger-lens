# Performance Budget

Status: Proposed targets; validate against the owner's representative local machine.

| Operation | Dataset/profile | Initial budget |
| --- | --- | --- |
| Local stack ready | warm images/dependencies | p95 <= 30 seconds |
| Dashboard API | 10 years, 25k entries, 100 instruments | p95 <= 500 ms warm |
| Holding/lot detail | 5k entries for one instrument | p95 <= 300 ms warm |
| Full ledger rebuild | 100k entries | <= 10 seconds |
| Import 1k normalized entries | validated batch | <= 5 seconds |
| On-demand quote/FX | excluding provider latency | local overhead <= 200 ms |
| Daily candle ingestion | 100 symbols x 10 years | <= 60 seconds after download |
| Angular simulation charts | ECharts is loaded only when a populated chart is rendered | Initial production bundle remains within 500 kB warning budget; component-style warning budget is 6 kB for the expanded responsive workspace |
| Slip extraction | one two-page document, CPU | p95 <= 10 seconds after model warm-up |
| Angular interaction | local data already loaded | p95 <= 100 ms input response |

Memory, disk growth, database size, document size, and OCR model size must also be measured. These are budgets for investigation, not promises; update with hardware, fixture, run count, warm/cold state, and percentile evidence.
