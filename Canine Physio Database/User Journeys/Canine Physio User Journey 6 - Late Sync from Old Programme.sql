USE canine_physiotherapy;

START TRANSACTION;

-- ============================================================
-- USER JOURNEY 6: LATE SYNC FROM OLD PROGRAMME VERSION
-- Purpose:
--   Simulate delayed mobile sync from Programme 1 after Programme 2
--   has already been created and published as the current schedule.
--
-- What this proves:
--   1. Late completions can still be stored against the correct
--      historical ProgrammeVersionId
--   2. Programme 2 remains the current active programme
--   3. Historical reporting remains intact
--
-- Scenario:
--   - Owner completed one previously unsynced AM session from the
--     tail end of Programme 1 while offline
--   - Data only reaches server after Programme 2 is already current
-- ============================================================

-- ------------------------------------------------------------
-- 1. LOOK UP PROGRAMME 1 AND PROGRAMME 2
-- ------------------------------------------------------------
SELECT
    MIN(ProgrammeId),
    MAX(ProgrammeId)
INTO
    @OldProgrammeId,
    @CurrentProgrammeId
FROM Programme;

SELECT
    p1.CurrentProgrammeVersionId,
    p1.TreatmentCaseId,
    tc.PetId
INTO
    @OldProgrammeVersionId,
    @TreatmentCaseId,
    @PetId
FROM Programme p1
JOIN TreatmentCase tc
  ON tc.TreatmentCaseId = p1.TreatmentCaseId
WHERE p1.ProgrammeId = @OldProgrammeId
LIMIT 1;

SELECT
    p2.CurrentProgrammeVersionId
INTO
    @CurrentProgrammeVersionId
FROM Programme p2
WHERE p2.ProgrammeId = @CurrentProgrammeId
LIMIT 1;

SELECT
    SessionId
INTO @OldSessionId_AM
FROM Session
WHERE ProgrammeId = @OldProgrammeId
  AND Period = 'AM'
LIMIT 1;

SELECT
    p1.EndDate
INTO @OldProgrammeEndDate
FROM Programme p1
WHERE p1.ProgrammeId = @OldProgrammeId
LIMIT 1;

-- Use the last day of Programme 1 for the late sync example
SET @LateSyncDate = @OldProgrammeEndDate;

SELECT
    @OldProgrammeId AS OldProgrammeId,
    @OldProgrammeVersionId AS OldProgrammeVersionId,
    @CurrentProgrammeId AS CurrentProgrammeId,
    @CurrentProgrammeVersionId AS CurrentProgrammeVersionId,
    @PetId AS PetId,
    @OldSessionId_AM AS OldSessionId_AM,
    @LateSyncDate AS LateSyncDate;

-- ------------------------------------------------------------
-- 2. GUARD CHECKS
-- ------------------------------------------------------------
SELECT
    CASE WHEN @OldProgrammeId IS NULL THEN 'MISSING OLD PROGRAMME' ELSE 'OK' END AS OldProgrammeCheck,
    CASE WHEN @OldProgrammeVersionId IS NULL THEN 'MISSING OLD PROGRAMME VERSION' ELSE 'OK' END AS OldProgrammeVersionCheck,
    CASE WHEN @CurrentProgrammeId IS NULL THEN 'MISSING CURRENT PROGRAMME' ELSE 'OK' END AS CurrentProgrammeCheck,
    CASE WHEN @CurrentProgrammeVersionId IS NULL THEN 'MISSING CURRENT PROGRAMME VERSION' ELSE 'OK' END AS CurrentProgrammeVersionCheck,
    CASE WHEN @OldSessionId_AM IS NULL THEN 'MISSING OLD AM SESSION' ELSE 'OK' END AS OldAMSessionCheck;

-- ------------------------------------------------------------
-- 3. REMOVE ANY PREVIOUS TEST LATE-SYNC DATA FOR SAME DATE/PERIOD
--    Makes the journey rerunnable.
-- ------------------------------------------------------------
DELETE FROM ExerciseCompletion
WHERE SessionOccurrenceId IN (
    SELECT SessionOccurrenceId
    FROM (
        SELECT so.SessionOccurrenceId
        FROM SessionOccurrence so
        WHERE so.ProgrammeVersionId = @OldProgrammeVersionId
          AND so.PetId = @PetId
          AND so.ScheduledDate = @LateSyncDate
          AND so.Period = 'AM'
    ) x
);

DELETE FROM SessionSkip
WHERE SessionOccurrenceId IN (
    SELECT SessionOccurrenceId
    FROM (
        SELECT so.SessionOccurrenceId
        FROM SessionOccurrence so
        WHERE so.ProgrammeVersionId = @OldProgrammeVersionId
          AND so.PetId = @PetId
          AND so.ScheduledDate = @LateSyncDate
          AND so.Period = 'AM'
    ) x
);

DELETE FROM SessionOccurrence
WHERE SessionOccurrenceId IN (
    SELECT SessionOccurrenceId
    FROM (
        SELECT so.SessionOccurrenceId
        FROM SessionOccurrence so
        WHERE so.ProgrammeVersionId = @OldProgrammeVersionId
          AND so.PetId = @PetId
          AND so.ScheduledDate = @LateSyncDate
          AND so.Period = 'AM'
    ) x
);

-- ------------------------------------------------------------
-- 4. INSERT LATE-SYNC SESSION OCCURRENCE
--    Note:
--    Device timestamps are old, sync timestamp is recent.
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
) VALUES (
    @OldProgrammeVersionId,
    @OldSessionId_AM,
    @PetId,
    @LateSyncDate,
    'AM',
    'completed',
    TIMESTAMP(@LateSyncDate, '08:10:00'),
    TIMESTAMP(@LateSyncDate, '08:10:00'),
    TIMESTAMP(@LateSyncDate, '08:29:00'),
    NULL,
    CURRENT_TIMESTAMP,
    'Late sync from offline mobile upload. Session was completed on the final morning of Programme 1 but only reached the server after Programme 2 had already been published.'
);

SET @LateSyncSessionOccurrenceId = LAST_INSERT_ID();

-- ------------------------------------------------------------
-- 5. INSERT EXERCISE COMPLETIONS AGAINST OLD PROGRAMME VERSION
--    Slightly improved compared with the earliest sessions.
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
    @LateSyncSessionOccurrenceId,
    se.SessionExerciseId,
    e.ExerciseKey,
    e.Title,
    TIMESTAMP(@LateSyncDate, '08:29:00'),
    TIMESTAMP(@LateSyncDate, '08:10:00'),
    CURRENT_TIMESTAMP,
    COALESCE(se.Reps, e.DefaultReps),
    COALESCE(se.Sets, e.DefaultSets),
    3,
    CASE
        WHEN se.SortOrder = 1 THEN 'Uploaded late from mobile. Buddy completed this comfortably with mild stiffness only.'
        WHEN se.SortOrder = 2 THEN 'Late-sync upload. Needed a brief pause midway but completed all prescribed work.'
        WHEN se.SortOrder = 3 THEN 'Late-sync upload. Movement quality looked smoother than earlier in Programme 1.'
        ELSE 'Late-sync upload from final offline session of Programme 1.'
    END,
    'completed'
FROM SessionExercise se
JOIN Exercise e
  ON e.ExerciseId = se.ExerciseId
WHERE se.SessionId = @OldSessionId_AM
ORDER BY se.SortOrder, se.SessionExerciseId;

COMMIT;

-- ============================================================
-- 6. VERIFICATION
-- ============================================================

-- A. Confirm the late-synced occurrence is linked to the OLD version
SELECT
    so.SessionOccurrenceId,
    so.ProgrammeVersionId,
    pv.ProgrammeId,
    so.PetId,
    so.ScheduledDate,
    so.Period,
    so.Status,
    so.DeviceRecordedDateTime,
    so.SyncedDateTime,
    so.Comments
FROM SessionOccurrence so
JOIN ProgrammeVersion pv
  ON pv.ProgrammeVersionId = so.ProgrammeVersionId
WHERE so.SessionOccurrenceId = @LateSyncSessionOccurrenceId;

-- B. Confirm the late-synced exercise rows exist
SELECT
    ec.ExerciseCompletionId,
    ec.SessionOccurrenceId,
    ec.ExerciseKeySnapshot,
    ec.RepsCompleted,
    ec.SetsCompleted,
    ec.PainScore,
    ec.CompletionStatus,
    ec.DeviceRecordedDateTime,
    ec.SyncedDateTime,
    ec.Comments
FROM ExerciseCompletion ec
WHERE ec.SessionOccurrenceId = @LateSyncSessionOccurrenceId
ORDER BY ec.ExerciseCompletionId;

-- C. Confirm Programme 2 is still the current active programme
SELECT
    p.ProgrammeId,
    p.ProgrammeName,
    p.Status,
    p.IsCurrent,
    p.CurrentProgrammeVersionId
FROM Programme p
ORDER BY p.ProgrammeId;

-- D. Show both programme versions for comparison
SELECT
    pv.ProgrammeVersionId,
    pv.ProgrammeId,
    pv.VersionNumber,
    pv.VersionStatus,
    pv.PublishedDate
FROM ProgrammeVersion pv
ORDER BY pv.ProgrammeId, pv.VersionNumber;
