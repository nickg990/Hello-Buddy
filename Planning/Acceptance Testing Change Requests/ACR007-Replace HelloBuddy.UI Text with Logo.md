# ACR007 - Replace "HelloBuddy.UI" Text with Logo

Date: 2026-06-09
Updated: 2026-06-10
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Branding / layout header)

## Why this change

The application currently displays the text "HelloBuddy.UI" (the project/module name) in the UI.
This should be replaced with a proper brand logo. The logo asset has been supplied:
`Designs/hello-buddy-logo-disc.svg`.

## Scope

In scope:
- Remove the "HelloBuddy.UI" text from the UI (header/brand area).
- Display a brand logo in its place.
- Accommodate the logo asset to be supplied separately.

Out of scope:
- Full rebrand of colours/typography across the app.
- Favicon and external document branding (unless specified later).

## Acceptance criteria

1. The "HelloBuddy.UI" text no longer appears in the UI.
2. A logo image is displayed in the brand/header area.
3. The logo renders at an appropriate size and is not distorted.
4. The logo links to the home/landing screen (if consistent with existing header behavior).

## Implementation notes (as built)

The "Hello Buddy" brand logo disc was recreated from the Canine Physio App `LogoDisc`
control (Components/Controls) as a self-contained, scalable SVG and applied to the admin
header.

- The navbar brand text was replaced with an `img` referencing the logo SVG, linking to Home
  and carrying `alt="Hello Buddy"` for accessibility.
  - File: `src/HelloBuddy.Ui/Views/Shared/_Layout.cshtml`
- The logo asset is a self-contained SVG (embeds the dog logo as base64; brand disc, white
  ring with faint grey border, graded inner shading, and graded outer drop shadow; gold
  "GETTING BETTER" tagline).
  - File: `src/HelloBuddy.Ui/wwwroot/img/logo-disc.svg`
  - Design reference copy: `Designs/hello-buddy-logo-disc.svg`
- Header sizing is controlled by a `.brand-logo` rule (height 48px, width auto) so the logo
  fits the top bar and preserves aspect ratio.
  - File: `src/HelloBuddy.Ui/wwwroot/css/site.css`

## Open items

- None. The logo is implemented using the in-house brand disc; no external asset is required.
  If the client later supplies an alternative asset, swap `logo-disc.svg` and adjust
  `.brand-logo` height if needed.

## Risks and mitigations

Risks:
- Sizing/aspect ratio issues in the header.

Mitigations:
- Logo is constrained with a fixed `height` and `width:auto`, preserving aspect ratio.
- SVG is resolution-independent, so it stays crisp at any header height.
