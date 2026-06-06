# Increment 4 — Error Log

## ERR-I4-004: 500 on "Publish PDF" from Builder page

**Date:** 2026-06-05
**Status:** Fixed
**Severity:** High (feature-blocking)
**Area:** Programme builder — PDF publish
**Reproduction:** Cases → select case → Builder → Publish PDF (local environment)

### Symptom

Clicking **Publish PDF** on the builder page returns a 500 from the API, surfaced in the UI as:

```
HttpRequestException: Response status code does not indicate success: 500 (Internal Server Error).
  HttpResponseMessage.EnsureSuccessStatusCode()
  AdminApiClient.EnsureSuccessOrThrowAsync(HttpResponseMessage, CancellationToken)
  AdminApiClient.PublishProgrammeAsync(ulong id, CancellationToken ct)
  ProgrammesController.Publish(ulong id, CancellationToken ct)
```

### Root cause

The API's local `PdfService:Uri` was pointing at the wrong port. The publish pipeline
(`ProgrammeService.PublishAsync`) renders the programme HTML and then calls the PDF sidecar
via `HttpPdfRenderer` (`POST {PdfService:Uri}/render`). In local:

- The PDF service `http` launch profile listens on `http://localhost:5081`
  (`HelloBuddy.PdfService/Properties/launchSettings.json`), and the run script's health
  check waits on `http://localhost:5081/healthz`.
- But `HelloBuddy.Api/appsettings.Development.json` set `PdfService:Uri` to
  `http://localhost:5068`.

So the API tried to POST to `http://localhost:5068/render`, where nothing was listening, the
HTTP call failed, and the publish endpoint returned 500.

### Storage clarification (was suspected, not the cause)

Local storage is **not** hitting real Azure blob storage:

- `appsettings.Development.json` sets `Storage:Mode = "Azurite"`, and `ResolveStorageMode`
  defaults to Azurite in Development regardless, so the emulator is used.
- The publish write path uses `UseDevelopmentStorage=true` (Azurite at
  `127.0.0.1:10000`), and the read-URL/download path uses shared-key SAS
  (`blob.CanGenerateSasUri == true`), which Azurite supports — the Azure AD
  user-delegation-key path is only taken in cloud (managed identity) mode.

### Fix

- Set `PdfService:Uri` to `http://localhost:5081` in
  `HelloBuddy.Api/appsettings.Development.json` to match the PDF service's local port.

### Follow-up

- Consider wrapping the API publish endpoint in a structured try/catch so an unreachable PDF
  service (or storage error) surfaces as a readable 503/400 instead of a bare 500.
- Keep the PDF port aligned across `launchSettings.json`, the run script health check, and
  `appsettings.Development.json` if it ever changes.

---

## ERR-I4-001: 500 on "Create draft programme"

**Date:** 2026-06-05
**Status:** Fixed
**Severity:** High (feature-blocking)
**Area:** Programme builder — draft programme creation

### Symptom

Clicking **Create draft programme** on the case detail page returned a 500 from the API, surfaced in the UI as:

```
HttpRequestException: Response status code does not indicate success: 500 (Internal Server Error).
  AdminApiClient.EnsureSuccessOrThrowAsync(...)
  AdminApiClient.CreateDraftProgrammeAsync(ulong caseId, ...)
  CaseDetailController.CreateProgramme(ulong id, ...)
```

The UI call (`POST /api/cases/{caseId}/programmes`) reached the API, but the API failed while persisting the new programme.

### Root cause

The new draft programme was inserted with `Programme.Status = "draft"`. The `programme.Status` column is a MySQL enum constrained to:

```
enum('planned','active','completed','cancelled')
```

`'draft'` is not a member of that enum, so the `INSERT` was rejected by MySQL and the API returned 500.

The `draft` concept belongs to `ProgrammeVersion.VersionStatus` (`enum('draft','published','superseded','retired')`), not to `Programme.Status`. The programme-level status for a newly created, not-yet-active plan is `'planned'`.

### Why tests did not catch it

- The in-memory test provider does not enforce MySQL enum constraints, so `"draft"` was accepted there and the test passed.
- The real-schema integration test that would have caught it requires a Docker/MySQL Testcontainer, which was not running during the local test pass, so that test did not execute.

### Fix

- `ProgrammeRepository.CreateDraftAsync` now sets `Status = "planned"` (a valid `programme` enum value).
- The session row continues to use `Status = "planned"` and `Period = "single"`, both valid for their respective enums.
- Tests and the UI smoke stub updated to assert/return `"planned"` instead of `"draft"`.

### Follow-up

- Keep the user-facing label as "draft programme" (the button text and programme name suffix) — only the persisted `Programme.Status` value changed.
- When publish/version work lands, the `draft` lifecycle should be modelled on `ProgrammeVersion.VersionStatus`, not on `Programme.Status`.
- Consider running the MySQL Testcontainer integration suite as part of the local gate so enum/schema mismatches are caught before deployment.

## ERR-I4-002: Redundant title pencil edit control (operator error)

**Date:** 2026-06-05
**Status:** Fixed
**Severity:** Low (cosmetic / redundant control)
**Area:** Programme builder — title editing
**Type:** Operator error (requirement misread during CR012-I4)

### Symptom

CR012-I4 added a pencil icon next to the programme title heading to toggle title editing. This duplicated functionality that was already provided by the editable **Programme title** field in the **Dates and session structure** card.

### Root cause

During CR012-I4 implementation the title-edit requirement was over-delivered: the setup card already exposed (or could expose) the title field, so the additional pencil affordance in the page heading was unnecessary.

### Fix

- Removed the pencil icon button from the programme title heading.
- Made the **Programme title** input in the **Dates and session structure** card directly editable (removed `readonly`).
- Editing the title now enables **Save setup** via the existing change-detection logic.
- Removed the now-unused pencil JavaScript handler.

## ERR-I4-003: "Save session edits" button rendered at top of page

**Date:** 2026-06-05
**Status:** Fixed
**Severity:** Medium (usability)
**Area:** Programme builder — session edits save action

### Symptom

After editing session exercises near the bottom of the builder page, the **Save session edits** button appeared at the **top** of the page. The user had to scroll all the way back up from the last session row to save, despite already being at the bottom of the content.

### Root cause

In CR012-I4 the single `Save session edits` button block (`#session-edits-bottom`) was placed immediately after the Razor `@{ }` header block, so it rendered at the very top of the page body instead of after the session cards.

### Fix

- Moved the `#session-edits-bottom` save button block to the end of the view, after the `@foreach` session loop, so it renders at the bottom of the page.
- Confirmed post-save scroll behaviour: saving session edits redirects with `BuilderScrollTarget = "bottom"`, and the page script scrolls to `document.body.scrollHeight` on load, so the user remains at the bottom after saving.
