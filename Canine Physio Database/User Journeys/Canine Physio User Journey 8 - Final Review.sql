USE canine_physiotherapy;

START TRANSACTION;

-- ============================================================
-- USER JOURNEY 8: FINAL REVIEW AFTER PROGRAMME 2
-- Purpose:
--   1. Review the completed week(s) on Programme 2
--   2. Add a final review TreatmentCaseNote
--   3. Mark Programme 2 as completed and not current
--   4. Leave the TreatmentCase open for a separate closure journey
-- ============================================================

-- ------------------------------------------------------------
-- 1. LOOK UP CURRENT PROGRAMME / CASE CONTEXT
-- ------------------------------------------------------------
SELECT
    p.ProgrammeId,
    p.TreatmentCaseId,
    tc.PractitionerId,
    tc.PetId
INTO
    @CurrentProgrammeId,
    @TreatmentCaseId,
    @PractitionerId,
    @PetId
FROM Programme p
JOIN TreatmentCase tc
  ON tc.TreatmentCaseId = p.TreatmentCaseId
WHERE p.IsCurrent = TRUE
ORDER BY p.ProgrammeId DESC
LIMIT 1;

SELECT
    @CurrentProgrammeId AS CurrentProgrammeId,
    @TreatmentCaseId AS TreatmentCaseId,
    @PractitionerId AS PractitionerId,
    @PetId AS PetId;

-- ------------------------------------------------------------
-- 2. ADD FINAL REVIEW NOTE
-- ------------------------------------------------------------
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
    'final_review',
    'Final review completed following the progressed home rehabilitation programme. Buddy completed the recent programme with full adherence, no missed sessions, and negligible pain scores. Movement quality, confidence, and exercise tolerance have improved significantly compared with the initial rehabilitation phase. No further structured home programme is required at this stage. Recommend discharge from active physiotherapy with advice to maintain general activity and monitor for any recurrence of stiffness, reluctance, or pain.',
    TRUE
);

SET @FinalReviewNoteId = LAST_INSERT_ID();

-- ------------------------------------------------------------
-- 3. COMPLETE PROGRAMME 2
-- ------------------------------------------------------------
UPDATE Programme
SET
    Status = 'completed',
    IsCurrent = FALSE,
    UpdatedDate = CURRENT_TIMESTAMP,
    Notes = CONCAT(
        COALESCE(Notes, ''),
        CASE
            WHEN Notes IS NULL OR TRIM(Notes) = '' THEN ''
            ELSE '\n'
        END,
        'Programme completed following final review on ',
        DATE_FORMAT(CURRENT_DATE, '%Y-%m-%d'),
        '. No further active programme prescribed.'
    )
WHERE ProgrammeId = @CurrentProgrammeId;

COMMIT;

-- ============================================================
-- 4. VERIFICATION
-- ============================================================
SELECT
    tcn.TreatmentCaseNoteId,
    tcn.TreatmentCaseId,
    tcn.PractitionerId,
    tcn.CreatedDate,
    tcn.NoteType,
    tcn.NoteText
FROM TreatmentCaseNote tcn
WHERE tcn.TreatmentCaseNoteId = @FinalReviewNoteId;

SELECT
    p.ProgrammeId,
    p.ProgrammeName,
    p.Status,
    p.IsCurrent,
    p.StartDate,
    p.EndDate,
    p.CurrentProgrammeVersionId,
    p.Notes
FROM Programme p
WHERE p.ProgrammeId = @CurrentProgrammeId;
