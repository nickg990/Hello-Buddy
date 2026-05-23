USE canine_physiotherapy;

START TRANSACTION;

-- ============================================================
-- USER JOURNEY 2: INITIAL REVIEW AND CASE SETUP
-- Purpose:
--   Link the pet to the practitioner following the initial review,
--   record the first assessment note, open a treatment case,
--   and issue a registration code for mobile app onboarding.
-- ============================================================

SELECT OwnerId INTO @OwnerId
FROM Owner
WHERE Email = 'emma.thompson@example.com'
LIMIT 1;

SELECT PetId INTO @PetId
FROM Pet
WHERE OwnerId = @OwnerId
  AND Name = 'Buddy'
LIMIT 1;

SELECT PractitionerId INTO @PractitionerId
FROM Practitioner
WHERE Email = 'amelia.carter@caninephysio.local'
LIMIT 1;

INSERT INTO Practitioner_Pet (
    PractitionerId,
    PetId,
    AssignedFrom,
    AssignedTo,
    IsPrimary,
    Status,
    ReferralSource,
    Notes
) VALUES (
    @PractitionerId,
    @PetId,
    CURRENT_TIMESTAMP,
    NULL,
    TRUE,
    'active',
    'Initial consultation booking',
    'Primary practitioner assigned following initial physiotherapy assessment.'
);

SET @PractitionerPetId = LAST_INSERT_ID();

INSERT INTO AssessmentNote (
    PetId,
    PractitionerId,
    CreatedDate,
    NoteText,
    NoteType,
    IsActive
) VALUES (
    @PetId,
    @PractitionerId,
    CURRENT_TIMESTAMP,
    'Initial assessment completed. Mild hind limb weakness observed, reduced hip extension on the left side, and slight reluctance during sit-to-stand transition. Home exercise programme recommended with controlled strengthening and mobility work.',
    'initial_assessment',
    TRUE
);

SET @AssessmentNoteId = LAST_INSERT_ID();

INSERT INTO TreatmentCase (
    PetId,
    PractitionerId,
    CaseTitle,
    ClinicalSummary,
    StartDate,
    EndDate,
    Status
) VALUES (
    @PetId,
    @PractitionerId,
    'Buddy hind limb rehabilitation',
    'Conservative physiotherapy management for mild hind limb weakness and reduced hip mobility following initial practitioner assessment.',
    CURRENT_DATE,
    NULL,
    'active'
);

SET @TreatmentCaseId = LAST_INSERT_ID();

INSERT INTO RegistrationCode (
    PractitionerId,
    PetId,
    Code,
    IssuedDate,
    ExpiryDate,
    UsedDate,
    Status,
    Notes
) VALUES (
    @PractitionerId,
    @PetId,
    CONCAT('REG-', DATE_FORMAT(NOW(), '%Y%m%d'), '-', LPAD(@PetId, 4, '0')),
    CURRENT_TIMESTAMP,
    DATE_ADD(CURRENT_TIMESTAMP, INTERVAL 14 DAY),
    NULL,
    'issued',
    'Registration code issued to owner following initial review.'
);

SET @RegistrationCodeId = LAST_INSERT_ID();

COMMIT;

SELECT
    p.PractitionerId,
    CONCAT(p.FirstName, ' ', p.LastName) AS PractitionerName,
    pet.PetId,
    pet.Name AS PetName,
    pp.PractitionerPetId,
    pp.IsPrimary,
    pp.Status AS AssignmentStatus,
    tc.TreatmentCaseId,
    tc.CaseTitle,
    tc.Status AS TreatmentCaseStatus,
    an.AssessmentNoteId,
    an.NoteType,
    an.CreatedDate AS AssessmentCreatedDate,
    rc.RegistrationCodeId,
    rc.Code AS RegistrationCode,
    rc.Status AS RegistrationStatus,
    rc.ExpiryDate
FROM Pet pet
JOIN Practitioner_Pet pp
    ON pp.PetId = pet.PetId
JOIN Practitioner p
    ON p.PractitionerId = pp.PractitionerId
JOIN TreatmentCase tc
    ON tc.PetId = pet.PetId
   AND tc.PractitionerId = p.PractitionerId
JOIN AssessmentNote an
    ON an.PetId = pet.PetId
   AND an.PractitionerId = p.PractitionerId
JOIN RegistrationCode rc
    ON rc.PetId = pet.PetId
   AND rc.PractitionerId = p.PractitionerId
WHERE pet.PetId = @PetId
ORDER BY an.CreatedDate DESC;
