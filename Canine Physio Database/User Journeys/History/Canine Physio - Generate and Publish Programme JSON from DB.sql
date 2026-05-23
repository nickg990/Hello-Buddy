USE canine_physiotherapy;

-- ============================================================
-- GENERATE MOBILE PROGRAMME JSON FROM RELATIONAL DATA
-- MySQL 8.x
--
-- Purpose:
--   Generate a mobile-ready JSON document from:
--     Programme
--     TreatmentCase
--     Pet
--     Practitioner
--     Session
--     SessionExercise
--     Exercise
--
-- Notes:
--   1. This script RETURNS the JSON document as a query result.
--   2. It does not insert into ProgrammeVersion.
--   3. Image and video values are derived from Exercise.ImageUrl
--      and Exercise.VideoUrl:
--        - image     = filename without extension
--        - videoName = filename with extension
--   4. The current schema does not explicitly store
--      exerciseSet.key or exerciseSet.description, so:
--        - key is derived from Session.Period
--        - description is taken from Session.Objective
--          (or blank if null)
-- ============================================================

-- ------------------------------------------------------------
-- INPUT
-- Set this to the ProgrammeId you want to export.
-- ------------------------------------------------------------
SET @ProgrammeId = 1;

-- Optional fallback warning text if you are not storing it elsewhere.
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

-- ------------------------------------------------------------
-- GENERATED JSON
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
                SELECT JSON_ARRAYAGG(
                    JSON_OBJECT(
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
                                SELECT JSON_ARRAYAGG(
                                    JSON_OBJECT(
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
                                                SELECT JSON_ARRAYAGG(step.InstructionText ORDER BY step.SortOrder)
                                                FROM ExerciseInstruction step
                                                WHERE step.ExerciseId = e.ExerciseId
                                            ),
                                        'reps', COALESCE(se.Reps, e.DefaultReps),
                                        'sets', COALESCE(se.Sets, e.DefaultSets),
                                        'holdSeconds', COALESCE(se.HoldSeconds, e.DefaultHoldSeconds),
                                        'videoName',
                                            CASE
                                                WHEN e.VideoUrl IS NULL OR TRIM(e.VideoUrl) = '' THEN NULL
                                                ELSE SUBSTRING_INDEX(e.VideoUrl, '/', -1)
                                            END
                                    )
                                    ORDER BY se.SortOrder
                                )
                                FROM SessionExercise se
                                JOIN Exercise e
                                  ON e.ExerciseId = se.ExerciseId
                                WHERE se.SessionId = s.SessionId
                            )
                        )
                    )
                    ORDER BY s.SortOrder
                )
                FROM Session s
                WHERE s.ProgrammeId = @ProgrammeIdOut
            )
        )
    )
) AS GeneratedProgrammeJson;

-- ------------------------------------------------------------
-- OPTIONAL: RETURN RAW JSON WITHOUT PRETTY PRINTING
-- Useful if you want to copy/paste a compact JSON payload.
-- ------------------------------------------------------------
/*
SELECT JSON_OBJECT(
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
            SELECT JSON_ARRAYAGG(
                JSON_OBJECT(
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
                            SELECT JSON_ARRAYAGG(
                                JSON_OBJECT(
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
                                            SELECT JSON_ARRAYAGG(step.InstructionText ORDER BY step.SortOrder)
                                            FROM ExerciseInstruction step
                                            WHERE step.ExerciseId = e.ExerciseId
                                        ),
                                    'reps', COALESCE(se.Reps, e.DefaultReps),
                                    'sets', COALESCE(se.Sets, e.DefaultSets),
                                    'holdSeconds', COALESCE(se.HoldSeconds, e.DefaultHoldSeconds),
                                    'videoName',
                                        CASE
                                            WHEN e.VideoUrl IS NULL OR TRIM(e.VideoUrl) = '' THEN NULL
                                            ELSE SUBSTRING_INDEX(e.VideoUrl, '/', -1)
                                        END
                                )
                                ORDER BY se.SortOrder
                            )
                            FROM SessionExercise se
                            JOIN Exercise e
                              ON e.ExerciseId = se.ExerciseId
                            WHERE se.SessionId = s.SessionId
                        )
                    )
                )
                ORDER BY s.SortOrder
            )
            FROM Session s
            WHERE s.ProgrammeId = @ProgrammeIdOut
        )
    )
) AS GeneratedProgrammeJson;
*/


-- ============================================================
-- COMPANION PUBLISH SCRIPT
-- Purpose:
--   Persist the generated JSON into ProgrammeVersion.PayloadJson
--   and update Programme.CurrentProgrammeVersionId.
--
-- Usage:
--   1. Leave @ProgrammeId set above.
--   2. Optionally set @CreatedByPractitionerId explicitly below.
--   3. Run this section after the generation query.
--
-- Notes:
--   - This creates the NEXT version number for the programme.
--   - It inserts the JSON as a published version immediately.
--   - It uses the same generation logic, so the stored JSON matches
--     the exported JSON.
-- ============================================================

-- Optional: set explicitly if you want to override the practitioner
-- associated with the TreatmentCase.
-- SET @CreatedByPractitionerId = 1;

START TRANSACTION;

-- ------------------------------------------------------------
-- Resolve practitioner if not explicitly set
-- ------------------------------------------------------------
SET @CreatedByPractitionerId = COALESCE(
    @CreatedByPractitionerId,
    (
        SELECT tc.PractitionerId
        FROM Programme p
        JOIN TreatmentCase tc
          ON tc.TreatmentCaseId = p.TreatmentCaseId
        WHERE p.ProgrammeId = @ProgrammeId
        LIMIT 1
    )
);

-- ------------------------------------------------------------
-- Get next version number
-- ------------------------------------------------------------
SET @NextVersionNumber = COALESCE(
    (
        SELECT MAX(pv.VersionNumber) + 1
        FROM ProgrammeVersion pv
        WHERE pv.ProgrammeId = @ProgrammeId
    ),
    1
);

-- ------------------------------------------------------------
-- Build and insert the JSON payload as a published version
-- ------------------------------------------------------------
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
                SELECT JSON_ARRAYAGG(
                    JSON_OBJECT(
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
                                SELECT JSON_ARRAYAGG(
                                    JSON_OBJECT(
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
                                                SELECT JSON_ARRAYAGG(step.InstructionText ORDER BY step.SortOrder)
                                                FROM ExerciseInstruction step
                                                WHERE step.ExerciseId = e.ExerciseId
                                            ),
                                        'reps', COALESCE(se.Reps, e.DefaultReps),
                                        'sets', COALESCE(se.Sets, e.DefaultSets),
                                        'holdSeconds', COALESCE(se.HoldSeconds, e.DefaultHoldSeconds),
                                        'videoName',
                                            CASE
                                                WHEN e.VideoUrl IS NULL OR TRIM(e.VideoUrl) = '' THEN NULL
                                                ELSE SUBSTRING_INDEX(e.VideoUrl, '/', -1)
                                            END
                                    )
                                    ORDER BY se.SortOrder
                                )
                                FROM SessionExercise se
                                JOIN Exercise e
                                  ON e.ExerciseId = se.ExerciseId
                                WHERE se.SessionId = s.SessionId
                            )
                        )
                    )
                    ORDER BY s.SortOrder
                )
                FROM Session s
                WHERE s.ProgrammeId = @ProgrammeIdOut
            )
        )
    ),
    '1.0',
    CONCAT('Published from relational programme authoring data. Version ', @NextVersionNumber),
    @CreatedByPractitionerId,
    CURRENT_TIMESTAMP,
    CURRENT_TIMESTAMP;

SET @ProgrammeVersionId = LAST_INSERT_ID();

-- ------------------------------------------------------------
-- Supersede any previously published version for this programme
-- ------------------------------------------------------------
UPDATE ProgrammeVersion
SET
    VersionStatus = 'superseded',
    SupersededDate = CURRENT_TIMESTAMP
WHERE ProgrammeId = @ProgrammeId
  AND ProgrammeVersionId <> @ProgrammeVersionId
  AND VersionStatus = 'published';

-- ------------------------------------------------------------
-- Point Programme at the new current version
-- ------------------------------------------------------------
UPDATE Programme
SET
    CurrentProgrammeVersionId = @ProgrammeVersionId,
    UpdatedDate = CURRENT_TIMESTAMP
WHERE ProgrammeId = @ProgrammeId;

COMMIT;

-- ------------------------------------------------------------
-- Verification
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
