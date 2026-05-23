USE canine_physiotherapy;

START TRANSACTION;

SELECT OwnerId INTO @OwnerId FROM Owner WHERE Email='emma.thompson@example.com' LIMIT 1;
SELECT PetId INTO @PetId FROM Pet WHERE OwnerId=@OwnerId AND Name='Buddy' LIMIT 1;
SELECT PractitionerId INTO @PractitionerId FROM Practitioner WHERE Email='amelia.carter@caninephysio.local' LIMIT 1;

INSERT INTO Practitioner_Pet (PractitionerId, PetId, AssignedFrom, AssignedTo, IsPrimary, Status, ReferralSource, Notes)
VALUES (@PractitionerId, @PetId, CURRENT_TIMESTAMP, NULL, TRUE, 'active', 'Initial consultation booking', 'Primary practitioner assigned following initial physiotherapy assessment.');

INSERT INTO TreatmentCase (PetId, PractitionerId, CaseTitle, ClinicalSummary, StartDate, EndDate, Status)
VALUES (@PetId, @PractitionerId, 'Buddy hind limb rehabilitation', 'Conservative physiotherapy management for mild hind limb weakness and reduced hip mobility following initial practitioner assessment.', CURRENT_DATE, NULL, 'active');

SET @TreatmentCaseId = LAST_INSERT_ID();

INSERT INTO TreatmentCaseNote (TreatmentCaseId, PractitionerId, CreatedDate, NoteType, NoteText, IsActive)
VALUES (@TreatmentCaseId, @PractitionerId, CURRENT_TIMESTAMP, 'initial_assessment', 'Initial assessment completed. Mild hind limb weakness observed, reduced hip extension on the left side, and slight reluctance during sit-to-stand transition. Home exercise programme recommended with controlled strengthening and mobility work.', TRUE);

INSERT INTO RegistrationCode (PractitionerId, PetId, Code, IssuedDate, ExpiryDate, UsedDate, Status, Notes)
VALUES (@PractitionerId, @PetId, CONCAT('REG-', DATE_FORMAT(NOW(), '%Y%m%d'), '-', LPAD(@PetId, 4, '0')), CURRENT_TIMESTAMP, DATE_ADD(CURRENT_TIMESTAMP, INTERVAL 14 DAY), NULL, 'issued', 'Registration code issued to owner following initial review.');

COMMIT;
