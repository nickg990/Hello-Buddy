USE canine_physiotherapy;

-- ============================================================
-- FINAL REVIEW CHECK
-- Purpose:
--   Review the outcome after Journey 8 and before / after Journey 9
-- ============================================================

-- 1. Latest review / closure notes for the treatment case
SELECT
    tcn.TreatmentCaseNoteId,
    tcn.TreatmentCaseId,
    tcn.CreatedDate,
    tcn.NoteType,
    tcn.NoteText,
    CONCAT(p.FirstName, ' ', p.LastName) AS PractitionerName
FROM TreatmentCaseNote tcn
JOIN Practitioner p
    ON p.PractitionerId = tcn.PractitionerId
ORDER BY tcn.TreatmentCaseNoteId DESC;

-- 2. Programme status summary
SELECT
    ProgrammeId,
    ProgrammeName,
    Status,
    IsCurrent,
    StartDate,
    EndDate,
    CurrentProgrammeVersionId,
    Notes
FROM Programme
ORDER BY ProgrammeId;

-- 3. Treatment case status
SELECT
    TreatmentCaseId,
    PetId,
    PractitionerId,
    CaseTitle,
    Status,
    StartDate,
    EndDate,
    ClinicalSummary
FROM TreatmentCase
ORDER BY TreatmentCaseId;

-- 4. Current practitioner-pet assignment state
SELECT
    PractitionerPetId,
    PractitionerId,
    PetId,
    AssignedFrom,
    AssignedTo,
    IsPrimary,
    Status,
    ReferralSource,
    Notes
FROM Practitioner_Pet
ORDER BY PractitionerPetId;

-- 5. Registration code state
SELECT
    RegistrationCodeId,
    PractitionerId,
    PetId,
    Code,
    IssuedDate,
    ExpiryDate,
    UsedDate,
    Status,
    Notes
FROM RegistrationCode
ORDER BY RegistrationCodeId;

-- 6. Latest programme version records
SELECT
    ProgrammeVersionId,
    ProgrammeId,
    VersionNumber,
    VersionStatus,
    PayloadSchemaVersion,
    CreatedByPractitionerId,
    CreatedDate,
    PublishedDate,
    SupersededDate,
    RetiredDate
FROM ProgrammeVersion
ORDER BY ProgrammeId, VersionNumber;

-- 7. Helpful “single view” summary
SELECT
    tc.TreatmentCaseId,
    tc.CaseTitle,
    tc.Status AS TreatmentCaseStatus,
    tc.StartDate AS TreatmentStartDate,
    tc.EndDate AS TreatmentEndDate,
    p.PetId,
    p.Name AS PetName,
    CONCAT(pr.FirstName, ' ', pr.LastName) AS PractitionerName
FROM TreatmentCase tc
JOIN Pet p
    ON p.PetId = tc.PetId
JOIN Practitioner pr
    ON pr.PractitionerId = tc.PractitionerId
ORDER BY tc.TreatmentCaseId DESC;