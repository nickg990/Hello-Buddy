# Increment 3 Error Log

Last Updated: 2026-06-06

## Status legend

- Open: issue is reproducible and unresolved.
- In Progress: investigation or fix is underway.
- Resolved: fix verified.
- Closed: signed off after validation.

## Error register

| Error ID | Increment | Title | Status | Environment | Date Logged |
| --- | --- | --- | --- | --- | --- |
| I3-ERR-001 | Increment 3 | Unable to preview image in Exercise Library due to blob public access restrictions | Open | Azure | 2026-06-04 |
| I3-ERR-002 | Increment 3 | Baited back stretch image URL not displayed on exercise screen | Open | Azure | 2026-06-04 |
| I3-ERR-003 | Increment 3 | Current image preview does not display uploaded image | Open | Azure | 2026-06-04 |
| I3-ERR-004 | Increment 3 | Edit/Add exercise image preview overflows its fixed-size box | Resolved | Azure | 2026-06-05 |
| I3-ERR-005 | Increment 3 | Selected image preview shows file name beneath the preview box | Resolved | Azure | 2026-06-05 |
| I3-ERR-006 | Increment 3 | Selected image preview does not reliably open a larger image when clicked | Resolved | Azure | 2026-06-05 |
| I3-ERR-007 | Increment 3 | Case note add action does not persist note from case screen | Open | Local / Azure (to confirm) | 2026-06-06 |
| I3-ERR-008 | Increment 3 | Video placeholder remains clickable when no video URL is set and opens not found target | Open | Local / Azure (to confirm) | 2026-06-06 |

## Details

### I3-ERR-001 - Unable to preview image in Exercise Library due to blob public access restrictions

- Error description:

```xml
<Error>
  <Code>PublicAccessNotPermitted</Code>
  <Message>Public access is not permitted on this storage account. RequestId:183a92e6-601e-0004-761f-f40920000000 Time:2026-06-04T12:40:24.0361428Z</Message>
</Error>
```

- Reproduction steps:
1. Go to Exercise Library.
2. Select baited back stretch.
3. Click Edit.
4. Choose file.
5. Upload file.
6. Save exercise.
7. Click image URL.

- Observed result:
Image URL returns Blob Storage access error and preview fails.

- Expected result:
Uploaded image should be previewable from the Exercise Library flow.

### I3-ERR-002 - Baited back stretch image URL not displayed on exercise screen

- Reproduction notes:
Image is confirmed as saved in Blob Storage, but the image URL is not shown in the baited back stretch screen.

- Observed result:
Image URL field/display is empty or missing on screen for baited back stretch.

- Expected result:
Saved image URL should be visible on the exercise detail/edit screen.

### I3-ERR-003 - Current image preview does not display uploaded image

- Reproduction notes:
After upload and save, the image preview panel does not render the uploaded image.

- Observed result:
Preview area does not show image.

- Expected result:
Preview area should render the currently saved uploaded image.

### I3-ERR-004 - Edit/Add exercise image preview overflows its fixed-size box

- Reproduction steps:
1. Go to Exercise Library.
2. Open an exercise and click Edit (or click Add exercise).
3. Observe the Current image / Selected image preview panels.

- Observed result:
The image rendered on top of / spilling outside the fixed 240x240 preview box instead of being contained within it. Affected both the Edit screen (current + selected panels) and the Add exercise screen (selected panel).

- Expected result:
Image should be fully contained inside the preview box, centered, with aspect ratio preserved, regardless of the source image dimensions.

- Root cause:
The image used `max-height: 100%`, which resolves against the inline `<a>` wrapper element. That wrapper had no defined height, so the percentage had no constraint to resolve against and the image rendered at its natural size, overflowing the box.

- Fix:
Images are sized with an explicit `width: 224px; height: 224px; object-fit: contain` (the 240px box minus `p-2` padding). `object-fit: contain` scales any source image to fit the box while preserving aspect ratio, downscaling large images and upscaling small ones, with letterboxing as needed. Panels also got `overflow-hidden` as a safety clip. Applied to the current-image panel (Edit) and the selected-image panel (shared by Edit and Add exercise).

- Status: Resolved (CR005-I3 refinement). Pending commit + UI redeploy to production.

### I3-ERR-005 - Selected image preview shows file name beneath the preview box

- Reproduction steps:
1. Go to Exercise Library.
2. Open an exercise and click Edit, or click Add exercise.
3. Choose an image file in Upload image.

- Observed result:
The selected image file name (for example `slow_lead.jpg`) is displayed beneath the selected image preview box.

- Expected result:
Only the image preview should be shown. The file name should not be rendered beneath the preview box.

- Root cause:
The view includes a dedicated `selected-image-filename` element and the client-side preview script explicitly sets `filename.textContent = file.name` and removes the `d-none` class after a file is selected.

- Candidate fix:
Remove the filename element from the view and stop populating / toggling it in the preview script.

- Fix:
Removed the `selected-image-filename` element from the shared edit/create view and removed the filename population/toggle logic from the preview script, so only the preview image is shown.

- Status: Resolved. Pending commit + UI redeploy to production.

### I3-ERR-006 - Selected image preview does not reliably open a larger image when clicked

- Reproduction steps:
1. Go to Exercise Library.
2. Open an exercise and click Edit, or click Add exercise.
3. Choose an image file in Upload image.
4. Click the selected image preview.

- Observed result:
Clicking the selected image preview does not reliably open a larger version in the same way as the current saved image.

- Expected result:
Clicking the selected image preview should open the selected image at a larger size in a new tab, matching the interaction used for the current saved image.

- Root cause:
The selected-image link currently uses a client-generated `data:` URL produced by `FileReader.readAsDataURL`. While this can work, it is a weaker interaction surface than the current-image flow and is more brittle for navigation/open-in-new-tab behavior than using a browser object URL derived directly from the selected file.

- Candidate fix:
Use `URL.createObjectURL(file)` for the selected image instead of a `data:` URL, assign that object URL to both the preview image `src` and the clickable anchor `href`, and revoke the previous object URL when a different file is selected or the page is unloaded.

- Fix:
Switched the pending-image preview to use `URL.createObjectURL(file)` for both the preview `src` and the clickable anchor `href`, with object URL cleanup on reselection and page unload.

- Status: Resolved. Pending commit + UI redeploy to production.

### I3-ERR-007 - Case note add action does not persist note from case screen

- Reproduction steps:
1. Go to Cases.
2. Open a case detail screen.
3. Enter note text in the Add case note input.
4. Click Add note.

- Observed result:
No new note appears in the case notes list and the screen continues to show "No case notes recorded yet."

- Expected result:
The note should be saved and immediately displayed in the case notes list for the selected case.

- Initial hypothesis:
Possible issue in case-note POST wiring (form action, antiforgery handling, model binding key, or API call/response handling) that prevents persistence or prevents refresh of the notes list after save.

- Status: Open. Investigation and fix pending.

### I3-ERR-008 - Video placeholder remains clickable when no video URL is set and opens not found target

- Reproduction steps:
1. Go to Exercise Library.
2. Open Add exercise or Edit exercise.
3. Leave Video URL empty (or clear the selected/current video URL).
4. Click the video placeholder tile.

- Observed result:
The placeholder behaves as a clickable link and navigation opens a not found target/error page.

- Expected result:
When no valid video URL exists, the placeholder should be non-clickable and should not attempt navigation.

- Initial hypothesis:
Anchor/link state is not gated by URL presence in all placeholder states, so click targets remain active even when the source URL is null/empty/invalid.

- Status: Open. Investigation and fix pending.

## Triage notes

- I3-ERR-001 may be caused by private blob container access with direct public URL usage instead of SAS/private serving path.
- I3-ERR-002 and I3-ERR-003 may share the same root cause if UI is suppressing URL/preview when image retrieval fails.

