# Hello Buddy Admin Decision Log

This log records material technical decisions for the Admin solution.

| Id | Date | Decision | Related ADR | Notes |
| --- | --- | --- | --- | --- |
| DEC-013 | 2026-06-08 | Increment 7 release validation uses a layered regression gate (UI smoke, API in-memory, API integration, optional Azurite lane) before cloud deploy sign-off. | ADR-0001 | Prioritises deterministic local verification before Azure retest. |
| DEC-014 | 2026-06-08 | Production deployment uses component-granular release and rollback drills (UI/API/PDF) in addition to full-stack deploy mode. | ADR-0002 | Uses direct Container App image update and revision pinning for rollback rehearsal. |
| DEC-015 | 2026-06-08 | Release operations are runbook-first with an explicit environment configuration matrix and backup/restore drill requirements. | ADR-0003 | Prevents environment drift and improves handover readiness. || DEC-016 | 2026-07-09 | Programme PDF page top/bottom margins (10 mm) implemented in CSS `@page` in the API-rendered template, not in the PDF service's C# `PdfOptions.MarginOptions`; CSS `@page` margin takes precedence over `MarginOptions` for the Chromium page box, so the C# margins are inert by design. Page-1 header kept full-bleed via a `-10mm` negative top margin. | — | PDF-S4. `PuppeteerPdfRenderer.cs` unchanged; pdf container out of scope. |