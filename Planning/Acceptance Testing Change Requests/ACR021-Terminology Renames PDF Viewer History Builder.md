# ACR021 - Terminology and Label Renames (PDF Viewer / PDF History / PDF Builder / New Programme)

Date: 2026-06-12
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Programme-related page titles, headings, buttons, links)

## Why this change

The programme PDF pages use inconsistent and unclear terminology. Standardise the naming so the
three PDF-related screens are clearly related and the draft-creation action reads naturally.

## Required renames

Apply consistently everywhere the labels appear (page titles, `<h1>` headings, nav/menu items,
buttons, links, and `ViewData["Title"]`):

1. **Preview → PDF Viewer**
   - The Preview page title and `<h1>` "Preview PDF" → **PDF Viewer**.
   - Any button/link labelled "Preview" or "Preview PDF" → **PDF Viewer**.
2. **History → PDF History**
   - The version-history page title/`<h1>` "Version history" → **PDF History**.
   - Any button/link labelled "History" or "View version history" → **PDF History**.
3. **Builder → PDF Builder**
   - The Builder page title and any button/link labelled "Builder" → **PDF Builder**.
4. **"Create draft programme" → "New programme"**
   - The button on the Case detail page labelled "Create draft programme" → **New programme**
     (and the adjacent helper text updated to match, e.g. "Create a programme for this case…").

## Acceptance criteria

1. The PDF Viewer page shows the title/heading **PDF Viewer** (no "Preview"/"Preview PDF" text
   remains).
2. The version-history page shows **PDF History** (no "History"/"Version history" text remains in
   labels/titles).
3. The builder page shows **PDF Builder** (no bare "Builder" label remains in titles/buttons).
4. The Case detail programme action reads **New programme** (no "Create draft programme" remains),
   with consistent helper text.
5. Route names/action names in code are unchanged (only user-facing labels change); links still
   resolve.
6. Implementation conforms to coding standards; suites green.

## Implementation guidance

Known locations (grep for each label to find any others):
- `src/HelloBuddy.Ui/Views/Programmes/Preview.cshtml` — `ViewData["Title"] = $"Preview: ..."` and
  `<h1 ...>Preview PDF</h1>` → PDF Viewer.
- `src/HelloBuddy.Ui/Views/Programmes/History.cshtml` — `ViewData["Title"] = "Programme version
  history"` and `<h1 ...>Version history</h1>` → PDF History.
- `src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml` — `ViewData["Title"] = $"Builder: ..."`
  and any "Builder" label → PDF Builder.
- `src/HelloBuddy.Ui/Views/CaseDetail/Index.cshtml`:
  - `<button ...>Create draft programme</button>` → **New programme**, plus the helper paragraph
    "Create a draft programme for this case or open an existing builder." updated accordingly.
  - The per-programme action links labelled `Builder` / `Preview` / `History` →
    **PDF Builder** / **PDF Viewer** / **PDF History**.
- Any back-links or breadcrumb labels referencing these names.
- Update UI smoke test assertions that match on the old label strings.

> Important: change **display text only**. Do not rename controller actions, routes
> (`asp-action="Preview"`, `"History"`, `"Builder"`), or `Url.Action(...)` names — only the
> visible labels/titles.

## Risks and mitigations

Risks:
- Renaming action/route names by mistake would break links.
- Tests asserting on old label text will fail.

Mitigations:
- Restrict edits to display strings; update affected test assertions in the same change.

## Verification

- Manual: the three pages show PDF Viewer / PDF History / PDF Builder; Case detail shows
  "New programme".
- Full solution builds clean; suites green (label assertions updated).
