# ACR023 - Disable the PDF Builder Button when a Programme is Published / Locked

Date: 2026-06-12
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Case detail programme list and any PDF Builder entry points)

## Why this change

Once a programme is published it is immutable (locked for editing). Navigating to the
**PDF Builder** (currently "Builder") for a locked programme leads to a form the practitioner
cannot edit, which is frustrating. The PDF Builder entry point should be **disabled** when the
programme is published/locked. To make further changes, the practitioner creates a new draft from
**PDF History** ("New draft from published").

## Scope

In scope:
- Disable (or hide) the **PDF Builder** link/button for programmes that are published/locked for
  edit, on every place that link is offered (primarily the Case detail programme list).

Out of scope:
- The Builder page's own server-side read-only/immutability behaviour (already enforced) — this
  ACR is about the navigation entry point.
- PDF Viewer and PDF History entry points — these remain available for locked programmes.

## Acceptance criteria

1. For a published/locked programme, the **PDF Builder** button/link is disabled (visibly
   non-interactive) or hidden, with an accessible explanation (e.g. tooltip/`title`
   "Published programmes are locked. Create a new draft from PDF History to edit.").
2. For an editable (draft) programme, the **PDF Builder** button works exactly as today.
3. The PDF Viewer and PDF History links remain available for locked programmes.
4. Published programmes are edited only by creating a new draft from PDF History (the existing
   "New draft from published" capability).
5. Implementation conforms to coding standards; suites green.

## Implementation guidance

- The locked state is the existing immutability signal
  (`IProgrammeService.IsLockedForEditAsync` and its API-client equivalent), driven by published
  version history.
- The Case detail programme list (`src/HelloBuddy.Ui/Views/CaseDetail/Index.cshtml`) renders the
  per-programme **Builder** link from `Model.Case.Programmes`. The current programme summary
  exposes `Status` (planned/active/completed) but **not** necessarily a published-lock flag, so:
  - Surface a per-programme `bool` (e.g. `IsLockedForEdit` / `HasPublishedVersion`) on the
    programme summary/list view-model used by Case detail, populated from the same
    published-history check used elsewhere. Do not duplicate the immutability logic — reuse the
    existing service/repository method.
  - When that flag is true, render the PDF Builder control as disabled (e.g. Bootstrap
    `disabled`/`aria-disabled` styling on a non-navigating element, or omit the link) with the
    explanatory `title`.
- Apply the same guard anywhere else a PDF Builder link is offered for a specific programme.

## Risks and mitigations

Risks:
- The Case detail listing may require an extra data field; fetching it per row could add queries.

Mitigations:
- Include the locked/published flag in the existing programme-list projection (single query),
  consistent with the "prefer projection" data-access standard — avoid per-row N+1 calls.

## Verification

- Manual: a published programme shows a disabled PDF Builder control with a tooltip; a draft
  programme's PDF Builder works; PDF History "New draft from published" remains the edit path.
- UI smoke: assert the disabled/omitted PDF Builder control for a locked programme and the
  enabled control for a draft.
- Full solution builds clean; suites green.
