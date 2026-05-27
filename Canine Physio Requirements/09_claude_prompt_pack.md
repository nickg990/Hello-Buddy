# 9. Claude Prompt Pack

## How to use these prompts

Do not give Claude the whole project and ask it to build everything at once. Use the prompts in sequence.

The goal is to keep each Claude chat focused, reduce context drift and avoid a polished-looking but incomplete solution.

## Prompt 1 — Requirements digestion

Paste the relevant markdown files, then use this prompt:

```text
You are helping design and build the Hello Buddy canine physiotherapy admin system.

Read the supplied requirements carefully. Before producing code, summarise:

1. the Release 1 scope;
2. the explicit out-of-scope items;
3. the fact that the database has already been designed and must not be redesigned;
4. the main practitioner workflows;
5. the first small implementation increment.

Do not invent new scope. Do not redesign the database. Raise questions only where a blocker exists.
```

## Prompt 2 — Workflow design

Use this when giving Claude the Mermaid files:

```text
Using the supplied Mermaid workflow files, check whether the Release 1 admin workflow is coherent.

Produce:
1. a short workflow summary;
2. any gaps or contradictions;
3. a recommended screen sequence;
4. no code yet.

Keep the database fixed. Treat mobile sync and JSON publication as future-phase only.
```

## Prompt 3 — Screen and form specification

```text
Design the Release 1 admin screens for the Hello Buddy canine physiotherapy admin system.

Use the supplied page-flow, UI design direction, PDF requirements and acceptance criteria.

Produce:
1. screen list;
2. fields per screen;
3. primary and secondary actions;
4. validation rules;
5. responsive layout notes;
6. component list.

Do not produce code yet. Keep the first implementation small enough to validate layout and workflow.
```

## Prompt 4 — First implementation only

```text
Build Increment 1 only.

Increment 1 is a small design-validation prototype using synthetic data. It should include:

- admin shell with Hello Buddy styling;
- dashboard;
- owner/pet/case summary;
- treatment case detail;
- programme builder view using synthetic data;
- PDF preview-style page using synthetic data;
- no real authentication;
- no real PDF generation;
- no database writes unless the project skeleton already supports them safely.

The aim is to validate page flow, layout, wording and visual direction before building the full backend.

Use the Hello Buddy colour palette:
#6392AE
#28404F
#B3CDD6
#D4E0E5
#FFFFFF

Keep the database model fixed. Do not invent a new schema.
```

## Prompt 5 — Review the first implementation

```text
Review the Increment 1 prototype against the requirements.

Produce:
1. what is implemented;
2. what is missing;
3. layout/design risks;
4. requirements risks;
5. suggested refinements before backend work begins.

Do not expand the scope beyond Increment 1.
```

## Prompt 6 — Add database integration

```text
Now extend the prototype to Increment 2: owner, pet and treatment case CRUD using the approved database schema.

Use the existing schema exactly. If a field or relationship is unclear, create a clearly labelled adapter or TODO rather than changing the schema.

Include:
- list pages;
- detail pages;
- create/edit forms;
- validation;
- synthetic seed data;
- tests for the main CRUD paths.

Do not implement PDF generation yet.
```

## Prompt 7 — Add exercise library

```text
Implement Increment 3: exercise library management.

Include:
- exercise list;
- exercise detail;
- create/edit exercise;
- ordered instructions;
- video link field;
- active/inactive status;
- use in programme builder.

Do not change the approved database schema without proposing a separate migration.
```

## Prompt 8 — Add programme builder

```text
Implement Increment 4: programme builder.

Include:
- create draft programme from treatment case;
- choose session structure;
- add exercises to sessions;
- edit reps, sets, hold seconds, frequency and notes;
- reorder exercises;
- save draft;
- validation before preview.

Do not publish PDFs yet. Do not add mobile JSON output yet.
```

## Prompt 9 — Add PDF preview and generation

```text
Implement Increment 5: PDF preview and generation.

Include:
- owner-facing PDF preview;
- publish validation;
- immutable ProgrammeVersion creation;
- PDF generation;
- stored file reference;
- version history;
- download/open PDF.

Published versions must not be overwritten. Changes after publishing create a new draft/version.
```

## Prompt 10 — Test and harden

```text
Review the whole Release 1 implementation against the acceptance criteria.

Produce:
1. test coverage summary;
2. failed/missing criteria;
3. security and GDPR risks;
4. design/layout issues;
5. next recommended implementation increment.

Do not claim the solution is complete unless every Release 1 acceptance criterion is met.
```
