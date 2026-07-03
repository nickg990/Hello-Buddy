# CR016-I7 - RTBF Privacy Page and Anonymised Owner Visibility Controls

Date: 2026-06-08
Status: Implemented
Owner: Product / Release 1 planning
Scope: Canine Physio Admin (Privacy page, Owner/Pet/Case visibility)

## Why this change

The increment introduced a Privacy-page action for right to be forgotten (RTBF), but the previous implementation shifted to full hard-delete for linked records.

For GDPR-aligned clinical retention, the required behavior is:
- hard delete when no linked data exists;
- anonymise owner personal data when linked pet/clinical records exist.

Additionally, anonymised records should not appear in normal operational views (Pets/Cases screens), while still being reviewable from the Owner area when explicitly requested.

## Scope

In scope:
- Keep RTBF action entry point on the Privacy page with confirmation and irreversible warning text.
- Restore data-control behavior:
  - no linked records: delete owner;
  - linked records: anonymise owner/account personal data and retain linked pet/clinical records.
- Hide anonymised owners by default on the Owners index.
- Add a Show anonymised / Hide anonymised toggle on the Owners index.
- Allow anonymised owner visibility/drill-down only from Owner screens when show anonymised is enabled.
- Prevent anonymised-owner related data appearing in Pets and Cases screens (lists/details/visibility checks).
- Update API and test coverage for these behaviors.

Out of scope:
- New database schema changes (no extra anonymised flag column in this CR).
- Global cross-screen toggle for anonymised data.

## Acceptance criteria

1. Privacy page offers RTBF action with clear GDPR context, confirmation, and irreversible warning.
2. RTBF with no linked pet/account data returns deleted outcome and owner no longer exists.
3. RTBF with linked pet/clinical data returns anonymised outcome and retained linked records.
4. Owners index excludes anonymised owners by default.
5. Owners index Show anonymised toggle reveals anonymised owners and supports opening owner details.
6. Pets and Cases screens do not surface anonymised-owner related records.
7. Automated API tests cover both delete and anonymise outcomes and visibility restrictions.

## Implementation notes

- Owner endpoints now support includeAnonymised query for owner list/detail API calls.
- Owner repository applies anonymisation marker rules and filtering for owner list/detail visibility.
- Pet and case repositories now suppress records tied to anonymised owners for list/detail/exists checks.
- Owner UI now includes Show anonymised toggle and preserves toggle state when navigating to owner details.
- Owner details show anonymised status and avoid directing users into hidden pet screens for anonymised owners.
- Privacy page text now reflects delete-or-anonymise behavior and avoids patient terminology.

## Risks and mitigations

Risks:
- Anonymisation detection currently uses marker conventions (name/email pattern) rather than a dedicated flag column.
- Existing historical anonymised rows must match marker pattern to be filtered consistently.

Mitigations:
- Use a single marker convention for anonymised owner records in data-control logic.
- Validate behavior in both in-memory and integration suites.
- Consider future DB-backed explicit anonymised status in a separate CR/ADR if needed.
