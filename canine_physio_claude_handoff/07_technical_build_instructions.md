# 7. Technical Build Instructions

## Preferred technology direction

Use a pragmatic web application stack suitable for a small practice MVP and future Azure deployment.

Recommended direction:

- ASP.NET Core web application or ASP.NET Core API plus frontend, depending on Claude's selected implementation plan;
- MySQL as the authoritative relational database;
- PDF generation component/service;
- local file storage for development;
- Azure Blob Storage or equivalent for production media/PDF storage;
- Azure App Service-compatible deployment structure;
- synthetic seed data for development and demonstration.

## Release 1 architecture

Release 1 should be simple:

```text
Practitioner Browser
        |
        v
Admin Web App / API
        |
        v
MySQL Database
        |
        v
PDF Generator
        |
        v
File Storage / Blob Storage
```

## Do not over-engineer the first increment

Do not start with:

- full multi-tenant administration;
- full authentication/authorisation complexity;
- mobile sync;
- background workers unless needed;
- event sourcing;
- microservices;
- distributed messaging;
- Kubernetes;
- semantic/RDF layers;
- XML export;
- advanced analytics.

These may be future architecture considerations but they should not be in the first working slice.

## Recommended project structure

A simple .NET solution could use:

```text
src/
  HelloBuddy.Admin/
  HelloBuddy.Application/
  HelloBuddy.Domain/
  HelloBuddy.Infrastructure/
  HelloBuddy.Pdf/
tests/
  HelloBuddy.Tests/
docs/
  requirements/
```

For the first implementation, a simpler single-project structure is acceptable if it helps validate the workflow and UI faster. The design should not become so complex that progress stalls before the first screen is usable.

## Domain boundaries

### Admin UI

Responsibilities:

- render practitioner-facing screens;
- validate obvious user input;
- call backend/application services;
- present PDF preview;
- show warnings and status.

### Application layer

Responsibilities:

- enforce workflow rules;
- coordinate programme creation and publishing;
- validate publish readiness;
- create programme versions;
- call PDF generator;
- store generated file metadata.

### Domain/data layer

Responsibilities:

- model approved database entities;
- query and update data;
- preserve relationships and version history;
- avoid overwriting published programme versions.

### PDF layer

Responsibilities:

- convert programme data into an owner-facing document;
- apply Hello Buddy styling;
- generate preview/final PDF;
- return file metadata.

## Security and privacy

Even in the MVP, design with health-adjacent personal data in mind.

Requirements:

- use synthetic data during development;
- avoid real pet owner personal data unless formally approved;
- use HTTPS in deployed environments;
- protect admin access;
- store only necessary data;
- avoid logging sensitive note contents unnecessarily;
- use role-based access in a later increment;
- maintain audit fields;
- make deletion/anonymisation workflows possible.

## GDPR-aware behaviour

- Personal data belongs mainly to owners.
- Clinical case records and programme history may need retention depending on professional/legal context.
- Deletion should not blindly remove clinical history if retention is required.
- Where deletion is not appropriate, anonymisation/restriction should be considered.
- Published PDFs should be treated as controlled clinical documents.

## Media handling

Exercise videos are initially links, not necessarily uploaded video files.

Requirements:

- store video URL or media reference per exercise;
- show whether an exercise has a video link;
- include clickable video links in the PDF;
- avoid embedding large videos into the PDF;
- allow future storage of images/videos in Blob Storage or equivalent.

## Environments

Suggested environments:

- local development;
- optional lightweight test/demo;
- production later.

Do not spend the first increment building complex DevOps pipelines. Keep the repo deployable and testable, but build the workflow first.

## Data seeding

The first build should include synthetic data:

- one practitioner;
- two owners;
- at least two pets;
- two treatment cases;
- several exercise library entries;
- one draft programme;
- one published programme example if useful.

## Error handling

Handle:

- missing owner/pet/case;
- incomplete programme;
- missing video link;
- PDF generation failure;
- file storage failure;
- invalid dates;
- empty exercise session;
- duplicate or inactive exercises.

## Logging

Log operational events such as:

- programme created;
- programme published;
- PDF generation failed;
- exercise updated;
- case note added.

Avoid logging sensitive clinical note content unless explicitly required and approved.
