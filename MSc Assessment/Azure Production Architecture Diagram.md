# Hello Buddy Cloud Admin

## Azure Production Architecture Diagram

## Mermaid Diagram

```mermaid
flowchart LR
    classDef public fill:#D4E0E5,stroke:#6392AE,stroke-width:1px,color:#28404F
    classDef service fill:#FFFFFF,stroke:#6392AE,stroke-width:1px,color:#28404F
    classDef platform fill:#E6EFC2,stroke:#5A6E2D,stroke-width:1px,color:#2E3B14
    classDef private fill:#FFF7E0,stroke:#B58900,stroke-width:1px,color:#5C4A00
    classDef registry fill:#F4F8FA,stroke:#5A6E7A,stroke-width:1px,color:#28404F

    U[Practitioner Browser]:::public
    I[Public HTTPS Ingress]:::public

    subgraph APP [Azure Container Apps Environment - cae-hellobuddy-prod]
        direction TB
        UI[UI Container App\nca-hello-buddy-ui\nPublic]:::service
        API[API Container App\nca-hello-buddy-api\nInternal]:::service
        PDF[PDF Generator Container App\nca-hello-buddy-pdf\nInternal]:::service
    end

    subgraph DATA [Private Data and Platform Services]
        direction TB
        DB[Azure Database for MySQL Flexible Server\nmysql-hellobuddy-prod]:::private
        BLOB[Azure Blob Storage\nsthellobuddyprod\nContainer: published-programmes]:::private
        KV[Azure Key Vault\nkv-hellobuddy-prod]:::private
    end

    subgraph OBS [Observability]
        direction TB
        LOG[Log Analytics Workspace\nlog-hellobuddy-prod]:::platform
        APPI[Application Insights\nappi-hellobuddy-prod]:::platform
    end

    ACR[Azure Container Registry\nacrhellobuddyprod]:::registry

    U --> I --> UI
    UI --> API
    API --> PDF
    API <--> DB
    API --> BLOB
    API --> KV
    PDF --> KV
    UI --> LOG
    API --> LOG
    PDF --> LOG
    UI --> APPI
    API --> APPI
    PDF --> APPI
    ACR --> UI
    ACR --> API
    ACR --> PDF
```

## Caption

The Hello Buddy Cloud Admin production environment uses Azure Container Apps to host three independently deployable workloads. Only the UI container is publicly exposed. The API and PDF generator remain internal services inside the shared Container Apps environment. Structured business data is stored in Azure Database for MySQL Flexible Server, while published PDF outputs are stored in Azure Blob Storage. Secure configuration and observability are provided through Azure Key Vault, Log Analytics, and Application Insights.

## Notes For Export

- Use this Mermaid diagram directly in Markdown-capable tooling if supported.
- For submission visuals, export the rendered diagram to SVG or PNG so labels remain crisp in the PDF report.
- If the diagram feels too dense in portrait format, move observability to a side note box rather than the main flow.
