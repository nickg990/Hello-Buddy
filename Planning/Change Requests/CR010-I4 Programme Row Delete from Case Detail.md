# CR010-I4 - Programme Row Delete from Case Detail (Bin Icon)

Date: 2026-06-05
Status: Implemented
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Case Detail -> Programmes)

## Why this change

Practitioners can now create draft programmes from a case, but there is no direct way to remove unwanted draft rows from the programme list on the individual case form. This creates clutter and increases risk of opening or publishing the wrong programme.

This CR introduces an explicit delete action per programme row using a bin icon so the case-level workflow stays clean and intentional.

Business and UX benefit:
- reduces accidental use of stale/duplicate draft programmes;
- keeps case-level programme list manageable;
- aligns with expected row-action pattern (open/edit/delete).

## Scope

In scope:
- Add a delete action on each programme row in the case detail programmes table.
- Action is displayed as a bin icon button with accessible label text (screen-reader friendly).
- Delete flow supports server-side POST/DELETE semantics with antiforgery for UI form posts.
- Add a confirmation step (inline confirm or confirmation page/modal) before destructive action.
- Success and error feedback shown on return to case detail page.
- Practitioner ownership checks enforced before delete.

Out of scope:
- Bulk programme deletion.
- Hard-delete behavior for published programme artefacts without business approval.
- Versioning policy redesign.

## Behaviour and guardrails

Recommended default policy for Increment 4:
- Allow delete for draft/planned programmes that are not published.
- Block delete for programmes that have published versions (or replace with archive/deactivate once available).
- If blocked, show a clear reason message rather than failing silently.

## Acceptance criteria

1. Each programme row on case detail includes a visible delete/bin action.
2. Clicking delete requires explicit confirmation before data is removed.
3. Deleting an eligible programme removes it from the case list after refresh/redirect.
4. Attempting to delete a non-owned programme returns not found/forbidden behavior without leaking data.
5. Attempting to delete an ineligible programme (for example already published) shows a safe validation message and leaves data unchanged.
6. Existing case detail and programme builder routes continue to work unchanged.
7. API integration tests and UI smoke/regression tests cover happy and blocked paths.

## Proposed implementation impact

- UI: case detail programmes table row actions and delete form/button.
- UI controller/client: add delete method to admin API client and case detail controller action.
- API: add delete endpoint for programmes.
- Application/infrastructure: delete operation with ownership + policy checks.
- Tests: API integration coverage and UI smoke coverage updates.

## Dependencies and risks

Dependencies:
- agreement on delete policy for published/versioned programmes;
- confirmation UX pattern choice consistent with existing UI.

Risks:
- accidental data loss if guardrails are weak;
- deleting records that are referenced by downstream publish/version data.

Mitigations:
- enforce eligibility rules server-side;
- require explicit user confirmation;
- add integration tests covering constrained deletion and referential constraints.
