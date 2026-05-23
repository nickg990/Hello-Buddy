USE canine_physiotherapy;

START TRANSACTION;

-- ============================================================
-- USER JOURNEY 3: FIRST PROGRAMME CREATION
-- Purpose:
--   Create the first programme after the initial review:
--   1. Link the programme to the existing treatment case
--   2. Create AM and PM sessions
--   3. Add ordered exercises to each session
--
-- Notes:
--   - This script does NOT publish JSON.
--   - Run the generate/publish script afterwards.
--   - The exercise order is deliberately different from the
--     original PhysioContent.json so you can verify the payload
--     is coming from database data.
-- ============================================================

-- ------------------------------------------------------------
-- 1. LOOK UP EXISTING TREATMENT CASE
-- Assumes Journey 1 and Journey 2 have already been run.
-- ------------------------------------------------------------
SELECT tc.TreatmentCaseId
INTO @TreatmentCaseId
FROM TreatmentCase tc
JOIN Pet pet
  ON pet.PetId = tc.PetId
JOIN Owner o
  ON o.OwnerId = pet.OwnerId
WHERE o.Email = 'emma.thompson@example.com'
  AND pet.Name = 'Buddy'
  AND tc.Status = 'active'
ORDER BY tc.StartDate DESC, tc.TreatmentCaseId DESC
LIMIT 1;

-- Diagnostic check
SELECT @TreatmentCaseId AS TreatmentCaseId;

-- ------------------------------------------------------------
-- 2. CREATE PROGRAMME HEADER
-- ------------------------------------------------------------
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
    'Buddy Initial Home Rehabilitation Programme',
    CURRENT_DATE,
    DATE_ADD(CURRENT_DATE, INTERVAL 14 DAY),
    'active',
    TRUE,
    'Initial home exercise programme created following first physiotherapy review.'
);

SET @ProgrammeId = LAST_INSERT_ID();

-- ------------------------------------------------------------
-- 3. CREATE AM SESSION
-- ------------------------------------------------------------
INSERT INTO Session (
    ProgrammeId,
    CreatedDate,
    Period,
    Objective,
    Status,
    SortOrder
) VALUES (
    @ProgrammeId,
    CURRENT_TIMESTAMP,
    'AM',
    'Morning strengthening and core control session.',
    'active',
    1
);

SET @SessionId_AM = LAST_INSERT_ID();

-- ------------------------------------------------------------
-- 4. CREATE PM SESSION
-- ------------------------------------------------------------
INSERT INTO Session (
    ProgrammeId,
    CreatedDate,
    Period,
    Objective,
    Status,
    SortOrder
) VALUES (
    @ProgrammeId,
    CURRENT_TIMESTAMP,
    'PM',
    'Evening mobility, balance and controlled movement session.',
    'active',
    2
);

SET @SessionId_PM = LAST_INSERT_ID();

-- ------------------------------------------------------------
-- 5. ADD EXERCISES TO AM SESSION
-- Reordered deliberately.
-- ------------------------------------------------------------

-- Step up (moved to AM position 1)
INSERT INTO SessionExercise (
    SessionId,
    ExerciseId,
    Reps,
    Sets,
    HoldSeconds,
    SortOrder,
    Notes
)
SELECT
    @SessionId_AM,
    e.ExerciseId,
    5,
    3,
    NULL,
    1,
    'Lead AM session with controlled forelimb loading.'
FROM Exercise e
WHERE e.ExerciseKey = 'stepUp';

-- Baited back stretch (AM position 2)
INSERT INTO SessionExercise (
    SessionId,
    ExerciseId,
    Reps,
    Sets,
    HoldSeconds,
    SortOrder,
    Notes
)
SELECT
    @SessionId_AM,
    e.ExerciseId,
    3,
    1,
    7,
    2,
    'Gentle spinal mobility exercise.'
FROM Exercise e
WHERE e.ExerciseKey = 'baitedBackStretch';

-- Raised sit stands (AM position 3)
INSERT INTO SessionExercise (
    SessionId,
    ExerciseId,
    Reps,
    Sets,
    HoldSeconds,
    SortOrder,
    Notes
)
SELECT
    @SessionId_AM,
    e.ExerciseId,
    5,
    2,
    NULL,
    3,
    'Controlled sit-to-stand strengthening.'
FROM Exercise e
WHERE e.ExerciseKey = 'raisedSitStands';

-- Raised poles (AM position 4)
INSERT INTO SessionExercise (
    SessionId,
    ExerciseId,
    Reps,
    Sets,
    HoldSeconds,
    SortOrder,
    Notes
)
SELECT
    @SessionId_AM,
    e.ExerciseId,
    5,
    1,
    NULL,
    4,
    'Finish AM session with controlled pole walking.'
FROM Exercise e
WHERE e.ExerciseKey = 'raisedPoles';

-- ------------------------------------------------------------
-- 6. ADD EXERCISES TO PM SESSION
-- Reordered deliberately.
-- ------------------------------------------------------------

-- Weaving (PM position 1)
INSERT INTO SessionExercise (
    SessionId,
    ExerciseId,
    Reps,
    Sets,
    HoldSeconds,
    SortOrder,
    Notes
)
SELECT
    @SessionId_PM,
    e.ExerciseId,
    3,
    2,
    NULL,
    1,
    'Start PM with slow weaving for spinal mobility.'
FROM Exercise e
WHERE e.ExerciseKey = 'weaving';

-- Lay to stand (PM position 2)
INSERT INTO SessionExercise (
    SessionId,
    ExerciseId,
    Reps,
    Sets,
    HoldSeconds,
    SortOrder,
    Notes
)
SELECT
    @SessionId_PM,
    e.ExerciseId,
    3,
    2,
    NULL,
    2,
    'Controlled hind limb strengthening from down position.'
FROM Exercise e
WHERE e.ExerciseKey = 'layToStand';

-- Slow lead walk (PM position 3)
INSERT INTO SessionExercise (
    SessionId,
    ExerciseId,
    Reps,
    Sets,
    HoldSeconds,
    SortOrder,
    Notes
)
SELECT
    @SessionId_PM,
    e.ExerciseId,
    3,
    2,
    NULL,
    3,
    'Promote symmetrical gait and steady loading.'
FROM Exercise e
WHERE e.ExerciseKey = 'slowLeadWalk';

-- Circles (PM position 4)
INSERT INTO SessionExercise (
    SessionId,
    ExerciseId,
    Reps,
    Sets,
    HoldSeconds,
    SortOrder,
    Notes
)
SELECT
    @SessionId_PM,
    e.ExerciseId,
    3,
    2,
    NULL,
    4,
    'Encourage controlled circular movement patterns.'
FROM Exercise e
WHERE e.ExerciseKey = 'circles';

-- Give paw (PM position 5)
INSERT INTO SessionExercise (
    SessionId,
    ExerciseId,
    Reps,
    Sets,
    HoldSeconds,
    SortOrder,
    Notes
)
SELECT
    @SessionId_PM,
    e.ExerciseId,
    3,
    2,
    NULL,
    5,
    'Forelimb control and gentle elbow flexion.'
FROM Exercise e
WHERE e.ExerciseKey = 'givePaw';

-- Raised sit stands (PM position 6)
INSERT INTO SessionExercise (
    SessionId,
    ExerciseId,
    Reps,
    Sets,
    HoldSeconds,
    SortOrder,
    Notes
)
SELECT
    @SessionId_PM,
    e.ExerciseId,
    3,
    2,
    NULL,
    6,
    'Repeat strengthening at end of PM session.'
FROM Exercise e
WHERE e.ExerciseKey = 'raisedSitStands';

COMMIT;

-- ============================================================
-- VERIFICATION REPORTS
-- ============================================================

-- Programme header
SELECT
    p.ProgrammeId,
    p.ProgrammeName,
    p.TreatmentCaseId,
    p.StartDate,
    p.EndDate,
    p.Status,
    p.IsCurrent,
    p.CurrentProgrammeVersionId
FROM Programme p
WHERE p.ProgrammeId = @ProgrammeId;

-- Sessions
SELECT
    s.SessionId,
    s.ProgrammeId,
    s.Period,
    s.Objective,
    s.Status,
    s.SortOrder
FROM Session s
WHERE s.ProgrammeId = @ProgrammeId
ORDER BY s.SortOrder, s.SessionId;

-- Session exercises in order
SELECT
    s.Period,
    se.SortOrder,
    e.ExerciseKey,
    e.Title,
    COALESCE(se.Reps, e.DefaultReps) AS Reps,
    COALESCE(se.Sets, e.DefaultSets) AS Sets,
    COALESCE(se.HoldSeconds, e.DefaultHoldSeconds) AS HoldSeconds,
    se.Notes
FROM SessionExercise se
JOIN Session s
  ON s.SessionId = se.SessionId
JOIN Exercise e
  ON e.ExerciseId = se.ExerciseId
WHERE s.ProgrammeId = @ProgrammeId
ORDER BY s.SortOrder, se.SortOrder, se.SessionExerciseId;
