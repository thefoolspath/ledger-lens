# ADR-0009: Dime Slip Extraction

## Status

Needs Evidence.

## Context

No representative Dime samples are available in the repository, and real samples must remain private.

## Decision

Propose embedded-text extraction first, then Tesseract 5 and PaddleOCR 3.x/PP-OCRv5 comparison, a versioned Dime template parser, arithmetic validation, per-field confidence/evidence, and mandatory human confirmation. No local vision model in the initial pipeline.

## Alternatives considered

OCR-only, cloud OCR, local vision first, and manual-only import.

## Evidence

[Dime extraction evaluation](../research/DIME_SLIP_EXTRACTION_EVALUATION.md) and [slip architecture](../architecture/SLIP_IMPORT.md).

## Consequences

The design is private and auditable; maintaining template variants and native/Python dependencies may be costly.

## Risks

Unknown layouts, Thai OCR accuracy, malformed documents, and false confidence.

## Revisit conditions

Decide only after private sample inspection and synthetic benchmark acceptance.
