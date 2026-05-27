# Wireframe & UI Prompt Template — Hello Buddy Admin

**Purpose:** A reusable preamble to paste at the top of any chat (Opus, Sonnet, GPT) when commissioning a new screen, component, partial, or UI iteration for the Canine Physio Admin app. Keeps every model output aligned with the standards, palette, mobile-first rules and WCAG 2.2 AA conformance — without having to re-explain them each time.

**How to use:**

1. Open a new chat with your chosen model (see §0 below for which model).
2. Paste **§1 — Standing brief** verbatim.
3. Fill in **§2 — Per-screen brief** for the specific page.
4. Append **§3 — Deliverables checklist** so the response is structured the same way every time.
5. Review against **§4 — Reviewer checklist** before accepting the output.

---

## 0. Which model to use

| Task                                                                                                        | Model                     |
| ----------------------------------------------------------------------------------------------------------- | ------------------------- |
| First cut of a new page type, new component vocabulary, mobile-collapse rules, hard layout judgement        | **Claude Opus 4.7**       |
| Additional pages that reuse an established pattern, CSS tweaks, copy variations, second/third card variants | **Claude Sonnet 4.5/4.7** |
| Second-opinion review of a finished wireframe before merge                                                  | **GPT-5**                 |
| Mechanical renames, doc tidy-ups                                                                            | **Claude Haiku**          |

Never paste this template into a model and ask it to "design and code" something the standards doc has not yet covered — raise an ADR first.

---

## 1. Standing brief (paste verbatim every time)

> You are designing a screen for the **Hello Buddy Canine Physiotherapy Admin** — a small ASP.NET Core Razor Pages web app used by practitioners to manage owners, pets, treatment cases, clinical notes, an exercise library and exercise programmes, and to publish branded PDF programmes for pet owners.
>
> **You must conform to the following without exception. Treat any conflict with my per-screen brief as a question to ask me, not a rule to break.**
>
> ### Standards source of truth
>
> - `Standards/coding-standards.md` is canonical. In particular:
>   - **§5** UI direction (palette, tone, components),
>   - **§8** Web layer rules (Razor Pages, partials, no business logic in views),
>   - **§18** Responsive & mobile-first design,
>   - **§19** Accessibility — WCAG 2.2 Level AA (release gate).
> - The approved database schema in `Canine Physio Database/` is canonical. Do not invent fields. If a field you need is missing, flag it as a question.
> - Requirements pack `Canine Physio Requirements/04_admin_page_flow_and_layout.md`, `05_ui_design_direction.md` and `06_pdf_programme_requirements.md` describe page flow and visual direction.
>
> ### Visual identity
>
> - Hello Buddy palette (CSS custom properties):
>   - `--c-primary: #6392AE;`
>   - `--c-deep: #28404F;`
>   - `--c-light: #B3CDD6;`
>   - `--c-pale: #D4E0E5;`
>   - `--c-surface: #FFFFFF;`
> - Tone: calm, clinical, friendly, uncluttered. Owner-facing PDF is warmer; admin screens are restrained.
> - System font stack only (no web fonts unless I approve).
> - Rounded cards, generous white space, one clear primary action per screen.
> - Use a `LogoDisc` placeholder where the real Hello Buddy logo asset is not present.
>
> ### Mobile-first (non-negotiable)
>
> - Design the **360 px wide** layout first, then enhance with `@media (min-width: 600px)` (tablet) and `@media (min-width: 1024px)` (desktop).
> - Single column by default. Multi-column only as an enhancement using CSS Grid `auto-fit` + `minmax()`.
> - Navigation: hamburger + optional bottom tab bar on phone; collapsible rail on tablet; full left rail on desktop.
> - Lists render as **stacked cards by default**, promoted to `<table>` only at `≥ 768 px` from a **single Razor partial** — no duplicate markup.
> - Forms: one field per row, labels above, native input types, **input font-size ≥ 16 px**, primary action sticky to the bottom on mobile.
> - Touch hit area ≥ **44 × 44 px**, minimum WCAG target ≥ **24 × 24 CSS px**.
> - No horizontal scrolling at any width ≥ 360 px.
> - No client-side framework. Server-rendered Razor only.
>
> ### Usability (user-centred design) — release gate
>
> Every output must:
>
> 1. **Have one obvious primary task per screen** and make the next step after success unambiguous.
> 2. Use **practitioner language**, not database language. Navigation labels and button copy match the §8 domain terms (owner, pet, treatment case, programme, session, prescription); never _customer_, _patient_, _user_ in UI copy.
> 3. **Front-load button copy with verbs and nouns**: "Publish programme", "Open case", "+ Add exercise". Never "Submit", "OK", "Click here".
> 4. **Show system status at all times**: save state ("Saved 14:02" / "Saving…" / "Offline — will sync"), validation state, wizard step.
> 5. **Prevent errors before they happen**: disable submit while invalid; constrain inputs (date pickers, dropdowns, numeric steppers); confirm before destructive or immutable actions (publish, close case, delete).
> 6. **Recognition over recall**: keep pet / owner / case context visible in headers and breadcrumbs so the practitioner never has to remember whose record they are in.
> 7. **Above-the-fold rule (mobile 360 × 640)**: page title, most-recent status and primary action visible without scrolling.
> 8. **Sensible defaults**: today's date, current practitioner, exercise prescription pre-filled from library defaults or previous published version.
> 9. **Readable rhythm**: 8 px base spacing unit; line length 50–75 characters for body copy; generous whitespace. Crowded clinical UI causes mistakes.
> 10. **Plain English microcopy**: short sentences, active voice, British spelling (`colour`, `programme`), units always shown (`10 reps × 3 sets`), correct singular/plural (`1 exercise`, `2 exercises`).
> 11. **Empty states earn their space**: one-line explanation of what would be here + the primary action that creates the first one. No blank panels.
> 12. **Useful errors**: explain _what is wrong_ and _what to do_, next to the field. No error codes; no "Operation failed".
> 13. **Don't overload screens**: if two actions feel equally primary, the screen is doing two jobs and probably needs splitting.
> 14. **Design for the real context**: practitioner may be one-handed in a barn, interrupted mid-task, returning days later. Design for that, not a desk worker on a 27" monitor.
>
> Full usability standard is `Standards/coding-standards.md` §20 (heuristics, microcopy, IA, layout, forms, perceived performance, measurement). Read it once; this list is the working summary.
>
> ### Accessibility — WCAG 2.2 Level AA (release gate)
>
> Every output must:
>
> 1. Use **semantic HTML** — `<header>`, `<nav>`, `<main>`, `<aside>`, `<footer>`, `<h1>`–`<h6>` in order, never `<div onclick>`.
> 2. Have **one `<h1>`** matching the visible title; headings nest without skipping levels.
> 3. Include a **skip-to-content** link as the first focusable element in the page shell (or note that it lives in `_Layout.cshtml`).
> 4. Show a **visible focus ring** on every focusable element; never `outline: none` without an equal-contrast replacement.
> 5. Have a **label for every form control** (`<label for>` or wrapping); placeholders are not labels.
> 6. Associate validation errors via `aria-describedby`; use `role="alert"` or `aria-live="polite"` for status messages.
> 7. Never convey status by colour alone — pair colour with an icon and text.
> 8. Meet contrast ≥ **4.5:1** for body text, ≥ **3:1** for large text and UI components, against the Hello Buddy palette.
> 9. Respect `prefers-reduced-motion: reduce` — disable non-essential animation.
> 10. Set `lang="en-GB"` on `<html>`; page `<title>` unique and meaningful (`<Screen> — <Context> — Hello Buddy`).
> 11. Give meaningful `alt` text (`alt=""` only for decorative).
> 12. For tables: `<caption>` and `<th scope>`; card-rendered mobile equivalents keep header/value pairing semantic.
> 13. **WCAG 2.2 specifics**:
>     - **SC 2.4.11** Focus not obscured — sticky headers/bottom action bars must not hide the focused element.
>     - **SC 2.5.7** Dragging movements — any drag interaction (e.g. exercise reorder) **must** have a non-drag alternative (up/down buttons + keyboard).
>     - **SC 2.5.8** Target size — minimum 24 × 24 CSS px (we prefer 44 × 44).
>     - **SC 3.2.6** Consistent help — help/contact in the same place on every page.
>     - **SC 3.3.7** Redundant entry — don't re-ask data already entered in the same flow.
>
> ### Domain language (use these terms exactly)
>
> owner, pet, treatment case, case note, programme (never _program_), session (AM / PM / Single), exercise, prescription (reps / sets / hold seconds / frequency), published version. Never _customer_, _patient_, _adherence_.
>
> ### What you must not do
>
> - Don't redesign the database schema.
> - Don't add a CSS framework, JS framework, web font or external CDN without me asking.
> - Don't invent fields, statuses or workflows the requirements don't already describe.
> - Don't produce code with `outline: none`, `<div>` buttons, placeholder-as-label, drag-only reorder, fixed pixel widths, or hover-only affordances.
> - Don't put business logic in `.cshtml` — page models call Application services per §8.
> - Don't soften the WCAG rules to make the layout prettier.

---

## 2. Per-screen brief (fill this in each time)

> ### Screen name
>
> _e.g._ Programme Builder
>
> ### Purpose (one sentence)
>
> _What is the practitioner trying to achieve on this screen?_
>
> ### Primary user
>
> _Practitioner / Admin user / (Pet owner is not a user in Release 1)_
>
> ### Entry points
>
> _Where does the user arrive from? e.g. Case Detail → "Create programme"._
>
> ### Exit points / primary action
>
> _What's the one most important action? Where does it take them?_
>
> ### Key data on the screen (reference schema tables/columns)
>
> _e.g._ Programme.Title, Programme.StartDate, Programme.EndDate, Session.Period, SessionExercise.Reps/Sets/HoldSeconds, Exercise.Title, Exercise.VideoUrl.
>
> ### Sections / regions needed
>
> _Bullet list of the regions you expect on the screen._
>
> ### States to cover
>
> - Empty (no data yet)
> - Loading
> - Loaded / normal
> - Validation errors
> - Saving / saved
> - Error / failure
>
> ### Breakpoint expectations
>
> - Mobile (360 px): _describe the collapse_
> - Tablet (≥ 600 px): _what enhances_
> - Desktop (≥ 1024 px): _final layout_
>
> ### Acceptance criteria reference
>
> _Which AC IDs in `08_acceptance_criteria_and_tests.md` does this screen satisfy?_
>
> ### Open questions for me
>
> _Anything you want clarified before designing — list it here, don't guess._

---

## 3. Deliverables checklist (request these in this order)

Ask the model to return:

1. **Open questions first** — anything ambiguous in the brief, before writing any markup.
2. **Wireframe description** in plain prose: page hierarchy, regions, primary action, what's above the fold at 360 / 768 / 1280.
3. **ASCII or Mermaid sketch** of the layout at the three breakpoints.
4. **Razor markup** (`.cshtml`) for the page and any new partials, using existing components from §5 where possible and proposing new ones explicitly.
5. **Razor page model** stub (`.cshtml.cs`) with the Application service call signature it expects — no business logic, no EF.
6. **CSS** scoped to new components only, using palette tokens, mobile-first media queries, no `outline: none`.
7. **Accessibility notes** — heading outline, focus order, ARIA used and why, keyboard equivalents for any drag/hover, contrast checks for any new colour pairing.
8. **Synthetic data** snippet (anonymised) sufficient to demo the screen.
9. **Playwright test stubs** asserting: page renders at 360/768/1280, no horizontal scroll, primary action visible, axe-core zero violations.
10. **Self-review against §4 below**, with each item explicitly ticked or flagged.

---

## 4. Reviewer checklist (use before accepting the output)

Tick every box. Any unticked box = back to the model.

### Standards conformance

- [ ] Uses Hello Buddy palette tokens, not hard-coded hex.
- [ ] No web fonts, no external CDN, no JS framework.
- [ ] Razor markup only; no business logic in `.cshtml`.
- [ ] Domain language correct (owner, pet, case, programme, session, prescription).
- [ ] Schema fields referenced match the approved DB; no invented columns.

### Mobile-first

- [ ] 360 px layout is the base; tablet/desktop are `min-width` enhancements.
- [ ] Single column at mobile; multi-column uses Grid `auto-fit` + `minmax()`.
- [ ] Lists render as cards on mobile via the same partial used for the table.
- [ ] Primary action sticky to bottom on mobile; visible without scroll.
- [ ] No horizontal scroll at 360 px.
- [ ] Form inputs ≥ 16 px font-size; native input types used.

### Usability (§20)

- [ ] One obvious primary task per screen; one primary action visually dominant.
- [ ] Navigation labels and button copy use §8 domain language; no "Submit", "OK", "Click here".
- [ ] Save state, validation state, and wizard step (if applicable) are always visible.
- [ ] Required fields marked; sensible defaults pre-filled (today, current practitioner, prior version values).
- [ ] Errors prevented by input constraints; remaining errors explain _what is wrong_ and _what to do_, next to the field.
- [ ] Pet / owner / case context visible in header or breadcrumbs (recognition over recall).
- [ ] Empty states have a one-line explanation and a primary create action; no blank panels.
- [ ] Above-the-fold rule at 360 × 640: title + status + primary action visible without scrolling.
- [ ] Spacing on the 8 px rhythm; body line length 50–75 characters; hierarchy scannable in 5 seconds.
- [ ] Microcopy plain English, active voice, British spelling, units and singular/plural correct.
- [ ] Destructive or immutable actions (publish, close case, delete) are confirmed.
- [ ] Browser back button works; wizard Back never loses data.

### WCAG 2.2 Level AA

- [ ] One `<h1>`, headings nest, no skipped levels.
- [ ] Semantic landmarks (`<header>`, `<nav>`, `<main>`, `<footer>`).
- [ ] Skip-to-content link present (or noted as in `_Layout.cshtml`).
- [ ] Visible focus ring on every focusable element.
- [ ] Every form control has a real `<label>`.
- [ ] Errors associated via `aria-describedby`; status uses `aria-live`.
- [ ] Colour never the only signal (icon + text accompany).
- [ ] Contrast ≥ 4.5:1 body / 3:1 large + UI; checked against palette.
- [ ] Any drag interaction has a non-drag keyboard/button alternative (SC 2.5.7).
- [ ] Sticky elements don't obscure focus (SC 2.4.11).
- [ ] Hit areas ≥ 44 × 44 px (minimum 24 × 24).
- [ ] `prefers-reduced-motion` respected.
- [ ] `lang="en-GB"`; page `<title>` unique and meaningful.
- [ ] Images have meaningful `alt` (or `alt=""` if decorative).

### Tests and evidence

- [ ] Playwright stubs at 360 / 768 / 1280.
- [ ] axe-core zero violations expected.
- [ ] Lighthouse Accessibility target ≥ 95 noted for this page if it's one of the four key screens (Dashboard, Case Detail, Programme Builder, PDF Preview).
- [ ] Synthetic data only; no real owner/pet/practitioner names.

### Documentation

- [ ] New components added to the §5 component vocabulary list (or proposed explicitly).
- [ ] Any deviation from the standards documented as an ADR draft, not silently shipped.

---

## 5. Example invocation

> Paste §1 verbatim, then:
>
> > **§2 — Per-screen brief**
> >
> > **Screen name:** Case Detail
> > **Purpose:** Let a practitioner see the full state of a treatment case at a glance and act on it.
> > **Primary user:** Practitioner.
> > **Entry points:** Case list row click; Pet detail → "Open case".
> > **Exit points / primary action:** "Add note" (most frequent); secondary "Create programme".
> > **Key data:** TreatmentCase.Title, .Status, .StartDate, .Goals; Pet.Name, .Breed; Owner.FullName, .Phone; TreatmentCaseNote rows (timeline); current Programme + ProgrammeVersion history.
> > **Sections:** header (pet + owner summary + case status), goals, notes timeline, active programme card, published version history.
> > **States:** empty notes, empty programmes, loading, validation error on add-note inline form.
> > **Breakpoints:** mobile — vertical stack with collapsible sections; tablet — two-column (left summary, right notes); desktop — three-column with programme history as right rail.
> > **AC reference:** AC-005, AC-006, AC-018.
> > **Open questions:** is "Close case" a primary action or hidden under an overflow menu?
> >
> > **§3 — Deliverables:** as listed.

---

## 6. When to skip this template

- Pure CSS token tweaks, copy edits, or single-line bug fixes — overkill.
- ADR-level architectural decisions — use the ADR template instead.
- PDF template work — use this template _plus_ the PDF rules in §9 and §6 of the requirements pack.
