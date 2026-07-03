# ACR002 - Add Note Must Persist and Display in Case Notes Panel

Date: 2026-06-09
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Case detail / Case notes)

## Why this change

When a practitioner enters a note and clicks Add note, the note is not reliably pushed into the
case notes panel. Notes must be added to the visible case notes panel immediately on clicking
Add note, and must be stored so that the case notes remain visible whenever the case is opened.

## Scope

In scope:
- On clicking Add note, the new note is added to the case notes panel.
- The note is persisted (stored) against the case.
- Stored case notes are always visible in the case notes panel when the case is viewed.
- The note entry field clears/readies for the next note after a successful add.

Out of scope:
- Editing or deleting existing notes (unless already in scope elsewhere).
- Rich-text or attachment support in notes.
- Note categorisation/tagging.

## Acceptance criteria

1. Clicking Add note adds the entered note to the case notes panel without losing other content.
2. The added note is stored against the case (survives reload/navigation).
3. Reopening the case shows all previously stored notes in the case notes panel.
4. An empty note is not added (basic validation).
5. After a successful add the input is ready for the next note.

## Implementation notes (proposed)

- Ensure the Add note action posts to the case notes persistence path and then re-renders the
  notes panel from stored data.
- Confirm the notes panel reads from persisted notes on case load, not only from in-memory state.
- Keep note ordering consistent (for example newest first or chronological) per existing UX.

## Risks and mitigations

Risks:
- UI updates without persistence (note appears then disappears on reload).
- Race between add and panel refresh.

Mitigations:
- Persist first, then refresh the panel from the stored source of truth.
- Add API/UI test coverage for add-and-reload behavior.
