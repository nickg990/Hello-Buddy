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
| 5    | 1:30-1:50 | Show a blocked publish state, such as missing prescription data or missing video link, then fix it                       | "I also tested a failure path. Publish is blocked when required data is missing. After correcting the issue, the system returns to a valid publishable state."                                                                   | Troubleshooting, validation, fault handling                    |
| 6    | 1:50-2:10 | Trigger publish and show the confirmation result or stored document outcome                                              | "Publishing creates a version, calls the separate PDF service, and stores the generated programme output. This demonstrates service interaction rather than a single monolithic container."                                      | Multi-service behaviour, PDF service boundary, storage outcome |
| 7    | 2:10-2:35 | Azure log stream, Log Analytics, or Application Insights showing a publish request and related service logs              | "These logs show the request path through the deployed services. I can trace the publish action, verify successful processing, and inspect failures when needed."                                                                | Logs, monitoring, observability                                |
| 8    | 2:35-2:55 | Azure Monitor or Application Insights charts for requests, latency, CPU, memory, failures, and if possible replica count | "These metrics provide measurable evidence of runtime behaviour, including request volume, latency, and resource usage, rather than relying only on screenshots of a working page."                                              | Metrics, monitoring, performance evidence                      |
| 9    | 2:55-3:15 | Scaling configuration and, if available, a scale event, replica change, or load test result                              | "This is the scaling policy and supporting evidence. Under load, the service can scale according to configured thresholds, and I used monitoring output to verify the behaviour."                                                | Scaling, load response, operations evidence                    |
| 10   | 3:15-3:20 | Closing frame with key outcomes                                                                                          | "In summary, the submission demonstrates cloud deployment, service separation, monitoring, troubleshooting, and measurable operational evidence."                                                                                | Strong finish, communication                                   |

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
- show session and exercise selection;
- open PDF preview.

**Suggested wording:**

"For the application slice itself, I start with a seeded treatment case, open the draft programme workflow, adjust the programme content, and preview the owner-facing PDF. This keeps the demo focused on one coherent end-to-end value path."

**Evidence goal:**

Prove the vertical slice is functional and connected to a believable user workflow.

### Clip 5: Validation failure and fix

**Duration:** 20 seconds

**On screen:**

- blocked publish state;
- validation message or warning list;
- fix the missing field;
- state changes to publishable.

**Suggested wording:**

"I tested a realistic failure condition by leaving required programme data incomplete. The system blocks publish, surfaces the issue clearly, and then allows the workflow to continue once the data is corrected."

**Evidence goal:**

This directly supports troubleshooting and resilience evidence instead of showing only a happy path.

### Clip 6: Publish and separate PDF service result

**Duration:** 20 seconds

**On screen:**

- click publish;
- confirmation state or published version page;
- stored file link, metadata, or confirmation panel.

**Suggested wording:**

"When I publish, the system creates a version and calls the separate PDF service. That service boundary is intentional because it isolates rendering work and creates a measurable inter-service workflow."

**Evidence goal:**

Demonstrate the second container is meaningful, not artificial.

### Clip 7: Logs and trace evidence

**Duration:** 25 seconds

**On screen:**

- log stream or Log Analytics query;
- entries for the publish request;
- optionally filter by time or correlation ID.

**Suggested wording:**

"Here I show the operational logs for the publish request. This provides evidence that the deployed services are observable and that faults can be investigated through platform monitoring rather than guesswork."

**Evidence goal:**

Cover the brief's explicit mention of logs and support the report's test, monitor, and fix section.

### Clip 8: Metrics dashboard

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

### Clip 9: Scaling evidence

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

### Clip 10: Close

**Duration:** 5 seconds

**On screen:**

- short summary slide with five keywords: deployment, publish flow, logs, metrics, scaling.

**Suggested wording:**

"This submission demonstrates deployment, service separation, monitoring, scaling evidence, and fault-aware design in a reproducible containerised cloud workflow."

## Minimal evidence checklist before recording

Record only after these are ready:

- deployed public URL works over HTTPS;
- both application services are visibly running;
- one seeded case supports the full demo path;
- one blocked publish scenario is ready to trigger;
- one successful publish has been tested already;
- logs for a publish request are queryable;
- metrics dashboard is populated with recent data;
- scaling settings are configured and visible;
- load test output exists if you plan to claim observed scaling.

## Fast retake guidance

If time is tight, keep Clips 1 to 8 and shorten or merge Clips 9 and 10.

That still gives a strong three-minute submission covering:

- deployment;
- end-to-end workflow;
- troubleshooting;
- logs;
- metrics.

## Distinction-oriented emphasis

If you want the video to support distinction-level evidence, prioritise these visible details:

- not just that the app works, but that the deployment is clearly cloud-hosted;
- real monitoring output rather than static UI screenshots only;
- one demonstrated fault or blocked state;
- measured data such as latency, CPU, memory, request count, or replica count;
- a brief sign of reproducibility, such as image-based deployment, revision history, or consistent configuration.

## Suggested file naming for exported clips

- `01-title.mp4`
- `02-azure-overview.mp4`
- `03-running-service.mp4`
- `04-end-to-end-flow.mp4`
- `05-validation-fix.mp4`
- `06-publish-result.mp4`
- `07-logs.mp4`
- `08-metrics.mp4`
- `09-scaling.mp4`
- `10-close.mp4`
