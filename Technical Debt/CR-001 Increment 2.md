# CR-001 Increment 2 - Admin Standards Compliance Review

Date: 2026-06-02
Scope reviewed: Canine Physio Admin `src` and `tests`
Reference standard: `Standards/coding-standards.md`
Reviewer: GitHub Copilot (GPT-5.3-Codex)

## Outcome
The current Increment 2 codebase does **not** fully meet the coding standard. The key deviations are listed below, ordered by severity.

## Findings

### 1. Critical - Target framework policy mismatch (`net8.0` required, `net9.0` used)
Standard: Section 3 requires `net8.0` (LTS).

Evidence:
- `src/HelloBuddy.Api/HelloBuddy.Api.csproj:4`
- `src/HelloBuddy.Ui/HelloBuddy.Ui.csproj:4`
- `src/HelloBuddy.Application/HelloBuddy.Application.csproj:4`
- `src/HelloBuddy.Infrastructure/HelloBuddy.Infrastructure.csproj:4`
- `src/HelloBuddy.Admin.Core/HelloBuddy.Admin.Core.csproj:4`
- `src/HelloBuddy.Contracts/HelloBuddy.Contracts.csproj:4`
- `src/HelloBuddy.Admin.Pdf/HelloBuddy.Admin.Pdf.csproj:12`
- `src/HelloBuddy.PdfService/HelloBuddy.PdfService.csproj:4`
- `tests/HelloBuddy.Api.IntegrationTests/HelloBuddy.Api.IntegrationTests.csproj:4`
- `tests/HelloBuddy.Api.InMemoryTests/HelloBuddy.Api.InMemoryTests.csproj:4`
- `tests/HelloBuddy.Ui.Tests/HelloBuddy.Ui.Tests.csproj:4`

Impact:
- Breaks explicit platform baseline and increases runtime/tooling drift risk.

Recommended action:
- Align all projects and package set to `net8.0` unless an ADR explicitly approves `net9.0`.

---

### 2. Critical - Architecture/dependency direction differs from standard boundaries
Standard: Section 2 requires dependency direction `Web -> Application -> Domain`, with data access in Infrastructure and no reverse/leaky coupling.

Evidence:
- `src/HelloBuddy.Application/HelloBuddy.Application.csproj:8` references `HelloBuddy.Contracts` directly.
- `src/HelloBuddy.Api/HelloBuddy.Api.csproj:12` references `HelloBuddy.Admin.Core` (where `CaninePhysioDbContext` lives).
- `src/HelloBuddy.Api/Program.cs:4` imports `HelloBuddy.Admin.Core.Data` directly.

Impact:
- Application and API are coupled to data/contracts in ways that weaken clean boundaries and future refactoring.

Recommended action:
- Introduce/restore explicit Domain layer and move persistence-only concerns behind Application abstractions.
- Remove direct Core/EF dependencies from API endpoints.

---

### 3. High - Business/data-access logic implemented in Web/API endpoint layer
Standard: Section 2 and Section 8 require web layer to stay thin (no business logic, no direct EF usage).

Evidence:
- `src/HelloBuddy.Api/Endpoints/ProgrammeEndpoints.cs:17`
- `src/HelloBuddy.Api/Endpoints/ProgrammeEndpoints.cs:28`
- `src/HelloBuddy.Api/Endpoints/ProgrammeEndpoints.cs:63`
- `src/HelloBuddy.Api/Endpoints/ProgrammeEndpoints.cs:102`

Details:
- `CaninePhysioDbContext` is injected and used directly in endpoint handlers.
- Update/publish orchestration and data mutation logic is performed in endpoint class.

Impact:
- Harder to test and evolve; web layer carries business responsibilities.

Recommended action:
- Move programme update/publish logic to Application services and repository interfaces.
- Keep endpoint handlers as transport adapters only.

---

### 4. High - Audit field handling does not follow interceptor policy
Standard: Section 5 requires audit fields to be populated in a `SaveChangesInterceptor`, not manually in services/repositories.

Evidence:
- `src/HelloBuddy.Infrastructure/Records/RecordRepositories.cs:402` (`CreatedDate = DateTime.UtcNow`)

Impact:
- Inconsistent audit behavior and policy bypass risk.

Recommended action:
- Implement an EF `SaveChangesInterceptor` for audit fields and remove manual timestamp assignment from repository methods.

---

### 5. High - Security model deviates from required authorization policy approach
Standard: Section 13 requires explicit AuthN/Z policy usage (`[Authorize]` / fallback policy, deny-by-default).

Evidence:
- `src/HelloBuddy.Api/Program.cs:93` custom header gate middleware
- `src/HelloBuddy.Api/Program.cs:100` returns 401 when `X-Practitioner-Id` missing/invalid
- No `[Authorize]` attributes found in reviewed `src` controllers/endpoints

Impact:
- Security is currently based on a custom header contract rather than framework authorization policies.

Recommended action:
- Move to ASP.NET Core authentication/authorization middleware and policy-based access controls.
- Keep temporary header approach only behind an explicit time-boxed ADR.

---

### 6. Medium - PDF template implementation violates templating rule
Standard: Section 9 requires templates in PDF layer as Razor templates; avoid string-concatenated HTML.

Evidence:
- `src/HelloBuddy.Api/Services/ProgrammePdfTemplate.cs:17`
- `src/HelloBuddy.Api/Services/ProgrammePdfTemplate.cs:18`

Impact:
- Template maintainability and preview/PDF parity are harder to guarantee.

Recommended action:
- Move template to PDF project templates directory and render through Razor-based template engine.

---

### 7. Medium - Domain language violation (`patient` term used)
Standard: Section 3 domain language forbids `patient`; use `pet` and defined terms.

Evidence:
- `src/HelloBuddy.Api/Services/ProgrammePdfTemplate.cs:44` (`Patient:`)

Impact:
- Terminology drift against requirements pack and UI/domain consistency.

Recommended action:
- Replace `Patient` with approved domain term (`Pet`) in generated document output.

---

### 8. Medium - One-top-level-type-per-file rule violated in multiple files
Standard: Section 3 naming/file convention expects one top-level type per file with matching file name.

Evidence:
- `src/HelloBuddy.Application/Records/RecordContracts.cs` contains multiple top-level types (e.g., lines 7, 16, 25, 35, 44)
- `src/HelloBuddy.Ui/Models/RecordPageModels.cs` contains multiple top-level types (lines 6, 12, 19, 26)
- `src/HelloBuddy.Contracts/OwnerContracts.cs` contains multiple top-level types
- `src/HelloBuddy.Contracts/PetContracts.cs` contains multiple top-level types
- `src/HelloBuddy.Contracts/TreatmentCaseContracts.cs` contains multiple top-level types

Impact:
- Reduced discoverability and weaker convention consistency.

Recommended action:
- Split top-level types into separate files and align file names to type names.

---

### 9. Medium - Public Application interfaces missing required XML docs
Standard: Section 15 requires XML doc comments on public Application interfaces.

Evidence:
- `src/HelloBuddy.Application/Records/RecordContracts.cs:16` (`IOwnerRepository`)
- `src/HelloBuddy.Application/Records/RecordContracts.cs:25` (`IPetRepository`)
- `src/HelloBuddy.Application/Records/RecordContracts.cs:35` (`ITreatmentCaseRepository`)

Impact:
- API intent and usage contracts are less explicit for maintainers.

Recommended action:
- Add XML docs for all public Application interfaces/methods.

---

### 10. Medium - Test strategy diverges from Testcontainers requirement
Standard: Section 12 requires Infrastructure integration tests using Testcontainers MySQL and schema scripts.

Evidence:
- `tests/HelloBuddy.Api.IntegrationTests/README.md:3` states tests run against a real/local MySQL database.
- `tests/HelloBuddy.Api.IntegrationTests/ApiIntegrationTests.cs:113` hard-coded localhost fallback connection string.
- `tests/HelloBuddy.Api.InMemoryTests/ApiInMemoryTests.cs:84` uses EF InMemory provider.
- No Testcontainers package references in test project files.

Impact:
- Reduced reproducibility and environment-dependent integration test behavior.

Recommended action:
- Add Testcontainers-based MySQL fixture and apply schema/seed scripts from database build scripts.

---

### 11. Low - UI viewport meta tag does not match mandated mobile directive
Standard: Section 18 requires exact viewport content:
`width=device-width, initial-scale=1, viewport-fit=cover`

Evidence:
- `src/HelloBuddy.Ui/Views/Shared/_Layout.cshtml:6` currently uses `width=device-width, initial-scale=1.0`

Impact:
- Minor deviation from prescribed mobile-safe viewport behavior.

Recommended action:
- Update viewport meta content to the mandated value.

---

### 12. Low - Inline styles present in UI views outside PDF template
Standard: Section 8 says no inline styles outside PDF template.

Evidence:
- `src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml:41`
- `src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml:42`
- `src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml:43`
- `src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml:44`
- `src/HelloBuddy.Ui/Views/Programmes/Builder.cshtml:45`
- `src/HelloBuddy.Ui/Views/Programmes/Preview.cshtml:128`
- `src/HelloBuddy.Ui/Views/Programmes/Preview.cshtml:129`
- `src/HelloBuddy.Ui/Views/Programmes/Preview.cshtml:130`
- `src/HelloBuddy.Ui/Views/Programmes/Preview.cshtml:131`

Impact:
- Styling consistency and maintainability drift from site stylesheet/token approach.

Recommended action:
- Move column widths and styling into stylesheet classes.

## Notes
- Anti-forgery usage on reviewed POST actions was present and aligned.
- CancellationToken usage on async repository/API methods was generally aligned.
- This review is based on current Increment 2 code in `src` and `tests`; generated/vendor files were not treated as authored violations unless directly used in architecture decisions.

---

## Resolution (post-review)

Resolved in two passes; deferred items captured for Increment 2.x.

### Pass A - low-risk fixes
- **#1 Target framework** - Standard updated to `net9.0` (ADR-002 note in `Standards/coding-standards.md`). All projects already on `net9.0`; decision ratified rather than reverted.
- **#7 Domain term** - PDF template now emits `Pet:` (no `Patient` references remain).
- **#8 One type per file** - Split `RecordContracts.cs`, `RecordPageModels.cs`, `OwnerContracts.cs`, `PetContracts.cs`, `TreatmentCaseContracts.cs` into per-type files.
- **#9 XML docs** - Added XML doc comments to all public Application interfaces (`IOwnerRepository`, `IPetRepository`, `ITreatmentCaseRepository`, `IProgrammeRepository`, `IProgrammeService`).
- **#11 Viewport** - `_Layout.cshtml` now uses `width=device-width, initial-scale=1, viewport-fit=cover`.
- **#12 Inline styles** - Builder/Preview column widths moved to `.programme-table` classes in `site.css`.

### Pass B - medium refactors
- **#3 Thin endpoint** - `ProgrammeEndpoints.cs` rewritten as transport-only. New `IProgrammeService` / `ProgrammeService` orchestrate the workflow; new `IProgrammeRepository` / `ProgrammeRepository` hold the EF queries.
- **#4 Audit interceptor** - Added `AuditSaveChangesInterceptor` (reflection-based; stamps `CreatedDate` / `UpdatedDate`). Registered on `CaninePhysioDbContext` in `Program.cs`. Manual `CreatedDate` assignment removed from `RecordRepositories.cs`.
- **#6 Razor PDF template** - Replaced string-concatenated HTML with `RazorEngineCore` template (`Templates/Programme.cshtml` embedded resource) behind `IProgrammePdfTemplate` in `HelloBuddy.Admin.Pdf`. `IFileStore` also moved into the Pdf project to remove the API->Api.Services coupling.
- **#10 Testcontainers** - Added `Testcontainers.MySql` fixture (`MySqlTestcontainerFixture`) applying `Canine Physio DB Scripts v2.3 (fresh).sql` + Day-1 + MSc seed scripts. Container uses `--lower-case-table-names=1` for Windows/Linux parity; schema DDL executed as root (DROP/CREATE DATABASE).

### Deferred to Increment 2.x (ADR required)
- **#2 Domain layer / dependency direction** - Introducing a separate `HelloBuddy.Domain` project and removing Application's reference to `HelloBuddy.Contracts` plus API's reference to `HelloBuddy.Admin.Core` is a structural refactor that touches every feature. Deferred to its own increment.
- **#5 AuthN/Z policy** - Replacing the `X-Practitioner-Id` header gate with ASP.NET Core authentication + deny-by-default policies is gated on choosing an identity provider for the Increment 3 mobile/owner flows. Header remains under a time-boxed ADR.

### Re-test results
- `dotnet build HelloBuddy.Admin.sln` - clean.
- `HelloBuddy.Api.InMemoryTests` - 3 / 3 passed.
- `HelloBuddy.Api.IntegrationTests` (host MySQL + Testcontainers) - 3 / 3 passed (~49s).
- `HelloBuddy.Ui.Tests` - 6 / 6 passed.
