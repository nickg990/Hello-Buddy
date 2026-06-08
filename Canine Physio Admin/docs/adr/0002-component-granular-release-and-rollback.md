# ADR-0002: Component-Granular Release and Rollback

- Status: Accepted
- Date: 2026-06-08

## Context

The platform is split into UI, API, and PDF container apps. Operationally, Release 1 needs proof that components can be redeployed and rolled back independently without forcing full-stack redeploys.

## Decision

Use component-level deployment and rollback drills as standard release operations:

- Deploy: `deploy.ps1` supports `-UiOnly`, `-ApiOnly`, and `-PdfOnly`.
- Rollback: pin traffic to a chosen prior revision for one component.
- Keep full-stack two-phase deploy as the default for broad infrastructure changes.

## Consequences

- Pros: lower blast radius and faster recovery for isolated faults.
- Pros: aligns with Increment 7 acceptance for deployment granularity.
- Cons: requires revision hygiene and explicit drill evidence.
- Cons: operators must verify dependency impact when rolling back one component.
