USE canine_physiotherapy;

-- ============================================================
-- GENERATE AND PUBLISH MOBILE PROGRAMME JSON FROM RELATIONAL DATA
-- MySQL 8.x compatible version
--
-- Purpose:
--   1. Generate a mobile-ready JSON document from relational data
--   2. Publish that JSON into ProgrammeVersion
-- ============================================================

SET @ProgrammeId = 1;
SET @WarningText = 'Always consult your veterinary physiotherapist before modifying exercises. Stop immediately if your dog shows signs of pain or distress.';

-- ------------------------------------------------------------
-- BASE LOOKUP
-- ------------------------------------------------------------
SELECT
    p.ProgrammeId,
    p.ProgrammeName,
    DATE_FORMAT(p.CreatedDate, '%Y-%m-%d') AS CreatedDateText,
    DATE_FORMAT(p.StartDate, '%Y-%m-%d')   AS StartDateText,
    DATE_FORMAT(p.EndDate, '%Y-%m-%d')     AS EndDateText,
    pet.Name                               AS DogName,
    CONCAT(pr.FirstName, ' ', pr.LastName) AS PractitionerName
INTO
    @ProgrammeIdOut,
    @ProgrammeNameOut,
    @CreatedDateOut,
    @StartDateOut,
    @EndDateOut,
    @DogNameOut,
    @PractitionerNameOut
FROM Programme p
JOIN TreatmentCase tc
  ON tc.TreatmentCaseId = p.TreatmentCaseId
JOIN Pet pet
  ON pet.PetId = tc.PetId
JOIN Practitioner pr
  ON pr.PractitionerId = tc.PractitionerId
WHERE p.ProgrammeId = @ProgrammeId
LIMIT 1;

-- Diagnostic check: these should all be populated
SELECT
    @ProgrammeIdOut AS ProgrammeIdOut,
    @ProgrammeNameOut AS ProgrammeNameOut,
    @DogNameOut AS DogNameOut,
    @PractitionerNameOut AS PractitionerNameOut;

-- ------------------------------------------------------------
-- GENERATE JSON
-- ------------------------------------------------------------
SELECT JSON_PRETTY(
    JSON_OBJECT(
        'programme', JSON_OBJECT(
            'id', CONCAT('prog-', LPAD(@ProgrammeIdOut, 6, '0')),
            'dogName', @DogNameOut,
            'practitionerName', @PractitionerNameOut,
            'createdDate', @CreatedDateOut,
            'startDate', @StartDateOut,
            'endDate', @EndDateOut,
            'warningText', @WarningText,
            'dailySessions',
            (
                SELECT JSON_ARRAYAGG(session_rows.session_json)
                FROM (
                    SELECT JSON_OBJECT(
                        'period', s.Period,
                        'exerciseSet', JSON_OBJECT(
                            'key',
                            CASE
                                WHEN s.Period = 'AM' THEN 'amSession'
                                WHEN s.Period = 'PM' THEN 'pmSession'
                                WHEN s.Period = 'single' THEN 'singleSession'
                                ELSE LOWER(s.Period)
                            END,
                            'description', COALESCE(s.Objective, ''),
                            'exercises',
                            (
                                SELECT JSON_ARRAYAGG(ex_rows.exercise_json)
                                FROM (
                                    SELECT JSON_OBJECT(
                                        'key', e.ExerciseKey,
                                        'title', e.Title,
                                        'image',
                                            CASE
                                                WHEN e.ImageUrl IS NULL OR TRIM(e.ImageUrl) = '' THEN NULL
                                                ELSE SUBSTRING_INDEX(
                                                    SUBSTRING_INDEX(e.ImageUrl, '/', -1),
                                                    '.',
                                                    1
                                                )
                                            END,
                                        'summary', COALESCE(e.ObjectiveSummary, ''),
                                        'instructions',
                                            (
                                                SELECT JSON_ARRAYAGG(instr_rows.InstructionText)
                                                FROM (
                                                    SELECT step.InstructionText
                                                    FROM ExerciseInstruction step
                                                    WHERE step.ExerciseId = e.ExerciseId
                                                    ORDER BY step.StepNumber
                                                ) instr_rows
                                            ),
                                        'reps', COALESCE(se.Reps, e.DefaultReps),
                                        'sets', COALESCE(se.Sets, e.DefaultSets),
                                        'holdSeconds', COALESCE(se.HoldSeconds, e.DefaultHoldSeconds),
                                        'videoName',
                                            CASE
                                                WHEN e.VideoUrl IS NULL OR TRIM(e.VideoUrl) = '' THEN NULL
                                                ELSE SUBSTRING_INDEX(e.VideoUrl, '/', -1)
                                            END
                                    ) AS exercise_json
                                    FROM SessionExercise se
                                    JOIN Exercise e
                                      ON e.ExerciseId = se.ExerciseId
                                    WHERE se.SessionId = s.SessionId
                                    ORDER BY se.SortOrder, se.SessionExerciseId
                                ) ex_rows
                            )
                        )
                    ) AS session_json
                    FROM Session s
                    WHERE s.ProgrammeId = @ProgrammeIdOut
                    ORDER BY s.SortOrder, s.SessionId
                ) session_rows
            )
        )
    )
) AS GeneratedProgrammeJson;

-- ------------------------------------------------------------
-- PUBLISH GENERATED JSON INTO PROGRAMMEVERSION
-- ------------------------------------------------------------

START TRANSACTION;

-- Reset stale session variable and resolve practitioner from TreatmentCase
SET @CreatedByPractitionerId = NULL;

SELECT tc.PractitionerId
INTO @CreatedByPractitionerId
FROM Programme p
JOIN TreatmentCase tc
  ON tc.TreatmentCaseId = p.TreatmentCaseId
WHERE p.ProgrammeId = @ProgrammeId
LIMIT 1;

-- Diagnostic check: this must not be NULL
SELECT
    @ProgrammeId AS ProgrammeId,
    @CreatedByPractitionerId AS CreatedByPractitionerId;

SET @NextVersionNumber = COALESCE(
    (
        SELECT MAX(pv.VersionNumber) + 1
        FROM ProgrammeVersion pv
        WHERE pv.ProgrammeId = @ProgrammeId
    ),
    1
);

INSERT INTO ProgrammeVersion (
    ProgrammeId,
    VersionNumber,
    VersionStatus,
    PayloadJson,
    PayloadSchemaVersion,
    ChangeSummary,
    CreatedByPractitionerId,
    CreatedDate,
    PublishedDate
)
SELECT
    @ProgrammeId,
    @NextVersionNumber,
    'published',
    JSON_OBJECT(
        'programme', JSON_OBJECT(
            'id', CONCAT('prog-', LPAD(@ProgrammeIdOut, 6, '0')),
            'dogName', @DogNameOut,
            'practitionerName', @PractitionerNameOut,
            'createdDate', @CreatedDateOut,
            'startDate', @StartDateOut,
            'endDate', @EndDateOut,
            'warningText', @WarningText,
            'dailySessions',
            (
                SELECT JSON_ARRAYAGG(session_rows.session_json)
                FROM (
                    SELECT JSON_OBJECT(
                        'period', s.Period,
                        'exerciseSet', JSON_OBJECT(
                            'key',
                            CASE
                                WHEN s.Period = 'AM' THEN 'amSession'
                                WHEN s.Period = 'PM' THEN 'pmSession'
                                WHEN s.Period = 'single' THEN 'singleSession'
                                ELSE LOWER(s.Period)
                            END,
                            'description', COALESCE(s.Objective, ''),
                            'exercises',
                            (
                                SELECT JSON_ARRAYAGG(ex_rows.exercise_json)
                                FROM (
                                    SELECT JSON_OBJECT(
                                        'key', e.ExerciseKey,
                                        'title', e.Title,
                                        'image',
                                            CASE
                                                WHEN e.ImageUrl IS NULL OR TRIM(e.ImageUrl) = '' THEN NULL
                                                ELSE SUBSTRING_INDEX(
                                                    SUBSTRING_INDEX(e.ImageUrl, '/', -1),
                                                    '.',
                                                    1
                                                )
                                            END,
                                        'summary', COALESCE(e.ObjectiveSummary, ''),
                                        'instructions',
                                            (
                                                SELECT JSON_ARRAYAGG(instr_rows.InstructionText)
                                                FROM (
                                                    SELECT step.InstructionText
                                                    FROM ExerciseInstruction step
                                                    WHERE step.ExerciseId = e.ExerciseId
                                                    ORDER BY step.StepNumber
                                                ) instr_rows
                                            ),
                                        'reps', COALESCE(se.Reps, e.DefaultReps),
                                        'sets', COALESCE(se.Sets, e.DefaultSets),
                                        'holdSeconds', COALESCE(se.HoldSeconds, e.DefaultHoldSeconds),
                                        'videoName',
                                            CASE
                                                WHEN e.VideoUrl IS NULL OR TRIM(e.VideoUrl) = '' THEN NULL
                                                ELSE SUBSTRING_INDEX(e.VideoUrl, '/', -1)
                                            END
                                    ) AS exercise_json
                                    FROM SessionExercise se
                                    JOIN Exercise e
                                      ON e.ExerciseId = se.ExerciseId
                                    WHERE se.SessionId = s.SessionId
                                    ORDER BY se.SortOrder, se.SessionExerciseId
                                ) ex_rows
                            )
                        )
                    ) AS session_json
                    FROM Session s
                    WHERE s.ProgrammeId = @ProgrammeIdOut
                    ORDER BY s.SortOrder, s.SessionId
                ) session_rows
            )
        )
    ),
    '1.0',
    CONCAT('Published from relational programme authoring data. Version ', @NextVersionNumber),
    @CreatedByPractitionerId,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP;

SET @ProgrammeVersionId = LAST_INSERT_ID();

UPDATE ProgrammeVersion
SET
    VersionStatus = 'superseded',
    SupersededDate = CURRENT_TIMESTAMP
WHERE ProgrammeId = @ProgrammeId
  AND ProgrammeVersionId <> @ProgrammeVersionId
  AND VersionStatus = 'published';

UPDATE Programme
SET
    CurrentProgrammeVersionId = @ProgrammeVersionId,
    UpdatedDate = CURRENT_TIMESTAMP
WHERE ProgrammeId = @ProgrammeId;

COMMIT;

-- ------------------------------------------------------------
-- VERIFICATION
-- ------------------------------------------------------------
SELECT
    pv.ProgrammeVersionId,
    pv.ProgrammeId,
    pv.VersionNumber,
    pv.VersionStatus,
    pv.PayloadSchemaVersion,
    pv.ChangeSummary,
    pv.CreatedByPractitionerId,
    pv.CreatedDate,
    pv.PublishedDate,
    p.CurrentProgrammeVersionId
FROM ProgrammeVersion pv
JOIN Programme p
  ON p.ProgrammeId = pv.ProgrammeId
WHERE pv.ProgrammeVersionId = @ProgrammeVersionId;

SELECT JSON_PRETTY(PayloadJson) AS PublishedProgrammeJson
FROM ProgrammeVersion
WHERE ProgrammeVersionId = @ProgrammeVersionId;
