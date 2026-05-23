USE canine_physiotherapy;

-- ============================================================
-- CANINE PHYSIOTHERAPY - DAY 1 SEED DATA V2.1
-- Purpose:
--   Initial practitioner-side setup only.
--   No owners, pets, treatment cases, programmes, or mobile history yet.
--
-- Seeds:
--   1) Two practitioners
--   2) Exercise categories
--   3) Exercise library records suitable for later AM / PM / single-session programmes
--   4) Session skip reasons (reference data for later sync testing)
-- ============================================================

SET NAMES utf8mb4;
START TRANSACTION;

-- ============================================================
-- 1. PRACTITIONERS
-- ============================================================
INSERT INTO Practitioner (
    FirstName,
    LastName,
    Email,
    PhoneNumber,
    IsActive
) VALUES
    ('Amelia', 'Carter', 'amelia.carter@caninephysio.local', '07700 900101', TRUE),
    ('James', 'Holloway', 'james.holloway@caninephysio.local', '07700 900102', TRUE);

-- ============================================================
-- 2. EXERCISE CATEGORIES
-- ============================================================
INSERT INTO ExerciseCategory (
    CategoryKey,
    CategoryName,
    Description,
    IsActive
) VALUES
    ('mobility', 'Mobility', 'Exercises focused on joint mobility, flexibility, and controlled range of motion.', TRUE),
    ('strengthening', 'Strengthening', 'Exercises intended to improve muscular strength, endurance, and stability.', TRUE),
    ('balance_proprioception', 'Balance and Proprioception', 'Exercises designed to improve balance, coordination, and limb awareness.', TRUE),
    ('functional_rehab', 'Functional Rehabilitation', 'Exercises supporting return to normal movement and day-to-day canine function.', TRUE);

-- ============================================================
-- 3. EXERCISE LIBRARY
-- Notes:
--   ExerciseKey is stable and can be copied into ProgrammeVersion payloads.
--   The mix below gives us enough variety later to create:
--     - AM session programmes
--     - PM session programmes
--     - single daily session programmes
-- ============================================================
INSERT INTO Exercise (
    ExerciseCategoryId,
    ExerciseKey,
    Title,
    ObjectiveSummary,
    InstructionsText,
    DefaultReps,
    DefaultSets,
    DefaultHoldSeconds,
    CreatedDate,
    IsActive,
    ImageUrl,
    VideoUrl
)
SELECT ExerciseCategoryId, 'sit_to_stand', 'Sit to Stand',
       'Improves hind limb strength and controlled transition between positions.',
       'Ask the dog to sit squarely, then encourage a controlled stand. Keep the movement slow and straight. Stop if compensatory twisting appears.',
       8, 2, NULL, CURRENT_TIMESTAMP, TRUE,
       'https://cdn.caninephysio.local/exercises/sit-to-stand.jpg',
       'https://cdn.caninephysio.local/exercises/sit-to-stand.mp4'
FROM ExerciseCategory WHERE CategoryKey = 'strengthening'
UNION ALL
SELECT ExerciseCategoryId, 'weight_shift_stand', 'Weight Shift in Standing',
       'Encourages controlled loading through all four limbs and improves postural stability.',
       'With the dog standing square, gently encourage weight transfer from side to side and forwards/backwards while maintaining a calm, supported stance.',
       10, 2, NULL, CURRENT_TIMESTAMP, TRUE,
       'https://cdn.caninephysio.local/exercises/weight-shift-stand.jpg',
       'https://cdn.caninephysio.local/exercises/weight-shift-stand.mp4'
FROM ExerciseCategory WHERE CategoryKey = 'balance_proprioception'
UNION ALL
SELECT ExerciseCategoryId, 'cavaletti_walk', 'Cavaletti Walk',
       'Improves limb flexion, coordination, and controlled stepping pattern.',
       'Lead the dog slowly over evenly spaced low poles. Keep pace slow and deliberate. Pause if the dog rushes, clips poles, or loses form.',
       6, 2, NULL, CURRENT_TIMESTAMP, TRUE,
       'https://cdn.caninephysio.local/exercises/cavaletti-walk.jpg',
       'https://cdn.caninephysio.local/exercises/cavaletti-walk.mp4'
FROM ExerciseCategory WHERE CategoryKey = 'functional_rehab'
UNION ALL
SELECT ExerciseCategoryId, 'cookie_stretch_left', 'Cookie Stretch Left',
       'Supports cervical and trunk flexibility with controlled active movement to the left.',
       'Using a treat, guide the dog to turn nose toward the left shoulder or flank within a comfortable range. Avoid forcing the movement.',
       5, 1, 5, CURRENT_TIMESTAMP, TRUE,
       'https://cdn.caninephysio.local/exercises/cookie-stretch-left.jpg',
       'https://cdn.caninephysio.local/exercises/cookie-stretch-left.mp4'
FROM ExerciseCategory WHERE CategoryKey = 'mobility'
UNION ALL
SELECT ExerciseCategoryId, 'cookie_stretch_right', 'Cookie Stretch Right',
       'Supports cervical and trunk flexibility with controlled active movement to the right.',
       'Using a treat, guide the dog to turn nose toward the right shoulder or flank within a comfortable range. Avoid forcing the movement.',
       5, 1, 5, CURRENT_TIMESTAMP, TRUE,
       'https://cdn.caninephysio.local/exercises/cookie-stretch-right.jpg',
       'https://cdn.caninephysio.local/exercises/cookie-stretch-right.mp4'
FROM ExerciseCategory WHERE CategoryKey = 'mobility'
UNION ALL
SELECT ExerciseCategoryId, 'three_leg_stand_support', 'Three Leg Stand with Support',
       'Promotes balance, controlled loading, and limb confidence with manual support as needed.',
       'While the dog stands calmly, gently support body position and briefly lift one limb as instructed by the practitioner. Keep duration short and controlled.',
       6, 2, NULL, CURRENT_TIMESTAMP, TRUE,
       'https://cdn.caninephysio.local/exercises/three-leg-stand-support.jpg',
       'https://cdn.caninephysio.local/exercises/three-leg-stand-support.mp4'
FROM ExerciseCategory WHERE CategoryKey = 'balance_proprioception'
UNION ALL
SELECT ExerciseCategoryId, 'slow_lead_walk', 'Slow Lead Walk',
       'Encourages even gait pattern, controlled loading, and low-impact functional movement.',
       'Walk the dog slowly on a short lead over a flat surface for the prescribed duration, encouraging steady pace and symmetrical movement.',
       1, 1, NULL, CURRENT_TIMESTAMP, TRUE,
       'https://cdn.caninephysio.local/exercises/slow-lead-walk.jpg',
       'https://cdn.caninephysio.local/exercises/slow-lead-walk.mp4'
FROM ExerciseCategory WHERE CategoryKey = 'functional_rehab'
UNION ALL
SELECT ExerciseCategoryId, 'paw_target_front', 'Front Paw Target',
       'Improves forelimb control, weight placement, and proprioceptive awareness.',
       'Guide the dog to place the front paws onto a stable target or pad. Hold briefly in a balanced stance before stepping off under control.',
       6, 2, NULL, CURRENT_TIMESTAMP, TRUE,
       'https://cdn.caninephysio.local/exercises/front-paw-target.jpg',
       'https://cdn.caninephysio.local/exercises/front-paw-target.mp4'
FROM ExerciseCategory WHERE CategoryKey = 'balance_proprioception';

-- ============================================================
-- 4. SESSION SKIP REASONS
-- Included now so later mobile sync dry-runs can use realistic reference data.
-- ============================================================
INSERT INTO SessionSkipReason (
    ReasonName,
    Description,
    IsActive
) VALUES
    ('Pet tired', 'The pet appeared too tired or fatigued to complete the planned session safely.', TRUE),
    ('Pain flare-up', 'The session was skipped because pain, discomfort, or sensitivity increased.', TRUE),
    ('Owner unavailable', 'The owner was unable to complete the scheduled session.', TRUE),
    ('Pet non-compliant', 'The pet was unwilling, distressed, or unable to cooperate sufficiently.', TRUE),
    ('Practitioner advice', 'Session was skipped based on updated practitioner advice.', TRUE);

COMMIT;

-- ============================================================
-- OPTIONAL VERIFICATION QUERIES
-- ============================================================
SELECT PractitionerId, FirstName, LastName, Email, IsActive
FROM Practitioner
ORDER BY PractitionerId;

SELECT ec.ExerciseCategoryId, ec.CategoryName, e.ExerciseId, e.ExerciseKey, e.Title, e.DefaultReps, e.DefaultSets, e.DefaultHoldSeconds
FROM Exercise e
JOIN ExerciseCategory ec
  ON ec.ExerciseCategoryId = e.ExerciseCategoryId
ORDER BY ec.CategoryName, e.Title;

SELECT SessionSkipReasonId, ReasonName, IsActive
FROM SessionSkipReason
ORDER BY SessionSkipReasonId;
