# CR-002 - Exercise Media Upload and PDF Click-Through Video

Date: 2026-06-04
Status: Draft for review
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Release 1)

## Why this change

Physiotherapists need a faster, less error-prone way to manage exercise media. Copy/paste URL entry is clunky and fragile. Uploading an image directly while still supporting a video link improves day-to-day authoring speed and consistency.

Business benefit:
- easier clinician workflow for exercise creation/editing;
- fewer broken/missing media references;
- better owner-facing PDF quality;
- safer path to future mobile/media reuse.

## Requirement clarification from deep dive

What current requirements explicitly say:
- exercise detail/edit includes image/media reference and video link;
- PDF includes image/thumbnail where available and a clickable video link.

What is not explicitly specified today:
- direct image file upload in admin;
- image itself being clickable to open video in PDF.

This CR adds those as explicit requirements.

## Architecture decision recommendation (question resolved)

Question raised: store images in DB or blob storage?

Recommendation: store image files in Blob Storage and persist only the link/URI in database.

Reason:
- current schema stores `Exercise.ImageUrl` and `Exercise.VideoUrl` as URL strings (not binary columns);
- keeping binaries out of relational tables avoids DB bloat and backup/restore overhead;
- existing platform already uses blob/file patterns for generated PDF artefacts;
- this aligns with future mobile media metadata flow.

Decision for this CR:
- image binary -> Blob Storage container;
- database -> canonical image URL in `Exercise.ImageUrl`;
- video remains URL-only (no video file upload in this CR unless separately approved).

## Epic

Epic CR-002-E1: Practitioner-managed exercise media upload and owner-friendly PDF media interaction

As a practitioner,
I want to upload an exercise image and set a video link,
so that programme outputs show clear images and owners can open the relevant video directly from the image/link in PDF.

### Epic outcomes
- Exercise Library supports local image upload + optional remote image URL path.
- Exercise media is persisted safely and consistently (Blob + DB URL).
- Programme/PDF rendering includes exercise image and clickable video behavior.
- Existing published versions remain immutable.
- Existing admin functionality is not regressed.

---

## Stories (full impact coverage)

### Story CR-002-S1 - Media storage model and config
As a platform engineer, I want dedicated exercise-media blob configuration so uploads are durable and environment-safe.

Include:
- new storage settings for exercise media container name and base URL policy;
- Key Vault/config entries and validation on startup;
- explicit content-type allowlist and max upload size.

Acceptance:
- app fails fast with clear error if media storage config is missing;
- media container is resolved in local/dev/cloud environments.

### Story CR-002-S2 - API contract for image upload and URL assignment
As a developer, I want clear API boundaries for media upload and exercise save so UI and API remain maintainable.

Include:
- upload endpoint (multipart) returning stored media URL and metadata;
- exercise save/update continues to persist `ImageUrl` + `VideoUrl`;
- validation for supported file types and URL formats.

Acceptance:
- invalid file type/size returns validation problem;
- successful upload returns canonical URL stored on exercise save.

### Story CR-002-S3 - Exercise Library create/edit UX upgrade
As a practitioner, I want to upload image files from local/remote folders and keep a video link so authoring is quick.

Include:
- create/edit screens support:
  - upload image file;
  - optional manual image URL override;
  - video URL entry;
  - preview of selected/uploaded image;
  - remove/replace image action;
- preserve all current fields, filters, and active/inactive controls.

Acceptance:
- user can create exercise with uploaded image + video URL in one flow;
- edit can replace image without breaking other exercise data.

### Story CR-002-S4 - Exercise list/detail/admin navigation updates
As a practitioner, I want list/detail screens to clearly indicate media availability.

Include:
- list indicator for image present and video present;
- detail screen shows image preview, media URL(s), and current status;
- no regression to existing nav route (`Exercise library`).

Acceptance:
- image and video presence are visible at list/detail level;
- existing list/search/filter behavior remains intact.

### Story CR-002-S5 - Programme builder media propagation
As a practitioner, I want selected exercises in programme builder to carry media metadata to preview/publish.

Include:
- programme view model and query projections include exercise image URL + video URL;
- builder cards optionally show thumbnail and video indicator.

Acceptance:
- exercises loaded in builder include media fields where present;
- no regression to prescription editing/order behavior.

### Story CR-002-S6 - PDF template media rendering and click-through behavior
As an owner-facing output consumer, I want PDF exercise cards to show image and open video via image/link interaction.

Include:
- PDF template updates to render exercise image when available;
- image wrapped with clickable hyperlink to video URL when video exists;
- fallback textual "Watch video" link when image absent;
- print-safe layout degradation when links are not clickable by viewer.

Acceptance:
- generated PDF shows image + clickable interaction where data exists;
- PDFs without media still render cleanly.

### Story CR-002-S7 - Security, governance, and content handling
As a platform/compliance owner, I want upload controls and retention rules for media.

Include:
- file scanning hook/placeholder policy;
- disallow executable/script content;
- deterministic file naming and non-guessable paths;
- optional soft-delete/replace cleanup policy for orphaned media.

Acceptance:
- non-image uploads are blocked;
- replaced images follow defined retention behavior.

### Story CR-002-S8 - Seeder and migration behavior for existing exercises
As a developer, I want predictable behavior for existing seeded/manual exercises.

Include:
- seed updates to include realistic image URLs + video URLs;
- no destructive wipe of unrelated data;
- migration plan for existing exercises with null image values.

Acceptance:
- seed remains idempotent;
- existing non-media exercises remain valid.

### Story CR-002-S9 - Integration tests (in-memory + testcontainer)
As a developer, I want end-to-end tests for upload, save, render, and immutability.

Include:
- upload endpoint tests (happy/invalid paths);
- create/edit exercise with uploaded image URL persisted;
- programme builder retrieval includes media fields;
- PDF render contract test includes clickable link markers;
- AC-009-style immutability re-check after media edits.

Acceptance:
- all media workflow tests pass locally and in testcontainer lane.

### Story CR-002-S10 - UI smoke and regression suite updates
As a developer, I want smoke coverage for new media controls and unchanged core flows.

Include:
- UI tests for create/edit media fields and detail rendering;
- ensure Owners/Pets/Cases/Programmes routes still pass unchanged;
- ensure Exercise navigation and non-media CRUD still works.

Acceptance:
- no regressions across existing admin areas.

### Story CR-002-S11 - Deployment and runbook updates
As an operator, I want safe deployment instructions for media-capable release.

Include:
- infra/deploy script updates for media storage settings;
- per-component deploy guidance remains valid (`-UiOnly`, `-ApiOnly`, `-PdfOnly`);
- runbook updates for troubleshooting media upload failures.

Acceptance:
- component-only redeploy remains usable;
- media settings documented per environment.

---

## Proposed acceptance criteria for this CR

- CR-AC1: Practitioner can upload an image during exercise create/edit and it is persisted as Blob URL in `Exercise.ImageUrl`.
- CR-AC2: Practitioner can still provide/edit `VideoUrl` and it is validated.
- CR-AC3: Programme/PDF output includes exercise image where available.
- CR-AC4: In PDF, image click opens video URL when video exists; fallback text link appears otherwise.
- CR-AC5: Existing published programme versions remain unchanged after media edits to exercises.
- CR-AC6: Existing non-media admin workflows (owners, pets, cases, programme builder core edits) are not regressed.

---

## Out of scope for CR-002

- video file upload/transcoding pipeline;
- image editing/cropping tools;
- CDN optimization and responsive image variants;
- owner mobile app media sync behavior changes.

---

## Dependencies and risks

Dependencies:
- storage configuration per environment;
- PDF template update coordination with publish path.

Risks:
- upload security and content-type spoofing;
- broken external video links;
- PDF viewer differences for clickable image behavior.

Mitigations:
- strict server-side validation;
- fallback textual links in PDF;
- integration tests plus Azure re-test.
