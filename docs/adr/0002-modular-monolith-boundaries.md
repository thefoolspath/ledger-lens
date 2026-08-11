# ADR-0002: Modular Monolith Boundaries

## Status

Superseded by [ADR-0015](0015-coarse-grained-microservices.md) on 2026-08-11.

## Context

The initial planning baseline favoured one deployable modular monolith because LedgerLens is single-user and local.

## Superseded decision

The former decision kept API and business modules in one deployable process with one shared application database and split workers only for measured runtime isolation.

## Reason for supersession

The owner selected an Aspire-orchestrated microservice foundation. The replacement keeps financial invariants together in a coarse Portfolio Core boundary while giving Market Data, Slip Import, Research, and Operations independent process and data ownership.

