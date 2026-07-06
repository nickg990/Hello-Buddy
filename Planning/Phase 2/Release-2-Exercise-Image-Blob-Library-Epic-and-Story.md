# Release 2 — Exercise Image Blob Library Picker Epic and Story

**Created:** 2026-07-06
**Status:** Draft for execution
**Scope:** Hello Buddy Canine Physiotherapy Admin (Release 2 — exercise media)
**Implementation model:** AI-led implementation (Sonnet, agent mode) + human testing. Estimates include a 20% contingency for bug fixes and rework. Azure deployment adds an approximate cloud wait/provisioning allowance.

---

## Which AI tool should implement this?

**Recommendation: Claude Sonnet (agent mode) is the right primary tool for this story.** The work is bounded, multi-file, pattern-following implementation against well-established repo conventions (minimal-API endpoints, `IFileStore` abstraction, DB-backed app settings, the existing managed `/exercise-media/` proxy). Every pattern this story needs already exists in the codebase and simply needs to be reused and extended.

- **Use Sonnet (agent mode)** for the full implementation. All conventions are established; there is no open design question requiring a stronger reasoning model.
- **Escalate to Opus only if** the blob-listing SAS/managed-identity behaviour behaves unexpectedly at runtime against real Azure Storage — but the local Azurite path should surface any issue first.
- **Not suitable:** non-agentic / single-file assistants — this spans the Application, Api, and Ui projects plus test projects and needs to read existing files to match conventions.

---

## Epic: Private-Safe Exercise Image Library Backed by Blob Storage

Replace the current Google-Drive-URL approach to exercise images with a **blob-storage-backed image library**. Images are uploaded manually (by the operator) into a configurable folder (prefix) inside the existing private storage container. The Edit Exercise page presents a **file-explorer-style picker** that lists the images in that folder and lets the practitioner click one to attach it. The selected image is stored on the exercise as a **managed `/exercise-media/` URL**, so it renders through the existing API blob proxy — no public storage, no SAS in the page, no Google Drive dependency for images.

### Why this is needed (verified constraints)

- Google Drive share/viewer links **cannot be hotlinked** as `<img src>`: `/file/d/{id}/view` returns an HTML viewer page, and the `uc?export=view&id=...` form is deprecated and now returns consent/scan HTML — both render as a broken image. Rewriting the URL form does **not** fix the access problem.
- The Azure storage account is locked down: `allow_nested_items_to_be_public = false` and both containers are `container_access_type = "private"` (see [Infrastructure/terraform/container-tier/main.tf](../../Infrastructure/terraform/container-tier/main.tf#L99)). A direct blob URL in `<img src>` returns **403**. Making storage public is **out of the question** (breaks the assessment's private-storage/SAS/least-privilege security posture).
- The application already has a **working managed image path**: any `ImageUrl` containing the marker `/exercise-media/` is detected server-side (`TryResolveManagedKey`) and streamed from the private container via managed identity through `GET /api/exercises/{id}/image`. The browser never touches blob directly — the API proxies the bytes. This is the pattern the picker must reuse.
- The app already has a **DB-backed app-settings** mechanism (`GET/PUT /api/admin/settings/{key}`, Admin → Settings page) so an operator can change configuration **without a redeploy**. The image-library folder must be one of these settings so a future folder swap needs no code change.

### Epic outcomes

- The Edit Exercise page shows a **thumbnail grid picker** of images from a configurable blob folder; clicking one attaches it to the exercise.
- The blob folder (prefix) is set via **`FileStorage.ImageLibraryFolder`** in Admin → Settings — swappable at any time with no redeploy.
- Attached images are stored as managed `/exercise-media/` URLs and render everywhere (exercise detail, exercise list, programme builder, programme preview, PDF) through the existing private-blob API proxy.
- **Google Drive is fully removed from the image path.** The video path is untouched and continues to use Google Drive.
- No public storage, no SAS embedded in pages, no secrets in the browser.

### Epic non-goals

- **Uploading** images from the Edit page. The operator uploads images to the blob folder manually (Azure Portal / Storage Explorer). The picker only **browses and selects**. (An upload button is explicitly out of scope for this story.)
- Any change to **video** media handling. The video picker, Google Drive video library, YouTube/Vimeo/Drive thumbnailing, and all video JS remain exactly as they are.
- Introducing new storage containers or changing storage access policy.
- Image resizing, cropping, format conversion, or CDN work.

---

## Delivery-wide guardrails

- **Do NOT change any video code.** The video path (`Form.VideoUrl`, `extractYouTubeId`, `extractVimeoId`, `extractGoogleDriveId`, `resolveVideoPreview`, `renderVideoPreview`, the video search popup, `VideoLibrary.GoogleDriveUrl` setting) must be byte-for-byte unaffected. Video Google-Drive thumbnailing stays.
- **Never expose storage publicly** or add public/anonymous access. All image bytes reach the browser only via the API's managed-identity blob proxy.
- **Reuse existing patterns**; do not invent parallel ones:
  - Minimal-API endpoints in `HelloBuddy.Api/Endpoints/ExerciseEndpoints.cs`.
  - The `IFileStore` abstraction ([HelloBuddy.Admin.Pdf/IFileStore.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/IFileStore.cs)) and its three implementations (`AzureBlobFileStore`, `LocalFileStore`, plus any test doubles).
  - DB app settings via `GET/PUT /api/admin/settings/{key}` and `IAdminApiClient.GetAppSettingAsync` / `SaveAppSettingAsync`.
  - The managed `/exercise-media/` marker resolution.
- **Prefix-guard all key-based blob access.** Any new endpoint that streams or lists blobs by caller-supplied key/prefix MUST hard-restrict to the `exercise-media/` prefix to prevent path traversal / reading other blobs (e.g. dataprotection keys, published PDFs).
- **Preserve existing behaviour for already-saved exercises.** Exercises whose `ImageUrl` already contains `/exercise-media/` must continue to resolve unchanged.
- Build must be clean (`dotnet build HelloBuddy.Admin.sln`, 0 warnings / 0 errors). **Do not run the test suite** — the operator runs tests manually per project convention.

---

## Story Index

| ID | Title | Increment |
|----|-------|-----------|
| R2-S8a | Revert Google Drive image-URL processing (keep video) | Increment 13 |
| R2-S8b | Deduplicate the `/exercise-media/` managed-key resolution | Increment 13 |
| R2-S8c | Blob image library: list endpoint + prefix-guarded proxy | Increment 13 |
| R2-S8d | `FileStorage.ImageLibraryFolder` setting (retire image URL) | Increment 13 |
| R2-S8e | Edit page blob image picker (replace Drive image popup) | Increment 13 |

> Implement the stories **in order** (a → e). Each builds on the prior. Do all five in one working session, then build clean and stop.

---

## Increment 13: Exercise Image Blob Library Picker

### Pre-work for Sonnet (read before writing any code)

> **You MUST read these files first and mirror their conventions. Do not guess.**
>
> - [Canine Physio Admin/src/HelloBuddy.Api/Endpoints/ExerciseEndpoints.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Api/Endpoints/ExerciseEndpoints.cs) — the `/api/exercises/{id}/image` proxy (~L57), the media upload endpoint (~L150), `TryResolveManagedKey` (~L287), the `/exercise-media/` key scheme (~L158).
> - [Canine Physio Admin/src/HelloBuddy.Application/Programmes/ProgrammeService.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Application/Programmes/ProgrammeService.cs) — `ResolveRenderableImageUrlAsync` (~L260) and its own copy of `TryResolveManagedKey` (~L305).
> - [Canine Physio Admin/src/HelloBuddy.Api/Services/ExerciseMediaGovernanceService.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Api/Services/ExerciseMediaGovernanceService.cs) — its third copy of `TryResolveManagedKey` (~L93).
> - [Canine Physio Admin/src/HelloBuddy.Admin.Pdf/IFileStore.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/IFileStore.cs) — the storage abstraction to extend.
> - [Canine Physio Admin/src/HelloBuddy.Api/Services/AzureBlobFileStore.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Api/Services/AzureBlobFileStore.cs) — blob impl; note it holds a `BlobContainerClient` and authenticates via managed identity.
> - [Canine Physio Admin/src/HelloBuddy.Api/Endpoints/AdminEndpoints.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Api/Endpoints/AdminEndpoints.cs#L135) — the app-settings GET/PUT, `IsValidSettingValue`, `GetSettingValidationMessage`.
> - [Canine Physio Admin/src/HelloBuddy.Ui/Controllers/ExercisesController.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Controllers/ExercisesController.cs) — `BuildImageLibraryUrlAsync` (~L282), `BuildEditorVmAsync`.
> - [Canine Physio Admin/src/HelloBuddy.Ui/Controllers/AdminController.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Controllers/AdminController.cs#L310) — `Settings` GET/POST.
> - [Canine Physio Admin/src/HelloBuddy.Ui/Models/SettingsPageVm.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Models/SettingsPageVm.cs) and [ExerciseEditorVm.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Models/ExerciseEditorVm.cs).
> - [Canine Physio Admin/src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml) — the whole page, especially the image-search popup markup (~L238) and the inline `<script>` IIFE (~L280 onward). **Identify precisely which functions are image vs video before editing.**
> - [Canine Physio Admin/src/HelloBuddy.Ui/Services/AdminApiClient.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Services/AdminApiClient.cs) — `GetAppSettingAsync` (~L516), `SaveAppSettingAsync` (~L525), `GetExerciseImageAsync` (~L156); and its interface `IAdminApiClient`.

---

### Story R2-S8a: Revert Google Drive image-URL processing (keep video)

**User story**
As the operator, I want the failed Google-Drive image-URL conversion removed, so that the codebase is clean before the blob picker is added and no dead Drive-image logic remains.

**Brief for Sonnet — do exactly this, no more:**

1. **Delete** `Canine Physio Admin/src/HelloBuddy.Application/Media/GoogleDriveImageHelper.cs`.
2. **Delete** `Canine Physio Admin/tests/HelloBuddy.Api.InMemoryTests/GoogleDriveImageHelperTests.cs`.
3. In [ExerciseEndpoints.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Api/Endpoints/ExerciseEndpoints.cs): remove the `using HelloBuddy.Application.Media;` import and revert the `/api/exercises/{id}/image` handler so that when `TryResolveManagedKey` fails it returns `Results.NotFound()` directly (delete the `GoogleDriveImageHelper.TryConvertToDirectUrl(...) → Results.Redirect(...)` block).
4. In [ProgrammeService.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Application/Programmes/ProgrammeService.cs): remove the `using HelloBuddy.Application.Media;` import and revert `ResolveRenderableImageUrlAsync` so that when `TryResolveManagedKey` fails it returns `originalUrl` directly (delete the Drive conversion branch).
5. In [Edit.cshtml](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Views/Exercises/Edit.cshtml): remove the `@using HelloBuddy.Application.Media` line and the `currentImageDisplayUrl` variable; restore the "Current image" panel `<a href>` and `<img src>` to use `@Model.Form.ImageUrl` (as they were before this chat).

**Acceptance criteria**
- No reference to `GoogleDriveImageHelper` remains anywhere (`grep` returns nothing).
- The image endpoint and PDF resolver behave exactly as they did before the Drive fix (non-managed URL → NotFound for the endpoint; passthrough for the PDF resolver).
- **No video code touched.**
- Build clean.

---

### Story R2-S8b: Deduplicate the `/exercise-media/` managed-key resolution

**User story**
As a developer, I want one shared implementation of the `/exercise-media/` key resolution, so that the picker code and existing consumers share a single, tested source of truth instead of a fourth copy.

**Brief for Sonnet:**

1. Create a single static helper in the **Application** project, e.g. `Canine Physio Admin/src/HelloBuddy.Application/Media/ExerciseMediaKey.cs`, with:
   ```csharp
   public static class ExerciseMediaKey
   {
       public const string Marker = "/exercise-media/";
       public const string Prefix = "exercise-media/";
       public static bool TryResolve(string? url, out string key); // mirror existing TryResolveManagedKey logic exactly
   }
   ```
   Copy the existing logic **verbatim** (absolute-URI parse, find `/exercise-media/` marker case-insensitively, take the remainder trimmed of `/`, reject empty). Behaviour must be identical to the current three copies.
2. Replace the three private `TryResolveManagedKey` copies with calls to `ExerciseMediaKey.TryResolve`:
   - [ExerciseEndpoints.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Api/Endpoints/ExerciseEndpoints.cs#L287)
   - [ProgrammeService.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Application/Programmes/ProgrammeService.cs#L305)
   - [ExerciseMediaGovernanceService.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Api/Services/ExerciseMediaGovernanceService.cs#L93)
   Delete each now-unused private method.
3. Keep the `exercise-media/` write-key scheme in the media upload handler (~L158) using `ExerciseMediaKey.Prefix` where it reads naturally, but do not change the generated key shape.

**Acceptance criteria**
- Exactly one implementation of the marker resolution exists; the three private copies are gone.
- No behavioural change to any existing consumer.
- Build clean.

---

### Story R2-S8c: Blob image library — list endpoint + prefix-guarded proxy

**User story**
As the Edit Exercise page, I need to list the images in the configured blob folder and render their thumbnails, so that the practitioner can browse and pick one.

**Brief for Sonnet:**

1. **Extend `IFileStore`** ([IFileStore.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Admin.Pdf/IFileStore.cs)) with:
   ```csharp
   /// <summary>Lists artefact keys under the given prefix (non-recursive-safe; returns full keys).</summary>
   Task<IReadOnlyList<string>> ListKeysAsync(string keyPrefix, CancellationToken ct = default);
   ```
   Implement in **all** implementations:
   - `AzureBlobFileStore`: use `_container.GetBlobsAsync(prefix: keyPrefix, cancellationToken: ct)` and collect `item.Name`.
   - `LocalFileStore`: enumerate files under the mapped directory for that prefix; return keys in the same `prefix/...` shape the blob store would.
   - Any test double / in-memory store implementing `IFileStore` — add the method returning an empty or seeded list as appropriate so the solution compiles.
2. **Add API endpoint** in `ExerciseEndpoints.cs`:
   `GET /api/exercises/media/library` →
   - Reads the folder prefix from the app setting `FileStorage.ImageLibraryFolder` (via `IAppSettingRepository`). If unset/blank, default to `exercise-media/images/`.
   - **Guard:** normalise the prefix and require it to start with `exercise-media/`. If a configured value does not, coerce to `exercise-media/` + value or reject — never allow listing outside `exercise-media/`.
   - Calls `IFileStore.ListKeysAsync(prefix)`, filters to image extensions (`.jpg`, `.jpeg`, `.png`, `.webp`), and returns a list of `{ name, url }` where `url` is the **managed** form: the configured `Storage:ExerciseMediaBaseUrl` joined to the key (same construction the upload endpoint uses via `BuildCanonicalUrl`), guaranteeing the URL contains `/exercise-media/`.
   - Return a typed contract (add a record to `HelloBuddy.Contracts`, e.g. `ExerciseMediaLibraryItem(string Name, string Url)`).
3. **Add prefix-guarded thumbnail proxy** in `ExerciseEndpoints.cs`:
   `GET /api/exercises/media/content?key={key}` →
   - **Reject any key that does not start with `exercise-media/`** (use `ExerciseMediaKey.Prefix`) with `Results.NotFound()` — this is the path-traversal guard. Also reject keys containing `..`.
   - `OpenReadAsync(key)`; if null → NotFound; else `Results.File(stream, contentType)`.
   - This endpoint renders library thumbnails that are **not yet attached** to any exercise (the existing `/api/exercises/{id}/image` only serves a saved exercise's image).
4. **Extend the UI client** `IAdminApiClient` / `AdminApiClient` with `Task<IReadOnlyList<ExerciseMediaLibraryItem>> GetExerciseImageLibraryAsync(CancellationToken ct)` calling `GET /api/exercises/media/library`. Add the same method to **every** `IAdminApiClient` test stub so the UI test project compiles (there are stubs in `UiSmokeTests.cs` and `UiAuthAdminFlowTests.cs`).

**Acceptance criteria**
- `IFileStore.ListKeysAsync` exists and is implemented in every implementation and stub.
- `GET /api/exercises/media/library` returns managed-form URLs and never lists outside `exercise-media/`.
- `GET /api/exercises/media/content` refuses any key outside `exercise-media/` (returns NotFound) and streams valid keys.
- Build clean.

---

### Story R2-S8d: `FileStorage.ImageLibraryFolder` setting (retire image URL)

**User story**
As the operator, I want to set the image-library **blob folder** in Settings, so that I can point the picker at a new folder in future without a redeploy.

**Brief for Sonnet:**

1. **New setting key:** `FileStorage.ImageLibraryFolder` (a blob prefix, e.g. `exercise-media/images/`).
2. **API validation** in [AdminEndpoints.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Api/Endpoints/AdminEndpoints.cs) — extend `IsValidSettingValue` and `GetSettingValidationMessage`:
   - Allow empty (clears → default).
   - When set: must be a relative path (not an absolute URL), contain only safe characters `[A-Za-z0-9/_-]`, not contain `..`, and (recommended) must start with `exercise-media/`. Message: `"Must be a blob folder path under exercise-media/, e.g. exercise-media/images/."`
3. **Retire `FileStorage.ImageLibraryUrl` for images:**
   - In [SettingsPageVm.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Models/SettingsPageVm.cs): replace `ImageLibraryUrl` with `ImageLibraryFolder`.
   - In [AdminController.cs](../../Canine%20Physio%20Admin/src/HelloBuddy.Ui/Controllers/AdminController.cs) `Settings` GET/POST: read/write `FileStorage.ImageLibraryFolder` instead of `FileStorage.ImageLibraryUrl`. Keep the **`VideoLibrary.GoogleDriveUrl`** read/write exactly as-is.
   - Update the Settings view (`Views/Admin/Settings.cshtml`) label/help text: image field becomes "Image library folder (blob path)" with placeholder `exercise-media/images/`. **Leave the video/Google Drive field untouched.**
   - Do **not** delete the `FileStorage.ImageLibraryUrl` key handling from `IsValidSettingValue` (harmless to leave for backward-compat), but it is no longer surfaced in the UI for images.
4. **ExercisesController:** replace `BuildImageLibraryUrlAsync` with `BuildImageLibraryFolderAsync` that returns the `FileStorage.ImageLibraryFolder` value (or default `exercise-media/images/`). Update `ExerciseEditorVm` accordingly (rename `ImageLibraryUrl` → `ImageLibraryFolder`, or repurpose — but keep the property name consistent with its new meaning).

**Acceptance criteria**
- Settings page shows an **Image library folder** field (blob path), saving to `FileStorage.ImageLibraryFolder`.
- Video Google Drive setting unchanged in behaviour and UI.
- Invalid values (absolute URLs, `..`, unsafe chars) are rejected with a clear message.
- Build clean.

---

### Story R2-S8e: Edit page blob image picker (replace Drive image popup)

**User story**
As a practitioner, I want to pick an exercise image from a grid of the images in blob storage, so that the image reliably displays without any Google Drive dependency.

**Brief for Sonnet — this touches the ONLY-image parts of Edit.cshtml. Do NOT alter video code:**

1. **Remove the image search/paste popup** (the Drive "Open library / paste URL" flow for images): the `#image-search-*` markup block (~L238–L270) and its JS handlers (`openImageSearchPopup`, `closeImageSearchPopup`, `runImageSearch`, `applyImageSelectedUrl`, and the `imageSearch*` element lookups). **Keep** the "Selected image (pending save)" preview panel and the `#Form_ImageUrl` hidden/input binding.
2. **Add a picker modal** (Bootstrap modal, consistent with the existing "Audit history" modal pattern used elsewhere): a button "Choose image from library" that opens a modal showing a **thumbnail grid**. On open, call `GET /api/exercises/media/library` (via a small fetch to a thin UI controller action that proxies to `IAdminApiClient.GetExerciseImageLibraryAsync`, OR render server-side into the page model on load — prefer **server-side render on page load** for KISS, mirroring how the Audit modal loads data with no AJAX). Each grid tile:
   - `<img src>` = the **content proxy** URL `/Exercises/MediaContent?key=...` (a thin UI controller action that streams from `IAdminApiClient` → `/api/exercises/media/content`), so private blobs render through the managed proxy.
   - On click: set `#Form_ImageUrl` value = the tile's **managed library URL** (the `url` from the library item), fire the existing `updateSelectedImagePanel()` so the "Selected image" preview updates, and close the modal.
3. **Add the thin UI controller actions** in `ExercisesController`:
   - `GET /Exercises/MediaContent?key=...` → calls `IAdminApiClient` content proxy, returns `File(bytes, contentType)`; returns `NotFound()` for non-`exercise-media/` keys (defence in depth even though the API also guards).
   - (If you render the library server-side, load it in `BuildEditorVmAsync` into `ExerciseEditorVm` as `IReadOnlyList<ExerciseMediaLibraryItem> ImageLibrary`.)
4. **Split the inline IIFE so image and video initialise independently.** Currently one IIFE guards on ALL elements at once, so a missing image element silently kills video too. Extract the image-related init into its **own** `(function(){...})()` block (image input, selected-image panel, picker modal). Leave the **video** init in its own block, unchanged in logic. This removes the coupling risk without rewriting video behaviour.
5. Ensure the "Current image" panel (saved image) continues to render via `@Model.Form.ImageUrl` (a managed `/exercise-media/` URL will resolve through the API proxy already).

**Acceptance criteria**
- The Edit page has a **"Choose image from library"** picker showing a thumbnail grid of blob images from the configured folder; clicking one attaches it and updates the preview.
- The old Drive image search/paste popup is gone.
- Saved managed images display in Current image, exercise list, builder, preview, and PDF (all via the existing proxy).
- **Video panel, video search, and video Drive thumbnailing are visibly and behaviourally unchanged.**
- Image and video JS are in separate init blocks; a missing image element cannot break video.
- Build clean.

---

## Manual operator step (outside code)

Upload exercise images into the configured blob folder (default `exercise-media/images/`) in the `published-programmes` container using Azure Portal → Storage browser, Azure Storage Explorer, or `az storage blob upload --auth-mode key` (the operator account lacks data-plane RBAC, so use the account key — consistent with `rebuild-azure-db.ps1` conventions). The picker browses this folder; it does not upload.

**Local testing note (already wired):** Azurite starts empty on every run (and after `-ResetAzurite`), so [Infrastructure/local-dev/run-local-admin-stack.ps1](../../Infrastructure/local-dev/run-local-admin-stack.ps1) now auto-seeds the sample JPGs into Azurite at `published-programmes/exercise-media/images/` via [Infrastructure/local-dev/seed-local-exercise-images.ps1](../../Infrastructure/local-dev/seed-local-exercise-images.ps1) (source: `Canine Physio App/Canine Physio App/Resources/Images`, `*.jpg` only). Skip with `-SkipImageSeed`. **Sonnet does not need to seed local images** — running the stack is sufficient. Set `FileStorage.ImageLibraryFolder = exercise-media/images/` (lowercase) so the picker lists them.

---

## Definition of done (whole increment)

- Google Drive removed from the **image** path end to end; **video** path untouched.
- Image-library folder configurable via `FileStorage.ImageLibraryFolder` in Settings — no redeploy to change folders.
- Picker lists + renders private blobs via the managed proxy; no public storage, no SAS in the page.
- All key-based blob endpoints hard-guard the `exercise-media/` prefix.
- One shared `/exercise-media/` key resolver; no duplicated copies.
- `dotnet build HelloBuddy.Admin.sln` → **0 warnings, 0 errors**. **Tests are NOT run by the AI** (operator runs them).
- Deploy via `cd Infrastructure/terraform/container-tier; .\deploy.ps1 -AppsOnly` (rebuilds UI + API images, which both reference the changed Application project).

---

## b) Estimate (AI implementation + human testing, incl. 20% contingency)

| Phase | Base | With 20% contingency |
|-------|------|----------------------|
| AI: R2-S8a revert Drive image processing | ~8 min | ~10 min |
| AI: R2-S8b dedupe `/exercise-media/` resolver | ~10 min | ~12 min |
| AI: R2-S8c `IFileStore.ListKeysAsync` + list endpoint + guarded proxy + client/stubs | ~25 min | ~30 min |
| AI: R2-S8d `ImageLibraryFolder` setting + validation + Settings UI | ~15 min | ~18 min |
| AI: R2-S8e Edit page picker + IIFE split + UI proxy action | ~25 min | ~30 min |
| Human: local validation (Azurite: upload to folder, pick, save, render, PDF) | 1.0 h | 1.2 h |
| Azure: build/push UI+API images, deploy, verify picker + render in prod | ~25 min | ~30 min |
| **Total** | **~2.9 h** | **~3.4 h** |

*Azure wait note:* `deploy.ps1 -AppsOnly` rebuilds and pushes the UI and API images via `az acr build` and updates the container apps, adding ~10–20 min of mostly-unattended cloud wait on top of hands-on time.

---

## Sequencing note

1. Implement R2-S8a → R2-S8e in order; build clean after each where practical, and once at the end.
2. Validate **locally** against Azurite: the stack auto-seeds the sample images into `exercise-media/images/` (see Manual operator step), so open Edit, pick one, save, confirm it renders in Current image / list / builder / preview / PDF.
3. Deploy to Azure (`deploy.ps1 -AppsOnly`).
4. Upload production images into the configured folder, set `FileStorage.ImageLibraryFolder` in Settings if not default, and verify end to end.
