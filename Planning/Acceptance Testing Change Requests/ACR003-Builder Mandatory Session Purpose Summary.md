# ACR003 - Builder Session Purpose Summary (Replace Duplicated Notes)

Date: 2026-06-09
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Programme Builder)

## Why this change

In the Builder screen, notes currently appear both as notes and again in the session area, which
is duplicative and confusing. Sessions do not need notes repeated; instead each session needs a
short summary describing the purpose of that session's exercises. This summary should be entered
in the Builder session area and is mandatory.

## Scope

In scope:
- Stop duplicating notes into the Builder session area.
- Add a per-session purpose summary field in the Builder session area that describes the purpose
  of the exercises in that session.
- Make the session purpose summary mandatory (cannot save/publish the session without it).
- Persist the session purpose summary with the session.

Out of scope:
- Removing or changing case notes elsewhere (case notes remain as their own concept).
- Per-exercise descriptions beyond the session-level summary.

## Acceptance criteria

1. Notes no longer appear duplicated in the Builder session area.
2. Each session in the Builder has a purpose summary field for describing the exercises' purpose.
3. The session purpose summary is mandatory and blocks save/publish when empty, with a clear
   validation message.
4. The entered session purpose summary is stored and reloads with the session.
5. The session purpose summary is associated with the correct session (independent per session).

## Implementation notes (proposed)

- Remove the note rendering currently injected into the session area; keep notes only where they
  legitimately belong.
- Add a `PurposeSummary` (or equivalent) field per session in the Builder model and view.
- Enforce required validation server-side and client-side before allowing save/publish.

## Risks and mitigations

Risks:
- Existing sessions without a summary may block save after the rule is introduced.
- Confusion between case notes and the new session summary concept.

Mitigations:
- Provide clear field labelling ("Purpose of this session's exercises").
- Consider a migration/default prompt for pre-existing sessions during validation.
