# ACR008 - RTBF (Privacy) Owner Selection by Name Dropdown, Not User ID

Date: 2026-06-09
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Privacy / RTBF)

## Why this change

In the right to be forgotten (RTBF) area on the Privacy page, the owner is currently selected by
user ID. Practitioners do not work with raw user IDs, which is error-prone and unfriendly. The
selection must use the owner name in a dropdown instead.

## Scope

In scope:
- Replace the user ID input/selection in the RTBF area with an owner name dropdown.
- Populate the dropdown with owner names for selection.
- Ensure the selected owner name maps to the correct underlying owner for the RTBF action.

Out of scope:
- Changing RTBF delete/anonymise behavior (covered by the existing RTBF CR).
- Changing owner identity model/storage.

## Acceptance criteria

1. The RTBF area presents owner selection as a dropdown of owner names.
2. The raw user ID is no longer used as the selection input in the RTBF area.
3. Selecting an owner name targets the correct owner for the RTBF action.
4. The confirmation/warning flow continues to reference the selected owner clearly.

## Implementation notes (proposed)

- Bind the RTBF owner selector to an owner-name dropdown sourced from the owner list, carrying
  the owner identifier behind the scenes.
- Keep the existing confirmation and irreversible-warning flow intact.
- Consider disambiguation for duplicate owner names (for example secondary detail in the option).

## Risks and mitigations

Risks:
- Duplicate owner names causing ambiguity.
- Large owner lists impacting dropdown usability.

Mitigations:
- Include a secondary distinguishing detail where names collide.
- Consider a searchable/typeahead dropdown if the list is large.
