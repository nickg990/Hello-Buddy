# ACR025 - Use the Canine Physio App Brand Colours for Primary and Secondary Buttons

Date: 2026-06-14
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (global button colour theming)

## Why this change

The admin web application currently renders its primary and secondary buttons using Bootstrap's
default blue/grey palette (`btn-primary`, `btn-outline-secondary`). The companion **Canine Physio
App** (the .NET MAUI mobile client) already defines the agreed Hello Buddy brand palette. The two
products must share a single, consistent brand identity, so the admin buttons must adopt the same
brand colours that the mobile app uses for its primary and secondary buttons.

This change is purely a **colour/theming** change. It does **not** change which Bootstrap variant
class a button carries by role (that role mapping was standardised in ACR022); it re-skins those
variants so `btn-primary` and `btn-outline-secondary` render in the brand colours.

## Source of truth for the colours

The brand tokens are defined in the mobile app's design tokens at
`Canine Physio App/Canine Physio App/Theming/Colors.xaml`. The values to adopt are:

| Token (mobile)      | Hex       | Admin usage                                                        |
| ------------------- | --------- | ------------------------------------------------------------------ |
| `BrandPrimary`      | `#4A7A96` | Primary button background; secondary button border + text          |
| `BrandPrimaryDark`  | `#28404F` | Primary button hover/active/focus background                        |
| `BrandPrimaryLight` | `#B3CDD6` | Optional disabled/hover wash for the secondary (outline) button     |
| `BrandMist`         | `#D4E0E5` | Secondary button hover background wash                               |
| `White` / `TextOnPrimary` | `#FFFFFF` | Primary button text                                            |
| `DisabledSurface`   | `#E0E0E0` | Disabled primary button background                                  |
| `DisabledText`      | `#9E9E9E` | Disabled button text/border                                         |

The mobile app's button definitions in
`Canine Physio App/Canine Physio App/Theming/Controls.xaml` are the behavioural reference:

- **Primary** (`ButtonPrimary`): solid `BrandPrimary` background, white text; hover/pressed uses
  `BrandPrimaryDark`; disabled uses `DisabledSurface` + `DisabledText`.
- **Secondary** (`ButtonSecondary`): transparent/Surface background, `BrandPrimary` border and
  text; hover wash uses `BrandMist`; disabled uses `DisabledSurface` border.

## Scope

In scope:

- Re-skin the Bootstrap **primary** button (`btn-primary`) to the brand primary colours.
- Re-skin the Bootstrap **secondary/outline** button (`btn-outline-secondary`) to the brand
  secondary (outline) colours.
- Cover the default, hover, focus, active, and disabled states for both.

Out of scope:

- Destructive buttons (`btn-outline-danger`) — leave on the existing red per ACR022.
- The Sign In / Sign Out buttons' role mapping (unchanged; they re-skin automatically as they use
  the same variant classes).
- Changing which variant class any button carries (role mapping already set in ACR022).
- Bootstrap link styling, badges, alerts, toasts.

## Acceptance criteria

1. All `btn-primary` buttons render with a `#4A7A96` background and white text in their default
   state across every page (forms, modals, page headers, nav Sign In).
2. On hover, focus, and active/pressed, `btn-primary` buttons render with a `#28404F` background
   and remain legible (white text). A visible focus ring is retained for keyboard users.
3. All `btn-outline-secondary` buttons render with a transparent/white background, a `#4A7A96`
   border, and `#4A7A96` text in their default state.
4. On hover/focus, `btn-outline-secondary` buttons show a subtle brand wash (`#D4E0E5`
   background) with the border/text remaining brand-coloured and legible.
5. Disabled states for both variants are visually distinct (greyed) and do not appear actionable.
6. Text/background colour contrast meets WCAG AA (≥ 4.5:1 for button label text). `#FFFFFF` on
   `#4A7A96` and on `#28404F` both satisfy this; verify the outline text colour on white.
7. No regression to button sizing, padding, the 44×44px touch-target rule, or the role mapping
   from ACR022.
8. Implementation conforms to the coding standards; full solution builds clean
   (warnings-as-errors); UI and API in-memory suites green.

## Implementation guidance

- Implement as a **CSS override layer**, not per-element inline styles or new component classes.
  Add brand button rules to `src/HelloBuddy.Ui/wwwroot/css/site.css` (the existing site stylesheet
  loaded after Bootstrap in `Views/Shared/_Layout.cshtml`), so the overrides win by load order
  without `!important` where avoidable.
- Define the brand hex values once as CSS custom properties (e.g. `--hb-brand-primary: #4A7A96;
  --hb-brand-primary-dark: #28404F; --hb-brand-primary-light: #B3CDD6; --hb-brand-mist: #D4E0E5;`)
  at `:root` in `site.css`, then reference them in the button rules. This keeps a single source of
  truth and mirrors the mobile token approach (DRY).
- Override the relevant Bootstrap button states. At minimum:
  - `.btn-primary` — `background-color`, `border-color`, `color`.
  - `.btn-primary:hover`, `.btn-primary:focus`, `.btn-primary:active`,
    `.btn-primary:focus-visible`, `.btn-primary:disabled`, `.btn-primary.disabled`.
  - `.btn-outline-secondary` and its `:hover`, `:focus`, `:active`, `:focus-visible`,
    `:disabled`/`.disabled` states.
  - Prefer overriding Bootstrap's CSS variables on the button (e.g. `--bs-btn-bg`,
    `--bs-btn-border-color`, `--bs-btn-hover-bg`, `--bs-btn-color`, `--bs-btn-active-bg`) rather
    than hard-setting properties, as the project uses Bootstrap 5 which is CSS-variable driven.
    This is cleaner and covers hover/active automatically.
- Do not introduce a new CSS framework or a Sass build step; reuse the existing plain-CSS
  `site.css` approach already used throughout the project.

## Risks and mitigations

Risks:

- Hard-coding properties instead of Bootstrap CSS variables can leave some states (e.g. active)
  on the old blue.

Mitigations:

- Prefer the Bootstrap `--bs-btn-*` variable override approach so all derived states recolour
  consistently; manually verify hover/focus/active/disabled in the browser.

Risks:

- Insufficient contrast on the outline button text against a white background.

Mitigations:

- `#4A7A96` text on white is ~4.0:1; if it fails AA for normal text, darken the outline text to
  `#28404F` (BrandPrimaryDark) which comfortably passes, keeping the brand border at `#4A7A96`.

## Verification

- Manual visual pass across the main screens and modals (Owners, Pets, Pet Details, Cases, Case
  detail, Exercises, Programme Builder, PDF Viewer, PDF History, Account/Admin) confirms primary
  buttons are brand teal and secondary buttons are brand outline, in all states.
- Confirm WCAG AA contrast for button label text with a contrast checker.
- Full solution builds clean (warnings-as-errors); UI and API in-memory suites green.
