# ADR-0001: Layered Release Regression Gate

- Status: Accepted
- Date: 2026-06-08

## Context

Release 1 needs a repeatable gate that catches regressions early, aligned to integration-first testing. Existing tests already cover UI smoke paths, API fast checks, and real-DB integration paths, but release execution needed a single operational pattern.

## Decision

Adopt a layered release gate with mandatory local order:

1. UI smoke tests.
2. API in-memory tests.
3. API real-DB integration tests.
4. Optional Azurite blob lane when enabled.
5. Cloud deployment and full post-deploy retest.

A dedicated script orchestrates the local gate to reduce operator variance.

## Consequences

- Pros: deterministic execution, faster failure detection, reduced release ambiguity.
- Pros: preserves integration-first quality strategy.
- Cons: longer local gate runtime than ad hoc test selection.
- Cons: requires clear environment variables for DB/Azurite lanes.
