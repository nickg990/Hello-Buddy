# CR-005 - Live Image Preview on Exercise Edit Screen

Date: 2026-06-04
Status: In Progress
Owner: Product / Release 1 engineering
Scope: Canine Physio Admin UI (Exercise Library editor)

## Why this change

After CR-002 delivered exercise image upload, clinicians editing an exercise had no visual confirmation of the file they had just chosen until the form was saved and the page refreshed. This caused confusion about whether the correct file had been selected and made it easy to save the wrong image.

The change introduces an in-form live preview so the selected file is shown immediately, side by side with the existing stored image where one is present.

Business and UX benefit:
- reduces save-and-refresh feedback loop;
- prevents accidentally saving the wrong image;
- gives a clear visual placeholder when no image is set, rather than an empty area.

## Scope

In scope:
- Exercise edit screen (`/Exercises/{id}/Edit`) shows two fixed-size (240x240) panels side by side with a small gap, wrapping to a new line on narrow screens:
  - Current image (from storage via the existing image proxy), or a placeholder with a sitting-dog silhouette and "No image" caption when none is set;
  - Selected image (pending save), populated client-side via `FileReader` the moment a file is chosen, with the file name displayed beneath the panel.
- Both the current image and the selected image are clickable: clicking the image itself opens the full-size version in a new tab. No separate "View image" text link.
- Multiple file selections are supported: each new selection replaces the preview, no commit required.
- Exercise create screen (`/Exercises/Create`, served by the same view in create mode) shows only the Selected panel with the placeholder until a file is picked. No current panel is rendered in create mode.
- Shared placeholder partial (`_NoImagePlaceholder.cshtml`) with an inline SVG sitting-dog silhouette and caption text, reusable on other screens.

Out of scope:
- No save-time confirmation popup. The user can iterate on the file picker freely and the final selection is what gets uploaded on save.
- No changes to the API upload endpoint or storage layout.
- No changes to the Details screen image rendering (already uses the image proxy from CR-002 follow-up).
- No image cropping, rotation, or client-side resizing.

## Acceptance criteria

1. On the Edit screen for an exercise that already has an image:
   - the Current panel shows the existing image via the image proxy;
   - the Selected panel shows the placeholder with sitting-dog silhouette and "No image selected" caption.
2. When the user picks a file in Upload image:
   - the Selected panel updates immediately with the chosen image and the file name;
   - no network request is made until Save.
3. Picking a different file again replaces the Selected preview each time, without page reload.
4. On the Edit screen for an exercise with no image:
   - the Current panel shows the placeholder with "No image" caption;
   - the Selected panel behaves as above.
5. On the Create screen:
   - only the Selected panel is shown;
   - placeholder appears until a file is picked, then updates per selection.
6. Saving works exactly as before: file fields, text fields, and Remove image checkbox are persisted unchanged.
7. Existing UI smoke tests continue to pass.

## Implementation notes

- View: `src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml` (serves both edit and create).
- Partial: `src/HelloBuddy.Ui/Views/Shared/_NoImagePlaceholder.cshtml` (inline SVG, accepts caption via `@model string`).
- Client logic: small inline script in the Edit view that wires `change` on the file input to a `FileReader.readAsDataURL` and swaps the preview / placeholder visibility classes.
- No new dependencies, no new API calls, no new endpoints.

## Risks and mitigations

- Risk: Very large selected files could slow the browser when base64-encoded for preview.
  - Mitigation: file size is already constrained by the API's existing validation; preview uses native `FileReader` which is fast for the supported sizes.
- Risk: Inline SVG silhouette may render inconsistently across browsers.
  - Mitigation: uses standard SVG path and `currentColor` fill to inherit Bootstrap text-muted styling.

## Status

- 2026-06-04: View and placeholder partial implemented; pending build, smoke-test run, and deployment.
- 2026-06-04: Refinement applied — panels constrained to a fixed 240x240 size and laid out side by side with a small gap, image itself is the clickable target (separate "View image" link removed). Pending rebuild and redeploy.
