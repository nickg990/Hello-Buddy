USE canine_physiotherapy;

START TRANSACTION;

SET @OLD_SQL_SAFE_UPDATES = @@SQL_SAFE_UPDATES;
SET SQL_SAFE_UPDATES = 0;

-- ============================================================
-- USER JOURNEY 4: MOBILE DOWNLOAD, COMPLETION, SKIP, AND SYNC
-- V3
--
-- Design notes:
-- 1. Assumes Journey 1, 2, 3 and Generate/Publish have run.
-- 2. Assumes Programme.CurrentProgrammeVersionId is populated.
-- 3. Safe-update-friendly: no joined deletes.
-- 4. Simulates 14 days of AM/PM usage for the current programme.
-- 5. Includes improving pain scores, early partial completions,
--    and three skipped sessions with reasons and comments.
-- ============================================================

-- ------------------------------------------------------------
-- 1. LOOK UP CURRENT PROGRAMME CONTEXT
-- ------------------------------------------------------------
SELECT
    p.ProgrammeId,
    p.TreatmentCaseId,
    p.CurrentProgrammeVersionId,
    tc.PetId,
    p.StartDate
INTO
    @ProgrammeId,
    @TreatmentCaseId,
    @ProgrammeVersionId,
    @PetId,
    @ProgrammeStartDate
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

-- Diagnostics
SELECT
    @ProgrammeId AS ProgrammeId,
    @ProgrammeVersionId AS ProgrammeVersionId,
    @PetId AS PetId,
    @ProgrammeStartDate AS ProgrammeStartDate,
    @SessionId_AM AS SessionId_AM,
    @SessionId_PM AS SessionId_PM;

-- ------------------------------------------------------------
-- 2. GUARD CHECKS
-- ------------------------------------------------------------
-- If any of these are null, stop and fix the earlier journey first.
SELECT
    CASE WHEN @ProgrammeId IS NULL THEN 'MISSING PROGRAMME' ELSE 'OK' END AS ProgrammeCheck,
    CASE WHEN @ProgrammeVersionId IS NULL THEN 'MISSING PROGRAMME VERSION' ELSE 'OK' END AS ProgrammeVersionCheck,
    CASE WHEN @PetId IS NULL THEN 'MISSING PET' ELSE 'OK' END AS PetCheck,
    CASE WHEN @SessionId_AM IS NULL THEN 'MISSING AM SESSION' ELSE 'OK' END AS AMSessionCheck,
    CASE WHEN @SessionId_PM IS NULL THEN 'MISSING PM SESSION' ELSE 'OK' END AS PMSessionCheck;

-- ------------------------------------------------------------
-- 3. ENSURE VETERINARY VISIT SKIP REASON EXISTS
-- ------------------------------------------------------------
INSERT INTO SessionSkipReason (
    ReasonName,
    Description,
    IsActive
)
SELECT
    'Veterinary visit',
    'The scheduled physiotherapy session was skipped because the pet had a veterinary appointment or follow-up visit.',
    TRUE
WHERE NOT EXISTS (
    SELECT 1
    FROM SessionSkipReason
    WHERE ReasonName = 'Veterinary visit'
);

SELECT SessionSkipReasonId
INTO @ReasonId_PainFlare
FROM SessionSkipReason
WHERE ReasonName = 'Pain flare-up'
LIMIT 1;

SELECT SessionSkipReasonId
INTO @ReasonId_PetReluctance
FROM SessionSkipReason
WHERE ReasonName = 'Pet non-compliant'
LIMIT 1;

SELECT SessionSkipReasonId
INTO @ReasonId_VetVisit
FROM SessionSkipReason
WHERE ReasonName = 'Veterinary visit'
LIMIT 1;

-- ------------------------------------------------------------
-- 4. CLEAN DOWN EXISTING TWO-WEEK HISTORY FOR THIS VERSION
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
                                   AND DATE_ADD(@ProgrammeStartDate, INTERVAL 13 DAY)
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
                                   AND DATE_ADD(@ProgrammeStartDate, INTERVAL 13 DAY)
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
                                   AND DATE_ADD(@ProgrammeStartDate, INTERVAL 13 DAY)
    ) x
);

-- ------------------------------------------------------------
-- 5. INSERT 14 DAYS OF SESSION OCCURRENCES
-- Skipped sessions:
--   day 2 PM = pain flare-up
--   day 4 AM = pet reluctance
--   day 6 PM = veterinary visit
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
    WHERE day_index < 13
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
    CASE
        WHEN s.day_index = 1 AND sm.Period = 'PM' THEN 'skipped'
        WHEN s.day_index = 3 AND sm.Period = 'AM' THEN 'skipped'
        WHEN s.day_index = 5 AND sm.Period = 'PM' THEN 'skipped'
        ELSE 'completed'
    END AS Status,
    CASE
        WHEN s.day_index = 1 AND sm.Period = 'PM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '18:10:00')
        WHEN s.day_index = 3 AND sm.Period = 'AM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '08:20:00')
        WHEN s.day_index = 5 AND sm.Period = 'PM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '17:45:00')
        WHEN sm.Period = 'AM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '08:15:00')
        ELSE TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '18:00:00')
    END,
    CASE
        WHEN s.day_index = 1 AND sm.Period = 'PM' THEN NULL
        WHEN s.day_index = 3 AND sm.Period = 'AM' THEN NULL
        WHEN s.day_index = 5 AND sm.Period = 'PM' THEN NULL
        WHEN sm.Period = 'AM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '08:15:00')
        ELSE TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '18:00:00')
    END,
    CASE
        WHEN s.day_index = 1 AND sm.Period = 'PM' THEN NULL
        WHEN s.day_index = 3 AND sm.Period = 'AM' THEN NULL
        WHEN s.day_index = 5 AND sm.Period = 'PM' THEN NULL
        WHEN sm.Period = 'AM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '08:32:00')
        ELSE TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '18:24:00')
    END,
    CASE
        WHEN s.day_index = 1 AND sm.Period = 'PM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '18:12:00')
        WHEN s.day_index = 3 AND sm.Period = 'AM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '08:22:00')
        WHEN s.day_index = 5 AND sm.Period = 'PM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '17:50:00')
        ELSE NULL
    END,
    CASE
        WHEN sm.Period = 'AM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '20:30:00')
        ELSE TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '20:35:00')
    END,
    CASE
        WHEN s.day_index = 0 AND sm.Period = 'AM' THEN 'Buddy was hesitant to bear full weight at the start. Stopped one exercise early due to discomfort.'
        WHEN s.day_index = 1 AND sm.Period = 'PM' THEN 'Skipped this evening session due to increased discomfort after the morning exercises.'
        WHEN s.day_index = 2 AND sm.Period = 'AM' THEN 'Still stiff this morning. Needed extra encouragement and several short pauses.'
        WHEN s.day_index = 3 AND sm.Period = 'AM' THEN 'Skipped because Buddy was reluctant to cooperate and resisted handling.'
        WHEN s.day_index = 4 AND sm.Period = 'PM' THEN 'More settled this evening. Movement looked smoother and Buddy was less resistant.'
        WHEN s.day_index = 5 AND sm.Period = 'PM' THEN 'Skipped due to veterinary follow-up visit and late return home.'
        WHEN s.day_index = 7 AND sm.Period = 'AM' THEN 'Completed the full session with only mild hesitation.'
        WHEN s.day_index = 9 AND sm.Period = 'PM' THEN 'Noticeable improvement in confidence and willingness to continue.'
        WHEN s.day_index = 11 AND sm.Period = 'AM' THEN 'Completed comfortably with improved control and reduced visible discomfort.'
        WHEN s.day_index = 13 AND sm.Period = 'PM' THEN 'Strong finish to the two-week block. Buddy completed all exercises well.'
        ELSE NULL
    END
FROM seq s
CROSS JOIN session_map sm
ORDER BY ScheduledDate, sm.PeriodSort;

-- ------------------------------------------------------------
-- 6. INSERT SKIP DETAILS
-- ------------------------------------------------------------
INSERT INTO SessionSkip (
    SessionOccurrenceId,
    SessionSkipReasonId,
    Comments,
    CreatedDate
)
SELECT
    so.SessionOccurrenceId,
    CASE
        WHEN so.ScheduledDate = DATE_ADD(@ProgrammeStartDate, INTERVAL 1 DAY) AND so.Period = 'PM' THEN @ReasonId_PainFlare
        WHEN so.ScheduledDate = DATE_ADD(@ProgrammeStartDate, INTERVAL 3 DAY) AND so.Period = 'AM' THEN @ReasonId_PetReluctance
        WHEN so.ScheduledDate = DATE_ADD(@ProgrammeStartDate, INTERVAL 5 DAY) AND so.Period = 'PM' THEN @ReasonId_VetVisit
    END,
    CASE
        WHEN so.ScheduledDate = DATE_ADD(@ProgrammeStartDate, INTERVAL 1 DAY) AND so.Period = 'PM'
            THEN 'Buddy showed increased pain and sensitivity this evening, so the session was stopped for comfort.'
        WHEN so.ScheduledDate = DATE_ADD(@ProgrammeStartDate, INTERVAL 3 DAY) AND so.Period = 'AM'
            THEN 'Buddy was reluctant to participate, moved away repeatedly, and would not settle safely for the exercises.'
        WHEN so.ScheduledDate = DATE_ADD(@ProgrammeStartDate, INTERVAL 5 DAY) AND so.Period = 'PM'
            THEN 'Session skipped due to veterinary appointment and recovery time afterwards.'
    END,
    COALESCE(so.SkippedDateTime, CURRENT_TIMESTAMP)
FROM SessionOccurrence so
WHERE so.ProgrammeVersionId = @ProgrammeVersionId
  AND so.PetId = @PetId
  AND so.Status = 'skipped';

-- ------------------------------------------------------------
-- 7. INSERT EXERCISE COMPLETIONS FOR NON-SKIPPED SESSIONS
-- Includes improving pain scores and some early partial completion.
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
    COALESCE(so.CompletedDateTime, so.DeviceRecordedDateTime),
    COALESCE(so.DeviceRecordedDateTime, so.StartedDateTime),
    so.SyncedDateTime,
    CASE
        WHEN so.day_index IN (0,1,2) AND se.SortOrder <= 2
            THEN GREATEST(1, FLOOR(COALESCE(se.Reps, e.DefaultReps) * 0.60))
        WHEN so.day_index IN (3,4) AND se.SortOrder = 1
            THEN GREATEST(1, FLOOR(COALESCE(se.Reps, e.DefaultReps) * 0.75))
        ELSE COALESCE(se.Reps, e.DefaultReps)
    END,
    CASE
        WHEN so.day_index IN (0,1,2) AND se.SortOrder <= 2
            THEN GREATEST(1, FLOOR(COALESCE(se.Sets, e.DefaultSets) * 0.50))
        WHEN so.day_index IN (3,4) AND se.SortOrder = 1
            THEN GREATEST(1, FLOOR(COALESCE(se.Sets, e.DefaultSets) * 0.75))
        ELSE COALESCE(se.Sets, e.DefaultSets)
    END,
    CASE
        WHEN so.day_index IN (0,1) THEN 7
        WHEN so.day_index IN (2,3) THEN 6
        WHEN so.day_index IN (4,5) THEN 5
        WHEN so.day_index IN (6,7) THEN 4
        WHEN so.day_index IN (8,9,10) THEN 3
        ELSE 2
    END,
    CASE
        WHEN so.day_index = 0 AND se.SortOrder = 1 THEN 'Stopped early after discomfort increased during the first exercise.'
        WHEN so.day_index = 2 AND se.SortOrder = 2 THEN 'Needed extra encouragement and did not manage full reps.'
        WHEN so.day_index = 4 AND se.SortOrder = 1 THEN 'Movement looked a little freer today and Buddy tolerated this better.'
        WHEN so.day_index = 7 AND se.SortOrder = 1 THEN 'Completed the full movement pattern with only mild hesitation.'
        WHEN so.day_index = 9 AND se.SortOrder = 1 THEN 'Pain clearly reduced. Buddy completed the exercise with better confidence.'
        WHEN so.day_index = 11 AND se.SortOrder = 1 THEN 'Completed comfortably with improved control and no obvious reluctance.'
        WHEN so.day_index = 13 AND se.SortOrder = 1 THEN 'Strong finish to the programme block; completed all prescribed work well.'
        ELSE NULL
    END,
    CASE
        WHEN so.day_index IN (0,1,2) AND se.SortOrder <= 2 THEN 'partial'
        WHEN so.day_index IN (3,4) AND se.SortOrder = 1 THEN 'partial'
        ELSE 'completed'
    END
FROM (
    SELECT
        so.SessionOccurrenceId,
        so.SessionId,
        so.ScheduledDate,
        so.StartedDateTime,
        so.CompletedDateTime,
        so.DeviceRecordedDateTime,
        so.SyncedDateTime,
        DATEDIFF(so.ScheduledDate, @ProgrammeStartDate) AS day_index
    FROM SessionOccurrence so
    WHERE so.ProgrammeVersionId = @ProgrammeVersionId
      AND so.PetId = @PetId
      AND so.Status = 'completed'
) so
JOIN SessionExercise se
  ON se.SessionId = so.SessionId
JOIN Exercise e
  ON e.ExerciseId = se.ExerciseId
ORDER BY so.ScheduledDate, so.SessionId, se.SortOrder, se.SessionExerciseId;

COMMIT;

-- ============================================================
-- 8. VERIFICATION REPORTS
-- ============================================================

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
ORDER BY so.ScheduledDate, FIELD(so.Period, 'AM', 'PM', 'single');

SELECT
    so.ScheduledDate,
    so.Period,
    ssr.ReasonName,
    ss.Comments
FROM SessionSkip ss
JOIN SessionOccurrence so
  ON so.SessionOccurrenceId = ss.SessionOccurrenceId
JOIN SessionSkipReason ssr
  ON ssr.SessionSkipReasonId = ss.SessionSkipReasonId
WHERE so.ProgrammeVersionId = @ProgrammeVersionId
  AND so.PetId = @PetId
ORDER BY so.ScheduledDate, FIELD(so.Period, 'AM', 'PM', 'single');

SELECT
    so.ScheduledDate,
    MIN(ec.PainScore) AS MinPainScore,
    MAX(ec.PainScore) AS MaxPainScore,
    SUM(CASE WHEN ec.CompletionStatus = 'completed' THEN 1 ELSE 0 END) AS ExercisesCompleted,
    SUM(CASE WHEN ec.CompletionStatus = 'partial' THEN 1 ELSE 0 END) AS ExercisesPartial
FROM ExerciseCompletion ec
JOIN SessionOccurrence so
  ON so.SessionOccurrenceId = ec.SessionOccurrenceId
WHERE so.ProgrammeVersionId = @ProgrammeVersionId
  AND so.PetId = @PetId
GROUP BY so.ScheduledDate
ORDER BY so.ScheduledDate;

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
ORDER BY so.ScheduledDate, FIELD(so.Period, 'AM', 'PM', 'single'), ec.ExerciseKeySnapshot;

COMMIT;

SET SQL_SAFE_UPDATES = @OLD_SQL_SAFE_UPDATES;
