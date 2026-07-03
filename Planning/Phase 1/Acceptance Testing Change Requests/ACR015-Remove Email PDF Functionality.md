# ACR015 - Remove Email PDF (Send PDF) Functionality Completely

Date: 2026-06-12
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Programmes — Email/Send PDF feature, all layers)

## Why this change

The "Email PDF" / "Send PDF" capability (delivering a programme PDF to the owner by email) was
implemented across every layer of the solution. It is over-complicating the product for Release 1
and is being removed completely. The Increment 9 (Client Email Delivery) plan is also withdrawn.

> **Implementer note:** this is a *full removal*, not a feature flag. After removal the solution
> must build clean (warnings-as-errors) and **all** tests must be green with no dangling
> references. Work outward layer by layer (UI → API → Application → Infrastructure → Contracts →
> DB → packages → tests) and finish by grepping the whole solution for the identifiers listed
> below to confirm zero remaining references.

## Scope

In scope (remove all of the following):
- UI: the `SendPdf` GET/POST controller actions, the `SendPdf.cshtml` view, the "Email PDF"
  buttons/links on the Preview and History views, and the UI view models/form requests.
- API: the email-send endpoints on the Programmes API.
- Application: the email sender abstraction, the send-programme-email service methods, and their
  result/outcome types.
- Infrastructure: the SMTP email sender implementation and its DI registration.
- Contracts: the email-send DTOs.
- Database: the `ProgrammeEmailSend` table — add a guarded drop script and remove the EF entity,
  `DbSet`, and any RTBF cascade handling for it.
- Packages: the MailKit/MimeKit NuGet references.
- Config: the SMTP/Email configuration section in all `appsettings*.json`.
- Tests: every test exercising the feature, plus stub members on the test doubles.
- Planning: mark the Increment 9 plan as **Cancelled** (withdrawn).

Out of scope:
- The existing **Download PDF** (Save PDF) and **Publish** behaviour, the version history
  ("View PDF") and the publish-to-blob pipeline — these stay. Only email delivery is removed.

## Acceptance criteria

1. There is no "Email PDF" / "Send PDF" affordance anywhere in the UI (Preview and History views
   no longer render an Email PDF button/link).
2. Navigating to `/Programmes/{id}/SendPdf` no longer resolves to a working page (the route and
   actions are gone).
3. No email/SMTP code remains in any layer; the MailKit/MimeKit packages and the Email/SMTP
   config section are removed.
4. The `ProgrammeEmailSend` table is removed via a guarded (idempotent) drop script consistent
   with the existing DB script conventions, and the EF model/`DbSet` no longer references it.
5. The right-to-be-forgotten (RTBF) anonymisation flow still works correctly with the email-send
   table and its cascade handling removed (no broken FK/cascade logic, no compile errors).
6. The Increment 9 (Client Email Delivery) plan is marked **Cancelled / Withdrawn**.
7. Full solution builds clean (warnings-as-errors); all unit, UI smoke, and API in-memory tests
   are green with the feature's tests removed.
8. Implementation conforms to coding standards (no orphaned interfaces, DI registrations, usings,
   or DTOs left behind).

## Implementation guidance (files and members to remove/edit)

Delete these files:
- `src/HelloBuddy.Application/Communication/IEmailSender.cs`
- `src/HelloBuddy.Application/Communication/EmailSendRequest.cs`
- `src/HelloBuddy.Application/Communication/EmailSendResult.cs`
- `src/HelloBuddy.Application/Communication/EmailSendOutcome.cs`
- `src/HelloBuddy.Application/Communication/EmailSendAttachment.cs`
- `src/HelloBuddy.Application/Programmes/SendProgrammeEmailResult.cs`
- `src/HelloBuddy.Application/Programmes/SendProgrammeEmailOutcome.cs`
- `src/HelloBuddy.Contracts/ProgrammeEmailSendContextVm.cs`
- `src/HelloBuddy.Contracts/SendProgrammePdfRequest.cs` (the send request DTO, if present)
- `src/HelloBuddy.Ui/Models/ProgrammeEmailSendPageVm.cs`
- `src/HelloBuddy.Ui/Models/ProgrammeEmailSendFormRequest.cs`
- `src/HelloBuddy.Ui/Views/Programmes/SendPdf.cshtml`
- The SMTP email sender implementation in `HelloBuddy.Infrastructure` (e.g. `SmtpEmailSender.cs`).
- The scaffolded `ProgrammeEmailSend` EF entity class.

Edit these files (remove the listed members/usages):
- `src/HelloBuddy.Ui/Controllers/ProgrammesController.cs` — remove the `SendPdf` `[HttpGet]` and
  `[HttpPost]` actions. Remove `BuildReturnUrl` **only if** it is used solely by `SendPdf`
  (confirm by reference search; keep it if shared).
- `src/HelloBuddy.Ui/Services/IAdminApiClient.cs` + `AdminApiClient.cs` — remove
  `GetProgrammeEmailSendContextAsync`, the send-pdf client method, and the
  `SendProgrammePdfClientOutcome` / `SendProgrammePdfClientResult` types.
- `src/HelloBuddy.Ui/Views/Programmes/Preview.cshtml` — remove the `sendPdfUrl` variable and the
  `<a ... >Email PDF</a>` button.
- `src/HelloBuddy.Ui/Views/Programmes/History.cshtml` — remove the per-version "Email PDF" link
  and the now-unused `historyReturnUrl` variable (if unused after removal).
- `src/HelloBuddy.Api/Endpoints/ProgrammeEndpoints.cs` — remove the email-send-context and
  send-pdf endpoints.
- `src/HelloBuddy.Application/Programmes/IProgrammeService.cs` + `ProgrammeService.cs` — remove
  `GetEmailSendContextAsync`, `SendPdfAsync`, the `IEmailSender _emailSender` field and its
  constructor parameter/assignment.
- `src/HelloBuddy.Application/Programmes/IProgrammeRepository.cs` +
  `src/HelloBuddy.Infrastructure/Records/RecordRepositories.cs` — remove
  `GetEmailSendContextAsync`, `RecordProgrammeEmailSendAsync`, the
  `services.AddScoped<IEmailSender, SmtpEmailSender>()` registration, and the
  `ProgrammeEmailSend` cascade-delete block inside the RTBF anonymisation routine (verify the
  RTBF flow still compiles and behaves correctly afterwards).
- EF `DbContext` — remove the `Programmeemailsends` `DbSet` and any model configuration.
- `Directory.Packages.props` and the relevant `.csproj` — remove MailKit/MimeKit entries.
- `appsettings.json` / `appsettings.*.json` — remove the Email/SMTP configuration section and any
  `IOptions` binding for it.

Database:
- Add a guarded `DROP TABLE IF EXISTS` script for `ProgrammeEmailSend` under
  `Canine Physio Database/Build and Initialise/` consistent with existing script style, and
  remove the table from the fresh-build/seed scripts.

Tests to remove (and clean up shared doubles):
- `tests/HelloBuddy.Ui.Tests/ProgrammesControllerTests.cs` — `SendPdf_Post_*` tests.
- `tests/HelloBuddy.Ui.Tests/UiSmokeTests.cs` — `HistoryPage_RendersSendPdfActionForPublishedVersion`,
  `SendPdfPage_RendersEditableRecipientSenderMessageAndBack`, and the stub client's
  `GetProgrammeEmailSendContextAsync` / send-pdf members.
- `tests/HelloBuddy.Ui.Tests/UiAuthAdminFlowTests.cs` — the stub client's
  `GetProgrammeEmailSendContextAsync` member.
- `tests/HelloBuddy.Api.InMemoryTests/ApiInMemoryTests.cs` — `ProgrammeEmailSendContext_*`,
  `ProgrammeSendPdf_*`, the `StubEmailSender` class, the `EmailSender` factory property, and the
  `IEmailSender` register/replace lines.

Final sweep — grep the whole `Canine Physio Admin/` solution (including tests) for each of these
identifiers and confirm **zero** matches remain:
`SendPdf`, `EmailSend`, `IEmailSender`, `EmailSender`, `SmtpEmailSender`, `MailKit`, `MimeKit`,
`SmtpClient`, `ProgrammeEmailSend`, `Programmeemailsends`, `SendProgrammeEmail`,
`GetEmailSendContext`, `GetProgrammeEmailSendContext`, `SendProgrammePdf`, and the `Email`/`Smtp`
config keys.

## Risks and mitigations

Risks:
- The RTBF anonymisation cascade currently deletes `ProgrammeEmailSend` rows; removing it
  carelessly could break the anonymisation flow.
- Leftover DI registrations, usings, or DTO references can cause warnings-as-errors build breaks.

Mitigations:
- Remove the cascade block as a deliberate step and re-run the RTBF tests.
- Use the identifier grep sweep above before building; build with warnings-as-errors and run all
  test suites.

## Verification

- `/Programmes/{id}/SendPdf` no longer resolves; Preview and History show no Email PDF control.
- Identifier grep sweep returns no matches across the solution.
- DB drop script applies idempotently against a fresh database.
- Full solution builds clean (warnings-as-errors); all suites green.
- Increment 9 plan shows status Cancelled/Withdrawn.
