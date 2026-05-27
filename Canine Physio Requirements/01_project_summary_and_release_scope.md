# 1. Project Summary and Release 1 Scope

## Product name

Hello Buddy — Canine Physiotherapy Admin System.

## Business context

The organisation currently creates canine physiotherapy exercise programmes manually and sends them to pet owners as WhatsApp attachments. This creates administrative burden and makes programme history, reuse and version control harder to manage.

Release 1 replaces the manual document process with an admin system that stores owner, pet, treatment case, clinical note, exercise and programme data in a structured database, then generates a branded PDF exercise programme with clickable links to exercise videos.

## Release 1 goal

Build a small but usable administration system for practitioners that supports:

- creating and maintaining owner and pet records;
- creating treatment cases for pets;
- recording clinical notes against treatment cases;
- maintaining a reusable exercise library;
- creating exercise programmes from structured exercise data;
- previewing a programme before publishing;
- generating a branded PDF programme for the pet owner;
- storing published programme versions for clinical history and audit;
- downloading or sharing the PDF outside the system, initially through existing channels such as WhatsApp or email.

## Release 1 users

### Practitioner

The physiotherapist who creates and manages cases, notes, programmes and PDFs.

### Admin user

A practice user who may help with owner/pet setup, document preparation or housekeeping.

### Pet owner

The owner is not a system user in Release 1. They receive a PDF generated from the admin system.

## Release 1 in scope

- Admin login placeholder or simple local authentication, depending on implementation phase.
- Dashboard.
- Owner management.
- Pet management.
- Treatment case management.
- Treatment case notes.
- Exercise library.
- Programme builder.
- Programme sessions such as AM, PM or single daily session.
- Exercise prescription details: reps, sets, hold seconds, frequency/period, notes and instructions.
- PDF preview.
- PDF generation.
- Published programme version history.
- Search/filter over owners, pets, cases and exercises.
- Synthetic seed data for development and demonstration.
- GDPR-conscious design: minimal data, role-based access later, audit history, deletion/anonymisation workflow where clinically/legal appropriate.

## Release 1 out of scope

These are not to be built in the first small implementation unless Nick explicitly extends the scope:

- Owner mobile app.
- Owner login.
- Offline sync.
- Exercise completion tracking.
- In-app comments or discomfort scoring.
- Push notifications.
- JSON programme publication to mobile.
- Vet XML export.
- RDF/semantic exercise reasoning.
- Multi-practice tenant administration beyond designing with future tenant isolation in mind.

## Future phases to keep architecture-ready

Although Release 1 is admin plus PDF, the design should not block later development of:

- .NET MAUI mobile app for pet owners;
- JSON programme snapshots for mobile delivery;
- offline programme viewing;
- offline recording of completions, skipped sessions, discomfort and owner comments;
- deferred sync using local SQLite and a sync queue;
- XML export for vets or third-party interchange;
- analytics to compare programme/exercise outcomes once sufficient data exists.

## Design principle

Build the smallest useful clinical administration workflow first, then refine layout and behaviour before adding breadth. The first implementation should validate the user journey and Hello Buddy visual direction before investing heavily in backend complexity.
