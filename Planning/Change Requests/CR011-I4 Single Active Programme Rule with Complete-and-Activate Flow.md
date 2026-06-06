# CR011-I4 - Single Active Programme Rule with Complete-and-Activate Flow

Date: 2026-06-05
Status: Implemented
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Programme lifecycle on a treatment case)

## Why this change

The system should not allow more than one active programme at the same time for the same treatment case. Practitioners need a clear lifecycle where they can complete the current active programme, then activate a planned programme.

Without an explicit rule and transition workflow:
- multiple active programmes can cause clinical ambiguity;
- users may be unclear which programme is currently in force;
- downstream reporting and PDF history can become inconsistent.

## Scope

In scope:
- Enforce a single-active-programme rule per treatment case.
- Add explicit action to complete an active programme.
- Add explicit action to activate a planned programme.
- Replace hardcoded programme status text in Case UI rows with a status dropdown that shows the current status by default and exposes other valid statuses for selection.
- Place the status Apply action in the actions column immediately to the left of the Builder button.
- Block activation when another programme in the same case is already active.
- Return clear, user-friendly conflict messages for blocked transitions.
- Ensure lifecycle transitions are enforced in API and UI paths.
- Add test coverage for happy paths and blocked paths.

Out of scope:
- Bulk activation/completion operations.
- Cross-case activation rules.
- Redesign of programme builder UI beyond status actions.

## Lifecycle rules

Recommended rule set for Increment 4:
- `planned` -> `active` is allowed only when no other programme for the same treatment case is currently `active`.
- `active` -> `completed` is allowed.
- `planned` -> `completed` is not allowed directly.
- `completed` is terminal for this increment (no reactivation).
- Optionally maintain `IsCurrent` in sync with status:
  - active programme: `IsCurrent = true`;
  - planned/completed programmes: `IsCurrent = false`.

## Acceptance criteria

1. At most one programme per treatment case can have status `active` at any time.
2. Completing an active programme changes its status to `completed`.
3. Activating a planned programme succeeds only when no other programme in the same case is active.
4. If activation is attempted while another programme is active, API returns conflict and UI shows a readable message.
5. Status transition rules are enforced server-side (not UI-only).
6. Existing create/edit/publish behaviours continue to work.
7. Integration tests cover:
   - complete active programme success;
   - activate planned success when no active exists;
   - activate planned conflict when active already exists.

## Proposed implementation impact

- Contracts/Application:
  - add transition result enums for complete/activate operations;
  - extend programme service/repository interfaces with transition methods.
- Infrastructure:
  - implement transition logic with transactional checks by treatment case;
  - enforce single-active constraint in write path.
- API:
  - add lifecycle endpoints (for example `POST /complete`, `POST /activate`);
  - return `409 Conflict` for single-active rule violations.
- UI:
  - replace case-row hardcoded status text with a dropdown showing current status plus valid alternatives;
  - place Apply button to the left of Builder in each programme row action cluster;
  - map selected status transitions to activate/complete lifecycle operations;
  - surface success/error TempData messages.
- Tests:
  - API in-memory + integration tests for transition matrix and conflict behavior;
  - UI smoke tests for action visibility and feedback.

## Dependencies and risks

Dependencies:
- agreement that the single-active rule is per treatment case;
- confirmation of allowed status transitions for this increment.

Risks:
- race condition if two activation requests happen simultaneously;
- stale UI views if status changed by another user/session.

Mitigations:
- perform activation checks and update atomically in a transaction;
- return conflict responses with deterministic messages;
- refresh programme list after transition actions.
