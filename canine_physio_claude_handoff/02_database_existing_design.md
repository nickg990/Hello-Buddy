# 2. Database Existing Design

## Critical instruction for Claude

The database has already been designed. Do not redesign the schema.

Use the existing SQL script, ERD or canonical schema provided by Nick as the source of truth. This document summarises the intended model but is not a replacement for the approved database design.

If the implementation reveals a schema issue, Claude should:

1. state the issue clearly;
2. explain the impact;
3. propose a migration separately;
4. wait for approval before changing the data model.

Do not silently rename tables, merge entities, remove history, or replace the relational model with a document model.

## Database role

The relational database is the authoritative system of record for Release 1.

It stores:

- practitioners/admin users;
- owners;
- pets;
- treatment cases;
- case notes;
- exercises;
- exercise instructions;
- programmes;
- programme sessions;
- exercises assigned to sessions;
- generated/published programme versions;
- PDF metadata.

## Expected core entities

The approved design may contain the following or equivalent entities:

- `Practitioner`
- `Owner`
- `Pet`
- `Pet_Practitioner` or equivalent practitioner-pet association/history table
- `TreatmentCase`
- `TreatmentCaseNote`
- `Exercise`
- `ExerciseInstruction`
- `Programme`
- `Session`
- `SessionExercise`
- `ProgrammeVersion`

## Naming and consistency

Prefer the naming already used in the approved schema. Where new application code is written, use consistent names and avoid mixing alternative terminology.

Preferred domain language:

- use "owner" rather than "customer" in the product UI;
- use "pet" rather than "patient" unless the clinical context needs patient language;
- use "programme" rather than "program";
- use "tracking completion of prescribed exercises" rather than "adherence";
- use "treatment case" for the clinical case around a pet;
- use "case note" for practitioner-entered clinical notes.

## Field guidance

The implementation should not invent its own fields if the approved database already provides them. The following are examples of expected data concepts, not independent schema authority.

### Owner

- owner identifier;
- name;
- contact email;
- phone number;
- address or partial address if needed;
- communication preferences if included in the approved schema;
- audit timestamps.

### Pet

- pet identifier;
- owner reference;
- pet name;
- species, expected to be dog for the initial scenario;
- breed;
- sex;
- age or date of birth, whichever the approved schema uses;
- relevant clinical notes or flags where included;
- audit timestamps.

### Practitioner

- practitioner identifier;
- name;
- email;
- role;
- active/inactive status.

### Treatment case

- case identifier;
- pet reference;
- practitioner reference;
- case title or presenting condition;
- start date;
- status such as open, active, review due, discharged;
- clinical goals;
- summary;
- audit timestamps.

### Treatment case note

- note identifier;
- treatment case reference;
- practitioner reference;
- note date/time;
- note type if included;
- note body;
- audit timestamps.

### Exercise

- exercise identifier;
- title;
- summary;
- category;
- default image/media reference;
- default video link;
- default instructions;
- default prescription guidance if included;
- active/inactive status.

### Exercise instruction

- instruction identifier;
- exercise reference;
- sequence/order;
- instruction text.

### Programme

- programme identifier;
- pet reference;
- treatment case reference;
- practitioner reference;
- programme title;
- start date;
- end date;
- status such as draft, published, superseded, archived.

### Session

- session identifier;
- programme reference;
- period such as AM, PM or Single;
- objective;
- display order.

### Session exercise

- session exercise identifier;
- session reference;
- exercise reference;
- display order;
- prescribed reps;
- prescribed sets;
- hold seconds;
- duration;
- frequency;
- practitioner notes;
- overridden instructions where allowed.

### Programme version

- programme version identifier;
- programme reference;
- version number;
- published date/time;
- published by;
- immutable payload snapshot if the approved design includes it;
- PDF file reference/path;
- status;
- audit metadata.

## Data integrity rules

- A pet must belong to an owner.
- A treatment case must belong to a pet.
- A programme must belong to a pet and normally to a treatment case.
- A programme contains one or more sessions.
- A session contains one or more exercises.
- Exercises should be reusable across many programmes.
- Published programme versions must be immutable.
- Editing a draft is allowed.
- Changing a published programme should create a new version, not overwrite the existing published version.

## Future data concepts

Do not implement these in Release 1 unless explicitly requested, but avoid design choices that block them:

- mobile programme JSON snapshots;
- mobile completion events;
- skipped session events;
- discomfort scores;
- owner comments;
- sync status and conflict metadata;
- XML export;
- analytics over exercise completion and outcomes.
