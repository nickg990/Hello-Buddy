# COM712 Assessment 3 Demo Video Filming Script

## Purpose

This script turns the COM712 optional demonstration video into a recordable sequence of short clips.

It is designed for:

- the Option C Hello Buddy vertical slice;
- a total running time of about 3 minutes 20 seconds;
- direct alignment to the evidence expectations in the assessment brief;
- a recording that can be captured as separate clips and stitched together.

The assessment brief states that an optional artefact may include:

> A demonstration video of two to four minutes showing your deployment, scaling, logs, or metrics.

The same brief also expects evidence of deployment, monitoring, troubleshooting, and metrics in the report, so this video should support that evidence rather than repeat it loosely.

## Recording approach

Record each clip separately.

For each clip, keep four things stable:

- one clear screen objective;
- one short spoken point;
- one visible piece of evidence;
- one clean transition to the next clip.

Prefer 1080p screen capture with zoom at 125% to 150% so labels are readable.

Use synthetic data only.

## Recommended clip sequence

| Clip | Time      | What to show                                                                                                             | What to say                                                                                                                                                                                                                      | Evidence covered                                               |
| ---- | --------- | ------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | -------------------------------------------------------------- |
| 1    | 0:00-0:15 | Title slide or a simple opening frame with project name, platform, and architecture summary                              | "This is my COM712 Option C submission: a containerised Hello Buddy canine physiotherapy admin slice deployed on Azure. It uses a practitioner web app, a separate PDF service, managed MySQL, Blob Storage, and Azure Monitor." | Clear framing, architecture scope, two-container design        |
| 2    | 0:15-0:35 | Azure portal resource overview showing the deployed services and environment                                             | "The solution is deployed in Azure with the web application and PDF service running as separate containerised services, supported by managed storage, monitoring, and database services."                                        | Deployment evidence, cloud configuration                       |
| 3    | 0:35-0:55 | Container service details showing image, revision, ingress URL, and healthy running state                                | "Here the deployed revision is live, reachable over HTTPS, and configured from container images rather than manual server setup."                                                                                                | Working deployment, reproducibility, service status            |
| 4    | 0:55-1:30 | Open the public URL and perform the core end-to-end workflow: case detail to programme builder to PDF preview            | "This is the assessed vertical slice. Starting from a seeded treatment case, I open the draft programme, review the builder, and move into the owner-facing PDF preview before publish."                                         | Working application, end-to-end flow, realistic scenario       |
| 5    | 1:30-1:50 | Trigger publish and show the confirmation result or stored document outcome                                              | "Publishing creates a version, calls the separate PDF service, and stores the generated programme output. This demonstrates service interaction rather than a single monolithic container."                                      | Multi-service behaviour, PDF service boundary, storage outcome |
| 6    | 1:50-2:20 | Azure log stream, Log Analytics, or Application Insights showing a publish request and related service logs              | "These logs show the request path through the deployed services. I can trace the publish action, verify successful processing, and inspect failures when needed."                                                                | Logs, monitoring, observability                                |
| 7    | 2:20-2:45 | Azure Monitor or Application Insights charts for requests, latency, CPU, memory, failures, and if possible replica count | "These metrics provide measurable evidence of runtime behaviour, including request volume, latency, and resource usage, rather than relying only on screenshots of a working page."                                              | Metrics, monitoring, performance evidence                      |
| 8    | 2:45-3:10 | Scaling configuration and, if available, a scale event, replica change, or load test result                              | "This is the scaling policy and supporting evidence. Under load, the service can scale according to configured thresholds, and I used monitoring output to verify the behaviour."                                                | Scaling, load response, operations evidence                    |
| 9    | 3:10-3:20 | Closing frame with key outcomes                                                                                          | "In summary, the submission demonstrates cloud deployment, service separation, monitoring, and measurable operational evidence."                                                                                                 | Strong finish, communication                                   |

## Clip-by-clip recording script

### Clip 1: Title and architecture context

**Duration:** 15 seconds

**On screen:**

- project title;
- module and option label;
- one-line architecture summary.

**Suggested wording:**

"This is my COM712 Assessment 3 Option C submission. I built a containerised Azure deployment for the Hello Buddy admin slice, using a web app, a separate PDF service, managed database services, Blob Storage, and Azure monitoring."

**Recording note:**

Keep this static and clean. It sets context fast and avoids spending valuable time explaining basics later.

### Clip 2: Azure deployment overview

**Duration:** 20 seconds

**On screen:**

- Azure resource group or architecture view;
- container runtime resource;
- database and storage resources;
- monitoring resources if visible.

**Suggested wording:**

"This is the deployed environment in Azure. The application is not just running locally in Docker Compose. It is deployed as a cloud solution with managed supporting services for storage, database, and observability."

**Evidence goal:**

Show that the solution is genuinely deployed in cloud infrastructure, not only simulated.

### Clip 3: Running service and ingress

**Duration:** 20 seconds

**On screen:**

- service details page;
- running revision or container status;
- ingress or application URL;
- image reference if readable.

**Suggested wording:**

"The service is running from container images, exposed through a public HTTPS endpoint, and configured as a deployed workload rather than a manually hosted server."

**Evidence goal:**

Support the report's build and deploy section with visible runtime proof.

### Clip 4: End-to-end clinical workflow

**Duration:** 35 seconds

**On screen:**

- open seeded case detail;
- move into programme builder;
- edit a seeded exercise (reps / sets / hold / notes) and save;
- open PDF preview.

**Suggested wording:**

"For the application slice itself, I start with a seeded treatment case, open the draft programme builder, adjust the prescription detail on a seeded exercise, and preview the owner-facing PDF. This keeps the demo focused on one coherent end-to-end value path."

**Evidence goal:**

Prove the vertical slice is functional and connected to a believable user workflow.

### Clip 5: Publish and separate PDF service result

**Duration:** 20 seconds

**On screen:**

- click publish;
- confirmation state or published version page;
- stored file link, metadata, or confirmation panel.

**Suggested wording:**

"When I publish, the system creates a version and calls the separate PDF service. That service boundary is intentional because it isolates rendering work and creates a measurable inter-service workflow."

**Evidence goal:**

Demonstrate the second container is meaningful, not artificial.

### Clip 6: Logs and trace evidence

**Duration:** 25 seconds

**On screen:**

- log stream or Log Analytics query;
- entries for the publish request;
- optionally filter by time or correlation ID.

**Suggested wording:**

"Here I show the operational logs for the publish request. This provides evidence that the deployed services are observable and that faults can be investigated through platform monitoring rather than guesswork."

**Evidence goal:**

Cover the brief's explicit mention of logs and support the report's test, monitor, and fix section.

### Clip 7: Metrics dashboard

**Duration:** 20 seconds

**On screen:**

- Azure Monitor or Application Insights charts;
- request count;
- response time;
- CPU or memory;
- failures if available.

**Suggested wording:**

"These metrics show measurable behaviour from the deployed system, including traffic, latency, and resource consumption. This moves the evidence beyond screenshots toward monitored operational data."

**Evidence goal:**

Hit the application-of-evidence rubric with real measurements.

### Clip 8: Scaling evidence

**Duration:** 20 seconds

**On screen:**

Option A, preferred:

- load test result next to monitor charts;
- replica count before and after;
- scaling event or timeline.

Option B, fallback:

- scale rule configuration;
- threshold definition;
- explanation of how scaling would trigger;
- any observed change in requests or resources.

**Suggested wording:**

"This is the scaling configuration and its supporting evidence. Where a live scale event is visible, I can show the replica change. If the event is not captured live, the configured thresholds and measured workload data still demonstrate how the service responds under load."

**Evidence goal:**

Show scaling as an operational concern, not just a checkbox.

### Clip 9: Close

**Duration:** 5 seconds

**On screen:**

- short summary slide with five keywords: deployment, publish flow, logs, metrics, scaling.

**Suggested wording:**

"This submission demonstrates deployment, service separation, monitoring, scaling evidence, and fault-aware design in a reproducible containerised cloud workflow."

## Pre-recording tab preparation

Open these browser tabs in order before recording so each clip is one click away. Keeping the same tab order as the clip sequence avoids fumbling on camera.

- **Tab 1** — Title/architecture slide (or the README header).
- **Tab 2** — Azure portal: `rg-hellobuddy-prod` Overview.
- **Tab 3** — Azure portal: `ca-hello-buddy-ui` Overview (revision, ingress URL, status).
- **Tab 4** — UI app: a seeded **case detail** page, ready to open the draft programme.
- **Tab 5** — UI app: **programme builder / PDF preview** for the seeded draft programme, ready to edit and publish (see steps below).
- **Tab 6** — Azure portal: Storage `published-programmes` (to show the stored output after publish).
- **Tab 7** — App Insights: Transaction search / end-to-end transaction view.
- **Tab 8** — App Insights: Metrics charts (requests, latency, CPU/memory, failures).
- **Tab 9** — `ca-hello-buddy-ui` → Scale (rules) and Revisions and replicas.

### Tab 5 setup — stage the builder and preview for Clips 4 and 5

The goal is that when you switch to Tab 5 on camera, the seeded draft programme is already open on the builder so you can show a quick edit and the live preview, then publish, without any fumbling. Prepare it like this:

1. **Warm the UI first.** Hit `GET https://<ui-fqdn>/healthz` and confirm `{"status":"ok"}` so there is no cold-start pause during the take.
2. **Open the seeded case** and open its **draft** programme (status must be *draft*, not already published, so the publish action is still available for Clip 5).
3. **Land on the builder step** with the seeded exercises visible. The editable fields are **reps, sets, hold seconds, sort order, and notes** on the already-seeded exercises — have one small edit in mind (for example bump reps or add a note) so the builder is clearly interactive on camera.
4. **Open the live Preview** once off-camera to confirm the owner-facing PDF preview renders, then return to the builder. This proves the preview round-trips before you record.
5. **Position the page with the Publish button visible** but do not click Publish yet — that is the on-camera action for Clip 5.

On camera: Clip 4 shows the seeded case → builder edit → live PDF preview. Clip 5 then clicks **Publish** and shows the stored output (Tab 6) and the inter-service call into the PDF service.

## Minimal evidence checklist before recording

Record only after these are ready:

- deployed public URL works over HTTPS;
- both application services are visibly running;
- one seeded case supports the full demo path;
- one successful publish has been tested already;
- logs for a publish request are queryable;
- metrics dashboard is populated with recent data;
- scaling settings are configured and visible;
- load test output exists if you plan to claim observed scaling.

## Fast retake guidance

If time is tight, keep Clips 1 to 7 and shorten or merge Clips 8 and 9.

That still gives a strong three-minute submission covering:

- deployment;
- end-to-end workflow;
- publish and service separation;
- logs;
- metrics.

## Distinction-oriented emphasis

If you want the video to support distinction-level evidence, prioritise these visible details:

- not just that the app works, but that the deployment is clearly cloud-hosted;
- real monitoring output rather than static UI screenshots only;
- the inter-service publish path crossing into the separate PDF service;
- measured data such as latency, CPU, memory, request count, or replica count;
- a brief sign of reproducibility, such as image-based deployment, revision history, or consistent configuration.

## Suggested file naming for exported clips

- `01-title.mp4`
- `02-azure-overview.mp4`
- `03-running-service.mp4`
- `04-end-to-end-flow.mp4`
- `05-publish-result.mp4`
- `06-logs.mp4`
- `07-metrics.mp4`
- `08-scaling.mp4`
- `09-close.mp4`
