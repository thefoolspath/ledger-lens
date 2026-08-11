# Benchmark Plan

Status: Proposed.

## Method

Use deterministic synthetic generators with checked seeds. Record CPU, RAM, storage, OS, Docker, runtime/package versions, dataset shape, warm-up, run count, median, p95, allocation/memory, and raw result artifact location. Run correctness assertions before measuring.

## Suites

1. Ledger append, correction, lot allocation, full rebuild, and snapshot comparison.
2. Dashboard and holdings queries under cold/warm cache.
3. Market candle normalization/upsert and provider-cache lookup.
4. Backup creation, checksum, clean restore, and projection rebuild.
5. Embedded PDF text versus Tesseract/PaddleOCR by layout and degradation class.
6. Aspire cold/warm startup and resource readiness.

## Decision gates

- Split a worker/process only when in-process measurements breach responsiveness or isolation budgets.
- Add an index only with query-plan evidence.
- Select OCR by required-field accuracy first, then latency/resource cost.
- Do not cache or retain provider payloads to improve performance unless terms allow it.

Benchmark output containing private data is prohibited and remains outside the repository unless fully synthetic.
