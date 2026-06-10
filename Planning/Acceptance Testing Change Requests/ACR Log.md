# Acceptance Testing Change Request Log

Last Updated: 2026-06-10

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

## Notes

- Update this file whenever an ACR status changes.
- Keep each ACR document as the detailed source of scope and acceptance criteria.
- ACR numbering is sequential in raise order; the source acceptance-test item numbers were 1-8
  and 10 (item 9 was not used in the original list).
- ACR011-ACR013 were raised together as a multi-part change request (note edit/delete, fading
  toast notifications, and no-scroll-after-submit) and implemented in the same change set. The
  related sticky-navigation defect is tracked in the Acceptance Testing Error Log (ERR-AT-007).
