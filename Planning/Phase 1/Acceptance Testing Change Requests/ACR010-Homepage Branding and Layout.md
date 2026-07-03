# ACR010 - Homepage Branding and Layout (Sub-header, Logo Disc, Remove Learn-About)

Date: 2026-06-10
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Home / landing page)

## Why this change

The homepage should present a clear, branded landing experience. The default "learn about"
placeholder text and link are not relevant to the product, and the page needs a descriptive
sub-header, supporting spacing, and the brand logo disc to reinforce identity.

## Scope

In scope:
- Remove the "learn about" text and its link from the homepage.
- Add a sub-header (H2, given "Hello Buddy" is the H1) directly under the "Hello Buddy" heading
  with the text: "Canine Physiotherapy Administration".
- Add buffer spacing below the sub-header.
- Add the logo disc to the homepage below the sub-header/spacing.
- Ensure the homepage layout is responsive across supported breakpoints.
- Ensure all code follows the project coding standards.

Out of scope:
- Changes to other pages or global navigation.
- New homepage feature tiles or dashboards (unless requested later).

## Acceptance criteria

1. The "learn about" text and link no longer appear on the homepage.
2. An H2 sub-header reading "Canine Physiotherapy Administration" appears directly under the
   "Hello Buddy" H1.
3. There is clear buffer spacing below the sub-header.
4. The logo disc is displayed on the homepage below the sub-header/spacing.
5. The homepage renders correctly and remains readable/usable on small, medium, and large
   viewport widths (responsive).
6. Implementation conforms to the coding standards (no inline styles where a CSS class is
   appropriate, semantic headings, accessible image alt text).

## Implementation notes (proposed)

- Update the Home index view to remove the default template "learn about" content and link.
- Add the H2 sub-header under the existing H1 with the specified text.
- Use a CSS class (not inline styles) for the buffer spacing below the sub-header, per standards.
- Reuse the existing logo disc asset (`src/HelloBuddy.Ui/wwwroot/img/logo-disc.svg`) with
  appropriate alt text and responsive sizing.
- Verify responsiveness with Bootstrap grid/utility classes already used in the project.

## Risks and mitigations

Risks:
- Logo disc sizing could dominate the page on small screens.
- Inline styling could creep in for spacing/sizing.

Mitigations:
- Use responsive utility/CSS classes for sizing and spacing.
- Keep all sizing/spacing in `site.css` (or equivalent) per coding standards.
