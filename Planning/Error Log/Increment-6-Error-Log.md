# Increment 6 - Error Log

## ERR-I6-001: Case note submission not visible immediately on case detail

**Date:** 2026-06-07  
**Status:** Fixed  
**Severity:** High (workflow interruption)  
**Area:** Case detail notes  
**Type:** UI behavior defect

### Symptom

After submitting a new case note, practitioners did not consistently see the newly posted note in the expected detail context.

### Root cause

Post-submit flow behavior in the case-detail controller/view path did not consistently refresh to the user-visible section that contains the latest note state.

### Fix

Adjusted case-note flow wiring in the case-detail UI/controller surface so successful submission returns with the updated notes context visible.

### Validation

- UI and targeted test suites pass after the update.

## ERR-I6-002: Invalid or empty video placeholder remained clickable

**Date:** 2026-06-07  
**Status:** Fixed  
**Severity:** Medium (UX correctness)  
**Area:** Exercise details/edit media preview  
**Type:** UI interaction defect

### Symptom

When exercise video metadata was invalid or empty, the placeholder could still render as an interactive/clickable element.

### Root cause

Presentation guard conditions did not fully gate click behavior on valid, non-empty media targets.

### Fix

Updated exercise detail/edit rendering behavior so invalid or empty video placeholders are non-clickable.

### Validation

- UI tests and smoke coverage pass with updated behavior.

## ERR-I6-003: Programme version-history endpoint returned empty history in in-memory path

**Date:** 2026-06-07  
**Status:** Fixed  
**Severity:** High (traceability break)  
**Area:** Programme version history API  
**Type:** Query/projection defect

### Symptom

`GET /api/programmes/{id}/versions` could return an empty history set under in-memory test conditions even when version rows existed.

### Root cause

A brittle projection shape depended on related practitioner rows and could drop effective results in test data where the relationship was incomplete.

### Fix

Reworked version-history projection to use a resilient author-name fallback path so existing version rows are still returned.

### Validation

- API in-memory tests pass after fix.
- Targeted Increment 6 run: 47 passed, 0 failed.

## ERR-I6-004: UI build failure due to missing contract import

**Date:** 2026-06-07  
**Status:** Fixed  
**Severity:** High (build break)  
**Area:** Case detail controller  
**Type:** Compile-time defect

### Symptom

UI project failed compilation with `CS0246` for `CreateCaseNoteRequest`.

### Root cause

Missing contracts namespace import in the case detail controller.

### Fix

Added required contracts import so `CreateCaseNoteRequest` resolves in the UI controller.

### Validation

- Solution build passes after fix.

## ERR-I6-005: Owner GDPR data-control endpoint has no ownership/authorization scoping

**Date:** 2026-06-08  
**Status:** Fixed  
**Severity:** High (security / GDPR — destructive, irreversible action)  
**Area:** Owner GDPR data control  
**Type:** Authorization scoping (deny-by-default)

### Symptom

`POST /api/owners/{id}/data-control` (`src/HelloBuddy.Api/Endpoints/OwnerEndpoints.cs`) permanently deletes or anonymises an owner using only the route `id`. The handler calls `IOwnerRepository.ApplyDataControlAsync(id, ct)` with no practitioner identity and no linkage check.

Unlike every programme endpoint (which scopes by `practitioner.PractitionerId`), this destructive path performs no per-owner authorization. The request gate in `Program.cs` only confirms a valid/allow-listed practitioner header is present — not that the caller is entitled to act on this specific owner. Any authenticated practitioner can therefore irreversibly destroy any owner record. Standards §13 require deny-by-default authorization.

### Root cause

The data-control endpoint and repository method were implemented without threading practitioner identity through to an ownership/linkage check, so authorization is effectively "any valid practitioner can act on any owner".

### Recommended fix

1. Thread `ICurrentPractitionerAccessor.PractitionerId` into the data-control endpoint and `ApplyDataControlAsync`.
2. Enforce that the practitioner is linked to the owner (via pets/treatment cases) before allowing the action; return `404`/`403` deny-by-default otherwise.
3. Consider requiring an explicit confirmation token for this irreversible action.

### Implementation update

- Threaded practitioner identity into the endpoint and repository, and added deny-by-default owner visibility checks (practitioner-pet/case linkage, with orphan-owner delete path preserved).

### Validation

- Add integration tests asserting a non-linked practitioner is denied, and a linked practitioner is permitted.

## ERR-I6-006: GDPR anonymise/delete writes no audit record

**Date:** 2026-06-08  
**Status:** Fixed  
**Severity:** Medium (GDPR accountability / audit completeness)  
**Area:** Owner GDPR data control  
**Type:** Audit trail gap

### Symptom

`OwnerRepository.ApplyDataControlAsync` (`src/HelloBuddy.Infrastructure/Records/RecordRepositories.cs`) anonymises or hard-deletes personal data with no audit-trail entry recording who triggered the action and when. Standards §13 state audit fields/records are mandatory, and GDPR accountability expects destructive personal-data actions to be logged.

### Root cause

The data-control behaviour was implemented as a direct mutate/remove with no corresponding audit write (and, per ERR-I6-005, without an available practitioner identity to record).

### Recommended fix

- Write an `AuditLog` record for both the anonymise and hard-delete branches, capturing the acting practitioner (from ERR-I6-005), the owner id, the outcome, and the timestamp.

### Implementation update

- Added `Auditlog` writes for both anonymise and hard-delete outcomes with practitioner id, owner id, action type, outcome payload, and timestamp.

### Validation

- Add integration tests asserting an audit record is written for each branch.

## ERR-I6-007: Version-status values are magic strings rather than constants

**Date:** 2026-06-08  
**Status:** Fixed  
**Severity:** Medium (maintainability / consistency)  
**Area:** Programme versioning  
**Type:** Standards conformance (DRY / naming)

### Symptom

Programme-version status values (`"draft"`, `"published"`, `"superseded"`) are scattered string literals across `ProgrammeRepository` — in `CreateDraftFromPublishedAsync`, `PersistPublishedVersionAsync` (Increment 5 publish path), and the version-history comparisons. Some comparisons use `OrdinalIgnoreCase` against lowercase literals, creating casing/typo drift risk.

### Root cause

Version statuses were introduced as inline literals, despite the codebase already establishing `ProgrammeDomainConstants` for programme statuses/periods.

### Recommended fix

- Add version-status constants to `ProgrammeDomainConstants` (e.g. `VersionStatusDraft`, `VersionStatusPublished`, `VersionStatusSuperseded`) and replace all literals in both the Increment 5 publish persistence and Increment 6 version-history/draft-from-published paths.

### Implementation update

- Added `VersionStatusDraft`, `VersionStatusPublished`, and `VersionStatusSuperseded` constants and replaced repository literals in publish/version-history/draft-from-published flows.

### Validation

- Build and run targeted version-history and publish tests to confirm no behavioural change.

## ERR-I6-008: GDPR data-control branches lack Testcontainers integration coverage

**Date:** 2026-06-08  
**Status:** Fixed  
**Severity:** Medium (test coverage for irreversible behaviour)  
**Area:** Owner GDPR data control  
**Type:** Test coverage gap

### Symptom

The two data-control branches — hard delete when no linked pets/accounts exist, and anonymise when clinical linkage exists — are not covered by Testcontainers MySQL integration tests. Given the action is irreversible and depends on FK/cascade behaviour, both branches need real-database coverage (§12).

### Root cause

Increment 6 added the data-control behaviour without integration tests proving both branches and their cascade/anonymisation effects against a real schema.

### Recommended fix

- Add Testcontainers MySQL integration tests covering: hard delete path (no links → owner removed), anonymise path (linked clinical data → personal fields redacted, accounts deactivated, clinical records retained).

### Implementation update

- Added Testcontainers integration tests covering hard-delete and anonymise branches, plus unlinked-practitioner denial and audit-log assertions.

### Validation

- New integration tests pass; assert post-state of owner, user accounts, pets and treatment cases for each branch.
