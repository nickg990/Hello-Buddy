USE canine_physiotherapy;

START TRANSACTION;

SELECT tc.TreatmentCaseId INTO @TreatmentCaseId
FROM TreatmentCase tc
JOIN Pet pet ON pet.PetId = tc.PetId
JOIN Owner o ON o.OwnerId = pet.OwnerId
WHERE o.Email='emma.thompson@example.com' AND pet.Name='Buddy' AND tc.Status='active'
ORDER BY tc.StartDate DESC, tc.TreatmentCaseId DESC
LIMIT 1;

INSERT INTO Programme (TreatmentCaseId, ProgrammeTemplateId, ProgrammeName, StartDate, EndDate, Status, IsCurrent, Notes)
VALUES (@TreatmentCaseId, NULL, 'Buddy Initial Home Rehabilitation Programme', CURRENT_DATE, DATE_ADD(CURRENT_DATE, INTERVAL 14 DAY), 'active', TRUE, 'Initial home exercise programme created following first physiotherapy review.');

SET @ProgrammeId = LAST_INSERT_ID();
SELECT SessionContentTypeId INTO @SessionContentTypeId_AM FROM SessionContentType WHERE ContentKey='hindlimbCore' LIMIT 1;
SELECT SessionContentTypeId INTO @SessionContentTypeId_PM FROM SessionContentType WHERE ContentKey='mobilityFlexibility' LIMIT 1;

INSERT INTO Session (ProgrammeId, SessionContentTypeId, CreatedDate, Period, Objective, Status, SortOrder)
VALUES (@ProgrammeId, @SessionContentTypeId_AM, CURRENT_TIMESTAMP, 'AM', 'Morning strengthening and core control session.', 'active', 1);
SET @SessionId_AM = LAST_INSERT_ID();

INSERT INTO Session (ProgrammeId, SessionContentTypeId, CreatedDate, Period, Objective, Status, SortOrder)
VALUES (@ProgrammeId, @SessionContentTypeId_PM, CURRENT_TIMESTAMP, 'PM', 'Evening mobility, balance and controlled movement session.', 'active', 2);
SET @SessionId_PM = LAST_INSERT_ID();

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionId_AM, e.ExerciseId, 5, 3, NULL, 1, 'Lead AM session with controlled forelimb loading.' FROM Exercise e WHERE e.ExerciseKey='stepUp';
INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionId_AM, e.ExerciseId, 3, 1, 7, 2, 'Gentle spinal mobility exercise.' FROM Exercise e WHERE e.ExerciseKey='baitedBackStretch';
INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionId_AM, e.ExerciseId, 5, 2, NULL, 3, 'Controlled sit-to-stand strengthening.' FROM Exercise e WHERE e.ExerciseKey='raisedSitStands';
INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionId_AM, e.ExerciseId, 5, 1, NULL, 4, 'Finish AM session with controlled pole walking.' FROM Exercise e WHERE e.ExerciseKey='raisedPoles';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionId_PM, e.ExerciseId, 3, 2, NULL, 1, 'Start PM with slow weaving for spinal mobility.' FROM Exercise e WHERE e.ExerciseKey='weaving';
INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionId_PM, e.ExerciseId, 3, 2, NULL, 2, 'Controlled hind limb strengthening from down position.' FROM Exercise e WHERE e.ExerciseKey='layToStand';
INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionId_PM, e.ExerciseId, 3, 2, NULL, 3, 'Promote symmetrical gait and steady loading.' FROM Exercise e WHERE e.ExerciseKey='slowLeadWalk';
INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionId_PM, e.ExerciseId, 3, 2, NULL, 4, 'Encourage controlled circular movement patterns.' FROM Exercise e WHERE e.ExerciseKey='circles';
INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionId_PM, e.ExerciseId, 3, 2, NULL, 5, 'Forelimb control and gentle elbow flexion.' FROM Exercise e WHERE e.ExerciseKey='givePaw';
INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @SessionId_PM, e.ExerciseId, 3, 2, NULL, 6, 'Repeat strengthening at end of PM session.' FROM Exercise e WHERE e.ExerciseKey='raisedSitStands';

COMMIT;
