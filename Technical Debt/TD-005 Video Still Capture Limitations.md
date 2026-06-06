# TD-005 - Video still/thumbnail capture limitations across providers and links

Raised: 2026-06-06
Scope: Canine Physio Admin UI exercise media preview
Source: CR015-I5 video search provider configuration follow-up
Owner: UI media preview workstream

---

## Decision

Video still/thumbnail rendering on the exercise edit, create, and details screens is provider- and link-shape dependent. The current behavior is intentionally left as-is for now, with the known limitations recorded here for a later review.

Deferred item:
- Provide reliable, provider-agnostic video still capture (including a true frame grab from arbitrary mp4 sources) rather than the current best-effort, per-provider thumbnail resolution.

---

## Current behavior

The preview logic resolves a still image (or a basic inline player) using `resolveVideoPreview` in:
- [Canine Physio Admin/src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml)
- [Canine Physio Admin/src/HelloBuddy.Ui/Views/Exercises/Details.cshtml](Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Exercises/Details.cshtml)

Supported still sources today:
- YouTube: derived from the video ID via `img.youtube.com`.
- Vimeo: derived from the numeric ID via `vumbnail.com`.
- Google Drive: derived from a single file ID via `drive.google.com/thumbnail?id=FILE_ID`.
- Direct media files ending in `.mp4`, `.webm`, `.ogg`, `.mov`: rendered through an inline `<video>` element (first-frame display only, not a generated still).

When no rule matches, a play-icon fallback is shown.

---

## Known limitations

1. Google Drive folder links do not produce a still.
   - A folder URL (`/drive/.../folders/...`) has no single video to thumbnail.
   - Only an individual Drive file link (`/file/d/FILE_ID/...` or `?id=FILE_ID`) yields a still.

2. No true frame capture from mp4 sources.
   - For direct media URLs the UI relies on the browser's `<video>` element to show the first frame.
   - There is no canvas-based frame grab, so there is no guaranteed poster image, no chosen timestamp, and no persisted still.

3. Provider thumbnail availability is external and best-effort.
   - YouTube, Vimeo, and Google Drive thumbnails depend on third-party endpoints remaining available and on the asset being shared publicly.
   - Private or permission-restricted items will fail to render a still even when the link is valid.

4. Limited provider coverage.
   - Only YouTube, Vimeo, Google Drive file links, and direct media files are recognized.
   - Other hosts (e.g., OneDrive, Dropbox, SharePoint, generic CDN preview pages) fall back to the play icon.

5. Link-shape sensitivity.
   - Stills depend on the exact pasted URL form.
   - Share/preview/redirect variants that do not expose an extractable ID will not resolve to a still.

6. No server-side validation or caching of stills.
   - Thumbnail resolution is purely client-side at render time.
   - There is no fallback image caching, no broken-thumbnail detection, and no server-stored preview.

---

## Impact

- Physios may see a play-icon fallback instead of a representative frame for some valid videos.
- Preview quality is inconsistent across providers and link formats.
- Reliance on external thumbnail endpoints introduces availability and privacy-sharing dependencies outside the application's control.

---

## Possible future scope (not committed)

- Add canvas-based frame capture for same-origin or CORS-permitted direct mp4 sources to generate a true still at a chosen timestamp.
- Normalize pasted Drive/`view` links to a canonical file form before resolution.
- Expand provider extractors (OneDrive, Dropbox, SharePoint, etc.).
- Detect broken thumbnail loads and fall back gracefully (e.g., to the inline player or play icon).
- Optionally persist a generated/selected still server-side for stable, permission-independent previews.

---

## Review checkpoint

Target review date: 2026-06-30

At checkpoint:
- Decide whether reliable cross-provider still capture is required for release.
- Prioritize canvas frame capture vs. expanded provider coverage vs. server-side persisted stills.
- Retire TD-005 only once an agreed, consistent still-capture approach is implemented and verified.
