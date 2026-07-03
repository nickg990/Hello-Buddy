# ACR022 - Consistent Button Styling Across the Application

Date: 2026-06-12
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (all pages — button styling conventions)

## Why this change

Buttons across the application currently use an inconsistent mix of Bootstrap variants (solid
`btn-primary`, `btn-outline-primary`, `btn-outline-secondary`, `btn-success`, etc.) for similar
roles. This is visually inconsistent. Buttons should follow a single, predictable convention based
on their role.

## Convention to standardise on

- **Primary action** (the main, affirmative action on a page/form/modal — e.g. Save, Create,
  the page's principal "go" action): solid **`btn-primary`**.
- **Secondary / Cancel / dismiss** (Cancel, Back, close, low-emphasis navigation): outline
  **`btn-outline-secondary`**.
- **Destructive** (Delete, irreversible actions): outline **`btn-outline-danger`**.
- Size modifiers (`btn-sm`) and icon usage may remain where already appropriate; only the
  **variant/role colouring** is standardised.

> Note: the publish action currently uses `btn-success`. Under this convention, the principal
> action of the Publish confirmation should be `btn-primary` (with the destructive/cancel buttons
> following the rules above). Confirm with product if green should be retained specifically for
> Publish; default is to standardise to `btn-primary`.

## Acceptance criteria

1. Across all pages, primary actions use `btn-primary`, cancel/secondary actions use
   `btn-outline-secondary`, and destructive actions use `btn-outline-danger`.
2. There is no mix of `btn-success` / `btn-outline-primary` / `btn-secondary` for these roles
   (unless a documented exception is agreed for Publish — see note).
3. Each page/modal has at most one visually dominant primary (`btn-primary`) action.
4. Buttons remain accessible (existing `aria-label`/`title` on icon-only buttons preserved) and
   meet the 44×44px touch-target rule from the coding standards.
5. Implementation conforms to coding standards; suites green.

## Implementation guidance

- Audit button classes across `src/HelloBuddy.Ui/Views/**/*.cshtml` and shared partials. Known
  hotspots to review:
  - `Views/Programmes/Builder.cshtml`, `Views/Programmes/Preview.cshtml`,
    `Views/Programmes/History.cshtml` (Publish, Download, View, Delete, Apply, status buttons).
  - `Views/CaseDetail/Index.cshtml` (Create/New programme, Apply, per-programme action links,
    Delete).
  - Owner/Pet/Case/Exercise form pages and their modals (Save / Cancel pairs).
- Apply the role → variant mapping above. Where an action's role is ambiguous, pick the variant
  matching the convention (affirmative submit = primary; cancel/back = outline-secondary;
  delete = outline-danger).
- Consider centralising via consistent classes rather than per-element overrides; do not invent a
  new CSS framework — reuse Bootstrap variants per the coding standards (CSS + Razor partials).

## Risks and mitigations

Risks:
- Over-applying `btn-primary` so multiple buttons compete for emphasis on one screen.

Mitigations:
- Enforce "one dominant primary per page/modal"; demote others to outline-secondary.

## Verification

- Manual visual pass across the main screens and modals confirms consistent variants by role.
- Update any UI smoke assertions that match specific button classes.
- Full solution builds clean; suites green.
