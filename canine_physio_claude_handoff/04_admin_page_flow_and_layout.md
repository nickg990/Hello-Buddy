# 4. Admin Page Flow and Layout

## Navigation model

Use a simple left-hand navigation for desktop/tablet admin use. On narrow screens, collapse navigation into a top menu or drawer.

Primary navigation:

1. Dashboard
2. Owners
3. Pets
4. Treatment Cases
5. Programmes
6. Exercise Library
7. Published PDFs
8. Settings

For the first implementation, include only:

1. Dashboard
2. Owners
3. Treatment Cases
4. Exercise Library
5. Programmes

## Global layout

### Desktop layout

- Left navigation rail.
- Top header with product name, current user and environment label.
- Main content area using cards, tables and forms.
- Primary action button in the top-right of each main page.
- Use breadcrumbs or a clear page title when inside a nested flow.

### Mobile/tablet responsive behaviour

The admin system is primarily for practitioners, likely used on laptop/tablet, but it should remain usable on smaller screens.

- Collapse left navigation into a menu.
- Stack form fields vertically.
- Convert wide tables into cards where needed.
- Keep action buttons visible and labelled clearly.
- Do not require horizontal scrolling for core tasks.

## Page flow

```text
Dashboard
  ├── Owners
  │     ├── Owner list
  │     ├── Owner detail
  │     │     ├── Edit owner
  │     │     ├── Add pet
  │     │     └── Open pet
  │     └── New owner
  │
  ├── Pets
  │     ├── Pet detail
  │     │     ├── Edit pet
  │     │     ├── Treatment cases
  │     │     └── Programmes
  │     └── New pet
  │
  ├── Treatment Cases
  │     ├── Case list
  │     ├── Case detail
  │     │     ├── Case notes
  │     │     ├── Add note
  │     │     ├── Create programme
  │     │     └── View programme history
  │     └── New case
  │
  ├── Exercise Library
  │     ├── Exercise list
  │     ├── Exercise detail
  │     ├── New exercise
  │     └── Edit exercise
  │
  └── Programmes
        ├── Programme list
        ├── Programme builder
        │     ├── Programme details
        │     ├── Session editor
        │     ├── Add exercise
        │     ├── Reorder exercises
        │     └── Save draft
        ├── PDF preview
        └── Publish programme
```

## Dashboard

Purpose: give the practitioner a quick route into active work.

Suggested content:

- count of open treatment cases;
- programmes in draft;
- recently published PDFs;
- recently updated pets/cases;
- quick action buttons:
  - New owner;
  - New pet;
  - New treatment case;
  - New programme;
  - Add exercise.

## Owner list

Fields/columns:

- owner name;
- phone;
- email;
- number of pets;
- last updated;
- active cases indicator.

Actions:

- search by owner name, pet name, email or phone;
- create owner;
- open owner detail.

## Owner detail

Sections:

- owner contact card;
- pets owned;
- related treatment cases through pets;
- recent PDFs/programmes;
- edit owner action;
- add pet action.

## Pet detail

Sections:

- pet profile card;
- owner contact summary;
- practitioner assignment;
- open treatment case;
- programme history;
- notes summary if useful.

Actions:

- edit pet;
- create treatment case;
- create programme from case;
- view published PDFs.

## Treatment case list

Fields/columns:

- pet name;
- owner name;
- case title/presenting condition;
- practitioner;
- status;
- start date;
- last note date;
- current programme status.

Filters:

- status;
- practitioner;
- active/open cases;
- review due.

## Treatment case detail

Sections:

- case summary;
- goals;
- status;
- practitioner;
- case notes timeline;
- active/draft programme card;
- published programme history.

Actions:

- add note;
- edit case;
- create programme;
- create new version from published programme;
- close/discharge case.

## Case note form

Fields:

- note date/time, default now;
- note type if supported;
- note text;
- optional follow-up date;
- practitioner, default current user.

Behaviour:

- notes should be timestamped;
- edits should be audited or restricted depending on final governance decision;
- never lose prior published programme context.

## Exercise library list

Fields/columns:

- title;
- category;
- summary;
- active/inactive;
- video link presence;
- last updated.

Filters:

- category;
- active/inactive;
- search by title/instruction text.

## Exercise detail/edit

Sections:

- exercise title;
- summary;
- category;
- image/media reference;
- video link;
- ordered instructions;
- default prescription values;
- active flag.

Actions:

- edit;
- duplicate;
- deactivate;
- preview in owner-facing programme style.

## Programme builder

The programme builder is the main Release 1 workflow.

Suggested layout:

- top programme header:
  - pet name;
  - owner name;
  - case;
  - start date;
  - end date;
  - status;
- left or top area for programme details;
- session cards for AM, PM or single session;
- within each session, ordered exercise cards;
- exercise cards show:
  - title;
  - thumbnail or icon;
  - reps;
  - sets;
  - hold time;
  - frequency;
  - practitioner notes;
  - video link indicator;
- actions:
  - add exercise;
  - edit prescription;
  - reorder;
  - remove from draft;
  - save draft;
  - preview PDF.

## PDF preview page

Purpose: show the practitioner what the owner will receive before publishing.

Sections:

- PDF-like preview area;
- validation panel;
- missing data warnings;
- actions:
  - back to edit;
  - publish;
  - download draft preview if allowed.

Validation warnings:

- missing owner name;
- missing pet name;
- missing exercise video link;
- no exercises in a session;
- missing programme date range;
- incomplete prescription values.

## Published programme page

Sections:

- current version;
- version history;
- generated date;
- generated by;
- PDF file link;
- status;
- create new draft from this version.

Rules:

- published versions are read-only;
- editing creates a new draft/version;
- the current version is clearly labelled.
