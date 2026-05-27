-- =============================================================================
-- CANINE PHYSIO DB - MSC ASSESSMENT SEED v1
-- =============================================================================
-- Parent scripts:
--   Canine Physio DB Scripts v2.3 (fresh).sql      (schema)
--   Canine Physio DB Day 1 Initialise v2.4.sql      (reference/lookup data)
--
-- Purpose:
--   Loads the synthetic demo case data required for the COM712 Assessment 3
--   first-increment demonstration. Assumes the schema and reference data
--   (Practitioner, ExerciseCategory, Exercise, SessionContentType,
--   TermsDocument) have already been applied by the scripts above.
--
-- Scope:
--   Owner, Pet, UserAccount, NotificationPreference, Practitioner_Pet,
--   TreatmentCase, TreatmentCaseNote, RegistrationCode, Programme,
--   Session, SessionExercise
--
-- All data is synthetic. No real personal or clinical information is used.
--
-- Re-runnable: yes. Deletes then re-inserts the demo records in FK-safe order.
-- Run order: 1) schema script, 2) init script (v2.4), 3) this seed script.
-- =============================================================================

USE canine_physiotherapy;

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ---------------------------------------------------------------------------
-- CLEAN UP: remove existing demo data in reverse FK order
-- Scoped tightly to the synthetic demo owner to avoid touching other data.
-- ---------------------------------------------------------------------------

SET @CleanOwnerId = NULL;
SELECT OwnerId INTO @CleanOwnerId
FROM Owner
WHERE Email = 'emma.thompson@example.com'
LIMIT 1;

-- Session-level and programme data cascades from TreatmentCase via Programme
-- Delete RegistrationCode rows linked to the demo pet (no cascade on RegistrationCode_Pet)
DELETE rc FROM RegistrationCode rc
JOIN Pet p ON p.PetId = rc.PetId
JOIN Owner o ON o.OwnerId = p.OwnerId
WHERE o.Email = 'emma.thompson@example.com';

-- Practitioner_Pet rows (no cascade from Pet)
DELETE pp FROM Practitioner_Pet pp
JOIN Pet p ON p.PetId = pp.PetId
JOIN Owner o ON o.OwnerId = p.OwnerId
WHERE o.Email = 'emma.thompson@example.com';

-- TreatmentCase (cascades to TreatmentCaseNote, Programme → Session → SessionExercise)
DELETE tc FROM TreatmentCase tc
JOIN Pet p ON p.PetId = tc.PetId
JOIN Owner o ON o.OwnerId = p.OwnerId
WHERE o.Email = 'emma.thompson@example.com';

-- UserAccount (cascades to NotificationPreference, TermsAcceptance)
DELETE ua FROM UserAccount ua
WHERE ua.Email = 'emma.thompson@example.com';

-- Pet (cascades to nothing beyond what is already deleted)
DELETE p FROM Pet p
JOIN Owner o ON o.OwnerId = p.OwnerId
WHERE o.Email = 'emma.thompson@example.com';

-- Owner
DELETE FROM Owner WHERE Email = 'emma.thompson@example.com';

SET FOREIGN_KEY_CHECKS = 1;

-- ---------------------------------------------------------------------------
-- INSERT DEMO DATA
-- ---------------------------------------------------------------------------

START TRANSACTION;

-- ---------- Owner ----------
INSERT INTO Owner (
    FirstName, LastName, Email, PhoneNumber,
    AddressLine1, AddressLine2, Town, Postcode
) VALUES (
    'Emma', 'Thompson', 'emma.thompson@example.com', '07700 900201',
    '14 Willow Close', NULL, 'Leighton Buzzard', 'LU7 2AB'
);
SET @OwnerId = LAST_INSERT_ID();

-- ---------- UserAccount ----------
INSERT INTO UserAccount (
    OwnerId, Email, PasswordHash, PasswordSalt, IsActive
) VALUES (
    @OwnerId,
    'emma.thompson@example.com',
    'HASH_PLACEHOLDER_EMMA_001',
    'SALT_PLACEHOLDER_EMMA_001',
    TRUE
);
SET @UserAccountId = LAST_INSERT_ID();

-- ---------- NotificationPreference ----------
INSERT INTO NotificationPreference (
    UserAccountId, NotificationsEnabled, NotificationTime,
    DownloadVideosEnabled, OfflineCachingEnabled
) VALUES (
    @UserAccountId, TRUE, '08:00:00', TRUE, TRUE
);

-- ---------- Pet ----------
INSERT INTO Pet (
    OwnerId, Name, Age, DateOfBirth, Breed, Sex, Weight, IsActive
) VALUES (
    @OwnerId, 'Buddy', 8, NULL, 'Labrador Retriever', 'male', 28.50, TRUE
);
SET @PetId = LAST_INSERT_ID();

-- ---------- Practitioner lookup ----------
SELECT PractitionerId INTO @PractitionerId
FROM Practitioner
WHERE Email = 'amelia.carter@caninephysio.local'
LIMIT 1;

-- ---------- Practitioner_Pet ----------
INSERT INTO Practitioner_Pet (
    PractitionerId, PetId, AssignedFrom, AssignedTo,
    IsPrimary, Status, ReferralSource, Notes
) VALUES (
    @PractitionerId, @PetId, CURRENT_TIMESTAMP, NULL,
    TRUE, 'active',
    'Initial consultation booking',
    'Primary practitioner assigned following initial physiotherapy assessment.'
);

-- ---------- TreatmentCase ----------
INSERT INTO TreatmentCase (
    PetId, PractitionerId, CaseTitle, ClinicalSummary,
    StartDate, EndDate, Status
) VALUES (
    @PetId, @PractitionerId,
    'Buddy hind limb rehabilitation',
    'Conservative physiotherapy management for mild hind limb weakness and reduced hip mobility following initial practitioner assessment.',
    CURRENT_DATE, NULL, 'active'
);
SET @TreatmentCaseId = LAST_INSERT_ID();

-- ---------- TreatmentCaseNote: initial assessment ----------
INSERT INTO TreatmentCaseNote (
    TreatmentCaseId, PractitionerId, CreatedDate, NoteType, NoteText, IsActive
) VALUES (
    @TreatmentCaseId, @PractitionerId, CURRENT_TIMESTAMP,
    'initial_assessment',
    'Initial assessment completed. Mild hind limb weakness observed, reduced hip extension on the left side, and slight reluctance during sit-to-stand transition. Home exercise programme recommended with controlled strengthening and mobility work.',
    TRUE
);

-- ---------- TreatmentCaseNote: follow-up note (adds demo timeline) ----------
INSERT INTO TreatmentCaseNote (
    TreatmentCaseId, PractitionerId, CreatedDate, NoteType, NoteText, IsActive
) VALUES (
    @TreatmentCaseId, @PractitionerId,
    DATE_ADD(CURRENT_TIMESTAMP, INTERVAL -3 DAY),
    'progress_note',
    'Owner reports good compliance with the exercise programme. Buddy is completing AM and PM sessions without difficulty. Slight improvement in sit-to-stand transition noted. Continue current programme and review at next appointment.',
    TRUE
);

-- ---------- RegistrationCode ----------
INSERT INTO RegistrationCode (
    PractitionerId, PetId, Code, IssuedDate, ExpiryDate, UsedDate, Status, Notes
) VALUES (
    @PractitionerId, @PetId,
    CONCAT('REG-MSC-', DATE_FORMAT(NOW(), '%Y%m%d'), '-', LPAD(@PetId, 4, '0')),
    CURRENT_TIMESTAMP, DATE_ADD(CURRENT_TIMESTAMP, INTERVAL 14 DAY),
    NULL, 'issued',
    'Registration code issued to owner following initial review — MSc assessment demo data.'
);

-- ---------- Programme ----------
SELECT SessionContentTypeId INTO @SessionTypeAM
FROM SessionContentType WHERE ContentKey = 'hindlimbCore' LIMIT 1;

SELECT SessionContentTypeId INTO @SessionTypePM
FROM SessionContentType WHERE ContentKey = 'mobilityFlexibility' LIMIT 1;

INSERT INTO Programme (
    TreatmentCaseId, ProgrammeTemplateId, ProgrammeName,
    StartDate, EndDate, Status, IsCurrent, Notes
) VALUES (
    @TreatmentCaseId, NULL,
    'Buddy Initial Home Rehabilitation Programme',
    CURRENT_DATE, DATE_ADD(CURRENT_DATE, INTERVAL 14 DAY),
    'active', TRUE,
    'Initial home exercise programme created following first physiotherapy review. MSc assessment demo data.'
);
SET @ProgrammeId = LAST_INSERT_ID();

-- ---------- Sessions ----------
INSERT INTO Session (
    ProgrammeId, SessionContentTypeId, CreatedDate,
    Period, Objective, Status, SortOrder
) VALUES (
    @ProgrammeId, @SessionTypeAM, CURRENT_TIMESTAMP,
    'AM', 'Morning strengthening and core control session.', 'active', 1
);
SET @SessionAM = LAST_INSERT_ID();

INSERT INTO Session (
    ProgrammeId, SessionContentTypeId, CreatedDate,
    Period, Objective, Status, SortOrder
) VALUES (
    @ProgrammeId, @SessionTypePM, CURRENT_TIMESTAMP,
    'PM', 'Evening mobility, balance and controlled movement session.', 'active', 2
);
SET @SessionPM = LAST_INSERT_ID();

-- ---------- AM Session exercises ----------
INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionAM, ExerciseId, 5, 3, NULL, 1, 'Lead AM session with controlled forelimb loading.'
FROM Exercise WHERE ExerciseKey = 'stepUp';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionAM, ExerciseId, 3, 1, 7, 2, 'Gentle spinal mobility exercise.'
FROM Exercise WHERE ExerciseKey = 'baitedBackStretch';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionAM, ExerciseId, 5, 2, NULL, 3, 'Controlled sit-to-stand strengthening.'
FROM Exercise WHERE ExerciseKey = 'raisedSitStands';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionAM, ExerciseId, 5, 1, NULL, 4, 'Finish AM session with controlled pole walking.'
FROM Exercise WHERE ExerciseKey = 'raisedPoles';

-- ---------- PM Session exercises ----------
INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionPM, ExerciseId, 3, 2, NULL, 1, 'Start PM with slow weaving for spinal mobility.'
FROM Exercise WHERE ExerciseKey = 'weaving';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionPM, ExerciseId, 3, 2, NULL, 2, 'Controlled hind limb strengthening from down position.'
FROM Exercise WHERE ExerciseKey = 'layToStand';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionPM, ExerciseId, 3, 2, NULL, 3, 'Promote symmetrical gait and steady loading.'
FROM Exercise WHERE ExerciseKey = 'slowLeadWalk';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionPM, ExerciseId, 3, 2, NULL, 4, 'Encourage controlled circular movement patterns.'
FROM Exercise WHERE ExerciseKey = 'circles';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionPM, ExerciseId, 3, 2, NULL, 5, 'Forelimb control and gentle elbow flexion.'
FROM Exercise WHERE ExerciseKey = 'givePaw';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionPM, ExerciseId, 3, 2, NULL, 6, 'Repeat strengthening at end of PM session.'
FROM Exercise WHERE ExerciseKey = 'raisedSitStands';

COMMIT;

-- ---------------------------------------------------------------------------
-- VERIFICATION QUERY
-- Run this to confirm the seed completed correctly.
-- ---------------------------------------------------------------------------
SELECT
    o.OwnerId,
    CONCAT(o.FirstName, ' ', o.LastName)   AS OwnerName,
    p.PetId,
    p.Name                                  AS PetName,
    p.Breed,
    tc.TreatmentCaseId,
    tc.CaseTitle,
    tc.Status                               AS CaseStatus,
    prog.ProgrammeId,
    prog.ProgrammeName,
    prog.Status                             AS ProgrammeStatus,
    COUNT(se.SessionExerciseId)             AS TotalSessionExercises
FROM Owner o
JOIN Pet p               ON p.OwnerId         = o.OwnerId
JOIN TreatmentCase tc    ON tc.PetId          = p.PetId
JOIN Programme prog      ON prog.TreatmentCaseId = tc.TreatmentCaseId
JOIN Session s           ON s.ProgrammeId     = prog.ProgrammeId
JOIN SessionExercise se  ON se.SessionId      = s.SessionId
WHERE o.Email = 'emma.thompson@example.com'
GROUP BY
    o.OwnerId, o.FirstName, o.LastName,
    p.PetId, p.Name, p.Breed,
    tc.TreatmentCaseId, tc.CaseTitle, tc.Status,
    prog.ProgrammeId, prog.ProgrammeName, prog.Status;
