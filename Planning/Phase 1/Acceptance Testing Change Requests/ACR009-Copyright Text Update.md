# ACR009 - Update Copyright Text to "(c) 2026 Net Intentions Ltd"

Date: 2026-06-09
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Footer / branding)

## Why this change

The copyright text shown after the word "Copyright" should read the copyright symbol followed by
"2026 Net Intentions Ltd" (for example "(c) 2026 Net Intentions Ltd"). The current text needs to
be updated to this value.

## Scope

In scope:
- Update the copyright text so that after "Copyright" it shows the copyright symbol, then
  "2026 Net Intentions Ltd".

Out of scope:
- Dynamic/auto-updating year logic (unless requested later).
- Other footer content changes.

## Acceptance criteria

1. The footer copyright displays the copyright symbol followed by "2026 Net Intentions Ltd".
2. The previous copyright text no longer appears.
3. The copyright symbol renders correctly across supported browsers.

## Implementation notes (proposed)

- Update the footer text in the shared layout to use the copyright symbol entity and the new
  company/year string.
- Verify rendering of the copyright symbol (use the proper character/HTML entity).

## Risks and mitigations

Risks:
- Encoding issues with the copyright symbol.

Mitigations:
- Use the correct HTML entity/character and verify in the rendered footer.
