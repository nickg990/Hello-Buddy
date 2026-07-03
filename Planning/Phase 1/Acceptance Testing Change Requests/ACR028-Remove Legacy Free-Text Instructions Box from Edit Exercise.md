# ACR028 - Remove the Legacy Free-Text Instructions Box from Edit Exercise (UI only)

Date: 2026-06-15
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin — Exercise editor (Edit Exercise) UI only

## Why this change

The Edit Exercise screen still renders a read-only "Legacy free-text instructions (read only)"
textarea for exercises that were imported with the old single free-text instruction field. Now that
instructions are authored as ordered steps, this legacy box is redundant clutter on the editor and
should be removed from the UI.

The legacy data must be preserved in the database — this is a UI-only removal. Nothing in the data
model, columns, or stored values is to be deleted.

## Scope

In scope:
- Remove the legacy free-text instructions read-only block from the Edit Exercise view.

Out of scope (must NOT change):
- The database: do not drop, alter, or null any legacy instruction column/data.
- The `LegacyInstructionsText` data flow on the server side may remain populated; only the UI
  rendering is removed. (Removing the now-unused view-model property is optional cleanup, but the
  underlying stored data must stay intact regardless.)

## Known occurrence (remove this block)

- [src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml#L185-L191) —
  the conditional block:

  ```cshtml
  @if (!string.IsNullOrWhiteSpace(Model.LegacyInstructionsText))
  {
      <div class="col-12">
          <label class="form-label">Legacy free-text instructions (read only)</label>
          <textarea class="form-control" rows="3" readonly>@Model.LegacyInstructionsText</textarea>
      </div>
  }
  ```

  → delete this entire block.

## Acceptance criteria

1. The Edit Exercise screen no longer displays the "Legacy free-text instructions (read only)" box
   for any exercise (including legacy-imported exercises that previously showed it).
2. The ordered-step instructions editor is unaffected.
3. No database schema or data is changed; legacy instruction values remain stored.
4. Implementation conforms to coding standards; the solution builds clean and the UI suite is green.

## Implementation guidance

- Delete the legacy block from `Edit.cshtml` (lines noted above).
- Optionally remove the now-unused `LegacyInstructionsText` references in
  `src/HelloBuddy.Ui/Models/ExerciseEditorVm.cs` and the controller's `BuildEditorVmAsync` parameter
  *only if* it is no longer used anywhere — do this only as tidy-up, and never touch the persisted
  source data/column.
- Update or remove any UI test asserting on the legacy textarea markup.

## Risks and mitigations

Risks:
- Accidentally removing or nulling the legacy data instead of just the UI.

Mitigations:
- Restrict the change to the Razor view (and optional unused view-model property). Do not touch
  repositories, EF entities, migrations, or the API contract for the stored legacy value.

## Verification

- Open an exercise known to have legacy free-text instructions; confirm the box is gone and the
  step editor still works.
- Confirm the legacy value is still present in the database after a save (data untouched).
- Full solution builds clean; UI suite green.
