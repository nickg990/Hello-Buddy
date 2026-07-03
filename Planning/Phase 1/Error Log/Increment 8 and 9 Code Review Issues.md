# Increment 8 and 9 Code Review Issues

Date: 2026-06-12  
Status: Open findings from standards review  
Scope: Increment 8, Increment 9, CR017-I8, CR018-I9

## Review basis

- Standards source: `Standards/coding-standards.md`
- Main changed areas reviewed:
  - Authentication, authorization, and admin management
  - GDPR owner data-control flow
  - Programme email send (SMTP + audit)
  - SQL scripts for Increment 8 and Increment 9

## Findings by increment (ordered by severity within each increment)

## Increment 8 Findings

## I89-CR-001: Admin API role enforcement is trust-the-UI, not deny-by-default

Severity: High  
Area: Security / authorization boundary  
Type: Standards deviation

### Evidence

- `src/HelloBuddy.Api/Endpoints/AdminEndpoints.cs` includes comment: "UI-side role enforcement only".
- Admin API endpoints are reachable with `.RequireAuthorization()` only, without explicit admin-role checks.
- `src/HelloBuddy.Api/Services/HeaderPractitionerAccessor.cs` reads `X-Practitioner-Role`, but admin endpoints do not enforce role from that accessor.
- `src/HelloBuddy.Api/Program.cs` middleware gate validates only `X-Practitioner-Id` (and optional allow-list), not administrator role.

### Why this is a standards issue

Coding standards require deny-by-default authorization and explicit authorization requirements on protected surfaces.

### Suggested fix

1. Enforce administrator role at API boundary for all `/api/admin/*` routes.
2. Add a dedicated authorization policy for admin API endpoints (or equivalent endpoint filter/guard) and apply it consistently.
3. Add integration tests proving non-admin requests are rejected for each admin route.

## I89-CR-002: API host lacks explicit authN/authZ pipeline configuration while admin routes require authorization

Severity: High  
Area: Security hardening  
Type: Standards deviation

### Evidence

- `src/HelloBuddy.Api/Program.cs` does not register/authenticate principals via the standard ASP.NET Core auth pipeline.
- `src/HelloBuddy.Api/Endpoints/AdminEndpoints.cs` uses `.RequireAuthorization()` on endpoints.

### Why this is a standards issue

Standards require explicit authentication/authorization enforcement. Relying on route-level authorization metadata without a properly configured auth pipeline risks inconsistent protection.

### Suggested fix

1. Add explicit API authentication/authorization registration and middleware in `Program.cs`.
2. Ensure the policy used by admin endpoints is tied to validated caller identity/role.
3. Add API tests that verify 401/403 outcomes for missing or insufficient identity.

## I89-CR-003: UI authorization is not deny-by-default for clinical controllers

Severity: High  
Area: UI security gating  
Type: Standards deviation

### Evidence

- `src/HelloBuddy.Ui/Program.cs` defines an `AdminOnly` policy, but no fallback deny-by-default policy.
- Core clinical controllers (for example `src/HelloBuddy.Ui/Controllers/ProgrammesController.cs`) are not globally `[Authorize]`.
- Only selected controllers/actions are protected (`AdminController`, parts of `AccountController`).

### Why this is a standards issue

Standards require protected surfaces to be authorization-gated by default and to explicitly declare requirements.

### Suggested fix

1. Apply a fallback authorization policy requiring authenticated users for non-public pages.
2. Keep explicit anonymous allowance only where intended (Home/login entry points).
3. Add UI auth smoke tests for deep-link access to each clinical controller route while signed out.

## I89-CR-004: Hardcoded password value in source

Severity: High  
Area: Secrets handling  
Type: Standards deviation

### Evidence

- `src/HelloBuddy.Infrastructure/Auth/PractitionerLoginSeedHostedService.cs` contains literal `"Password12345!"` in code/comments used by seeding logic.

### Why this is a standards issue

Standards prohibit passwords/secrets in source and require secure configuration handling.

### Suggested fix

1. Remove hardcoded password literals from source and comments.
2. Resolve seed credential from secure configuration for development-only scenarios.
3. Fail startup if required seed secret is not configured in environments where seeding is enabled.
4. Add guardrails/tests ensuring seeded hash is produced from configuration input only.

## I89-CR-005: Antiforgery disable usage lacks the required endpoint-level non-browser control-boundary note

Severity: Medium  
Area: CSRF policy documentation  
Type: Standards conformance gap

### Evidence

- `src/HelloBuddy.Api/Endpoints/AdminEndpoints.cs` applies `.DisableAntiforgery()` on admin routes.
- Existing comments discuss UI-side role enforcement, not the required non-browser CSRF control-boundary rationale.

### Why this is a standards issue

Standards require each antiforgery-disabled endpoint to include an inline explanation of non-browser nature and control boundary.

### Suggested fix

1. Add endpoint-level inline comments documenting why CSRF is not applicable and what boundary protects the endpoint.
2. Confirm caller-auth and ingress constraints are documented consistently with standards.

## I89-CR-006: One-type-per-file convention violated in admin endpoints file

Severity: Medium  
Area: Structure / maintainability  
Type: Standards deviation

### Evidence

- `src/HelloBuddy.Api/Endpoints/AdminEndpoints.cs` contains endpoint mapping class plus multiple request DTO record types in the same file.

### Why this is a standards issue

Standards naming/layout section requires one top-level type per file.

### Suggested fix

1. Move `AddPractitionerRequest`, `RenamePractitionerRequest`, `SetPasswordRequest`, and `ChangeOwnPasswordRequest` into separate files named after each type.
2. Keep endpoint mapping class isolated in `AdminEndpoints.cs`.

## Increment 9 Findings

## I89-CR-007: Documentation drift in owner data-control contract after CR018 behavior change

Severity: Low  
Area: Documentation accuracy  
Type: Standards conformance gap

### Evidence

- `src/HelloBuddy.Application/Records/IOwnerRepository.cs` summary still states hard-delete otherwise anonymise.
- Current implementation in `src/HelloBuddy.Infrastructure/Records/RecordRepositories.cs` performs complete deletion flow and returns only Deleted/NotFound outcomes.

### Why this is a standards issue

Standards require documentation to remain aligned with behavior and avoid drift.

### Suggested fix

1. Update interface XML documentation to reflect current CR018 delete-only behavior.
2. Ensure related contract comments in UI/API layers are also aligned.

## Positive notes from this review

- Increment 8 introduced structured login lockout, role claims, and admin policy usage in UI.
- Increment 9 introduced boundary validation for send-PDF request via FluentValidation (`SendProgrammePdfRequestValidator`).
- Increment 9 maintains Application-level abstraction for email sending (`IEmailSender`) with Infrastructure implementation.
- Increment 9 / CR018 implementation includes practitioner-scoped owner data-control and explicit GDPR deletion audit entry (`ActionType = 'gdpr-deletion'`).

## Suggested remediation order

1. Increment 8: I89-CR-001, I89-CR-002, I89-CR-003, I89-CR-004 (security-critical)
2. Increment 8: I89-CR-005, I89-CR-006 (standards conformance)
3. Increment 9: I89-CR-007 (documentation hygiene)
