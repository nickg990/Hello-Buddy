# Increment 3 Error Log

Last Updated: 2026-06-05

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

- Status: Resolved (CR-005 refinement). Pending commit + UI redeploy to production.

## Triage notes

- I3-ERR-001 may be caused by private blob container access with direct public URL usage instead of SAS/private serving path.
- I3-ERR-002 and I3-ERR-003 may share the same root cause if UI is suppressing URL/preview when image retrieval fails.
