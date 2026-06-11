# Increment 8 — Login and System Administration Plan

**Created:** 2026-06-11
**Status:** Draft for review (key decisions confirmed 2026-06-11; minor questions outstanding — see Section 10)
**Scope:** Hello Buddy Canine Physiotherapy Admin — Release 1, Increment 8
**Parent plan:** [Release-1-Prototype-Epic-and-Increment-Stories.md](../Release-1-Prototype-Epic-and-Increment-Stories.md)
**Depends on:** Increment 7 handoff in [Increment-7-Summary-and-Handoff.md](Increment-7-Summary-and-Handoff.md)

---

## 1. Increment goal

Introduce real authentication and a system administration capability to the Canine Physiotherapy
Admin application. Until now the application has run with a single seeded practitioner identity
forwarded to the API via the `X-Practitioner-Id` header (DEC-010). Increment 8 replaces that
implicit identity with an explicit login, attributes data changes to the logged-in
physiotherapist, and adds role-based administration (including relocating the GDPR
right-to-be-forgotten controls into an Admin area).

This increment delivers three capabilities:

1. **Physiotherapist login** — a user must authenticate (user id + password) before any clinical
   functionality is visible. Unauthenticated users see only Home and Login.
2. **Change attribution** — once logged in, all inserts and updates are tagged with the
   physiotherapist id and a timestamp.
3. **Role-based administration** — two roles (physiotherapist, administrator). An administrator
   sees an additional Admin menu offering password reset, practitioner delete, practitioner
   rename (preserving historical attribution names), and the GDPR RTBF control moved from the
   (now removed) Privacy/policy page.

---

## 2. Scope summary

### In scope

- Cookie-based login session in the UI (user id + password) with a login popup/modal.
- A new login/credentials table plus a build-and-seed script, and an initial seeded administrator.
- The login identifier is the practitioner **email** (the existing unique `Practitioner.Email`).
- Navigation gating: only Home + Login visible when signed out; full menu after sign-in; Admin
  menu added for administrators only; a Logout affordance when signed in.
- Forwarding the authenticated practitioner id (and role) from UI to API in place of the static
  seeded header value.
- Audit attribution columns (nullable) for created-by / updated-by practitioner id + name
  snapshot + timestamp on **all tables where the application performs CRUD**, with an update
  (migration) script.
- Admin functions: **add** a physiotherapist (email + password), reset a physiotherapist's
  password (admin sets it directly), delete (soft delete / deactivate) a physiotherapist, rename
  a physiotherapist (without rewriting historical attribution names), GDPR RTBF, and a **change
  admin password** function (belt-and-braces in case physios learn the admin password).
- Removal of the Privacy/policy page, its controller action, and its nav/footer links.
- Account lockout after **5 failed attempts**, plus a **honeypot field** on the login form to
  block bot traffic.
- Unit and integration test coverage for authentication, gating, attribution, and each admin
  function.

### Out of scope

- External identity providers / Entra ID / OIDC / JWT. **Deferred to the mobile-integration phase**
  (tracked as technical debt); cookie auth is the Release 1 mechanism. This narrows but does not
  close TD-005.
- Self-service registration, email verification, or password-reset-by-email flows.
- Multi-factor authentication.
- Fine-grained permissions beyond the two roles.
- Per-field audit history / full temporal tables (attribution is created-by/updated-by snapshot
  only, not a change-by-change ledger).

---

## 3. Story mapping

| Story | Increment 8 treatment |
|---|---|
| I8-S1: Login table + seed/build script | New credentials table (1:1 with Practitioner) keyed by **email**, with password hash, role, and lockout fields; idempotent build-and-seed script; seeded initial administrator. |
| I8-S2: Authentication service + password hashing | Verify credentials against the hash; cookie sign-in/sign-out; password hashing via ASP.NET Core `PasswordHasher`; lockout after 5 failed attempts; honeypot field to reject bot submissions. |
| I8-S3: Login popup + navigation gating | Login modal; signed-out users see only Home + Login; full menu + Logout after sign-in. |
| I8-S4: Forward authenticated identity (+role) to API | Replace the static `SeededPractitionerId` header value with the signed-in practitioner id/role; API trusts the header as today (TD-005 unchanged). |
| I8-S5: Audit attribution columns + update script | Add nullable created-by/updated-by id + name-snapshot + timestamp columns to **all CRUD tables**; migration/update script; write path populates them. |
| I8-S6: Rename practitioner preserving historical names | Rename updates the current practitioner record only; historical attribution name snapshots are unchanged. |
| I8-S7: Admin — set/reset password | Administrator sets a physiotherapist's password directly (admin controls access). |
| I8-S8: Admin — delete (soft delete) practitioner | Administrator deactivates a physiotherapist; data + history preserved. |
| I8-S9: Admin — GDPR RTBF relocation + remove Privacy page | Move RTBF into Admin; delete Privacy page + nav/footer links. |
| I8-S10: Role-based authorisation enforcement | Admin functions require the administrator role at both UI and API boundaries. |
| I8-S11: Admin — add physiotherapist | Administrator adds a new practitioner (email + password + role) which creates the Practitioner and login rows. |
| I8-S12: Admin — change admin password | An administrator can change the administrator password (belt-and-braces if physios learn it). |
| I8-S13: Test consolidation (unit + integration) | Authentication, lockout, honeypot, gating, attribution, role enforcement, and each admin function covered. |
| I8-S14: Azure deployment + DB script execution + re-test | Apply DB scripts in the deployed environment and re-verify the login + admin flows. |

---

## 4. Stories and acceptance criteria

> Assumptions used to draft these stories are listed in Section 9 and flagged inline as
> **[ASSUMPTION]**. Confirm or correct via the questions in Section 10.

### I8-S1 — Login credentials table and build/seed script

Add a credentials table to the database to back authentication.

**Decision:** A new `PractitionerLogin` table, 1:1 with `Practitioner`, holding: `PractitionerId`
(FK, also the link to the login email via `Practitioner.Email`), `PasswordHash`, `Role`
(`physiotherapist` | `administrator`), `IsActive`, `MustChangePassword`, `FailedAttemptCount`,
`LockedUntil` (nullable), `LastLoginDate` (nullable), `CreatedDate`, `UpdatedDate`. The **login
identifier is the practitioner email** (the existing unique `Practitioner.Email`), so no separate
username column is added.

Acceptance criteria:
1. A script under `Canine Physio Database/Build and Initialise/` creates the credentials table
   with a foreign key to `Practitioner`.
2. The script is safe to re-run (idempotent: guarded `DROP`/`CREATE` or `CREATE IF NOT EXISTS`
   consistent with the existing fresh-build script style).
3. The script seeds an initial administrator login and login rows for the existing seeded
   practitioners (Amelia Carter, James Holloway).
4. Seeded passwords are stored only as hashes (never plaintext in the committed script). The
   seeded administrator uses the **known dev password `Password12345!` with
   `MustChangePassword = true`** so it must
   be changed at first login.
5. The script aligns with the existing schema conventions (charset/collation, `BIGINT UNSIGNED`
   keys, `PK_`/`FK_`/`UQ_` naming).

Tests:
- Integration (real-schema, Testcontainers/MySQL when available): apply the script to a fresh
  database and assert the table, constraints, and seeded rows exist; re-running the script does
  not error.

### I8-S2 — Authentication service and password hashing

Provide credential verification and session sign-in/out.

Acceptance criteria:
1. A login attempt with a valid **email** + password for an active account succeeds.
2. An invalid email or password fails with a generic "invalid credentials" message (no user
   enumeration).
3. Passwords are verified against a salted hash using ASP.NET Core `PasswordHasher` (PBKDF2);
   no plaintext comparison.
4. Successful sign-in establishes a cookie-authenticated session containing the practitioner id,
   display name, and role claims.
5. Sign-out clears the session.
6. After **5 failed attempts** the account is temporarily locked (`LockedUntil`); attempts during
   the lock window are rejected with the generic message.
7. The login form includes a **honeypot field** (hidden from real users); any submission with the
   honeypot populated is rejected as bot traffic without attempting authentication.
8. A seeded/admin account flagged `MustChangePassword` is forced to set a new password before
   gaining access.

Tests:
- Unit: password hash verify (valid/invalid), generic-failure message, 5-attempt lockout logic,
  honeypot rejection, forced-change gate.
- Integration: POST valid credentials → authenticated session; POST invalid → unauthenticated;
  6th attempt within window → locked; honeypot populated → rejected.

### I8-S3 — Login popup and navigation gating

Gate the UI by authentication state.

Acceptance criteria:
1. When signed out, the navigation bar shows only **Home** and a **Login** item in the far right.
2. Clicking **Login** opens a popup (modal) with email and password fields (plus the hidden
   honeypot field).
3. A successful login closes the popup and reveals the remaining navigation items.
4. A failed login keeps the popup open and shows a generic error (rendered via the existing
   top-right toast mechanism where appropriate).
5. When signed in, a **Logout** affordance is available; logging out returns the user to the
   signed-out state (only Home + Login visible).
6. Deep-linking to a gated page while signed out redirects to Home/login rather than rendering
   the page.
7. Styling uses CSS classes per coding standards (no inline styles).

Tests:
- UI smoke: signed-out layout shows only Home + Login; gated route redirects when signed out;
  after sign-in the full menu renders; Admin item hidden for physiotherapist role.

### I8-S4 — Forward authenticated identity (and role) to the API

Replace the static seeded header identity with the signed-in user.

Acceptance criteria:
1. Authenticated API calls send the signed-in practitioner id in `X-Practitioner-Id` (replacing
   the static `SeededPractitionerId` value).
2. The user's role is conveyed to the API via an `X-Practitioner-Role` header for admin-endpoint
   authorisation **[OPEN: Section 10 Q9 — whether the API independently enforces the role]**.
3. Unauthenticated UI sessions make no gated API calls.
4. The API continues to reject `/api/*` requests lacking the identity header (existing 401 gate
   unchanged).

Tests:
- Integration: a signed-in session's API calls carry the correct practitioner id; data queries
  remain scoped to that practitioner.

### I8-S5 — Audit attribution columns and update script

Tag inserts and updates with who and when.

**Decision:** Add nullable columns `CreatedByPractitionerId`, `CreatedByPractitionerName`,
`UpdatedByPractitionerId`, `UpdatedByPractitionerName` to **all tables where the application
performs CRUD** (e.g. `Owner`, `Pet`, `TreatmentCase`, `Treatmentcasenote`, `Programme`,
`ProgrammeVersion`, `ProgrammeSession`, `ProgrammeSessionExercise`, `Exercise`, and any other
user-editable tables). Existing `CreatedDate`/`UpdatedDate` columns satisfy the timestamp
requirement where present; add them where missing. The `*Name` snapshot supports I8-S6.

Acceptance criteria:
1. An update (idempotent) script adds the nullable attribution columns to every in-scope CRUD
   table without dropping or altering existing data.
2. All columns are nullable so historical rows remain valid.
3. On insert, the create-by id + name snapshot + created timestamp are populated from the
   signed-in user.
4. On update, the update-by id + name snapshot + updated timestamp are populated from the
   signed-in user.
5. The full list of in-scope CRUD tables is enumerated during implementation by auditing the
   write paths in the Application/Infrastructure layers.

Tests:
- Integration: create a record while signed in → created-by id/name/timestamp populated; update
  it as a different practitioner → updated-by id/name/timestamp reflect the second user, created-by
  unchanged.

### I8-S6 — Rename practitioner preserving historical attribution names

Renaming a practitioner must not rewrite the name recorded against past changes.

Acceptance criteria:
1. An administrator can rename a physiotherapist (display/first/last name).
2. The rename updates only the current `Practitioner` record.
3. Attribution name snapshots already written against prior inserts/updates remain unchanged
   (i.e. they still show the name at the time of that change).
4. Subsequent changes by the renamed practitioner record the new name.

Tests:
- Integration: practitioner makes a change (name A recorded), is renamed to B, makes another
  change (name B recorded); assert the first record still shows A and the second shows B.

### I8-S7 — Admin: set/reset physiotherapist password

Acceptance criteria:
1. An administrator can set or reset any physiotherapist's password from the Admin page by
   entering the new password directly (the administrator controls who can access the application).
2. The new credential is stored only as a hash.
3. The affected user can sign in with the new credential.
4. **[ASSUMPTION]** The admin may optionally mark the account `MustChangePassword` so the user is
   prompted to choose their own password at next login (default behaviour to be confirmed).
5. Feedback is shown via the existing toast mechanism.

Tests:
- Integration: admin sets a password → old hash replaced; user can authenticate with the new
  credential; forced-change flag behaves as specified.
- Unit: set/reset updates hash + flags and does not expose plaintext.

### I8-S8 — Admin: delete (soft delete) physiotherapist

Acceptance criteria:
1. An administrator can remove a physiotherapist from the Admin page, behind a confirm/cancel
   prompt.
2. **Decision:** Removal is a **soft delete** (`Practitioner.IsActive = false` +
   `PractitionerLogin.IsActive = false`); the row and its historical attribution are preserved
   (cases/programmes reference the practitioner via `ON DELETE RESTRICT`).
3. A removed physiotherapist can no longer sign in.
4. Historical attribution (id + name snapshot) on existing records is preserved.
5. An administrator cannot delete their own account **[ASSUMPTION — Section 10 Q10]**.

Tests:
- Integration: soft-delete a physiotherapist → cannot authenticate; their historical attribution
  rows remain; referential integrity preserved; self-delete blocked.

### I8-S9 — Admin: GDPR RTBF relocation and Privacy page removal

Acceptance criteria:
1. The right-to-be-forgotten owner control is available on the Admin page (administrator only).
2. RTBF behaviour matches the existing implementation (owner selected by name dropdown per ACR008;
   confirm + anonymise).
3. The Privacy/policy page (`Home/Privacy` view, controller action, view model) is removed.
4. The Privacy link is removed from the navigation/footer.
5. No route or link to the removed Privacy page remains.

Tests:
- UI smoke: Privacy route no longer resolves / link absent; Admin page exposes the RTBF control
  for administrators and not for physiotherapists.
- Integration: RTBF from the Admin page anonymises the selected owner (re-uses existing RTBF
  coverage relocated to the admin surface).

### I8-S10 — Role-based authorisation enforcement

Acceptance criteria:
1. The Admin navigation item is shown only to administrators.
2. Admin pages/actions are not reachable by a physiotherapist, even by direct URL (UI returns
   forbidden/redirect).
3. **[ASSUMPTION]** Admin API endpoints validate the administrator role (Section 10 Q9); a
   physiotherapist-role request to an admin endpoint is rejected.

Tests:
- UI smoke: physiotherapist hitting an admin URL is denied; administrator is allowed.
- Integration: admin API endpoint rejects a non-administrator role.

### I8-S11 — Admin: add physiotherapist

Acceptance criteria:
1. An administrator can add a new practitioner from the Admin page by entering their details
   (name + **email**) and an initial password.
2. The action creates the `Practitioner` record and the matching `PractitionerLogin` row (role
   `physiotherapist` by default), with the password stored only as a hash.
3. The new practitioner can sign in with the supplied credentials; **[ASSUMPTION]** the account
   may be flagged `MustChangePassword` so they choose their own password at first login.
4. Adding a practitioner with an email that already exists is rejected with a clear message
   (the unique `Practitioner.Email` constraint is respected).
5. Feedback is shown via the existing toast mechanism.

Tests:
- Integration: admin adds a practitioner → Practitioner + login rows created, password hashed,
  new user can authenticate; duplicate-email add is rejected.
- Unit: add-practitioner does not store plaintext and defaults to the physiotherapist role.

### I8-S12 — Admin: change administrator password

Acceptance criteria:
1. An administrator can change the administrator account password (belt-and-braces in case
   physiotherapists learn the admin password).
2. The change requires the current admin password (or current admin session) before applying.
3. The new credential is stored only as a hash.
4. After change, the old admin password no longer authenticates and the new one does.
5. Feedback is shown via the existing toast mechanism.

Tests:
- Integration: change admin password → old password fails, new password succeeds.
- Unit: change requires current credential and stores only a hash.

### I8-S13 — Test consolidation (unit + integration)

Acceptance criteria:
1. New unit tests: password hashing/verification, 5-attempt lockout logic, honeypot rejection,
   attribution snapshot logic, role checks.
2. New integration tests: login success/failure, lockout, navigation gating, identity forwarding,
   attribution on insert/update, rename-preserves-history, set/reset password, add practitioner,
   change admin password, soft-delete practitioner, RTBF from Admin, role enforcement.
3. Existing UI smoke and API in-memory suites updated for the new auth requirement (e.g. test
   harness signs in, or a test authentication scheme is registered) and remain green.
4. Full solution builds clean (warnings-as-errors).
5. **Coding standards adherence:** all new/changed code conforms to
   [coding-standards.md](../../Standards/coding-standards.md) — nullable enabled, file-scoped
   namespaces, constructor injection only (no service locator), `Async` suffix + `CancellationToken`
   on async/DB calls, DTOs/view-models distinct from entities, FluentValidation at the boundary,
   `Result`/typed outcomes for recoverable failures (throw only for unexpected ones), structured
   logging that **never logs credentials or personal data**, secrets via User Secrets/Key Vault
   (no secrets in source or `appsettings`), antiforgery on browser POSTs, deny-by-default
   authorisation, and `dotnet format` + analyzers (StyleCop) producing zero diffs/warnings.
6. Any **deliberate deviation** from the standards is recorded as an ADR and logged in
   [TD-001 Admin Standards Deviations.md](../../Technical%20Debt/TD-001%20Admin%20Standards%20Deviations.md)
   (per the standards change policy); no silent deviations.

### I8-S14 — Azure deployment and database script execution

Acceptance criteria:
1. The build/seed and update scripts are executed against the deployed MySQL (private, via the
   established in-VNet seeding method).
2. The login + admin flows are re-verified in the deployed environment.
3. Local-then-Azure test order is followed (per testing preferences).

---

## 5. Implementation slice (suggested order)

1. **Database first:** I8-S1 (credentials table + seed) and I8-S5 schema (attribution columns +
   update script). Validate with a real-schema integration test.
2. **Authentication core:** I8-S2 (hashing + cookie sign-in/out) and I8-S4 (identity forwarding).
3. **UI gating:** I8-S3 (login modal + nav gating + logout).
4. **Attribution write path:** populate created-by/updated-by on insert/update (I8-S5 app/repo).
5. **Admin area + role enforcement:** I8-S10 first (so the area is protected), then I8-S7, I8-S8,
   I8-S6, and I8-S9 (RTBF move + Privacy removal).
5. **Admin area + role enforcement:** I8-S10 first (so the area is protected), then I8-S7
   (set/reset password), I8-S11 (add practitioner), I8-S12 (change admin password), I8-S6
   (rename), I8-S8 (soft delete), and I8-S9 (RTBF move + Privacy removal).
6. **Tests + deploy:** I8-S13 then I8-S14.

---

## 6. Test strategy

- Unit: hashing/verification, lockout, attribution snapshot, role checks (xUnit, no DB).
- API in-memory: identity forwarding, attribution on insert/update, role enforcement on admin
  endpoints, RTBF behaviour.
- UI smoke (WebApplicationFactory): navigation gating by auth state and role, login modal markup,
  Privacy route removed, admin URL protection. A test authentication scheme or sign-in helper
  will be registered so existing gated-page tests continue to exercise authenticated flows.
- Real-schema integration (Testcontainers/MySQL when Docker available): apply build + update
  scripts; verify constraints, seeds, idempotency, and that EF maps the new columns. (Per the
  Increment 4 lesson, the EF in-memory provider does not enforce MySQL constraints, so schema
  changes must be validated against the real schema.)
- Order: run against local DB first, then deploy to Azure and re-test (testing preference).

---

## 7. Security considerations

- Store only salted password hashes; never log or commit plaintext credentials.
- Generic authentication failure messages (no user enumeration).
- Account lockout after **5 failed attempts** to slow brute force.
- **Honeypot field** on the login form to reject automated bot submissions.
- Cookie hardening: HttpOnly, Secure, SameSite; reuse the existing DataProtection-to-Blob key
  ring (DEC-012) so cookies survive across UI replicas.
- Antiforgery tokens on the login and all admin POST forms.
- Role checks enforced server-side at both UI and API, not only by hiding menu items.
- This increment keeps the `X-Practitioner-Id` trust model (DEC-010 / TD-005) between UI and API.
  Hardening to signed tokens / JWT is **deferred to the mobile-integration phase** (out of scope
  for phase 1) and should be added to the technical-debt backlog at that time.

---

## 8. Handoff / dependencies

- Builds on the existing `ICurrentPractitionerAccessor` abstraction (UI `SeededPractitionerAccessor`
  / API `HeaderPractitionerAccessor`) — the UI accessor becomes session-backed.
- Reuses the existing RTBF anonymisation logic (currently in `HomeController.RightToBeForgotten`)
  relocated to the Admin surface.
- Reuses the toast notification mechanism (ACR012) for login/admin feedback and the sticky
  navigation (ERR-AT-007) for the gated menu.
- DB scripts follow the established 3-script build/seed convention and the in-VNet seeding method
  recorded in project memory.

---

## 9. Confirmed decisions (2026-06-11)

1. **Login identifier = practitioner email** (existing unique `Practitioner.Email`); no separate
   username. Adding a practitioner = creating a Practitioner + login with email + password.
2. UI authentication is **cookie-based** (ASP.NET Core cookie authentication). External
   identity / JWT is **deferred to the mobile-integration phase** (out of scope for phase 1).
3. Credentials live in a **new `PractitionerLogin` table** (1:1 with `Practitioner`).
4. Password hashing uses **ASP.NET Core `PasswordHasher`** (PBKDF2).
5. Practitioner delete is a **soft delete / deactivate**.
6. Attribution columns are added to **all tables where the application performs CRUD**
   (created-by/updated-by id + name snapshot + timestamp); the name snapshot is how rename
   preserves history.
7. **Account lockout after 5 failed attempts**, plus a **honeypot field** to block bots.
8. **Admin sets/resets passwords directly** (full admin control over access). An **initial
   administrator** is seeded with the **known dev password `Password12345!` + forced change at
   first login**.
9. A **change-admin-password** function is included as belt-and-braces.
10. The UI forwards the signed-in **role** to the API; RTBF becomes **administrator-only** once
    moved into Admin.

---

## 10. Remaining (minor) questions

These do not block drafting; sensible defaults are noted. (Implementation deferred — Increment 9
is next.)

1. **Password policy:** Any minimum length/complexity rules or expiry/rotation requirement?
   (Default: minimum length only, no expiry.)
2. **Forced change on admin-set passwords:** When an admin sets/adds a password, should the user
   be forced to change it at first login? (Default: yes for the seeded admin; optional flag for
   others.)
3. **API role enforcement:** Should the API independently enforce the administrator role on admin
   endpoints (recommended), or is UI-side enforcement sufficient for Release 1?
4. **Self-management guards:** Prevent an administrator from soft-deleting or role-changing their
   own account (to avoid locking out the last admin)? (Default: yes, blocked.)
5. **Rename fields:** Does "rename" cover first/last name only, or also the login email? (Default:
   name only; changing email is a separate concern since it is the login identifier.)
6. **Multiple admins:** Is more than one administrator allowed, and can admins manage other
   admins, or only physiotherapists? (Default: multiple admins allowed; admins manage physios
   only, not other admins.)
