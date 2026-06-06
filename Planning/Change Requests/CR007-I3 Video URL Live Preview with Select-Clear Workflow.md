# CR007-I3 - Video URL Live Preview with Select/Clear Workflow

Date: 2026-06-05
Status: Implemented
Owner: Product / Release 1 engineering
Scope: Canine Physio Admin UI (Exercise editor create/edit view)

## Why this change

Image selection now has a richer immediate-feedback workflow. Video URL entry currently remains a plain text input with a simple open link, which does not provide equivalent guidance, validation feedback, or easy reset affordances.

The request is to provide a video URL flow that feels consistent with the improved image workflow.

Business and UX benefit:
- consistent media-edit experience across image and video;
- reduced invalid URL saves;
- clearer preview/open behavior before save.

## Requested behavior

- Add video interaction model parallel to image panels:
  - Current video panel and Selected video panel side by side;
  - left click opens the video in a new tab;
  - right click on selected video offers deselect option.
- Add a search popup to help user find a video on the internet and select it into the pending video value.

## Proposed implementation direction (for approval)

- Add Current video and Selected video panels in edit mode; show only Selected video panel in create mode.
- Keep left click on either panel to open video URL in a new tab.
- Add custom right-click context menu on Selected video panel with action: "Deselect video".
- Add Search video popup with:
  - search term input;
  - provider selectors (for example YouTube/Vimeo/general web);
  - result links opened in new tabs, then user confirms selected URL into the Selected video panel.
- Keep current server-side validation as source of truth; client-side checks remain assistive.

## Acceptance criteria

1. Entering a valid URL updates the selected video panel immediately without save.
2. User can clear pending selected video URL before save.
3. Edit mode shows both Current video and Selected video; create mode shows Selected video only.
4. Left click opens current/selected video in new tab.
5. Right click on selected video offers deselect option.
6. Search popup allows user to search and choose a URL into pending selected video.
7. Invalid URLs are clearly flagged in the UI before submit.
8. Saving persists the final value shown in the form as today.
9. Existing UI smoke tests continue to pass.

## Risks and mitigations

- Risk: treating all video providers equally may be brittle.
  - Mitigation: keep generic URL support with optional provider hints, avoid hard dependency on embeds.
- Risk: client-side validation mismatch with server rules.
  - Mitigation: align client checks to server contract and preserve server-side enforcement.
- Risk: in-app embedded internet search is constrained by external site CSP/X-Frame-Options and cannot reliably render third-party results inside the app.
  - Mitigation: search popup should generate provider searches and open results in new tabs, then capture user-selected URL back in the app.
- Risk: right-click-only deselect is less discoverable for some users.
  - Mitigation: include concise helper text near selected video panel describing right-click to deselect.

## Status

- 2026-06-05: Logged from user request.
- 2026-06-05: Implemented current/selected video panels, left-click open behavior, right-click deselect on selected video, and search helper popup that opens provider searches in new tabs and applies chosen URL back to the form. Pending deployment validation.

