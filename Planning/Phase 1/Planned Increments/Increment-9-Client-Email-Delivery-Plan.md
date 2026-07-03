# Increment 9 — Client Email Delivery (Send Programme PDF) Plan

> **CANCELLED / WITHDRAWN (2026-06-12).** The email/Send PDF capability was implemented and then
> withdrawn as over-complicating Release 1. Removal is tracked by
> [ACR015](../Acceptance%20Testing%20Change%20Requests/ACR015-Remove%20Email%20PDF%20Functionality.md).
> This plan is retained for historical context only and should not be implemented.

**Created:** 2026-06-11
**Status:** Cancelled / Withdrawn (2026-06-12) — superseded by ACR015 (remove email functionality)
**Scope:** Hello Buddy Canine Physiotherapy Admin — Release 1, Increment 9
**Parent plan:** [Release-1-Prototype-Epic-and-Increment-Stories.md](../Release-1-Prototype-Epic-and-Increment-Stories.md)
**Depends on:** Increment 8 plan in [Increment-8-Login-and-System-Administration-Plan.md](Increment-8-Login-and-System-Administration-Plan.md) (logged-in physio identity/email)

---

## 1. Increment goal

Add the ability to **email a published programme PDF to the pet owner** directly from the Admin
application. Today a programme can be previewed, saved (published) as a version-controlled PDF in
Azure Blob storage, and re-viewed from version history — but there is no way to deliver it to the
owner. Increment 9 introduces a reusable **email send page** reachable from both the **Preview**
screen and the **version history (view saved PDF)** screen, sends the PDF as an attachment over
SMTP, records each send for audit, and reports success/failure using the existing toast mechanism.

This increment delivers three capabilities:

1. **Send from Preview** — a "Send PDF" action on the Preview screen opens the email send page
   with the owner's email pre-filled (editable), a message body, the sending physio's email
   (editable), Send, and Back.
2. **Send from version history** — the "view saved PDF" / version history screen exposes the same
   "Send PDF" capability, reusing the same email send page for a chosen version.
3. **Delivery + audit** — sending attaches the relevant published PDF (reusing the latest
   published version, publishing first only if none exists), records the send in a new audit
   table, and surfaces a success toast (green) or an error popup (red) for invalid email / send
   failure, with no email sent on validation failure.

---

## 2. Scope summary

### In scope

- A reusable **email send page** (full page, not a modal) with: recipient email (pre-filled from
  the owner, editable), **sender email** (pre-filled from the logged-in physio, **editable** — to
  cover a physio picking up an absent colleague's work), a message body, **Send**, and **Back**.
- A **"Send PDF" action on the Preview screen** and on the **version history ("view saved PDF")
  screen**, both routing to the same email send page.
- **SMTP-based delivery** via **MailKit** with a configurable host (provider-agnostic; Mailpit /
  Papercut for local dev, real SMTP relay in Azure). Configuration-driven host/port/credentials.
- Attaching the programme PDF: **reuse the latest already-published version**; **publish first
  only if no published version exists** (so a send is never blocked by a missing version).
- **Email format validation** server-side (and client-side as a convenience): an invalid recipient
  (or sender) address produces a **red error popup** and **no email is sent**.
- **Success feedback** via the existing toast mechanism (green), consistent with other messages,
  including a note that the PDF has been **saved to blob storage**.
- A new **email-send audit table** recording: programme id, programme version id, recipient email,
  sender email, sending practitioner id + name snapshot, message body (or a flag/length), status
  (sent/failed), failure reason (nullable), and timestamp — plus a build/update script.
- **Back** returns the user to the originating page (Preview or version history) without sending.
- Surfacing the **owner email** to the UI (it is not currently on `ProgrammeVm`).
- Unit, API in-memory, and UI smoke test coverage for validation, send success/failure, blob
  reuse-or-publish behaviour, and audit recording.

### Out of scope

- Owner-facing portal, owner reply handling, or inbound email.
- Rich-text / HTML template editor, attachments other than the single programme PDF, or multiple
  recipients / CC / BCC.
- Scheduled or queued/retried delivery, bounce processing, and delivery-receipt tracking.
- SMS or push notifications.
- Real authentication itself — the logged-in physio email is provided by Increment 8. The send
  functionality is **only available when signed in**; there is **no interim/anonymous fallback**.
  Increment 9 therefore depends on Increment 8 landing first.
- Mobile JSON publication / owner progress sync (separate future workflows).

---

## 3. Story mapping

| Story | Increment 9 treatment |
|---|---|
| I9-S1: Email-send audit table + build/update script | New `ProgrammeEmailSend` table (FK to programme + version), idempotent build/update script consistent with existing DB script conventions. |
| I9-S2: SMTP email service (MailKit) | Provider-agnostic SMTP client behind an `IEmailSender` abstraction; configurable host/port/credentials/from-display-name; PDF attachment support. |
| I9-S3: Reuse-or-publish PDF resolution | Resolve the PDF to attach: use the latest published version; if none published, publish first, then attach. |
| I9-S4: Email send page (shared) | A shared page/view: editable recipient + editable sender + message body + Send + Back; owner email pre-filled; sender pre-filled from logged-in physio. |
| I9-S5: "Send PDF" entry point on Preview | Add a "Send PDF" action to the Preview screen routing to the send page for that programme. |
| I9-S6: "Send PDF" entry point on version history | Add a "Send PDF" action to the version history ("view saved PDF") screen routing to the send page for the chosen version. |
| I9-S7: Validation + feedback | Server-side email-format validation; invalid address → red error popup, no send; success → green toast incl. "saved to blob storage" note; failure → red error popup. |
| I9-S8: Audit recording | On send attempt, write a `ProgrammeEmailSend` row (sent or failed + reason), attributed to the sending physio. |
| I9-S9: Surface owner email to UI | Add owner email to the programme view model (or a dedicated lookup) so the send page can pre-fill it. |
| I9-S10: Tests (unit + API in-memory + UI smoke) | Cover validation, reuse-or-publish, send success/failure, audit writes, and both entry points. |
| I9-S11: Azure deployment + DB script execution + re-test | Apply DB script and SMTP config in the deployed environment and re-verify end-to-end. |

---

## 4. Stories and acceptance criteria

> Assumptions are listed in Section 9 and flagged inline as **[ASSUMPTION]**. Confirm or correct
> via the questions in Section 10.

### I9-S1 — Email-send audit table and build/update script

Add a table to record every programme email send attempt.

**Decision:** A new `ProgrammeEmailSend` table holding: `ProgrammeEmailSendId` (PK), `ProgrammeId`
(FK), `ProgrammeVersionId` (FK to the attached version), `RecipientEmail`, `SenderEmail`,
`SentByPractitionerId` (+ `SentByPractitionerName` snapshot, consistent with Increment 8
attribution), `MessageBody` (or length/flag — see Section 10 Q1), `Status` (`sent` | `failed`),
`FailureReason` (nullable), `CreatedDate`.

Acceptance criteria:
1. A script under `Canine Physio Database/Build and Initialise/` creates the table with foreign
   keys to the programme and programme-version tables (`ON DELETE RESTRICT`, consistent with
   existing FK conventions).
2. The script is safe to re-run (idempotent, consistent with the existing build script style).
3. The script aligns with existing schema conventions (charset/collation, `BIGINT UNSIGNED` keys,
   `PK_`/`FK_`/`UQ_` naming).
4. An update/migration variant exists so the table can be added to already-provisioned databases.

Tests:
- Integration (real-schema, Testcontainers/MySQL when Docker available): apply the script to a
  fresh database; assert the table, constraints, and FKs exist; re-running does not error.

### I9-S2 — SMTP email service (MailKit)

Provide a configurable, provider-agnostic email sender.

**Decision:** An `IEmailSender` abstraction in Application, implemented in Infrastructure using
**MailKit/MimeKit**. Configuration (`Email:` section): host, port, use-SSL/STARTTLS, username,
password (from secret store / app settings), default from-address, from-display-name. Supports a
single PDF attachment, a plain-text (and optional simple HTML) body, recipient, and a configurable
sender address.

Acceptance criteria:
1. An `IEmailSender.SendAsync(...)` accepts recipient, sender, subject, body, and a PDF attachment
   (bytes + filename + content type) and returns a success/failure result.
2. SMTP settings are fully configuration-driven; no host/credentials are hard-coded.
3. For local development the service targets a local SMTP sink (e.g. Mailpit/Papercut) via config;
   no real mail is sent in dev.
4. Failures (connection/auth/transport) are caught and returned as a failure result with a reason
   (not an unhandled exception), so the caller can show an error popup and record the audit row.
5. Credentials and message contents are never logged in plaintext.

Tests:
- Unit: the sender builds a well-formed message (recipient, sender, subject, attachment present)
  given a fake/stub SMTP transport; a transport error yields a failure result, not an exception.

### I9-S3 — Reuse-or-publish PDF resolution

Resolve which PDF to attach when sending.

Acceptance criteria:
1. When a **published version exists**, the **latest published version's PDF** is attached and **no
   new version is created**.
2. When **no published version exists**, the programme is **published first** (creating the
   version-controlled blob), and that newly published PDF is attached.
3. From the **version history screen**, the **specific selected version's** PDF is attached
   (that version already exists in blob storage); history sends **never create a new version**.
   "Send PDF" is available on every published version row so a lost/deleted email can be resent.
4. The success message states that the PDF has been **saved to blob storage** (always true after a
   send, whether reused or freshly published). The owner-facing attachment is named
   `<PetName> Exercise Programme <StartDate> <EndDate>` while the canonical blob/version name is
   unchanged.
5. Publish validation rules (existing) still apply: if a programme cannot be published (incomplete
   draft) and no published version exists, the send is rejected with a clear error and **no email
   is sent**.

Tests:
- API in-memory: send with an existing published version reuses it (no new version row); send with
  no published version publishes first then attaches; incomplete draft with no published version
  is rejected.

### I9-S4 — Email send page (shared)

A single reusable page used by both entry points.

Acceptance criteria:
1. A full page (not a modal) shows: **recipient email** pre-filled with the owner's email and
   **editable**; **sender email** pre-filled with the logged-in physio's email and **editable**
   (free-text, any valid email address — no restriction to known practitioners); a **message
   body** that starts **empty** (no default template); a **Send** button; and a **Back** link.
2. **Back** returns to the originating page (Preview or version history) without sending.
3. The page identifies the programme (and, from history, the version) being sent so the correct
   PDF is attached.
4. Antiforgery token is present on the send form; the send is a POST.
5. The page reuses the existing layout, sticky navigation (ERR-AT-007), and toast container
   (ACR012).

Tests:
- UI smoke: the send page renders both pre-filled-but-editable email fields, the message body,
  Send, and Back; Back targets the originating screen; the form posts with an antiforgery token.

### I9-S5 — "Send PDF" entry point on Preview

Acceptance criteria:
1. The **Preview** screen gains a **"Send PDF"** action alongside the existing "Save PDF".
2. Clicking it opens the shared email send page for that programme, with owner + sender emails
   pre-filled.
3. The action is available whenever Preview is available.

Tests:
- UI smoke: Preview renders a "Send PDF" control linking to the send page for the programme id.

### I9-S6 — "Send PDF" entry point on version history (view saved PDF)

Acceptance criteria:
1. The **version history ("view saved PDF")** screen gains a **"Send PDF"** action **per published
   version** (every published version row, not only the latest), so a prior version can be resent
   if the client accidentally deletes or loses an email.
2. Clicking it opens the same shared email send page, scoped to the chosen version, with owner +
   sender emails pre-filled.

Tests:
- UI smoke: the history page renders a "Send PDF" control for a published version linking to the
  send page scoped to that version.

### I9-S7 — Validation and feedback

Acceptance criteria:
1. The **recipient** and **sender** addresses are validated for correct email format
   **server-side** (client-side validation is a convenience only).
2. An **invalid email format** produces a **red error popup** (the existing error-toast style) and
   **no email is sent**.
3. A **successful send** produces a **green success toast** consistent with other messages,
   including a note that the PDF was **saved to blob storage**.
4. A **send/transport failure** produces a **red error popup** and is recorded as a failed send
   (I9-S8); no success message is shown.
5. After send (success or handled failure), the user is returned via PRG to the originating screen
   with the toast shown (no resubmission on refresh).

Tests:
- UI smoke / API in-memory: invalid email → error, no send, no audit "sent" row; valid email →
  success toast text includes the blob-save note.
- Unit: email-format validation accepts valid and rejects malformed addresses.

### I9-S8 — Audit recording

Acceptance criteria:
1. Every send **attempt** writes a `ProgrammeEmailSend` row with status `sent` or `failed`.
2. The row records programme id, attached version id, recipient, sender, sending practitioner id +
   name snapshot, and timestamp; failures record a `FailureReason`.
3. The **full message body is recorded** in the audit row (resolved 2026-06-11).
4. Attribution uses the logged-in physio (Increment 8 dependency; see Section 9).

Tests:
- API in-memory: a successful send writes a `sent` row referencing the attached version; a
  transport failure writes a `failed` row with a reason; a validation failure writes **no** row.

### I9-S9 — Surface owner email to the UI

Acceptance criteria:
1. The owner's email is available to the send page for pre-filling (added to `ProgrammeVm` or via a
   dedicated lookup), since `ProgrammeVm` currently exposes `OwnerName` but **not** the email.
2. Anonymised owners (RTBF, `@redacted.local`) are handled gracefully — **[ASSUMPTION]** sending to
   an anonymised/redacted owner is blocked with a clear message (no send).

Tests:
- API in-memory / unit: the owner email is returned for a normal owner; an anonymised owner does
  not expose a usable address and send is blocked.

### I9-S10 — Tests (unit + API in-memory + UI smoke)

Acceptance criteria:
1. Unit: email-format validation, message construction, transport-failure handling.
2. API in-memory: reuse-or-publish resolution, audit writes (sent/failed/none), anonymised-owner
   block.
3. UI smoke: both entry points, send page markup (editable recipient + sender + body + Send +
   Back), Back navigation, success vs error feedback.
4. Existing UI smoke and API in-memory suites remain green; full solution builds clean
   (warnings-as-errors).
5. **Coding standards adherence:** all new/changed code conforms to
   [coding-standards.md](../../Standards/coding-standards.md) — the `IEmailSender` abstraction lives
   in Application with its MailKit implementation in Infrastructure (dependency direction
   respected), nullable enabled, file-scoped namespaces, constructor injection only, `Async`
   suffix + `CancellationToken` on async calls, DTOs/view-models distinct from entities,
   email-format validation via FluentValidation at the boundary, recoverable send failures
   returned as a typed `Result`/outcome (throw only for unexpected errors), structured logging that
   **never logs SMTP credentials, recipient addresses, or message bodies**, SMTP host/credentials
   via `IOptions<T>` from config + Key Vault (no secrets in source or `appsettings`), antiforgery on
   the send POST, and `dotnet format` + analyzers (StyleCop) producing zero diffs/warnings.
6. Any **deliberate deviation** from the standards is recorded as an ADR and logged in
   [TD-001 Admin Standards Deviations.md](../../Technical%20Debt/TD-001%20Admin%20Standards%20Deviations.md)
   (per the standards change policy); no silent deviations.

### I9-S11 — Azure deployment and database script execution

Acceptance criteria:
1. The audit table build/update script is executed against the deployed MySQL (private, via the
   established in-VNet seeding method).
2. SMTP configuration is provided to the deployed UI/API (host/credentials via the secure config
   store, not committed).
3. The send flow is re-verified in the deployed environment from both entry points.
4. Local-then-Azure test order is followed (per testing preferences).

---

## 5. Implementation slice (suggested order)

1. **Database first:** I9-S1 (audit table + build/update script). Validate with a real-schema
   integration test.
2. **Email core:** I9-S2 (`IEmailSender` + MailKit) and I9-S3 (reuse-or-publish PDF resolution).
3. **Owner email surfacing:** I9-S9 (expose owner email to the view model).
4. **Send page + endpoints:** I9-S4 (shared send page), with the API endpoint that validates,
   resolves the PDF, sends, and writes the audit row (I9-S7 + I9-S8).
5. **Entry points:** I9-S5 (Preview) and I9-S6 (version history).
6. **Tests + deploy:** I9-S10 then I9-S11.

---

## 6. Test strategy

- Unit: email-format validation, MailKit message construction, transport-failure handling
  (xUnit, no DB, fake SMTP transport).
- API in-memory: reuse-or-publish resolution, validation rejection (no send/no audit), audit row
  writes for sent and failed, anonymised-owner block.
- UI smoke (WebApplicationFactory): both "Send PDF" entry points, send page markup (editable
  recipient + sender + message body + Send + Back), Back navigation target, and toast feedback
  (success green / error red). The existing `StubAdminApiClient` gains stub send/owner-email
  members.
- Real-schema integration (Testcontainers/MySQL when Docker available): apply the audit table
  script; verify constraints, FKs, and idempotency. (Per the Increment 4 lesson, the EF in-memory
  provider does not enforce MySQL constraints, so schema changes are validated against the real
  schema.)
- Order: run against local DB first (with a local SMTP sink), then deploy to Azure and re-test
  (testing preference).

---

## 7. Security and privacy considerations

- SMTP host and credentials are **configuration-driven** and sourced from the secure config store;
  never hard-coded or committed.
- Email message contents and credentials are **never logged in plaintext**.
- Server-side **email-format validation**; the client-side check is a convenience only.
- **Antiforgery tokens** on the send form; the send is a POST behind PRG.
- The PDF attachment is the owner's own programme; **anonymised/redacted owners** (RTBF) must not
  receive email — sending is blocked for `@redacted.local` addresses.
- The audit table stores personal data (recipient email, message); it is covered by the same GDPR
  retention/erasure handling as other owner data (consider RTBF cascade — see Section 10 Q4).
- The editable **sender** address is constrained to a valid email **format** only; it is **not**
  restricted to known practitioner emails — a physio may set any valid address (accepted trade-off
  for covering colleague-absence cover).
- The send page (and its endpoint) are **only reachable when signed in** (Increment 8); there is
  no anonymous send path.
- Inherits the `X-Practitioner-Id` trust model (DEC-010 / TD-005); unchanged by this increment.

---

## 8. Handoff / dependencies

- **Increment 8 (auth):** the logged-in physio's email/identity pre-fills the sender field and
  attributes the audit row. Increment 9 must be implemented **after** Increment 8 — the send
  functionality is **gated behind sign-in** and is unavailable when not logged in (no interim
  fallback).
- Reuses the existing **publish-to-blob** pipeline (`PublishProgrammeAsync`) and **version
  history** (`GetProgrammeVersionHistoryAsync`) — no new blob mechanism is introduced.
- Reuses the **toast** mechanism (ACR012) for success/error feedback and the **sticky navigation**
  (ERR-AT-007).
- New dependency: **MailKit/MimeKit** NuGet package (managed via `Directory.Packages.props`).
- DB script follows the established build/seed convention and the in-VNet seeding method recorded
  in project memory.

---

## 9. Confirmed decisions (2026-06-11)

1. **Deliverable:** this increment is a **plan/epic + stories document only** for now; no code is
   implemented yet.
2. **Delivery mechanism:** **SMTP via MailKit** with a **configurable host** (provider-agnostic;
   local SMTP sink such as Mailpit/Papercut for dev, real relay in Azure).
3. **Blob/version behaviour on send:** **reuse the latest already-published version**; **publish
   first only if no published version exists**. From version history, the selected version is
   attached as-is.
4. **Audit:** add a **`ProgrammeEmailSend` audit table** recording recipient, sender, sending
   practitioner, attached version, status, failure reason, and timestamp.
5. **Sender identity:** planned **against Increment 8 auth** and treated as a **dependency** — the
   send page pre-fills the sender from the signed-in physio, and the field remains **editable**
   (any valid email, **no restriction**) so a physio can send on behalf of an absent colleague.
   The functionality is **only available when signed in**; there is **no interim/anonymous
   fallback**.
6. **Send page is a full page** (not a modal), reachable from both **Preview** and **version
   history**, with editable recipient + editable sender + an **empty** message body (**no default
   template**) + Send + Back, and PRG + toast feedback (green success incl. "saved to blob
   storage"; red error popup for invalid email or send failure, with no email sent on validation
   failure).
7. **Attachment filename:** the owner-facing PDF attachment is named
   **`<PetName> Exercise Programme <StartDate> <EndDate>`** (the canonical blob/version name is
   unchanged).

---

## 10. Remaining (minor) questions

These do not block drafting.

**Resolved (2026-06-11):**

- **RTBF interaction:** when an owner is anonymised (right to be forgotten), their **recipient
  email and message body are scrubbed/redacted** in historical `ProgrammeEmailSend` rows, folded
  into the existing RTBF anonymisation cascade, so no real personal data lingers in the audit
  table.
- **History entry-point granularity:** "Send PDF" appears on **every published version row** in
  version history (not just the latest), so any prior version can be **resent** if the client
  accidentally deletes or loses an email.
- No default message body (starts empty); the full message body is stored in the audit table; the
  sender is free-text with email-format validation only (no restriction to known practitioners);
  the send functionality is gated behind sign-in with no interim fallback; the attachment filename
  is `<PetName> Exercise Programme <StartDate> <EndDate>`.
