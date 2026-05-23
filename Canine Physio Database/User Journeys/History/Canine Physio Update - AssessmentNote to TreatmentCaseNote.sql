USE canine_physiotherapy;

START TRANSACTION;

-- ============================================================
-- CANINE PHYSIO - UPDATE SCRIPT
-- Replace AssessmentNote with TreatmentCaseNote
--
-- Purpose:
--   1. Add TreatmentCaseNote tied directly to TreatmentCase
--   2. Migrate existing AssessmentNote rows where a matching
--      TreatmentCase exists for the same Pet and Practitioner
--   3. Drop AssessmentNote after migration
--
-- Notes:
--   - This assumes each existing AssessmentNote belongs to the
--     most relevant TreatmentCase for the same Pet/Practitioner.
--   - If multiple TreatmentCase rows exist for the same Pet and
--     Practitioner, the most recent case by StartDate is used.
--   - Review migrated data before running the DROP TABLE if you
--     want a more cautious two-step migration.
-- ============================================================

-- ------------------------------------------------------------
-- 1. CREATE NEW TREATMENTCASENOTE TABLE
-- ------------------------------------------------------------
CREATE TABLE IF NOT EXISTS TreatmentCaseNote (
    TreatmentCaseNoteId   BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    TreatmentCaseId       BIGINT UNSIGNED NOT NULL,
    PractitionerId        BIGINT UNSIGNED NOT NULL,
    CreatedDate           DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    NoteType              VARCHAR(50) NULL,
    NoteText              TEXT NOT NULL,
    IsActive              BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT PK_TreatmentCaseNote PRIMARY KEY (TreatmentCaseNoteId),
    CONSTRAINT FK_TreatmentCaseNote_TreatmentCase
        FOREIGN KEY (TreatmentCaseId) REFERENCES TreatmentCase (TreatmentCaseId)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT FK_TreatmentCaseNote_Practitioner
        FOREIGN KEY (PractitionerId) REFERENCES Practitioner (PractitionerId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_TreatmentCaseNote_TreatmentCaseId
    ON TreatmentCaseNote (TreatmentCaseId);

CREATE INDEX IX_TreatmentCaseNote_PractitionerId
    ON TreatmentCaseNote (PractitionerId);

CREATE INDEX IX_TreatmentCaseNote_IsActive
    ON TreatmentCaseNote (IsActive);

-- ------------------------------------------------------------
-- 2. MIGRATE EXISTING ASSESSMENTNOTE DATA
--    Picks the most recent TreatmentCase per Pet/Practitioner.
-- ------------------------------------------------------------
INSERT INTO TreatmentCaseNote (
    TreatmentCaseId,
    PractitionerId,
    CreatedDate,
    NoteType,
    NoteText,
    IsActive
)
SELECT
    tc_latest.TreatmentCaseId,
    an.PractitionerId,
    an.CreatedDate,
    an.NoteType,
    an.NoteText,
    an.IsActive
FROM AssessmentNote an
JOIN (
    SELECT tc1.TreatmentCaseId, tc1.PetId, tc1.PractitionerId
    FROM TreatmentCase tc1
    JOIN (
        SELECT
            PetId,
            PractitionerId,
            MAX(StartDate) AS MaxStartDate
        FROM TreatmentCase
        GROUP BY PetId, PractitionerId
    ) tc2
      ON tc1.PetId = tc2.PetId
     AND tc1.PractitionerId = tc2.PractitionerId
     AND tc1.StartDate = tc2.MaxStartDate
) tc_latest
  ON tc_latest.PetId = an.PetId
 AND tc_latest.PractitionerId = an.PractitionerId;

-- ------------------------------------------------------------
-- 3. OPTIONAL VERIFICATION BEFORE DROP
-- ------------------------------------------------------------
SELECT
    tcn.TreatmentCaseNoteId,
    tcn.TreatmentCaseId,
    tcn.PractitionerId,
    tcn.CreatedDate,
    tcn.NoteType,
    LEFT(tcn.NoteText, 120) AS NotePreview,
    tcn.IsActive
FROM TreatmentCaseNote tcn
ORDER BY tcn.TreatmentCaseNoteId;

-- ------------------------------------------------------------
-- 4. DROP OLD TABLE
--    Comment this section out if you want to inspect the migrated
--    data first and drop the old table later.
-- ------------------------------------------------------------
DROP TABLE IF EXISTS AssessmentNote;

COMMIT;

-- ------------------------------------------------------------
-- 5. FINAL STRUCTURE CHECK
-- ------------------------------------------------------------
SHOW TABLES LIKE 'TreatmentCaseNote';
SHOW TABLES LIKE 'AssessmentNote';
