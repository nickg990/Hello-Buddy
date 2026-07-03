# CR018-I9: Complete Owner Deletion for GDPR Right to be Forgotten

## Status
Draft

## Priority
High (GDPR Compliance)

## Increment
9 (Programme Email Send + Audit Trail)

## Change Summary
Replace the current anonymisation approach (which redacts PII but retains records) with complete cascading deletion of owners and all associated data. This simplifies GDPR compliance by ensuring "right to be forgotten" requests result in total data removal rather than managed anonymisation state.

## Rationale
- **Simplicity**: One deletion flow vs. managing anonymisation state across multiple entities.
- **GDPR Compliance**: Complete removal of personal data is more defensible than redaction + retention.
- **Operational**: No need to manage anonymised users in UI/API queries (no special cases, visibility filters, or error handling).
- **Data Integrity**: Cascading delete removes all derived data (PDFs, audit metadata, email records) in one transaction.

## Current Behaviour
- Owner marked as anonymised (FirstName="Anonymised", Email="anonymised-owner-{id}@redacted.local")
- Anonymised owners hidden from list views via WHERE clause filters
- Anonymised owners can still be accessed with `?includeAnonymised=true` flag
- Email send history and message bodies redacted
- Programme versions retained for audit trail
- Cases and pets still linked but hidden

## New Behaviour
- Owner deletion cascade removes:
  - Owner record
  - All linked pets
  - All linked cases (treatment cases)
  - All linked programmes (draft + published versions)
  - All linked programme versions (published history)
  - All linked email send audit records
  - All linked user accounts
  - **All PDF files from blob storage** for published programme versions
- Single audit log entry records deletion with `ActionType = 'gdpr-deletion'`
- No special UI visibility logic needed; owner simply does not exist after deletion

## Implementation Scope

### Code Changes
1. **Infrastructure/Records/RecordRepositories.cs**
   - Replace anonymisation behaviour in `ApplyDataControlAsync` with explicit owner deletion
   - Delete dependents child-to-parent so restrictive FKs are handled consistently in EF and MySQL
   - Delete stored programme PDFs via `IFileStore.DeleteByPrefixAsync("programme-{id}-")`
   - Replace `AddOwnerDataControlAudit` with `AddOwnerGdprDeletionAudit`

2. **API/Endpoints/OwnerEndpoints.cs**
   - Update POST `/api/owners/{id}/data-control` to return only deletion or not-found outcomes
   - Remove `includeAnonymised` query handling from owner endpoints

3. **Contracts**
   - Remove owner anonymisation flags from owner DTOs
   - Remove anonymisation outcomes from application/UI enums
   - Extend `IFileStore` with prefix deletion for PDF cleanup

4. **Tests**
   - Update no-linked-data deletion audit tests to expect `gdpr-deletion`
   - Replace linked-pet anonymisation tests with linked-record deletion tests
   - Retain not-found practitioner-scope tests
   - Update UI smoke/integration tests to remove anonymisation filter scenarios

5. **UI (if applicable)**
   - Remove any admin UI for "include anonymised" toggles
   - Remove visibility filters checking for anonymised owners

### Database Changes
- No schema migration needed; deletion is handled explicitly in application code where FKs are restrictive
- Note: Audit logs are retained indefinitely per compliance policy

### Configuration
- No new config flags needed

## Testing
- Integration test: Owner with pets, cases, programmes, email sends → all deleted, single audit entry
- Practitioner scope: Only practitioner linked to owner can request deletion (return 404 otherwise)
- Audit trail: Verify `ActionType = 'gdpr-deletion'` logged with deletion timestamp

## Acceptance Criteria
1. Owner deletion removes owner + all cascade-linked records in single transaction
2. Audit log entry created with outcome = 'deleted'
3. Deleted owner returns 404 on GET (no visibility flag bypass)
4. No anonymised owner logic in codebase (search/grep for "Anonymised"/"anonymised" returns zero hits)
5. All GDPR deletion tests pass
6. Full test suite passes (118/118 or greater)

## Risk Assessment
- **Low**: Scope limited to owner deletion path; existing case/programme/pet logic unaffected
- **Medium**: Cascading delete may take longer if owner has many records; consider transaction management

## Future Considerations
- If legal requires audit retention separate from operational DB, implement archival before deletion
- If owner wants data export before deletion, implement GDPR data portability endpoint first
