# TD-006 - Publish validator refactor deferred pending manual end-to-end verification

Raised: 2026-06-08
Scope: Increment 5 publish/preview validation pathway
Source: ERR-I5-004 (Increment-5-Error-Log)
Owner: Programme publish workstream

---

## Decision

The ERR-I5-004 implementation is intentionally deferred until manual end-to-end testing of the current publish flow is completed.

Deferred item:
- Replace inline publish validation with a reusable FluentValidation-based `PublishProgrammeValidator` shared by preview and publish.

---

## Why deferred

- The current publish path is actively used and requires operator-led end-to-end verification before introducing validation-architecture changes.
- The proposed refactor touches validation behavior and response-shape expectations, so manual UX confirmation is required first.

---

## Current state

- ERR-I5-004 remains open in [Planning/Error Log/Increment-5-Error-Log.md](Planning/Error%20Log/Increment-5-Error-Log.md).
- Existing inline validation remains in the publish service path.

---

## Planned implementation scope (after manual E2E sign-off)

1. Introduce `PublishProgrammeValidator` in Application using FluentValidation.
2. Implement the full standards rule set and keep missing video links as warning-level behavior.
3. Reuse the same validator for preview and publish to guarantee parity.
4. Add/update unit and integration tests for rules and parity behavior.

---

## Exit criteria

- Manual end-to-end publish verification completed and accepted.
- Validator refactor implemented and tests green.
- ERR-I5-004 status changed to Fixed.

---

## Review checkpoint

Target review date: 2026-06-15
