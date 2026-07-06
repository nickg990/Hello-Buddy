-- =============================================================================
-- CANINE PHYSIOTHERAPY - RELEASE 2 / INCREMENT 10: EXERCISE AUDIT TRAIL
-- =============================================================================
-- Exercise create/update/activate/deactivate now writes to the EXISTING
-- AuditLog table (EntityName = 'Exercise'), the same table already used for
-- owner GDPR-deletion audit rows. No new tables or columns are introduced.
-- This script only ASSERTS the required table/columns exist and adds the
-- (EntityName, EntityId) lookup index if it is missing.
-- Idempotent: safe to run against a fresh build or an existing production DB.
-- =============================================================================

USE canine_physiotherapy;
SET NAMES utf8mb4;

-- ---------------------------------------------------------------------------
-- 1. Assert the AuditLog table and required columns exist.
-- ---------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS AssertAuditLogSchema;
DELIMITER $$
CREATE PROCEDURE AssertAuditLogSchema()
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables
        WHERE table_schema = DATABASE() AND table_name = 'auditlog'
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'AuditLog table is missing. Run the base schema script first.';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE() AND table_name = 'auditlog' AND column_name = 'EntityName'
    ) OR NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE() AND table_name = 'auditlog' AND column_name = 'EntityId'
    ) OR NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE() AND table_name = 'auditlog' AND column_name = 'OldValuesJson'
    ) OR NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE() AND table_name = 'auditlog' AND column_name = 'NewValuesJson'
    ) OR NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_schema = DATABASE() AND table_name = 'auditlog' AND column_name = 'ActionDateTime'
    ) THEN
        SIGNAL SQLSTATE '45000'
            SET MESSAGE_TEXT = 'AuditLog table is missing one or more required columns.';
    END IF;
END$$
DELIMITER ;

CALL AssertAuditLogSchema();
DROP PROCEDURE IF EXISTS AssertAuditLogSchema;

-- ---------------------------------------------------------------------------
-- 2. Add the (EntityName, EntityId) lookup index if missing.
--    (Not present in the original v2.3 fresh-build schema.)
-- ---------------------------------------------------------------------------
DROP PROCEDURE IF EXISTS AddAuditLogEntityIndexIfMissing;
DELIMITER $$
CREATE PROCEDURE AddAuditLogEntityIndexIfMissing()
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.statistics
        WHERE table_schema = DATABASE()
          AND table_name = 'auditlog'
          AND index_name = 'IX_AuditLog_EntityName_EntityId'
    ) THEN
        ALTER TABLE AuditLog
            ADD INDEX IX_AuditLog_EntityName_EntityId (EntityName, EntityId);
    END IF;
END$$
DELIMITER ;

CALL AddAuditLogEntityIndexIfMissing();
DROP PROCEDURE IF EXISTS AddAuditLogEntityIndexIfMissing;
