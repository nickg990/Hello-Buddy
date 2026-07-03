# ACR018 - Name the Downloaded PDF File after the Programme (Header) Name + Date Range

Date: 2026-06-12
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Programme PDF download filename)

## Why this change

When a programme PDF is downloaded, the saved file should have a meaningful, human-readable name
based on the programme's header (the document H1 = the programme name) plus its date range, e.g.
`Buddy home rehab 2026-06-12 to 2026-07-12.pdf`. The current download uses a system-generated
filename, which is not friendly for the practitioner or the owner.

## Scope

In scope:
- The **Download PDF** filename on the PDF Viewer (currently "Preview PDF") page.

Out of scope:
- The canonical blob/version storage name used by Publish/version history — that stays unchanged.
- The PDF content itself.

## Filename format

`<ProgrammeName> <StartDate> to <EndDate>.pdf`

- `ProgrammeName` = the PDF header name (`ProgrammeVm.ProgrammeName`, rendered as the document H1
  in `_ProgrammePreviewDocument.cshtml`).
- `StartDate` / `EndDate` = `ProgrammeVm.StartDate` / `EndDate` formatted `yyyy-MM-dd`.
- If `EndDate` is null, use just `<ProgrammeName> <StartDate>.pdf` (omit the "to <EndDate>").
- The filename must be **sanitised** for filesystem safety: strip/replace characters invalid in
  filenames (`\ / : * ? " < > |`), collapse whitespace, and trim. Always end in `.pdf`.

Example: programme "Buddy home rehab" starting 2026-06-12, ending 2026-07-12 →
`Buddy home rehab 2026-06-12 to 2026-07-12.pdf`.

## Acceptance criteria

1. Downloading a programme PDF saves it with the name `<ProgrammeName> <StartDate> to <EndDate>.pdf`
   (or `<ProgrammeName> <StartDate>.pdf` when there is no end date).
2. The filename is sanitised so it is always a valid, safe filename and ends in `.pdf`.
3. The PDF content is unchanged; only the download filename changes.
4. The canonical blob/version filename used by Publish and version history is unchanged.
5. Implementation conforms to coding standards; suites green.

## Implementation guidance

- In `src/HelloBuddy.Ui/Controllers/ProgrammesController.cs`, the `PreviewPdf` action returns
  `File(pdf.Bytes, pdf.ContentType, pdf.FileName)` on the download branch. Replace the
  `pdf.FileName` argument with a name built from the programme's name + date range.
  - The action has the `id`; fetch the `ProgrammeVm` (or pass the needed fields) to build the
    name. Prefer building the display filename from the programme data already available rather
    than changing the API's stored filename.
- Implement a small, well-named sanitiser helper (e.g. in the UI layer) used only for the
  download filename; do not over-engineer (single use, per coding standards).

## Risks and mitigations

Risks:
- Unsanitised programme names could produce invalid filenames or header-injection issues in the
  `Content-Disposition` header.

Mitigations:
- Sanitise the filename and rely on the framework's `File(...)` result to encode the
  `Content-Disposition` header safely.

## Verification

- Manual: download a programme PDF and confirm the saved filename matches the format and is valid.
- UI/controller test: the download response's `Content-Disposition` filename matches the expected
  sanitised `<ProgrammeName> <StartDate> to <EndDate>.pdf`.
- Full solution builds clean; suites green.
