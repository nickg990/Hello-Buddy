-- =============================================================================
-- CANINE PHYSIOTHERAPY - INCREMENT 8: PRACTITIONER LOGIN + ATTRIBUTION
-- =============================================================================
-- Safe to run against an EXISTING database (idempotent: CREATE TABLE IF NOT EXISTS
-- and ADD COLUMN IF NOT EXISTS guards). Also safe to include in a fresh-build run
-- after the v2.3 fresh schema.
-- Credential seeding is handled by PractitionerLoginSeedHostedService at API
-- startup, not in this script, to avoid committing password hashes to source.
-- =============================================================================

USE canine_physiotherapy;
SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ---------------------------------------------------------------------------
-- 1. PractitionerLogin table
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS PractitionerLogin (
    PractitionerId      BIGINT UNSIGNED NOT NULL,
    PasswordHash        VARCHAR(512)    NOT NULL,
    Role                ENUM('physiotherapist','administrator') NOT NULL DEFAULT 'physiotherapist',
    IsActive            BOOLEAN         NOT NULL DEFAULT TRUE,
    MustChangePassword  BOOLEAN         NOT NULL DEFAULT FALSE,
    FailedAttemptCount  TINYINT UNSIGNED NOT NULL DEFAULT 0,
    LockedUntil         DATETIME        NULL,
    LastLoginDate       DATETIME        NULL,
    CreatedDate         DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate         DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT PK_PractitionerLogin PRIMARY KEY (PractitionerId),
    CONSTRAINT FK_PractitionerLogin_Practitioner
        FOREIGN KEY (PractitionerId) REFERENCES Practitioner (PractitionerId)
        ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ---------------------------------------------------------------------------
-- 2. Attribution columns — added with IF NOT EXISTS guards
-- ---------------------------------------------------------------------------

DROP PROCEDURE IF EXISTS AddColumnIfMissing;
DELIMITER $$
CREATE PROCEDURE AddColumnIfMissing(
    IN p_table_name VARCHAR(64),
    IN p_column_name VARCHAR(64),
    IN p_column_definition TEXT
)
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = DATABASE()
          AND table_name = p_table_name
          AND column_name = p_column_name
    ) THEN
        SET @sql = CONCAT('ALTER TABLE `', p_table_name, '` ADD COLUMN ', p_column_definition);
        PREPARE stmt FROM @sql;
        EXECUTE stmt;
        DEALLOCATE PREPARE stmt;
    END IF;
END $$
DELIMITER ;

-- Owner
CALL AddColumnIfMissing('Owner', 'CreatedByPractitionerId', '`CreatedByPractitionerId` BIGINT UNSIGNED NULL');
CALL AddColumnIfMissing('Owner', 'CreatedByPractitionerName', '`CreatedByPractitionerName` VARCHAR(255) NULL');
CALL AddColumnIfMissing('Owner', 'UpdatedByPractitionerId', '`UpdatedByPractitionerId` BIGINT UNSIGNED NULL');
CALL AddColumnIfMissing('Owner', 'UpdatedByPractitionerName', '`UpdatedByPractitionerName` VARCHAR(255) NULL');

-- Pet
CALL AddColumnIfMissing('Pet', 'CreatedByPractitionerId', '`CreatedByPractitionerId` BIGINT UNSIGNED NULL');
CALL AddColumnIfMissing('Pet', 'CreatedByPractitionerName', '`CreatedByPractitionerName` VARCHAR(255) NULL');
CALL AddColumnIfMissing('Pet', 'UpdatedByPractitionerId', '`UpdatedByPractitionerId` BIGINT UNSIGNED NULL');
CALL AddColumnIfMissing('Pet', 'UpdatedByPractitionerName', '`UpdatedByPractitionerName` VARCHAR(255) NULL');

-- TreatmentCase
CALL AddColumnIfMissing('TreatmentCase', 'CreatedByPractitionerId', '`CreatedByPractitionerId` BIGINT UNSIGNED NULL');
CALL AddColumnIfMissing('TreatmentCase', 'CreatedByPractitionerName', '`CreatedByPractitionerName` VARCHAR(255) NULL');
CALL AddColumnIfMissing('TreatmentCase', 'UpdatedByPractitionerId', '`UpdatedByPractitionerId` BIGINT UNSIGNED NULL');
CALL AddColumnIfMissing('TreatmentCase', 'UpdatedByPractitionerName', '`UpdatedByPractitionerName` VARCHAR(255) NULL');

-- TreatmentCaseNote (created-by only — notes are never updated, only soft-deleted)
CALL AddColumnIfMissing('TreatmentCaseNote', 'CreatedByPractitionerName', '`CreatedByPractitionerName` VARCHAR(255) NULL');

-- Programme
CALL AddColumnIfMissing('Programme', 'CreatedByPractitionerId', '`CreatedByPractitionerId` BIGINT UNSIGNED NULL');
CALL AddColumnIfMissing('Programme', 'CreatedByPractitionerName', '`CreatedByPractitionerName` VARCHAR(255) NULL');
CALL AddColumnIfMissing('Programme', 'UpdatedByPractitionerId', '`UpdatedByPractitionerId` BIGINT UNSIGNED NULL');
CALL AddColumnIfMissing('Programme', 'UpdatedByPractitionerName', '`UpdatedByPractitionerName` VARCHAR(255) NULL');

-- Exercise
CALL AddColumnIfMissing('Exercise', 'CreatedByPractitionerId', '`CreatedByPractitionerId` BIGINT UNSIGNED NULL');
CALL AddColumnIfMissing('Exercise', 'CreatedByPractitionerName', '`CreatedByPractitionerName` VARCHAR(255) NULL');
CALL AddColumnIfMissing('Exercise', 'UpdatedByPractitionerId', '`UpdatedByPractitionerId` BIGINT UNSIGNED NULL');
CALL AddColumnIfMissing('Exercise', 'UpdatedByPractitionerName', '`UpdatedByPractitionerName` VARCHAR(255) NULL');

-- ProgrammeVersion (id already exists; add name snapshot only)
CALL AddColumnIfMissing('ProgrammeVersion', 'CreatedByPractitionerName', '`CreatedByPractitionerName` VARCHAR(255) NULL');

DROP PROCEDURE IF EXISTS AddColumnIfMissing;

SET FOREIGN_KEY_CHECKS = 1;
