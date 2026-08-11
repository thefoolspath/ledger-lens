# Research-to-Project Fit

Last reviewed: 2026-08-11.

| Finding | LedgerLens consequence |
| --- | --- |
| Current supported stack aligns on major version 10/.NET 10 and PostgreSQL 18 | Use this as the implementation baseline, rechecking patches at kickoff |
| Aspire supports JavaScript apps | Keep a C# AppHost and model Angular as a JavaScript resource |
| Free market feeds differ materially in coverage and terms | Preserve provider abstraction and visible feed/freshness metadata |
| BOT offers official daily THB/USD data | Use it for historical/reference FX, not as a claimed real-time quote |
| OCR capability alone does not prove Dime accuracy | Keep manual entry and a sample-driven extraction gate |
| Average basis may not be a tax method for ordinary US shares | Present it as an analytical report, not tax advice |
| Localhost remains reachable by browser-origin attacks | Require strict Host/Origin/content-type controls before MVP |
| User does not want continuous operation | Make refresh and OCR on-demand; defer alerts and notifications |
