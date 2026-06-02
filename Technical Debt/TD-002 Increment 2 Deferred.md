# TD-002 - Increment 2 deferred items from CR-001

Raised: 2026-06-02
Scope: Canine Physio Admin (Increment 2 standards review closure)
Source: CR-001 Increment 2 review and remediation pass
Owner: Increment 2.x architecture and platform hardening workstream

---

## Decision

Two CR-001 findings were intentionally deferred during Increment 2 closure to avoid high-risk structural/security changes in the same delivery window as standards remediation and test hardening. These items are mandatory follow-up work in Increment 2.x.

Deferred items:
- #2 Domain layer and dependency direction correction
- #5 Replace header-based access control with ASP.NET Core authentication/authorization policies

---

## Deferred item 1 - Domain layer and dependency direction (CR-001 #2)

Standard intent:
- Preserve clear dependency flow and boundaries so web transport, business logic, and persistence can evolve independently.

Current state:
- API and Application still retain coupling that prevents full clean-boundary compliance.
- The full split to an explicit Domain layer was not executed in this pass.

Why deferred:
- This is a broad, cross-cutting refactor across project references, contracts, repositories, and composition roots.
- Executing it together with active standards fixes and test-container adoption would have increased regression risk.

Required completion scope (Increment 2.x):
- Introduce an explicit Domain project and move domain model/behavioral contracts there.
- Ensure dependency direction is enforced in project references and DI registration.
- Remove boundary-leaking dependencies from upper layers.
- Keep web/API handlers transport-thin, with business orchestration in Application services.

Acceptance criteria:
- Build succeeds with updated dependency graph.
- Existing integration and UI test lanes remain green.
- No direct data-access boundary leakage from web transport layer.

---

## Deferred item 2 - AuthN/Z policy model (CR-001 #5)

Standard intent:
- Use ASP.NET Core authentication and authorization policy pipeline (deny-by-default) rather than custom header gates.

Current state:
- Access control still relies on custom practitioner header behavior.

Why deferred:
- Final identity-provider and token strategy alignment is tied to upcoming mobile/owner flow work.
- Moving immediately without that decision would risk rework and unstable policy boundaries.

Required completion scope (Increment 2.x):
- Implement framework authentication middleware and policy-based authorization.
- Replace custom header-gate behavior with policy checks.
- Apply explicit authorization policies on endpoints and enforce deny-by-default posture.

Acceptance criteria:
- Authenticated requests follow framework middleware and policy evaluation.
- Unauthorized/forbidden responses are produced by policy outcomes, not custom header parsing logic.
- Integration tests include authorized and unauthorized access-path coverage.

---

## Validation status at deferral point

Completed in CR-001 remediation pass:
- Build clean.
- In-memory tests: pass.
- Integration tests (including Testcontainers lane): pass.
- UI tests: pass.

Deferred only:
- Domain boundary split (#2)
- AuthN/Z policy migration (#5)

---

## Review checkpoint

Target review date: 2026-06-30

At checkpoint:
- Confirm implementation sequencing for #2 and #5.
- Convert this debt item into actionable engineering tasks if not already scheduled.
- Retire TD-002 only once both deferred items meet acceptance criteria and regression tests are green.
