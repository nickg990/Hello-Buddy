USE canine_physiotherapy;

SET @ProgrammeId = 1;
SET @WarningText = 'Always consult your veterinary physiotherapist before modifying exercises. Stop immediately if your dog shows signs of pain or distress.';

SELECT
    p.ProgrammeId,
    p.ProgrammeName,
    DATE_FORMAT(p.CreatedDate, '%Y-%m-%d') AS CreatedDateText,
    DATE_FORMAT(p.StartDate, '%Y-%m-%d') AS StartDateText,
    DATE_FORMAT(p.EndDate, '%Y-%m-%d') AS EndDateText,
    pet.Name AS DogName,
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
JOIN TreatmentCase tc ON tc.TreatmentCaseId = p.TreatmentCaseId
JOIN Pet pet ON pet.PetId = tc.PetId
JOIN Practitioner pr ON pr.PractitionerId = tc.PractitionerId
WHERE p.ProgrammeId = @ProgrammeId
LIMIT 1;

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
                            'key', COALESCE(sct.ContentKey, CASE WHEN s.Period='AM' THEN 'amSession' WHEN s.Period='PM' THEN 'pmSession' WHEN s.Period='single' THEN 'singleSession' ELSE LOWER(s.Period) END),
                            'description', COALESCE(sct.MobileDescription, COALESCE(s.Objective,'')),
                            'exercises',
                            (
                                SELECT JSON_ARRAYAGG(ex_rows.exercise_json)
                                FROM (
                                    SELECT JSON_OBJECT(
                                        'key', e.ExerciseKey,
                                        'title', e.Title,
                                        'image', CASE WHEN e.ImageUrl IS NULL OR TRIM(e.ImageUrl)='' THEN NULL ELSE SUBSTRING_INDEX(SUBSTRING_INDEX(e.ImageUrl,'/',-1),'.',1) END,
                                        'summary', COALESCE(e.ObjectiveSummary,''),
                                        'instructions', (
                                            SELECT JSON_ARRAYAGG(instr_rows.InstructionText)
                                            FROM (
                                                SELECT step.InstructionText
                                                FROM ExerciseInstruction step
                                                WHERE step.ExerciseId = e.ExerciseId
                                                ORDER BY step.StepNumber
                                            ) instr_rows
                                        ),
                                        'reps', COALESCE(se.Reps,e.DefaultReps),
                                        'sets', COALESCE(se.Sets,e.DefaultSets),
                                        'holdSeconds', COALESCE(se.HoldSeconds,e.DefaultHoldSeconds),
                                        'videoName', CASE WHEN e.VideoUrl IS NULL OR TRIM(e.VideoUrl)='' THEN NULL ELSE SUBSTRING_INDEX(e.VideoUrl,'/',-1) END
                                    ) AS exercise_json
                                    FROM SessionExercise se
                                    JOIN Exercise e ON e.ExerciseId = se.ExerciseId
                                    WHERE se.SessionId = s.SessionId
                                    ORDER BY se.SortOrder, se.SessionExerciseId
                                ) ex_rows
                            )
                        )
                    ) AS session_json
                    FROM Session s
                    LEFT JOIN SessionContentType sct ON sct.SessionContentTypeId = s.SessionContentTypeId
                    WHERE s.ProgrammeId = @ProgrammeIdOut
                    ORDER BY s.SortOrder, s.SessionId
                ) session_rows
            )
        )
    )
) AS GeneratedProgrammeJson;

START TRANSACTION;

SET @CreatedByPractitionerId = NULL;
SELECT tc.PractitionerId INTO @CreatedByPractitionerId
FROM Programme p
JOIN TreatmentCase tc ON tc.TreatmentCaseId = p.TreatmentCaseId
WHERE p.ProgrammeId = @ProgrammeId
LIMIT 1;

SET @NextVersionNumber = COALESCE((SELECT MAX(pv.VersionNumber)+1 FROM ProgrammeVersion pv WHERE pv.ProgrammeId=@ProgrammeId),1);

INSERT INTO ProgrammeVersion (
    ProgrammeId, VersionNumber, VersionStatus, PayloadJson, PayloadSchemaVersion,
    ChangeSummary, CreatedByPractitionerId, CreatedDate, PublishedDate
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
                            'key', COALESCE(sct.ContentKey, CASE WHEN s.Period='AM' THEN 'amSession' WHEN s.Period='PM' THEN 'pmSession' WHEN s.Period='single' THEN 'singleSession' ELSE LOWER(s.Period) END),
                            'description', COALESCE(sct.MobileDescription, COALESCE(s.Objective,'')),
                            'exercises',
                            (
                                SELECT JSON_ARRAYAGG(ex_rows.exercise_json)
                                FROM (
                                    SELECT JSON_OBJECT(
                                        'key', e.ExerciseKey,
                                        'title', e.Title,
                                        'image', CASE WHEN e.ImageUrl IS NULL OR TRIM(e.ImageUrl)='' THEN NULL ELSE SUBSTRING_INDEX(SUBSTRING_INDEX(e.ImageUrl,'/',-1),'.',1) END,
                                        'summary', COALESCE(e.ObjectiveSummary,''),
                                        'instructions', (
                                            SELECT JSON_ARRAYAGG(instr_rows.InstructionText)
                                            FROM (
                                                SELECT step.InstructionText
                                                FROM ExerciseInstruction step
                                                WHERE step.ExerciseId = e.ExerciseId
                                                ORDER BY step.StepNumber
                                            ) instr_rows
                                        ),
                                        'reps', COALESCE(se.Reps,e.DefaultReps),
                                        'sets', COALESCE(se.Sets,e.DefaultSets),
                                        'holdSeconds', COALESCE(se.HoldSeconds,e.DefaultHoldSeconds),
                                        'videoName', CASE WHEN e.VideoUrl IS NULL OR TRIM(e.VideoUrl)='' THEN NULL ELSE SUBSTRING_INDEX(e.VideoUrl,'/',-1) END
                                    ) AS exercise_json
                                    FROM SessionExercise se
                                    JOIN Exercise e ON e.ExerciseId = se.ExerciseId
                                    WHERE se.SessionId = s.SessionId
                                    ORDER BY se.SortOrder, se.SessionExerciseId
                                ) ex_rows
                            )
                        )
                    ) AS session_json
                    FROM Session s
                    LEFT JOIN SessionContentType sct ON sct.SessionContentTypeId = s.SessionContentTypeId
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

COMMIT;
