# ADR-0013: Defer Local AI

## Status

Deferred.

## Context

Canonical calculations, extraction, portfolio views, and source-linked research do not require an LLM for MVP.

## Decision

Do not add Ollama, embeddings, vector storage, RAG, or an AI runtime dependency before Phase 5. Future AI may summarize evidence but never author canonical numeric values or uncited facts.

## Alternatives considered

AI-first research, local vision as default slip parser, and hosted LLM integration.

## Evidence

[Product roadmap](../product/ROADMAP.md) and [slip evaluation](../research/DIME_SLIP_EXTRACTION_EVALUATION.md).

## Consequences

The core stays deterministic and smaller; AI assistance arrives later.

## Risks

Future local models can leak data through configuration, logs, plugins, or remote fallbacks.

## Revisit conditions

Require an explicit use case, model/license/resource evaluation, offline verification, citation design, and hallucination/privacy test suite.
