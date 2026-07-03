# CR015-I5 - Exercise Video Search Provider Base URL Configuration

Date: 2026-06-06
Status: Proposed
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Exercise edit/create video search helper)

## Why this change

The current exercise editor search helper assumes public internet video discovery (for example YouTube/Vimeo style search). Practitioners are likely to move to a controlled, public-facing cloud storage location for approved exercise videos.

To support that operating model, the search helper should allow a configurable base URL so practitioners can be directed to their own approved media library/search destination instead of a hard-wired internet endpoint.

## Scope

In scope:
- Add configuration for a video-search base URL used by the exercise editor search helper action.
- Update the exercise editor helper button/link to open the configured destination.
- Preserve existing flow where user can copy/paste resulting video URL back into the exercise form.
- Validate URL format and provide safe fallback behavior when not configured.

Out of scope:
- Building a full in-app media browser for cloud storage.
- Provider-specific API integrations (Google Drive, SharePoint Graph, S3 index APIs, etc.).
- Authentication/token broker implementation for private media stores.

## Acceptance criteria

1. A configurable base URL exists for the video search helper.
2. Clicking the video search helper opens the configured base URL in a new tab.
3. If no base URL is configured, behavior falls back to current default helper target.
4. Invalid URL configuration is handled safely (helper disabled or fallback target used; no runtime exception).
5. Existing exercise create/edit save flows remain unchanged.

## Implementation options

### Option A - Global app setting (fastest)

Summary:
- Add `MediaSearch:VideoBaseUrl` to application configuration.
- Render helper link from this value in exercise create/edit views.

Pros:
- Fastest to deliver.
- Minimal schema or API changes.
- Easy to manage per environment.

Cons:
- Single value for all practitioners.
- Requires deploy/config change for updates.

Best fit:
- Release 1 where one organisation/provider target is sufficient.

### Option B - Practitioner-level configurable setting (flexible)

Summary:
- Add practitioner setting storage (DB + API + UI settings page).
- Exercise helper resolves current practitioner setting.

Pros:
- Supports multi-practitioner custom provider/library URLs.
- No redeploy needed for updates.

Cons:
- Larger scope (data model, settings UX, permissions, tests).
- More validation and migration overhead.

Best fit:
- Post-Release 1 or when multi-tenant variability is mandatory.

### Option C - Case/programme-level URL override (most granular)

Summary:
- Allow URL override on treatment case or programme context.

Pros:
- Maximum flexibility per workflow.

Cons:
- Highest complexity and risk.
- More UI and validation burden.

Best fit:
- Not recommended for current increment unless explicit requirement emerges.

## Recommended approach

Recommended now: Option A (global app setting), with design shaped so Option B can be added later without breaking UI behavior.

Suggested design notes:
- Introduce a small UI-facing options class for media search settings.
- Resolve helper URL in one place (controller/view model), not inline in Razor.
- Keep default fallback URL in config, not hard-coded in views.

## Risks and mitigations

Risks:
- Misconfigured URL could break helper action.
- Redirecting to external domains can increase phishing/misdirection risk.

Mitigations:
- Enforce absolute `http/https` URL validation at startup and/or render-time guard.
- Optionally constrain to an allow-list of hosts in higher-security environments.
- Open helper destination with `target="_blank"` and `rel="noopener noreferrer"`.

## Test impacts

Minimum tests to add/adjust:
- UI test: exercise edit page renders helper link with configured base URL.
- UI test: fallback helper target appears when config is absent.
- Optional controller/unit test if helper URL is resolved in controller/view model logic.

## Delivery estimate

- Option A: Small (roughly 0.5 to 1 day including tests).
- Option B: Medium/Large (2 to 4 days depending on settings UX scope).
- Option C: Large (defer unless required).
