# Dime Slip Import

Status: Needs Evidence until representative local-only samples are evaluated.

```text
Uploaded -> Stored -> Text Extracted/OCR -> Parsed -> Validated
         -> Needs Review/Ready -> Confirmed -> Posted -> Recalculated
```

Additional terminal or exceptional states: Duplicate, Failed, Unsupported, Rejected, and Manually Corrected.

## Layered extraction

1. Validate media type, size, page count, and document structure.
2. Store original outside the repository and compute SHA-256.
3. Prefer embedded PDF text.
4. Render/OCR only pages that lack usable text.
5. Apply a versioned Dime template parser using anchors and strict field formats.
6. Validate arithmetic, dates, symbol, side, currency, and required fields.
7. Require human confirmation before posting.

No model may invent unreadable values. Missing and low-confidence fields remain explicit. Local vision is deferred and cannot become a silent fallback.

## Proposed extraction contract

`SlipExtraction` contains document/reference number, trade/settlement time, side, symbol/name/market, quantity, unit price, gross, fees, tax, net, currency, displayed FX, parser/version, raw-text artifact identity, and per-field value/confidence/evidence coordinates.

## Duplicate policy

Exact content hash is a hard duplicate candidate. Broker reference within account is a semantic duplicate candidate. Similar amounts/dates are warnings only. Posting uses an idempotency key tied to the reviewed extraction.

See [Dime evaluation](../research/DIME_SLIP_EXTRACTION_EVALUATION.md).
