# CR017-I8 - Remove Forced Admin Password Change at Login

Date: 2026-06-11
Status: Implemented
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (authentication seed/default login behavior)

## Why this change

The current login behavior forced the seeded administrator account to change password on first login.

For current release workflows this created confusion during local testing and acceptance walkthroughs, where the expected behavior is direct access after successful login with seeded credentials.

## Scope

In scope:
- Remove the default forced-change requirement for the seeded administrator account.
- Preserve existing password-change capability from admin/account flows.
- Preserve the underlying `MustChangePassword` field and service support for future policy use.

Out of scope:
- Removing password-change screens or endpoints.
- Removing `MustChangePassword` from schema/domain model.
- Introducing a new global password-expiry policy.

## Acceptance criteria

1. Seeded administrator account is created with `MustChangePassword = false`.
2. Existing local administrator record is updated so `MustChangePassword = false`.
3. Administrator can log in directly without being blocked by a forced password-change step.
4. Existing UI auth tests continue to pass.

## Implementation notes

- Updated admin seed behavior in `PractitionerLoginSeedHostedService` to set `MustChangePassword = false` for the seeded admin login row.
- Updated seed service comments/log message to reflect the new default.
- Applied a local database corrective update to clear `MustChangePassword` for `admin@caninephysio.local` so behavior aligns immediately without waiting for data reset.

## Validation

- Verified local DB state for admin login row: `MustChangePassword = 0`.
- UI suite executed successfully after change: 39/39 passed.

## Risks and mitigations

Risks:
- If future security policy requires first-login password rotation, default behavior will need re-introduction.

Mitigations:
- `MustChangePassword` support remains in schema/service and can be re-enabled by policy decision in a future CR.
