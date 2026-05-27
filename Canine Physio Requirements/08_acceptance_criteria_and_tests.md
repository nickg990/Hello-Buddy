# 8. Acceptance Criteria and Test Scenarios

## Acceptance criteria: owner and pet setup

### AC-001 Create owner

Given a practitioner is on the owner list  
When they create an owner with valid required fields  
Then the owner is saved and appears in owner search/list results.

### AC-002 Create pet for owner

Given an owner exists  
When a practitioner adds a pet to that owner  
Then the pet appears on the owner detail page and can be opened.

### AC-003 Edit owner or pet details

Given an owner or pet exists  
When a practitioner edits allowed fields  
Then the updated details are saved and visible on the detail page.

## Acceptance criteria: treatment cases and notes

### AC-004 Create treatment case

Given a pet exists  
When a practitioner creates a treatment case  
Then the case is linked to the pet and appears in the case list.

### AC-005 Add case note

Given a treatment case exists  
When a practitioner adds a note  
Then the note appears in the case note timeline with date/time and practitioner.

### AC-006 Case status

Given a treatment case exists  
When a practitioner changes status  
Then the case list and case detail reflect the new status.

## Acceptance criteria: exercise library

### AC-007 Create exercise

Given a practitioner is on the exercise library page  
When they create an exercise with title, summary, instructions and video link  
Then the exercise is available in the programme builder.

### AC-008 Edit exercise

Given an exercise exists  
When it is edited  
Then future draft programmes can use the updated exercise details.

### AC-009 Published programmes remain stable

Given an exercise is used in a published programme  
When the exercise library entry is later edited  
Then the existing published programme/PDF remains unchanged.

## Acceptance criteria: programme builder

### AC-010 Create draft programme

Given a treatment case exists  
When a practitioner creates a programme with dates and session structure  
Then the programme is saved as draft.

### AC-011 Add exercise to session

Given a draft programme exists  
When a practitioner adds an exercise to a session and enters prescription values  
Then the exercise appears in the correct session in the chosen order.

### AC-012 Reorder exercises

Given a draft session contains multiple exercises  
When a practitioner reorders them  
Then the order is saved and reflected in the PDF preview.

### AC-013 Remove exercise from draft

Given a draft programme has an exercise  
When the practitioner removes the exercise  
Then it is removed from the draft programme only.

## Acceptance criteria: PDF preview and publishing

### AC-014 Preview PDF

Given a draft programme exists  
When the practitioner opens PDF preview  
Then the preview shows owner, pet, programme dates, sessions and exercises.

### AC-015 Validate before publish

Given a draft programme has missing critical data  
When the practitioner attempts to publish  
Then the system prevents publishing and shows clear validation messages.

### AC-016 Publish programme

Given a valid draft programme exists  
When the practitioner publishes it  
Then the system creates an immutable programme version and generated PDF.

### AC-017 Download PDF

Given a programme version has been published  
When the practitioner opens the published programme page  
Then they can download or open the generated PDF.

### AC-018 Version immutability

Given a programme has been published  
When the practitioner needs to make a change  
Then the system creates a new draft/version rather than editing the published version.

## Acceptance criteria: security and data handling

### AC-019 Synthetic development data

Given the system is running in development/demo mode  
Then it should use synthetic data and not real owner/pet records.

### AC-020 Basic access control

Given the admin system contains personal and clinical data  
Then access should be protected before any real data is used.

### AC-021 Audit-friendly design

Given a practitioner publishes a programme  
Then the system records who published it and when.

## Suggested test scenarios

### Scenario 1: First end-to-end path

1. Create owner.
2. Create pet.
3. Create treatment case.
4. Add case note.
5. Create programme.
6. Add AM and PM sessions.
7. Add exercises.
8. Preview PDF.
9. Publish PDF.
10. Download/open PDF.

### Scenario 2: Incomplete programme

1. Create draft programme.
2. Add session but no exercises.
3. Attempt to publish.
4. Confirm validation prevents publishing.

### Scenario 3: Missing video link

1. Add exercise without video link.
2. Add to programme.
3. Preview PDF.
4. Confirm warning is shown.

### Scenario 4: Published programme update

1. Publish programme version 1.
2. Create a new draft from version 1.
3. Change exercise prescription.
4. Publish version 2.
5. Confirm version 1 PDF remains unchanged.

### Scenario 5: Exercise library update

1. Create exercise.
2. Publish a programme using it.
3. Edit exercise library text.
4. Confirm old PDF remains unchanged.
5. Confirm new draft programmes can use updated exercise details.

## First implementation success criteria

The first implementation is successful if Nick can:

- open the admin shell;
- see the Hello Buddy visual direction;
- navigate between dashboard, owners, case detail, programme builder and PDF preview;
- use synthetic data;
- understand the intended practitioner workflow;
- identify layout or wording changes before backend complexity is added.
