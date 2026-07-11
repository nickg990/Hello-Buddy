# Hello Buddy Exercise Import – Claude Handover

## Objective

Implement an importer that reads the structured exercise Markdown file and creates exercise-library records. Existing records must be deleted prior to the load. 


## Input files

- `hello_buddy_exercises_import.md`

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

## Categories

Remove existing categories and use the categories supplied in the Markdown file. Create a category only when it does not already exist, using a case-insensitive comparison. Do not create duplicate categories caused by differences in case or surrounding whitespace.

## Acceptance checks

The implementation is complete when:

- the Markdown file can be parsed without manual editing;
- all valid records are created once;
- rerunning the import does not duplicate existing records;
- blank reps, sets and hold seconds remain null;
- instruction order is preserved;
- categories are reused case-insensitively;
- missing media is reported without stopping unrelated valid imports;
- NSFU media does not generate exercise content;

Wait for approval before making code changes.
