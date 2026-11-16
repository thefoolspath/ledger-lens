# 0007.02 — IdentityAccessDemo failure-boundary implementation plan

Status: Two-operation demo implemented and verified; owner review before other-service rollout remains. Last reviewed: 2026-11-16.

Implementation progress (2026-11-13): the two create-conflict variants now have distinct `identity_demo.users.*` Problem Details codes and English/Thai RESX text through an opt-in `ApiProblemFactory` overload. Malformed JSON is handled as a safe `400` instead of generic `500`. The API-local unexpected handler logs bounded exception type and method-frame identifiers without logging exception messages, and does not write a new body after the response starts. The two-operation HTTP and distributed tests passed. System-wide plan 0004 and owner review remain open.

> This is a demo-first implementation plan for the existing `UsersCreate` and `UsersGetOne` operations. The owner will review the resulting pattern before it is applied to another service. It does not close the system-wide [0007.02 decision](02-typed-failures-and-exception-handling.md) or [plan 0004](../0004-unified-api-response-contract.md).

## Goal and boundaries

Make expected failures, transport errors, unexpected exceptions, cancellation, localization, and safe diagnostic logging explicit in the IdentityAccessDemo API. Keep the `IExceptionHandler` API-local for this demonstration. Extend the shared `ApiProblemFactory` only with an opt-in overload needed to preserve the existing Problem Details diagnostics while supplying demo-specific localized conflict text. Do not register a new shared exception handler or change another service's response behavior.

The source is now named `IdentityAccessDemo` under plan [0007.08](08-identityaccess-demo-complete-patterns.md). The stable error codes and behavior were carried through the rename. The page, import, export, and job operations planned in 0007.08 adopt the reviewed pattern in their own stages. Authentication, authorization, timeout values, durable log retention, and the wider 0004 rollout are outside this milestone.

## Contract decisions

| Condition | HTTP result | Code and response |
| --- | --- | --- |
| Valid create, including a matching request-ID replay | `201` | Existing shared success envelope and current `Location` policy; retain the stored user on replay. |
| Valid get-one | `200` | Existing shared success envelope. |
| Invalid `UsersCreate` command | `400` | `common.validation.invalid_request`, with existing `errors.request`; no exception text. |
| Malformed JSON or binding failure | `400` | Safe Problem Details with `common.validation.invalid_request`; no raw parser text. |
| Unknown user | `404` | Existing `common.resource.not_found`. |
| Existing normalized email with a different request ID | `409` | `identity_demo.users.email_already_exists`. |
| Existing request ID with different input | `409` | `identity_demo.users.request_id_conflict`. |
| Unexpected exception or invalid handler/store result | `500` | Localized generic `common.internal.unexpected`; no exception details in the response. |
| Operation cancelled because `RequestAborted` fired | No Problem Details body | Stop response writing; do not record an application Error. A local `499` status may be assigned for telemetry if the response has not started, but it is not a client contract. |

All Problem Details keep `application/problem+json`, matching HTTP and body status, `code`, `traceId`, and `correlationId`. The two new codes are stable, lowercase, dot-separated API contract values. Their en-US and th-TH titles/details live in the demo API's RESX resources, with neutral English fallback. Missing resource keys fall back to a safe generic message and produce a resource-key-only warning. Continue to use synthetic `@example.test` data in fixtures; 0007.08 owns the later input restriction itself.

## Implementation stages

1. **Pin the current contract.** Extend handler and HTTP tests for invalid create input, both database conflict classifications, matching request-ID replay, unknown get-one, and the current success envelopes. Record existing response shape before changing error codes. Do not change persistence schema, transaction or idempotency logic.
2. **Map typed failures at the API boundary.** Keep `UsersCreateFailure` transport-independent and `UsersGetOne` absence as its current nullable result. Add demo-owned error-code constants, localized resources, and a small `UsersCreate` failure mapper. Add an opt-in `ApiProblemFactory` conflict overload accepting a stable code and already localized safe title/detail; it must reuse the existing diagnostics and content-type construction. Only the demo calls it. Keep the generic `400` validation payload and `404` behavior unchanged.
3. **Separate transport, cancellation, and unexpected failures.** Handle ASP.NET Core `BadHttpRequestException` as a safe `400` without broadly mapping `ArgumentException` or `InvalidOperationException`. Update the API-local unexpected handler so request-aborted cancellation is not treated as a server failure, while other unhandled exceptions produce the generic localized `500`. Never attempt a new Problem Details response after the response has started or the client has disconnected.
4. **Make unexpected diagnostics useful and private.** Emit exactly one structured Error record for a handled unexpected exception. Allowlist operation/endpoint name, outer and bounded inner exception type names, bounded method-level stack-frame identifiers, trace ID, and correlation ID. Exclude raw exception messages, `Exception.ToString()`, stack text containing messages, argument values, source file paths, request URL/body, email, credentials, provider payloads, and financial values. Bound the diagnostic chain to eight exception types and 64 frames; include truncation flags. Do not emit an application Error for expected validation, conflict, not-found, malformed input, or client cancellation. Rely on the .NET 10 default suppression of middleware diagnostics for exceptions handled by `IExceptionHandler`; test for missing or duplicate records.
5. **Verify and document.** Add focused unit, API-host, and Gateway tests. Update this plan, the parent 0007.02 decision record, 0007.08, and relevant API/observability documentation with actual results. Update `PROJECT_STATE.md` only when verified behavior or project state changes. Keep the whole-system 0007.02 and plan 0004 statuses open for Portfolio Core and Market Data.

## Acceptance evidence

- Handler tests prove invalid input returns a typed failure, duplicate email and request-ID conflicts remain distinct, a matching retry returns the original user, and cancellation reaches the store.
- API-host tests assert the exact status/code/content type/diagnostic fields for every table row, both supported languages and English fallback, and absence of raw exception and parser text.
- A test logger asserts one safe Error record for an unexpected failure, no duplicate middleware Error, bounded diagnostic fields, and no Error for request-aborted cancellation or expected failures. Inject an exception containing a synthetic email and secret-like marker to prove neither appears in the response nor logs.
- A malformed JSON request remains `400`; an unrelated `ArgumentException` or `InvalidOperationException` raised below the endpoint becomes `500`, not an accidental `400` or `409`.
- Gateway create/get-one, both conflict variants, and generic `500` preserve the backend Problem Details contract and correlation identifiers through the loopback route. Use ephemeral PostgreSQL for the retry and unique-index cases; host tests with a fake store alone do not establish database behavior.
- Run focused .NET tests, the relevant distributed test, solution and Angular production builds, `lg docs`, a tracked-file privacy scan, and `git diff --check`. Record exact commands and outcomes; do not claim runtime or log-retention behavior from static checks alone.

## Review gate

After the two-operation demo passes, present its response fixtures, sanitized log fixture, and test evidence to the owner. Do not apply the pattern to Portfolio Core, Market Data, or other services until the owner accepts it. Subsequent demo operations use the same rules, with operation-specific failure codes and tests added in their own 0007.08 stages.

## Verification through 2026-11-16

- IdentityAccessDemo HTTP tests passed 12/12. They cover two conflict codes, Thai text, English fallback, malformed JSON, generic `500`, correlation, safe bounded Error logging, unrelated `ArgumentException`, request-aborted cancellation, invalid handler result shape, and allowlisted operation logging. Expected validation, conflict, not-found, and malformed requests generated no application Error in the test logger.
- Gateway integration tests passed 16/16, including the new `500` pass-through test. An explicitly enabled Aspire test passed 1/1 through Web and Gateway against ephemeral PostgreSQL, including migration, matching request-ID replay, duplicate email, and changed-input request-ID conflict. The test asserted both new codes. Docker was started locally for this test.
- The .NET solution built with zero warnings and errors after restoring packages with `NuGetAudit=false`. The Angular production build passed. The broad .NET test command exited `0`; distributed scenarios were disabled in that broad run and exercised separately above.
- A Gateway test observed two identical `X-Correlation-ID` response header values when both Gateway and backend supplied the header. The body and both header values agreed. Header deduplication affects shared Gateway behavior and remains outside this demo-first change.
- No log-retention backend was added. The test logger verifies the demo handler's Error record and non-disclosure for synthetic markers; it does not prove an external collector's retention behavior.
- On 2026-11-16, the full .NET solution test command exited `0`, with Gateway integration tests passing 16/16. The explicitly enabled IdentityAccessDemo distributed test passed 1/1 again with repository-pinned Node 24.19.0. A preceding attempt failed to start the Angular web resource because machine-global Node 24.4.1 was below Angular CLI's minimum; that run did not test the API contract.
