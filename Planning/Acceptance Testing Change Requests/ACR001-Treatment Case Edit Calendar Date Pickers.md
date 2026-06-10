# ACR001 - Treatment Case Edit: Calendar Date Pickers for Start and End Dates

Date: 2026-06-09
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Treatment case edit screen)

## Why this change

On the treatment case edit screen the start date and end date are captured as freeform text
inputs. This forces practitioners to type dates by hand, which is slow and error-prone and
allows inconsistent or invalid date formats to be entered.

## Scope

In scope:
- Replace the freeform start date input with a date picker (calendar) control on the treatment
  case edit screen.
- Replace the freeform end date input with a date picker (calendar) control.
- Keep the existing stored date format/data type unchanged; only the input experience changes.
- Preserve existing validation (for example end date not before start date, where applicable).

Out of scope:
- Changing the underlying date storage model or API contract.
- Date pickers on unrelated screens.
- Time-of-day capture.

## Acceptance criteria

1. The start date field on the treatment case edit screen presents a calendar/date picker.
2. The end date field on the treatment case edit screen presents a calendar/date picker.
3. A date selected from the calendar populates the field and is saved correctly.
4. Manual typing, if still permitted, accepts only valid dates and falls back to the picker
   format.
5. Existing date validation rules continue to apply after the change.

## Implementation notes (proposed)

- Use a native HTML `type="date"` input or the project's standard date picker component for
  consistency across the admin UI.
- Bind the picker to the existing start/end date model properties so persistence is unchanged.
- Confirm culture/format handling matches the rest of the admin application.

## Risks and mitigations

Risks:
- Browser/native date picker rendering differences across platforms.
- Format mismatch between picker output and existing model binding.

Mitigations:
- Standardise on one picker approach used elsewhere in the admin UI.
- Verify round-trip save/load of dates in local and Azure environments.
