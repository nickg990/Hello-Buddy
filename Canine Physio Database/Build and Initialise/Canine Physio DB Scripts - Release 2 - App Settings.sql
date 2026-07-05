-- =============================================================================
-- Release 2 — App Settings table
-- Idempotent: safe to run multiple times.
-- =============================================================================

USE canine_physiotherapy;

-- ---------------------------------------------------------------------------
-- AppSetting table: generic key/value store for admin-managed settings.
-- ---------------------------------------------------------------------------
CREATE TABLE IF NOT EXISTS AppSetting (
    SettingKey              VARCHAR(255)        NOT NULL,
    SettingValue            TEXT                NULL,
    UpdatedDate             DATETIME            NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    UpdatedByPractitionerId BIGINT UNSIGNED     NULL,
    PRIMARY KEY (SettingKey)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ---------------------------------------------------------------------------
-- Seed: Google Drive video library URL.
-- INSERT IGNORE preserves any value already set by the administrator.
-- ---------------------------------------------------------------------------
INSERT IGNORE INTO AppSetting (SettingKey, SettingValue)
VALUES ('VideoLibrary.GoogleDriveUrl', 'https://drive.google.com/drive/folders/1FQXInuGCPdFP5ywFaNnO39Be0ffeZMGm');
