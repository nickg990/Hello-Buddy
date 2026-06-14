# Acceptance Testing Change Request Log

Last Updated: 2026-06-14 (ACR025-ACR027 added)

## Purpose

Single place to track Acceptance Testing Change Request (ACR) status, progress, and next actions.
ACRs capture changes raised during acceptance testing of the Canine Physio Admin application.

## Status legend

- Proposed: documented but not approved.
- Approved: agreed and queued for implementation.
- In Progress: actively being implemented.
- Implemented: code complete, pending final sign-off/release.
- Deferred: intentionally paused for future decision.
- Closed: completed and signed off.

## ACR register

| ACR | Title | Status | Owner | Date Opened | Date Updated | Progress Summary | Next Action |
|---|---|---|---|---|---|---|---|
| ACR001 | Treatment Case Edit: Calendar Date Pickers for Start and End Dates | Proposed | Product / Release 1 planning | 2026-06-09 | 2026-06-09 | Raised in acceptance testing: replace freeform start/end date inputs on the treatment case edit screen with calendar date pickers while keeping storage and validation unchanged. | Approve and schedule implementation. |
| ACR002 | Add Note Must Persist and Display in Case Notes Panel | Proposed | Product / Release 1 planning | 2026-06-09 | 2026-06-09 | Raised in acceptance testing: Add note must push the note to the case notes panel on click and persist it so case notes are always visible on reopen. | Approve and schedule implementation. |
| ACR003 | Builder Session Purpose Summary (Replace Duplicated Notes) | Proposed | Product / Release 1 planning | 2026-06-09 | 2026-06-09 | Raised in acceptance testing: stop duplicating notes into the Builder session area and add a mandatory per-session purpose summary for the exercises. | Approve and schedule implementation. |
| ACR004 | Remove Cases from Main Menu (Owner to Pet to Case Navigation) | Proposed | Product / Release 1 planning | 2026-06-09 | 2026-06-09 | Raised in acceptance testing: remove the top-level Cases menu item so cases are reached via Owner -> Pet -> Case or Pet -> Case. | Approve and schedule implementation. |
| ACR005 | Builder Live Preview Moved to Popup Window with Boxed Media Layout | Proposed | Product / Release 1 planning | 2026-06-09 | 2026-06-09 | Raised in acceptance testing: remove the compressed inline preview and show it in an in-app popup with boxed left-thumbnail layout, clickable image opening video in a new tab, and a no-video-link popup. | Approve and schedule implementation. |
| ACR006 | Sticky (Non-Scrolling) Main Menu | Proposed | Product / Release 1 planning | 2026-06-09 | 2026-06-09 | Raised in acceptance testing: make the main menu sticky so it does not scroll out of view on long pages. | Approve and schedule implementation. |
| ACR007 | Replace "HelloBuddy.UI" Text with Logo | Proposed | Product / Release 1 planning | 2026-06-09 | 2026-06-09 | Raised in acceptance testing: replace the "HelloBuddy.UI" text with a brand logo; logo asset to be supplied separately by the client. | Obtain logo asset, then implement. |
| ACR008 | RTBF (Privacy) Owner Selection by Name Dropdown, Not User ID | Proposed | Product / Release 1 planning | 2026-06-09 | 2026-06-09 | Raised in acceptance testing: replace user ID selection in the RTBF area with an owner name dropdown mapping to the correct owner. | Approve and schedule implementation. |
| ACR009 | Update Copyright Text to "(c) 2026 Net Intentions Ltd" | Proposed | Product / Release 1 planning | 2026-06-09 | 2026-06-09 | Raised in acceptance testing: update footer copyright to show the copyright symbol followed by "2026 Net Intentions Ltd". | Approve and schedule implementation. |
| ACR010 | Homepage Branding and Layout (Sub-header, Logo Disc, Remove Learn-About) | Proposed | Product / Release 1 planning | 2026-06-10 | 2026-06-10 | Raised in acceptance testing: remove the default "learn about" text/link, add an H2 sub-header "Canine Physiotherapy Administration" under the Hello Buddy H1, add buffer spacing and the logo disc, and ensure the homepage is responsive and standards-compliant. | Approve and schedule implementation. |
| ACR011 | Case Notes Edit and Delete Controls (Per-Note Edit Popup and Confirmed Delete) | Implemented | Product / Release 1 planning | 2026-06-10 | 2026-06-10 | Per-note pencil/bin icons added: delete confirms then hard-deletes and refreshes; edit opens a popup that updates the DB, closes, and refreshes. Backend PUT/DELETE note endpoints, repo methods, UI client, controller actions, view modal, and UI+API tests complete; suites green. | Final acceptance sign-off. |
| ACR012 | Top-Right Fading Toast Notifications for All Pages | Implemented | Product / Release 1 planning | 2026-06-10 | 2026-06-10 | Replaced inline alert banners with Bootstrap top-right fading toasts across all message-producing pages (Home, Programmes, Cases, Owners, Case detail) and the async builder; layout/JS/CSS updated; builds clean and suites green. (Original "tiny mce" request resolved to Bootstrap Toasts.) | Final acceptance sign-off. |
| ACR013 | Pages Must Not Scroll After a Submit | Implemented | Product / Release 1 planning | 2026-06-10 | 2026-06-10 | Removed post-submit scroll-to-message behaviour; submits use Post-Redirect-Get so pages render from the top, with feedback shown via toasts (ACR012). Builder's deliberate async scroll-position retention left intact. Builds clean and suites green. | Final acceptance sign-off. |
| ACR014 | Realtime Loading Spinner on Preview PDF Page | Implemented | Product / Release 1 planning | 2026-06-10 | 2026-06-10 | Added a Bootstrap spinner overlay over the Preview PDF iframe with accessible status text that hides on load (with a readyState safety net). View/CSS updated and UI test added; builds clean and suite green. | Final acceptance sign-off. |
| ACR015 | Remove Email PDF (Send PDF) Functionality Completely | Proposed | Product / Release 1 planning | 2026-06-12 | 2026-06-12 | Raised in acceptance testing: the implemented Email/Send PDF feature is over-complicating Release 1 and must be removed across all layers (UI/API/Application/Infrastructure/Contracts/DB table/packages/config/tests); Increment 9 plan withdrawn. | Approve and schedule implementation. |
| ACR016 | Remove "#<number>" Record Identifiers from Page Sub-headers | Proposed | Product / Release 1 planning | 2026-06-12 | 2026-06-12 | Raised in acceptance testing: remove distracting raw record ids (e.g. "Case #12", "Owner #5") from page headers while keeping ids in routes. | Approve and schedule implementation. |
| ACR017 | Loading Spinner on PDF Download | Proposed | Product / Release 1 planning | 2026-06-12 | 2026-06-12 | Raised in acceptance testing: show a spinner when Download PDF is clicked that clears when the download completes (cookie-token completion signal + timeout fallback). | Approve and schedule implementation. |
| ACR018 | Name the Downloaded PDF after the Programme Name + Date Range | Proposed | Product / Release 1 planning | 2026-06-12 | 2026-06-12 | Raised in acceptance testing: download filename = `<ProgrammeName> <StartDate> to <EndDate>.pdf` (sanitised); canonical blob name unchanged. | Approve and schedule implementation. |
| ACR019 | Remove "Create Duplicate" Button from the Builder Page | Proposed | Product / Release 1 planning | 2026-06-12 | 2026-06-12 | Raised in acceptance testing: remove the Builder-page Create duplicate button + modal; keep the capability on PDF History. | Approve and schedule implementation. |
| ACR020 | Move Publish Button and Functionality from Builder to PDF Viewer | Proposed | Product / Release 1 planning | 2026-06-12 | 2026-06-12 | Raised in acceptance testing: relocate the Publish button + confirmation from the Builder to the PDF Viewer page; same action/endpoint; hide when already published. | Approve and schedule implementation. |
| ACR021 | Terminology Renames (PDF Viewer / PDF History / PDF Builder / New Programme) | Proposed | Product / Release 1 planning | 2026-06-12 | 2026-06-12 | Raised in acceptance testing: rename Preview→PDF Viewer, History→PDF History, Builder→PDF Builder, and "Create draft programme"→"New programme" (display labels only; routes unchanged). | Approve and schedule implementation. |
| ACR022 | Consistent Button Styling Across the Application | Proposed | Product / Release 1 planning | 2026-06-12 | 2026-06-12 | Raised in acceptance testing: standardise button variants by role — primary action `btn-primary`, cancel/secondary `btn-outline-secondary`, destructive `btn-outline-danger`. | Approve and schedule implementation. |
| ACR023 | Disable the PDF Builder Button when a Programme is Published / Locked | Proposed | Product / Release 1 planning | 2026-06-12 | 2026-06-12 | Raised in acceptance testing: disable/hide the PDF Builder entry point for published/locked programmes (reuse the existing locked-for-edit signal); editing via "New draft from published" on PDF History. | Approve and schedule implementation. |
| ACR024 | Publish Spinner in the PDF Viewer Popup and Redirect to the Case Page | Proposed | Product / Release 1 planning | 2026-06-12 | 2026-06-12 | Raised in acceptance testing: show a spinner in the PDF Viewer publish popup while publishing runs (guard double-submit), then redirect to the Case page on success (Builder is locked post-publish); validation failures return to the PDF Viewer with the error toast. | Approve and schedule implementation. |
| ACR025 | Brand Colours on Primary and Secondary Buttons | Proposed | Product / Release 1 planning | 2026-06-14 | 2026-06-14 | Raised in acceptance testing (iteration 2): re-skin `btn-primary` and `btn-outline-secondary` to the Canine Physio App brand palette (primary `#4A7A96`, hover `#28404F`, secondary outline `#4A7A96`) via `site.css` overrides; colour-only change, role mapping (ACR022) unchanged. | Approve and schedule implementation. |
| ACR026 | PDF Visual Restyle to Owner Programme Layout Guide | Proposed | Product / Release 1 planning | 2026-06-14 | 2026-06-14 | Raised in acceptance testing (iteration 2): restyle the programme PDF (`Programme.cshtml`) to look like `Designs/PDF/owner_programme_pdf.svg` (header/summary bands, intro panel, session bands, boxed left-hand media) while keeping current content and content layout; keep click-image-to-open-video; drop exercise numbering. | Approve and schedule implementation. |
| ACR027 | Standard Page Title (H2) with Breadcrumb Subtitle (H4) on Main Pages | Proposed | Product / Release 1 planning | 2026-06-14 | 2026-06-14 | Raised in acceptance testing (iteration 2): each main page (Owner, Pets, Pet Details, Treatment Case, Treatment Case Details edit-only, Exercise Programme Builder, PDF Viewer, PDF History) shows a fixed title as H2 and a breadcrumb-trail subtitle as H4 beneath it (items joined by `" . "`, no `#<number>` ids, every ancestor item is a clickable link to its page). CQ-1 resolved: Exercise Programme = Builder page (page title `Exercise Programme Builder`; case-page button stays `Builder`). CQ-2 resolved: subtitle = breadcrumbs. | Approve and schedule implementation. |

## Notes

- Update this file whenever an ACR status changes.
- Keep each ACR document as the detailed source of scope and acceptance criteria.
- ACR numbering is sequential in raise order; the source acceptance-test item numbers were 1-8
  and 10 (item 9 was not used in the original list).
- ACR011-ACR013 were raised together as a multi-part change request (note edit/delete, fading
  toast notifications, and no-scroll-after-submit) and implemented in the same change set. The
  related sticky-navigation defect is tracked in the Acceptance Testing Error Log (ERR-AT-007).
- ACR015-ACR023 were raised together (2026-06-12) as a multi-part acceptance-test change request
  covering removal of the email PDF feature, PDF page terminology/label changes, download
  filename + spinner, button relocation/styling, and locked-programme navigation. ACR015 also
  withdraws the Increment 9 (Client Email Delivery) plan.
- ACR025-ACR027 were raised together (2026-06-14) from acceptance testing iteration 2 (local):
  brand button colours, PDF visual restyle to the owner-programme layout guide, and the standard
  H2 title / H4 subtitle on main pages. The same iteration-2 round also raised two defects tracked
  in the Acceptance Testing Error Log: ERR-AT-009 (stable explicit back navigation, superseding the
  history-based back link from ERR-AT-004) and ERR-AT-010 (application-wide no-page-jump sweep,
  generalising ACR013).
