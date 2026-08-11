# Dime Slip Extraction Evaluation

Status: Needs Evidence. Research date: 2026-08-11.

## Facts

- [Tesseract documentation](https://tesseract-ocr.github.io/tessdoc/) identifies Tesseract 5.x as the stable open-source OCR engine under Apache-2.0.
- [PaddleOCR PP-OCRv5 multilingual documentation](https://www.paddleocr.ai/latest/en/version3.x/algorithm/PP-OCRv5/PP-OCRv5_multi_languages.html) includes Thai and English support.
- [PaddleOCR 3.x usage](https://www.paddleocr.ai/main/en/version3.x/pipeline_usage/OCR.html) documents current OCR pipelines and model choices. PP-OCRv6 does not list Thai in its unified 50-language set, so PP-OCRv5 remains a required Thai candidate.

## Recommendation

Use a deterministic layered pipeline: embedded PDF text, strict Dime anchor/template parser, OCR only when necessary, arithmetic validation, then human review. Benchmark Tesseract 5 and PaddleOCR 3.x/PP-OCRv5; do not select by generic benchmark claims.

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
