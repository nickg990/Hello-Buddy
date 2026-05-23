# 5. UI Design Direction

## Visual identity

Use the Hello Buddy mobile visual language for the admin system, but adapt it for a practitioner-facing web app.

Known colour palette:

- Primary blue: `#6392AE`
- Deep blue/navy: `#28404F`
- Light blue: `#B3CDD6`
- Pale blue/grey: `#D4E0E5`
- White: `#FFFFFF`

## Logo

Use the existing Hello Buddy mobile page logo where the asset is available.

If the logo asset is not present in the implementation repo, create a placeholder `LogoDisc` component that reserves the correct space and can later be replaced by the real logo.

Do not invent a completely different brand identity.

## Tone

The UI should feel:

- calm;
- clinical but friendly;
- trustworthy;
- uncluttered;
- easy for a busy practitioner to use quickly;
- pet-owner friendly when generating PDF output.

Avoid a playful design that undermines clinical trust. The owner-facing PDF can be warmer than the practitioner admin screens.

## Layout style

Use:

- rounded cards;
- soft spacing;
- clear headings;
- accessible contrast;
- restrained use of colour;
- clear primary actions;
- minimal decoration;
- plenty of white/pale background space.

## Suggested component style

### Header

- deep blue or white header;
- Hello Buddy logo at left;
- current practitioner/user on right;
- optional environment badge such as Development.

### Navigation

- deep blue left rail on desktop;
- active item highlighted using primary blue/light blue;
- icons optional but not required;
- labels must always be visible in the first implementation.

### Cards

- white background;
- rounded corners;
- subtle border using pale blue/grey;
- title, summary and action area;
- avoid dense dashboards.

### Forms

- labels above fields;
- one clear primary action per form;
- secondary action as outline/ghost button;
- inline validation;
- required fields marked clearly;
- avoid excessive multi-column layout in the first implementation.

### Tables/lists

- use tables for desktop list views;
- use card layouts on narrow screens;
- include search and simple filters;
- avoid complicated grid components initially.

### Programme builder

The programme builder should use session cards.

Each session card should include:

- session name: AM, PM or Single;
- objective/notes if available;
- ordered exercise cards;
- add exercise button.

Each exercise card should include:

- exercise title;
- prescription summary;
- video link indicator;
- edit button;
- remove button for draft only;
- drag/reorder option later.

## Accessibility

- All form fields must have labels.
- Buttons must have descriptive text.
- Do not rely on colour alone to show status.
- Maintain readable contrast.
- Use semantic HTML for headings, main content and navigation.
- Support keyboard navigation for forms and primary flows.

## Owner-facing PDF visual direction

The PDF should use the same Hello Buddy brand but be simpler and warmer:

- logo at top;
- pet name and programme dates clearly visible;
- practitioner/practice details;
- session sections;
- exercise cards or rows;
- clear instructions;
- video links displayed as clickable text;
- enough spacing for readability when printed;
- avoid making the PDF look like a database export.

## Implementation note for Claude

In the first build increment, focus on visual structure and workflow clarity rather than pixel-perfect design.

Use reusable components such as:

- `AppShell`
- `PageHeader`
- `LogoDisc`
- `DashboardCard`
- `OwnerCard`
- `PetSummaryCard`
- `CaseSummaryCard`
- `ExerciseCard`
- `SessionCard`
- `ProgrammeBuilder`
- `PdfPreview`
