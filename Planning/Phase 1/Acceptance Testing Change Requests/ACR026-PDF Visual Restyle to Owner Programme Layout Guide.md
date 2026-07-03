# ACR026 - Restyle the Owner Programme PDF to the Approved Layout Guide

Date: 2026-06-14
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (programme PDF template — `HelloBuddy.Admin.Pdf`)

## Why this change

The published owner programme PDF currently uses a plain, table-based layout. A designed layout
guide now exists at `Designs/PDF/owner_programme_pdf.svg` showing the intended Hello Buddy branded
appearance (dark header band, branded summary band, "Before you start" panel, per-session bands,
and boxed left-hand exercise media with the image acting as the video link). The PDF must be
restyled to **look like** that guide.

Critically, this is a **visual restyle only**. The **content and the content layout stay exactly as
they are today** — the same data fields, the same per-session grouping, the same exercise
information, and the same click-the-image-to-open-the-video behaviour. The SVG is a styling
reference for colours, spacing, banding, and typography, **not** a content specification.

## What changes vs. what stays

Changes (visual styling, taken from the SVG guide):

- Apply the branded **header band** (dark `#28404F` background) with the Hello Buddy mark/initials
  disc, the programme/clinic title in white, and the existing sub-line text.
- Apply the branded **summary band** (`#D9E7EC` panel with `#B9CBD4` border) for the
  programme title, version/date line, and pet/owner meta that the PDF already shows.
- Apply the **"Before you start" panel** styling (light `#F8FBFC` panel) — using whatever
  intro/instruction content the template already renders (do not invent new clinical copy).
- Apply per-session **band headings** (the `#D9E7EC` band style) in place of the current
  underlined `<h2>`.
- Apply the **boxed exercise rows** styling with the left-hand media thumbnail box
  (`#D4E0E5` fill, `#6392AE` border) and the right-hand exercise info column.
- Adopt the guide's serif-led heading treatment and colour palette (`#223843` body text,
  `#5A6E7A` muted text, `#245E7C` link colour) as appropriate for print.
- Apply the branded **footer** styling.

Stays exactly as now (content and content layout):

- The set of data fields rendered for the programme, pet, owner, sessions, and exercises is
  unchanged (programme name, case title, status, pet, owner, start/end dates, per-session period
  and objective, per-exercise title, objective summary, reps, sets, hold seconds).
- The grouping of exercises under sessions is unchanged.
- **Clicking the exercise image opens the video** — keep the existing behaviour where the
  thumbnail image is wrapped in an anchor to the video URL (ACR005 / ERR-AT-006). The image is the
  link; there is no separate text video link when an image is present.
- The fallback rendering when there is no image and/or no video stays consistent (a placeholder
  box that still renders).

Explicitly requested content change (the only one):

- **Drop the exercise numbering.** Remove the numbered circle/index shown per exercise in the
  guide and any existing numeric prefix. Exercises render without an ordinal number.

Out of scope:

- Pixel-for-pixel reproduction of the SVG, exact dummy text, or the SVG's sample data values.
- Re-ordering or adding/removing data fields.
- Changing the publish/versioning flow, the rendering pipeline (PuppeteerSharp), or the contracts
  (`ProgrammeVm`).
- Changing the on-screen PDF Viewer/Preview HTML (this ACR is about the generated PDF document).
  Note any deliberate parity desired with the on-screen preview as a follow-up if required.

## Reference assets

- Layout guide: `Designs/PDF/owner_programme_pdf.svg` (visual reference only).
- Template to edit: `src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml` (embedded Razor template
  compiled by `RazorProgrammePdfTemplate`; rendered to PDF via the PuppeteerSharp renderer).
- Model: `HelloBuddy.Contracts.ProgrammeVm` (fields available: `ProgrammeName`, `CaseTitle`,
  `ProgrammeId`, `Status`, `PetName`, `OwnerName`, `StartDate`, `EndDate`, and `Sessions` each with
  `SortOrder`, `Period`, `Objective`, and `Exercises` each with `ImageUrl`, `VideoUrl`,
  `ExerciseTitle`, `ObjectiveSummary`, `Reps`, `Sets`, `HoldSeconds`).

## Acceptance criteria

1. The generated PDF visually matches the structure and palette of
   `Designs/PDF/owner_programme_pdf.svg`: dark header band, branded summary band, intro panel,
   per-session bands, and boxed exercise rows with a left-hand media box.
2. All content fields currently shown in the PDF are still present, with the same grouping and
   reading order; no fields are added or removed (other than numbering — see #4).
3. When an exercise has an image, the image renders inside the left media box and **clicking the
   image opens the exercise video** in the reader/browser. When there is an image but no video,
   the image renders without a link. When there is no image, the box still renders (with the
   existing placeholder/fallback), and any video is reachable via the existing fallback.
4. Exercise numbering is removed entirely — no numbered disc, no ordinal prefix on exercise rows.
5. The PDF renders correctly on A4, paginates sensibly (sessions/rows avoid awkward splits using
   the existing `break-inside: avoid` approach), and prints legibly in greyscale.
6. The branded footer renders with the existing generation metadata.
7. Rendering remains server-side via the existing PuppeteerSharp pipeline; no new runtime
   dependency is introduced. Embedded-resource template wiring is preserved (the template stays an
   `<EmbeddedResource>` and the resource name is unchanged, or `RazorProgrammePdfTemplate` is
   updated in lockstep if it is renamed).
8. Implementation conforms to the coding standards; full solution builds clean
   (warnings-as-errors); PDF and related suites green.

## Implementation guidance

- Edit `src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml` only (plus its CSS in the `<style>`
  block). Do not change `ProgrammeVm` or the renderer.
- Translate the SVG's `defs`/CSS classes into print CSS in the template's `<style>` block:
  - `.header` → dark band `#28404F`, white title text; include the HB mark/initials disc.
  - `.band` → `#D9E7EC` fill, `#B9CBD4` 1px border (summary band + per-session headings).
  - `.panel` → `#F8FBFC` fill, `#B9CBD4` border (intro + exercise rows).
  - `.thumb` → media box `#D4E0E5` fill, `#6392AE` border, fixed square, image scaled to fit.
  - `.link` → `#245E7C`; `.body` `#223843`; `.muted`/`.small` `#5A6E7A`.
  - Heading sizes per the guide (`.h1` ~28px, `.h2` ~20px, `.h3` ~18px) tuned to A4 print points.
- Keep the existing image-as-video-link markup pattern (anchor wrapping the `<img>`), and keep the
  no-image / no-video fallbacks. Remove the numbered element only.
- Preserve `@@page { size: A4; margin: 0; }` and `break-inside: avoid` for sessions/rows.
- Use inline/embedded CSS within the template (PuppeteerSharp renders a self-contained HTML
  string); do not reference external stylesheets that will not be available to the PDF service.

## Risks and mitigations

Risks:

- Over-fitting to the SVG's sample text/values and accidentally changing real content.

Mitigations:

- Treat the SVG strictly as a styling reference; bind every visible value from `ProgrammeVm`
  exactly as the current template does.

Risks:

- Pagination regressions (rows or session bands splitting across pages) after restyle.

Mitigations:

- Retain `break-inside: avoid` on session blocks/rows; verify a multi-session, multi-exercise
  programme renders across pages cleanly.

Risks:

- Brand colours reducing print legibility in greyscale.

Mitigations:

- Keep body text near-black (`#223843`) and ensure header band text stays white; verify a
  greyscale print proof.

## Verification

- Render a published PDF for a representative programme (multiple sessions, exercises with and
  without images/videos) and compare side-by-side with `Designs/PDF/owner_programme_pdf.svg` for
  structure and palette.
- Click an exercise image in the rendered PDF and confirm it opens the correct video URL.
- Confirm no exercise numbering appears anywhere.
- Confirm all previously shown fields are still present and correctly bound.
- Full solution builds clean (warnings-as-errors); suites green.
