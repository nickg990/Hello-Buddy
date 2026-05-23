USE canine_physiotherapy;

START TRANSACTION;

-- ============================================================
-- USER JOURNEY 1: SIGNUP
-- Purpose:
--   Register a new owner in the admin system
--   Create their login account
--   Create their pet record
--   Create default notification preferences
-- ============================================================

-- ------------------------------------------------------------
-- 1. CREATE OWNER
-- ------------------------------------------------------------
INSERT INTO Owner (
    FirstName,
    LastName,
    Email,
    PhoneNumber,
    AddressLine1,
    AddressLine2,
    Town,
    Postcode
) VALUES (
    'Emma',
    'Thompson',
    'emma.thompson@example.com',
    '07700 900201',
    '14 Willow Close',
    NULL,
    'Leighton Buzzard',
    'LU7 2AB'
);

SET @OwnerId = LAST_INSERT_ID();

-- ------------------------------------------------------------
-- 2. CREATE USER ACCOUNT
-- Note:
--   PasswordHash is example seed data for dry-run purposes only.
-- ------------------------------------------------------------
INSERT INTO UserAccount (
    OwnerId,
    Email,
    PasswordHash,
    PasswordSalt,
    IsActive
) VALUES (
    @OwnerId,
    'emma.thompson@example.com',
    'HASH_PLACEHOLDER_EMMA_001',
    'SALT_PLACEHOLDER_EMMA_001',
    TRUE
);

SET @UserAccountId = LAST_INSERT_ID();

-- ------------------------------------------------------------
-- 3. CREATE DEFAULT NOTIFICATION PREFERENCES
-- ------------------------------------------------------------
INSERT INTO NotificationPreference (
    UserAccountId,
    NotificationsEnabled,
    NotificationTime,
    DownloadVideosEnabled,
    OfflineCachingEnabled
) VALUES (
    @UserAccountId,
    TRUE,
    '08:00:00',
    TRUE,
    TRUE
);

-- ------------------------------------------------------------
-- 4. CREATE PET
-- Age retained because DOB may be unknown.
-- ------------------------------------------------------------
INSERT INTO Pet (
    OwnerId,
    Name,
    Age,
    DateOfBirth,
    Breed,
    Sex,
    Weight,
    IsActive
) VALUES (
    @OwnerId,
    'Buddy',
    8,
    NULL,
    'Labrador Retriever',
    'male',
    28.50,
    TRUE
);

SET @PetId = LAST_INSERT_ID();

COMMIT;

-- ============================================================
-- VERIFICATION REPORT
-- ============================================================
SELECT
    o.OwnerId,
    CONCAT(o.FirstName, ' ', o.LastName) AS OwnerName,
    o.Email AS OwnerEmail,
    o.PhoneNumber,
    p.PetId,
    p.Name AS PetName,
    p.Age,
    p.DateOfBirth,
    p.Breed,
    p.Sex,
    p.Weight,
    ua.UserAccountId,
    ua.Email AS LoginEmail,
    CASE
        WHEN ua.IsActive = TRUE THEN 'Active'
        ELSE 'Inactive'
    END AS AccountStatus,
    np.NotificationTime,
    np.NotificationsEnabled,
    np.DownloadVideosEnabled,
    np.OfflineCachingEnabled
FROM Owner o
JOIN UserAccount ua
    ON ua.OwnerId = o.OwnerId
JOIN Pet p
    ON p.OwnerId = o.OwnerId
LEFT JOIN NotificationPreference np
    ON np.UserAccountId = ua.UserAccountId
WHERE o.OwnerId = @OwnerId;
