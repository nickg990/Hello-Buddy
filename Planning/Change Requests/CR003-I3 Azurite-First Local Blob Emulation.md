# CR003-I3 - Azurite-First Local Blob Emulation for Whole Admin Application

Date: 2026-06-04
Status: Draft for review
Owner: Platform / Release 1 engineering
Scope: Canine Physio Admin (UI, API, PDF, local tooling, tests, docs)

## Why this change

Local development currently uses mixed storage behavior: some blob-backed production capabilities are emulated via local filesystem fallback. This reduces fidelity and can hide defects in URI handling, container setup, SAS issuance, and blob metadata semantics.

Business and engineering benefit:
- local behavior more closely matches Azure production;
- fewer late-stage environment defects;
- simpler developer mental model (one local storage path);
- better confidence for upcoming image/media upload features.

## Decision

Adopt Azurite as the default local blob emulator for all blob-dependent functionality in the Admin application.

Policy:
- local default = Azurite (required for normal local run/test workflows);
- optional emergency fallback = filesystem mode behind explicit opt-out switch;
- production remains Azure Blob Storage via managed identity.

## Current-state deep dive summary

Today:
- API can use Azure Blob (`AzureBlobFileStore`) when `Storage:BlobServiceUri` is configured.
- API falls back to `LocalFileStore` when blob config is absent.
- local dev launcher starts UI/API/PDF, but does not provision or start Azurite.
- Terraform and cloud runtime are already blob-native.

Gap:
- no standard local Azurite orchestration/config baseline.

---

## Epic

Epic CR003-I3-E1: Blob-consistent local platform using Azurite across all admin blob workflows

As a developer/operator,
I want local runs and tests to use Azurite for every blob-backed feature,
so that local behavior mirrors production storage semantics and regressions are caught earlier.

### Epic outcomes
- all local blob reads/writes route through Azurite by default;
- a single configuration model exists for local storage;
- tests verify blob behavior against emulator semantics;
- emergency local fallback exists without becoming the default path.

---

## Stories (full impact coverage)

### Story CR003-I3-S1 - Local storage mode contract and configuration
As a platform engineer, I want explicit storage mode configuration so local and cloud behavior are deterministic.

Include:
- define `Storage:Mode` (`Azurite`, `Azure`, `FileSystem`) with startup validation;
- default Development profile to `Azurite`;
- centralize storage config in API host composition.

Acceptance:
- Development starts in Azurite mode by default;
- misconfigured mode fails fast with actionable error.

### Story CR003-I3-S2 - Azurite provisioning in local stack tooling
As a developer, I want one command to boot local services plus blob emulator.

Include:
- update local stack script to start/verify Azurite (Docker or npm strategy);
- idempotent container creation for required blob containers;
- health checks and clear console guidance.

Acceptance:
- `run-local-admin-stack.ps1` brings up Azurite + app services in a repeatable way;
- startup reports container readiness.

### Story CR003-I3-S3 - API storage registration for Azurite connection model
As a developer, I want API storage DI to support emulator auth semantics.

Include:
- support connection-string based blob client for Azurite (`UseDevelopmentStorage=true` or explicit endpoint string);
- retain Azure managed-identity path for production;
- keep file-system fallback only as explicit opt-out path.

Acceptance:
- API can read/write/list blobs in Azurite without cloud credentials;
- production registration remains unchanged.

### Story CR003-I3-S4 - Align all blob consumers to unified local blob path
As a platform owner, I want every blob-using feature to share the same local emulator path.

Include:
- published programme PDFs;
- any existing DataProtection blob usage in local mode where applicable;
- upcoming exercise media upload container path conventions.

Acceptance:
- no blob-consuming feature silently falls back to filesystem in default local mode;
- container naming and key conventions are documented and consistent.

### Story CR003-I3-S5 - Local security and SAS behavior parity
As a developer, I want local link-generation semantics to resemble production behavior.

Include:
- ensure read URL generation path is deterministic in local emulator mode;
- define local strategy for signed URL behavior where Azurite feature parity differs;
- document any known emulator limitations.

Acceptance:
- local download links are functional and testable end-to-end;
- parity differences are explicit and non-breaking.

### Story CR003-I3-S6 - Tests: emulator-backed integration lane
As a developer, I want automated tests that exercise blob interactions via Azurite.

Include:
- integration tests for write/read URL flow via emulator;
- negative tests for missing containers/config;
- regression tests for publish/download pathways.

Acceptance:
- integration lane validates blob behavior with Azurite;
- failures clearly isolate storage-mode/config issues.

### Story CR003-I3-S7 - Developer experience and documentation updates
As a team member, I want clear local setup and troubleshooting guidance.

Include:
- Infrastructure README and local run instructions updated;
- troubleshooting for port conflicts, stale containers, emulator resets;
- expected local env vars/settings documented.

Acceptance:
- a new team member can run blob-backed local flows from docs alone.

### Story CR003-I3-S8 - Rollout and fallback governance
As an operator, I want safe rollout with controlled fallback behavior.

Include:
- phased enablement (opt-in -> default-on);
- explicit `FileSystem` fallback switch for emergency unblock only;
- telemetry/log marker for storage mode in startup logs.

Acceptance:
- rollout can be reversed quickly if blocking issue occurs;
- mode selection is visible in logs for support.

---

## Proposed acceptance criteria for this CR

- CR3-AC1: Local Development mode uses Azurite as default blob backend for all blob-dependent features.
- CR3-AC2: API publish/download blob workflow executes end-to-end against Azurite.
- CR3-AC3: Local stack command starts or validates Azurite and required containers.
- CR3-AC4: Integration tests include an emulator-backed storage lane.
- CR3-AC5: File-system storage remains available only via explicit fallback configuration, not by default.
- CR3-AC6: Local setup/runbook docs are updated and validated by clean-machine steps.

---

## Out of scope for CR003-I3

- replacing Azure Blob in production architecture;
- introducing a new non-Azure object store for production;
- media-domain feature work itself (covered in CR002-I3);
- deep infra changes to cloud Terraform beyond local emulation needs.

---

## Dependencies and risks

Dependencies:
- stable Azurite execution path (Docker Desktop or Node/npm);
- aligned local configuration across UI/API/PDF and tests.

Risks:
- emulator parity gaps vs real Azure Blob for certain SAS/permission behaviors;
- local machine variability (ports/process lifecycle);
- accidental drift back to implicit filesystem mode.

Mitigations:
- explicit storage mode configuration with fail-fast startup;
- documented parity limitations and fallback policy;
- CI/local integration tests pinned to emulator-backed scenarios.

---

## Implementation notes for planning

Suggested sequence:
1. Add storage mode config + DI wiring.
2. Add Azurite orchestration to local stack script.
3. Add emulator-backed integration tests.
4. Update docs and enforce default mode.
5. Keep explicit emergency fallback switch.

Definition of done:
- local developer can run full admin stack and publish/download flow without Azure credentials,
- all blob interactions are via Azurite by default,
- existing behavior remains available through explicit fallback only.

