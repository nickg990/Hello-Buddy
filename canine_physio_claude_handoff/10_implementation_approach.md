# 10. Proposed Implementation Approach

## Principle

Build the system in small vertical increments. The first increment should be deliberately small so that Nick can review the workflow, visual direction and page layout before too much backend code exists.

The safest path is:

1. validate the design and navigation;
2. add database-backed core records;
3. add exercise library;
4. add programme builder;
5. add PDF preview/generation;
6. add versioning and hardening;
7. only then consider mobile/future functionality.

## Increment 0 — Repository and design skeleton

### Goal

Create the project skeleton and shared visual foundation.

### Include

- solution/project structure;
- Hello Buddy theme variables;
- layout shell;
- navigation;
- reusable components;
- synthetic data file;
- basic routes/pages;
- README with setup instructions.

### Exclude

- real authentication;
- database integration;
- PDF generation;
- deployment automation;
- mobile features.

### Done when

The app runs locally and displays the admin shell using the Hello Buddy palette.

## Increment 1 — Small design-validation prototype

### Goal

Validate the practitioner workflow and layout before investing in the backend.

### Include

- dashboard;
- owner/pet/case summary using synthetic data;
- treatment case detail;
- case note timeline mock;
- programme builder mock;
- PDF preview mock;
- navigation between these screens;
- visual design based on the Hello Buddy palette;
- placeholder logo component.

### Exclude

- real database writes;
- real PDF generation;
- login/security complexity;
- full CRUD;
- mobile JSON;
- offline sync.

### Why this should come first

This is the cheapest point to refine design. It allows Nick to decide whether the flow feels right:

Dashboard -> owner/pet/case -> programme builder -> PDF preview.

### Done when

Nick can click through the main path and comment on screen layout, wording and programme/PDF structure.

## Increment 2 — Core database-backed records

### Goal

Connect the admin workflow to the approved database for the core records.

### Include

- owner list/detail/create/edit;
- pet list/detail/create/edit;
- treatment case list/detail/create/edit;
- case note creation;
- basic validation;
- synthetic seed data;
- tests for core CRUD.

### Exclude

- exercise library;
- programme builder persistence;
- PDF generation.

### Done when

A practitioner can create an owner, add a pet, create a treatment case and add a case note.

## Increment 3 — Exercise library

### Goal

Create the reusable exercise data that programmes will use.

### Include

- exercise list;
- create/edit exercise;
- ordered instructions;
- category;
- summary;
- video link;
- image/media reference;
- active/inactive status;
- default prescription guidance if supported by schema.

### Exclude

- programme publishing;
- PDF generation.

### Done when

Exercises can be created and selected for use in a programme.

## Increment 4 — Programme builder

### Goal

Create draft programmes from treatment cases.

### Include

- create draft programme;
- set programme dates;
- choose single or AM/PM session structure;
- add exercises to sessions;
- edit reps, sets, hold seconds, frequency and notes;
- reorder session exercises;
- save draft;
- validation before preview.

### Exclude

- final PDF generation;
- immutable publish/version logic unless trivial;
- mobile JSON.

### Done when

A practitioner can build and save a complete draft programme.

## Increment 5 — PDF preview and generation

### Goal

Make the Release 1 output real.

### Include

- PDF-style preview from draft programme;
- publish validation;
- final PDF generation;
- storage of PDF file;
- programme version creation;
- download/open published PDF.

### Exclude

- owner portal;
- email/WhatsApp automation;
- mobile JSON.

### Done when

A valid programme can be published and downloaded as a branded PDF.

## Increment 6 — Versioning, audit and hardening

### Goal

Make the workflow safer and more clinically appropriate.

### Include

- immutable published versions;
- create new draft from published version;
- version history page;
- audit metadata;
- error handling;
- better validation;
- basic access control;
- GDPR-aware data controls;
- test coverage against acceptance criteria.

### Done when

Published PDFs are stable and changes create new versions rather than overwriting history.

## Increment 7 — Deployment-ready MVP

### Goal

Prepare for a small live or demo deployment.

### Include

- production configuration;
- HTTPS;
- managed database connection settings;
- file/blob storage configuration;
- basic logging and monitoring;
- backup/restore notes;
- environment-specific settings;
- deployment README.

### Done when

The system can be deployed to a small Azure App Service-style environment.

## Future Increment 8 — Mobile-ready programme snapshots

### Goal

Prepare for later Hello Buddy mobile app integration.

### Include

- JSON programme snapshot generation;
- media metadata;
- mobile sync endpoint design;
- programme version payloads.

### Exclude from Release 1 unless explicitly requested.

## Future Increment 9 — Owner progress and offline sync

### Goal

Support mobile owners recording completion of prescribed exercises.

### Include later

- local SQLite on mobile;
- offline capture;
- sync queue;
- completion events;
- skipped sessions;
- discomfort score;
- comments;
- server-side event acceptance;
- conflict handling.

### Exclude from Release 1.

## Recommended first Claude task

Ask Claude to build only Increment 1.

The first prompt should say:

```text
Build Increment 1 only: a small design-validation prototype using synthetic data. Focus on page flow, layout, Hello Buddy styling, programme builder mock and PDF preview mock. Do not build the full backend yet. Do not redesign the database.
```

## Review gate after Increment 1

Before moving to Increment 2, review:

- Does the navigation match how a practitioner would work?
- Are owner, pet and case relationships clear?
- Does the programme builder feel easy enough?
- Does the PDF preview look close to the required owner-facing output?
- Are labels and terms right?
- Are any fields missing from the forms?
- Is the layout too dense or too sparse?
- Does the Hello Buddy styling feel appropriate for clinical admin?

Only after this review should Claude start database integration.
