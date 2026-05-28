# Day 4b — SAS download, welcome-text polish, MySQL firewall cleanup

**Controller chat handoff. Open a fresh chat for this. Time budget: ~2 hours.** Smaller, focused follow-up to Day 4a. All work lands in one UI image rebuild plus one infrastructure tidy-up.

## Why we're doing this

Three things have to converge before we record the demo video:

1. **PDF must reach a clinic owner without us emailing it manually.** The published-programmes container is private (correct), so the UI needs a "Download PDF" link that the admin can save and paste into WhatsApp. Per the controller's [Option B in the lockdown discussion](../Infrastructure/lockdown-check.log) and the user's "the admin will download and paste it into a whatsapp" workflow, the link is a **short-lived user-delegation SAS URL** minted by the API. The container stays private. No `allowBlobPublicAccess` change.
2. **The home page still says "Hello Buddy rebuild".** That string came from a Day 4a smoke-test edit and never got cleaned up. It will appear in the demo video front and centre.
3. **The two temporary MySQL firewall rules from Day 1** (`developer-ip-temp`, `devbox-ip-temp`) need to go before submission. The deployed app reaches MySQL through `AllowAzureServices` — no human needs direct 3306 access any more.

This brief also creates **DEC-011** (SAS path chosen over anonymous public read) and **TD-007** (the `allowBlobPublicAccess=false` posture and SAS choice should be revisited at production).

## Acceptance criteria

In order. Stop the moment any of them fails and report back to the controller chat.

1. The deployed UI shows the text **"Hello Buddy"** on `/Home/Index`. No "rebuild" anywhere visible.
2. After a Publish, the UI shows a "Download PDF" link (or button). Clicking it from a browser results in the PDF being saved to disk (no 401, no "access denied"). The downloaded bytes start with `%PDF`.
3. The SAS URL embedded in that link is valid for **no more than 30 minutes** from issue.
4. A second click on the same Builder page after 35 minutes (or just clicking a stale URL pasted somewhere) returns a 403 / "Signature did not match" from the blob endpoint. (You can simulate this without waiting by hand-editing the `se=` query parameter to a past time.)
5. Direct anonymous `GET` on the blob URL **without** the SAS still returns 409 / `PublicAccessNotPermitted` (i.e. the lockdown posture is unchanged).
6. `az mysql flexible-server firewall-rule list -g rg-hellobuddy-prod -n mysql-hellobuddy-prod -o table` shows only `AllowAzureServices`. The two `*-temp` rules are gone.
7. The deployed UI URL still passes DoD 1–5 from Day 3 (cases list, programme rows, edit/save, publish, downloadable PDF). Do not re-run DoD 6 — App Insights coverage is unchanged.
8. DecisionLog has DEC-011. TD-001 has TD-007.

## Implementation outline

### Step 1 — Welcome text (2 min)

Edit `Canine Physio Admin/src/HelloBuddy.Ui/Views/Home/Index.cshtml`:

```diff
- <h1 class="display-4">Hello Buddy rebuild</h1>
+ <h1 class="display-4">Hello Buddy</h1>
```

That's the only "rebuild" reference — grep the whole `HelloBuddy.Ui` project to be sure before rebuilding.

### Step 2 — IFileStore.GetReadUrlAsync (15–20 min)

In `Canine Physio Admin/src/HelloBuddy.Api/Services/IFileStore.cs` add one method:

```csharp
/// <summary>
/// Issues a short-lived read URL for the given key. The local file store
/// returns a controller route that streams from disk; the blob store
/// returns a user-delegation SAS URL.
/// </summary>
Task<Uri> GetReadUrlAsync(string key, TimeSpan ttl, CancellationToken ct = default);
```

**`LocalFileStore`** — return a `Uri` like `/published-programmes/{key}` and add a minimal endpoint in `HelloBuddy.Api/Program.cs` (or wherever your dev-only routes live) that streams the file. Dev convenience only.

**`AzureBlobFileStore`** — mint a **user-delegation SAS** (not a service SAS — the UAMI has no account key, by design). The `BlobServiceClient` is already DI-registered with `DefaultAzureCredential`; resolve a `UserDelegationKey` via `GetUserDelegationKeyAsync(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.Add(ttl))` then build a `BlobSasBuilder` with `BlobContainerName`, `BlobName`, `Resource = "b"`, `StartsOn = now-5min`, `ExpiresOn = now+ttl`, `Permissions = BlobSasPermissions.Read`. Sign with the user-delegation key and append the SAS query string to the blob URI.

The `published-programmes` container's UAMI grant (`Storage Blob Data Contributor`) covers user-delegation SAS issuance — no extra RBAC change needed.

Cache the `UserDelegationKey` for ~6 days less a buffer (the max is 7 days) to avoid round-tripping on every download.

### Step 3 — API endpoint to mint the download URL (10 min)

Add to `Canine Physio Admin/src/HelloBuddy.Api/Endpoints/ProgrammeEndpoints.cs`:

```csharp
app.MapGet("/api/programmes/published/{fileName}/download-url", async (
    string fileName,
    IFileStore fileStore,
    CancellationToken ct) =>
{
    // simple guard - must look like our naming pattern
    if (!Regex.IsMatch(fileName, @"^programme-\d+-\d{8}-\d{6}\.pdf$"))
        return Results.BadRequest();
    var url = await fileStore.GetReadUrlAsync(fileName, TimeSpan.FromMinutes(30), ct);
    return Results.Ok(new DownloadUrlResponse(url.ToString()));
});
```

New contract in `HelloBuddy.Contracts`: `public sealed record DownloadUrlResponse(string Url);`

Extend `PublishResponse` to also carry the filename (currently it's just `BlobUri`). Simplest is a second field `FileName` so the UI doesn't have to parse the URI. Update both producer (`ProgrammeEndpoints` Publish handler) and consumer (`AdminApiClient`, `ProgrammesController.Publish`).

### Step 4 — UI download link (15 min)

In `HelloBuddy.Ui/Controllers/ProgrammesController.cs`, after Publish, put **both** the filename and a download-action URL into TempData:

```csharp
TempData["PublishedFile"] = resp.FileName;
TempData["Published"] = $"Published {resp.FileName} ({resp.Bytes:N0} bytes).";
```

Add a new action:

```csharp
[HttpGet("Download/{fileName}")]
public async Task<IActionResult> Download(string fileName, CancellationToken ct)
{
    var url = await _api.GetDownloadUrlAsync(fileName, ct);
    return Redirect(url);
}
```

In `Views/Shared/_Layout.cshtml`, where the `TempData["Published"]` alert already renders inside the aria-live region, append a link when `TempData["PublishedFile"]` is present:

```cshtml
@if (TempData["Published"] is string layoutPublished)
{
    <div class="alert alert-success" role="alert">
        @layoutPublished
        @if (TempData["PublishedFile"] is string fileName)
        {
            <a class="alert-link ms-2"
               asp-controller="Programmes" asp-action="Download"
               asp-route-fileName="@fileName"
               target="_blank" rel="noopener">Download PDF</a>
        }
    </div>
}
```

(The aria-live region already wraps the alert in the post-Day-4a layout — re-read `_Layout.cshtml` before editing to keep the structure right.)

### Step 5 — Add `Azure.Storage.Sas` reference if missing (5 min)

`BlobSasBuilder` and `BlobSasPermissions` live in `Azure.Storage.Blobs` (which is already referenced). No new package needed in most cases — confirm by building.

### Step 6 — Rebuild + redeploy UI only (10 min)

Only `HelloBuddy.Ui` and `HelloBuddy.Api` images change. Tag both with the same short SHA so the trio stays version-aligned in the diagram and report.

```powershell
$tag = (git rev-parse --short HEAD)
cd 'C:\Projects\Hello-Buddy\Canine Physio Admin'
az acr build --registry acrhellobuddyprod --image hello-buddy-ui:$tag  --file src/HelloBuddy.Ui/Dockerfile  . --no-logs
az acr build --registry acrhellobuddyprod --image hello-buddy-api:$tag --file src/HelloBuddy.Api/Dockerfile . --no-logs
cd 'C:\Projects\Hello-Buddy\Infrastructure\terraform\container-platform'
terraform apply -auto-approve `
    -var "deploy_container_app=true" `
    -var "ui_image=acrhellobuddyprod.azurecr.io/hello-buddy-ui:$tag" `
    -var "api_image=acrhellobuddyprod.azurecr.io/hello-buddy-api:$tag" `
    -var "pdf_image=acrhellobuddyprod.azurecr.io/hello-buddy-pdf:ab4c354"   # unchanged from 4a
```

(Variable names may differ — read the current `variables.tf` from the Day 4a state.)

### Step 7 — Remove the two MySQL firewall rules (2 min)

```powershell
az mysql flexible-server firewall-rule delete -g rg-hellobuddy-prod -n mysql-hellobuddy-prod -r developer-ip-temp --yes
az mysql flexible-server firewall-rule delete -g rg-hellobuddy-prod -n mysql-hellobuddy-prod -r devbox-ip-temp     --yes
az mysql flexible-server firewall-rule list   -g rg-hellobuddy-prod -n mysql-hellobuddy-prod -o table
```

The remaining list must be exactly one row: `AllowAzureServices`. The deployed app's MySQL traffic exits the Container Apps environment through that rule and is unaffected.

### Step 8 — Verify (15 min)

Re-run the existing Day 3 acceptance script with the new download step bolted on:

```powershell
$ui = 'https://ca-hello-buddy-ui.mangocliff-d46e000d.uksouth.azurecontainerapps.io'
# 1. Welcome text
$home = Invoke-WebRequest $ui -UseBasicParsing
if ($home.Content -match 'Hello Buddy rebuild') { throw 'welcome text not updated' }
if ($home.Content -notmatch 'Hello Buddy') { throw 'welcome text missing' }
# 2. End-to-end publish + SAS download
# (follow the publish flow from Infrastructure/dod-test-publish.ps1 and then GET /Programmes/Download/<file> and save the response)
# 3. Anonymous blob GET still 409
# 4. Firewall list shows only AllowAzureServices
```

### Step 9 — Documentation (15 min)

**`MSc Assessment/DecisionLog.md`** — append:

```markdown
## DEC-011 — SAS-based download for published PDFs (not anonymous public read)

**Date:** 28 May 2026
**Area:** Identity / blob access
**Decision:** The published-programmes container stays private (`allowBlobPublicAccess=false` at the storage account, container access level `none`). The admin downloads PDFs via a UI link that the UI controller redirects to a user-delegation SAS URL minted by the API. SAS TTL = 30 minutes.

**Alternative considered:** Set the container's public access level to `blob`, exposing PDF URLs anonymously. Rejected because (a) it would require relaxing `allowBlobPublicAccess` at the account level, weakening posture for every future container, and (b) every published PDF would be permanently world-readable to anyone who guessed the deterministic filename pattern (`programme-{id}-{yyyyMMdd-HHmmss}.pdf`) — guessable within a small search space.

**Trade-off:** The admin can't share the URL itself outside the 30-minute window — they download the PDF and attach it to WhatsApp instead. This matches the actual workflow described in the user journey.
```

**`Technical Debt/TD-001 Admin Standards Deviations.md`** — supersede TD-003 (was "disable shared-key access once verification tooling is Entra-only") and add:

```markdown
| TD-007 | Revisit blob download path at production scale: replace SAS-on-demand with Azure Front Door + signed URLs + content negotiation. | Acceptable for assessment-scale; production should not synthesise SAS per request. |
```

Also flip the row that lists the temporary MySQL firewall rules to **Done 2026-05-28** (rules deleted in Step 7).

## Files you will touch

- `Canine Physio Admin/src/HelloBuddy.Ui/Views/Home/Index.cshtml`
- `Canine Physio Admin/src/HelloBuddy.Ui/Views/Shared/_Layout.cshtml`
- `Canine Physio Admin/src/HelloBuddy.Ui/Controllers/ProgrammesController.cs`
- `Canine Physio Admin/src/HelloBuddy.Ui/Services/IAdminApiClient.cs`
- `Canine Physio Admin/src/HelloBuddy.Ui/Services/AdminApiClient.cs`
- `Canine Physio Admin/src/HelloBuddy.Api/Services/IFileStore.cs`
- `Canine Physio Admin/src/HelloBuddy.Api/Services/AzureBlobFileStore.cs`
- `Canine Physio Admin/src/HelloBuddy.Api/Services/LocalFileStore.cs`
- `Canine Physio Admin/src/HelloBuddy.Api/Endpoints/ProgrammeEndpoints.cs`
- `Canine Physio Admin/src/HelloBuddy.Contracts/PublishResponse.cs`
- `Canine Physio Admin/src/HelloBuddy.Contracts/DownloadUrlResponse.cs` (new)
- `MSc Assessment/DecisionLog.md`
- `Technical Debt/TD-001 Admin Standards Deviations.md`

## Fallback gate (45 min in)

If user-delegation SAS issuance fails repeatedly (most likely cause: UAMI lacks the data-plane role propagation — wait 10 min and retry; second-most-likely: the Container App env is missing outbound DNS for `*.blob.core.windows.net` — unlikely given Day-3 evidence), stop and report back. **Do not** fall back to anonymous public read — that's an active reversal of DEC-011 and a controller-level call.

## Report back

When you return to the controller chat, give:

1. Acceptance criteria 1–8 pass/fail.
2. The image tags now deployed for ui / api / pdf.
3. The deployed UI URL with one live download URL pasted in (so the controller can sanity-check it expires).
4. Any new DEC or TD entries beyond DEC-011 + TD-007.
5. Anything that blocks Phase 4 evidence capture.
