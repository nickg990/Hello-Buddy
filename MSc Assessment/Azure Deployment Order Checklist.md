# Hello Buddy Cloud Admin

## Azure Deployment Order Checklist

## Purpose

This checklist defines the recommended order for provisioning and configuring the Azure production environment for the Hello Buddy Cloud Admin assessment deployment.

The aim is to minimise rework and keep dependencies in the correct sequence.

## Pre-Deployment Preparation

Before creating Azure resources:

1. Finalise container names and image repository names.
2. Confirm the production resource group name.
3. Confirm Azure region.
4. Prepare synthetic seed data only.
5. Decide whether the UI will use the default Container Apps domain or a custom domain.
6. Prepare initial environment variables and secret names.

## Deployment Order

### Phase 1: Foundation resources

Create these first because other resources depend on them.

1. Create the production resource group.
2. Create the Azure Container Registry.
3. Create the Log Analytics workspace.
4. Create Application Insights.
5. Create Azure Key Vault.
6. Create the Storage Account.
7. Create the Blob container `published-programmes`.
8. Create the MySQL Flexible Server.

## Phase 2: Security and configuration

Set up the configuration dependencies before deploying applications.

1. Add database connection secrets to Key Vault.
2. Add storage configuration secrets to Key Vault if needed.
3. Create managed identities for container apps if used.
4. Assign access rights:
   - API to Key Vault
   - API to Blob Storage
   - PDF service to Key Vault if needed
5. Configure MySQL firewall or private access rules as required.
6. Confirm Blob container access is private.

## Phase 3: Container runtime

Provision the shared application runtime next.

1. Create the Azure Container Apps Environment.
2. Connect the environment to Log Analytics.
3. Confirm environment-level networking and ingress settings.

## Phase 4: Build and publish images

Prepare deployable images before creating app revisions.

1. Build the UI image.
2. Build the API image.
3. Build the PDF image.
4. Push all three images to Azure Container Registry.
5. Verify that each image is tagged clearly for the deployment version.

## Phase 5: Deploy internal services first

Deploy the private services before the public UI so dependencies are ready.

1. Deploy `ca-hello-buddy-api` with internal ingress only.
2. Configure API secrets and environment variables.
3. Validate API access to MySQL.
4. Validate API access to Blob Storage.
5. Deploy `ca-hello-buddy-pdf` with internal ingress only.
6. Configure PDF secrets and environment variables.
7. Validate API to PDF service communication.

## Phase 6: Deploy the public UI

Deploy the UI after internal dependencies are confirmed.

1. Deploy `ca-hello-buddy-ui` with external ingress.
2. Configure UI environment variables.
3. Point the UI to the internal API endpoint.
4. Validate public HTTPS access.

## Phase 7: Seed and validate application data

Load the minimum data required for the demo flow.

1. Seed the demo treatment case.
2. Seed the draft programme data.
3. Seed the exercise catalogue entries needed by the first increment.
4. Validate case detail page load.
5. Validate programme builder interactions.
6. Validate PDF preview flow.

## Phase 8: Configure scaling

Apply the agreed starting scale rules.

### UI container app

1. Set minimum replicas to `1`.
2. Set maximum replicas to `3`.
3. Set HTTP concurrency rule to the agreed threshold.

### API container app

1. Set minimum replicas to `0`.
2. Set maximum replicas to `3`.
3. Set HTTP concurrency and optional CPU scale rule.

### PDF container app

1. Set minimum replicas to `0`.
2. Set maximum replicas to `2`.
3. Set low concurrency threshold or CPU-based rule suitable for rendering.

## Phase 9: End-to-end validation

Run the production-slice validation after deployment.

1. Open the public UI.
2. Load the seeded treatment case.
3. Open the programme builder.
4. Save a draft change.
5. Open PDF preview.
6. Trigger publish.
7. Confirm PDF creation.
8. Confirm Blob Storage contains the new PDF.
9. Confirm MySQL metadata is updated.

## Phase 10: Monitoring and evidence capture

Prepare the environment for the assessment report.

1. Confirm logs are arriving in Log Analytics.
2. Confirm request telemetry appears in Application Insights.
3. Record one successful publish trace.
4. Record one validation-blocked scenario.
5. Record baseline replica counts.
6. Run a light load test.
7. Capture scaling evidence for UI and API if possible.

## Recommended Validation Commands And Checks

At minimum, validate:

1. UI public endpoint responds over HTTPS.
2. API health endpoint responds internally.
3. PDF service health endpoint responds internally.
4. MySQL connection succeeds from the API.
5. Blob upload succeeds during publish.

## Failure Checkpoints

If deployment fails, check in this order:

1. image pull from Azure Container Registry;
2. Key Vault secret resolution;
3. internal service DNS or endpoint configuration;
4. MySQL connectivity;
5. Blob Storage permissions;
6. ingress configuration.

## Go-Live Readiness Checklist

Before considering the production environment ready for demo or assessment evidence:

1. UI is reachable publicly.
2. API and PDF are not publicly exposed.
3. database is private.
4. Blob container is private.
5. seeded case and programme load correctly.
6. publish creates a PDF successfully.
7. logs and telemetry are visible.
8. scale settings match the cost plan.
9. all data remains synthetic.

## Recommendation

Deploy internal services before the public UI, keep the initial environment deliberately small, and validate every dependency before moving to the next layer. That approach reduces cost, simplifies troubleshooting, and produces cleaner evidence for the assessment.
