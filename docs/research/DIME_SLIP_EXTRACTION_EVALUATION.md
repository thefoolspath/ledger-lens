# Dime Slip Extraction Evaluation

Status: Needs Evidence. Research date: 2026-08-11.

## Private sample observations

Ten owner-supplied raster screenshots were inspected locally on 2026-08-12 and retained on the owner's machine under `%LOCALAPPDATA%\LedgerLens\private-samples\dime-slips\2026-08-13\`. The files and extracted personal or transaction values remain outside the repository; only the following structural observations are recorded:

- At least four document families are present: English transfer, Thai transfer, FX exchange, and asset-order detail.
- Transfer layouts contain amount, fee, sender, recipient, masked accounts, transaction date/time, slip identifier, and a QR verification block.
- FX layouts contain status, source and destination amounts/currencies, account roles, exchange rate, order/submission/settlement dates, order identifier, and purpose. Direction can be either foreign-currency-to-THB or THB-to-foreign-currency.
- Asset-order layouts add instrument, side, local and foreign values, unit price, quantity, fees/tax, order type, payment/receiving accounts, portfolio or order references, and completion state.
- Thai and English labels, Buddhist Era dates in two- and four-digit forms, Latin identifiers, masked account text, long portrait images, decorative headers, faint backgrounds, and completed/pending states must be handled.
- These samples are screenshots without an embedded PDF text layer, so they require OCR or an equivalent raster text-extraction path. Future PDF inputs must still attempt embedded-text extraction first.

These observations expand the template classes but do not select an OCR engine or satisfy the accuracy gate. A sanitized, newly generated synthetic corpus with expected fields is still required before implementation acceptance.

## Facts

- [Tesseract documentation](https://tesseract-ocr.github.io/tessdoc/) identifies Tesseract 5.x as the stable open-source OCR engine under Apache-2.0.
- [PaddleOCR PP-OCRv5 multilingual documentation](https://www.paddleocr.ai/latest/en/version3.x/algorithm/PP-OCRv5/PP-OCRv5_multi_languages.html) includes Thai and English support.
- [PaddleOCR 3.x usage](https://www.paddleocr.ai/main/en/version3.x/pipeline_usage/OCR.html) documents current OCR pipelines and model choices. PP-OCRv6 does not list Thai in its unified 50-language set, so PP-OCRv5 remains a required Thai candidate.

## Recommendation

Use a deterministic layered pipeline: document-family and language classification, embedded PDF text when present, OCR for raster content, a strict versioned Dime anchor/template parser per document family, arithmetic and state validation, then human review. Benchmark Tesseract 5 and PaddleOCR 3.x/PP-OCRv5; do not select by generic benchmark claims.

## Private evaluation protocol

1. Inspect representative Dime files locally without copying them into the repository.
2. Record only structural findings: media type, page count, text layer presence, layout variants, language, and field locations.
3. Create new synthetic slips reproducing layout classes without names, references, amounts, images, or metadata from real files.
4. Annotate expected fields and confidence on synthetic fixtures.
5. Measure field exact-match, numeric/date accuracy, missing-field rate, false-positive rate, latency, memory, and install size.
6. Test rotated, downscaled, blurred, cropped, duplicate, malformed, encrypted, and unsupported files.

## Acceptance gate

Choose the smallest pipeline that meets 100% accuracy for side, symbol, quantity, price, currency, and net amount on the agreed synthetic validation set, with all uncertain values routed to review. Until samples exist, ADR-0009 remains `Needs Evidence`.

Local vision models remain deferred unless deterministic OCR/parser evidence is inadequate and a separate privacy/accuracy evaluation is approved.
