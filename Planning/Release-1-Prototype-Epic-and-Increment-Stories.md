# Release 1 Prototype Epic and Increment Stories

**Created:** 2026-06-02  
**Status:** Draft for execution  
**Scope:** Hello Buddy Canine Physiotherapy Admin (Release 1, Increments 2-7)

---

## Epic: Deliver a Fully Working Release 1 Prototype

Deliver a standards-aligned, end-to-end working practitioner admin system that supports owner/pet/case management, case notes, exercise library management, programme authoring, preview, publish, immutable version history, and operational deployment capability.

### Epic outcomes

- Practitioner can complete the full workflow from owner creation through PDF publication.
- Architecture aligns with standards and Increment 2 blockers are paid down before feature expansion.
- Deployment is granular enough to apply/redeploy infrastructure and each app component independently.
- Testing is integration-first: local database verification first, then Azure deployment and re-test.

### Epic non-goals at this stage

- UI styling refinement and visual polish.
- Broad unit test coverage (deferred until solution stability is confirmed).
- Release 1 out-of-scope mobile/offline/owner-app features.

---

## Delivery-wide guardrails

- Database schema remains the source of truth.
- No new feature breadth until Increment 2 blocker debt is cleared.
- Every increment must pass integration tests locally before cloud deployment.
- Every increment must be re-tested in Azure after deployment.
- Unit tests are optional/deferred for now; integration tests are the primary quality gate.
- Material technical decisions are recorded in ADRs and DecisionLog.

### Priority execution lane (agreed)

1. Complete mandatory Increment 2 blocker-remediation stories first (architecture, validation, test harness, deployment split baseline).
2. Prioritise Increment 3 and Increment 4 as the only active feature increments.
3. Do not start Increment 5 or later feature stories until Increment 3 and Increment 4 are completed and re-tested in Azure.

---

## Increment 2: Core Records and Blockers-First Refactor

### Increment objective

Clear architecture/deployment blockers and deliver usable owner, pet, treatment case, and case-note workflows backed by the real database.

### Stories

1. **Story I2-S1: Layered project split and standards baseline**  
As a developer, I want the codebase split into clear Domain/Application/Infrastructure/Web/Pdf boundaries (per container context) so that Release 1 can scale without architectural drift.
2. **Story I2-S2: Data access abstraction and validation pipeline**  
As a developer, I want repository/query abstractions plus FluentValidation and ProblemDetails handling so that CRUD behavior is consistent and maintainable.
3. **Story I2-S3: Owner management CRUD**  
As a practitioner, I want to list, create, edit, and view owners so that owner records are maintained in-system.
4. **Story I2-S4: Pet management CRUD**  
As a practitioner, I want to list, create, edit, and view pets linked to owners so that patient records are complete.
5. **Story I2-S5: Treatment case management CRUD and status**  
As a practitioner, I want to create and maintain treatment cases so that each case is tracked correctly over time.
6. **Story I2-S6: Case note creation and timeline**  
As a practitioner, I want to add dated case notes to a case so that clinical updates are auditable.
7. **Story I2-S7: Integration test harness and local-first verification**  
As a developer, I want integration test projects and local DB test execution so that core workflows are validated before deployment.
8. **Story I2-S8: Azure deployment and post-deploy re-test**  
As a developer, I want Increment 2 deployed and re-tested in Azure so that parity with local behavior is confirmed.
9. **Story I2-S9: Granular infra split foundation (C7 closure start)**  
As a platform engineer, I want infrastructure split into foundation and app-deployable units so that partial updates are safe and fast.

### Increment 2 acceptance focus

- AC-001 to AC-006 functionally satisfied.
- Integration tests pass against local DB.
- Increment deployed to Azure and integration smoke paths re-verified.
- C7 split deployment approach established and usable.

---

## Increment 3: Exercise Library

> **Detailed delivery plan:** [Increment-3-Exercise-Library-Plan.md](Increment-3-Exercise-Library-Plan.md) — authoritative source for scope, contracts, seeder design, test matrix, and Increment 4 handoff.

### Increment objective

Enable maintainable, reusable exercise data for programme creation.

### Stories

1. **Story I3-S1: Exercise list/search/filter**  
As a practitioner, I want to find exercises quickly so that programme authoring is efficient.
2. **Story I3-S2: Exercise create/edit lifecycle**  
As a practitioner, I want to create and update exercises with structured metadata so that instructions are reusable.
3. **Story I3-S3: Active/inactive exercise controls**  
As a practitioner, I want inactive exercises hidden from new drafts so that outdated content is controlled.
4. **Story I3-S4: Exercise data contract alignment**  
As a developer, I want exercise DTOs/contracts validated at boundaries so that downstream programme and PDF layers remain stable.
5. **Story I3-S5: Integration tests for exercise workflows (local DB)**  
As a developer, I want integration tests for create/edit/list/status so that regressions are caught early.
6. **Story I3-S6: Azure deployment and re-test for exercise workflows**  
As a developer, I want deployed validation in Azure so that behavior remains consistent in cloud runtime.
7. **Story I3-S7: Per-component deployment scripts for app-level granularity**  
As a platform engineer, I want per-app deployment scripts so UI/API/PDF can be redeployed independently.

### Increment 3 acceptance focus

- AC-007 and AC-008 satisfied.
- Local integration tests pass.
- Azure re-test completed for exercise stories.

---

## Increment 4: Programme Builder Persistence

> **Depends on Increment 3.** Consume the exercise contracts, `IExerciseRepository`, and seeded exercise data established in [Increment-3-Exercise-Library-Plan.md](Increment-3-Exercise-Library-Plan.md) (see its §9 "Handoff to Increment 4"). Do not introduce parallel exercise inserts in Increment 4 tests — reuse seeded `ExerciseKey` values.

### Increment objective

Support real draft programme authoring from treatment cases.

### Stories

1. **Story I4-S1: Create draft programme from case**  
As a practitioner, I want to create a draft programme tied to a treatment case so that treatment planning is structured.
2. **Story I4-S2: Configure dates and session structure**  
As a practitioner, I want to define programme dates and single/AM/PM structure so that plans match treatment cadence.
3. **Story I4-S3: Add/remove exercises in sessions**  
As a practitioner, I want to assemble session exercises from the library so that programmes are tailored.
4. **Story I4-S4: Prescription editing and notes**  
As a practitioner, I want to edit reps/sets/holds/frequency/notes so that exercise instructions are complete.
5. **Story I4-S5: Exercise ordering persistence**  
As a practitioner, I want reordering preserved so that instruction sequence is clear in preview and PDF.
6. **Story I4-S6: Draft validation before preview/publish**  
As a practitioner, I want clear validation feedback on incomplete drafts so that invalid programmes cannot proceed silently.
7. **Story I4-S7: Integration tests for draft builder (local DB)**  
As a developer, I want end-to-end integration tests of draft creation/editing so that core builder behavior is dependable.
8. **Story I4-S8: Azure deployment and re-test for builder**  
As a developer, I want cloud re-validation so environment differences are detected before next increment.
9. **Story I4-S9: Granular config/secret validation in deployment pipeline**  
As a platform engineer, I want per-component config checks during deploy so partial deploys fail fast and safely.

### Increment 4 acceptance focus

- AC-010 to AC-013 satisfied.
- Local integration suite for builder passes.
- Azure re-test confirms equivalent behavior.

---

## Increment 5: Preview, Publish, and File Delivery

### Increment objective

Deliver the publish path with validation, document generation, storage, and access.

### Stories

1. **Story I5-S1: Unified preview and publish document model**  
As a developer, I want preview and final output generated from the same model/contract so that parity is guaranteed.
2. **Story I5-S2: Publish validation rules implementation**  
As a practitioner, I want blocking validation for critical gaps and warnings for non-blocking issues so that publish quality is controlled.
3. **Story I5-S3: Publish action creates immutable version and stored file**  
As a practitioner, I want publish to create a fixed version and retrievable file so that clinical records are stable.
4. **Story I5-S4: Download/open published document flow**  
As a practitioner, I want secure access to generated documents so I can share via existing channels.
5. **Story I5-S5: Failure handling for render and storage paths**  
As a developer, I want robust error handling for renderer/storage failures so that issues are recoverable and diagnosable.
6. **Story I5-S6: Integration tests for preview/publish/download (local DB)**  
As a developer, I want end-to-end integration tests for publish success/failure paths so high-risk behavior is verified.
7. **Story I5-S7: Azure deployment and cloud re-test for publish path**  
As a developer, I want the publish workflow re-tested post-deploy so runtime and dependency differences are validated.
8. **Story I5-S8: Granular rollback procedure per app component**  
As a platform engineer, I want per-component rollback instructions so failed releases can be reversed quickly.

### Increment 5 acceptance focus

- AC-014 to AC-017 satisfied.
- Publish validation demonstrably enforced.
- Local integration tests pass, then Azure re-test passes.

---

## Increment 6: Versioning, Audit, Security Hardening

### Increment objective

Make published history immutable and improve operational/clinical safety.

### Stories

1. **Story I6-S1: Immutable published version enforcement**  
As a practitioner, I want published versions locked from edit so historical records stay accurate.
2. **Story I6-S2: New draft from published version workflow**  
As a practitioner, I want edits to create a new draft/version so updates do not overwrite history.
3. **Story I6-S3: Version history and traceability view**  
As a practitioner, I want clear version history so treatment evolution is visible.
4. **Story I6-S4: Audit metadata completion and consistency**  
As a compliance-conscious team, I want who/when metadata consistently captured so audit review is possible.
5. **Story I6-S5: Basic access control and boundary hardening**  
As a platform team, I want improved access control patterns so non-demo risk is reduced.
6. **Story I6-S6: GDPR-aware controls for retention/deletion/anonymisation patterns**  
As a compliance-conscious team, I want practical data controls so personal data obligations can be met.
7. **Story I6-S7: Integration tests for version immutability and audit behavior (local DB)**  
As a developer, I want integration tests of history and immutability rules so regressions are prevented.
8. **Story I6-S8: Azure deployment and re-test for hardening outcomes**  
As a developer, I want cloud re-validation of hardening features so production behavior is trusted.
9. **Story I6-S9: Granular observability checks per component deployment**  
As an operator, I want post-deploy health and telemetry checks per component so incidents are caught quickly.

### Increment 6 acceptance focus

- AC-018, AC-020, AC-021 satisfied.
- Local integration tests pass; Azure re-test confirms behavior.

---

## Increment 7: Deployment-Ready Prototype Operations

### Increment objective

Finalize a repeatable, supportable, deployment-ready Release 1 prototype operating model.

### Stories

1. **Story I7-S1: Environment configuration matrix and release runbook**  
As a team, I want environment-specific config and runbooks so deployments are predictable.
2. **Story I7-S2: Backup/restore and seed/runbook validation**  
As an operator, I want tested data setup and restore procedures so recovery is realistic.
3. **Story I7-S3: End-to-end integration regression suite (local DB gate)**  
As a team, I want a stable release gate suite so every increment regression-check is repeatable.
4. **Story I7-S4: Azure release deployment and full cloud regression re-test**  
As a team, I want complete post-deploy regression evidence so release confidence is high.
5. **Story I7-S5: Component-level release/rollback drills**  
As a platform engineer, I want rehearsal of partial deploy and rollback so granular release strategy is proven.
6. **Story I7-S6: Decision and architecture record completion**  
As a team, I want ADR and DecisionLog completion so technical intent remains traceable.

### Increment 7 acceptance focus

- End-to-end Release 1 workflow verified in local integration suite and Azure retest suite.
- Deployment granularity and rollback approach demonstrated.
- Operational documentation complete enough for handover.

---

## Integration test strategy (agreed for now)

1. Integration tests are the primary test investment in Increments 2-7.
2. Test sequence per increment is mandatory:
   - run integration tests against local database;
   - fix failures locally;
   - deploy increment to Azure;
   - run cloud re-test pack against deployed environment.
3. Unit tests are intentionally deferred and can be introduced later after solution stability is confirmed.
4. Existing acceptance criteria drive test case coverage and increment sign-off.

---

## Definition of done per increment

1. Stories in increment meet acceptance intent and standards guardrails.
2. Integration tests pass locally against local DB.
3. Increment is deployed to Azure.
4. Azure re-test pass evidence is captured.
5. Deployment/runbook updates are complete for changed components.
6. ADR/DecisionLog updates are completed for material decisions.

---

## Notes

- This plan intentionally excludes styling/polish tasks at this stage.
- Carried production debts (e.g., stronger service auth, front door/WAF, async PDF scale path) remain visible and should be scheduled into hardening where relevant.
