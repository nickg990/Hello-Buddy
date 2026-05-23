USE canine_physiotherapy;

START TRANSACTION;

-- ============================================================
-- USER JOURNEY 7: ONE WEEK OF FULL ADHERENCE ON NEW PROGRAMME
-- Purpose:
--   Simulate 7 days of successful use on Programme 2:
--   - AM and PM sessions completed every day
--   - all prescribed reps and sets completed
--   - negligible pain scores
--   - no skipped sessions
--   - a few positive progress notes
--
-- Assumptions:
--   - Programme 2 is the current active programme
--   - its published ProgrammeVersion already exists
-- ============================================================

-- ------------------------------------------------------------
-- 1. LOOK UP CURRENT PROGRAMME 2 CONTEXT
-- ------------------------------------------------------------
SELECT
    p.ProgrammeId,
    p.CurrentProgrammeVersionId,
    p.StartDate,
    tc.PetId
INTO
    @ProgrammeId,
    @ProgrammeVersionId,
    @ProgrammeStartDate,
    @PetId
FROM Programme p
JOIN TreatmentCase tc
  ON tc.TreatmentCaseId = p.TreatmentCaseId
WHERE p.IsCurrent = TRUE
ORDER BY p.ProgrammeId DESC
LIMIT 1;

SELECT SessionId
INTO @SessionId_AM
FROM Session
WHERE ProgrammeId = @ProgrammeId
  AND Period = 'AM'
LIMIT 1;

SELECT SessionId
INTO @SessionId_PM
FROM Session
WHERE ProgrammeId = @ProgrammeId
  AND Period = 'PM'
LIMIT 1;

SELECT
    @ProgrammeId AS ProgrammeId,
    @ProgrammeVersionId AS ProgrammeVersionId,
    @ProgrammeStartDate AS ProgrammeStartDate,
    @PetId AS PetId,
    @SessionId_AM AS SessionId_AM,
    @SessionId_PM AS SessionId_PM;

-- ------------------------------------------------------------
-- 2. REMOVE ANY PRIOR TEST DATA FOR FIRST 7 DAYS OF PROGRAMME 2
-- ------------------------------------------------------------
DELETE FROM ExerciseCompletion
WHERE SessionOccurrenceId IN (
    SELECT SessionOccurrenceId
    FROM (
        SELECT so.SessionOccurrenceId
        FROM SessionOccurrence so
        WHERE so.ProgrammeVersionId = @ProgrammeVersionId
          AND so.PetId = @PetId
          AND so.ScheduledDate BETWEEN @ProgrammeStartDate
                                   AND DATE_ADD(@ProgrammeStartDate, INTERVAL 6 DAY)
    ) x
);

DELETE FROM SessionSkip
WHERE SessionOccurrenceId IN (
    SELECT SessionOccurrenceId
    FROM (
        SELECT so.SessionOccurrenceId
        FROM SessionOccurrence so
        WHERE so.ProgrammeVersionId = @ProgrammeVersionId
          AND so.PetId = @PetId
          AND so.ScheduledDate BETWEEN @ProgrammeStartDate
                                   AND DATE_ADD(@ProgrammeStartDate, INTERVAL 6 DAY)
    ) x
);

DELETE FROM SessionOccurrence
WHERE SessionOccurrenceId IN (
    SELECT SessionOccurrenceId
    FROM (
        SELECT so.SessionOccurrenceId
        FROM SessionOccurrence so
        WHERE so.ProgrammeVersionId = @ProgrammeVersionId
          AND so.PetId = @PetId
          AND so.ScheduledDate BETWEEN @ProgrammeStartDate
                                   AND DATE_ADD(@ProgrammeStartDate, INTERVAL 6 DAY)
    ) x
);

-- ------------------------------------------------------------
-- 3. INSERT 7 DAYS OF AM/PM SESSION OCCURRENCES
--    All sessions completed successfully.
-- ------------------------------------------------------------
INSERT INTO SessionOccurrence (
    ProgrammeVersionId,
    SessionId,
    PetId,
    ScheduledDate,
    Period,
    Status,
    DeviceRecordedDateTime,
    StartedDateTime,
    CompletedDateTime,
    SkippedDateTime,
    SyncedDateTime,
    Comments
)
WITH RECURSIVE seq AS (
    SELECT 0 AS day_index
    UNION ALL
    SELECT day_index + 1
    FROM seq
    WHERE day_index < 6
),
session_map AS (
    SELECT @SessionId_AM AS SessionId, 'AM' AS Period, 1 AS PeriodSort
    UNION ALL
    SELECT @SessionId_PM AS SessionId, 'PM' AS Period, 2 AS PeriodSort
)
SELECT
    @ProgrammeVersionId,
    sm.SessionId,
    @PetId,
    DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY) AS ScheduledDate,
    sm.Period,
    'completed' AS Status,
    CASE
        WHEN sm.Period = 'AM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '08:05:00')
        ELSE TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '18:05:00')
    END AS DeviceRecordedDateTime,
    CASE
        WHEN sm.Period = 'AM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '08:05:00')
        ELSE TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '18:05:00')
    END AS StartedDateTime,
    CASE
        WHEN sm.Period = 'AM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '08:26:00')
        ELSE TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '18:28:00')
    END AS CompletedDateTime,
    NULL AS SkippedDateTime,
    CASE
        WHEN sm.Period = 'AM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '20:10:00')
        ELSE TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '20:15:00')
    END AS SyncedDateTime,
    CASE
        WHEN s.day_index = 0 AND sm.Period = 'AM' THEN 'Buddy settled into the progressed programme well and completed everything comfortably.'
        WHEN s.day_index = 2 AND sm.Period = 'PM' THEN 'All exercises completed with good control and minimal hesitation.'
        WHEN s.day_index = 4 AND sm.Period = 'AM' THEN 'Movement looked confident and pain remained negligible.'
        WHEN s.day_index = 6 AND sm.Period = 'PM' THEN 'Completed the full first week strongly with excellent adherence.'
        ELSE NULL
    END AS Comments
FROM seq s
CROSS JOIN session_map sm
ORDER BY ScheduledDate, sm.PeriodSort;

-- ------------------------------------------------------------
-- 4. INSERT EXERCISE COMPLETIONS
--    All prescribed work completed; negligible pain.
-- ------------------------------------------------------------
INSERT INTO ExerciseCompletion (
    SessionOccurrenceId,
    SessionExerciseId,
    ExerciseKeySnapshot,
    ExerciseTitleSnapshot,
    CompletedDateTime,
    DeviceRecordedDateTime,
    SyncedDateTime,
    RepsCompleted,
    SetsCompleted,
    PainScore,
    Comments,
    CompletionStatus
)
SELECT
    so.SessionOccurrenceId,
    se.SessionExerciseId,
    e.ExerciseKey,
    e.Title,
    so.CompletedDateTime,
    so.DeviceRecordedDateTime,
    so.SyncedDateTime,
    COALESCE(se.Reps, e.DefaultReps),
    COALESCE(se.Sets, e.DefaultSets),
    CASE
        WHEN DATEDIFF(so.ScheduledDate, @ProgrammeStartDate) IN (0,1) THEN 2
        ELSE 1
    END AS PainScore,
    CASE
        WHEN DATEDIFF(so.ScheduledDate, @ProgrammeStartDate) = 0 AND se.SortOrder = 1
            THEN 'Progressed exercise completed fully with only very mild stiffness.'
        WHEN DATEDIFF(so.ScheduledDate, @ProgrammeStartDate) = 3 AND se.SortOrder = 1
            THEN 'Completed smoothly with no obvious discomfort.'
        WHEN DATEDIFF(so.ScheduledDate, @ProgrammeStartDate) = 6 AND se.SortOrder = 1
            THEN 'Very comfortable by the end of week one on the progressed programme.'
        ELSE NULL
    END AS Comments,
    'completed' AS CompletionStatus
FROM SessionOccurrence so
JOIN SessionExercise se
  ON se.SessionId = so.SessionId
JOIN Exercise e
  ON e.ExerciseId = se.ExerciseId
WHERE so.ProgrammeVersionId = @ProgrammeVersionId
  AND so.PetId = @PetId
  AND so.ScheduledDate BETWEEN @ProgrammeStartDate
                           AND DATE_ADD(@ProgrammeStartDate, INTERVAL 6 DAY)
ORDER BY so.ScheduledDate, so.SessionId, se.SortOrder, se.SessionExerciseId;

COMMIT;

-- ============================================================
-- 5. VERIFICATION
-- ============================================================

-- A. Session occurrence summary for the week
SELECT
    so.SessionOccurrenceId,
    so.ScheduledDate,
    so.Period,
    so.Status,
    so.Comments,
    so.DeviceRecordedDateTime,
    so.SyncedDateTime
FROM SessionOccurrence so
WHERE so.ProgrammeVersionId = @ProgrammeVersionId
  AND so.PetId = @PetId
  AND so.ScheduledDate BETWEEN @ProgrammeStartDate
                           AND DATE_ADD(@ProgrammeStartDate, INTERVAL 6 DAY)
ORDER BY so.ScheduledDate, FIELD(so.Period, 'AM', 'PM', 'single');

-- B. Check no skips exist for this week
SELECT
    COUNT(*) AS SessionSkipCount
FROM SessionSkip ss
JOIN SessionOccurrence so
  ON so.SessionOccurrenceId = ss.SessionOccurrenceId
WHERE so.ProgrammeVersionId = @ProgrammeVersionId
  AND so.PetId = @PetId
  AND so.ScheduledDate BETWEEN @ProgrammeStartDate
                           AND DATE_ADD(@ProgrammeStartDate, INTERVAL 6 DAY);

-- C. Pain / adherence summary by day
SELECT
    so.ScheduledDate,
    MIN(ec.PainScore) AS MinPainScore,
    MAX(ec.PainScore) AS MaxPainScore,
    COUNT(*) AS ExerciseRows,
    SUM(CASE WHEN ec.CompletionStatus = 'completed' THEN 1 ELSE 0 END) AS ExercisesCompleted,
    SUM(CASE WHEN ec.CompletionStatus <> 'completed' THEN 1 ELSE 0 END) AS ExercisesNotCompleted
FROM ExerciseCompletion ec
JOIN SessionOccurrence so
  ON so.SessionOccurrenceId = ec.SessionOccurrenceId
WHERE so.ProgrammeVersionId = @ProgrammeVersionId
  AND so.PetId = @PetId
  AND so.ScheduledDate BETWEEN @ProgrammeStartDate
                           AND DATE_ADD(@ProgrammeStartDate, INTERVAL 6 DAY)
GROUP BY so.ScheduledDate
ORDER BY so.ScheduledDate;

-- D. Detailed exercise rows
SELECT
    so.ScheduledDate,
    so.Period,
    ec.ExerciseKeySnapshot,
    ec.RepsCompleted,
    ec.SetsCompleted,
    ec.PainScore,
    ec.CompletionStatus,
    ec.Comments
FROM ExerciseCompletion ec
JOIN SessionOccurrence so
  ON so.SessionOccurrenceId = ec.SessionOccurrenceId
WHERE so.ProgrammeVersionId = @ProgrammeVersionId
  AND so.PetId = @PetId
  AND so.ScheduledDate BETWEEN @ProgrammeStartDate
                           AND DATE_ADD(@ProgrammeStartDate, INTERVAL 6 DAY)
ORDER BY so.ScheduledDate, FIELD(so.Period, 'AM', 'PM', 'single'), ec.ExerciseKeySnapshot;
