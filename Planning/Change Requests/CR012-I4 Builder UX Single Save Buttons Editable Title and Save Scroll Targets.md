# CR012-I4 - Builder UX: Single Save Buttons, Editable Title, and Save Scroll Targets

Date: 2026-06-05
Status: Implemented
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Programme Builder UX)

## Why this change

Builder editing currently requires multiple local save controls and does not provide clear change-aware enablement. Title editing is not discoverable from the page heading, and post-save scroll behavior is not tuned for the two save intents.

This CR improves editing clarity and workflow speed by:
- providing a single page-level Save session edits action;
- enabling save buttons only when form values have changed;
- exposing title edit affordance from the page title;
- applying intentional post-save scroll behavior per save type.

## Scope

In scope:
- Replace per-session save controls with a single Save session edits button for the whole builder page.
- Keep Save session edits disabled until at least one session edit field changes.
- Keep Save setup disabled until setup values change.
- Add pencil icon to the right of the page title to enable title editing.
- Persist edited title through Save setup.
- On Save setup redirect, keep default scroll-to-top.
- On Save session edits redirect, scroll to bottom of page.

Out of scope:
- Rework of add/remove exercise controls.
- Autosave.
- Multi-user merge/conflict handling.

## Acceptance criteria

1. Builder shows one Save session edits button for all session edit fields.
2. Save session edits is disabled when no session values have changed and enabled after any change.
3. Save setup is disabled when no setup values have changed and enabled after any setup change.
4. Page title area shows a pencil icon on the right side of the title.
5. Clicking pencil enables programme title editing.
6. Saving setup persists title, dates, and structure changes.
7. After Save setup, page lands at top.
8. After Save session edits, page lands at bottom.

## Implementation notes

- Contracts/API/infrastructure setup update path extended to include `ProgrammeName` and reject blank names.
- Builder UI uses one hidden form for all session edit inputs via HTML `form` attribute binding.
- Change-aware button state is handled client-side using initial-value tracking.
- Scroll behavior controlled by temporary server flag set on redirect target.

## Risks and mitigations

Risks:
- Button state drifting from real form state.
- Missing edge-case handling when title remains readonly.

Mitigations:
- Unified change detection for setup/session controls.
- Explicit title-edit action that unlocks input and re-evaluates setup save state.
