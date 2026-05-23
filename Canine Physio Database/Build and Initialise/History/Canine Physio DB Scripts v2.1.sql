DROP DATABASE IF EXISTS canine_physiotherapy;
CREATE DATABASE canine_physiotherapy;
USE canine_physiotherapy;

-- ============================================================
-- CANINE PHYSIOTHERAPY - REVISED DDL V2
-- MySQL 8.x / InnoDB / utf8mb4
--
-- Design intent:
-- 1) SQL remains the authoritative admin database.
-- 2) Programmes are authored relationally, but published as immutable,
--    versioned JSON snapshots for mobile delivery and history.
-- 3) Mobile devices may work offline in SQLite and sync history later.
-- 4) Historical completions must remain attributable to the programme
--    version active on the device when recorded.
-- ============================================================

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ============================================================
-- DROP TABLES (reverse dependency order)
-- ============================================================
/*
DROP TABLE IF EXISTS AuditLog;
DROP TABLE IF EXISTS NotificationPreference;
DROP TABLE IF EXISTS ExerciseCompletion;
DROP TABLE IF EXISTS SessionSkip;
DROP TABLE IF EXISTS SessionSkipReason;
DROP TABLE IF EXISTS SessionOccurrence;
DROP TABLE IF EXISTS SessionExercise;
DROP TABLE IF EXISTS Session;
DROP TABLE IF EXISTS ProgrammeVersion;
DROP TABLE IF EXISTS Programme;
DROP TABLE IF EXISTS Exercise;
DROP TABLE IF EXISTS ExerciseCategory;
DROP TABLE IF EXISTS ProgrammeTemplate;
DROP TABLE IF EXISTS TreatmentCase;
DROP TABLE IF EXISTS AssessmentNote;
DROP TABLE IF EXISTS TermsAcceptance;
DROP TABLE IF EXISTS TermsDocument;
DROP TABLE IF EXISTS PasswordResetRequest;
DROP TABLE IF EXISTS RegistrationCode;
DROP TABLE IF EXISTS Practitioner_Pet;
DROP TABLE IF EXISTS UserAccount;
DROP TABLE IF EXISTS Pet;
DROP TABLE IF EXISTS Owner;
DROP TABLE IF EXISTS Practitioner;
*/

-- ============================================================
-- 1. PRACTITIONER
-- ============================================================
CREATE TABLE Practitioner (
    PractitionerId      BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    FirstName           VARCHAR(100) NOT NULL,
    LastName            VARCHAR(100) NOT NULL,
    Email               VARCHAR(255) NOT NULL,
    PhoneNumber         VARCHAR(30) NULL,
    IsActive            BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedDate         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT PK_Practitioner PRIMARY KEY (PractitionerId),
    CONSTRAINT UQ_Practitioner_Email UNIQUE (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ============================================================
-- 2. OWNER
-- ============================================================
CREATE TABLE Owner (
    OwnerId             BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    FirstName           VARCHAR(100) NOT NULL,
    LastName            VARCHAR(100) NOT NULL,
    Email               VARCHAR(255) NOT NULL,
    PhoneNumber         VARCHAR(30) NULL,
    AddressLine1        VARCHAR(255) NULL,
    AddressLine2        VARCHAR(255) NULL,
    Town                VARCHAR(100) NULL,
    Postcode            VARCHAR(20) NULL,
    CreatedDate         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT PK_Owner PRIMARY KEY (OwnerId),
    CONSTRAINT UQ_Owner_Email UNIQUE (Email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- ============================================================
-- 3. USER ACCOUNT
-- ============================================================
CREATE TABLE UserAccount (
    UserAccountId       BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    OwnerId             BIGINT UNSIGNED NOT NULL,
    Email               VARCHAR(255) NOT NULL,
    PasswordHash        VARCHAR(255) NOT NULL,
    PasswordSalt        VARCHAR(255) NULL,
    IsActive            BOOLEAN NOT NULL DEFAULT TRUE,
    LastLoginDate       DATETIME NULL,
    CreatedDate         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT PK_UserAccount PRIMARY KEY (UserAccountId),
    CONSTRAINT UQ_UserAccount_Email UNIQUE (Email),
    CONSTRAINT FK_UserAccount_Owner
        FOREIGN KEY (OwnerId) REFERENCES Owner (OwnerId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_UserAccount_OwnerId ON UserAccount (OwnerId);

-- ============================================================
-- 4. PET
-- Age retained because many owners do not know DOB, especially rescues.
-- DOB is optional and may be omitted if not clinically available.
-- ============================================================
CREATE TABLE Pet (
    PetId                BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    OwnerId              BIGINT UNSIGNED NOT NULL,
    Name                 VARCHAR(100) NOT NULL,
    Age                  TINYINT UNSIGNED NULL,
    DateOfBirth          DATE NULL,
    Breed                VARCHAR(100) NULL,
    Sex                  ENUM('male', 'female', 'unknown') NOT NULL DEFAULT 'unknown',
    Weight               DECIMAL(5,2) NULL,
    IsActive             BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedDate          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT PK_Pet PRIMARY KEY (PetId),
    CONSTRAINT FK_Pet_Owner
        FOREIGN KEY (OwnerId) REFERENCES Owner (OwnerId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,
    CONSTRAINT CHK_Pet_Age CHECK (Age IS NULL OR Age <= 40)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_Pet_OwnerId ON Pet (OwnerId);
CREATE INDEX IX_Pet_IsActive ON Pet (IsActive);

-- ============================================================
-- 5. PRACTITIONER_PET
-- ============================================================
CREATE TABLE Practitioner_Pet (
    PractitionerPetId    BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    PractitionerId       BIGINT UNSIGNED NOT NULL,
    PetId                BIGINT UNSIGNED NOT NULL,
    AssignedFrom         DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AssignedTo           DATETIME NULL,
    IsPrimary            BOOLEAN NOT NULL DEFAULT FALSE,
    Status               ENUM('planned', 'active', 'ended', 'suspended') NOT NULL DEFAULT 'active',
    ReferralSource       VARCHAR(255) NULL,
    Notes                TEXT NULL,
    CreatedDate          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_Practitioner_Pet PRIMARY KEY (PractitionerPetId),
    CONSTRAINT FK_Practitioner_Pet_Practitioner
        FOREIGN KEY (PractitionerId) REFERENCES Practitioner (PractitionerId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,
    CONSTRAINT FK_Practitioner_Pet_Pet
        FOREIGN KEY (PetId) REFERENCES Pet (PetId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_Practitioner_Pet_PractitionerId ON Practitioner_Pet (PractitionerId);
CREATE INDEX IX_Practitioner_Pet_PetId ON Practitioner_Pet (PetId);
CREATE INDEX IX_Practitioner_Pet_Status ON Practitioner_Pet (Status);

-- ============================================================
-- 6. REGISTRATION CODE
-- ============================================================
CREATE TABLE RegistrationCode (
    RegistrationCodeId   BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    PractitionerId       BIGINT UNSIGNED NOT NULL,
    PetId                BIGINT UNSIGNED NULL,
    Code                 VARCHAR(100) NOT NULL,
    IssuedDate           DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ExpiryDate           DATETIME NOT NULL,
    UsedDate             DATETIME NULL,
    Status               ENUM('issued', 'used', 'expired', 'cancelled') NOT NULL DEFAULT 'issued',
    Notes                TEXT NULL,
    CONSTRAINT PK_RegistrationCode PRIMARY KEY (RegistrationCodeId),
    CONSTRAINT UQ_RegistrationCode_Code UNIQUE (Code),
    CONSTRAINT FK_RegistrationCode_Practitioner
        FOREIGN KEY (PractitionerId) REFERENCES Practitioner (PractitionerId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,
    CONSTRAINT FK_RegistrationCode_Pet
        FOREIGN KEY (PetId) REFERENCES Pet (PetId)
        ON DELETE SET NULL
        ON UPDATE CASCADE,
    CONSTRAINT CHK_RegistrationCode_Dates CHECK (ExpiryDate > IssuedDate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_RegistrationCode_PractitionerId ON RegistrationCode (PractitionerId);
CREATE INDEX IX_RegistrationCode_PetId ON RegistrationCode (PetId);
CREATE INDEX IX_RegistrationCode_Status ON RegistrationCode (Status);

-- ============================================================
-- 7. PASSWORD RESET REQUEST
-- ============================================================
CREATE TABLE PasswordResetRequest (
    PasswordResetRequestId BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    UserAccountId          BIGINT UNSIGNED NOT NULL,
    ResetToken             VARCHAR(255) NOT NULL,
    RequestedDate          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    ExpiryDate             DATETIME NOT NULL,
    ConsumedDate           DATETIME NULL,
    Status                 ENUM('requested', 'used', 'expired', 'cancelled') NOT NULL DEFAULT 'requested',
    CONSTRAINT PK_PasswordResetRequest PRIMARY KEY (PasswordResetRequestId),
    CONSTRAINT UQ_PasswordResetRequest_ResetToken UNIQUE (ResetToken),
    CONSTRAINT FK_PasswordResetRequest_UserAccount
        FOREIGN KEY (UserAccountId) REFERENCES UserAccount (UserAccountId)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT CHK_PasswordResetRequest_Dates CHECK (ExpiryDate > RequestedDate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_PasswordResetRequest_UserAccountId ON PasswordResetRequest (UserAccountId);
CREATE INDEX IX_PasswordResetRequest_Status ON PasswordResetRequest (Status);

-- ============================================================
-- 8. TERMS DOCUMENT
-- ============================================================
CREATE TABLE TermsDocument (
    TermsDocumentId      BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    DocumentType         ENUM('terms_of_service', 'privacy_policy', 'acceptable_use') NOT NULL,
    VersionNumber        VARCHAR(50) NOT NULL,
    Title                VARCHAR(255) NOT NULL,
    ContentText          LONGTEXT NOT NULL,
    EffectiveFrom        DATETIME NOT NULL,
    EffectiveTo          DATETIME NULL,
    IsActive             BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedDate          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_TermsDocument PRIMARY KEY (TermsDocumentId),
    CONSTRAINT UQ_TermsDocument_Type_Version UNIQUE (DocumentType, VersionNumber)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_TermsDocument_DocumentType ON TermsDocument (DocumentType);
CREATE INDEX IX_TermsDocument_IsActive ON TermsDocument (IsActive);

-- ============================================================
-- 9. TERMS ACCEPTANCE
-- ============================================================
CREATE TABLE TermsAcceptance (
    TermsAcceptanceId    BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    UserAccountId        BIGINT UNSIGNED NOT NULL,
    TermsDocumentId      BIGINT UNSIGNED NOT NULL,
    AcceptedDateTime     DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    AcceptanceMethod     VARCHAR(50) NULL,
    AcceptedVersionText  VARCHAR(100) NULL,
    CONSTRAINT PK_TermsAcceptance PRIMARY KEY (TermsAcceptanceId),
    CONSTRAINT FK_TermsAcceptance_UserAccount
        FOREIGN KEY (UserAccountId) REFERENCES UserAccount (UserAccountId)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT FK_TermsAcceptance_TermsDocument
        FOREIGN KEY (TermsDocumentId) REFERENCES TermsDocument (TermsDocumentId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_TermsAcceptance_UserAccountId ON TermsAcceptance (UserAccountId);
CREATE INDEX IX_TermsAcceptance_TermsDocumentId ON TermsAcceptance (TermsDocumentId);

-- ============================================================
-- 10. ASSESSMENT NOTE
-- ============================================================
CREATE TABLE AssessmentNote (
    AssessmentNoteId     BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    PetId                BIGINT UNSIGNED NOT NULL,
    PractitionerId       BIGINT UNSIGNED NOT NULL,
    CreatedDate          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    NoteText             TEXT NOT NULL,
    NoteType             VARCHAR(50) NULL,
    IsActive             BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT PK_AssessmentNote PRIMARY KEY (AssessmentNoteId),
    CONSTRAINT FK_AssessmentNote_Pet
        FOREIGN KEY (PetId) REFERENCES Pet (PetId)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT FK_AssessmentNote_Practitioner
        FOREIGN KEY (PractitionerId) REFERENCES Practitioner (PractitionerId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_AssessmentNote_PetId ON AssessmentNote (PetId);
CREATE INDEX IX_AssessmentNote_PractitionerId ON AssessmentNote (PractitionerId);
CREATE INDEX IX_AssessmentNote_IsActive ON AssessmentNote (IsActive);

-- ============================================================
-- 11. TREATMENT CASE
-- ============================================================
CREATE TABLE TreatmentCase (
    TreatmentCaseId      BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    PetId                BIGINT UNSIGNED NOT NULL,
    PractitionerId       BIGINT UNSIGNED NOT NULL,
    CaseTitle            VARCHAR(255) NOT NULL,
    ClinicalSummary      TEXT NULL,
    StartDate            DATE NOT NULL,
    EndDate              DATE NULL,
    Status               ENUM('planned', 'active', 'completed', 'cancelled') NOT NULL DEFAULT 'planned',
    CreatedDate          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT PK_TreatmentCase PRIMARY KEY (TreatmentCaseId),
    CONSTRAINT FK_TreatmentCase_Pet
        FOREIGN KEY (PetId) REFERENCES Pet (PetId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,
    CONSTRAINT FK_TreatmentCase_Practitioner
        FOREIGN KEY (PractitionerId) REFERENCES Practitioner (PractitionerId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,
    CONSTRAINT CHK_TreatmentCase_Dates CHECK (EndDate IS NULL OR EndDate >= StartDate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_TreatmentCase_PetId ON TreatmentCase (PetId);
CREATE INDEX IX_TreatmentCase_PractitionerId ON TreatmentCase (PractitionerId);
CREATE INDEX IX_TreatmentCase_Status ON TreatmentCase (Status);

-- ============================================================
-- 12. PROGRAMME TEMPLATE
-- Authoring template, not the immutable mobile delivery snapshot.
-- ============================================================
CREATE TABLE ProgrammeTemplate (
    ProgrammeTemplateId  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    PractitionerId       BIGINT UNSIGNED NOT NULL,
    TemplateName         VARCHAR(255) NOT NULL,
    Description          TEXT NULL,
    IsActive             BOOLEAN NOT NULL DEFAULT TRUE,
    CreatedDate          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT PK_ProgrammeTemplate PRIMARY KEY (ProgrammeTemplateId),
    CONSTRAINT FK_ProgrammeTemplate_Practitioner
        FOREIGN KEY (PractitionerId) REFERENCES Practitioner (PractitionerId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_ProgrammeTemplate_PractitionerId ON ProgrammeTemplate (PractitionerId);
CREATE INDEX IX_ProgrammeTemplate_IsActive ON ProgrammeTemplate (IsActive);

-- ============================================================
-- 13. EXERCISE CATEGORY
-- ============================================================
CREATE TABLE ExerciseCategory (
    ExerciseCategoryId   BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    CategoryKey          VARCHAR(100) NOT NULL,
    CategoryName         VARCHAR(150) NOT NULL,
    Description          TEXT NULL,
    IsActive             BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT PK_ExerciseCategory PRIMARY KEY (ExerciseCategoryId),
    CONSTRAINT UQ_ExerciseCategory_CategoryKey UNIQUE (CategoryKey),
    CONSTRAINT UQ_ExerciseCategory_CategoryName UNIQUE (CategoryName)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_ExerciseCategory_IsActive ON ExerciseCategory (IsActive);

-- ============================================================
-- 14. EXERCISE
-- Master/reference exercise library.
-- Packaged programme versions should snapshot the fields needed by mobile.
-- ============================================================
CREATE TABLE Exercise (
    ExerciseId           BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    ExerciseCategoryId   BIGINT UNSIGNED NULL,
    ExerciseKey          VARCHAR(100) NOT NULL,
    Title                VARCHAR(255) NOT NULL,
    ObjectiveSummary     TEXT NULL,
    InstructionsText     LONGTEXT NULL,
    DefaultReps          SMALLINT UNSIGNED NULL,
    DefaultSets          SMALLINT UNSIGNED NULL,
    DefaultHoldSeconds   SMALLINT UNSIGNED NULL,
    CreatedDate          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    IsActive             BOOLEAN NOT NULL DEFAULT TRUE,
    ImageUrl             VARCHAR(500) NULL,
    VideoUrl             VARCHAR(500) NULL,
    CONSTRAINT PK_Exercise PRIMARY KEY (ExerciseId),
    CONSTRAINT UQ_Exercise_ExerciseKey UNIQUE (ExerciseKey),
    CONSTRAINT FK_Exercise_ExerciseCategory
        FOREIGN KEY (ExerciseCategoryId) REFERENCES ExerciseCategory (ExerciseCategoryId)
        ON DELETE SET NULL
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_Exercise_ExerciseCategoryId ON Exercise (ExerciseCategoryId);
CREATE INDEX IX_Exercise_IsActive ON Exercise (IsActive);

-- ============================================================
-- 15. PROGRAMME
-- Parent business record linked to TreatmentCase.
-- CurrentProgrammeVersionId added after ProgrammeVersion is created.
-- ============================================================
CREATE TABLE Programme (
    ProgrammeId               BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    TreatmentCaseId           BIGINT UNSIGNED NOT NULL,
    ProgrammeTemplateId       BIGINT UNSIGNED NULL,
    ProgrammeName             VARCHAR(255) NOT NULL,
    StartDate                 DATE NOT NULL,
    EndDate                   DATE NULL,
    Status                    ENUM('planned', 'active', 'completed', 'cancelled') NOT NULL DEFAULT 'planned',
    IsCurrent                 BOOLEAN NOT NULL DEFAULT TRUE,
    CurrentProgrammeVersionId BIGINT UNSIGNED NULL,
    Notes                     TEXT NULL,
    CreatedDate               DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UpdatedDate               DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT PK_Programme PRIMARY KEY (ProgrammeId),
    CONSTRAINT FK_Programme_TreatmentCase
        FOREIGN KEY (TreatmentCaseId) REFERENCES TreatmentCase (TreatmentCaseId)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT FK_Programme_ProgrammeTemplate
        FOREIGN KEY (ProgrammeTemplateId) REFERENCES ProgrammeTemplate (ProgrammeTemplateId)
        ON DELETE SET NULL
        ON UPDATE CASCADE,
    CONSTRAINT CHK_Programme_Dates CHECK (EndDate IS NULL OR EndDate >= StartDate)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_Programme_TreatmentCaseId ON Programme (TreatmentCaseId);
CREATE INDEX IX_Programme_ProgrammeTemplateId ON Programme (ProgrammeTemplateId);
CREATE INDEX IX_Programme_Status ON Programme (Status);
CREATE INDEX IX_Programme_IsCurrent ON Programme (IsCurrent);

-- ============================================================
-- 16. SESSION
-- Authoring table used to compose a programme before / alongside publishing.
-- ============================================================
CREATE TABLE Session (
    SessionId            BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    ProgrammeId          BIGINT UNSIGNED NOT NULL,
    CreatedDate          DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    Period               ENUM('single', 'AM', 'PM') NOT NULL,
    Objective            TEXT NULL,
    Status               ENUM('planned', 'active', 'completed', 'cancelled') NOT NULL DEFAULT 'planned',
    SortOrder            TINYINT UNSIGNED NOT NULL DEFAULT 1,
    CONSTRAINT PK_Session PRIMARY KEY (SessionId),
    CONSTRAINT UQ_Session_Programme_Period UNIQUE (ProgrammeId, Period),
    CONSTRAINT FK_Session_Programme
        FOREIGN KEY (ProgrammeId) REFERENCES Programme (ProgrammeId)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_Session_ProgrammeId ON Session (ProgrammeId);
CREATE INDEX IX_Session_Status ON Session (Status);

-- ============================================================
-- 17. SESSION EXERCISE
-- Authoring/link table for exercise selection inside a session.
-- ============================================================
CREATE TABLE SessionExercise (
    SessionExerciseId    BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    SessionId            BIGINT UNSIGNED NOT NULL,
    ExerciseId           BIGINT UNSIGNED NOT NULL,
    Reps                 SMALLINT UNSIGNED NULL,
    Sets                 SMALLINT UNSIGNED NULL,
    HoldSeconds          SMALLINT UNSIGNED NULL,
    SortOrder            SMALLINT UNSIGNED NOT NULL DEFAULT 1,
    Notes                TEXT NULL,
    CONSTRAINT PK_SessionExercise PRIMARY KEY (SessionExerciseId),
    CONSTRAINT UQ_SessionExercise_Session_Exercise UNIQUE (SessionId, ExerciseId),
    CONSTRAINT FK_SessionExercise_Session
        FOREIGN KEY (SessionId) REFERENCES Session (SessionId)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT FK_SessionExercise_Exercise
        FOREIGN KEY (ExerciseId) REFERENCES Exercise (ExerciseId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_SessionExercise_SessionId ON SessionExercise (SessionId);
CREATE INDEX IX_SessionExercise_ExerciseId ON SessionExercise (ExerciseId);

-- ============================================================
-- 18. PROGRAMME VERSION
-- Immutable, versioned JSON snapshot for mobile delivery and history.
-- One Programme can have many versions.
-- ============================================================
CREATE TABLE ProgrammeVersion (
    ProgrammeVersionId     BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    ProgrammeId            BIGINT UNSIGNED NOT NULL,
    VersionNumber          INT UNSIGNED NOT NULL,
    VersionStatus          ENUM('draft', 'published', 'superseded', 'retired') NOT NULL DEFAULT 'draft',
    PayloadJson            JSON NOT NULL,
    PayloadSchemaVersion   VARCHAR(50) NOT NULL DEFAULT '1.0',
    ChangeSummary          VARCHAR(500) NULL,
    CreatedByPractitionerId BIGINT UNSIGNED NOT NULL,
    CreatedDate            DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PublishedDate          DATETIME NULL,
    SupersededDate         DATETIME NULL,
    RetiredDate            DATETIME NULL,
    CONSTRAINT PK_ProgrammeVersion PRIMARY KEY (ProgrammeVersionId),
    CONSTRAINT UQ_ProgrammeVersion_Programme_Version UNIQUE (ProgrammeId, VersionNumber),
    CONSTRAINT FK_ProgrammeVersion_Programme
        FOREIGN KEY (ProgrammeId) REFERENCES Programme (ProgrammeId)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT FK_ProgrammeVersion_Practitioner
        FOREIGN KEY (CreatedByPractitionerId) REFERENCES Practitioner (PractitionerId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,
    CONSTRAINT CHK_ProgrammeVersion_Published
        CHECK (
            (VersionStatus = 'published' AND PublishedDate IS NOT NULL)
            OR (VersionStatus <> 'published')
        )
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_ProgrammeVersion_ProgrammeId ON ProgrammeVersion (ProgrammeId);
CREATE INDEX IX_ProgrammeVersion_Status ON ProgrammeVersion (VersionStatus);
CREATE INDEX IX_ProgrammeVersion_CreatedByPractitionerId ON ProgrammeVersion (CreatedByPractitionerId);
CREATE INDEX IX_ProgrammeVersion_PublishedDate ON ProgrammeVersion (PublishedDate);

ALTER TABLE Programme
    ADD CONSTRAINT FK_Programme_CurrentProgrammeVersion
        FOREIGN KEY (CurrentProgrammeVersionId) REFERENCES ProgrammeVersion (ProgrammeVersionId)
        ON DELETE SET NULL
        ON UPDATE CASCADE;

CREATE INDEX IX_Programme_CurrentProgrammeVersionId ON Programme (CurrentProgrammeVersionId);

-- ============================================================
-- 19. SESSION OCCURRENCE
-- Historical execution event tied to the programme version that was active
-- on the device when the occurrence was recorded.
-- SessionId retained as an optional authoring/link reference.
-- Period retained as a historical snapshot value.
-- ============================================================
CREATE TABLE SessionOccurrence (
    SessionOccurrenceId   BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    ProgrammeVersionId    BIGINT UNSIGNED NOT NULL,
    SessionId             BIGINT UNSIGNED NULL,
    PetId                 BIGINT UNSIGNED NOT NULL,
    ScheduledDate         DATE NOT NULL,
    Period                ENUM('single', 'AM', 'PM') NOT NULL,
    Status                ENUM('planned', 'active', 'completed', 'skipped', 'cancelled') NOT NULL DEFAULT 'planned',
    DeviceRecordedDateTime DATETIME NULL,
    StartedDateTime       DATETIME NULL,
    CompletedDateTime     DATETIME NULL,
    SkippedDateTime       DATETIME NULL,
    SyncedDateTime        DATETIME NULL,
    Comments              TEXT NULL,
    CONSTRAINT PK_SessionOccurrence PRIMARY KEY (SessionOccurrenceId),
    CONSTRAINT UQ_SessionOccurrence_Pet_ProgrammeVersion_Date_Period
        UNIQUE (PetId, ProgrammeVersionId, ScheduledDate, Period),
    CONSTRAINT FK_SessionOccurrence_ProgrammeVersion
        FOREIGN KEY (ProgrammeVersionId) REFERENCES ProgrammeVersion (ProgrammeVersionId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE,
    CONSTRAINT FK_SessionOccurrence_Session
        FOREIGN KEY (SessionId) REFERENCES Session (SessionId)
        ON DELETE SET NULL
        ON UPDATE CASCADE,
    CONSTRAINT FK_SessionOccurrence_Pet
        FOREIGN KEY (PetId) REFERENCES Pet (PetId)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_SessionOccurrence_ProgrammeVersionId ON SessionOccurrence (ProgrammeVersionId);
CREATE INDEX IX_SessionOccurrence_SessionId ON SessionOccurrence (SessionId);
CREATE INDEX IX_SessionOccurrence_PetId ON SessionOccurrence (PetId);
CREATE INDEX IX_SessionOccurrence_ScheduledDate ON SessionOccurrence (ScheduledDate);
CREATE INDEX IX_SessionOccurrence_Status ON SessionOccurrence (Status);

-- ============================================================
-- 20. SESSION SKIP REASON
-- ============================================================
CREATE TABLE SessionSkipReason (
    SessionSkipReasonId  BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    ReasonName           VARCHAR(100) NOT NULL,
    Description          TEXT NULL,
    IsActive             BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT PK_SessionSkipReason PRIMARY KEY (SessionSkipReasonId),
    CONSTRAINT UQ_SessionSkipReason_ReasonName UNIQUE (ReasonName)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_SessionSkipReason_IsActive ON SessionSkipReason (IsActive);

-- ============================================================
-- 21. SESSION SKIP
-- ============================================================
CREATE TABLE SessionSkip (
    SessionSkipId         BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    SessionOccurrenceId   BIGINT UNSIGNED NOT NULL,
    SessionSkipReasonId   BIGINT UNSIGNED NOT NULL,
    Comments              TEXT NULL,
    CreatedDate           DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_SessionSkip PRIMARY KEY (SessionSkipId),
    CONSTRAINT UQ_SessionSkip_SessionOccurrenceId UNIQUE (SessionOccurrenceId),
    CONSTRAINT FK_SessionSkip_SessionOccurrence
        FOREIGN KEY (SessionOccurrenceId) REFERENCES SessionOccurrence (SessionOccurrenceId)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT FK_SessionSkip_SessionSkipReason
        FOREIGN KEY (SessionSkipReasonId) REFERENCES SessionSkipReason (SessionSkipReasonId)
        ON DELETE RESTRICT
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_SessionSkip_SessionSkipReasonId ON SessionSkip (SessionSkipReasonId);

-- ============================================================
-- 22. EXERCISE COMPLETION
-- Stores exercise-level completion synced from mobile.
-- ExerciseKeySnapshot is the durable identifier from the published
-- programme version payload. SessionExerciseId is optional.
-- ============================================================
CREATE TABLE ExerciseCompletion (
    ExerciseCompletionId   BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    SessionOccurrenceId    BIGINT UNSIGNED NOT NULL,
    SessionExerciseId      BIGINT UNSIGNED NULL,
    ExerciseKeySnapshot    VARCHAR(100) NOT NULL,
    ExerciseTitleSnapshot  VARCHAR(255) NULL,
    CompletedDateTime      DATETIME NULL,
    DeviceRecordedDateTime DATETIME NULL,
    SyncedDateTime         DATETIME NULL,
    RepsCompleted          SMALLINT UNSIGNED NULL,
    SetsCompleted          SMALLINT UNSIGNED NULL,
    PainScore              TINYINT UNSIGNED NULL,
    Comments               TEXT NULL,
    CompletionStatus       ENUM('completed', 'partial', 'not_done') NOT NULL DEFAULT 'completed',
    CONSTRAINT PK_ExerciseCompletion PRIMARY KEY (ExerciseCompletionId),
    CONSTRAINT UQ_ExerciseCompletion_Occurrence_ExerciseKey
        UNIQUE (SessionOccurrenceId, ExerciseKeySnapshot),
    CONSTRAINT FK_ExerciseCompletion_SessionOccurrence
        FOREIGN KEY (SessionOccurrenceId) REFERENCES SessionOccurrence (SessionOccurrenceId)
        ON DELETE CASCADE
        ON UPDATE CASCADE,
    CONSTRAINT FK_ExerciseCompletion_SessionExercise
        FOREIGN KEY (SessionExerciseId) REFERENCES SessionExercise (SessionExerciseId)
        ON DELETE SET NULL
        ON UPDATE CASCADE,
    CONSTRAINT CHK_ExerciseCompletion_PainScore CHECK (PainScore IS NULL OR PainScore <= 10)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_ExerciseCompletion_SessionOccurrenceId ON ExerciseCompletion (SessionOccurrenceId);
CREATE INDEX IX_ExerciseCompletion_SessionExerciseId ON ExerciseCompletion (SessionExerciseId);
CREATE INDEX IX_ExerciseCompletion_CompletionStatus ON ExerciseCompletion (CompletionStatus);
CREATE INDEX IX_ExerciseCompletion_ExerciseKeySnapshot ON ExerciseCompletion (ExerciseKeySnapshot);

-- ============================================================
-- 23. NOTIFICATION PREFERENCE
-- One row per user account.
-- ============================================================
CREATE TABLE NotificationPreference (
    NotificationPreferenceId BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    UserAccountId            BIGINT UNSIGNED NOT NULL,
    NotificationsEnabled     BOOLEAN NOT NULL DEFAULT TRUE,
    NotificationTime         TIME NULL,
    DownloadVideosEnabled    BOOLEAN NOT NULL DEFAULT FALSE,
    OfflineCachingEnabled    BOOLEAN NOT NULL DEFAULT FALSE,
    UpdatedDate              DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT PK_NotificationPreference PRIMARY KEY (NotificationPreferenceId),
    CONSTRAINT UQ_NotificationPreference_UserAccountId UNIQUE (UserAccountId),
    CONSTRAINT FK_NotificationPreference_UserAccount
        FOREIGN KEY (UserAccountId) REFERENCES UserAccount (UserAccountId)
        ON DELETE CASCADE
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_NotificationPreference_UserAccountId ON NotificationPreference (UserAccountId);

-- ============================================================
-- 24. AUDIT LOG
-- ============================================================
CREATE TABLE AuditLog (
    AuditLogId           BIGINT UNSIGNED NOT NULL AUTO_INCREMENT,
    PractitionerId       BIGINT UNSIGNED NULL,
    UserAccountId        BIGINT UNSIGNED NULL,
    EntityName           VARCHAR(100) NOT NULL,
    EntityId             BIGINT UNSIGNED NOT NULL,
    ActionType           VARCHAR(50) NOT NULL,
    OldValuesJson        JSON NULL,
    NewValuesJson        JSON NULL,
    ActionDateTime       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT PK_AuditLog PRIMARY KEY (AuditLogId),
    CONSTRAINT FK_AuditLog_Practitioner
        FOREIGN KEY (PractitionerId) REFERENCES Practitioner (PractitionerId)
        ON DELETE SET NULL
        ON UPDATE CASCADE,
    CONSTRAINT FK_AuditLog_UserAccount
        FOREIGN KEY (UserAccountId) REFERENCES UserAccount (UserAccountId)
        ON DELETE SET NULL
        ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

CREATE INDEX IX_AuditLog_PractitionerId ON AuditLog (PractitionerId);
CREATE INDEX IX_AuditLog_UserAccountId ON AuditLog (UserAccountId);
CREATE INDEX IX_AuditLog_EntityName_EntityId ON AuditLog (EntityName, EntityId);
CREATE INDEX IX_AuditLog_ActionDateTime ON AuditLog (ActionDateTime);

SET FOREIGN_KEY_CHECKS = 1;
