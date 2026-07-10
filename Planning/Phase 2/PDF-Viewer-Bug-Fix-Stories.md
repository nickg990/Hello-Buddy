# PDF Viewer — Bug-Fix Stories (Programme PDF)

**Created:** 2026-07-09
**Status:** Implementation complete — PDF-S1, PDF-S2, PDF-S3, PDF-S4 all done and unit-tested (13/13 green). Pending: combined api+ui deploy + live A4 verification.
**Scope:** Hello Buddy — Canine Physiotherapy **Admin** (`Canine Physio Admin`), Programme PDF generation path
**Source:** Four defects found during live testing of the Exercise Programme Builder → View PDF → PDF Viewer flow.
**Implementation model:** AI-led implementation (Sonnet 4.6, measured in minutes) + human testing (~30–60 min per story). Estimates include 20% contingency. Azure PDF-image rebuild/rollout allowance added where a live check is needed.

> **Standards:** Follow [Standards/coding-standards.md](../../Standards/coding-standards.md) and the admin `docs/adr` conventions (clean-architecture-lite, async + `CancellationToken` end-to-end, domain language — "programme" never "program", constructor injection, `TreatWarningsAsErrors`). Any deviation requires an ADR + a one-line entry in the decision log.

---

## Working notes for the implementer (Sonnet 4.6) — read first

These are deliberately explicit because they reflect where an AI implementer tends to drift on this codebase:

1. **The data already flows.** Exercise `Notes` are persisted and mapped all the way to the PDF view model. Do **not** re-plumb the model — the only missing step for Bug 1 is *rendering* and *length-limiting*. Verify the chain before writing code:
   `ProgrammeBuilderForm.SessionExerciseEdit.Notes` → [ProgrammeRepository.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Infrastructure/Programmes/ProgrammeRepository.cs) `UpdateAsync` (`entity.Notes = edit.Notes;`) → `ProgrammeVm.SessionExerciseRow.Notes` ([ProgrammeVm.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Contracts/ProgrammeVm.cs)) → template field `ex.Notes`.
2. **One template drives the PDF:** [Canine Physio Admin/src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml). It is a RazorEngineCore string template — `@@page` is an escaped `@page` (keep the double-at).
3. **Rendering engine:** [PuppeteerPdfRenderer.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/PuppeteerPdfRenderer.cs) (headless-shell Chromium, A4, `PrintBackground = true`, `MarginOptions` currently 12 mm all round) runs in the **pdf** container and only converts pre-built HTML. **Chromium print quirk:** when the CSS declares `@page { margin: ... }`, that CSS wins over `PdfOptions.MarginOptions` for the page box. **Decision:** Bug 4's margin is implemented in the template CSS (API side), **not** in this C# file — do **not** edit `PuppeteerPdfRenderer.cs`. Still **verify against a real generated PDF** that the effective margin is the CSS value.
4. **Do not change stored enum values.** `Period` is stored lowercase (`"single"`) via `ProgrammeDomainConstants.SessionPeriodSingle` and is used in logic. Bug 2 is a **display** fix only.
5. **Verify visually.** These are layout defects; unit tests alone cannot prove them. Each story lists a concrete manual verification against an actual A4 PDF. Where a live check is called for, rebuild the PDF image (`az acr build -t hello-buddy-pdf:<tag> -f Dockerfile.pdf -r acrhellobuddyprod .` from the admin solution root) and redeploy `ca-hello-buddy-pdf`; App Insights/Log Analytics ingestion lags ~1–2 min.
6. **Keep changes minimal and sequential.** Do one story at a time, build after each (`dotnet build "Canine Physio Admin/HelloBuddy.Admin.sln"`), and keep `TreatWarningsAsErrors` green. Do not refactor unrelated CSS.
7. **Line endings:** `Programme.cshtml` is a normal source file — leave it LF/as-is; do not reformat the whole file.

---

## Increment: PDF Viewer defect remediation

### Story PDF-S1: Render exercise notes on the prescription line (60-char limit)

> **✅ Implemented 2026-07-09.** Notes render inline on `.ex-prescription` after the hold segment (truncate 60 + `…`); `[StringLength(60)]` on `ProgrammeBuilderForm.SessionExerciseEdit.Notes`; builder input `maxlength="60"` + hint. Unit tests green.

#### a) User story and brief for Sonnet
**User story**
As a practitioner, I want the exercise note I entered in the Programme Builder to appear on the PDF **on the same line as reps, sets and hold seconds**, in the **same font and size** as reps/sets, limited to **60 characters** so it always fits on one line, so that owners see my short cue alongside the prescription.

**Brief for Sonnet**
Current state: notes are captured, persisted and mapped end-to-end but **never rendered**. In [Programme.cshtml](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml) the prescription line is:
```html
<p class="ex-prescription">@(ex.Reps?.ToString() ?? "&mdash;") reps &middot; @(ex.Sets?.ToString() ?? "&mdash;") sets@(ex.HoldSeconds.HasValue ? " &middot; " + ex.HoldSeconds.Value + "s hold" : "")</p>
```
The note lives on `ex.Notes` (`ProgrammeVm.SessionExerciseRow.Notes`).

Implement:
1. **Render notes inline** on the existing `.ex-prescription` paragraph, appended after the hold segment, separated by the same `&middot;` delimiter, only when `ex.Notes` is non-empty. Because it sits inside `.ex-prescription` it inherits the correct **9.5 pt** font/size automatically — do **not** introduce a new class with a different size. HTML-encode the note (Razor `@` encodes by default; ensure no `Html.Raw`).
2. **60-character cap at three layers** (defence in depth, so the single-line guarantee holds regardless of entry path):
   - **Client:** add `maxlength="60"` to the notes input in [_BuilderEditor.cshtml](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Programmes/_BuilderEditor.cshtml) (the `name="@(prefix).Notes"` text input) and a short `form-text` hint "Max 60 characters".
   - **Server (model):** add `[StringLength(60)]` to `ProgrammeBuilderForm.SessionExerciseEdit.Notes` ([ProgrammeBuilderForm.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Contracts/ProgrammeBuilderForm.cs)); the builder PUT already binds this form.
   - **Template (last resort):** when rendering, if a note somehow exceeds 60 chars, truncate to 60 and append an ellipsis `…` (so legacy/over-long data can never break the layout). Keep the truncation in the template so the PDF is self-protecting.
3. Do **not** widen the DB column or change persistence logic; `entity.Notes = edit.Notes;` stays as-is.
4. Tests:
   - In-memory API/UI test that a note ≤ 60 chars round-trips and a > 60 char note is rejected/annotated by validation (`[StringLength(60)]`).
   - A template/unit assertion (or golden-HTML assertion if one exists) that the rendered prescription line contains the note text after the hold segment and that a 70-char note renders truncated to 60 + `…`.

Constraints: keep the note **on the same physical line** — it must share the `.ex-prescription` paragraph, not wrap to a new element. Same font/size as reps/sets (inherited). Async + `CancellationToken` unaffected (no new IO). Respect `TreatWarningsAsErrors`.

#### b) Acceptance criteria
- Entering a note in the builder, saving, and viewing the PDF shows the note on the reps · sets · hold line, same font and size.
- The builder input will not accept more than 60 characters; server rejects an over-length note; an over-length note that reaches the template renders truncated with `…`.
- No note → the prescription line is unchanged (no trailing `&middot;`).

#### c) Estimate (incl. 20% contingency)
| Phase | Base | With 20% |
|-------|------|----------|
| AI implementation (template render + 3-layer cap + tests) | ~15 min | ~18 min |
| Human testing (enter note, save, view PDF; over-length attempt) | 30 min | 36 min |
| Azure deploy (PDF image) + rollout wait | ~20 min | ~24 min |
| **Total** | **~1.1 h** | **~1.3 h** |

---

### Story PDF-S2: Capitalise the session period header ("Single")

> **✅ Implemented 2026-07-09.** Period display-cased at render (first char upper, culture-invariant); `single`→`Single`, `AM`/`PM` unchanged; stored constant untouched. Unit tests green.

#### a) User story and brief for Sonnet
**User story**
As a practitioner, when I build a **single-session** programme, I want the PDF session header to read **"Single"** (capital S), not "single", so the document looks professionally cased.

**Brief for Sonnet**
Current state: [Programme.cshtml](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml) renders the session heading raw: `<div class="session-heading"><h2>@session.Period</h2></div>`. `Period` is stored lowercase `"single"` (`ProgrammeDomainConstants.SessionPeriodSingle`), while `"AM"`/`"PM"` are already upper-cased. Do **not** change the stored constant — it is used in logic elsewhere.

Implement:
1. Display-case the period **at render time only**. Preferred: a tiny, culture-invariant display helper (e.g. a `static string ToDisplayPeriod(string period)` in the PDF template's model/helpers, or an existing display-formatting location) that upper-cases the first letter for word-style periods (`single` → `Single`) while leaving acronyms (`AM`, `PM`) untouched. Apply it in the heading: `<h2>@DisplayPeriod(session.Period)</h2>`.
2. If a helper location is not obvious, an inline template expression that upper-cases the first character and leaves the remainder is acceptable — but must leave `AM`/`PM` as-is (they are already upper, so first-char upper is a no-op for them).
3. Keep it culture-invariant (`CultureInfo.InvariantCulture`) to avoid Turkish-I style issues.
4. Test: assertion that `single` → `Single`, `AM` → `AM`, `PM` → `PM`.

Constraints: display-only; no DB or constant change; no new dependency. Respect `TreatWarningsAsErrors`.

#### b) Acceptance criteria
- Single-session programme PDF header shows "Single".
- AM/PM programme headers still show "AM"/"PM".
- Stored `Period` value is unchanged (`"single"`).

#### c) Estimate (incl. 20% contingency)
| Phase | Base | With 20% |
|-------|------|----------|
| AI implementation (display helper + apply + test) | ~8 min | ~10 min |
| Human testing (view single + AM/PM PDFs) | 20 min | 24 min |
| Azure deploy (PDF image) + rollout wait | ~20 min | ~24 min |
| **Total** | **~0.8 h** | **~1.0 h** |

> **Batch note:** PDF-S1–S4 all touch the same template + PDF image. Recommend implementing all four, then a **single** PDF-image rebuild/redeploy to avoid four rollouts. The per-story Azure lines above are therefore additive only if deployed separately.

---

### Story PDF-S3: Never split an exercise across a page boundary (A4)

> **✅ Implemented 2026-07-09.** Added legacy `page-break-inside: avoid` alongside modern `break-inside: avoid` on `.ex-row`, `.ex-row-top`, `.ex-instructions`; comment added near ERR-AT-013. Needs live multi-page A4 verification.

#### a) User story and brief for Sonnet
**User story**
As an owner, I want each exercise to stay wholly on one page — if it will not fit in the remaining space, it should start on a new page — so I never have an exercise cut in half across an A4 page break.

**Brief for Sonnet**
Current state: [Programme.cshtml](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml) already sets `.ex-row { break-inside: avoid; }`. Live testing shows exercises still splitting. Likely causes: (a) only the modern `break-inside` is present — Chromium print honours the legacy `page-break-inside` more reliably; (b) the atomic unit is larger than `.ex-row` alone when the instruction list (`<ol class="ex-instructions">`) is a sibling **inside** `.ex-row` — confirm the whole exercise (media + info + instructions) is within a single `.ex-row` so avoiding a break on `.ex-row` keeps it together; (c) a tall exercise (large image + many instructions) can exceed page height and *must* still break — acceptable only when the exercise genuinely cannot fit on one page.

Implement:
1. Add the legacy property alongside the modern one on the atomic exercise container: `.ex-row { break-inside: avoid; page-break-inside: avoid; }`. Reinforce on inner blocks if needed (`.ex-row-top`, `.ex-instructions`) so the instruction list cannot detach from its exercise.
2. Confirm the instruction `<ol>` is rendered **inside** the same `.ex-row` element as the exercise header/media (per current markup it is) so the "avoid" applies to the whole unit.
3. Keep `break-after: avoid` on `.session-heading` (already present) so a session title never sits alone at the foot of a page.
4. Do **not** force each exercise onto its own page (no blanket `break-before`) — only break when it will not fit. The `break-inside: avoid` behaviour already gives "move to next page if it doesn't fit here"; that is the required behaviour.
5. Note in a code comment (near the existing `ERR-AT-013` note) that both modern and legacy break properties are required for Chromium print reliability.

Constraints: pure CSS in the template; no renderer change required for this story. Must be validated on a real multi-page A4 PDF (a programme with enough exercises to overflow one page). Respect existing session-flow behaviour (AM/PM sessions still flow continuously per `ERR-AT-013`).

#### b) Acceptance criteria
- On a programme long enough to span ≥ 2 pages, **no exercise (media + prescription + instructions) is split** across a page break; an exercise that would overflow starts on the next page.
- Sessions still flow continuously (AM then PM) rather than each forced to a new page.
- A session heading never appears as the last line on a page with its first exercise overflowing to the next.

#### c) Estimate (incl. 20% contingency)
| Phase | Base | With 20% |
|-------|------|----------|
| AI implementation (CSS break rules + comment) | ~8 min | ~10 min |
| Human testing (multi-page programme, inspect every page boundary) | 40 min | 48 min |
| Azure deploy (PDF image) + rollout wait | ~20 min | ~24 min |
| **Total** | **~1.0 h** | **~1.2 h** |

---

### Story PDF-S4: Small top and bottom margin on every page (A4)

> **✅ Implemented 2026-07-09.** CSS-only: `@@page { margin: 10mm 0; }` + `.pdf-header { margin-top: -10mm; }` for page-1 full-bleed; precedence documented in CSS comment + DEC-016 in Admin Decision Log. `PuppeteerPdfRenderer.cs` unchanged. Needs live multi-page A4 verification (page 2+ margins, page-1 header).

#### a) User story and brief for Sonnet
**User story**
As an owner, I want a small, consistent top and bottom margin on **every** page (not just page 1), so content on continuation pages does not run right to the top or bottom edge of the A4 sheet.

**Brief for Sonnet**
Current state: the CSS declares `@@page { size: A4; margin: 0; }` and the C# renderer sets `MarginOptions` 12 mm all round ([PuppeteerPdfRenderer.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/PuppeteerPdfRenderer.cs)). Because the CSS `@page { margin: 0 }` overrides Chromium's `PdfOptions.MarginOptions`, continuation pages currently have **no** top/bottom margin — content butts against the sheet edge. Page 1 looks fine only because the full-bleed dark **header band** provides visual top spacing; page 2+ have no header, so they need an explicit page margin. The `.content` wrapper has `padding: 0 12mm 10mm 12mm` (top padding is 0), which does not help continuation pages.

The design intent is: **full-bleed dark header on page 1** *and* **a small top/bottom margin on all pages**. These two goals conflict if you simply add a top `@page` margin (it would push the page-1 header down and leave a white strip above it). Choose the approach that satisfies both and **verify on a real PDF**.

**Decision (agreed with product owner, 2026-07-09): implement the margin in CSS only, in the API-rendered template.** The API builds the HTML and hands finished HTML to the PDF service purely for Chromium conversion, so the margin belongs in the template's CSS `@page`, **not** in the PDF service's C# `MarginOptions`. Do **not** modify [PuppeteerPdfRenderer.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/PuppeteerPdfRenderer.cs). This keeps the **pdf** container out of scope for the whole bug batch.

Implement (CSS-only; confirm empirically):
1. Set a small **top and bottom** page margin that applies to every page via the template CSS, e.g. `@@page { size: A4; margin: 10mm 0; }` (top/bottom 10 mm, left/right 0 so the horizontal full-bleed of the header band is preserved; horizontal insets already come from `.content` and `.pdf-header` padding).
2. **Chromium/`MarginOptions` interaction:** the PDF service currently passes `MarginOptions` 12 mm all round, but CSS `@page` margin takes precedence for the page box, so the CSS value is what will apply. Leave the C# untouched; just verify on a real PDF that the effective margin is the CSS 10 mm (not a doubled/added value). Document this precedence in a CSS comment next to the `@page` rule so the next maintainer understands why the C# margins appear inert.
3. To keep the **page-1 header full-bleed to the very top** despite the new top margin, pull the header up into the top margin using a negative top margin on `.pdf-header` (e.g. `margin-top: -10mm;`). If negative-margin bleed proves unreliable in headless-shell Chromium (test it), prefer accepting a uniform small top margin on page 1 too (header no longer full-bleed to the very edge) — **ask the product owner only if the visual result is ambiguous**; otherwise pick the uniform-margin result, which most directly satisfies this story's requirement.
4. Ensure the **bottom** margin also applies on every page so the footer/disclaimer and any last exercise do not touch the bottom edge.
5. Add a one-line decision-log entry (or ADR) recording the CSS-only margin mechanism and the page-1 header treatment.

Constraints: A4. **CSS-only — no change to the PDF service.** Must be verified on a **multi-page** PDF — check page 2+ top and bottom spacing explicitly. Do not regress Bug 3 (no exercise splitting) or the header appearance on page 1 more than necessary. Respect `TreatWarningsAsErrors`.

#### b) Acceptance criteria
- Every page, including continuation pages, has a small (≈10 mm) top and bottom margin; no content touches the top or bottom edge on page 2+.
- Page-1 header remains visually correct (full-bleed to left/right; top treatment agreed and documented).
- The margin is implemented in CSS `@page` only; [PuppeteerPdfRenderer.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/PuppeteerPdfRenderer.cs) is unchanged; the CSS-vs-C# precedence is documented in a comment + decision-log line.

#### c) Estimate (incl. 20% contingency)
| Phase | Base | With 20% |
|-------|------|----------|
| AI implementation (CSS `@page` margin + header reconcile + decision-log) | ~12 min | ~15 min |
| Human testing (multi-page PDF, measure top/bottom on page 2+, check page-1 header) | 40 min | 48 min |
| Azure deploy (**api** image) + rollout wait | ~20 min | ~24 min |
| **Total** | **~1.1 h** | **~1.3 h** |

---

## Suggested execution order & combined deploy

1. **PDF-S2** (trivial display fix) → 2. **PDF-S1** (notes render + cap) → 3. **PDF-S3** (no split) → 4. **PDF-S4** (page margins, most layout-sensitive; do last so it is verified against the final content).

**Container impact:** all four fixes are template (API-rendered) + builder input + shared Contracts. The rebuild set is **api + ui only**; the **pdf** container is unchanged (S4 is CSS-only per the agreed decision). Implement and locally verify all four, run `dotnet build "Canine Physio Admin/HelloBuddy.Admin.sln"` and the test suite once, then do a **single** combined rebuild + redeploy of **`ca-hello-buddy-api` and `ca-hello-buddy-ui`** for one combined live verification. This collapses the Azure rollout waits into one.

**Combined live verification checklist (one multi-page A4 PDF):**
- [ ] Notes appear on the reps · sets · hold line, same font/size, ≤ 60 chars (PDF-S1)
- [ ] Single-session header reads "Single"; AM/PM unchanged (PDF-S2)
- [ ] No exercise is split across any page boundary (PDF-S3)
- [ ] Every page (esp. page 2+) has a small top and bottom margin; page-1 header still correct (PDF-S4)

## Files touched (reference)
- [Canine Physio Admin/src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml) — render notes, case period, break rules, **CSS page margins** (S1–S4). *Rendered by the **API**; changing it rebuilds the **api** container.*
- [Canine Physio Admin/src/HelloBuddy.Ui/Views/Programmes/_BuilderEditor.cshtml](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Programmes/_BuilderEditor.cshtml) — notes input `maxlength="60"` + hint (S1). *Rebuilds the **ui** container.*
- [Canine Physio Admin/src/HelloBuddy.Contracts/ProgrammeBuilderForm.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Contracts/ProgrammeBuilderForm.cs) — `[StringLength(60)]` on `SessionExerciseEdit.Notes` (S1). *Shared contract — rebuilds **api** + **ui**.*
- **Not changed:** [PuppeteerPdfRenderer.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/PuppeteerPdfRenderer.cs) — S4 is CSS-only, so the **pdf** container is **out of scope**.
- No change needed: [ProgrammeVm.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Contracts/ProgrammeVm.cs) (Notes already present), [ProgrammeRepository.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Infrastructure/Programmes/ProgrammeRepository.cs) (Notes already persisted + mapped), `ProgrammeDomainConstants.cs` (do not change stored `Period`).

## Container rebuild summary
| Container | Rebuild? | Why |
|-----------|----------|-----|
| **api** | **Yes** | Renders `Programme.cshtml` (S1 notes, S2 period, S3 breaks, S4 CSS margins) + validates `[StringLength(60)]` |
| **ui** | **Yes** | Builder notes input `maxlength` (S1) + shared Contracts validation attribute |
| **pdf** | **No** | S4 is CSS-only in the template; the PDF service only converts pre-built HTML — unchanged |
