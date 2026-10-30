# ADR-0020: Audience-Bound Internal Service Tokens from the Gateway

## Status

Proposed; owner confirmation and implementation evidence are pending.

## Context

After the Gateway authenticates a browser session, internal APIs still need a principal they can verify independently. Browser-provided identity headers are forgeable. Forwarding the browser cookie shares Data Protection and browser credential semantics across services. Forwarding the Google ID token gives it the wrong audience and couples every service to Google.

## Proposed decision

For each proxied business request, the Gateway strips inbound cookies, authorization, and identity headers, then issues a signed internal JWT with a 60-second proposed lifetime, one exact service audience, the internal UUIDv7 subject, session identifier, and only relevant permission claims. The Gateway holds the asymmetric private key; services receive only trusted public verification material.

Each API validates signature, issuer, audience, lifetime, and required claims using ASP.NET Core JWT bearer authentication with inbound claim mapping disabled. Direct requests without a valid service-audience token return `401`. Endpoint policies enforce action permissions, while Application/Infrastructure ownership predicates remain mandatory.

## Alternatives considered

- shared cookie: rejected because every service would share cookie trust and CSRF/browser semantics;
- unsigned identity headers: rejected as forgeable;
- custom signed headers: rejected because canonicalization, expiry, audience, and replay handling would be bespoke security protocol work;
- full OAuth token exchange or an external authorization server: deferred until external clients, independent service trust, or delegated cross-service access justifies it;
- per-service Google validation: rejected because services must trust LedgerLens identity, not a provider-specific browser credential.

## Consequences

LedgerLens becomes responsible for a small closed-system internal issuer and its signing-key lifecycle. Tokens must be short-lived, audience-bound, claim-minimal, hidden from logs, and covered by wrong-audience/expiry/rotation tests. If LedgerLens later exposes APIs to third-party clients, this decision must be revisited in favor of a standards-based authorization server/token exchange design.

## Evidence gate

Accept only after key storage/rotation, YARP transforms, claim schema, session revocation delay, direct-service denial, and distributed two-user isolation are demonstrated.
