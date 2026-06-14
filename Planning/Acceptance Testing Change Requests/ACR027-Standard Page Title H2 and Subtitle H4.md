# ACR027 - Standard Page Title (H2) with Breadcrumb Subtitle (H4) on Main Pages

Date: 2026-06-14
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (page heading structure on the main pages)

## Why this change

Page headings across the application are inconsistent: most main pages render the record name as a
single `<h1>` with no consistent page-level title or descriptive subtitle. The product requires a
predictable two-line heading on every main page — a fixed **page title** rendered as `<h2>` and a
**breadcrumb trail subtitle** rendered as `<h4>` directly beneath it. This gives users a consistent
"where am I / how did I get here" header on every screen and improves orientation across the
Owner → Pet → Case → Programme hierarchy.

## Required page titles

Each listed page must show a fixed title (H2) and a subtitle (H4) underneath. The titles are:

| # | Page (as named by product) | Title (H2)                   | View (file)                                         |
| - | -------------------------- | ---------------------------- | --------------------------------------------------- |
| 1 | Owner                      | `Owner`                      | `src/HelloBuddy.Ui/Views/Owners/Details.cshtml`     |
| 2 | Pets                       | `Pets`                       | `src/HelloBuddy.Ui/Views/Pets/Index.cshtml`         |
| 3 | Pet Details                | `Pet Details`                | `src/HelloBuddy.Ui/Views/Pets/Details.cshtml`       |
| 4 | Treatment Case             | `Treatment Case`             | `src/HelloBuddy.Ui/Views/CaseDetail/Index.cshtml`   |
| 5 | Treatment Case Details (edit only) | `Treatment Case Details` | `src/HelloBuddy.Ui/Views/Cases/Edit.cshtml`    |
| 6 | Exercise Programme Builder | `Exercise Programme Builder` | `src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml` |
| 7 | PDF Viewer                 | `PDF Viewer`                 | `src/HelloBuddy.Ui/Views/Programmes/Preview.cshtml` |
| 8 | PDF History                | `PDF History`                | `src/HelloBuddy.Ui/Views/Programmes/History.cshtml` |

> **Resolved (CQ-1):** "Exercise Programme" and the Builder are the **same page**. The Builder page
> H2 title becomes **`Exercise Programme Builder`**. There is therefore no separate "Exercise
> Programme" page. The read-only programme document page remains **PDF Viewer** (ACR021).
>
> **Button label vs page title:** On the Treatment Case page, the entry-point button that opens the
> Builder must **stay labelled `Builder`** (to save horizontal space). Only the **page heading**
> (the H2 on the Builder page itself, and `ViewData["Title"]` / breadcrumb leaf) uses the full
> `Exercise Programme Builder` wording. Do not rename the case-page button.

## Subtitle (H4) content — navigable breadcrumb trail

**Resolved (CQ-2):** The H4 subtitle is implemented as a **navigable breadcrumb trail** to aid
orientation, rendered in the `<h4>` slot directly under the H2. Each breadcrumb item is separated by
a **space, a dot, and a space** (`" . "`). **Do not** include any `#<number>` record identifier in
the breadcrumb (raw ids are confusing — consistent with ACR016); use names only.

**Every ancestor item is a clickable link** that navigates to that item's page; only the **leaf**
(the current page) is plain, non-link text. Clicking any item between the dots returns the user to
that page.

> Separator: a literal middot/period surrounded by single spaces, e.g.
> `Mrs S. Patel . Bella . Right hind cruciate post-op rehabilitation`, where `Mrs S. Patel`
> (Owner) and `Bella` (Pet) are links and the case title is the current page (plain text).

Breadcrumb trail per page (built from data already on the page — except the ids/names noted below):

| Page                       | Breadcrumb trail (H4), items separated by `" . "`                                   |
| -------------------------- | ----------------------------------------------------------------------------------- |
| Owner                      | `Owners . <Owner full name>`                                                        |
| Pets                       | `Pets` (single item; or simply omit a trail and show `Pets`)                        |
| Pet Details                | `Pets . <Pet name>`                                                                 |
| Treatment Case             | `<Owner name> . <Pet name> . <Case title>`                                          |
| Treatment Case Details (edit) | `<Owner name> . <Pet name> . <Case title> . Edit`                              |
| Exercise Programme Builder | `<Owner name> . <Pet name> . <Case title> . <Programme name>`                       |
| PDF Viewer                 | `<Owner name> . <Pet name> . <Case title> . <Programme name> . PDF Viewer`          |
| PDF History                | `<Owner name> . <Pet name> . <Case title> . <Programme name> . PDF History`         |

### Link target for each breadcrumb item

Each item (except the current-page leaf) links to a fixed route. Targets and the route id required:

| Breadcrumb item   | Links to (controller/action) | Route id needed        |
| ----------------- | ---------------------------- | ---------------------- |
| `Owners`          | `Owners/Index`               | none                   |
| `<Owner name>`    | `Owners/Details`             | `OwnerId`              |
| `Pets`            | `Pets/Index`                 | none                   |
| `<Pet name>`      | `Pets/Details`               | `PetId`                |
| `<Case title>`    | `CaseDetail/Index`           | `TreatmentCaseId`      |
| `<Programme name>`| `Programmes/Builder`         | `ProgrammeId`          |
| Leaf (`Edit` / `PDF Viewer` / `PDF History` / current record) | — (plain text) | —     |

### Data available vs. data to add (so every ancestor is clickable)

- **Pet Details** (`PetDetailVm`): has `Name`, `OwnerId`, `OwnerName`, `PetId` — the `<Owner name>`
  item can link to `Owners/Details` via `OwnerId`. Default trail is `Pets . <Pet name>`; if the
  deeper `Owners . <Owner name> . <Pet name>` trail is preferred, all items are already linkable.
- **Treatment Case** (`CaseDetailVm`): has `OwnerName`, `PetId`, `PetName`, `CaseTitle` but **no
  `OwnerId`**. To make the `<Owner name>` item navigable, **add `OwnerId` to `CaseDetailVm`** and
  populate it in the case projection. `<Pet name>` links via `PetId`; `<Case title>` is the leaf.
- **Builder / PDF Viewer** (`ProgrammeVm`): has `OwnerName`, `PetName`, `CaseTitle`,
  `TreatmentCaseId`, `ProgrammeName`, `ProgrammeId` but **no `OwnerId` and no `PetId`**. To make the
  `<Owner name>` and `<Pet name>` items navigable, **add `OwnerId` and `PetId` to `ProgrammeVm`**
  and populate them in the programme projection. `<Case title>` links via `TreatmentCaseId`. On the
  Builder page the programme is the leaf; on PDF Viewer the `<Programme name>` links to
  `Programmes/Builder` and `PDF Viewer` is the leaf.
- **PDF History** (`ProgrammeVersionHistoryVm`): currently exposes `ProgrammeName` and
  `TreatmentCaseId` only. **Option (a) — chosen:** extend it with `OwnerName`, `PetName`,
  `CaseTitle`, **plus `OwnerId` and `PetId`** so every ancestor (`<Owner name>`, `<Pet name>`,
  `<Case title>`, `<Programme name>`) is clickable and `PDF History` is the leaf (see implementation
  guidance for the mapping change).

> The required id additions (`OwnerId` on `CaseDetailVm`; `OwnerId` + `PetId` on `ProgrammeVm`;
> `OwnerName`/`PetName`/`CaseTitle` + `OwnerId`/`PetId` on `ProgrammeVersionHistoryVm`) are all
> reachable from existing navigation properties in the repositories (no extra round-trips). See
> implementation guidance.

The previously-shown record name that currently sits in the `<h1>` is represented by the **leaf**
breadcrumb item; the fixed page title takes the H2 slot above the breadcrumb.


## Scope

In scope:

- Replace the current top heading on each listed page so the fixed page title renders as `<h2>` and
  a breadcrumb trail renders as `<h4>` directly beneath it.
- Keep `ViewData["Title"]` (browser tab title) sensible; it may differ from the on-page H2.

Out of scope:

- Pages not in the list (Home, Owners list, Exercise library, Account, Admin) unless product later
  extends this; do not change their headings under this ACR.
- The card/section sub-headings inside pages that already use `<h2 class="h4">` for panels (e.g.
  "Patient details", "Owner contact"). These are section headings, not the page title; leave them,
  but ensure the new page H2 is visually dominant over them (see AC #5).
- Removing record id numbers from sub-headers — already covered by ACR016.

## Acceptance criteria

1. Each page listed in the table renders its fixed title as a single `<h2>` element with the exact
   title text shown (the Builder page title is `Exercise Programme Builder`).
2. Directly beneath the H2, each page renders an `<h4>` **breadcrumb trail** for the current record
   (per the breadcrumb table), with items separated by a space-dot-space (`" . "`).
3. The breadcrumb contains **no** `#<number>` record identifier — names only.
4. **Every ancestor breadcrumb item is a working link** to its page (Owner → `Owners/Details`,
   Pet → `Pets/Details`, Case → `CaseDetail/Index`, Programme → `Programmes/Builder`, `Owners`/`Pets`
   roots → their list pages). Clicking any item between the dots navigates to that page. Only the
   leaf (current page) item is plain, non-link text.
5. There is exactly one H2 page title per page; the existing record-name `<h1>` is removed or
   demoted so it is not duplicated (its value becomes the breadcrumb leaf).
6. The Treatment Case page's entry-point button that opens the Builder remains labelled `Builder`
   (unchanged); only the Builder page heading uses the full `Exercise Programme Builder` wording.
7. Heading order is logical for accessibility (the page title H2 precedes any in-page section
   headings); no heading levels are skipped in a way that breaks document outline.
8. The H2 page title is visually the dominant heading on the page; existing panel sub-headings
   (which use the `.h4` Bootstrap size) remain visually subordinate.
9. Existing back navigation, action buttons, and toasts on these pages continue to work and are not
   displaced by the heading change.
10. Implementation conforms to the coding standards; full solution builds clean
    (warnings-as-errors); UI suite green (update any smoke assertions that match the old `<h1>`).

## Implementation guidance

- In each listed view, replace the current heading block. Typical pattern today:

  ```cshtml
  <h1>@Model.Name</h1>
  ```

  becomes an H2 page title plus an H4 breadcrumb trail, for example Pet Details:

  ```cshtml
  <h2>Pet Details</h2>
  <h4 class="text-muted page-breadcrumb">
      <a asp-controller="Pets" asp-action="Index">Pets</a>
      <span class="sep"> . </span>
      <span>@Model.Name</span>
  </h4>
  ```

  and Treatment Case (full trail with **every ancestor linked**, leaf as plain text):

  ```cshtml
  <h2>Treatment Case</h2>
  <h4 class="text-muted page-breadcrumb">
      <a asp-controller="Owners" asp-action="Details" asp-route-id="@Model.Case.OwnerId">@Model.Case.OwnerName</a>
      <span class="sep"> . </span>
      <a asp-controller="Pets" asp-action="Details" asp-route-id="@Model.Case.PetId">@Model.Case.PetName</a>
      <span class="sep"> . </span>
      <span>@Model.Case.CaseTitle</span>
  </h4>
  ```

  (`@Model.Case.OwnerId` requires the `OwnerId` addition to `CaseDetailVm` — see the data section.)
  On the Builder / PDF Viewer pages the owner and pet items link the same way using the
  `ProgrammeVm` `OwnerId` / `PetId` additions, the case links via `TreatmentCaseId`, and the
  programme name links to `Programmes/Builder` via `ProgrammeId` (on PDF Viewer/PDF History).

- **Builder page:** set the H2 to `Exercise Programme Builder` and `ViewData["Title"]` accordingly,
  but **do not** change the `Builder` button on the Treatment Case page
  (`src/HelloBuddy.Ui/Views/CaseDetail/Index.cshtml`) — that label stays `Builder` to save space.
- **Separator:** render the divider as a literal `" . "` (space, period/middot, space). A small
  helper/partial that joins an ordered list of `(label, href?)` items with the separator is
  acceptable and keeps the markup DRY, but a plain inline approach as above is fine too. Do not pull
  in a new breadcrumb component/library; reuse Bootstrap/plain markup. (Bootstrap's `breadcrumb`
  component uses a `/` divider by default; if used, override the divider to `" . "` via
  `--bs-breadcrumb-divider: ' . ';` — but the requirement is the dot separator, not Bootstrap's
  chrome.)
- **No raw ids:** never emit `#<id>` in the breadcrumb (consistent with ACR016); use names only.
- **View-model id additions (required so every ancestor is clickable):**
  - `CaseDetailVm` (`src/HelloBuddy.Contracts/CaseDetailVm.cs`): add **`OwnerId`** (it already has
    `PetId`). Populate it in the case projection in the records repository
    (`src/HelloBuddy.Infrastructure/Records/RecordRepositories.cs`) from `tc.Pet.OwnerId` /
    `x.Pet.Owner.OwnerId` (the owner navigation is already used there for `OwnerName`/`OwnerEmail`).
  - `ProgrammeVm` (`src/HelloBuddy.Contracts/ProgrammeVm.cs`): add **`OwnerId`** and **`PetId`** (it
    already has `TreatmentCaseId` and `ProgrammeId`). Populate from `p.TreatmentCase.Pet.OwnerId`
    and `p.TreatmentCase.PetId` in the programme projection in `ProgrammeRepository`.
- **PDF History data (option a — required):** extend `ProgrammeVersionHistoryVm`
  (`src/HelloBuddy.Contracts/ProgrammeVersionHistoryVm.cs`) with **`OwnerName`, `PetName`,
  `CaseTitle`, `OwnerId`, `PetId`** so the PDF History page renders the full, fully-linked
  breadcrumb like the other pages. Populate them in the existing projection in
  `ProgrammeRepository.GetVersionHistoryAsync`
  (`src/HelloBuddy.Infrastructure/Programmes/ProgrammeRepository.cs`): the `programmeMeta` query
  already filters on `p.TreatmentCase.PractitionerId`, so the values are reachable via navigation
  properties without an extra round-trip — add to the `Select`:
  `p.TreatmentCase.CaseTitle`, `p.TreatmentCase.PetId`, `p.TreatmentCase.Pet.Name` (PetName),
  `p.TreatmentCase.Pet.OwnerId`, and the owner name
  (`p.TreatmentCase.Pet.Owner.FirstName + " " + p.TreatmentCase.Pet.Owner.LastName`), matching the
  navigation names used elsewhere in this repository. Pass them into the
  `new ProgrammeVersionHistoryVm(...)` constructor. Update the two test constructions of
  `ProgrammeVersionHistoryVm` (`tests/HelloBuddy.Ui.Tests/UiSmokeTests.cs` and
  `ProgrammesControllerTests.cs`) for the new required parameters. Link `<Owner name>` to
  `Owners/Details` (`OwnerId`), `<Pet name>` to `Pets/Details` (`PetId`), `<Case title>` to
  `CaseDetail/Index` (`TreatmentCaseId`), and `<Programme name>` to `Programmes/Builder`
  (`ProgrammeId`), consistent with the Builder/PDF Viewer trails.
- **Update other affected VM constructions / tests:** adding required parameters to `CaseDetailVm`
  and `ProgrammeVm` will touch their projection sites and any test fixtures that construct them;
  update those in the same change set so the solution builds (warnings-as-errors).
- Where a page header currently uses a flex row (e.g. Pet Details, Case detail with action buttons
  on the right), keep the action buttons in place and put the H2 + H4 breadcrumb in the left column
  so the layout is unchanged apart from the heading levels.
- Do not hard-code colours; if the breadcrumb should look muted, use the existing Bootstrap
  `text-muted` utility (already used elsewhere) rather than new CSS.

## Resolved questions

- **CQ-1 (resolved):** "Exercise Programme" and the Builder are the **same page**. The Builder page
  title becomes `Exercise Programme Builder`; the case-page entry button stays `Builder`. There is
  no separate "Exercise Programme" page; the read-only document page remains "PDF Viewer".
- **CQ-2 (resolved):** The H4 subtitle is a **navigable breadcrumb trail** with `" . "` separators
  and **no** `#<number>` identifiers; every ancestor item is a working link and only the leaf is
  plain text (see the breadcrumb and link-target tables).

## Risks and mitigations

Risks:

- Two competing page titles (old `<h1>` left in place alongside the new `<h2>`).

Mitigations:

- Remove/replace the old `<h1>` in each view; verify a single H2 page title per page.

Risks:

- Heading-level changes break UI smoke assertions that match `<h1>`.

Mitigations:

- Update the affected smoke-test selectors/strings as part of the change.

## Verification

- Manual pass across all eight listed pages confirms the H2 title and the H4 breadcrumb trail render
  correctly, in the right order, with `" . "` separators and no `#<number>` ids.
- Confirm the Builder page heading reads `Exercise Programme Builder` while the Treatment Case
  page's button still reads `Builder`.
- Confirm **every** ancestor breadcrumb item is a link and clicking each one navigates to the
  correct page (Owner page, Pet Details, Treatment Case, Builder, and the Owners/Pets roots); the
  leaf (current page) is plain text.
- Accessibility check (heading outline) confirms no broken heading hierarchy.
- Full solution builds clean (warnings-as-errors); UI and API in-memory suites green.
