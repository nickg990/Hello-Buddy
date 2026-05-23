USE canine_physiotherapy;

START TRANSACTION;

-- ============================================================
-- USER JOURNEY 4: MOBILE DOWNLOAD, COMPLETION, SKIP, AND SYNC
-- Purpose:
--   Simulate the owner downloading two weeks of the current
--   programme to the mobile app, completing most sessions,
--   skipping a few early sessions, and syncing results back.
--
-- Behaviour model:
--   - 14 days of AM/PM sessions are created from the current
--     published programme version
--   - early pain scores are higher and some exercises are only
--     partially completed
--   - pain scores improve across the two weeks
--   - skipped sessions include:
--       * pain flare-up
--       * pet reluctance / non-compliance
--       * veterinary visit
--   - comments are added for exceptions and improvement
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

-- Diagnostic check
SELECT
    @ProgrammeId AS ProgrammeId,
    @ProgrammeVersionId AS ProgrammeVersionId,
    @PetId AS PetId,
    @ProgrammeStartDate AS ProgrammeStartDate,
    @SessionId_AM AS SessionId_AM,
    @SessionId_PM AS SessionId_PM;

-- ------------------------------------------------------------
-- 2. ENSURE VETERINARY VISIT SKIP REASON EXISTS
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
-- 3. CLEAN DOWN ANY EXISTING TWO-WEEK HISTORY FOR THIS VERSION
--    Makes the script safe to rerun on the same rebuilt database.
-- ------------------------------------------------------------
DELETE ec
FROM ExerciseCompletion ec
JOIN SessionOccurrence so
  ON so.SessionOccurrenceId = ec.SessionOccurrenceId
WHERE so.ProgrammeVersionId = @ProgrammeVersionId
  AND so.PetId = @PetId
  AND so.ScheduledDate BETWEEN @ProgrammeStartDate
                           AND DATE_ADD(@ProgrammeStartDate, INTERVAL 13 DAY);

DELETE ss
FROM SessionSkip ss
JOIN SessionOccurrence so
  ON so.SessionOccurrenceId = ss.SessionOccurrenceId
WHERE so.ProgrammeVersionId = @ProgrammeVersionId
  AND so.PetId = @PetId
  AND so.ScheduledDate BETWEEN @ProgrammeStartDate
                           AND DATE_ADD(@ProgrammeStartDate, INTERVAL 13 DAY);

DELETE FROM SessionOccurrence
WHERE ProgrammeVersionId = @ProgrammeVersionId
  AND PetId = @PetId
  AND ScheduledDate BETWEEN @ProgrammeStartDate
                       AND DATE_ADD(@ProgrammeStartDate, INTERVAL 13 DAY);

-- ------------------------------------------------------------
-- 4. INSERT 14 DAYS OF SESSION OCCURRENCES
--    Specific skipped sessions:
--      day 2 PM = pain flare-up
--      day 4 AM = pet reluctance
--      day 6 PM = veterinary visit
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
    END AS DeviceRecordedDateTime,
    CASE
        WHEN s.day_index = 1 AND sm.Period = 'PM' THEN NULL
        WHEN s.day_index = 3 AND sm.Period = 'AM' THEN NULL
        WHEN s.day_index = 5 AND sm.Period = 'PM' THEN NULL
        WHEN sm.Period = 'AM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '08:15:00')
        ELSE TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '18:00:00')
    END AS StartedDateTime,
    CASE
        WHEN s.day_index = 1 AND sm.Period = 'PM' THEN NULL
        WHEN s.day_index = 3 AND sm.Period = 'AM' THEN NULL
        WHEN s.day_index = 5 AND sm.Period = 'PM' THEN NULL
        WHEN sm.Period = 'AM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '08:32:00')
        ELSE TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '18:24:00')
    END AS CompletedDateTime,
    CASE
        WHEN s.day_index = 1 AND sm.Period = 'PM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '18:12:00')
        WHEN s.day_index = 3 AND sm.Period = 'AM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '08:22:00')
        WHEN s.day_index = 5 AND sm.Period = 'PM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '17:50:00')
        ELSE NULL
    END AS SkippedDateTime,
    CASE
        WHEN sm.Period = 'AM' THEN TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '20:30:00')
        ELSE TIMESTAMP(DATE_ADD(@ProgrammeStartDate, INTERVAL s.day_index DAY), '20:35:00')
    END AS SyncedDateTime,
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
    END AS Comments
FROM seq s
CROSS JOIN session_map sm
ORDER BY ScheduledDate, sm.PeriodSort;

-- ------------------------------------------------------------
-- 5. INSERT SKIP DETAILS
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
-- 6. INSERT EXERCISE COMPLETIONS FOR NON-SKIPPED SESSIONS
--    Pain scores improve over time; early sessions are partial.
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
        WHEN day_index IN (0,1,2) AND se.SortOrder <= 2
            THEN GREATEST(1, FLOOR(COALESCE(se.Reps, e.DefaultReps) * 0.60))
        WHEN day_index IN (3,4) AND se.SortOrder = 1
            THEN GREATEST(1, FLOOR(COALESCE(se.Reps, e.DefaultReps) * 0.75))
        ELSE COALESCE(se.Reps, e.DefaultReps)
    END AS RepsCompleted,
    CASE
        WHEN day_index IN (0,1,2) AND se.SortOrder <= 2
            THEN GREATEST(1, FLOOR(COALESCE(se.Sets, e.DefaultSets) * 0.50))
        WHEN day_index IN (3,4) AND se.SortOrder = 1
            THEN GREATEST(1, FLOOR(COALESCE(se.Sets, e.DefaultSets) * 0.75))
        ELSE COALESCE(se.Sets, e.DefaultSets)
    END AS SetsCompleted,
    CASE
        WHEN day_index IN (0,1) THEN 7
        WHEN day_index IN (2,3) THEN 6
        WHEN day_index IN (4,5) THEN 5
        WHEN day_index IN (6,7) THEN 4
        WHEN day_index IN (8,9,10) THEN 3
        ELSE 2
    END AS PainScore,
    CASE
        WHEN day_index = 0 AND se.SortOrder = 1 THEN 'Stopped early after discomfort increased during the first exercise.'
        WHEN day_index = 2 AND se.SortOrder = 2 THEN 'Needed extra encouragement and did not manage full reps.'
        WHEN day_index = 4 AND se.SortOrder = 1 THEN 'Movement looked a little freer today and Buddy tolerated this better.'
        WHEN day_index = 7 AND se.SortOrder = 1 THEN 'Completed the full movement pattern with only mild hesitation.'
        WHEN day_index = 9 AND se.SortOrder = 1 THEN 'Pain clearly reduced. Buddy completed the exercise with better confidence.'
        WHEN day_index = 11 AND se.SortOrder = 1 THEN 'Completed comfortably with improved control and no obvious reluctance.'
        WHEN day_index = 13 AND se.SortOrder = 1 THEN 'Strong finish to the programme block; completed all prescribed work well.'
        ELSE NULL
    END AS Comments,
    CASE
        WHEN day_index IN (0,1,2) AND se.SortOrder <= 2 THEN 'partial'
        WHEN day_index IN (3,4) AND se.SortOrder = 1 THEN 'partial'
        ELSE 'completed'
    END AS CompletionStatus
FROM (
    SELECT
        so.SessionOccurrenceId,
        so.SessionId,
        so.ScheduledDate,
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
-- 7. VERIFICATION REPORTS
-- ============================================================

-- A. Session timeline for the two-week block
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

-- B. Skipped sessions with reasons
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

-- C. Exercise completion trend summary by day
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

-- D. Detailed completion rows with notes
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
