USE canine_physiotherapy;

START TRANSACTION;

-- USER JOURNEY 5: END OF PROGRAMME REVIEW AND NEW PROGRAMME

SELECT
    p.ProgrammeId,
    p.TreatmentCaseId,
    tc.PractitionerId,
    tc.PetId
INTO
    @PreviousProgrammeId,
    @TreatmentCaseId,
    @PractitionerId,
    @PetId
FROM Programme p
JOIN TreatmentCase tc
  ON tc.TreatmentCaseId = p.TreatmentCaseId
WHERE p.IsCurrent = TRUE
ORDER BY p.ProgrammeId DESC
LIMIT 1;

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
    'follow_up_review',
    'End-of-programme review completed. Buddy has shown clear improvement over the initial two-week programme. Early sessions were affected by pain, hesitation, and a small number of skipped exercises, but tolerance improved steadily. Pain scores reduced across the programme, participation became more consistent, and movement quality appeared smoother. Recommend progression to a second home programme with slightly increased loading and continued mobility and balance work.',
    TRUE
);

SET @TreatmentCaseNoteId = LAST_INSERT_ID();

UPDATE Programme
SET
    Status = 'completed',
    IsCurrent = FALSE,
    UpdatedDate = CURRENT_TIMESTAMP,
    Notes = CONCAT(
        COALESCE(Notes, ''),
        CASE
            WHEN Notes IS NULL OR TRIM(Notes) = '' THEN ''
            ELSE '
'
        END,
        'Programme completed following follow-up review on ',
        DATE_FORMAT(CURRENT_DATE, '%Y-%m-%d'),
        '. Progression to Programme 2 created.'
    )
WHERE ProgrammeId = @PreviousProgrammeId;

INSERT INTO Programme (
    TreatmentCaseId,
    ProgrammeTemplateId,
    ProgrammeName,
    StartDate,
    EndDate,
    Status,
    IsCurrent,
    Notes
) VALUES (
    @TreatmentCaseId,
    NULL,
    'Buddy Progressed Home Rehabilitation Programme',
    CURRENT_DATE,
    DATE_ADD(CURRENT_DATE, INTERVAL 14 DAY),
    'active',
    TRUE,
    'Second programme created after end-of-programme review. Progressed loading and continued mobility/balance work.'
);

SET @NewProgrammeId = LAST_INSERT_ID();

SELECT SessionContentTypeId INTO @SessionContentTypeId_AM
FROM SessionContentType
WHERE ContentKey = 'hindlimbCore'
LIMIT 1;

SELECT SessionContentTypeId INTO @SessionContentTypeId_PM
FROM SessionContentType
WHERE ContentKey = 'mobilityFlexibility'
LIMIT 1;

INSERT INTO Session (
    ProgrammeId,
    SessionContentTypeId,
    CreatedDate,
    Period,
    Objective,
    Status,
    SortOrder
) VALUES (
    @NewProgrammeId,
    @SessionContentTypeId_AM,
    CURRENT_TIMESTAMP,
    'AM',
    'Progressed morning strengthening and core control session with improved tolerance expected.',
    'active',
    1
);

SET @NewSessionId_AM = LAST_INSERT_ID();

INSERT INTO Session (
    ProgrammeId,
    SessionContentTypeId,
    CreatedDate,
    Period,
    Objective,
    Status,
    SortOrder
) VALUES (
    @NewProgrammeId,
    @SessionContentTypeId_PM,
    CURRENT_TIMESTAMP,
    'PM',
    'Progressed evening mobility, balance and controlled movement session.',
    'active',
    2
);

SET @NewSessionId_PM = LAST_INSERT_ID();

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @NewSessionId_AM, e.ExerciseId, 6, 3, NULL, 1,
'Progressed from 5 reps to 6 reps. Buddy tolerated forelimb loading better by end of Programme 1.'
FROM Exercise e WHERE e.ExerciseKey = 'stepUp';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @NewSessionId_AM, e.ExerciseId, 6, 2, NULL, 2,
'Progressed from 5 reps to 6 reps while keeping sets stable.'
FROM Exercise e WHERE e.ExerciseKey = 'raisedSitStands';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @NewSessionId_AM, e.ExerciseId, 3, 1, 10, 3,
'Maintain rep count but progress the hold duration from 7 seconds to 10 seconds.'
FROM Exercise e WHERE e.ExerciseKey = 'baitedBackStretch';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @NewSessionId_AM, e.ExerciseId, 6, 1, NULL, 4,
'Progressed from 5 passes to 6 passes with continued controlled pole walking.'
FROM Exercise e WHERE e.ExerciseKey = 'raisedPoles';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @NewSessionId_PM, e.ExerciseId, 4, 2, NULL, 1,
'Progressed from 3 reps to 4 reps as confidence improved.'
FROM Exercise e WHERE e.ExerciseKey = 'weaving';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @NewSessionId_PM, e.ExerciseId, 4, 2, NULL, 2,
'Progressed circular movement work from 3 to 4 reps.'
FROM Exercise e WHERE e.ExerciseKey = 'circles';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @NewSessionId_PM, e.ExerciseId, 4, 2, NULL, 3,
'Slight progression in total work while retaining controlled pace and form.'
FROM Exercise e WHERE e.ExerciseKey = 'slowLeadWalk';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @NewSessionId_PM, e.ExerciseId, 4, 2, NULL, 4,
'Progressed from 3 reps to 4 reps with ongoing hind limb strengthening.'
FROM Exercise e WHERE e.ExerciseKey = 'layToStand';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @NewSessionId_PM, e.ExerciseId, 4, 2, NULL, 5,
'Slight increase in forelimb control work.'
FROM Exercise e WHERE e.ExerciseKey = 'givePaw';

INSERT INTO SessionExercise (SessionId, ExerciseId, Reps, Sets, HoldSeconds, SortOrder, Notes)
SELECT @NewSessionId_PM, e.ExerciseId, 4, 2, NULL, 6,
'Retained as a second strengthening touchpoint in the PM session.'
FROM Exercise e WHERE e.ExerciseKey = 'raisedSitStands';

COMMIT;

SELECT
    tcn.TreatmentCaseNoteId,
    tcn.TreatmentCaseId,
    tcn.PractitionerId,
    tcn.CreatedDate,
    tcn.NoteType,
    tcn.NoteText
FROM TreatmentCaseNote tcn
WHERE tcn.TreatmentCaseNoteId = @TreatmentCaseNoteId;

SELECT
    p.ProgrammeId,
    p.ProgrammeName,
    p.Status,
    p.IsCurrent,
    p.StartDate,
    p.EndDate,
    p.CurrentProgrammeVersionId
FROM Programme p
WHERE p.ProgrammeId IN (@PreviousProgrammeId, @NewProgrammeId)
ORDER BY p.ProgrammeId;

SELECT
    s.SessionId,
    s.ProgrammeId,
    s.Period,
    s.Objective,
    s.Status,
    s.SortOrder,
    sct.ContentKey,
    sct.DisplayName
FROM Session s
LEFT JOIN SessionContentType sct
  ON sct.SessionContentTypeId = s.SessionContentTypeId
WHERE s.ProgrammeId = @NewProgrammeId
ORDER BY s.SortOrder, s.SessionId;

SELECT
    s.Period,
    se.SortOrder,
    e.ExerciseKey,
    e.Title,
    se.Reps,
    se.Sets,
    se.HoldSeconds,
    se.Notes
FROM SessionExercise se
JOIN Session s
  ON s.SessionId = se.SessionId
JOIN Exercise e
  ON e.ExerciseId = se.ExerciseId
WHERE s.ProgrammeId = @NewProgrammeId
ORDER BY s.SortOrder, se.SortOrder, se.SessionExerciseId;
