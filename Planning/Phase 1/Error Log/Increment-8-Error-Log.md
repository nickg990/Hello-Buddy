# Increment 8 - Error Log

Increment 8 delivered Practitioner Login and Attribution. After the functional work was complete (93/93 tests passing), a coding-standards compliance review was run against `Standards/coding-standards.md`. The entries below capture each deviation the Codex 5.3 agent introduced (it was instructed to follow the standards), the fix applied, and the validation. A test-value sweep is recorded at the end.

All items below are **Fixed**. Post-fix state: build clean (0 warnings / 0 errors, warnings-as-errors), UI tests 30 → 37, in-memory tests 39 → 58, all green. Testcontainers integration tests unaffected.

## ERR-I8-001: Browser POST handlers missing antiforgery validation (CSRF)

**Date:** 2026-06-11  
**Status:** Fixed  
**Severity:** High (security — CSRF / OWASP)  
**Area:** `AccountController`, `AdminController`  
**Type:** Security defect (CSRF, deny-by-default)

### Symptom

None of the new MVC POST actions carried `[ValidateAntiForgeryToken]`. The forms rendered antiforgery tokens (via the `asp-action` tag helper) but the server never validated them, so every login, forced-password-change, logout, and practitioner-management POST was vulnerable to cross-site request forgery.

### Root cause

The agent assumed the Razor Pages "antiforgery on by default" behaviour from §8 applied to MVC controllers. It does not — MVC controller actions only validate when `[ValidateAntiForgeryToken]`/`[AutoValidateAntiforgeryToken]` is applied, and there was no global filter registered. Every other controller in the solution (`OwnersController`, `CasesController`) already used the per-action attribute, so this was an inconsistency the agent missed.

### Fix

Added `[ValidateAntiForgeryToken]` to all POST actions: `AccountController.Login`, `MustChangePassword`, `Logout`; `AdminController.Add`, `Edit`, `SetPassword`, `Delete`, `ChangePassword`. Matches the established `OwnersController` pattern. Forms already inject the token, so protection is complete end-to-end.

### Validation

- UI auth-flow tests updated to GET each page, extract `__RequestVerificationToken`, and post it. 7/7 pass.
- Full UI suite 37/37.

## ERR-I8-002: Personal data (email addresses) written to logs

**Date:** 2026-06-11  
**Status:** Fixed  
**Severity:** High (GDPR — PII in logs)  
**Area:** `PractitionerLoginSeedHostedService`  
**Type:** GDPR / privacy defect

### Symptom

The login-seed hosted service logged practitioner email addresses on seed (two `LogInformation` calls), writing personal data into application logs/telemetry.

### Root cause

Convenience logging during seeding; the agent did not apply §10 ("never log owner contact details … log identifiers only") or §13 ("personal data is never written to logs"). Email is personal data.

### Fix

Changed both log messages to emit `{PractitionerId}` instead of `{Email}`.

### Validation

- Build clean; no test asserted on the log strings, so no behavioural regression.

## ERR-I8-003: Inline style in Razor markup

**Date:** 2026-06-11  
**Status:** Fixed  
**Severity:** Low (standards / maintainability)  
**Area:** `Views/Admin/Practitioners.cshtml`  
**Type:** Standards deviation (§8 — no inline styles)

### Symptom

The Delete form used `style="display: inline;"`, violating §8 ("No inline styles outside the PDF template").

### Root cause

Inline style used as a quick layout fix instead of an existing utility class.

### Fix

Replaced `style="display: inline;"` with the Bootstrap `d-inline` utility class.

### Validation

- UI renders unchanged; full UI suite green.

## ERR-I8-004: Request CancellationToken not threaded (`CancellationToken.None`)

**Date:** 2026-06-11  
**Status:** Fixed  
**Severity:** Medium (correctness / responsiveness)  
**Area:** `AccountController`, `AdminController`  
**Type:** Standards deviation (§5 — async + CancellationToken)

### Symptom

Controller actions called async services with `CancellationToken.None`, so requests could not be cooperatively cancelled when the client disconnected.

### Root cause

The agent did not accept the framework-supplied request token in the action signatures and passed `CancellationToken.None` instead, contrary to §5's "all DB calls async with a CancellationToken parameter" intent.

### Fix

Added a `CancellationToken ct` parameter to every async action and threaded it through to the service calls (e.g. `Login([FromForm] LoginRequest request, string? returnUrl, CancellationToken ct)`).

### Validation

- Build clean; tests green.

## ERR-I8-005: Multiple top-level types per file (view models inlined in controllers)

**Date:** 2026-06-11  
**Status:** Fixed  
**Severity:** Low (standards / maintainability)  
**Area:** `AccountController`, `AdminController`, `Ui/Models`  
**Type:** Standards deviation (§3 — one top-level type per file)

### Symptom

Six request/view-model classes were declared inside the two controller files, breaking §3 ("one top-level type per file, same name as type"). The rest of the UI keeps one view model per file under `Models/`.

### Root cause

Models were inlined for speed rather than placed in `Models/` per the existing convention.

### Fix

Extracted to dedicated files in `src/HelloBuddy.Ui/Models/`: `LoginRequest`, `MustChangePasswordRequest`, `AddPractitionerViewModel`, `RenamePractitionerViewModel`, `SetPasswordViewModel`, `ChangeOwnPasswordViewModel` (namespace `HelloBuddy.Ui.Models`, already imported by `_ViewImports.cshtml`). Removed the now-redundant `@using HelloBuddy.Ui.Controllers` lines from the affected views.

### Validation

- Build clean; views resolve models via `_ViewImports`; tests green.

## ERR-I8-006: No server-side validation (confirm-password never compared)

**Date:** 2026-06-11  
**Status:** Fixed  
**Severity:** Medium (correctness / security)  
**Area:** `Ui/Models` view models  
**Type:** Standards deviation (§6 boundary validation, §13 input validation)

### Symptom

View models were plain POCOs with no validation attributes, so `ModelState.IsValid` was always true server-side and the confirm-password fields were never checked against the new password. A mismatched confirmation would pass the boundary.

### Root cause

The agent relied on client-side behaviour and did not add server-side validation at the boundary, contrary to §6 and §13 ("input validation at the boundary … both — never just one").

### Fix

Added DataAnnotations to the view models: `[Required]`, `[EmailAddress]`, `[Phone]`, password `[MinLength(8)]`/`[DataType(Password)]`, and `[Compare]` between new and confirm password fields. Actions guard on `ModelState.IsValid`.

### Validation

- Build clean; behaviour exercised by the auth-flow and in-memory tests; all green.

## ERR-I8-007: Test sweep — missing auth-service unit coverage (no low-value tests found)

**Date:** 2026-06-11  
**Status:** Fixed  
**Severity:** Medium (test coverage gap)  
**Area:** `tests/HelloBuddy.Api.InMemoryTests`  
**Type:** Coverage gap

### Symptom

The real `LoginService` / `PractitionerAdminService` behaviour (lockout, honeypot, inactivity, must-change-password, duplicate-email, email-conflict, self-deactivation guard, wrong-current-password) was only exercised indirectly through UI stubs. No direct behaviour tests existed for these services.

The complementary half of the request — a check for low-value tests — found **none**: no skipped, tautological, or placeholder tests existed (no `Skip=`, `Assert.True(true)`, `[Ignore]`, or `// TODO` markers), and the in-memory/integration overlap is deliberate layering per §12, not redundancy. Nothing needed removing.

### Root cause

Increment 8 service logic was added without accompanying unit tests; coverage leaned on end-to-end UI flow tests only.

### Fix

Added 19 behaviour-focused unit tests in `tests/HelloBuddy.Api.InMemoryTests/AuthServiceTests.cs` (EF Core InMemory) following the `MethodUnderTest_Scenario_ExpectedResult` naming from §12, covering: honeypot, unknown email, wrong-password increments count, 5-failure lockout, locked rejects valid password, inactive login, success resets state, email casing, must-change-password, force-change; AddPractitioner duplicate-email and success (hashing, role default, MustChangePassword), Rename email-conflict and success, SetPassword clears lockout, ChangeOwnPassword wrong-current rejected and success, Deactivate self-guard and success. Added the required `HelloBuddy.Application` and `HelloBuddy.Infrastructure` project references to the test project.

### Validation

- In-memory suite 39 → 58, all green.
- Full UI suite 37/37.

## ERR-I8-008: Seeded physiotherapist login always rejected in local UI startup

**Date:** 2026-06-11  
**Status:** Fixed  
**Severity:** High (auth workflow blocked)  
**Area:** Local startup / UI auth mode  
**Type:** Environment/configuration defect

### Symptom

Local login attempts for seeded practitioners (for example `james.holloway@caninephysio.local`) returned "invalid email or password" despite seeded rows existing, account active, and no lockout.

### Root cause

UI startup ran with DB-backed auth disabled by default (`Auth:UseDbBackedServices=false` path). In this mode DI resolves `NoOpLoginService`, which intentionally returns `LoginOutcome.InvalidCredentials` for every attempt. The local stack launcher did not set the auth mode, so seeded credentials could never succeed.

### Fix

Updated `Infrastructure/local-dev/run-local-admin-stack.ps1` to set `Auth__UseDbBackedServices=true` when launching the UI process.

### Validation

- Direct DB inspection confirmed seeded users existed and were active/unlocked.
- After enabling DB-backed auth mode, login path moved from deterministic NoOp rejection to real auth-service evaluation.

## ERR-I8-009: UI startup crash after enabling DB-backed auth (missing DbContext registration)

**Date:** 2026-06-11  
**Status:** Fixed  
**Severity:** High (startup failure)  
**Area:** `HelloBuddy.Ui` DI composition  
**Type:** Dependency-injection configuration defect

### Symptom

After enabling DB-backed auth, UI startup failed with service-construction exceptions:

- `Unable to resolve service for type 'HelloBuddy.Admin.Core.Data.CaninePhysioDbContext' while attempting to activate 'HelloBuddy.Infrastructure.Auth.LoginService'`
- same for `PractitionerAdminService`

### Root cause

`Program.cs` registered concrete DB-backed auth services (`LoginService`, `PractitionerAdminService`) when `Auth:UseDbBackedServices=true`, but did not register `CaninePhysioDbContext` in the UI host container.

### Fix

- Added conditional UI-host `CaninePhysioDbContext` registration in `src/HelloBuddy.Ui/Program.cs` when DB-backed auth is enabled.
- Added a guard clause that throws a clear configuration error if `ConnectionStrings:CaninePhysioDb` is missing while DB-backed auth is enabled.
- Added local-development defaults in `src/HelloBuddy.Ui/appsettings.Development.json`:
	- `Auth:UseDbBackedServices = true`
	- `ConnectionStrings:CaninePhysioDb = Server=localhost;Port=3306;Database=canine_physiotherapy;User=root;Password=P3nyf@n01;SslMode=None`

### Validation

- `dotnet build src/HelloBuddy.Ui/HelloBuddy.Ui.csproj` succeeded after the fix.
- UI container now composes `ILoginService` / `IPractitionerAdminService` without DbContext resolution errors.

## ERR-I8-010: Data Control page included an unintended "Back to Practitioners" action

**Date:** 2026-06-11  
**Status:** Fixed  
**Severity:** Low (UI behaviour mismatch)  
**Area:** `Views/Admin/DataControl.cshtml`  
**Type:** UX/acceptance deviation

### Symptom

The GDPR Data Control page displayed a "Back to Practitioners" button in the header, which was not required for the Increment 8 target screen behaviour.

### Root cause

The view reused a split-header layout pattern and introduced an extra navigation action without explicit requirement approval.

### Fix

Removed the "Back to Practitioners" button and simplified the page header to a single title block.

### Validation

- Manual view review confirms the button is no longer rendered.

## ERR-I8-011: Practitioners page showed non-required global action buttons

**Date:** 2026-06-11  
**Status:** Fixed  
**Severity:** Low (UI clutter / acceptance mismatch)  
**Area:** `Views/Admin/Practitioners.cshtml`  
**Type:** UX/acceptance deviation

### Symptom

The Practitioners page rendered a bottom action group containing "GDPR Data Control (RTBF)", "Change My Password", and "Sign Out" buttons.

### Root cause

A convenience action group was added during earlier implementation, but these actions are already available through existing navigation and were not required on this page.

### Fix

Removed the bottom action group from the Practitioners page so only practitioner-management actions remain in context.

### Validation

- Manual view review confirms the three buttons are no longer rendered on the Practitioners page.
