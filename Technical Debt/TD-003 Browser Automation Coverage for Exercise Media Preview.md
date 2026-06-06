# TD-003 - Browser automation coverage for exercise media preview interactions

Raised: 2026-06-05
Scope: Canine Physio Admin UI test coverage
Source: CR005-I3 live image preview follow-up
Owner: UI quality and test automation workstream

---

## Decision

Server-side UI smoke tests are now in place for the exercise edit/create preview markup, but the browser-driven interaction path remains deferred.

Deferred item:
- Add end-to-end browser automation coverage for file-picker preview behavior and click-through opening of the selected image preview.

---

## Deferred item 1 - Browser interaction coverage for pending image preview

Test intent:
- Prove that the exercise media preview works in a real browser, not just in server-rendered HTML.

Current state:
- [Canine Physio Admin/tests/HelloBuddy.Ui.Tests/UiSmokeTests.cs](Canine%20Physio%20Admin/tests/HelloBuddy.Ui.Tests/UiSmokeTests.cs) verifies the rendered structure for edit/create pages:
  - current image panel on edit;
  - selected image panel on edit/create;
  - placeholder presence;
  - absence of the selected filename element.
- No automated test currently verifies browser-only behaviors such as selecting a file, replacing the placeholder with the preview image, or opening the selected image in a larger tab/window before save.

Why deferred:
- This behavior depends on browser APIs (`input type=file`, `URL.createObjectURL`, new-tab navigation), which cannot be meaningfully proven by the existing server-side smoke test harness.
- Proper coverage requires browser automation infrastructure such as Playwright or equivalent, which is not yet part of the current UI test lane.

Required completion scope:
- Introduce browser automation for the HelloBuddy UI, or extend an existing browser test lane if one is added elsewhere.
- Cover both edit and create exercise flows.
- Verify the following in a real browser:
  - selecting an image replaces the placeholder immediately;
  - selecting a second image replaces the first preview;
  - the selected image can be clicked to open larger before save;
  - the current image remains clickable on the edit screen;
  - no selected filename text is rendered below the preview.

Acceptance criteria:
- A browser automation test runs in CI or a documented local lane.
- The test attaches a real image file to the upload input.
- The test asserts the preview image becomes visible without saving.
- The test asserts click-through behavior opens the larger selected image successfully.
- Existing build and smoke test lanes remain green.

---

## Validation status at deferral point

Completed:
- UI smoke tests for edit/create preview markup are implemented and passing.
- The selected preview uses object URLs for pre-save click-through behavior.

Deferred only:
- Browser automation coverage for file selection and click-to-open preview interactions.

---

## Review checkpoint

Target review date: 2026-06-30

At checkpoint:
- Decide whether to add Playwright (or equivalent) to the solution.
- Convert this debt item into a concrete test automation task.
- Retire TD-003 only once browser-level preview interaction tests are running and stable.
