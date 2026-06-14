# ACR016 - Remove "#<number>" Record Identifiers from Page Sub-headers

Date: 2026-06-12
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (all pages displaying a raw record id)

## Why this change

Several pages show the internal database record id in the sub-header (e.g. "Case #12",
"Owner #5"). These technical identifiers are distracting and add no value for the practitioner.
They must be removed from all user-facing pages.

## Scope

In scope:
- Remove the `#<number>` record-id text from page sub-headers/meta lines across the UI.

Out of scope:
- Removing ids from URLs/routes (these stay; only the visible "#<number>" text is removed).
- The PDF document template's internal subhead (see note below — confirm with product if it
  should also drop the "Programme #" text; default: yes, remove for consistency).

## Known occurrences (remove the "#<number>" portion only)

- `src/HelloBuddy.Ui/Views/CaseDetail/Index.cshtml` — `Case #@Model.Case.TreatmentCaseId · ...`
  → remove the `Case #<id> · ` prefix, keeping the status and remaining meta.
- `src/HelloBuddy.Ui/Views/Owners/Details.cshtml` — `Owner #@Model.OwnerId` → remove the line (or
  replace with non-id content if a sub-header is still wanted).
- `src/HelloBuddy.Ui/Views/Pets/Details.cshtml` — `Pet #@Model.PetId` → remove the line.
- `src/HelloBuddy.Ui/Views/Programmes/_ProgrammePreviewDocument.cshtml` —
  `@Model.CaseTitle - Programme #@Model.ProgrammeId - @Model.Status` → remove the
  `- Programme #<id>` portion, keeping case title and status (confirm with product).

> The implementing agent must also grep the views for any other `#@Model...Id` / `#@...Id`
> occurrences and remove them, since the list above may not be exhaustive.

## Acceptance criteria

1. No user-facing page displays a raw "#<number>" database record identifier in its header/meta.
2. Surrounding meaningful content (status, titles, dates, owner/pet names) is preserved and the
   layout still reads cleanly (no stray separators like a leading "· ").
3. Routing and functionality are unchanged (ids remain in URLs).
4. Implementation conforms to coding standards; UI suite remains green.

## Implementation guidance

- Edit each Razor view to delete the `#<number>` token and tidy any now-redundant separator
  (e.g. a leading `· ` or trailing ` - `).
- Grep across `src/HelloBuddy.Ui/Views/**/*.cshtml` for `#@` to catch any further occurrences.

## Risks and mitigations

Risks:
- Leaving a dangling separator/punctuation after removing the id.

Mitigations:
- Review each edited sub-header for clean punctuation; update/extend UI smoke assertions if any
  assert on the old "#id" text.

## Verification

- Manual check of Case, Owner, Pet detail pages and the PDF preview header.
- Full solution builds clean; UI suite green.
