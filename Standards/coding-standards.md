# Hello Buddy — Canine Physiotherapy Admin: Coding Standards

**Status:** Draft v1.0
**Applies to:** `Canine Physio Admin` ASP.NET Core solution (Release 1 onwards).
**Owner:** Nick.
**Change policy:** Any deviation from these standards requires a short Architecture Decision Record (ADR) in `Standards/decisions/` and a one-line note in `DecisionLog.md`.

---

## 1. Guiding principles

These principles override any specific rule below if they conflict.

1. **SOLID** — design for change, but don't speculate about change that hasn't been asked for.
2. **DRY** — duplication of _knowledge_ is the enemy, not duplication of _code lines_. Two similar-looking methods that change for different reasons are not duplication.
3. **YAGNI** — build only what an active acceptance criterion requires. Architecture must not block future increments (mobile JSON, sync, XML export) but must not implement them either.
4. **KISS** — prefer the simplest design that satisfies the requirement and the next foreseeable increment.
5. **Vertical slices first** — a working end-to-end thin slice beats a perfect layer in isolation.
6. **Boundaries are contracts** — anything crossing a layer or process boundary is validated, versioned and explicit.
7. **The database is the source of truth** — schema is already approved; code conforms to the schema, not the other way round.

---

## 2. Solution and project layout

A Clean-Architecture-lite layout in **one** Azure App Service. Multiple projects, single deployable web host.

```text
Canine Physio Admin/
  Canine Physio Admin.slnx
  src/
    HelloBuddy.Domain/            # Entities, value objects, domain exceptions. No deps.
    HelloBuddy.Application/       # Use-case services, DTOs, validators, abstractions. Depends on Domain only.
    HelloBuddy.Infrastructure/    # EF Core, MySQL, Serilog sinks, file/blob storage, external services.
    HelloBuddy.Pdf/               # IProgrammeDocumentRenderer + HTML/Puppeteer implementation.
    HelloBuddy.Web/               # ASP.NET Core host: Razor Pages, MVC controllers, DI composition root.
  tests/
    HelloBuddy.Domain.Tests/
    HelloBuddy.Application.Tests/
    HelloBuddy.Infrastructure.Tests/   # Integration: Testcontainers MySQL
    HelloBuddy.Pdf.Tests/
    HelloBuddy.Web.Tests/              # WebApplicationFactory end-to-end
  docs/
    adr/                          # Architecture Decision Records
```

### Dependency direction (enforced by project references)

```
Web ──► Application ──► Domain
 │           ▲
 ▼           │
Infrastructure ─┘
Pdf ──► Application ──► Domain
```

- **Domain** depends on nothing.
- **Application** depends on Domain only. Defines interfaces (e.g. `IExerciseRepository`, `IProgrammeDocumentRenderer`, `IFileStore`).
- **Infrastructure** and **Pdf** implement Application's interfaces.
- **Web** composes everything via DI in `Program.cs`. No business logic in Web.
- Tests reference whichever project(s) they exercise; never reverse.

### When the layout is overkill

For pure prototyping (Increment 0/1 design-validation), put everything in `HelloBuddy.Web` and lift it out _as_ boundaries crystallise. Once any real DB call or PDF generation lands, move it to the proper project in the same PR.

---

## 3. Language, formatting and tooling

- **Target framework:** `net8.0` (LTS).
- **C# language version:** `latest` (12).
- **Nullable reference types:** `enable` solution-wide.
- **Implicit usings:** `enable`.
- **File-scoped namespaces** everywhere.
- **TreatWarningsAsErrors:** `true` in `Directory.Build.props`.
- **EditorConfig:** root `.editorconfig` defines spaces (4), CRLF, UTF-8, trailing-comma in multi-line collection initializers, `var` only when the type is apparent on the right.
- **Format on save:** `dotnet format` must produce zero diffs in CI.
- **Analyzers:** Microsoft.CodeAnalysis.NetAnalyzers + StyleCop.Analyzers (curated ruleset — see `Standards/stylecop.ruleset`).
- **Centralised package versions:** use `Directory.Packages.props` with `ManagePackageVersionsCentrally=true`.

### Naming

| Element                     | Convention                                     | Example               |
| --------------------------- | ---------------------------------------------- | --------------------- |
| Class, record, struct, enum | `PascalCase`                                   | `ProgrammeVersion`    |
| Interface                   | `IPascalCase`                                  | `IExerciseRepository` |
| Method                      | `PascalCase`, verb-first                       | `PublishAsync`        |
| Async method                | `Async` suffix                                 | `GetByIdAsync`        |
| Local, parameter            | `camelCase`                                    | `programmeId`         |
| Private field               | `_camelCase`                                   | `_dbContext`          |
| Constant                    | `PascalCase`                                   | `MaxSessionExercises` |
| File                        | one top-level type per file, same name as type | `ProgrammeBuilder.cs` |

### Domain language (must match requirements pack)

owner, pet, treatment case, case note, programme (never _program_), session, exercise, prescription, published version. Never _customer_, _patient_, _adherence_.

---

## 4. Dependency injection

- **Constructor injection only.** No `IServiceProvider` in business code, no static service locator.
- **Default lifetime is `Scoped`** for application services and repositories.
- Use `Singleton` only for stateless, thread-safe collaborators (e.g. clock, options).
- Use `Transient` for lightweight stateless helpers (e.g. validators).
- Register collaborators behind their interface in the module they implement, exposed via an extension method:
  - `services.AddHelloBuddyInfrastructure(configuration);`
  - `services.AddHelloBuddyApplication();`
  - `services.AddHelloBuddyPdf();`
- `Program.cs` calls only these extension methods plus framework wiring.
- Forbid the **service locator anti-pattern**. If you find yourself reaching into `HttpContext.RequestServices`, redesign.

---

## 5. Data access (EF Core + MySQL)

- **EF Core 8** with **Pomelo.EntityFrameworkCore.MySql**.
- **Database-first**: the schema is canonical. Entities are scaffolded with `dotnet ef dbcontext scaffold` into `HelloBuddy.Infrastructure/Persistence/Generated/`, then **wrapped** by hand-written aggregates in `HelloBuddy.Domain` where richer behaviour is required.
- **No EF Migrations in Release 1.** Schema changes go through Nick's SQL scripts in `Canine Physio Database/`. Any proposed change is raised as an ADR with the migration SQL attached.
- **DbContext** lives in Infrastructure, exposed to Application only via narrow repository interfaces:
  - `IOwnerRepository`, `IPetRepository`, `ITreatmentCaseRepository`, `IExerciseRepository`, `IProgrammeRepository`, `IProgrammeVersionRepository`.
- Repositories return **aggregates or read-models**, never `IQueryable<T>` to upper layers.
- **Unit of work** is the `DbContext`; commit via `IUnitOfWork.SaveChangesAsync(CancellationToken)`.
- **All DB calls async** with a `CancellationToken` parameter.
- **Audit fields** (`CreatedUtc`, `CreatedBy`, `ModifiedUtc`, `ModifiedBy`) are populated in a `SaveChangesInterceptor`, never by hand in services.
- **Immutability of published programme versions** is enforced in two places: (1) repository refuses updates to a `ProgrammeVersion` whose `Status = Published`; (2) DB-level trigger/constraint where the schema provides one. Both are tested.
- **Query performance:** prefer projection (`.Select(...)`) for list/read-model queries; never load full aggregates for display-only screens.
- **N+1 audit:** integration tests assert query counts for hot paths.

---

## 6. Validation and error handling

- **FluentValidation** validators live next to their command/DTO in `HelloBuddy.Application`.
- Validation runs at the boundary (Razor Page handler or controller). Application services may assume their inputs are structurally valid.
- **Predictable, recoverable failures** (missing video link, validation breach, not-found, conflict) → return a `Result<T>` or `OperationResult` with typed error codes. Do **not** throw.
- **Unexpected failures** (DB down, file system failure, programmer error) → throw. A global middleware translates to RFC 7807 ProblemDetails for API responses and a friendly error page for Razor Pages.
- **Never swallow exceptions.** Catch only to add context, then rethrow.
- **Domain exceptions** derive from `HelloBuddyDomainException` and live in Domain.

### Validation rules for "publish programme" (from requirements §06)

Implement once, in `PublishProgrammeValidator`, reused by preview and publish:
owner exists; pet exists; case exists; title or date range; start + end dates; ≥1 session; each session ≥1 exercise; each exercise has prescription; missing video link = warning, not error.

---

## 7. Domain modelling

- **Aggregates** are the unit of consistency: `Owner` (with `Pets`), `TreatmentCase` (with `Notes`), `Programme` (with `Sessions` → `SessionExercises`), `ProgrammeVersion`.
- **Invariants enforced in the aggregate**, e.g. `Programme.AddExercise(session, exercise, prescription)` rejects duplicates, assigns display order, prevents adding to a published version.
- **Value objects** for things with identity-by-value: `Prescription` (reps, sets, holdSeconds, frequency), `DateRange`, `EmailAddress`, `PhoneNumber`, `VideoLink`.
- **Records** for DTOs and value objects; **classes** for entities/aggregates with identity.
- **Anaemic models are allowed** for pure CRUD entities (Owner contact details, Practitioner) — do not invent behaviour to satisfy a pattern.
- **Mapping**: hand-written or **Mapperly** (source-generated). No AutoMapper.

---

## 8. Web layer (Razor Pages + MVC)

- **Razor Pages** for screens; **MVC controllers** only for non-page concerns (file downloads, webhooks, JSON endpoints).
- **Page-per-feature** folder layout: `/Pages/Programmes/Build.cshtml(.cs)`, `/Pages/Cases/Detail.cshtml(.cs)`.
- Page models are **thin**: gather input, call an Application service, populate view-model, return. No EF, no business rules.
- **DTOs/view-models are distinct from entities.** Never bind a request directly to a domain entity.
- **Antiforgery** on every POST handler (default in Razor Pages — do not disable).
- **No business logic in `.cshtml`.** Conditionals are presentation only.
- **Tag helpers and partials** for layout reuse; reusable components listed in requirements §05 (`AppShell`, `PageHeader`, `LogoDisc`, `DashboardCard`, `OwnerCard`, `PetSummaryCard`, `CaseSummaryCard`, `ExerciseCard`, `SessionCard`, `ProgrammeBuilder`, `PdfPreview`) implemented as Razor partials or view components.
- **CSS**: one site stylesheet driven by CSS custom properties for the Hello Buddy palette tokens. No inline styles outside the PDF template.

### Navigation patterns

The admin uses three complementary navigation patterns. Each has a single, distinct purpose — do not mix them.

#### 1. Entity-driven left navigation — the structural backbone

- The primary navigation always reflects **entities**, not workflows or phases.
- Release 1 nav set: **Dashboard, Owners, Treatment Cases, Exercise Library, Programmes** (plus Pets, Published PDFs and Settings in a later increment, per requirements §04).
- Mirrors the database mental model — practitioners always know where to find a record.
- Carries the 80% of work that is _not_ "new patient" — editing, follow-ups, history, library maintenance.
- On mobile this collapses to a hamburger drawer with optional bottom tab bar (see §18).

#### 2. Workflow wizards — for compound cross-entity intents

- A wizard is a thin Razor shell that orchestrates a small set of standalone forms into one guided flow.
- Use a wizard **only** when a single user intent spans multiple entities and is common enough to warrant short-cutting the left nav.
- Release 1 wizards:
  - **New patient journey** — Owner → Pet → Case (→ optionally Programme). Launched from Dashboard "+ New" or the header quick-add menu.
  - **New programme** — already implicit in the Programme builder; treat it as a wizard with explicit "Details → Sessions → Exercises → Preview → Publish" steps.
- **DRY rule**: a wizard step **must** compose the same Razor partial (`_OwnerForm.cshtml`, `_PetForm.cshtml`, `_CaseForm.cshtml`, …) used by the corresponding standalone page. Never duplicate form markup, validation, or page model logic between the wizard and the standalone entry point.
- **State**: wizards persist progress as draft records as soon as the first step validates — never carry significant state in session/TempData alone. A practitioner interrupted mid-wizard can resume from the Dashboard "Drafts" panel.
- **Accessibility** (per §19):
  - Step indicator marked up as an ordered list with `aria-current="step"` on the active step.
  - Each step has a unique `<h1>` and `<title>`.
  - "Back", "Save and exit", "Next" / "Finish" are always available and keyboard-reachable.
  - The wizard must never trap focus or break the browser back button.
- Wizards are **shortcuts, not silos** — every entity created inside a wizard remains fully editable via its entity-nav page afterwards.

#### 3. In-page tabs — for facets of one record

- Use tabs **only** to organise multiple facets of a single entity on one detail page, not as workflow steps and not for cross-entity navigation.
- Release 1 tabbed detail pages:
  - **Case detail**: Summary / Notes / Programme / History.
  - **Owner detail**: Contact / Pets / Cases / Documents.
- Implementation: native HTML pattern with `role="tablist"`, `role="tab"`, `role="tabpanel"`, arrow-key navigation, `aria-selected`, `aria-controls`. No JS framework required.
- On mobile, tabs collapse to a vertical accordion when there are more than three, to avoid horizontal scrolling at 360 px.

#### Anti-patterns (do not do this)

- **Workflow-driven left nav** (e.g. an "Onboarding" top-level item containing Owner / Pet / Case). Breaks the moment a practitioner needs to edit one record outside the original flow.
- **Tabs as workflow steps**. Tabs imply parallel siblings of equal weight; workflow steps imply a linear order. They are different patterns and must not be conflated.
- **Two routes producing two implementations** of the same form. The wizard and the standalone page must share the same partial, the same validator, and the same Application service call.

---

## 9. PDF layer

### Strategy

A single HTML template renders both the **on-screen PDF preview** and the **downloadable PDF**. The PDF is produced by **PuppeteerSharp** (headless Chromium) rendering that same HTML.

### Abstraction

Defined in `HelloBuddy.Application`:

```csharp
public interface IProgrammeDocumentRenderer
{
    Task<ProgrammeDocumentResult> RenderAsync(
        ProgrammeDocumentModel model,
        ProgrammeRenderOptions options,
        CancellationToken ct);
}

public sealed record ProgrammeDocumentResult(byte[] PdfBytes, string HtmlPreview, string ContentHash);
```

- `ProgrammeDocumentModel` is built **once** from `ProgrammeVersion` data — the same model feeds preview and final PDF, guaranteeing parity.
- The renderer is **deterministic** for a given model and options: same input → identical bytes (used for change detection and tests).
- A second implementation, `QuestPdfProgrammeDocumentRenderer`, sits behind the same interface and can be swapped in via config if Chromium becomes problematic on App Service.

### Rules

- PDF generation runs **server-side only**, never on a request thread for long operations — wrap in `IHostedService`/background channel if generation exceeds 2 seconds.
- Chromium is launched **once per process** and pooled (`IBrowserPool`), not per request.
- All templates live in `HelloBuddy.Pdf/Templates/` as Razor `.cshtml` rendered via `RazorLight` or pre-rendered Razor views; never string-concatenate HTML.
- Output goes to `IFileStore` (local in dev, Azure Blob in prod) — see §11.

---

## 10. Logging, configuration, secrets

- **Serilog** with console + rolling file sinks in dev; Application Insights sink in production.
- **Structured logging**: pass values as message properties, not interpolated strings. `_log.LogInformation("Programme {ProgrammeId} published by {PractitionerId}", id, userId);`
- **Log levels**: `Trace`/`Debug` off in production by default; `Information` for state changes; `Warning` for recoverable issues; `Error` for unexpected failures with exception.
- **Never log** clinical note content, owner contact details, or PDF payloads. Log identifiers only.
- **Correlation ID** middleware on every request; propagated into all log scopes.
- Configuration via `IOptions<T>` bound from `appsettings.{Environment}.json`; **never** `IConfiguration` injected into business code.
- **Secrets**: User Secrets in dev, **Azure Key Vault** in production via Managed Identity. No connection strings, API keys or passwords in source, `appsettings.json`, or commit history.

---

## 11. File storage

- Abstracted by `IFileStore` in Application.
- `LocalFileStore` (Infrastructure) for development — writes under `App_Data/files/`.
- `AzureBlobFileStore` for production — single container per environment, key = `programmes/{programmeId}/v{versionNumber}.pdf`.
- Storage metadata (path, size, content type, hash, generatedUtc, generatedBy) is stored on `ProgrammeVersion`; the file store itself is treated as opaque blob storage.

---

## 12. Testing

- **Framework:** xUnit + FluentAssertions + NSubstitute (mocks only where a real collaborator is awkward).
- **Unit tests**: Domain and Application — fast, no I/O. One assertion topic per test. Arrange / Act / Assert with blank lines.
- **Integration tests**: Infrastructure — **Testcontainers** spin up a real MySQL 8 container, schema applied from `Canine Physio Database/Build and Initialise/*.sql`, seed via Day 1 script.
- **Web tests**: `WebApplicationFactory<Program>` covers each acceptance criterion in `08_acceptance_criteria_and_tests.md` end-to-end.
- **PDF tests**: render a fixed `ProgrammeDocumentModel` and assert the SHA-256 of the resulting bytes matches a checked-in golden hash. Visual regression: store the first byte stream as the golden file; PR fails if hash differs without an explicit update.
- **No test reaches the real Azure resources.**
- **Coverage target**: 80% line coverage on Application + Domain; coverage is a signal, not a goal.
- **Test naming**: `MethodUnderTest_Scenario_ExpectedResult`.

---

## 13. Security and GDPR

- **OWASP Top 10** checklist runs in code review:
  - SQL injection: only EF Core / parameterised SQL. Raw SQL requires reviewer sign-off.
  - XSS: Razor encodes by default — never use `@Html.Raw` on user content.
  - CSRF: antiforgery on by default.
  - AuthN/Z: every page declares its requirement via `[Authorize]` / fallback policy; deny by default.
  - Dependencies: `dotnet list package --vulnerable` runs in CI; PR fails on Critical/High.
- **GDPR-aware behaviour**:
  - Synthetic data only in dev/test.
  - Owner deletion = anonymisation where clinical retention applies; hard delete only when permitted.
  - Audit fields are mandatory and immutable post-write.
  - Personal data is never written to logs, telemetry or error pages.
- **HTTPS only** in non-dev environments. HSTS enabled. Secure + HttpOnly cookies. SameSite=Lax.
- **Input validation** at the boundary; **output encoding** at the view; both — never just one.

---

## 14. Git, branching and PR hygiene

- **Branching**: trunk-based. Feature branches named `feature/<increment>-<short-slug>`, e.g. `feature/inc4-programme-builder-reorder`.
- **Commits**: [Conventional Commits](https://www.conventionalcommits.org/) — `feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `chore:`. Imperative mood, ≤72-char subject.
- **PR size**: small. A PR that touches >400 lines (excluding generated code and test fixtures) needs a justification in the description.
- **PR template** requires: linked acceptance criterion, screenshots for UI changes, test evidence, GDPR/security note if relevant.
- **CI must be green** before merge: build, format, analyzers, unit tests, integration tests, vulnerable-package scan.
- **No direct commits to `main`.**
- **ADRs** for any decision that changes these standards or the architecture; file under `docs/adr/NNNN-title.md` using the Nygard template.

---

## 15. Documentation

- **XML doc comments** on all public Application interfaces and Domain aggregate methods. Internals only commented where intent is non-obvious.
- **README.md** in each project explaining its responsibility in one paragraph and a "how to run / test" snippet.
- **DecisionLog.md** at solution root captures one-line entries linking to ADRs.
- **No documentation drift**: if a code change invalidates a requirements doc, raise it in the PR and update the doc in the same merge train.

---

## 16. Performance budgets (Release 1, indicative)

| Operation                  | Budget                                             |
| -------------------------- | -------------------------------------------------- |
| Page load (warm)           | < 500 ms server time                               |
| Programme builder save     | < 300 ms                                           |
| PDF preview render         | < 1.5 s                                            |
| PDF publish (full)         | < 3 s for typical 2-session, 12-exercise programme |
| DB query for any list page | < 50 ms with seed data                             |

Exceeding a budget is not a blocker but requires a note in the PR.

---

## 17. What this document deliberately does _not_ mandate

- A specific UI component library — keep it CSS + Razor partials until a real need emerges.
- MediatR / CQRS — plain Application services are enough for Release 1.
- Repository pattern over every entity — only where it adds value.
- Event sourcing, messaging, microservices, RDF, Kubernetes — explicitly out of scope per requirements §07.

---

## 18. Responsive and mobile-first design

The admin is primarily used on laptop/tablet but **must remain fully usable on a phone** so a practitioner can pick up a case, add a note or check a programme from anywhere. Mobile-first is a CSS/markup discipline — no framework change.

### Core rules

1. **Design the smallest screen first.** Base styles target a ~360 px wide viewport. Larger layouts are _enhancements_ added via `min-width` media queries — never the other way round.
2. **Breakpoints** (single set, used everywhere; defined as CSS custom properties):
   - `--bp-tablet: 600px`
   - `--bp-desktop: 1024px`
   - `--bp-wide: 1440px`
3. **One column by default.** Multi-column layouts only inside a `@media (min-width: var(--bp-tablet))` block, using CSS Grid with `auto-fit` + `minmax()` so reflow is automatic.
4. **No horizontal scrolling** for any core task at any width ≥ 360 px (per requirements §04).
5. **Viewport meta tag** is mandatory in `_Layout.cshtml`: `<meta name="viewport" content="width=device-width, initial-scale=1, viewport-fit=cover">`.
6. **Fluid type and spacing** via `clamp()` so headings and gaps scale without breakpoint stairs.

### Navigation

- **Mobile (< 600 px)**: top app bar with hamburger that opens a slide-in drawer; optional sticky bottom tab bar for the 4 most-used destinations (Dashboard, Cases, Programmes, Exercises).
- **Tablet (≥ 600 px)**: collapsible left rail, icons + labels.
- **Desktop (≥ 1024 px)**: full left rail per requirements §04.

### Tables and lists

- **Default rendering is a stack of cards** (`_ListItemCard.cshtml` partial).
- A `<table>` layout is applied **only** at `≥ 768 px` via CSS; the same Razor partial supplies both — never duplicate the markup.
- Filters and search collapse into a single "Filter" sheet on mobile.

### Forms

- One field per row on mobile; multi-column form layouts only at `≥ 1024 px` and only where they aid scanning.
- Labels **above** fields (never floating-only — accessibility).
- Input font-size **≥ 16 px** to prevent iOS auto-zoom.
- Primary action **sticky to the bottom** of long forms on mobile; inline at the bottom of the form on larger screens.
- Use native input types (`type="email"`, `type="tel"`, `type="date"`) so mobile keyboards adapt.

### Programme builder on mobile

- Sessions stack vertically; each session is collapsible to keep navigation cheap.
- Exercise cards full-width with prescription summary always visible.
- Reorder via **long-press drag** using a touch-and-mouse capable library (SortableJS). No HTML5-drag-only solutions.
- Add-exercise opens a full-screen modal sheet with search, not a side panel.

### PDF preview on mobile

- Full-width vertically scrollable preview (the same HTML template used for PDF generation, §9).
- Edit ↔ preview is a tab switch on mobile; side-by-side split only at `≥ 1024 px`.

### Touch and input

- **Minimum hit area 44 × 44 px** for every interactive element.
- Hover-only affordances are forbidden — every hover state must have a touch/keyboard equivalent.
- Avoid right-click menus; provide an explicit "…" overflow button.

### Performance budgets for mobile (additive to §16)

| Metric                                                 | Budget on a mid-range Android over 4G |
| ------------------------------------------------------ | ------------------------------------- |
| First Contentful Paint                                 | < 1.5 s                               |
| Largest Contentful Paint                               | < 2.5 s                               |
| Total transferred per page (HTML+CSS+JS, excl. images) | < 150 KB gzipped                      |
| JavaScript bundle                                      | < 50 KB gzipped per page              |

Achieved by: server-rendered Razor, inline critical CSS, system font stack, lazy-loaded below-the-fold images (`loading="lazy"`), no client-side framework, no web fonts unless approved.

### Accessibility (mobile-relevant additions to §19)

- All interactive elements reachable and operable by keyboard _and_ by touch.
- Respect `prefers-reduced-motion` for any animation.
- Respect `prefers-color-scheme` only if a dark theme is formally approved — otherwise pin to light.

### Testing

- Playwright tests in `HelloBuddy.Web.Tests` run every page at **360 × 740, 768 × 1024, and 1280 × 800** and assert: no horizontal scroll, primary action visible, navigation reachable.
- Lighthouse mobile score ≥ 90 for Performance, Accessibility and Best Practices on the Dashboard, Case Detail and Programme Builder pages; checked in CI for the seeded synthetic data.

### Not in scope

- Native mobile app, offline support, push notifications, installable PWA — all explicitly Future Increment 8/9.
- A separate mobile codebase or styleset.

---

## 19. Accessibility — WCAG 2.2 Level AA

The admin app and the owner-facing PDF **must conform to [WCAG 2.2 Level AA](https://www.w3.org/TR/WCAG22/)**. Level AAA is aspirational and welcome where it doesn't compromise the workflow, but is not required.

### Conformance commitment

- Every page, component, partial, modal, error state and empty state ships **WCAG 2.2 AA conformant** or it does not ship. Accessibility is a release gate, not a polish task.
- The owner-facing PDF (§9) is treated as a published document and **also** targets WCAG 2.2 AA: tagged PDF output, reading order, alt text on images, sufficient contrast, no information conveyed by colour alone.
- Every PR description must include an **a11y checklist** confirming keyboard, screen-reader and contrast checks were performed for changed UI.

### WCAG 2.2 success criteria most likely to bite this app

WCAG 2.2 added nine new criteria over 2.1. The ones we will explicitly design for:

| SC         | Title                                    | What it means here                                                                                                                                                   |
| ---------- | ---------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **2.4.11** | Focus Not Obscured (Minimum) — AA        | Sticky headers, bottom action bars and modals must not hide the focused element. Test with keyboard tab order at every breakpoint.                                   |
| **2.5.7**  | Dragging Movements — AA                  | The programme-builder exercise reorder **must have a non-drag alternative** (e.g. up/down buttons or keyboard reorder). Drag is an enhancement, never the only path. |
| **2.5.8**  | Target Size (Minimum) — AA               | Interactive controls **≥ 24 × 24 CSS px** (we already require 44 × 44 in §19 — keep the stricter rule).                                                              |
| **3.2.6**  | Consistent Help — A                      | Help/contact affordance appears in the same place on every page.                                                                                                     |
| **3.3.7**  | Redundant Entry — A                      | Don't ask the practitioner to retype information already entered in the same flow (e.g. owner details when creating a pet).                                          |
| **3.3.8**  | Accessible Authentication (Minimum) — AA | Login must not depend on a cognitive test (no "remember this image" puzzles). Password managers must work; do not block paste.                                       |

Plus the WCAG 2.1 AA criteria still apply in full — reflow at 320 px, text resize to 200%, contrast 4.5:1 (3:1 for large text and UI components), no keyboard traps, visible focus, etc.

### Baseline rules (per page)

1. **Semantic HTML first.** `<header>`, `<nav>`, `<main>`, `<aside>`, `<footer>`, `<h1>`–`<h6>` in logical order. ARIA only where semantics are insufficient. Never `<div onclick>`.
2. **One `<h1>` per page**, matching the visible page title. Headings nest without skipping levels.
3. **Skip-to-content link** as the first focusable element in the page shell.
4. **Visible focus ring** on every focusable element, never `outline: none` without a replacement of equal contrast.
5. **Labels for every form control** (`<label for>` or wrapping `<label>`). Placeholders are not labels.
6. **Errors announced**: validation messages associated to the field via `aria-describedby`, summary region uses `role="alert"` / `aria-live="polite"`.
7. **Status messages** (saved, published, generated) use `aria-live` regions, not toast-only UX.
8. **Colour is never the only signal** — pair status colour with an icon and text.
9. **Contrast** ≥ 4.5:1 for body text, ≥ 3:1 for large text (≥ 18.66 px bold or ≥ 24 px) and UI components/graphical objects. Hello Buddy palette tokens (§05) are tested against this — see the palette contrast matrix in `Standards/palette-contrast.md` (to be added at Increment 0).
10. **Motion** respects `prefers-reduced-motion: reduce` — disable non-essential animation entirely.
11. **Language** set on `<html lang="en-GB">`.
12. **Page titles** are unique and meaningful (`Case detail — Buddy Carter — Hello Buddy`).
13. **Images**: meaningful alt text; decorative images use `alt=""`; exercise thumbnails use the exercise title as alt.
14. **Videos** (exercise links in PDF) open in a new tab with the destination announced; link text is the exercise name, never "click here".
15. **Tables** have `<caption>` and `<th scope>`. Card-rendered tables on mobile keep header/value pairing semantic.

### Tooling and testing

- **axe-core via Playwright** runs on every page at every breakpoint in `HelloBuddy.Web.Tests`. **Zero violations** at WCAG 2.2 AA — build fails on any violation.
- **Lighthouse Accessibility ≥ 95** on Dashboard, Case Detail, Programme Builder, PDF Preview (tightens §19's 90 target for accessibility specifically).
- **Manual screen-reader smoke test** before any increment is marked done:
  - **NVDA + Firefox** (primary)
  - **VoiceOver + Safari iOS** (mobile)
  - Walk the happy path: create owner → add pet → create case → add note → build programme → preview → publish.
- **Keyboard-only walkthrough** of the same happy path before sign-off — no mouse, no touch.
- **Colour-contrast check** for any new palette token or CSS change (axe covers most, but new tokens get an entry in `palette-contrast.md`).
- **PDF accessibility check** via `pdfa-validator` or equivalent on the generated programme PDF — output must be tagged and pass the structural checks.

### Authoring guidance for AI-assisted UI work

- When asking a model (Opus, Sonnet, GPT) to produce Razor markup or CSS, **always include "must conform to WCAG 2.2 Level AA"** in the prompt and paste the relevant subset of this section.
- Reject any generated UI that uses `<div>` for buttons, omits labels, uses colour-only status, removes focus rings, or breaks the heading hierarchy — regardless of how good it looks.
- Wireframes and mockups generated before code must still indicate focus order, heading levels and error states.

### Out of scope (Release 1)

- Multi-language support beyond `en-GB`.
- Personalisation (font size override, high-contrast theme switcher) beyond honouring OS-level preferences.
- AAA criteria (sign language video, extended audio description, etc.).

---

## 20. Usability and user-centred design

§19 is the floor for whether a user _can_ use the app. This section is about whether they _want_ to — whether tasks complete quickly, predictably and without frustration. Usability is a **release gate** alongside accessibility, not a polish task.

### Conformance commitment

- Every screen, component and flow ships to the bar below, or it does not ship.
- Design decisions are justified by **evidence about real practitioners in their real context** — task observation, interview notes, support requests, telemetry — never personal preference or organisational priorities. Where evidence does not yet exist, the decision is recorded as an assumption in an ADR and validated in the next usability pass.
- The five guiding principles below are non-negotiable; the heuristics and patterns underneath them are how we deliver them.

### Five guiding principles

1. **User-centred design.** Make decisions from evidence about real users in real conditions, not "I think practitioners will want…" or "the boss prefers it this way".
2. **Usability is task success.** A screen is usable when target users complete its primary task **effectively** (right outcome), **efficiently** (fewest steps, least cognitive load) and **without frustration** (no dead-ends, surprises or rework).
3. **Clarity, consistency, simplicity.** Say things plainly, do the same thing the same way every time, and remove anything that isn't earning its place on the screen.
4. **Accessibility and usability overlap.** Most §19 rules also make the app faster and clearer for everyone — clear labels, semantic structure, generous targets, no colour-only signals, predictable focus. Treat them as one budget, not two.
5. **Small choices, big impact.** Plain navigation labels, an obvious next step, readable spacing and line length, scannable hierarchy, sensible defaults — these decide whether the practitioner trusts the app or works around it.

### Practitioner context (what we are designing for)

We are not designing for a desk worker on a calibrated 27" monitor. We are designing for:

- A practitioner standing in a clinic, barn, kitchen or kennel.
- Frequent one-handed use while restraining or comforting a dog.
- Mid-task interruptions — phone calls, an owner question, the next appointment arriving.
- Variable lighting, glove or wet hands, mobile data, tablet or phone.
- Returning to the same screen days or weeks later and needing to resume work without re-orienting.

Every layout decision is judged against that reality.

### Industry heuristics we apply

These map to Nielsen's 10 usability heuristics and are baked into PR review:

| #   | Heuristic                                | What we do                                                                                                                                                                                         |
| --- | ---------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1   | Visibility of system status              | Show save state ("Saved 14:02", "Saving…", "Offline — will sync"), validation state and current wizard step.                                                                                       |
| 2   | Match between system and real world      | Use the §8 domain language: owner, pet, treatment case, programme, session, prescription. Never _customer_, _patient_, _user_ in UI copy. Dates in `d MMM yyyy` (`24 May 2026`), times in 24-hour. |
| 3   | User control and freedom                 | Every destructive action has confirm or undo. Wizard "Back" never loses data. Browser back button always works.                                                                                    |
| 4   | Consistency and standards                | Same icon = same meaning. Same control = same behaviour. Primary action sits in the same place on every screen (top-right desktop, sticky bottom mobile).                                          |
| 5   | Error prevention                         | Disable submit while invalid; constrain inputs (date pickers, dropdowns, numeric steppers); confirm before publishing immutable versions; warn before navigating away from unsaved drafts.         |
| 6   | Recognition rather than recall           | Pet/owner/case context lives in headers and breadcrumbs so the practitioner never has to remember whose record they are in. Recently-used exercises surface in the picker.                         |
| 7   | Flexibility and efficiency of use        | Power features — keyboard shortcuts, multi-select picker, bulk reorder, "copy from previous version" — never block the slow path for new users.                                                    |
| 8   | Aesthetic and minimalist design          | Every element justifies its presence. If two cards say the same thing in different words, one goes.                                                                                                |
| 9   | Recognise, diagnose, recover from errors | Plain-English errors next to the field, never error codes; explain _what_ is wrong and _what to do_.                                                                                               |
| 10  | Help and documentation                   | Inline help where it's needed; consistent help affordance (per §19 SC 3.2.6).                                                                                                                      |

### Content and microcopy

- **Plain English.** Reading age ≈ 12. Short sentences. Active voice.
- **Front-load nouns.** Buttons and links describe their destination or action: "Publish programme", not "Submit"; "Open case", not "Click here".
- **Numbers and units.** Always show units (reps, sets, hold seconds, days). Singular/plural correct (`1 exercise`, `2 exercises`).
- **Empty states earn their space.** Empty list = one sentence explaining what would be here + the primary action that creates the first one.
- **Status messages are useful.** "Saved" not "Operation completed successfully". "Couldn't reach the server — your draft is safe locally" not "Error 500".
- **British English** throughout (`colour`, `recognise`, `programme`).

### Information architecture and navigation

- **The three-click rule is a myth — but findability isn't.** Any primary task must be reachable from the dashboard in **≤ 2 deliberate steps**, and the route must be the same every time.
- **Labels are tested, not guessed.** Navigation labels match the user's word, not the database table ("Treatment cases", not "Cases"; "Exercise library", not "Catalogue").
- **Hierarchy visible at a glance.** One `<h1>`, scannable section headings, generous whitespace, primary action visually dominant.
- **Breadcrumbs on any page ≥ 2 levels deep** so the practitioner can always back out.

### Layout, hierarchy, readability

- **Line length 50–75 characters** for body copy. Constrain wide content regions accordingly.
- **Vertical rhythm**: 8 px base unit; spacing increments 8 / 16 / 24 / 32 / 48. No arbitrary margins.
- **Whitespace is a feature**, not wasted space. Crowded clinical UI causes mistakes.
- **One primary action per screen.** Everything else is secondary. If two actions feel equally primary, the screen is doing two jobs and probably needs splitting.
- **Above-the-fold rule (mobile)**: at 360 × 640 the page title, the most-recent status and the primary action are visible without scrolling.

### Forms

- One field per row. Labels above. Native input types. Inline help under the label, never in the placeholder.
- Required fields marked; if optional is the exception, state that in the form legend.
- Validate **on blur**, not on every keystroke. Surface field errors at the field; summarise at the top for keyboard and screen-reader users.
- Sensible defaults wherever possible (today's date, current practitioner, copy from previous version).
- Long forms use sectioned scroll or a wizard (§8) — never a single 30-field wall.

### Feedback and perceived performance

- Any action that takes > **100 ms** shows immediate feedback (button depress, optimistic UI).
- Any action > **1 s** shows a determinate or indeterminate progress indicator.
- Any action > **10 s** can be backgrounded so the practitioner can carry on.
- Skeleton states are preferred over spinners for list and card content; spinners are fine for short ops.

### Measurement and evidence

- Task-success criteria per release live in `08_acceptance_criteria_and_tests.md`. Headline ones for Release 1:
  - New owner → first programme published: median **≤ 8 minutes** end-to-end.
  - Adding 3 exercises to a session via the picker: **≤ 30 seconds**.
  - Resuming a draft the next day: **≤ 15 seconds** to reach the right session.
- Friction signals tracked in Application Insights: rage clicks, dead clicks, abandonment mid-wizard, repeated validation failures on the same field.
- One **usability review** per increment — five-user think-aloud, severity-rated findings, fixes prioritised into the next increment.
- Findings that change a standard or invalidate a pattern in this document → ADR.

### Authoring guidance for AI-assisted UI work

- When commissioning wireframes or Razor, paste the relevant subset of this section alongside §18 and §19 (see `Standards/wireframe-prompt-template.md`).
- Reject any AI-generated UI that buries the primary action, repeats the same label in different words, uses "Submit" or "Click here", or invents fields not in the schema.
- Every wireframe must answer: _what is the one task this screen exists for, and what is the obvious next step after success?_

### Out of scope (Release 1)

- Quantitative usability scoring (SUS, UMUX) — qualitative review + the task-success criteria above are the bar.
- Customisable themes or layouts.
- A/B testing infrastructure.

---

## Appendix A — Quick decision cheatsheet

- New screen? → Razor Page under `/Pages/<Feature>/`, page model calls an Application service.
- New business rule? → Aggregate method in Domain, exercised by an Application service, covered by a unit test.
- New DB read? → Repository method returning a DTO/read-model, projected in EF Core.
- New DB write? → Aggregate change + repository `Update`/`Add` + `IUnitOfWork.SaveChangesAsync`.
- New external dependency? → Interface in Application, implementation in Infrastructure, registered via the module extension method.
- New PDF tweak? → Edit the single HTML template; update the golden hash with reviewer approval.
- Tempted to break a rule? → Write a one-page ADR first.
