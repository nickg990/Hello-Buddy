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

SELECT
    o.OwnerId,
    CONCAT(o.FirstName, ' ', o.LastName) AS OwnerName,
    o.Email AS OwnerEmail,
    p.PetId,
    p.Name AS PetName,
    p.Age,
    p.Breed,
    p.Sex,
    p.Weight,
    ua.UserAccountId,
    ua.Email AS LoginEmail,
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
