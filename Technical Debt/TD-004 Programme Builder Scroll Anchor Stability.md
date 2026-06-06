# TD-004 - Programme builder scroll anchor stability after updates

Raised: 2026-06-06
Scope: Canine Physio Admin UI (Programme Builder)
Source: Manual UX test feedback after Increment 3 and 4 fixes
Owner: UI quality and interaction hardening workstream

---

## Decision

The current Programme Builder post-update flow causes forced viewport jumps (top, bottom, or session-start anchor) after add, remove, and save actions. This creates visual repaint and operator disorientation. A targeted remediation is required so the page remains anchored to the user's current scroll position after updates.

Deferred item:
- Replace forced postback scroll jumps with stable scroll-position restoration.

---

## Problem statement

Current behavior:
- Builder actions use POST-Redirect-GET with TempData scroll directives.
- UI script then forces scroll to top, bottom, or a session card anchor.

Observed impact:
- The page repaints and jumps away from the user's working context.
- Repeated updates in lower-page sessions are distracting and reduce operator efficiency.

Primary objective:
- Preserve user scroll position across update actions unless an explicit navigation change is intended.

---

## Options considered

### Option 1 - Preserve exact scroll offset across PRG (recommended)

Approach:
1. Capture `window.scrollY` before form submit.
2. Send it with the POST payload (hidden input/query value).
3. On redirected GET, restore the exact position after DOM load.
4. De-prioritise top/bottom forcing logic except where explicitly required.

Pros:
- Low implementation risk.
- Minimal architecture change.
- Immediate UX improvement while retaining existing server-side flow and TempData messaging.

Cons:
- Still performs full-page POST-Redirect-GET repaint.
- Position restoration may need small tolerance handling if content height changes materially.

Effort:
- Low to medium.

Risk:
- Low.

---

### Option 2 - Session anchor plus relative viewport offset

Approach:
1. Keep current session anchor targeting.
2. Capture relative offset from the target session block before POST.
3. Restore to target session plus relative offset on GET.

Pros:
- Better than hard top/bottom jumps.
- Maintains context within session-focused edits.

Cons:
- More complex than Option 1.
- Not guaranteed to preserve exact viewport position in all content-change scenarios.

Effort:
- Medium.

Risk:
- Low to medium.

---

### Option 3 - Move session mutations to partial/AJAX updates

Approach:
1. Replace full-page POST-Redirect-GET for add/remove/update interactions with targeted asynchronous updates.
2. Re-render only changed session regions.
3. Keep page scroll unchanged by avoiding full reload.

Pros:
- Best UX (no repaint jump).
- Better long-term interaction model for frequent inline edits.

Cons:
- Highest implementation complexity.
- Requires broader JavaScript state, error, and accessibility handling.
- Larger regression surface.

Effort:
- High.

Risk:
- Medium.

---

## Recommendation

Recommended path:
- Implement Option 1 in the next UI hardening slice.
- Record Option 3 as a future enhancement once current increment stability goals are met.

Rationale:
- Option 1 delivers immediate operator value with low risk and preserves the current architecture.
- It avoids introducing broad behavioral regressions while addressing the core UX pain now.

---

## Required completion scope

1. Remove forced top/bottom post-update jumps in builder workflows unless explicitly required by user intent.
2. Capture and restore scroll position across add/remove/save setup/save session edits actions.
3. Preserve existing success/error TempData messages and current form behavior.
4. Add UI test coverage (or browser automation follow-up debt item) for scroll restoration behavior.

---

## Acceptance criteria

1. After add/remove/update actions, viewport remains anchored to prior position (within tolerance).
2. No default jump to top or bottom on routine builder updates.
3. Session edit flows remain functional with existing validation and status messaging.
4. Build and existing test lanes remain green after implementation.

---

## Validation status at deferral point

Completed:
- Manual test confirms functional correctness of updates but identifies disruptive scroll-jump behavior.

Deferred only:
- Scroll-anchor stability implementation and associated coverage.

---

## Review checkpoint

Target review date: 2026-06-30

At checkpoint:
- Confirm Option 1 implementation sequencing in the next planned UI maintenance increment.
- Re-evaluate whether Option 3 should be promoted from future enhancement to active work item.
- Retire TD-004 once acceptance criteria are met and verified.
