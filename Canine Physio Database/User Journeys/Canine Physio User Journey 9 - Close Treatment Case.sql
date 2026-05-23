USE canine_physiotherapy;

START TRANSACTION;

-- ============================================================
-- USER JOURNEY 9: CLOSE TREATMENT CASE
-- Purpose:
--   1. Add a discharge / closure TreatmentCaseNote
--   2. Mark the TreatmentCase as completed
--   3. End any active Practitioner_Pet assignment
--   4. Expire any still-issued registration codes for the pet
-- ============================================================

-- ------------------------------------------------------------
-- 1. LOOK UP ACTIVE TREATMENT CASE
-- ------------------------------------------------------------
SELECT
    tc.TreatmentCaseId,
    tc.PractitionerId,
    tc.PetId
INTO
    @TreatmentCaseId,
    @PractitionerId,
    @PetId
FROM TreatmentCase tc
WHERE tc.Status = 'active'
ORDER BY tc.TreatmentCaseId DESC
LIMIT 1;

SELECT
    @TreatmentCaseId AS TreatmentCaseId,
    @PractitionerId AS PractitionerId,
    @PetId AS PetId;

-- ------------------------------------------------------------
-- 2. ADD CLOSURE NOTE
-- ------------------------------------------------------------
INSERT INTO TreatmentCaseNote (
    TreatmentCaseId,
    PractitionerId,
    CreatedDate,
    NoteType,
    NoteText,
    IsActive
) VALUES (
    @TreatmentCaseId,
    @PractitionerId,
    CURRENT_TIMESTAMP,
    'case_closure',
    'Treatment case closed following successful completion of the rehabilitation plan. Buddy has been discharged from active physiotherapy input with good functional improvement and negligible pain reported at final review. Owner advised to continue normal activity, monitor for recurrence of symptoms, and seek reassessment if any deterioration is observed.',
    TRUE
);

SET @ClosureNoteId = LAST_INSERT_ID();

-- ------------------------------------------------------------
-- 3. CLOSE TREATMENT CASE
-- ------------------------------------------------------------
UPDATE TreatmentCase
SET
    Status = 'completed',
    EndDate = CURRENT_DATE,
    UpdatedDate = CURRENT_TIMESTAMP,
    ClinicalSummary = CONCAT(
        COALESCE(ClinicalSummary, ''),
        CASE
            WHEN ClinicalSummary IS NULL OR TRIM(ClinicalSummary) = '' THEN ''
            ELSE '\n'
        END,
        'Case closed on ',
        DATE_FORMAT(CURRENT_DATE, '%Y-%m-%d'),
        ' after successful completion of physiotherapy.'
    )
WHERE TreatmentCaseId = @TreatmentCaseId;

-- ------------------------------------------------------------
-- 4. END ANY ACTIVE PRACTITIONER_PET ASSIGNMENT
-- ------------------------------------------------------------
UPDATE Practitioner_Pet
SET
    AssignedTo = CURRENT_TIMESTAMP,
    Status = 'ended',
    Notes = CONCAT(
        COALESCE(Notes, ''),
        CASE
            WHEN Notes IS NULL OR TRIM(Notes) = '' THEN ''
            ELSE '\n'
        END,
        'Assignment ended following treatment case closure on ',
        DATE_FORMAT(CURRENT_DATE, '%Y-%m-%d'),
        '.'
    )
WHERE PetId = @PetId
  AND PractitionerId = @PractitionerId
  AND Status = 'active';

-- ------------------------------------------------------------
-- 5. EXPIRE ANY UNUSED OR ISSUED REGISTRATION CODES
-- ------------------------------------------------------------
UPDATE RegistrationCode
SET
    Status = 'expired',
    Notes = CONCAT(
        COALESCE(Notes, ''),
        CASE
            WHEN Notes IS NULL OR TRIM(Notes) = '' THEN ''
            ELSE '\n'
        END,
        'Registration code expired automatically on treatment closure.'
    )
WHERE PetId = @PetId
  AND PractitionerId = @PractitionerId
  AND Status = 'issued';

COMMIT;

-- ============================================================
-- 6. VERIFICATION
-- ============================================================
SELECT
    tcn.TreatmentCaseNoteId,
    tcn.TreatmentCaseId,
    tcn.PractitionerId,
    tcn.CreatedDate,
    tcn.NoteType,
    tcn.NoteText
FROM TreatmentCaseNote tcn
WHERE tcn.TreatmentCaseNoteId = @ClosureNoteId;

SELECT
    tc.TreatmentCaseId,
    tc.PetId,
    tc.PractitionerId,
    tc.CaseTitle,
    tc.Status,
    tc.StartDate,
    tc.EndDate,
    tc.ClinicalSummary
FROM TreatmentCase tc
WHERE tc.TreatmentCaseId = @TreatmentCaseId;

SELECT
    pp.PractitionerPetId,
    pp.PractitionerId,
    pp.PetId,
    pp.AssignedFrom,
    pp.AssignedTo,
    pp.Status,
    pp.Notes
FROM Practitioner_Pet pp
WHERE pp.PetId = @PetId
ORDER BY pp.PractitionerPetId DESC;

SELECT
    rc.RegistrationCodeId,
    rc.Code,
    rc.Status,
    rc.IssuedDate,
    rc.ExpiryDate,
    rc.UsedDate,
    rc.Notes
FROM RegistrationCode rc
WHERE rc.PetId = @PetId
ORDER BY rc.RegistrationCodeId DESC;
