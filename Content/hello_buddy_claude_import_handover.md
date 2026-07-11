# Hello Buddy Exercise Import – Claude Handover

## Objective

Implement a one-off importer that reads the structured exercise Markdown file, creates exercise-library records, uploads the associated JPG and MP4 files to the existing Blob Storage locations, and links one image and one video to each exercise.

Use the existing application architecture, repositories, services, validation rules and Blob Storage conventions. Do not redesign unrelated areas of the system.

## Input files

- `hello_buddy_exercises_import.md`
- `hello_buddy_exercise_images.zip`
- The renamed MP4 files supplied separately in the source video ZIP

The Markdown file contains the definitive exercise titles, categories, summaries, dosage fields, media filenames and instructions.

## Markdown record structure

Each record begins with:

```text
## Exercise title
```

It is followed by:

```text
Category:
Summary:
Reps:
Sets:
Hold seconds:
Video:
Image:

Instructions:
First instruction line
Second instruction line
```

Instructions are not numbered in the file. Preserve their order and store each non-empty line as the next ordered instruction. The application already adds numbering when displaying them.

## Import rules

1. Treat each `##` heading as a new exercise.
2. Trim leading and trailing whitespace from all fields.
3. Preserve titles and clinical wording exactly, apart from normal whitespace cleanup.
4. Blank `Reps`, `Sets` and `Hold seconds` values must remain null; do not convert them to zero or invent defaults.
5. Parse numeric dosage fields only when a value is present and valid.
6. Create or reuse the category named in the record.
7. Each exercise supports one image and one video only.
8. Records with different media variants are intentionally separate exercises, even where their instructions are otherwise identical.
9. Do not deduplicate exercises by title alone.
10. Use the source video filename as the import identity where a video exists. For records without video, use a stable hash or equivalent based on title plus image filename.
11. The importer must be safe to rerun without creating another copy of every exercise.
12. Keep imported exercises active. Existing inactive functionality should remain unchanged.

## Media handling

1. Match `Video:` and `Image:` values exactly to supplied filenames.
2. Upload the MP4 and JPG using the application's existing Blob Storage services and naming conventions.
3. Store the resulting Blob references against the exercise record.
4. Do not rename files during import unless the existing storage service requires a generated internal name.
5. If a media file is missing, record a validation error for that exercise and continue processing the remaining valid records.
6. Empty `Video:` or `Image:` fields are valid and must not fail the import.
7. NSFU videos and JPGs are present in the media collection but have no structured Markdown exercise records. Do not create exercise content for them.

## Categories

Use the categories supplied in the Markdown file. Create a category only when it does not already exist, using a case-insensitive comparison. Do not create duplicate categories caused by differences in case or surrounding whitespace.

Do not add category delete functionality as part of this task. Existing inactive behaviour remains the current approach.

## Validation and safety

Before inserting data, provide a dry-run or validation result showing:

- total records found;
- categories to create;
- exercises to create;
- exercises already imported;
- missing image files;
- missing video files;
- invalid numeric dosage values;
- malformed records.

The real import should run only after validation succeeds or the user explicitly accepts reported non-blocking issues.

Use a transaction for database changes where practical. Blob uploads that succeed before a database failure should be cleaned up or reported clearly.

## Logging

Produce a concise import result containing:

- created exercises;
- skipped existing exercises;
- failed exercises with reasons;
- created categories;
- uploaded images;
- uploaded videos.

Do not log passwords, connection strings, storage keys or other secrets.

## Acceptance checks

The implementation is complete when:

- the Markdown file can be parsed without manual editing;
- all valid records are created once;
- rerunning the import does not duplicate existing records;
- blank reps, sets and hold seconds remain null;
- instruction order is preserved;
- each exercise links to the correct single image and video where supplied;
- media variants appear as separate searchable exercises;
- categories are reused case-insensitively;
- missing media is reported without stopping unrelated valid imports;
- NSFU media does not generate exercise content;
- existing exercise-library pages continue to work unchanged.

## Scope control

Inspect only the importer, exercise-library persistence, category persistence and existing Blob Storage integration required for this change. Do not load or refactor the full solution unless a direct dependency makes it necessary.

Before implementation, report:

1. the files/classes that need changing;
2. the proposed import flow;
3. any assumptions or blockers;
4. the estimated scope.

Wait for approval before making code changes.
