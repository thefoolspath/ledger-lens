# Minimum Viable Product

The MVP is the smallest release that can replace a private spreadsheet for core portfolio accounting without losing auditability.

## Required user journey

1. Start the local stack with one documented command.
2. Create a USD portfolio and Dime account.
3. Enter deposits, buys, sells, fees, taxes, dividends, and withdrawals manually.
4. See ledger-derived holdings, remaining lots, FIFO and average-cost views.
5. Fetch the latest available stock price and USD/THB rate on demand.
6. See provider, data-as-of, retrieval time, and delay class.
7. View USD P/L and exact two-part THB attribution.
8. Correct an error without erasing the original financial history.
9. Back up and restore the database and document store.

## Exit criteria

- Calculation and reconciliation test suites pass using synthetic cases.
- Rebuilding projections from the ledger produces the same holdings and balances.
- No tracked or logged personal data or secret is found by safety checks.
- Loopback and malicious-origin security tests pass.
- Restore verification succeeds on a clean local environment.

Dime OCR is not an MVP blocker until representative samples complete the extraction gate. Manual entry remains the safe fallback.
