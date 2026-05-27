# Hello Buddy Canine Physiotherapy Admin

## Users, Tasks, Requirements, Sitemap and Wireframe Plan

## Purpose

This planning pack applies the approach in [MSc Assessment/Planning approach.md](../MSc%20Assessment/Planning%20approach.md) to the full Hello Buddy administration system.

It is intended to:

- define the whole admin-system scope before narrowing the assessment slice;
- keep design decisions traceable from users to tasks to requirements to pages;
- prioritise intuitive ease of use for busy canine physiotherapists;
- identify the representative wireframes needed to confirm scope.

## Planning Principles

The most important design priority is intuitive ease of use for canine physiotherapists.

That means the admin system should:

- surface active work quickly;
- minimise unnecessary clicks and screen-hopping;
- keep navigation stable across pages;
- show clear status and next actions;
- reduce cognitive load during clinical documentation and programme creation;
- remain readable and usable on laptop and tablet without dense layouts.

## Core Users And Tasks

### User 1: Practitioner physiotherapist

Intent:

- manage active canine cases safely and efficiently;
- create and revise rehabilitation programmes;
- generate clear owner-facing outputs.

Key tasks:

- review active cases and items needing attention;
- create a new owner, pet, and case when starting a new patient journey;
- record or review condition, goals, and case notes;
- select or refine exercises from the library;
- add multiple exercises to a session in one pass, then reorder them quickly by drag and drop;
- build a structured programme;
- preview and publish the owner-facing PDF;
- review previous versions before making changes;
- close or discharge a case when complete.

Top-priority tasks:

- open the right case quickly;
- create or resume a draft programme;
- preview and publish with confidence;
- understand what needs fixing before publish.

What the system must make easy:

- one clear route from case context to programme context to publish.
- one clear route from session to filtered exercise picker to confirmed selection to drag-and-drop reordering.

### User 2: Practice admin or coordinator

Intent:

- keep owner and pet records tidy;
- help set up new cases and reduce practitioner admin overhead.

Key tasks:

- create and update owner records;
- create and update pet records;
- start the new patient journey wizard;
- search existing owners, pets, and cases;
- locate published PDFs when owners need another copy.

Top-priority tasks:

- new patient setup;
- find the correct record quickly;
- retrieve the latest published PDF.

What the system must make easy:

- safe data entry and clear search results.

### User 3: Practice lead or clinical owner

Intent:

- maintain clinical quality and operational oversight.

Key tasks:

- review drafts and published programme activity;
- monitor library quality and missing video links;
- confirm version history is preserved;
- manage operational settings and practitioner defaults.

Top-priority tasks:

- see what is active, overdue, incomplete, or recently published;
- maintain confidence that records and outputs are controlled.

What the system must make easy:

- high-level oversight without needing to open many screens.

### User 4: Pet owner as external recipient

Intent:

- receive a clear programme and follow it at home.

Key tasks:

- open the latest PDF;
- understand the exercise plan;
- use video links correctly.

Top-priority tasks:

- understand what to do next;
- trust the document.

What the admin system must make easy:

- generate a clean, readable, friendly PDF output.

## Task Priorities

### Must be effortless

- find an active case;
- start a new patient journey;
- resume a draft programme;
- preview and publish a programme;
- find the latest published PDF.

### Must be straightforward

- update owner and pet records;
- add or review case notes;
- filter exercises and detect missing video links;
- create a new draft from a published version.

### Can be secondary in early scope

- detailed settings management;
- richer reporting and analytics;
- advanced exercise-library authoring workflows.

## Requirements

| ID  | Priority | Requirement                                                                                                                                                                                                                                                                                  | Primary user              | Task supported                                         | Why it matters                                                      |
| --- | -------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------- | ------------------------------------------------------ | ------------------------------------------------------------------- |
| R1  | Must     | The system must use stable primary navigation with clear labels for Dashboard, Owners, Pets, Treatment Cases, Programmes, Exercise Library, Published PDFs, and Settings.                                                                                                                    | Practitioner, Admin       | Move between core work areas                           | Reduces cognitive load and supports intuitive use.                  |
| R2  | Must     | The dashboard must surface active work, drafts, review-due items, recent publishes, and direct quick actions.                                                                                                                                                                                | Practitioner, Lead        | Triage daily work                                      | Busy clinicians need a clear starting point.                        |
| R3  | Must     | The system must support a guided new patient journey for Owner -> Pet -> Case setup.                                                                                                                                                                                                         | Admin, Practitioner       | Start a new patient record                             | Makes onboarding safer and easier than separate disconnected forms. |
| R4  | Must     | Owner records must show contact details, linked pets, linked cases, and recent programme activity.                                                                                                                                                                                           | Admin, Practitioner       | Find the right owner context                           | Supports quick lookup and continuity.                               |
| R5  | Must     | Pet records must show owner context, active cases, current programme state, and recent activity.                                                                                                                                                                                             | Practitioner, Admin       | Work from the pet as a clinical anchor                 | Practitioners often think in pet-first terms.                       |
| R6  | Must     | Treatment case detail must show condition, goals, notes, status, and current programme history together.                                                                                                                                                                                     | Practitioner              | Understand clinical context before editing a programme | Prevents programme design in isolation.                             |
| R7  | Must     | Case notes must be easy to review and add, with timestamps and clear chronology.                                                                                                                                                                                                             | Practitioner              | Record progress and follow-up decisions                | Note-taking is part of routine clinical work.                       |
| R8  | Must     | The exercise library must support quick search, filtering, active/inactive status, and clear indication of missing video links.                                                                                                                                                              | Practitioner, Lead        | Select safe exercises quickly                          | The library must speed work up, not slow it down.                   |
| R9  | Must     | The programme builder must support single or AM/PM sessions with structured prescription fields, a session-scoped multi-select exercise picker, editable metadata prefilled from exercise defaults, and fast drag-and-drop reordering after add, with a non-drag fallback for accessibility. | Practitioner              | Create a clinically useful programme                   | This is the core release workflow.                                  |
| R10 | Must     | The programme builder must make validation issues visible before publish.                                                                                                                                                                                                                    | Practitioner              | Fix blockers confidently                               | Reduces publishing errors and hesitation.                           |
| R11 | Must     | The PDF preview must reflect the final owner-facing layout closely enough to support confident review before publish.                                                                                                                                                                        | Practitioner, Owner       | Validate final output                                  | Prevents mismatch between preview and published result.             |
| R12 | Must     | Publish must create a read-only version and make the latest PDF easy to retrieve later.                                                                                                                                                                                                      | Practitioner, Admin, Lead | Publish and retrieve controlled outputs                | Supports auditability and continuity.                               |
| R13 | Must     | Published PDFs must have a dedicated area with current version, history, generated date, and download access.                                                                                                                                                                                | Admin, Practitioner       | Retrieve the right document quickly                    | Owners will need re-sends and staff need certainty.                 |
| R14 | Must     | Search and filters must be available across owners, pets, treatment cases, programmes, and exercises.                                                                                                                                                                                        | Admin, Practitioner       | Find records quickly                                   | Efficient retrieval is a daily need.                                |
| R15 | Must     | Core pages must remain readable and usable on laptop and tablet without horizontal scrolling.                                                                                                                                                                                                | Practitioner, Admin       | Work comfortably in clinic settings                    | Reflects likely device use.                                         |
| R16 | Must     | Primary actions, statuses, and validation messages must use plain language and must not rely on colour alone.                                                                                                                                                                                | All direct users          | Understand what to do next                             | Accessibility and safety requirement.                               |
| R17 | Should   | The dashboard should show operational oversight such as missing video links, stale drafts, and review due counts.                                                                                                                                                                            | Lead, Practitioner        | Spot risk early                                        | Helps maintain clinical quality.                                    |
| R18 | Should   | Settings should support practitioner profile, PDF defaults, and environment or practice details.                                                                                                                                                                                             | Lead, Admin               | Configure the system for production use                | Useful for hardening and future release work.                       |
| R19 | Should   | Published programme pages should support creating a new draft from the latest published version.                                                                                                                                                                                             | Practitioner              | Revise without losing history                          | Supports the normal clinical follow-up loop.                        |
| R20 | Should   | The UI should prioritise the case-to-programme-to-PDF workflow over broad dashboard density.                                                                                                                                                                                                 | Practitioner              | Complete high-value tasks with low friction            | Matches the most important clinical use path.                       |

## Sitemap

### Primary navigation

1. Dashboard
2. Owners
3. Pets
4. Treatment Cases
5. Programmes
6. Exercise Library
7. Published PDFs
8. Settings

### Whole admin-system hierarchy

```text
Dashboard
  ├── Reviews due
  ├── Draft programmes
  ├── Recent publishes
  └── Quick actions

New Patient Journey
  ├── Owner
  ├── Pet
  └── Treatment Case

Owners
  ├── Owner list
  ├── Owner detail
  ├── Edit owner
  └── Add pet

Pets
  ├── Pet list
  ├── Pet detail
  ├── Edit pet
  ├── Active treatment cases
  └── Programme history

Treatment Cases
  ├── Case list
  ├── Case detail
  ├── Add note
  ├── Edit case
  ├── Create programme
  ├── Programme history
  └── Close case

Programmes
  ├── Programme list
  ├── Draft detail
  ├── Programme builder
  ├── PDF preview
  ├── Publish
  └── Published version detail

Exercise Library
  ├── Exercise list
  ├── Exercise detail
  ├── New exercise
  └── Edit exercise

Published PDFs
  ├── Published list
  ├── Current version
  ├── Version history
  └── Download / share reference

Settings
  ├── Practitioner profile
  ├── Practice details
  ├── PDF defaults
  └── Environment / support
```

### Sitemap rationale

- It is task-led rather than entity-led alone.
- It gives clinicians a direct path from case work into programme work.
- It gives admin staff obvious record-management destinations.
- It gives published outputs their own clear destination rather than burying them inside cases only.
- It keeps Settings present but secondary.

## Wireframe Set

The wireframe set should cover the full admin system with a representative group of high-value screens rather than every minor page state.

### Existing wireframes retained and updated

1. [Designs/wireframes/01_dashboard.svg](wireframes/01_dashboard.svg)
2. [Designs/wireframes/02_owner_list.svg](wireframes/02_owner_list.svg)
3. [Designs/wireframes/03_owner_detail.svg](wireframes/03_owner_detail.svg)
4. [Designs/wireframes/04_case_detail.svg](wireframes/04_case_detail.svg)
5. [Designs/wireframes/05_programme_builder.svg](wireframes/05_programme_builder.svg)
6. [Designs/wireframes/06_pdf_preview.svg](wireframes/06_pdf_preview.svg)
7. [Designs/wireframes/07_exercise_library.svg](wireframes/07_exercise_library.svg)

### New wireframes needed for whole-system coverage

8. [Designs/wireframes/08_pet_detail.svg](wireframes/08_pet_detail.svg)
9. [Designs/wireframes/09_published_programme.svg](wireframes/09_published_programme.svg)
10. [Designs/wireframes/10_new_patient_wizard.svg](wireframes/10_new_patient_wizard.svg)

## Assessment Wireframes

These are the wireframes most likely to shape the later assessment slice discussion.

### Core workflow candidate set

1. Dashboard
2. Treatment case detail
3. Programme builder
4. PDF preview
5. Published programme

### Supporting workflow candidate set

1. Owner list
2. Owner detail
3. Pet detail
4. New patient wizard
5. Exercise library

## Required Updates To The Existing Wireframes

The existing wireframes should be updated to reflect the whole-admin-system requirements.

### Navigation updates

- desktop left navigation should include all primary sections;
- mobile should keep the compact primary tabs but rely on menu access for secondary sections;
- labels must stay plain language.

### Structural updates

- pet detail should become a first-class destination rather than only a section inside owner detail;
- published programme history should become visible as its own destination and page state;
- the new patient journey should be represented explicitly as a guided flow;
- dashboard quick actions should reflect the highest-priority admin actions.

### Ease-of-use updates

- the most important action on each screen should be visually obvious;
- record context should remain visible when moving into programme work;
- programme building should let the physio open a popup, filter by exercise type, tick multiple exercises, confirm them into the active session, then drag and drop them into order without extra navigation;
- warnings and blockers should be specific and readable;
- dense data grids should be avoided where a simpler card or summary pattern is sufficient.

## Traceability Summary

### Users to tasks

- Practitioner -> case review, programme creation, PDF publication
- Admin -> new patient setup, owner and pet record management, PDF retrieval
- Practice lead -> oversight, quality control, settings and defaults
- Owner external -> clear PDF output

### Tasks to requirements

- quick case access -> R1, R2, R6, R14
- new patient setup -> R3, R4, R5
- safe programme creation -> R8, R9, R10, R11
- controlled publishing -> R12, R13, R19
- intuitive use -> R15, R16, R20

### Requirements to pages

- R1 to R3 -> Dashboard and New Patient Journey
- R4 to R7 -> Owners, Pets, Treatment Cases
- R8 to R13 -> Exercise Library, Programmes, Published PDFs
- R14 to R18 -> cross-cutting navigation, filters, and Settings

## Recommendation

Confirm the whole-admin-system structure using this wider wireframe set first. After that, choose the assessment scope as a deliberate subset rather than designing the entire system around the assessment too early.
