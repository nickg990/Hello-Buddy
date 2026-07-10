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
| **api** | **Yes** | Renders `Programme.cshtml` (S1 notes, S2 period, S3 breaks, S4 CSS margins, S6 top border) + validates `[StringLength(60)]` |
| **ui** | **Yes** | Builder notes input `maxlength` (S1/S5) + shared Contracts validation attribute |
| **pdf** | **No** | S4 is CSS-only in the template; the PDF service only converts pre-built HTML — unchanged |

---

## Increment: PDF Viewer defect remediation — round 2 (found in live testing 2026-07-10)

> **Depends on PDF-S1–S4 (already implemented 2026-07-09).** These two stories build directly on that work — read the round-1 working notes above first (same template, same RazorEngineCore rules). **Reminder: every `@page` in `Programme.cshtml` — including inside CSS comments — must be written `@@page` or RazorEngineCore fails to compile (bit us on S4).**

### Story PDF-S5: Move the "max 60 characters" note limit off the builder input onto the session period header

> **✅ Implemented 2026-07-10.** Per-row `form-text` hint removed; notes `<th>` header updated to "Notes (max 60 characters)". `maxlength="60"` retained on input. Build + 14 tests green.

#### a) User story and brief for Sonnet
**User story**
As a practitioner, I do **not** want a per-row "Max 60 characters" hint under every notes input (it clutters the builder table). Instead I want the 60-character limit communicated **once per session**, on the session period header, so the header reads **"AM — Notes (max 60 characters)"**, **"PM — Notes (max 60 characters)"** and **"Single — Notes (max 60 characters)"**.

**Brief for Sonnet**
Current state (from PDF-S1): the builder notes input in [_BuilderEditor.cshtml](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Programmes/_BuilderEditor.cshtml) has both `maxlength="60"` and a `<div class="form-text">Max 60 characters</div>` hint rendered under **every** row's notes cell. The builder table `<thead>` has a plain `<th>Notes</th>`.

Implement:
1. **Remove the per-row hint.** Delete the `<div class="form-text">Max 60 characters</div>` line that was added under the notes `<input>` in PDF-S1. **Keep** `maxlength="60"` on the input (the client cap stays; only the visible per-row hint text moves).
2. **Add the limit to the notes column header instead.** Change the builder table header cell from `<th>Notes</th>` to `<th>Notes <span class="text-muted fw-normal">(max 60 characters)</span></th>` (or equivalent) so the "max 60 characters" guidance appears once, in the column header, not on every row.
3. **Scope confirmation on "headers (AM, PM and Single)":** the builder groups exercises **per session**, and each session table is headed by its period (AM/PM/Single). The single "Notes (max 60 characters)" column header therefore appears once under each session's period grouping — satisfying "add it to the headers (AM, PM and Single)". Do **not** alter the **PDF** session period header ([Programme.cshtml](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml)) — this is a **builder-UI-only** change; the PDF does not show the limit text.
4. Do **not** change `[StringLength(60)]` on `ProgrammeBuilderForm.SessionExerciseEdit.Notes` (server cap stays) or the template's 60-char truncation (last-resort cap stays). Only the **visible builder hint** relocates.
5. Tests:
   - UI view/markup test that the per-row `form-text` "Max 60 characters" no longer renders under the notes input.
   - UI view/markup test that the notes column header contains "max 60 characters".
   - Existing PDF-S1 tests (server `[StringLength(60)]`, template truncation) must remain green.

Constraints: builder UI only; `maxlength="60"` retained on the input; server + template caps unchanged. Respect `TreatWarningsAsErrors`.

#### b) Acceptance criteria
- No "Max 60 characters" hint appears under individual notes inputs.
- Each session's notes column header reads "Notes (max 60 characters)".
- Typing in the builder still stops at 60 chars (input `maxlength` intact); server still rejects > 60; template still truncates legacy > 60.

#### c) Estimate (incl. 20% contingency)
| Phase | Base | With 20% |
|-------|------|----------|
| AI implementation (move hint to header + tests) | ~8 min | ~10 min |
| Human testing (builder visual check across AM/PM/Single sessions) | 15 min | 18 min |
| Azure deploy (**ui** image) + rollout wait | ~20 min | ~24 min |
| **Total** | **~0.7 h** | **~0.9 h** |

---

### Story PDF-S6: Restore the top border on the first exercise box when it starts a new page (A4)

> **✅ Implemented 2026-07-10.** `.ex-row` `border-top: 0` replaced with full `border: 1px` + `margin-top: -1px` collapse trick. Comment added. Build + 14 tests green. Needs live multi-page A4 verification.

#### a) User story and brief for Sonnet
**User story**
As an owner, when an exercise box is the **first** on a continuation page, I want it to have a **top border line** like every other box, so it does not look like an open-topped box missing its lid.

**Brief for Sonnet**
Root cause (confirmed): in [Programme.cshtml](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/Templates/Programme.cshtml) the exercise box rule is:
```css
.ex-row { background: #F8FBFC; border: 1px solid #B9CBD4; border-top: 0; padding: 4mm 5mm; break-inside: avoid; page-break-inside: avoid; }
```
`border-top: 0` is deliberate — each `.ex-row` normally *borrows* the bottom edge of the element **above** it (the previous `.ex-row`, or the `.session-heading` which has a full border). This looks correct mid-page. **But when a page break pushes an exercise to the top of a new page, the element it would have borrowed from is on the previous page**, so the new page's first box has **no top line** — an open-topped box.

The fix must add a top border **only** to the box that lands at the top of a page, without double-bordering every mid-page box (which would create a 2px doubled line against the box above). This is a known CSS-print challenge because "first box on a page" is not directly selectable.

Implement (CSS-only in the template; **verify empirically on a real multi-page PDF** — CSS print behaviour varies in headless-shell Chromium):
1. **Preferred approach — give every `.ex-row` its own top border and collapse the overlap.** Set `.ex-row { border-top: 1px solid #B9CBD4; margin-top: -1px; }` (replace `border-top: 0`). The `-1px` top margin pulls each box up by one pixel so its own top border sits exactly over the previous box's bottom border mid-page (no visible doubling), while a box that starts a **new page** keeps its own top border (the negative margin has nothing to overlap at the page top, and the browser clamps it), so the line reappears. **This is the most robust and is the required first attempt.**
2. If the `-1px` overlap proves visually imperfect in headless-shell Chromium (e.g. a faint doubled line or a 1px gap on some boxes), fall back to: keep `border-top: 0` on `.ex-row` and instead add a top border to the **session container's first child** and rely on `break-before`/`box-decoration-break: clone` — but **only** adopt a fallback if approach 1 is demonstrably wrong on a real PDF. Do not over-engineer if approach 1 works.
3. Ensure the change does **not** regress PDF-S3 (no exercise splitting) — the atomic `.ex-row` break rules stay; you are only changing its **border/margin**, not its break behaviour.
4. Ensure `.ex-row:last-child { border-radius: 0 0 2px 2px; }` still renders a clean rounded bottom, and the first `.ex-row` under a `.session-heading` still butts cleanly against the heading (the heading has `border-radius: 2px 2px 0 0` and a full border; with per-box top borders + `-1px` margin the seam should remain clean — **verify**).
5. Add a short CSS comment explaining the `-1px` margin trick and why `border-top` was restored (reference PDF-S6 + the new-page open-box symptom).

Constraints: CSS-only in the template; no renderer change; **must be validated on a multi-page A4 PDF** where at least one exercise starts at the top of a continuation page — confirm that box has a top line and mid-page boxes show no doubled line. Respect `TreatWarningsAsErrors` and the `@@page`-escaping rule.

#### b) Acceptance criteria
- An exercise box that starts at the top of a continuation page has a visible top border line.
- Mid-page exercise boxes show a single clean divider (no doubled 2px line, no 1px gap) between consecutive boxes.
- The first box under a session heading still seams cleanly with the heading; the last box keeps its rounded bottom.
- PDF-S3 (no exercise split across pages) is not regressed.

#### c) Estimate (incl. 20% contingency)
| Phase | Base | With 20% |
|-------|------|----------|
| AI implementation (border + `-1px` margin + comment) | ~8 min | ~10 min |
| Human testing (multi-page PDF; inspect page-top box + mid-page seams) | 35 min | 42 min |
| Azure deploy (**api** image) + rollout wait | ~20 min | ~24 min |
| **Total** | **~1.0 h** | **~1.2 h** |

---

### Round-2 combined deploy note
Both round-2 stories share the template + builder. S5 = **ui** only; S6 = **api** only (template). Implement both, run `dotnet build` + in-memory tests once, then a single combined **api + ui** rebuild/redeploy. **pdf** container remains out of scope (no Chromium change; pinned image untouched).
