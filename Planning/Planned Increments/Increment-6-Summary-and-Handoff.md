# Increment 6 Summary and Increment 7 Handoff

**Date:** 2026-06-07
**Status:** Complete and handed off
**Scope:** Hello Buddy Canine Physiotherapy Admin - Release 1, Increment 6

## What Increment 6 delivered

Increment 6 delivered the governance, traceability, and control slice on top of the Increment 5 preview/publish pipeline. The implementation now covers:

- immutable edit guardrails for programmes with publish history;
- version-history API and UI view for practitioner traceability;
- create-new-draft-from-published workflow (branching from published history);
- owner GDPR data-control flow (hard delete when safe, anonymise when retention applies);
- optional practitioner allow-list hardening for API caller identity;
- Increment 6 defect fixes in UI behavior and API query behavior;
- standards conformance pass for Increment 6 touched files.

## Key technical outcomes

- Programme immutability is enforced across mutating programme routes by checking publish-history lock state before update/structure/add/remove operations.
- Version-history retrieval returns stable rows even when practitioner reference data is missing, by using a resilient projection fallback for author display.
- Draft-from-published clones programme structure and session exercises into a new planned draft and seeds a draft programme-version row for audit continuity.
- Owner data-control behavior is implemented in repository logic as:
  - hard delete when no linked pets/accounts are present;
  - anonymise owner and linked account personal data when clinical linkage exists.
- API practitioner header hardening supports optional `Security:AllowedPractitionerIds` deny-by-default filtering when configured.
- Configuration load order was corrected so allow-list values are read after Key Vault provider layering.
- Programme history UI was refactored to card-based rendering to align with mobile-first standards (no table-first dependency).

## Change requests completed during/after Increment 6

- Increment 6 scope items for version traceability, immutable post-publish behavior, and GDPR owner controls were implemented in code and tests.
- Standards-driven hardening updates applied after review:
  - security config load-order fix in API startup;
  - mobile-first rendering adjustment in programme history UI.

## Defects raised and resolved in Increment 6

See `Planning/Error Log/Increment-6-Error-Log.md` for full defect records and fix details.

- ERR-I6-001: Case note submission visibility issue on case detail flow. Resolved.
- ERR-I6-002: Invalid/empty exercise video placeholder remained clickable. Resolved.
- ERR-I6-003: Programme version-history endpoint returned empty list in in-memory path due brittle projection join behavior. Resolved.
- ERR-I6-004: UI compile failure (`CreateCaseNoteRequest` missing type import in case-detail controller). Resolved.

## Open code-review findings and proposed fixes

- ERR-I6-005: Add owner-level authorization scoping for GDPR data-control so only linked practitioners can invoke irreversible actions.
- ERR-I6-006: Write explicit audit-log records for both GDPR outcomes (anonymise and hard-delete), including actor, timestamp, and outcome.
- ERR-I6-007: Replace programme-version status magic strings with shared constants to reduce drift and typo risk.
- ERR-I6-008: Add integration coverage for both GDPR branches against a real relational database path.

## Validation status

- Full solution build: passing.
- Targeted Increment 6 tests: passing (47 passed, 0 failed).
- Standards conformance review pass completed for Increment 6 touched files.

### Post-review test implementation note (2026-06-08)

- Added and passed Testcontainers integration coverage for GDPR data-control branches and scoping:
  - hard-delete branch audit coverage,
  - anonymise branch audit coverage,
  - unlinked practitioner deny-by-default behavior.
- Re-ran targeted suites after Increment 5/6 hardening updates: API in-memory + UI controller tests passing (37 passed, 0 failed).

## What Increment 7 should consume

Increment 7 should build on the Increment 6 governance surface rather than introducing parallel traceability or data-control paths.

Recommended dependencies for the next chat:

- Programme immutability and draft-from-published are now the canonical post-publish editing path.
- Programme version-history API and UI are stable interface points for any future audit/reporting enhancements.
- Owner GDPR data-control endpoint and UI action are the canonical owner-retention behavior for Release 1.
- Practitioner allow-list remains optional-by-config and should be environment-configured rather than code-forked.

## Open items carried forward

- Azure deployment and post-deploy re-test for Increment 6 were not executed in this chat and remain pending environment execution.
- TD-002 remains deferred: domain split and AuthN/Z policy migration.
- TD-003 remains deferred: browser automation coverage for exercise media preview interactions.
- TD-005 remains deferred: video still capture limitations across provider URL shapes.

## Handoff note

Increment 6 is complete enough for Increment 7 to begin. The next conversation should prioritize deployment-lane verification (Azure re-test and observability checks) and then move to the highest-value Release 1 backlog slice, while preserving Increment 6 contracts for immutability, version history, and GDPR owner data control.
