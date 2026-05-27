# Hello Buddy Admin

## First Increment Demo Scope and Workflow

## Purpose

This document translates the end-to-end clinical journey into a realistic first increment that can be demonstrated to a client and also defended in COM712 Assessment 3 Option C.

It is intentionally narrow. The objective is to show a complete and believable practitioner workflow slice, not the full Release 1 lifecycle.

## Source Alignment

This scope is derived from:

- [Designs/03i_mermaid_end_to_end_clinical_journey.md](Designs/03i_mermaid_end_to_end_clinical_journey.md)
- [Canine Physio Requirements/03c_mermaid_programme_creation.md](Canine%20Physio%20Requirements/03c_mermaid_programme_creation.md)
- [Canine Physio Requirements/03f_mermaid_pdf_generation_sequence.md](Canine%20Physio%20Requirements/03f_mermaid_pdf_generation_sequence.md)
- [Canine Physio Requirements/10_implementation_approach.md](Canine%20Physio%20Requirements/10_implementation_approach.md)

## Recommended First Increment Boundary

The first increment should cover one coherent sub-journey from the full clinical workflow:

1. Open an existing treatment case.
2. Review case summary and current programme state.
3. Create or resume a draft programme.
4. Add or adjust exercises within sessions.
5. Preview the owner-facing PDF.
6. Validate required fields and blockers.
7. Publish the programme.
8. Show the published result.

This gives a true end-to-end demonstration of the core Release 1 value without attempting the entire clinical administration platform.

## Workflow Mapping To The Swimlane

The first increment covers these steps from the end-to-end journey:

- `P8` Create draft programme
- `P9` Add exercises to sessions and set prescription values
- `P10` Refine prescription
- `P11` Open PDF preview
- `S1` Validation gate
- `S2` Create immutable version and PDF
- `S3` Store PDF and metadata

Optionally, for a better demonstration opening, include the visible context from:

- `P3` Open treatment case
- `P4` Record condition and goals as already-seeded case data
- `P5` Add initial note as already-seeded case data

## What The Client Can See End To End

The client can still be shown a believable end-to-end journey if the walkthrough is framed as:

Case detail -> Programme builder -> PDF preview -> Publish -> Published confirmation

That is sufficient to validate:

- screen flow;
- information architecture;
- practitioner usability;
- the owner-facing PDF concept;
- whether the publish workflow feels clinically sensible.

## What Is Explicitly Deferred

The first increment does not need to implement the full lifecycle shown in the swimlane.

Deferred items:

- new owner, pet, and case creation wizard;
- full owner and pet CRUD;
- exercise-library authoring and maintenance;
- follow-up appointment loop;
- progress note entry beyond seeded examples;
- create new version from published;
- case closure and archive;
- re-injury or future episode handling.

## First Increment Demo Script

This is the recommended client and assessment demonstration script.

### Step 1: Open a seeded treatment case

Show the practitioner landing on an existing case such as Bella, including:

- pet name;
- owner name;
- clinical summary;
- status;
- current programme status;
- recent note summary.

Explain that this increment starts from an existing active case so the workflow focus remains on programme creation and publication.

### Step 2: Review case context

Use the case detail screen to show:

- presenting condition;
- goals;
- practitioner;
- current active or draft programme status.

The purpose is to show that programme design is grounded in a clinical case rather than created in isolation.

### Step 3: Create or resume draft programme

Click the primary action to create or resume a draft programme.

Demonstrate:

- programme title;
- date range;
- session structure such as AM and PM or single daily session.

### Step 4: Edit exercises inside sessions

Show the programme builder and demonstrate:

- selecting exercises from a seeded library list;
- adding them to a session;
- entering reps, sets, hold seconds, frequency, and notes;
- showing validation warnings where data is incomplete.

### Step 5: Open PDF preview

Open the preview page and show:

- owner-facing formatting;
- pet and programme details;
- session layout;
- exercise cards or rows;
- video-link presentation.

Explain that the preview represents the same structure used for final PDF generation.

### Step 6: Show validation gate

Demonstrate one blocked publish state, for example a missing video link or missing prescription value.

Show:

- blocker list;
- warning list;
- disabled publish button.

Then fix the issue and show the state becoming publishable.

### Step 7: Publish programme

Trigger publish and explain the system behaviour:

- create a programme version;
- generate the PDF through the PDF service;
- store the file and metadata;
- return a published result.

### Step 8: Show published confirmation

Show a confirmation or published page with:

- version number;
- publish timestamp;
- download or open link;
- indication that the workflow has completed successfully.

## Covered Now Vs Deferred Later

| Area               | Covered in first increment         | Deferred                                            |
| ------------------ | ---------------------------------- | --------------------------------------------------- |
| Case context       | View seeded case summary and notes | Create and edit full cases                          |
| Owner and pet      | Read-only seeded references        | Full owner and pet CRUD                             |
| Programme builder  | Yes                                | Advanced reorder and full draft lifecycle           |
| Exercise selection | Yes, from seeded library data      | Full exercise-library management                    |
| PDF preview        | Yes                                | Polished print options and advanced export features |
| Publish workflow   | Yes                                | Full version history and supersede flows            |
| Notes              | Read-only seeded examples          | Add and edit notes                                  |
| Follow-up loop     | No                                 | Future increment                                    |
| Case closure       | No                                 | Future increment                                    |
| New patient wizard | No                                 | Future increment                                    |

## Minimum Screens Required

The first increment should include only the screens needed to support the demo flow.

### Screen 1: Treatment case detail

Purpose:

- establish clinical context;
- show active or draft programme status;
- provide the primary action into the programme workflow.

Minimum content:

- case summary card;
- goals;
- practitioner;
- recent note summary;
- active programme or draft status;
- primary action button.

### Screen 2: Programme builder

Purpose:

- let the practitioner assemble and adjust the draft programme.

Minimum content:

- programme header details;
- session cards;
- seeded exercise picker;
- editable prescription fields;
- draft save action;
- preview action;
- validation summary.

### Screen 3: PDF preview

Purpose:

- let the practitioner review the owner-facing output before publish.

Minimum content:

- preview pane;
- validation panel;
- back to edit action;
- publish action.

### Screen 4: Published confirmation

Purpose:

- confirm successful completion of the flow.

Minimum content:

- published version label;
- timestamp;
- PDF access link;
- route back to case or programme history.

## Minimum Data Model Needed For The Demo

This is the thinnest practical data model for a clickable prototype with light persistence.

### Recommended entities

1. `TreatmentCase`
2. `Programme`
3. `ProgrammeSession`
4. `ProgrammeExercise`
5. `Exercise`

### Seeded supporting fields

The case seed should also include enough read-only context for:

- owner name;
- pet name;
- practitioner name;
- case title or condition;
- goals;
- one or two recent note summaries.

These can be stored directly in seeded case data for the first increment if full relational owner and pet entities are intentionally deferred.

## Persistence Options

### Option A: Pure clickable prototype

- seeded JSON or in-memory data only;
- no durable business persistence;
- suitable for design validation only.

### Option B: Clickable prototype with thin persistence

- seeded initial case and exercise data;
- draft programme and publish state persisted;
- enough backend realism to strengthen the assessment argument.

Option B is the better choice if the solution is intended to support both client demo and assessment submission.

## Containers For The First Increment

The minimum meaningful container split is:

1. Admin web container
   Provides the practitioner UI and orchestrates the workflow.

2. PDF service container
   Generates final PDF output from the shared preview template.

If persistence is added, the database can remain thin and focused on the programme workflow rather than full admin CRUD.

## Assessment Value Of This Scope

This first increment is narrow, but it still gives strong assessment evidence because it demonstrates:

- a realistic practitioner workflow;
- two meaningful application containers;
- storage or persistence design decisions;
- validation and publish logic;
- an owner-facing output;
- a clean path to deployment, monitoring, and testing.

## Recommendation

For both client review and assessment strength, the best first increment is not a dashboard-heavy prototype and not broad CRUD. It is a focused programme-publication slice beginning from a seeded case and ending at a published PDF.

That is the smallest slice that still feels clinically meaningful, visually demonstrable, and technically defensible.
