# ACR004 - Remove Cases from Main Menu (Owner to Pet to Case Navigation)

Date: 2026-06-09
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Main navigation / menu)

## Why this change

Cases currently appear as a top-level item in the main menu. The intended navigation model is
to reach a case via its owning records: Owner -> Pet -> Case, or Pet -> Case. A top-level Cases
entry bypasses this hierarchy and is not the desired flow.

## Scope

In scope:
- Remove the Cases entry from the main menu.
- Ensure cases are reachable via Owner -> Pet -> Case and Pet -> Case navigation paths.

Out of scope:
- Removing case functionality or the case detail screens themselves.
- Changing owner or pet menu entries beyond removing the standalone Cases item.

## Acceptance criteria

1. The main menu no longer shows a top-level Cases item.
2. A case can be opened by navigating Owner -> Pet -> Case.
3. A case can be opened by navigating Pet -> Case.
4. No broken links or orphaned routes remain after removing the menu item.

## Implementation notes (proposed)

- Remove the Cases navigation link from the main menu/layout.
- Verify drill-down links from owner and pet screens lead to the relevant cases.
- Keep direct case routes available for in-app drill-down even though the menu entry is removed.

## Risks and mitigations

Risks:
- Users relying on the top-level Cases shortcut.
- Hidden routes that previously depended on the menu entry.

Mitigations:
- Confirm owner/pet drill-down covers all case access needs.
- Smoke test case navigation paths after removal.
