-- Hello Buddy exercise library import.
-- GENERATED FILE - do not edit by hand.
-- Source: exercises.md  Generator: Generate-ExerciseImportSql.ps1
-- Exercises: 144  Categories: 15
--
-- Idempotent: exercises upsert on ExerciseKey, categories on CategoryKey,
-- instructions are rebuilt per exercise. Wrapped in a single transaction.

SET NAMES utf8mb4;
START TRANSACTION;

-- Categories -------------------------------------------------------------
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('pole_and_obstacle_work', 'Pole and Obstacle Work', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('balance_and_weight_shifting', 'Balance and Weight Shifting', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('gait_and_walking', 'Gait and Walking', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('neurological_rehabilitation', 'Neurological Rehabilitation', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('neck_mobilisation', 'Neck Mobilisation', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('strength_and_transitions', 'Strength and Transitions', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('passive_range_of_motion', 'Passive Range of Motion', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('thermal_therapy', 'Thermal Therapy', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('limb_lifts_and_proprioception', 'Limb Lifts and Proprioception', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('massage', 'Massage', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('core_and_posture', 'Core and Posture', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('stretching_and_mobility', 'Stretching and Mobility', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('general_exercise', 'General Exercise', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('neck_stretching', 'Neck Stretching', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;
INSERT INTO ExerciseCategory (CategoryKey, CategoryName, IsActive) VALUES ('step_and_pivot_work', 'Step and Pivot Work', TRUE)
  ON DUPLICATE KEY UPDATE CategoryName=VALUES(CategoryName), IsActive=TRUE;

-- Exercises --------------------------------------------------------------
-- Alternating Poles
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'alternatingPoles', 'Alternating Poles', 'This exercise should be performed on a flat, non-slip surface and at an active walk.', 'This exercise should be performed on a flat, non-slip surface and at an active walk.
Start by raising one pole on one side by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.
You can then gradually add more raised poles until you reach all poles as cross poles.
The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).
Upon completion of the poles, make sure you turn your dog nice and wide.
If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.
If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.', NULL, NULL, NULL, TRUE, 'Raised alternating poles - be354210148b41dfb9890e371600fdb0.jpg', 'Raised alternating poles - be354210148b41dfb9890e371600fdb0.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='alternatingPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and at an active walk.' FROM Exercise e WHERE e.ExerciseKey='alternatingPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Start by raising one pole on one side by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.' FROM Exercise e WHERE e.ExerciseKey='alternatingPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'You can then gradually add more raised poles until you reach all poles as cross poles.' FROM Exercise e WHERE e.ExerciseKey='alternatingPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).' FROM Exercise e WHERE e.ExerciseKey='alternatingPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Upon completion of the poles, make sure you turn your dog nice and wide.' FROM Exercise e WHERE e.ExerciseKey='alternatingPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.' FROM Exercise e WHERE e.ExerciseKey='alternatingPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.' FROM Exercise e WHERE e.ExerciseKey='alternatingPoles';

-- Alternating Poles – Above View
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'alternatingPolesAboveView', 'Alternating Poles – Above View', 'This exercise should be performed on a flat, non-slip surface and at an active walk.', 'This exercise should be performed on a flat, non-slip surface and at an active walk.
Start by raising one pole on one side by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.
You can then gradually add more raised poles until you reach all poles as cross poles.
The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).
Upon completion of the poles, make sure you turn your dog nice and wide.
If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.
If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.', NULL, NULL, NULL, TRUE, 'Raised alternating poles (above) - IMG_0234.jpg', 'Raised alternating poles (above) - IMG_0234.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='alternatingPolesAboveView';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and at an active walk.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesAboveView';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Start by raising one pole on one side by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesAboveView';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'You can then gradually add more raised poles until you reach all poles as cross poles.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesAboveView';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesAboveView';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Upon completion of the poles, make sure you turn your dog nice and wide.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesAboveView';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesAboveView';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesAboveView';

-- Alternating Poles – Alternative View
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'alternatingPolesAlternativeView', 'Alternating Poles – Alternative View', 'This exercise should be performed on a flat, non-slip surface and at an active walk.', 'This exercise should be performed on a flat, non-slip surface and at an active walk.
Start by raising one pole on one side by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.
You can then gradually add more raised poles until you reach all poles as cross poles.
The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).
Upon completion of the poles, make sure you turn your dog nice and wide.
If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.
If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.', NULL, NULL, NULL, TRUE, 'Raised alternating poles - IMG_0233.jpg', 'Raised alternating poles - IMG_0233.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='alternatingPolesAlternativeView';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and at an active walk.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesAlternativeView';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Start by raising one pole on one side by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesAlternativeView';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'You can then gradually add more raised poles until you reach all poles as cross poles.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesAlternativeView';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesAlternativeView';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Upon completion of the poles, make sure you turn your dog nice and wide.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesAlternativeView';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesAlternativeView';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesAlternativeView';

-- Alternating Poles – Slow Motion
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'alternatingPolesSlowMotion', 'Alternating Poles – Slow Motion', 'This exercise should be performed on a flat, non-slip surface and at an active walk.', 'This exercise should be performed on a flat, non-slip surface and at an active walk.
Start by raising one pole on one side by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.
You can then gradually add more raised poles until you reach all poles as cross poles.
The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).
Upon completion of the poles, make sure you turn your dog nice and wide.
If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.
If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.', NULL, NULL, NULL, TRUE, 'Slo-Mo Raised alternating poles - IMG_0231.jpg', 'Slo-Mo Raised alternating poles - IMG_0231.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='alternatingPolesSlowMotion';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and at an active walk.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesSlowMotion';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Start by raising one pole on one side by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesSlowMotion';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'You can then gradually add more raised poles until you reach all poles as cross poles.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesSlowMotion';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesSlowMotion';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Upon completion of the poles, make sure you turn your dog nice and wide.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesSlowMotion';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesSlowMotion';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesSlowMotion';

-- Alternating Poles – Slow Motion Right
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'alternatingPolesSlowMotionRight', 'Alternating Poles – Slow Motion Right', 'This exercise should be performed on a flat, non-slip surface and at an active walk.', 'This exercise should be performed on a flat, non-slip surface and at an active walk.
Start by raising one pole on one side by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.
You can then gradually add more raised poles until you reach all poles as cross poles.
The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).
Upon completion of the poles, make sure you turn your dog nice and wide.
If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.
If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.', NULL, NULL, NULL, TRUE, 'Slo-Mo Raised alternating poles to R - IMG_0235.jpg', 'Slo-Mo Raised alternating poles to R - IMG_0235.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='alternatingPolesSlowMotionRight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and at an active walk.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesSlowMotionRight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Start by raising one pole on one side by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesSlowMotionRight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'You can then gradually add more raised poles until you reach all poles as cross poles.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesSlowMotionRight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesSlowMotionRight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Upon completion of the poles, make sure you turn your dog nice and wide.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesSlowMotionRight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesSlowMotionRight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.' FROM Exercise e WHERE e.ExerciseKey='alternatingPolesSlowMotionRight';

-- Assisted standing
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'assistedStanding', 'Assisted standing', 'Perform this exercise on a flat, non-slip surface.', 'Perform this exercise on a flat, non-slip surface.
Place a comfortable but supportive cushion underneath your dog’s chest and allow them to stand in a square position with all feet flat to the floor.
Remember, we want them to take most of their weight and support themselves.
Use a sling underneath the stomach to gently lift your dog.
Before doing this, make sure you are capable of lifting something equivalent to your dog’s weight.
Ensure to lift with your legs and not your back.
Remember, we want them to take most of their weight to support themselves so make sure they are standing in a square position with all feet flat to the floor.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='assistedStanding';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this exercise on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='assistedStanding';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a comfortable but supportive cushion underneath your dog’s chest and allow them to stand in a square position with all feet flat to the floor.' FROM Exercise e WHERE e.ExerciseKey='assistedStanding';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Remember, we want them to take most of their weight and support themselves.' FROM Exercise e WHERE e.ExerciseKey='assistedStanding';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Use a sling underneath the stomach to gently lift your dog.' FROM Exercise e WHERE e.ExerciseKey='assistedStanding';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Before doing this, make sure you are capable of lifting something equivalent to your dog’s weight.' FROM Exercise e WHERE e.ExerciseKey='assistedStanding';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Ensure to lift with your legs and not your back.' FROM Exercise e WHERE e.ExerciseKey='assistedStanding';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Remember, we want them to take most of their weight to support themselves so make sure they are standing in a square position with all feet flat to the floor.' FROM Exercise e WHERE e.ExerciseKey='assistedStanding';

-- Assisted standing (neuro)
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'assistedStandingNeuro', 'Assisted standing (neuro)', 'Perform this exercise on a flat, non-slip surface.', 'Perform this exercise on a flat, non-slip surface.
Place a comfortable but supportive cushion underneath your dog’s chest and allow them to stand in a square position with all feet flat to the floor.
Remember, we want them to take most of their weight and support themselves.
Use a sling underneath the stomach to gently lift your dog.
Before doing this, make sure you are capable of lifting something equivalent to your dog’s weight.
Ensure to lift with your legs and not your back.
Remember, we want them to take most of their weight to support themselves so make sure they are standing in a square position with all feet flat to the floor.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='assistedStandingNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this exercise on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='assistedStandingNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a comfortable but supportive cushion underneath your dog’s chest and allow them to stand in a square position with all feet flat to the floor.' FROM Exercise e WHERE e.ExerciseKey='assistedStandingNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Remember, we want them to take most of their weight and support themselves.' FROM Exercise e WHERE e.ExerciseKey='assistedStandingNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Use a sling underneath the stomach to gently lift your dog.' FROM Exercise e WHERE e.ExerciseKey='assistedStandingNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Before doing this, make sure you are capable of lifting something equivalent to your dog’s weight.' FROM Exercise e WHERE e.ExerciseKey='assistedStandingNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Ensure to lift with your legs and not your back.' FROM Exercise e WHERE e.ExerciseKey='assistedStandingNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Remember, we want them to take most of their weight to support themselves so make sure they are standing in a square position with all feet flat to the floor.' FROM Exercise e WHERE e.ExerciseKey='assistedStandingNeuro';

-- Backing up
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'backingUp', 'Backing up', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Start with your dog standing square and facing you.
Then, ask them to take a few steps backwards in a straight line.
You can do this by either taking a treat in hand and pushing back against their nose or holding it above their head and walking towards them.
The hindlimbs should be clearing the ground when backing up.
You can use a narrow space to keep them straight (i.e., a hallway).', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='gait_and_walking'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='backingUp';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='backingUp';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Start with your dog standing square and facing you.' FROM Exercise e WHERE e.ExerciseKey='backingUp';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Then, ask them to take a few steps backwards in a straight line.' FROM Exercise e WHERE e.ExerciseKey='backingUp';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'You can do this by either taking a treat in hand and pushing back against their nose or holding it above their head and walking towards them.' FROM Exercise e WHERE e.ExerciseKey='backingUp';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'The hindlimbs should be clearing the ground when backing up.' FROM Exercise e WHERE e.ExerciseKey='backingUp';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'You can use a narrow space to keep them straight (i.e., a hallway).' FROM Exercise e WHERE e.ExerciseKey='backingUp';

-- Backing Up on decline
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'backingUpOnDecline', 'Backing Up on decline', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Start with your dog standing square and facing you.
Then, ask them to take a few steps backwards in a straight line down the slope.
You can do this by either taking a treat in hand and pushing back against their nose or holding it above their head and walking towards them.
The hindlimbs should be clearing the ground when backing up and always be at the lowest point.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='gait_and_walking'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='backingUpOnDecline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='backingUpOnDecline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Start with your dog standing square and facing you.' FROM Exercise e WHERE e.ExerciseKey='backingUpOnDecline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Then, ask them to take a few steps backwards in a straight line down the slope.' FROM Exercise e WHERE e.ExerciseKey='backingUpOnDecline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'You can do this by either taking a treat in hand and pushing back against their nose or holding it above their head and walking towards them.' FROM Exercise e WHERE e.ExerciseKey='backingUpOnDecline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'The hindlimbs should be clearing the ground when backing up and always be at the lowest point.' FROM Exercise e WHERE e.ExerciseKey='backingUpOnDecline';

-- Backward hair brushing
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'backwardHairBrushing', 'Backward hair brushing', 'Lay your dog in a neutral position flat on their side with hindlimbs placed to the side rather than behind them.', 'Lay your dog in a neutral position flat on their side with hindlimbs placed to the side rather than behind them.
Gently brush their hair backwards up towards their head starting around the hindlimbs toes.
Do this in random patterns to stimulate nerves.
Make sure to perform this on both sides.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neurological_rehabilitation'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='backwardHairBrushing';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Lay your dog in a neutral position flat on their side with hindlimbs placed to the side rather than behind them.' FROM Exercise e WHERE e.ExerciseKey='backwardHairBrushing';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Gently brush their hair backwards up towards their head starting around the hindlimbs toes.' FROM Exercise e WHERE e.ExerciseKey='backwardHairBrushing';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Do this in random patterns to stimulate nerves.' FROM Exercise e WHERE e.ExerciseKey='backwardHairBrushing';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Make sure to perform this on both sides.' FROM Exercise e WHERE e.ExerciseKey='backwardHairBrushing';

-- Baited neck mobilisations – Cranial / Step Ups
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'baitedNeckMobilisationsCranialStepUps', 'Baited neck mobilisations – Cranial / Step Ups', 'This exercise should be performed on a flat, stable surface with a treat in hand.', 'This exercise should be performed on a flat, stable surface with a treat in hand.
This action should be relatively quick, without allowing them to move their limbs, and does not require a hold.
Cranial/Step ups: Take your hand in front of your dog’s nose and guide their head forwards and then back to neutral.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_mobilisation'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='baitedNeckMobilisationsCranialStepUps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, stable surface with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='baitedNeckMobilisationsCranialStepUps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'This action should be relatively quick, without allowing them to move their limbs, and does not require a hold.' FROM Exercise e WHERE e.ExerciseKey='baitedNeckMobilisationsCranialStepUps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Cranial/Step ups: Take your hand in front of your dog’s nose and guide their head forwards and then back to neutral.' FROM Exercise e WHERE e.ExerciseKey='baitedNeckMobilisationsCranialStepUps';

-- Baited neck mobilisations – Lateral
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'baitedNeckMobilisationsLateral', 'Baited neck mobilisations – Lateral', 'This exercise should be performed on a flat, stable surface with a treat in hand.', 'This exercise should be performed on a flat, stable surface with a treat in hand.
This action should be relatively quick, without allowing them to move their limbs, and does not require a hold.
Lateral: Take your hand from your dog’s nose from side to side in a relatively quick motion without allowing them to move their feet.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_mobilisation'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='baitedNeckMobilisationsLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, stable surface with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='baitedNeckMobilisationsLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'This action should be relatively quick, without allowing them to move their limbs, and does not require a hold.' FROM Exercise e WHERE e.ExerciseKey='baitedNeckMobilisationsLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Lateral: Take your hand from your dog’s nose from side to side in a relatively quick motion without allowing them to move their feet.' FROM Exercise e WHERE e.ExerciseKey='baitedNeckMobilisationsLateral';

-- Baited neck mobilisations – Ventral
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'baitedNeckMobilisationsVentral', 'Baited neck mobilisations – Ventral', 'This exercise should be performed on a flat, stable surface with a treat in hand.', 'This exercise should be performed on a flat, stable surface with a treat in hand.
This action should be relatively quick, without allowing them to move their limbs, and does not require a hold.
Ventral: Encourage their head down (to the level specified by your physiotherapist) without your dog laying down by moving a treat between the forelimbs from the level of their nose down.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_mobilisation'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='baitedNeckMobilisationsVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, stable surface with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='baitedNeckMobilisationsVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'This action should be relatively quick, without allowing them to move their limbs, and does not require a hold.' FROM Exercise e WHERE e.ExerciseKey='baitedNeckMobilisationsVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Ventral: Encourage their head down (to the level specified by your physiotherapist) without your dog laying down by moving a treat between the forelimbs from the level of their nose down.' FROM Exercise e WHERE e.ExerciseKey='baitedNeckMobilisationsVentral';

-- Beg
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'beg', 'Beg', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Ask for a square sit then gradually raise a treat/toy upwards to encourage their head to look up.
Continue to raise the reward slowly to encourage them to lift their forelimbs and sit in a beg position.
Make sure the spine is neutral and not twisted/bowed in any way.
Hold this position for a few seconds then allow them to resume the sit position before repeating.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='beg';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='beg';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask for a square sit then gradually raise a treat/toy upwards to encourage their head to look up.' FROM Exercise e WHERE e.ExerciseKey='beg';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Continue to raise the reward slowly to encourage them to lift their forelimbs and sit in a beg position.' FROM Exercise e WHERE e.ExerciseKey='beg';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Make sure the spine is neutral and not twisted/bowed in any way.' FROM Exercise e WHERE e.ExerciseKey='beg';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Hold this position for a few seconds then allow them to resume the sit position before repeating.' FROM Exercise e WHERE e.ExerciseKey='beg';

-- Bicycle PROM
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'bicycleProm', 'Bicycle PROM', 'Try to stand your dog on a flat non-slip surface and give them plenty of support. Alternatively, ask them to lay flat on their side with their limbs in a neutral position (i.e., not tucked under themselves or rotated in any way). Keep the limb close to the body and in a straight line without any twisting. Take your dog''s joints in a forward and back motion making sure not to push them past their capabilities. Remember to do this gently as forcing a joint can cause damage. Remember, slow and gentle and watch for signs of discomfort (tensing muscles or pulling the leg away). If they do this, try to reduce the degree of motion you''re asking for (ie. move the joint less).', 'Bicycle: Take their limb in a circular pattern similar to a bicycle pedal action.
Do this gently and within their capacity as pushing too hard can damage joints.
Make sure to grip the lower hindlimb below the hock (ankle) and support the stifle/knee with your other hand.
For the forelimbs, grip the limb around the carpus (wrist) and stabilise the elbow with your other hand.', NULL, NULL, NULL, TRUE, 'LR HL bicycle PROM - e4ea105b559a4effa1e5641ee1f9b223.jpg', 'LR HL bicycle PROM - e4ea105b559a4effa1e5641ee1f9b223.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='passive_range_of_motion'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='bicycleProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Bicycle: Take their limb in a circular pattern similar to a bicycle pedal action.' FROM Exercise e WHERE e.ExerciseKey='bicycleProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Do this gently and within their capacity as pushing too hard can damage joints.' FROM Exercise e WHERE e.ExerciseKey='bicycleProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Make sure to grip the lower hindlimb below the hock (ankle) and support the stifle/knee with your other hand.' FROM Exercise e WHERE e.ExerciseKey='bicycleProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'For the forelimbs, grip the limb around the carpus (wrist) and stabilise the elbow with your other hand.' FROM Exercise e WHERE e.ExerciseKey='bicycleProm';

-- Bicycle PROM – Stood Hindlimb Variant
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'bicyclePromStoodHindlimbVariant', 'Bicycle PROM – Stood Hindlimb Variant', 'Try to stand your dog on a flat non-slip surface and give them plenty of support. Alternatively, ask them to lay flat on their side with their limbs in a neutral position (i.e., not tucked under themselves or rotated in any way). Keep the limb close to the body and in a straight line without any twisting. Take your dog''s joints in a forward and back motion making sure not to push them past their capabilities. Remember to do this gently as forcing a joint can cause damage. Remember, slow and gentle and watch for signs of discomfort (tensing muscles or pulling the leg away). If they do this, try to reduce the degree of motion you''re asking for (ie. move the joint less).', 'Bicycle: Take their limb in a circular pattern similar to a bicycle pedal action.
Do this gently and within their capacity as pushing too hard can damage joints.
Make sure to grip the lower hindlimb below the hock (ankle) and support the stifle/knee with your other hand.
For the forelimbs, grip the limb around the carpus (wrist) and stabilise the elbow with your other hand.', NULL, NULL, NULL, TRUE, 'Stood HL bicycle and DV PROM - da001329-74c0-4217-8514-c11507de42c1.jpg', 'Stood HL bicycle and DV PROM - da001329-74c0-4217-8514-c11507de42c1.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='passive_range_of_motion'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='bicyclePromStoodHindlimbVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Bicycle: Take their limb in a circular pattern similar to a bicycle pedal action.' FROM Exercise e WHERE e.ExerciseKey='bicyclePromStoodHindlimbVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Do this gently and within their capacity as pushing too hard can damage joints.' FROM Exercise e WHERE e.ExerciseKey='bicyclePromStoodHindlimbVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Make sure to grip the lower hindlimb below the hock (ankle) and support the stifle/knee with your other hand.' FROM Exercise e WHERE e.ExerciseKey='bicyclePromStoodHindlimbVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'For the forelimbs, grip the limb around the carpus (wrist) and stabilise the elbow with your other hand.' FROM Exercise e WHERE e.ExerciseKey='bicyclePromStoodHindlimbVariant';

-- Bow
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'bow', 'Bow', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Start with a square stand and ask the dog to remain stationary with their hindlimbs.
Then guide your dog’s nose towards the floor to encourage them into a bow position without laying fully to the floor.
If needed, place a supportive hand under the stomach to prevent full lay.
Progression (do not progress unless told to by a physiotherapist):
Add a block/cushion underneath hindlimbs and/or forelimbs', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='bow';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='bow';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Start with a square stand and ask the dog to remain stationary with their hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='bow';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Then guide your dog’s nose towards the floor to encourage them into a bow position without laying fully to the floor.' FROM Exercise e WHERE e.ExerciseKey='bow';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'If needed, place a supportive hand under the stomach to prevent full lay.' FROM Exercise e WHERE e.ExerciseKey='bow';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Progression (do not progress unless told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='bow';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Add a block/cushion underneath hindlimbs and/or forelimbs' FROM Exercise e WHERE e.ExerciseKey='bow';

-- Bow – Raised Hindlimb Variant
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'bowRaisedHindlimbVariant', 'Bow – Raised Hindlimb Variant', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Start with a square stand and ask the dog to remain stationary with their hindlimbs.
Then guide your dog’s nose towards the floor to encourage them into a bow position without laying fully to the floor.
If needed, place a supportive hand under the stomach to prevent full lay.
Progression (do not progress unless told to by a physiotherapist):
Add a block/cushion underneath hindlimbs and/or forelimbs', NULL, NULL, NULL, TRUE, 'Raised HL bows - IMG_0181.jpg', 'Raised HL bows - IMG_0181.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='bowRaisedHindlimbVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='bowRaisedHindlimbVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Start with a square stand and ask the dog to remain stationary with their hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='bowRaisedHindlimbVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Then guide your dog’s nose towards the floor to encourage them into a bow position without laying fully to the floor.' FROM Exercise e WHERE e.ExerciseKey='bowRaisedHindlimbVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'If needed, place a supportive hand under the stomach to prevent full lay.' FROM Exercise e WHERE e.ExerciseKey='bowRaisedHindlimbVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Progression (do not progress unless told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='bowRaisedHindlimbVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Add a block/cushion underneath hindlimbs and/or forelimbs' FROM Exercise e WHERE e.ExerciseKey='bowRaisedHindlimbVariant';

-- Box Jump
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'boxJump', 'Box Jump', 'This exercise should be performed on a flat, non-slip surface with traction.', 'This exercise should be performed on a flat, non-slip surface with traction.
Use a stable block that is roughly elbow height.
Ask your dog into either a square sit or stand position (guided by physiotherapist).
Then encourage them to jump onto the box, making sure that all limbs have plenty of room to land safely.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='boxJump';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface with traction.' FROM Exercise e WHERE e.ExerciseKey='boxJump';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Use a stable block that is roughly elbow height.' FROM Exercise e WHERE e.ExerciseKey='boxJump';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Ask your dog into either a square sit or stand position (guided by physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='boxJump';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Then encourage them to jump onto the box, making sure that all limbs have plenty of room to land safely.' FROM Exercise e WHERE e.ExerciseKey='boxJump';

-- Butt Scratch / Tail stimulation
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'buttScratchTailStimulation', 'Butt Scratch / Tail stimulation', 'Ask your dog to stand squarely on a flat, non-slip surface.', 'Ask your dog to stand squarely on a flat, non-slip surface.
Gently scratch or apply light stimulation to the base of the tail up to the lower lumbar region of the spine.
This should cause a subtle transfer weight transfer onto the hindlimbs.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neurological_rehabilitation'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='buttScratchTailStimulation';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Ask your dog to stand squarely on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='buttScratchTailStimulation';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Gently scratch or apply light stimulation to the base of the tail up to the lower lumbar region of the spine.' FROM Exercise e WHERE e.ExerciseKey='buttScratchTailStimulation';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'This should cause a subtle transfer weight transfer onto the hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='buttScratchTailStimulation';

-- Carpus PROM
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'carpusProm', 'Carpus PROM', 'Try to stand your dog on a flat non-slip surface and give them plenty of support. Alternatively, ask them to lay flat on their side with their limbs in a neutral position (i.e., not tucked under themselves or rotated in any way). Keep the limb close to the body and in a straight line without any twisting. Take your dog''s joints in a forward and back motion making sure not to push them past their capabilities. Remember to do this gently as forcing a joint can cause damage. Remember, slow and gentle and watch for signs of discomfort (tensing muscles or pulling the leg away). If they do this, try to reduce the degree of motion you''re asking for (ie. move the joint less).', 'Carpus: Cup one of your hands around the carpus (wrist) placing 2 fingers in the gap behind the big pad to stabilise the joint.
Use your other hand to cup the digits/toes and use this hand to flex the joint by bringing it upwards and then extend the joint by pushing from the pad forwards.
When you swap to the other side, you will need to switch your hands over too.', NULL, NULL, NULL, TRUE, 'LR Carpus PROM - 019c041c-3b0b-4871-a66b-15742e169522.jpg', 'LR Carpus PROM - 019c041c-3b0b-4871-a66b-15742e169522.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='passive_range_of_motion'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='carpusProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Carpus: Cup one of your hands around the carpus (wrist) placing 2 fingers in the gap behind the big pad to stabilise the joint.' FROM Exercise e WHERE e.ExerciseKey='carpusProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Use your other hand to cup the digits/toes and use this hand to flex the joint by bringing it upwards and then extend the joint by pushing from the pad forwards.' FROM Exercise e WHERE e.ExerciseKey='carpusProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'When you swap to the other side, you will need to switch your hands over too.' FROM Exercise e WHERE e.ExerciseKey='carpusProm';

-- Circles
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'circles', 'Circles', 'This exercise should be performed on a flat, non-slip surface at an active walk.', 'This exercise should be performed on a flat, non-slip surface at an active walk.
Place four markers/cones on the floor (radius determined by physiotherapist) to create a circle and walk your dog around them.
Your dog should be on the inside, closest to the circle markers.
Remember to do this exercise on both sides (i.e. clockwise and anticlockwise).
Progression (never progress unless told to by a physiotherapist):
Reduce distance between markers/cones to encourage tighter turns', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='circles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface at an active walk.' FROM Exercise e WHERE e.ExerciseKey='circles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place four markers/cones on the floor (radius determined by physiotherapist) to create a circle and walk your dog around them.' FROM Exercise e WHERE e.ExerciseKey='circles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Your dog should be on the inside, closest to the circle markers.' FROM Exercise e WHERE e.ExerciseKey='circles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Remember to do this exercise on both sides (i.e. clockwise and anticlockwise).' FROM Exercise e WHERE e.ExerciseKey='circles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Progression (never progress unless told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='circles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Reduce distance between markers/cones to encourage tighter turns' FROM Exercise e WHERE e.ExerciseKey='circles';

-- Clamshells
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'clamshells', 'Clamshells', 'This exercise should be performed with your dog flat on their side on a flat, non-slip surface.', 'This exercise should be performed with your dog flat on their side on a flat, non-slip surface.
Use a toy or a treat to gradually lure your dog’s head to between its hindlimbs so one of the hind legs lifts upwards.
Try to hold this position for a couple of seconds once your dog is more proficient.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='clamshells';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed with your dog flat on their side on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='clamshells';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Use a toy or a treat to gradually lure your dog’s head to between its hindlimbs so one of the hind legs lifts upwards.' FROM Exercise e WHERE e.ExerciseKey='clamshells';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Try to hold this position for a couple of seconds once your dog is more proficient.' FROM Exercise e WHERE e.ExerciseKey='clamshells';

-- Cold packs
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'coldPacks', 'Cold packs', 'Wrap a cold pack in a damp towel and apply it to the area.', 'Wrap a cold pack in a damp towel and apply it to the area.
Before applying, make sure to check the temperature of the pack on your own wrist first to ensure it’s not too cold.
You will have to hold it to apply light pressure and keep the pack in place.
Make sure to check the temperature of your dog’s skin every 2 mins or so.
Do not leave them unattended with the cold pack applied.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='thermal_therapy'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='coldPacks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Wrap a cold pack in a damp towel and apply it to the area.' FROM Exercise e WHERE e.ExerciseKey='coldPacks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Before applying, make sure to check the temperature of the pack on your own wrist first to ensure it’s not too cold.' FROM Exercise e WHERE e.ExerciseKey='coldPacks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'You will have to hold it to apply light pressure and keep the pack in place.' FROM Exercise e WHERE e.ExerciseKey='coldPacks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Make sure to check the temperature of your dog’s skin every 2 mins or so.' FROM Exercise e WHERE e.ExerciseKey='coldPacks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Do not leave them unattended with the cold pack applied.' FROM Exercise e WHERE e.ExerciseKey='coldPacks';

-- Cold packs (neuro)
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'coldPacksNeuro', 'Cold packs (neuro)', 'Wrap a cold pack in a damp towel and press it against your dog’s hindlimbs.', 'Wrap a cold pack in a damp towel and press it against your dog’s hindlimbs.
You will have to hold it to apply light pressure and keep the pack in place for a few seconds at a time.
Make sure to check the temperature of your dog’s skin after every application.
Do not leave them unattended with the cold pack applied.
The idea is to initiate nerve stimulation and induce a muscle contraction (similar to shivering when you are cold).', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='thermal_therapy'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='coldPacksNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Wrap a cold pack in a damp towel and press it against your dog’s hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='coldPacksNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'You will have to hold it to apply light pressure and keep the pack in place for a few seconds at a time.' FROM Exercise e WHERE e.ExerciseKey='coldPacksNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Make sure to check the temperature of your dog’s skin after every application.' FROM Exercise e WHERE e.ExerciseKey='coldPacksNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Do not leave them unattended with the cold pack applied.' FROM Exercise e WHERE e.ExerciseKey='coldPacksNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'The idea is to initiate nerve stimulation and induce a muscle contraction (similar to shivering when you are cold).' FROM Exercise e WHERE e.ExerciseKey='coldPacksNeuro';

-- Crawling
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'crawling', 'Crawling', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Ask your dog into a square lay position.
Encourage them to crawl forwards where the stifles come over the hindlimb digits and legs remain on the floor at all times.
You can do this by placing treats on the floor and asking them to crawl under low height furniture.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='crawling';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='crawling';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask your dog into a square lay position.' FROM Exercise e WHERE e.ExerciseKey='crawling';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Encourage them to crawl forwards where the stifles come over the hindlimb digits and legs remain on the floor at all times.' FROM Exercise e WHERE e.ExerciseKey='crawling';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'You can do this by placing treats on the floor and asking them to crawl under low height furniture.' FROM Exercise e WHERE e.ExerciseKey='crawling';

-- Crouch
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'crouch', 'Crouch', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Ask for a square sit then encourage your dog to reach forwards/upwards so the hindlimbs remain on the floor but the stifles/knees come over the hindlimb digits.
This should elongate the spine and engage core muscles.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='crouch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='crouch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask for a square sit then encourage your dog to reach forwards/upwards so the hindlimbs remain on the floor but the stifles/knees come over the hindlimb digits.' FROM Exercise e WHERE e.ExerciseKey='crouch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'This should elongate the spine and engage core muscles.' FROM Exercise e WHERE e.ExerciseKey='crouch';

-- Crunch
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'crunch', 'Crunch', 'This exercise should be performed with your dog flat on their side on a flat, non-slip surface.', 'This exercise should be performed with your dog flat on their side on a flat, non-slip surface.
Use a toy or treat to gradually lure your dog’s head to the level of their shoulder.
As your dog develops, you can increase the level of the turn to the abdomen then hip (ask your physiotherapist for guidance).', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='crunch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed with your dog flat on their side on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='crunch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Use a toy or treat to gradually lure your dog’s head to the level of their shoulder.' FROM Exercise e WHERE e.ExerciseKey='crunch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'As your dog develops, you can increase the level of the turn to the abdomen then hip (ask your physiotherapist for guidance).' FROM Exercise e WHERE e.ExerciseKey='crunch';

-- Curb Walks
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'curbWalks', 'Curb Walks', 'This exercise should be conducted on a stable surface where likelihood of slipping is limited.', 'This exercise should be conducted on a stable surface where likelihood of slipping is limited.
Lead your dog up and down a curb in a weave pattern.
One foot should rise per time lead by forelimbs then hindlimbs one at a time.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='curbWalks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be conducted on a stable surface where likelihood of slipping is limited.' FROM Exercise e WHERE e.ExerciseKey='curbWalks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Lead your dog up and down a curb in a weave pattern.' FROM Exercise e WHERE e.ExerciseKey='curbWalks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'One foot should rise per time lead by forelimbs then hindlimbs one at a time.' FROM Exercise e WHERE e.ExerciseKey='curbWalks';

-- Dancing
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'dancing', 'Dancing', 'This exercise should be performed on a flat, non-slip surface with traction.', 'This exercise should be performed on a flat, non-slip surface with traction.
Either lift the forelimbs off the floor by gently gripping the wrists and allowing flexion through the elbows or lure them with a treat or a toy so their forelimbs leave the floor.
Slowly guide your dog in a forwards-backwards direction.
Start by providing support by holding the forelimbs, then remove it as they become more proficient.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='dancing';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface with traction.' FROM Exercise e WHERE e.ExerciseKey='dancing';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Either lift the forelimbs off the floor by gently gripping the wrists and allowing flexion through the elbows or lure them with a treat or a toy so their forelimbs leave the floor.' FROM Exercise e WHERE e.ExerciseKey='dancing';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Slowly guide your dog in a forwards-backwards direction.' FROM Exercise e WHERE e.ExerciseKey='dancing';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Start by providing support by holding the forelimbs, then remove it as they become more proficient.' FROM Exercise e WHERE e.ExerciseKey='dancing';

-- Diagonal limb lifts
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'diagonalLimbLifts', 'Diagonal limb lifts', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Increasing the intensity from give paw, ask your dog for a forelimb then lift the opposite hindlimb at the same time.
You should support the forelimb at above the wrist/carpus and the hindlimb below the ankle/hock.
Keep both the forelimb and hindlimb in slight flexion to allow your dog to gradually adjust to balancing.
You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.
Once they have held the position for a few seconds, place the limbs back on the floor one at a time and switch to the other diagonal pair.
This exercise will test your dog''s core to balance themselves as well as work on joint range of motion.
You may need someone around to help you.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='limb_lifts_and_proprioception'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='diagonalLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='diagonalLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Increasing the intensity from give paw, ask your dog for a forelimb then lift the opposite hindlimb at the same time.' FROM Exercise e WHERE e.ExerciseKey='diagonalLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'You should support the forelimb at above the wrist/carpus and the hindlimb below the ankle/hock.' FROM Exercise e WHERE e.ExerciseKey='diagonalLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Keep both the forelimb and hindlimb in slight flexion to allow your dog to gradually adjust to balancing.' FROM Exercise e WHERE e.ExerciseKey='diagonalLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.' FROM Exercise e WHERE e.ExerciseKey='diagonalLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Once they have held the position for a few seconds, place the limbs back on the floor one at a time and switch to the other diagonal pair.' FROM Exercise e WHERE e.ExerciseKey='diagonalLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'This exercise will test your dog''s core to balance themselves as well as work on joint range of motion.' FROM Exercise e WHERE e.ExerciseKey='diagonalLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'You may need someone around to help you.' FROM Exercise e WHERE e.ExerciseKey='diagonalLimbLifts';

-- Digging
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'digging', 'Digging', 'This exercise should be performed on a flat, non-slip surface in a square lay position.', 'This exercise should be performed on a flat, non-slip surface in a square lay position.
Use a blanket to hide a treat or toy underneath after your dog has seen it.
Ask them to wait during this then ask them to “find it” and let them dig whilst still in a lay position.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='digging';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface in a square lay position.' FROM Exercise e WHERE e.ExerciseKey='digging';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Use a blanket to hide a treat or toy underneath after your dog has seen it.' FROM Exercise e WHERE e.ExerciseKey='digging';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Ask them to wait during this then ask them to “find it” and let them dig whilst still in a lay position.' FROM Exercise e WHERE e.ExerciseKey='digging';

-- Digit lifts
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'digitLifts', 'Digit lifts', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Place your dog’s forelimbs and/or hindlimbs onto a non-slip cushion a few inches tall.
Guide them forwards with their head raised using a treat so the digits flex.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='limb_lifts_and_proprioception'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='digitLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='digitLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place your dog’s forelimbs and/or hindlimbs onto a non-slip cushion a few inches tall.' FROM Exercise e WHERE e.ExerciseKey='digitLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Guide them forwards with their head raised using a treat so the digits flex.' FROM Exercise e WHERE e.ExerciseKey='digitLifts';

-- Digits PROM
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'digitsProm', 'Digits PROM', 'Try to stand your dog on a flat non-slip surface and give them plenty of support. Alternatively, ask them to lay flat on their side with their limbs in a neutral position (i.e., not tucked under themselves or rotated in any way). Keep the limb close to the body and in a straight line without any twisting. Take your dog''s joints in a forward and back motion making sure not to push them past their capabilities. Remember to do this gently as forcing a joint can cause damage. Remember, slow and gentle and watch for signs of discomfort (tensing muscles or pulling the leg away). If they do this, try to reduce the degree of motion you''re asking for (ie. move the joint less).', 'Digits: For the digits/toes, gently grasp your hand below the wrist and take each individual toe in a forward and back motion.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='passive_range_of_motion'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='digitsProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Digits: For the digits/toes, gently grasp your hand below the wrist and take each individual toe in a forward and back motion.' FROM Exercise e WHERE e.ExerciseKey='digitsProm';

-- Effleurage massage
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'effleurageMassage', 'Effleurage massage', 'Make sure your dog is calm and relaxed on a comfortable surface in a relatively flat position.', 'Make sure your dog is calm and relaxed on a comfortable surface in a relatively flat position.
Apply your palms to the area following one hand with the other and gradually increasing pressure as the tissue warms up.
Travel in the direction of the hair line.
Do not rush the movement.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='massage'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='effleurageMassage';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Make sure your dog is calm and relaxed on a comfortable surface in a relatively flat position.' FROM Exercise e WHERE e.ExerciseKey='effleurageMassage';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Apply your palms to the area following one hand with the other and gradually increasing pressure as the tissue warms up.' FROM Exercise e WHERE e.ExerciseKey='effleurageMassage';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Travel in the direction of the hair line.' FROM Exercise e WHERE e.ExerciseKey='effleurageMassage';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Do not rush the movement.' FROM Exercise e WHERE e.ExerciseKey='effleurageMassage';

-- Elbow PROM
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'elbowProm', 'Elbow PROM', 'Try to stand your dog on a flat non-slip surface and give them plenty of support. Alternatively, ask them to lay flat on their side with their limbs in a neutral position (i.e., not tucked under themselves or rotated in any way). Keep the limb close to the body and in a straight line without any twisting. Take your dog''s joints in a forward and back motion making sure not to push them past their capabilities. Remember to do this gently as forcing a joint can cause damage. Remember, slow and gentle and watch for signs of discomfort (tensing muscles or pulling the leg away). If they do this, try to reduce the degree of motion you''re asking for (ie. move the joint less).', 'Elbow: Cup your hand around the back of the elbow to stabilise the joint.
Take your other hand and cup around the carpus (wrist) with your palm facing upwards to initiate motion.
Gently bring the joint into flexion by taking it upwards, then take it into extension by pushing from the carpus backwards so the limb is more or less straight.
When you swap to the other side, you will need to switch your hands over too.', NULL, NULL, NULL, TRUE, 'LR elbow PROM - v24044gl0000d44vnqfog65vpaqk1nk0.jpg', 'LR elbow PROM - v24044gl0000d44vnqfog65vpaqk1nk0.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='passive_range_of_motion'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='elbowProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Elbow: Cup your hand around the back of the elbow to stabilise the joint.' FROM Exercise e WHERE e.ExerciseKey='elbowProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Take your other hand and cup around the carpus (wrist) with your palm facing upwards to initiate motion.' FROM Exercise e WHERE e.ExerciseKey='elbowProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Gently bring the joint into flexion by taking it upwards, then take it into extension by pushing from the carpus backwards so the limb is more or less straight.' FROM Exercise e WHERE e.ExerciseKey='elbowProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'When you swap to the other side, you will need to switch your hands over too.' FROM Exercise e WHERE e.ExerciseKey='elbowProm';

-- Elevate head
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'elevateHead', 'Elevate head', 'This can be done in either a sit, stand, or lay position (as guided by your physiotherapist).', 'This can be done in either a sit, stand, or lay position (as guided by your physiotherapist).
Use a treat to encourage your dog to lift its head.
Try to hold the position for a few seconds before allowing them to rest.
This can be done in various directions – either to the side, rotated, up, or down.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='core_and_posture'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='elevateHead';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This can be done in either a sit, stand, or lay position (as guided by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='elevateHead';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Use a treat to encourage your dog to lift its head.' FROM Exercise e WHERE e.ExerciseKey='elevateHead';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Try to hold the position for a few seconds before allowing them to rest.' FROM Exercise e WHERE e.ExerciseKey='elevateHead';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'This can be done in various directions – either to the side, rotated, up, or down.' FROM Exercise e WHERE e.ExerciseKey='elevateHead';

-- Figures-of-Eight
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'figuresOfEight', 'Figures-of-Eight', 'This exercise should be performed on a flat, non-slip surface at an active walk.', 'This exercise should be performed on a flat, non-slip surface at an active walk.
Place two markers/cones on the floor (distance between determined by physiotherapist) and walk your dog around them in a figure-of-eight pattern.
Turning must be slow and controlled with wide turns encouraged to allow bending.
Start with a shallow turn, close to the markers and progress to a deeper turn to encourage spinal bending slowly.
Progression (never progress unless told to by a physiotherapist):
Reduce distance between markers/cones to encourage tighter turns', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='figuresOfEight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface at an active walk.' FROM Exercise e WHERE e.ExerciseKey='figuresOfEight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place two markers/cones on the floor (distance between determined by physiotherapist) and walk your dog around them in a figure-of-eight pattern.' FROM Exercise e WHERE e.ExerciseKey='figuresOfEight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Turning must be slow and controlled with wide turns encouraged to allow bending.' FROM Exercise e WHERE e.ExerciseKey='figuresOfEight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Start with a shallow turn, close to the markers and progress to a deeper turn to encourage spinal bending slowly.' FROM Exercise e WHERE e.ExerciseKey='figuresOfEight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Progression (never progress unless told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='figuresOfEight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Reduce distance between markers/cones to encourage tighter turns' FROM Exercise e WHERE e.ExerciseKey='figuresOfEight';

-- Flat pivot / Around the world
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'flatPivotAroundTheWorld', 'Flat pivot / Around the world', 'Perform this exercise on a flat, non-slip surface with your dog standing square to start.', 'Perform this exercise on a flat, non-slip surface with your dog standing square to start.
Stand in front of your dog with a treat in hand and start to walk slowly in a circle around them, encouraging them to turn with you whilst keeping their forelimbs firmly on the block.
The hindlimbs will move to the side causing a pivot around the forelimbs on the block at the centre.
Make sure to perform this exercise evenly in both directions and keep to a slow and steady pace.
Once your dog becomes more proficient, you can start to speed up slightly without causing them to stumble over their hindlimbs.', NULL, NULL, NULL, TRUE, 'Around the worlds_Pivots - IMG_0179.jpg', 'Around the worlds_Pivots - IMG_0179.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='flatPivotAroundTheWorld';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this exercise on a flat, non-slip surface with your dog standing square to start.' FROM Exercise e WHERE e.ExerciseKey='flatPivotAroundTheWorld';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Stand in front of your dog with a treat in hand and start to walk slowly in a circle around them, encouraging them to turn with you whilst keeping their forelimbs firmly on the block.' FROM Exercise e WHERE e.ExerciseKey='flatPivotAroundTheWorld';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'The hindlimbs will move to the side causing a pivot around the forelimbs on the block at the centre.' FROM Exercise e WHERE e.ExerciseKey='flatPivotAroundTheWorld';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Make sure to perform this exercise evenly in both directions and keep to a slow and steady pace.' FROM Exercise e WHERE e.ExerciseKey='flatPivotAroundTheWorld';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Once your dog becomes more proficient, you can start to speed up slightly without causing them to stumble over their hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='flatPivotAroundTheWorld';

-- Flat Poles
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'flatPoles', 'Flat Poles', 'This exercise should be performed on a flat, non-slip surface and at an active walk.', 'This exercise should be performed on a flat, non-slip surface and at an active walk.
Lay flat poles on the ground and ask your dog to walk over the centre of each.
The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).
Upon completion of the poles, make sure you turn your dog nice and wide.
If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.
If they are still scuffing poles, you should remove one pole to reduce the intensity of the exercise.', NULL, NULL, NULL, TRUE, 'Flat poles - 069204782e3140ba95ccdf89e797b929.jpg', 'Flat poles - 069204782e3140ba95ccdf89e797b929.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='flatPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and at an active walk.' FROM Exercise e WHERE e.ExerciseKey='flatPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Lay flat poles on the ground and ask your dog to walk over the centre of each.' FROM Exercise e WHERE e.ExerciseKey='flatPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).' FROM Exercise e WHERE e.ExerciseKey='flatPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Upon completion of the poles, make sure you turn your dog nice and wide.' FROM Exercise e WHERE e.ExerciseKey='flatPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.' FROM Exercise e WHERE e.ExerciseKey='flatPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'If they are still scuffing poles, you should remove one pole to reduce the intensity of the exercise.' FROM Exercise e WHERE e.ExerciseKey='flatPoles';

-- Forelimb and Hindlimb limb lifts
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'forelimbAndHindlimbLimbLifts', 'Forelimb and Hindlimb limb lifts', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Ask your dog to stand square then ask for their paw.
Depending on their level of training, you may have to start by picking up the limb yourself at first.
Bring the paw upwards slightly to gain a little flexion from the elbow (forelimb) or stifle/knee (hindlimb) joint.
You should support the forelimb at above the wrist/carpus and the hindlimb below the ankle/hock.
You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.
Hold the position for a few seconds before gently placing the limb back on the floor.
You may have to provide support under the stomach with your hand when lifting the hindlimbs.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='limb_lifts_and_proprioception'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='forelimbAndHindlimbLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='forelimbAndHindlimbLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask your dog to stand square then ask for their paw.' FROM Exercise e WHERE e.ExerciseKey='forelimbAndHindlimbLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Depending on their level of training, you may have to start by picking up the limb yourself at first.' FROM Exercise e WHERE e.ExerciseKey='forelimbAndHindlimbLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Bring the paw upwards slightly to gain a little flexion from the elbow (forelimb) or stifle/knee (hindlimb) joint.' FROM Exercise e WHERE e.ExerciseKey='forelimbAndHindlimbLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'You should support the forelimb at above the wrist/carpus and the hindlimb below the ankle/hock.' FROM Exercise e WHERE e.ExerciseKey='forelimbAndHindlimbLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.' FROM Exercise e WHERE e.ExerciseKey='forelimbAndHindlimbLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Hold the position for a few seconds before gently placing the limb back on the floor.' FROM Exercise e WHERE e.ExerciseKey='forelimbAndHindlimbLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'You may have to provide support under the stomach with your hand when lifting the hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='forelimbAndHindlimbLimbLifts';

-- Forelimb Isolated Joint PROM
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'forelimbIsolatedJointProm', 'Forelimb Isolated Joint PROM', 'Try to stand your dog on a flat non-slip surface and give them plenty of support. Alternatively, ask them to lay flat on their side with their limbs in a neutral position (i.e., not tucked under themselves or rotated in any way). Keep the limb close to the body and in a straight line without any twisting. Take your dog''s joints in a forward and back motion making sure not to push them past their capabilities. Remember to do this gently as forcing a joint can cause damage. Remember, slow and gentle and watch for signs of discomfort (tensing muscles or pulling the leg away). If they do this, try to reduce the degree of motion you''re asking for (ie. move the joint less).', 'Digits: For the digits/toes, gently grasp your hand below the wrist and take each individual toe in a forward and back motion.', NULL, NULL, NULL, TRUE, 'LR FL isolated joint PROM - 019c041c-3b0b-4871-a66b-15742e169522.jpg', 'LR FL isolated joint PROM - 019c041c-3b0b-4871-a66b-15742e169522.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='passive_range_of_motion'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='forelimbIsolatedJointProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Digits: For the digits/toes, gently grasp your hand below the wrist and take each individual toe in a forward and back motion.' FROM Exercise e WHERE e.ExerciseKey='forelimbIsolatedJointProm';

-- Forelimb limb lifts / Give paw
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'forelimbLimbLiftsGivePaw', 'Forelimb limb lifts / Give paw', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Ask your dog to stand square then ask for their paw.
Depending on their level of training, you may have to start by picking up the limb yourself at first by supporting the back of the wrist/carpus.
Bring the paw upwards slightly to gain a little flexion from the elbow joint.
You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.
Hold the position for a few seconds before gently placing the limb back on the floor.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='limb_lifts_and_proprioception'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='forelimbLimbLiftsGivePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='forelimbLimbLiftsGivePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask your dog to stand square then ask for their paw.' FROM Exercise e WHERE e.ExerciseKey='forelimbLimbLiftsGivePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Depending on their level of training, you may have to start by picking up the limb yourself at first by supporting the back of the wrist/carpus.' FROM Exercise e WHERE e.ExerciseKey='forelimbLimbLiftsGivePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Bring the paw upwards slightly to gain a little flexion from the elbow joint.' FROM Exercise e WHERE e.ExerciseKey='forelimbLimbLiftsGivePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.' FROM Exercise e WHERE e.ExerciseKey='forelimbLimbLiftsGivePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Hold the position for a few seconds before gently placing the limb back on the floor.' FROM Exercise e WHERE e.ExerciseKey='forelimbLimbLiftsGivePaw';

-- Forelimb limb lifts / Give paw – Stood Variant
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'forelimbLimbLiftsGivePawStoodVariant', 'Forelimb limb lifts / Give paw – Stood Variant', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Ask your dog to stand square then ask for their paw.
Depending on their level of training, you may have to start by picking up the limb yourself at first by supporting the back of the wrist/carpus.
Bring the paw upwards slightly to gain a little flexion from the elbow joint.
You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.
Hold the position for a few seconds before gently placing the limb back on the floor.', NULL, NULL, NULL, TRUE, 'Stood DV PROM FLs - a83783da-cfdf-44cc-9406-0eeb2498385f.jpg', 'Stood DV PROM FLs - a83783da-cfdf-44cc-9406-0eeb2498385f.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='limb_lifts_and_proprioception'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='forelimbLimbLiftsGivePawStoodVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='forelimbLimbLiftsGivePawStoodVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask your dog to stand square then ask for their paw.' FROM Exercise e WHERE e.ExerciseKey='forelimbLimbLiftsGivePawStoodVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Depending on their level of training, you may have to start by picking up the limb yourself at first by supporting the back of the wrist/carpus.' FROM Exercise e WHERE e.ExerciseKey='forelimbLimbLiftsGivePawStoodVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Bring the paw upwards slightly to gain a little flexion from the elbow joint.' FROM Exercise e WHERE e.ExerciseKey='forelimbLimbLiftsGivePawStoodVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.' FROM Exercise e WHERE e.ExerciseKey='forelimbLimbLiftsGivePawStoodVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Hold the position for a few seconds before gently placing the limb back on the floor.' FROM Exercise e WHERE e.ExerciseKey='forelimbLimbLiftsGivePawStoodVariant';

-- Forelimb Protraction Stretch
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'forelimbProtractionStretch', 'Forelimb Protraction Stretch', 'ask your physiotherapist for direction).', 'Forelimb protraction: place a hand behind the elbow (origin of motion) and support the leg with the other hand around the carpus/wrist.
Gently guide the limb forwards in a straight line and hold the position.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='stretching_and_mobility'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='forelimbProtractionStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Forelimb protraction: place a hand behind the elbow (origin of motion) and support the leg with the other hand around the carpus/wrist.' FROM Exercise e WHERE e.ExerciseKey='forelimbProtractionStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Gently guide the limb forwards in a straight line and hold the position.' FROM Exercise e WHERE e.ExerciseKey='forelimbProtractionStretch';

-- Forelimb Retraction Stretch
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'forelimbRetractionStretch', 'Forelimb Retraction Stretch', 'ask your physiotherapist for direction).', 'Forelimb retraction: place a hand between the shoulder and elbow joints and guide the leg with the hand around the carpus/wrist.
Gently push the limb backwards towards the hindlimbs in a straight line and hold the position.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='stretching_and_mobility'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='forelimbRetractionStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Forelimb retraction: place a hand between the shoulder and elbow joints and guide the leg with the hand around the carpus/wrist.' FROM Exercise e WHERE e.ExerciseKey='forelimbRetractionStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Gently push the limb backwards towards the hindlimbs in a straight line and hold the position.' FROM Exercise e WHERE e.ExerciseKey='forelimbRetractionStretch';

-- Forelimb Triceps Stretch
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'forelimbTricepsStretch', 'Forelimb Triceps Stretch', 'ask your physiotherapist for direction).', 'Forelimb triceps: wrap a hand around the elbow and guide the motion with a hand around the carpus/wrist.
Guide the leg upwards towards the shoulder (swan head) so it is flexed through the elbow.
Then gently push the leg forward from behind the elbow and hold the position.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='stretching_and_mobility'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='forelimbTricepsStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Forelimb triceps: wrap a hand around the elbow and guide the motion with a hand around the carpus/wrist.' FROM Exercise e WHERE e.ExerciseKey='forelimbTricepsStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Guide the leg upwards towards the shoulder (swan head) so it is flexed through the elbow.' FROM Exercise e WHERE e.ExerciseKey='forelimbTricepsStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Then gently push the leg forward from behind the elbow and hold the position.' FROM Exercise e WHERE e.ExerciseKey='forelimbTricepsStretch';

-- Half Sit to Stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'halfSitToStands', 'Half Sit to Stands', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Use a flat, stable block or use the bottom step of your stairs (depending on dog size – it should be at the height of your dog’s stifle/knee).
Ask your dog to do a square sit with its pelvis on the block/step and hindlimb paws on the floor (think of sitting on a chair).
Once the sit has been achieved, ask them to stand (you can use a treat or a toy for encouragement but only if controlled).
Control the standing action by taking your hand with a treat in it from their head down past your leg.
Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.
This is similar to a squat, with them using their hindlimbs to push themselves upwards.
If necessary, you can use wall support by backing them into a corner to prevent splaying/rotation of hindlimbs.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='halfSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='halfSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Use a flat, stable block or use the bottom step of your stairs (depending on dog size – it should be at the height of your dog’s stifle/knee).' FROM Exercise e WHERE e.ExerciseKey='halfSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Ask your dog to do a square sit with its pelvis on the block/step and hindlimb paws on the floor (think of sitting on a chair).' FROM Exercise e WHERE e.ExerciseKey='halfSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Once the sit has been achieved, ask them to stand (you can use a treat or a toy for encouragement but only if controlled).' FROM Exercise e WHERE e.ExerciseKey='halfSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Control the standing action by taking your hand with a treat in it from their head down past your leg.' FROM Exercise e WHERE e.ExerciseKey='halfSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.' FROM Exercise e WHERE e.ExerciseKey='halfSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'This is similar to a squat, with them using their hindlimbs to push themselves upwards.' FROM Exercise e WHERE e.ExerciseKey='halfSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'If necessary, you can use wall support by backing them into a corner to prevent splaying/rotation of hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='halfSitToStands';

-- Heat packs
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'heatPacks', 'Heat packs', 'Wrap a heat pack in a dry towel and apply it to the area.', 'Wrap a heat pack in a dry towel and apply it to the area.
Before applying, make sure to check the temperature of the pack on your own wrist first to ensure it’s not too hot.
You will have to hold it to apply light pressure and keep the pack in place.
Make sure to check the temperature of your dog’s skin every 2 mins or so.
Do not leave them unattended with the heat pack applied.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='thermal_therapy'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='heatPacks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Wrap a heat pack in a dry towel and apply it to the area.' FROM Exercise e WHERE e.ExerciseKey='heatPacks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Before applying, make sure to check the temperature of the pack on your own wrist first to ensure it’s not too hot.' FROM Exercise e WHERE e.ExerciseKey='heatPacks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'You will have to hold it to apply light pressure and keep the pack in place.' FROM Exercise e WHERE e.ExerciseKey='heatPacks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Make sure to check the temperature of your dog’s skin every 2 mins or so.' FROM Exercise e WHERE e.ExerciseKey='heatPacks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Do not leave them unattended with the heat pack applied.' FROM Exercise e WHERE e.ExerciseKey='heatPacks';

-- Hindlimb Abduction Stretch
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'hindlimbAbductionStretch', 'Hindlimb Abduction Stretch', 'ask your physiotherapist for direction).', 'Hindlimb abduction: place a hand over the front of the stifle/knee and wrap a hand around the tarsus/ankle.
Flex the leg up until you reach a 90-degree angle where the stifle and ankle are level.
Then gently push from the inside leg to lift the leg up and to the side.
Think of it as opening the leg.
Hold this position.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='stretching_and_mobility'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='hindlimbAbductionStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Hindlimb abduction: place a hand over the front of the stifle/knee and wrap a hand around the tarsus/ankle.' FROM Exercise e WHERE e.ExerciseKey='hindlimbAbductionStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Flex the leg up until you reach a 90-degree angle where the stifle and ankle are level.' FROM Exercise e WHERE e.ExerciseKey='hindlimbAbductionStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Then gently push from the inside leg to lift the leg up and to the side.' FROM Exercise e WHERE e.ExerciseKey='hindlimbAbductionStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Think of it as opening the leg.' FROM Exercise e WHERE e.ExerciseKey='hindlimbAbductionStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Hold this position.' FROM Exercise e WHERE e.ExerciseKey='hindlimbAbductionStretch';

-- Hindlimb limb lifts
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'hindlimbLimbLifts', 'Hindlimb limb lifts', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Ask your dog to stand square then gently pick up their hindlimb foot by supporting the back of the ankle/hock joint.
Bring the paw upwards slightly to gain a little flexion from the stifle joint.
You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.
Hold the position for a few seconds before gently placing the limb back on the floor.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='limb_lifts_and_proprioception'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='hindlimbLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='hindlimbLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask your dog to stand square then gently pick up their hindlimb foot by supporting the back of the ankle/hock joint.' FROM Exercise e WHERE e.ExerciseKey='hindlimbLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Bring the paw upwards slightly to gain a little flexion from the stifle joint.' FROM Exercise e WHERE e.ExerciseKey='hindlimbLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.' FROM Exercise e WHERE e.ExerciseKey='hindlimbLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Hold the position for a few seconds before gently placing the limb back on the floor.' FROM Exercise e WHERE e.ExerciseKey='hindlimbLimbLifts';

-- Hindlimb limb lifts – Stood Variant
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'hindlimbLimbLiftsStoodVariant', 'Hindlimb limb lifts – Stood Variant', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Ask your dog to stand square then gently pick up their hindlimb foot by supporting the back of the ankle/hock joint.
Bring the paw upwards slightly to gain a little flexion from the stifle joint.
You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.
Hold the position for a few seconds before gently placing the limb back on the floor.', NULL, NULL, NULL, TRUE, 'Stood HL DV PROM - da001329-74c0-4217-8514-c11507de42c1.jpg', 'Stood HL DV PROM - da001329-74c0-4217-8514-c11507de42c1.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='limb_lifts_and_proprioception'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='hindlimbLimbLiftsStoodVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='hindlimbLimbLiftsStoodVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask your dog to stand square then gently pick up their hindlimb foot by supporting the back of the ankle/hock joint.' FROM Exercise e WHERE e.ExerciseKey='hindlimbLimbLiftsStoodVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Bring the paw upwards slightly to gain a little flexion from the stifle joint.' FROM Exercise e WHERE e.ExerciseKey='hindlimbLimbLiftsStoodVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.' FROM Exercise e WHERE e.ExerciseKey='hindlimbLimbLiftsStoodVariant';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Hold the position for a few seconds before gently placing the limb back on the floor.' FROM Exercise e WHERE e.ExerciseKey='hindlimbLimbLiftsStoodVariant';

-- Hindlimb Protraction Stretch
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'hindlimbProtractionStretch', 'Hindlimb Protraction Stretch', 'ask your physiotherapist for direction).', 'Hindlimb protraction: support the limb with a hand around the stifle/knee joint then guide the limb with a hand around the tarsus/ankle.
Guide the leg forwards until straight and reaching behind the forelimb on the same side.
Hold this position.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='stretching_and_mobility'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='hindlimbProtractionStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Hindlimb protraction: support the limb with a hand around the stifle/knee joint then guide the limb with a hand around the tarsus/ankle.' FROM Exercise e WHERE e.ExerciseKey='hindlimbProtractionStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Guide the leg forwards until straight and reaching behind the forelimb on the same side.' FROM Exercise e WHERE e.ExerciseKey='hindlimbProtractionStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Hold this position.' FROM Exercise e WHERE e.ExerciseKey='hindlimbProtractionStretch';

-- Hindlimb Quadriceps Stretch
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'hindlimbQuadricepsStretch', 'Hindlimb Quadriceps Stretch', 'ask your physiotherapist for direction).', 'Hindlimb quadriceps: place a hand over the front of the stifle/knee and wrap a hand around the tarsus/ankle.
Guide the first action with the hand around the tarsus to flex the limb upwards until the ankle touches the bum.
Then push the hand supporting the stifle/knee backwards to guide the leg straighter.
Hold this position.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='stretching_and_mobility'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='hindlimbQuadricepsStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Hindlimb quadriceps: place a hand over the front of the stifle/knee and wrap a hand around the tarsus/ankle.' FROM Exercise e WHERE e.ExerciseKey='hindlimbQuadricepsStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Guide the first action with the hand around the tarsus to flex the limb upwards until the ankle touches the bum.' FROM Exercise e WHERE e.ExerciseKey='hindlimbQuadricepsStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Then push the hand supporting the stifle/knee backwards to guide the leg straighter.' FROM Exercise e WHERE e.ExerciseKey='hindlimbQuadricepsStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Hold this position.' FROM Exercise e WHERE e.ExerciseKey='hindlimbQuadricepsStretch';

-- Hindlimb Retraction Stretch
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'hindlimbRetractionStretch', 'Hindlimb Retraction Stretch', 'ask your physiotherapist for direction).', 'Hindlimb retraction: support the limb with a hand around the tarsus/ankle and guide the limb with a hand in front of the stifle/knee.
Take the leg backwards until straight and hold this position.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='stretching_and_mobility'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='hindlimbRetractionStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Hindlimb retraction: support the limb with a hand around the tarsus/ankle and guide the limb with a hand in front of the stifle/knee.' FROM Exercise e WHERE e.ExerciseKey='hindlimbRetractionStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Take the leg backwards until straight and hold this position.' FROM Exercise e WHERE e.ExerciseKey='hindlimbRetractionStretch';

-- Hindlimb tapping
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'hindlimbTapping', 'Hindlimb tapping', 'Encourage your dog into a flat position on their side with hindlimbs placed neutrally (to the side not out behind).', 'Encourage your dog into a flat position on their side with hindlimbs placed neutrally (to the side not out behind).
Rapidly tap your fingers in random patterns with a gentle pressure against the hindlimb muscles to stimulate the tissue.
Allow your dog to rest following the 30s bursts then repeat.
Try to do assisted standing or slow lead walking immediately following.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neurological_rehabilitation'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='hindlimbTapping';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Encourage your dog into a flat position on their side with hindlimbs placed neutrally (to the side not out behind).' FROM Exercise e WHERE e.ExerciseKey='hindlimbTapping';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Rapidly tap your fingers in random patterns with a gentle pressure against the hindlimb muscles to stimulate the tissue.' FROM Exercise e WHERE e.ExerciseKey='hindlimbTapping';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Allow your dog to rest following the 30s bursts then repeat.' FROM Exercise e WHERE e.ExerciseKey='hindlimbTapping';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Try to do assisted standing or slow lead walking immediately following.' FROM Exercise e WHERE e.ExerciseKey='hindlimbTapping';

-- Hip PROM
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'hipProm', 'Hip PROM', 'Try to stand your dog on a flat non-slip surface and give them plenty of support. Alternatively, ask them to lay flat on their side with their limbs in a neutral position (i.e., not tucked under themselves or rotated in any way). Keep the limb close to the body and in a straight line without any twisting. Take your dog''s joints in a forward and back motion making sure not to push them past their capabilities. Remember to do this gently as forcing a joint can cause damage. Remember, slow and gentle and watch for signs of discomfort (tensing muscles or pulling the leg away). If they do this, try to reduce the degree of motion you''re asking for (ie. move the joint less).', 'Hip: Cup your hand around the stifle (this will be the origin of motion) and place your other hand on their hip so you can feel the motion of that joint.
Gently flex the leg upwards towards their back and then extend it back behind them.
When you swap to the other side, you will need to switch your hands over too.', NULL, NULL, NULL, TRUE, 'LR hip PROM - e4ea105b559a4effa1e5641ee1f9b223.jpg', 'LR hip PROM - e4ea105b559a4effa1e5641ee1f9b223.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='passive_range_of_motion'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='hipProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Hip: Cup your hand around the stifle (this will be the origin of motion) and place your other hand on their hip so you can feel the motion of that joint.' FROM Exercise e WHERE e.ExerciseKey='hipProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Gently flex the leg upwards towards their back and then extend it back behind them.' FROM Exercise e WHERE e.ExerciseKey='hipProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'When you swap to the other side, you will need to switch your hands over too.' FROM Exercise e WHERE e.ExerciseKey='hipProm';

-- Incline / Decline walks
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'inclineDeclineWalks', 'Incline / Decline walks', 'This exercise should be performed on a flat surface of limited slipperiness.', 'This exercise should be performed on a flat surface of limited slipperiness.
Start with a low slope of 10-15 degrees then progress steadily (ask your physiotherapist for guidance).
This should be done at an active walk speed.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='gait_and_walking'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='inclineDeclineWalks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat surface of limited slipperiness.' FROM Exercise e WHERE e.ExerciseKey='inclineDeclineWalks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Start with a low slope of 10-15 degrees then progress steadily (ask your physiotherapist for guidance).' FROM Exercise e WHERE e.ExerciseKey='inclineDeclineWalks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'This should be done at an active walk speed.' FROM Exercise e WHERE e.ExerciseKey='inclineDeclineWalks';

-- Ipsilateral limb lifts
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'ipsilateralLimbLifts', 'Ipsilateral limb lifts', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Increasing the intensity from give paw, ask your dog for a forelimb then lift the hindlimb on the same side at the same time.
You should support the forelimb at above the wrist/carpus and the hindlimb below the ankle/hock.
Keep both the forelimb and hindlimb in slight flexion to allow your dog to gradually adjust to balancing.
You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.
Once they have held the position for a few seconds, place the limbs back on the floor one at a time and switch to the other pair on the other side.
This exercise will test your dog''s core to balance themselves as well as work on joint range of motion.
You may need someone around to help you.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='limb_lifts_and_proprioception'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='ipsilateralLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='ipsilateralLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Increasing the intensity from give paw, ask your dog for a forelimb then lift the hindlimb on the same side at the same time.' FROM Exercise e WHERE e.ExerciseKey='ipsilateralLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'You should support the forelimb at above the wrist/carpus and the hindlimb below the ankle/hock.' FROM Exercise e WHERE e.ExerciseKey='ipsilateralLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Keep both the forelimb and hindlimb in slight flexion to allow your dog to gradually adjust to balancing.' FROM Exercise e WHERE e.ExerciseKey='ipsilateralLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.' FROM Exercise e WHERE e.ExerciseKey='ipsilateralLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Once they have held the position for a few seconds, place the limbs back on the floor one at a time and switch to the other pair on the other side.' FROM Exercise e WHERE e.ExerciseKey='ipsilateralLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'This exercise will test your dog''s core to balance themselves as well as work on joint range of motion.' FROM Exercise e WHERE e.ExerciseKey='ipsilateralLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'You may need someone around to help you.' FROM Exercise e WHERE e.ExerciseKey='ipsilateralLimbLifts';

-- Kick Back Stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'kickBackStands', 'Kick Back Stands', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Ask your dog for a square sitting position.
Once established, take your hand and place it underneath your dog’s stomach.
Then apply a little pressure to encourage them to stand.
They should use their hindlimbs to stand by shuffling them backwards with forelimbs stood stationary.
Remember, we want them to do the work themselves so avoid lifting them with your hand.
You can further help this motion by holding a treat/toy at chest level then moving it backwards to encourage your dog to follow it and transfer its weight onto the hindlimbs.
Once completed, allow them to sit again and repeat the action.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='kickBackStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='kickBackStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask your dog for a square sitting position.' FROM Exercise e WHERE e.ExerciseKey='kickBackStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Once established, take your hand and place it underneath your dog’s stomach.' FROM Exercise e WHERE e.ExerciseKey='kickBackStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Then apply a little pressure to encourage them to stand.' FROM Exercise e WHERE e.ExerciseKey='kickBackStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'They should use their hindlimbs to stand by shuffling them backwards with forelimbs stood stationary.' FROM Exercise e WHERE e.ExerciseKey='kickBackStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Remember, we want them to do the work themselves so avoid lifting them with your hand.' FROM Exercise e WHERE e.ExerciseKey='kickBackStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'You can further help this motion by holding a treat/toy at chest level then moving it backwards to encourage your dog to follow it and transfer its weight onto the hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='kickBackStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Once completed, allow them to sit again and repeat the action.' FROM Exercise e WHERE e.ExerciseKey='kickBackStands';

-- Knuckle correction
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'knuckleCorrection', 'Knuckle correction', 'Stand your dog in a square position on a flat, non-slip floor.', 'Stand your dog in a square position on a flat, non-slip floor.
Place a forearm under your dog’s stomach for support.
Gently pick up a hindlimb and turn the paw over so the tops of the digits are flat against the floor.
Allow your dog to correct the paw so the pad is back on the floor again.
If your dog does not correct it themselves after 5s, tickle the underside of the paw pad to encourage them to do so.
If they still have not corrected, replace the foot yourself and try again.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neurological_rehabilitation'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='knuckleCorrection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Stand your dog in a square position on a flat, non-slip floor.' FROM Exercise e WHERE e.ExerciseKey='knuckleCorrection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a forearm under your dog’s stomach for support.' FROM Exercise e WHERE e.ExerciseKey='knuckleCorrection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Gently pick up a hindlimb and turn the paw over so the tops of the digits are flat against the floor.' FROM Exercise e WHERE e.ExerciseKey='knuckleCorrection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Allow your dog to correct the paw so the pad is back on the floor again.' FROM Exercise e WHERE e.ExerciseKey='knuckleCorrection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'If your dog does not correct it themselves after 5s, tickle the underside of the paw pad to encourage them to do so.' FROM Exercise e WHERE e.ExerciseKey='knuckleCorrection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'If they still have not corrected, replace the foot yourself and try again.' FROM Exercise e WHERE e.ExerciseKey='knuckleCorrection';

-- Labyrinth poles
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'labyrinthPoles', 'Labyrinth poles', 'This exercise should be performed on a flat, non-slip surface at an active walk.', 'This exercise should be performed on a flat, non-slip surface at an active walk.
Place poles in random patterns on the floor and chose a path to walk through.
Make sure the spacing between the poles is varied but possible (do not allow them to trip).
Progression (never progress unless told to by a physiotherapist):
Raise the pole(s) on one side by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern
Raise the pole(s) on both sides by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern
Complete at a trot speed – you may have to adjust the distance between poles.', NULL, NULL, NULL, TRUE, 'Labyrinth poles - IMG_0240.jpg', 'Labyrinth poles - IMG_0240.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='labyrinthPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface at an active walk.' FROM Exercise e WHERE e.ExerciseKey='labyrinthPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place poles in random patterns on the floor and chose a path to walk through.' FROM Exercise e WHERE e.ExerciseKey='labyrinthPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Make sure the spacing between the poles is varied but possible (do not allow them to trip).' FROM Exercise e WHERE e.ExerciseKey='labyrinthPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Progression (never progress unless told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='labyrinthPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Raise the pole(s) on one side by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern' FROM Exercise e WHERE e.ExerciseKey='labyrinthPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Raise the pole(s) on both sides by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern' FROM Exercise e WHERE e.ExerciseKey='labyrinthPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Complete at a trot speed – you may have to adjust the distance between poles.' FROM Exercise e WHERE e.ExerciseKey='labyrinthPoles';

-- Labyrinth poles – Left Direction
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'labyrinthPolesLeftDirection', 'Labyrinth poles – Left Direction', 'This exercise should be performed on a flat, non-slip surface at an active walk.', 'This exercise should be performed on a flat, non-slip surface at an active walk.
Place poles in random patterns on the floor and chose a path to walk through.
Make sure the spacing between the poles is varied but possible (do not allow them to trip).
Progression (never progress unless told to by a physiotherapist):
Raise the pole(s) on one side by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern
Raise the pole(s) on both sides by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern
Complete at a trot speed – you may have to adjust the distance between poles.', NULL, NULL, NULL, TRUE, 'Labyrinth poles to L - IMG_0242.jpg', 'Labyrinth poles to L - IMG_0242.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='labyrinthPolesLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface at an active walk.' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place poles in random patterns on the floor and chose a path to walk through.' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Make sure the spacing between the poles is varied but possible (do not allow them to trip).' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Progression (never progress unless told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Raise the pole(s) on one side by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Raise the pole(s) on both sides by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Complete at a trot speed – you may have to adjust the distance between poles.' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesLeftDirection';

-- Labyrinth poles – Right Direction
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'labyrinthPolesRightDirection', 'Labyrinth poles – Right Direction', 'This exercise should be performed on a flat, non-slip surface at an active walk.', 'This exercise should be performed on a flat, non-slip surface at an active walk.
Place poles in random patterns on the floor and chose a path to walk through.
Make sure the spacing between the poles is varied but possible (do not allow them to trip).
Progression (never progress unless told to by a physiotherapist):
Raise the pole(s) on one side by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern
Raise the pole(s) on both sides by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern
Complete at a trot speed – you may have to adjust the distance between poles.', NULL, NULL, NULL, TRUE, 'Labyrinth poles to R - IMG_0241.jpg', 'Labyrinth poles to R - IMG_0241.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='labyrinthPolesRightDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface at an active walk.' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesRightDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place poles in random patterns on the floor and chose a path to walk through.' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesRightDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Make sure the spacing between the poles is varied but possible (do not allow them to trip).' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesRightDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Progression (never progress unless told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesRightDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Raise the pole(s) on one side by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesRightDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Raise the pole(s) on both sides by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesRightDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Complete at a trot speed – you may have to adjust the distance between poles.' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesRightDirection';

-- Labyrinth poles – Slow Motion Right
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'labyrinthPolesSlowMotionRight', 'Labyrinth poles – Slow Motion Right', 'This exercise should be performed on a flat, non-slip surface at an active walk.', 'This exercise should be performed on a flat, non-slip surface at an active walk.
Place poles in random patterns on the floor and chose a path to walk through.
Make sure the spacing between the poles is varied but possible (do not allow them to trip).
Progression (never progress unless told to by a physiotherapist):
Raise the pole(s) on one side by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern
Raise the pole(s) on both sides by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern
Complete at a trot speed – you may have to adjust the distance between poles.', NULL, NULL, NULL, TRUE, 'Slow-Mo Labyrinth poles to R - IMG_0243.jpg', 'Slow-Mo Labyrinth poles to R - IMG_0243.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='labyrinthPolesSlowMotionRight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface at an active walk.' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesSlowMotionRight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place poles in random patterns on the floor and chose a path to walk through.' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesSlowMotionRight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Make sure the spacing between the poles is varied but possible (do not allow them to trip).' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesSlowMotionRight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Progression (never progress unless told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesSlowMotionRight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Raise the pole(s) on one side by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesSlowMotionRight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Raise the pole(s) on both sides by a few centimetres (never higher than your dog’s hock/ankle) – this should be a random pattern' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesSlowMotionRight';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Complete at a trot speed – you may have to adjust the distance between poles.' FROM Exercise e WHERE e.ExerciseKey='labyrinthPolesSlowMotionRight';

-- Ladder
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'ladder', 'Ladder', 'This exercise should be performed on a flat, non-slip surface with traction.', 'This exercise should be performed on a flat, non-slip surface with traction.
Place a stable ladder with non-slip rungs on the floor.
Encourage your dog up and onto each rung, taking one step at a time slowly.
Make sure each limb has made contact before rewarding.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='ladder';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface with traction.' FROM Exercise e WHERE e.ExerciseKey='ladder';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a stable ladder with non-slip rungs on the floor.' FROM Exercise e WHERE e.ExerciseKey='ladder';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Encourage your dog up and onto each rung, taking one step at a time slowly.' FROM Exercise e WHERE e.ExerciseKey='ladder';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Make sure each limb has made contact before rewarding.' FROM Exercise e WHERE e.ExerciseKey='ladder';

-- Lay to Stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'layToStands', 'Lay to Stands', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.
Once the lay has been achieved, ask them to stand (you can use a treat or a toy for encouragement but only if controlled).
Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.
This is similar to a push-up, with them using their hindlimbs to push themselves upwards.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='layToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.' FROM Exercise e WHERE e.ExerciseKey='layToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Once the lay has been achieved, ask them to stand (you can use a treat or a toy for encouragement but only if controlled).' FROM Exercise e WHERE e.ExerciseKey='layToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.' FROM Exercise e WHERE e.ExerciseKey='layToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'This is similar to a push-up, with them using their hindlimbs to push themselves upwards.' FROM Exercise e WHERE e.ExerciseKey='layToStands';

-- Lays to Stands on Incline
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'laysToStandsOnIncline', 'Lays to Stands on Incline', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.
Encourage them to lay facing into the incline with their forelimbs at the highest point.
Once the lay has been achieved, ask them to stand with their forelimbs at the highest point (you can use a treat or a toy for encouragement but only if controlled).
Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.
This is similar to a push-up, with them using their hindlimbs to push themselves upwards.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='general_exercise'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='laysToStandsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.' FROM Exercise e WHERE e.ExerciseKey='laysToStandsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Encourage them to lay facing into the incline with their forelimbs at the highest point.' FROM Exercise e WHERE e.ExerciseKey='laysToStandsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Once the lay has been achieved, ask them to stand with their forelimbs at the highest point (you can use a treat or a toy for encouragement but only if controlled).' FROM Exercise e WHERE e.ExerciseKey='laysToStandsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.' FROM Exercise e WHERE e.ExerciseKey='laysToStandsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'This is similar to a push-up, with them using their hindlimbs to push themselves upwards.' FROM Exercise e WHERE e.ExerciseKey='laysToStandsOnIncline';

-- Narrow walking
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'narrowWalking', 'Narrow walking', 'This exercise should be performed on a flat, non-slip surface with traction.', 'This exercise should be performed on a flat, non-slip surface with traction.
Place a narrow non-slip board flat on the floor.
Guide your dog up and onto the board and ask them to walk forwards, placing one limb in front of the other (think of a tightrope).', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='gait_and_walking'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='narrowWalking';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface with traction.' FROM Exercise e WHERE e.ExerciseKey='narrowWalking';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a narrow non-slip board flat on the floor.' FROM Exercise e WHERE e.ExerciseKey='narrowWalking';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Guide your dog up and onto the board and ask them to walk forwards, placing one limb in front of the other (think of a tightrope).' FROM Exercise e WHERE e.ExerciseKey='narrowWalking';

-- Object on foot
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'objectOnFoot', 'Object on foot', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Perform this in either sit, stand, or lay position (guided by physiotherapist).
Place a foreign object that is not sharp and too big to swallow on your dog’s foot to encourage them to look down towards it.
Use a sticky note or a hair scrunchie.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='limb_lifts_and_proprioception'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='objectOnFoot';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='objectOnFoot';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Perform this in either sit, stand, or lay position (guided by physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='objectOnFoot';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Place a foreign object that is not sharp and too big to swallow on your dog’s foot to encourage them to look down towards it.' FROM Exercise e WHERE e.ExerciseKey='objectOnFoot';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Use a sticky note or a hair scrunchie.' FROM Exercise e WHERE e.ExerciseKey='objectOnFoot';

-- Obstacle course
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'obstacleCourse', 'Obstacle course', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Set up a course of obstacles including cones, poles, ramps, blocks, and cushions (guided by physiotherapist for suitability of obstacles).
Walk the patient through the course without them scuffing/knocking any obstacles.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='obstacleCourse';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='obstacleCourse';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Set up a course of obstacles including cones, poles, ramps, blocks, and cushions (guided by physiotherapist for suitability of obstacles).' FROM Exercise e WHERE e.ExerciseKey='obstacleCourse';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Walk the patient through the course without them scuffing/knocking any obstacles.' FROM Exercise e WHERE e.ExerciseKey='obstacleCourse';

-- Paw brushing
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'pawBrushing', 'Paw brushing', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Stand your dog in a square position and support them either with your arm under their stomach or a sling.
Place your hand behind their hock (ankle) joint and gently lift it upwards.
Protract the limb forwards slightly and drag the paw pad flat against the floor backwards in a circular action.
Imagine how a dog’s leg is supposed to move and mimic the action.
Do this gently and within their capacity as pushing too hard can damage joints.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neurological_rehabilitation'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='pawBrushing';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='pawBrushing';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Stand your dog in a square position and support them either with your arm under their stomach or a sling.' FROM Exercise e WHERE e.ExerciseKey='pawBrushing';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Place your hand behind their hock (ankle) joint and gently lift it upwards.' FROM Exercise e WHERE e.ExerciseKey='pawBrushing';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Protract the limb forwards slightly and drag the paw pad flat against the floor backwards in a circular action.' FROM Exercise e WHERE e.ExerciseKey='pawBrushing';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Imagine how a dog’s leg is supposed to move and mimic the action.' FROM Exercise e WHERE e.ExerciseKey='pawBrushing';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Do this gently and within their capacity as pushing too hard can damage joints.' FROM Exercise e WHERE e.ExerciseKey='pawBrushing';

-- Poles on fan
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'polesOnFan', 'Poles on fan', 'This exercise should be performed on a flat, non-slip surface and at an active walk.', 'This exercise should be performed on a flat, non-slip surface and at an active walk.
Place a marker/cone on the floor with flat poles radiating in equal distances from it.
The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).
If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.
If they are still scuffing poles, you should remove one pole to reduce the intensity of the exercise.
Progression (never progress unless told to by a physiotherapist):
Raise the poles onto the marker/cone one at a time (no higher than your dog’s ankle/hock) and complete evenly in both directions.
If they consistently scuff poles, you will need to lower at least one pole to reduce the exercise intensity.
Distance between poles – keep the alternating foot fall pattern but allow for a slight stretch to extend further through the shoulder and hip joints.
Complete at a trot speed – you may have to adjust the distance between poles.', NULL, NULL, NULL, TRUE, 'Poles on a fan - 25c9273b62a742c8a84dc8c5f396ad17.jpg', 'Poles on a fan - 25c9273b62a742c8a84dc8c5f396ad17.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='polesOnFan';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and at an active walk.' FROM Exercise e WHERE e.ExerciseKey='polesOnFan';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a marker/cone on the floor with flat poles radiating in equal distances from it.' FROM Exercise e WHERE e.ExerciseKey='polesOnFan';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).' FROM Exercise e WHERE e.ExerciseKey='polesOnFan';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.' FROM Exercise e WHERE e.ExerciseKey='polesOnFan';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'If they are still scuffing poles, you should remove one pole to reduce the intensity of the exercise.' FROM Exercise e WHERE e.ExerciseKey='polesOnFan';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Progression (never progress unless told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='polesOnFan';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Raise the poles onto the marker/cone one at a time (no higher than your dog’s ankle/hock) and complete evenly in both directions.' FROM Exercise e WHERE e.ExerciseKey='polesOnFan';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'If they consistently scuff poles, you will need to lower at least one pole to reduce the exercise intensity.' FROM Exercise e WHERE e.ExerciseKey='polesOnFan';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, 'Distance between poles – keep the alternating foot fall pattern but allow for a slight stretch to extend further through the shoulder and hip joints.' FROM Exercise e WHERE e.ExerciseKey='polesOnFan';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 10, 'Complete at a trot speed – you may have to adjust the distance between poles.' FROM Exercise e WHERE e.ExerciseKey='polesOnFan';

-- Poles on fan – Alternative View 1
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'polesOnFanAlternativeView1', 'Poles on fan – Alternative View 1', 'This exercise should be performed on a flat, non-slip surface and at an active walk.', 'This exercise should be performed on a flat, non-slip surface and at an active walk.
Place a marker/cone on the floor with flat poles radiating in equal distances from it.
The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).
If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.
If they are still scuffing poles, you should remove one pole to reduce the intensity of the exercise.
Progression (never progress unless told to by a physiotherapist):
Raise the poles onto the marker/cone one at a time (no higher than your dog’s ankle/hock) and complete evenly in both directions.
If they consistently scuff poles, you will need to lower at least one pole to reduce the exercise intensity.
Distance between poles – keep the alternating foot fall pattern but allow for a slight stretch to extend further through the shoulder and hip joints.
Complete at a trot speed – you may have to adjust the distance between poles.', NULL, NULL, NULL, TRUE, 'Poles on a fan - IMG_0244.jpg', 'Poles on a fan - IMG_0244.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='polesOnFanAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and at an active walk.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a marker/cone on the floor with flat poles radiating in equal distances from it.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'If they are still scuffing poles, you should remove one pole to reduce the intensity of the exercise.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Progression (never progress unless told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Raise the poles onto the marker/cone one at a time (no higher than your dog’s ankle/hock) and complete evenly in both directions.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'If they consistently scuff poles, you will need to lower at least one pole to reduce the exercise intensity.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, 'Distance between poles – keep the alternating foot fall pattern but allow for a slight stretch to extend further through the shoulder and hip joints.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 10, 'Complete at a trot speed – you may have to adjust the distance between poles.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView1';

-- Poles on fan – Alternative View 2
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'polesOnFanAlternativeView2', 'Poles on fan – Alternative View 2', 'This exercise should be performed on a flat, non-slip surface and at an active walk.', 'This exercise should be performed on a flat, non-slip surface and at an active walk.
Place a marker/cone on the floor with flat poles radiating in equal distances from it.
The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).
If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.
If they are still scuffing poles, you should remove one pole to reduce the intensity of the exercise.
Progression (never progress unless told to by a physiotherapist):
Raise the poles onto the marker/cone one at a time (no higher than your dog’s ankle/hock) and complete evenly in both directions.
If they consistently scuff poles, you will need to lower at least one pole to reduce the exercise intensity.
Distance between poles – keep the alternating foot fall pattern but allow for a slight stretch to extend further through the shoulder and hip joints.
Complete at a trot speed – you may have to adjust the distance between poles.', NULL, NULL, NULL, TRUE, 'Poles on a fan - IMG_0246.jpg', 'Poles on a fan - IMG_0246.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='polesOnFanAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and at an active walk.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a marker/cone on the floor with flat poles radiating in equal distances from it.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'If they are still scuffing poles, you should remove one pole to reduce the intensity of the exercise.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Progression (never progress unless told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Raise the poles onto the marker/cone one at a time (no higher than your dog’s ankle/hock) and complete evenly in both directions.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'If they consistently scuff poles, you will need to lower at least one pole to reduce the exercise intensity.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, 'Distance between poles – keep the alternating foot fall pattern but allow for a slight stretch to extend further through the shoulder and hip joints.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 10, 'Complete at a trot speed – you may have to adjust the distance between poles.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanAlternativeView2';

-- Poles on fan – Left Direction
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'polesOnFanLeftDirection', 'Poles on fan – Left Direction', 'This exercise should be performed on a flat, non-slip surface and at an active walk.', 'This exercise should be performed on a flat, non-slip surface and at an active walk.
Place a marker/cone on the floor with flat poles radiating in equal distances from it.
The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).
If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.
If they are still scuffing poles, you should remove one pole to reduce the intensity of the exercise.
Progression (never progress unless told to by a physiotherapist):
Raise the poles onto the marker/cone one at a time (no higher than your dog’s ankle/hock) and complete evenly in both directions.
If they consistently scuff poles, you will need to lower at least one pole to reduce the exercise intensity.
Distance between poles – keep the alternating foot fall pattern but allow for a slight stretch to extend further through the shoulder and hip joints.
Complete at a trot speed – you may have to adjust the distance between poles.', NULL, NULL, NULL, TRUE, 'Poles on a fan to L - IMG_0245.jpg', 'Poles on a fan to L - IMG_0245.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='polesOnFanLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and at an active walk.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a marker/cone on the floor with flat poles radiating in equal distances from it.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).' FROM Exercise e WHERE e.ExerciseKey='polesOnFanLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'If they are still scuffing poles, you should remove one pole to reduce the intensity of the exercise.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Progression (never progress unless told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='polesOnFanLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Raise the poles onto the marker/cone one at a time (no higher than your dog’s ankle/hock) and complete evenly in both directions.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'If they consistently scuff poles, you will need to lower at least one pole to reduce the exercise intensity.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, 'Distance between poles – keep the alternating foot fall pattern but allow for a slight stretch to extend further through the shoulder and hip joints.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanLeftDirection';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 10, 'Complete at a trot speed – you may have to adjust the distance between poles.' FROM Exercise e WHERE e.ExerciseKey='polesOnFanLeftDirection';

-- Poles raised on both sides
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'polesRaisedOnBothSides', 'Poles raised on both sides', 'This exercise should be performed on a flat, non-slip surface and at an active walk.', 'This exercise should be performed on a flat, non-slip surface and at an active walk.
Start by raising one pole on both sides by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.
You can then gradually add more raised poles until you reach all poles raised on both sides.
The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).
Upon completion of the poles, make sure you turn your dog nice and wide.
If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.
If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.
Progression (never progress until told to by a physiotherapist):
Raise one pole on one side by a few centimetres (never higher than your dog’s hock/ankle).
Add to this until you have all poles as cross poles.
Raise one pole on both sides by a few centimetres (never higher than your dog’s hock/ankle).
Add to this until you have all poles raised on both sides.
Distance between poles – keep the alternating foot fall pattern but allow for a slight stretch to extend further through the shoulder and hip joints.
Complete at a trot speed – you may have to adjust the distance between poles.', NULL, NULL, NULL, TRUE, 'Poles with both sides raised - 7fc1cf9f417845cfb790a2f07e6d0f0b.jpg', 'Poles with both sides raised - 7fc1cf9f417845cfb790a2f07e6d0f0b.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='polesRaisedOnBothSides';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and at an active walk.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSides';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Start by raising one pole on both sides by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSides';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'You can then gradually add more raised poles until you reach all poles raised on both sides.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSides';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSides';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Upon completion of the poles, make sure you turn your dog nice and wide.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSides';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSides';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSides';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Progression (never progress until told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSides';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, 'Raise one pole on one side by a few centimetres (never higher than your dog’s hock/ankle).' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSides';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 10, 'Add to this until you have all poles as cross poles.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSides';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 11, 'Raise one pole on both sides by a few centimetres (never higher than your dog’s hock/ankle).' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSides';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 12, 'Add to this until you have all poles raised on both sides.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSides';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 13, 'Distance between poles – keep the alternating foot fall pattern but allow for a slight stretch to extend further through the shoulder and hip joints.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSides';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 14, 'Complete at a trot speed – you may have to adjust the distance between poles.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSides';

-- Poles raised on both sides – Alternative View 1
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'polesRaisedOnBothSidesAlternativeView1', 'Poles raised on both sides – Alternative View 1', 'This exercise should be performed on a flat, non-slip surface and at an active walk.', 'This exercise should be performed on a flat, non-slip surface and at an active walk.
Start by raising one pole on both sides by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.
You can then gradually add more raised poles until you reach all poles raised on both sides.
The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).
Upon completion of the poles, make sure you turn your dog nice and wide.
If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.
If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.
Progression (never progress until told to by a physiotherapist):
Raise one pole on one side by a few centimetres (never higher than your dog’s hock/ankle).
Add to this until you have all poles as cross poles.
Raise one pole on both sides by a few centimetres (never higher than your dog’s hock/ankle).
Add to this until you have all poles raised on both sides.
Distance between poles – keep the alternating foot fall pattern but allow for a slight stretch to extend further through the shoulder and hip joints.
Complete at a trot speed – you may have to adjust the distance between poles.', NULL, NULL, NULL, TRUE, 'Poles with both sides raised - IMG_0236.jpg', 'Poles with both sides raised - IMG_0236.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and at an active walk.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Start by raising one pole on both sides by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'You can then gradually add more raised poles until you reach all poles raised on both sides.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Upon completion of the poles, make sure you turn your dog nice and wide.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Progression (never progress until told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, 'Raise one pole on one side by a few centimetres (never higher than your dog’s hock/ankle).' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 10, 'Add to this until you have all poles as cross poles.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 11, 'Raise one pole on both sides by a few centimetres (never higher than your dog’s hock/ankle).' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 12, 'Add to this until you have all poles raised on both sides.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 13, 'Distance between poles – keep the alternating foot fall pattern but allow for a slight stretch to extend further through the shoulder and hip joints.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 14, 'Complete at a trot speed – you may have to adjust the distance between poles.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView1';

-- Poles raised on both sides – Alternative View 2
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'polesRaisedOnBothSidesAlternativeView2', 'Poles raised on both sides – Alternative View 2', 'This exercise should be performed on a flat, non-slip surface and at an active walk.', 'This exercise should be performed on a flat, non-slip surface and at an active walk.
Start by raising one pole on both sides by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.
You can then gradually add more raised poles until you reach all poles raised on both sides.
The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).
Upon completion of the poles, make sure you turn your dog nice and wide.
If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.
If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.
Progression (never progress until told to by a physiotherapist):
Raise one pole on one side by a few centimetres (never higher than your dog’s hock/ankle).
Add to this until you have all poles as cross poles.
Raise one pole on both sides by a few centimetres (never higher than your dog’s hock/ankle).
Add to this until you have all poles raised on both sides.
Distance between poles – keep the alternating foot fall pattern but allow for a slight stretch to extend further through the shoulder and hip joints.
Complete at a trot speed – you may have to adjust the distance between poles.', NULL, NULL, NULL, TRUE, 'Poles with both sides raised - IMG_0237.jpg', 'Poles with both sides raised - IMG_0237.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and at an active walk.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Start by raising one pole on both sides by a few centimetres (no higher than your dog’s ankle/hock) and ask your dog to walk over the poles evenly in both directions.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'You can then gradually add more raised poles until you reach all poles raised on both sides.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'The poles should be distanced so there is an alternating foot fall pattern (i.e., left fore in gap one and right fore in gap 2).' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Upon completion of the poles, make sure you turn your dog nice and wide.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'If they are still scuffing poles, you should lower one pole to reduce the intensity of the exercise.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Progression (never progress until told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, 'Raise one pole on one side by a few centimetres (never higher than your dog’s hock/ankle).' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 10, 'Add to this until you have all poles as cross poles.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 11, 'Raise one pole on both sides by a few centimetres (never higher than your dog’s hock/ankle).' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 12, 'Add to this until you have all poles raised on both sides.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 13, 'Distance between poles – keep the alternating foot fall pattern but allow for a slight stretch to extend further through the shoulder and hip joints.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 14, 'Complete at a trot speed – you may have to adjust the distance between poles.' FROM Exercise e WHERE e.ExerciseKey='polesRaisedOnBothSidesAlternativeView2';

-- Posture lay
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'postureLay', 'Posture lay', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Ask for a square lay and then guide their head forward with a treat without letting them move forwards.
This should elongate the spine with stifles/knees moving forwards over the hindlimb digits.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='postureLay';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='postureLay';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask for a square lay and then guide their head forward with a treat without letting them move forwards.' FROM Exercise e WHERE e.ExerciseKey='postureLay';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'This should elongate the spine with stifles/knees moving forwards over the hindlimb digits.' FROM Exercise e WHERE e.ExerciseKey='postureLay';

-- Raised baited neck mobilisations – Cranial / Step Ups
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedBaitedNeckMobilisationsCranialStepUps', 'Raised baited neck mobilisations – Cranial / Step Ups', 'This exercise should be performed on a flat, stable surface with a treat in hand.', 'This exercise should be performed on a flat, stable surface with a treat in hand.
This action should be relatively quick, without allowing them to move their limbs, and does not require a hold.
Raise your dog’s forelimbs and/or hindlimbs onto stable block(s)/cushion(s) (as guided by your physiotherapist).
Cranial/Step ups: Take your hand in front of your dog’s nose and guide their head forwards and then back to neutral.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_mobilisation'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsCranialStepUps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, stable surface with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsCranialStepUps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'This action should be relatively quick, without allowing them to move their limbs, and does not require a hold.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsCranialStepUps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Raise your dog’s forelimbs and/or hindlimbs onto stable block(s)/cushion(s) (as guided by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsCranialStepUps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Cranial/Step ups: Take your hand in front of your dog’s nose and guide their head forwards and then back to neutral.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsCranialStepUps';

-- Raised baited neck mobilisations – Lateral
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedBaitedNeckMobilisationsLateral', 'Raised baited neck mobilisations – Lateral', 'This exercise should be performed on a flat, stable surface with a treat in hand.', 'This exercise should be performed on a flat, stable surface with a treat in hand.
This action should be relatively quick, without allowing them to move their limbs, and does not require a hold.
Raise your dog’s forelimbs and/or hindlimbs onto stable block(s)/cushion(s) (as guided by your physiotherapist).
Lateral: Take your hand from your dog’s nose from side to side in a relatively quick motion without allowing them to move their feet.', NULL, NULL, NULL, TRUE, 'Raised baited neck mobilisations - IMG_0182.jpg', 'Raised baited neck mobilisations - IMG_0182.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_mobilisation'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, stable surface with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'This action should be relatively quick, without allowing them to move their limbs, and does not require a hold.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Raise your dog’s forelimbs and/or hindlimbs onto stable block(s)/cushion(s) (as guided by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Lateral: Take your hand from your dog’s nose from side to side in a relatively quick motion without allowing them to move their feet.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsLateral';

-- Raised baited neck mobilisations – Ventral
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedBaitedNeckMobilisationsVentral', 'Raised baited neck mobilisations – Ventral', 'This exercise should be performed on a flat, stable surface with a treat in hand.', 'This exercise should be performed on a flat, stable surface with a treat in hand.
This action should be relatively quick, without allowing them to move their limbs, and does not require a hold.
Raise your dog’s forelimbs and/or hindlimbs onto stable block(s)/cushion(s) (as guided by your physiotherapist).
Ventral: Encourage their head down (to the level specified by your physiotherapist) without your dog laying down by moving a treat between the forelimbs from the level of their nose down.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_mobilisation'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, stable surface with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'This action should be relatively quick, without allowing them to move their limbs, and does not require a hold.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Raise your dog’s forelimbs and/or hindlimbs onto stable block(s)/cushion(s) (as guided by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Ventral: Encourage their head down (to the level specified by your physiotherapist) without your dog laying down by moving a treat between the forelimbs from the level of their nose down.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckMobilisationsVentral';

-- Raised baited neck stretches – Cranial / Step Ups
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedBaitedNeckStretchesCranialStepUps', 'Raised baited neck stretches – Cranial / Step Ups', 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.', 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.
Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion (as directed by your physiotherapist) a couple of inches high.
Cranial/Step ups: Take your hand in front of your dog’s nose and guide their head forwards and hold the position.
You may need to provide support with a hand either at the sternum or in front of their hindlimbs.
Remember to always return to neutral before asking for another stretch.
Do not take your dog’s head beyond point of comfort.', NULL, NULL, NULL, TRUE, 'Step ups - 81cf2d85ad724accb4ac22538d0838c9.jpg', 'Step ups - 81cf2d85ad724accb4ac22538d0838c9.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_stretching'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedBaitedNeckStretchesCranialStepUps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesCranialStepUps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion (as directed by your physiotherapist) a couple of inches high.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesCranialStepUps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Cranial/Step ups: Take your hand in front of your dog’s nose and guide their head forwards and hold the position.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesCranialStepUps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'You may need to provide support with a hand either at the sternum or in front of their hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesCranialStepUps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Remember to always return to neutral before asking for another stretch.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesCranialStepUps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Do not take your dog’s head beyond point of comfort.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesCranialStepUps';

-- Raised baited neck stretches – Lateral
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedBaitedNeckStretchesLateral', 'Raised baited neck stretches – Lateral', 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.', 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.
Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion (as directed by your physiotherapist) a couple of inches high.
Lateral: Take your hand from your dog’s nose to the side, stopping just before they start to rotate their head (to the level specified by your physiotherapist).
Hold this position for 7s then return to neutral head position (i.e., normal straight head position).
Repeat the action to the other side after a few seconds.
You can stand next to them at their shoulder and encourage their head around your leg.
Remember to always return to neutral before asking for another stretch.
Do not take your dog’s head beyond point of comfort.', NULL, NULL, 7, TRUE, 'Raised lateral neck stretches - 91e5ebcbb8604959ba30a47cbfc25b33.jpg', 'Raised lateral neck stretches - 91e5ebcbb8604959ba30a47cbfc25b33.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_stretching'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion (as directed by your physiotherapist) a couple of inches high.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Lateral: Take your hand from your dog’s nose to the side, stopping just before they start to rotate their head (to the level specified by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Hold this position for 7s then return to neutral head position (i.e., normal straight head position).' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Repeat the action to the other side after a few seconds.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'You can stand next to them at their shoulder and encourage their head around your leg.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Remember to always return to neutral before asking for another stretch.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Do not take your dog’s head beyond point of comfort.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesLateral';

-- Raised baited neck stretches – Ventral
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedBaitedNeckStretchesVentral', 'Raised baited neck stretches – Ventral', 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.', 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.
Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion (as directed by your physiotherapist) a couple of inches high.
Ventral: Encourage their head down (to the level specified by your physiotherapist) without your dog laying down to stretch the muscles on the top of the neck by holding a treat between the forelimbs.
Remember to always return to neutral before asking for another stretch.
Do not take your dog’s head beyond point of comfort.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_stretching'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedBaitedNeckStretchesVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion (as directed by your physiotherapist) a couple of inches high.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Ventral: Encourage their head down (to the level specified by your physiotherapist) without your dog laying down to stretch the muscles on the top of the neck by holding a treat between the forelimbs.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Remember to always return to neutral before asking for another stretch.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Do not take your dog’s head beyond point of comfort.' FROM Exercise e WHERE e.ExerciseKey='raisedBaitedNeckStretchesVentral';

-- Raised forelimb sit to stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedForelimbSitToStands', 'Raised forelimb sit to stands', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with forelimb paws on a firm block/cushion a couple of inches tall and hindlimbs flat on the floor.', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with forelimb paws on a firm block/cushion a couple of inches tall and hindlimbs flat on the floor.
Once the sit has been achieved, ask them to stand with their forelimbs still on the block/cushion (you can use a treat or a toy for encouragement but only if controlled).
Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.
This is similar to a squat, with them using their hindlimbs to push themselves upwards.
If necessary, you can use wall support by placing their weaker side next to a wall to prevent splaying/rotation of hindlimbs.', NULL, NULL, NULL, TRUE, 'Raised FL sit to stands - 8d9f8440eb034b969fa7c0e3f33050d4.jpg', 'Raised FL sit to stands - 8d9f8440eb034b969fa7c0e3f33050d4.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedForelimbSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with forelimb paws on a firm block/cushion a couple of inches tall and hindlimbs flat on the floor.' FROM Exercise e WHERE e.ExerciseKey='raisedForelimbSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Once the sit has been achieved, ask them to stand with their forelimbs still on the block/cushion (you can use a treat or a toy for encouragement but only if controlled).' FROM Exercise e WHERE e.ExerciseKey='raisedForelimbSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.' FROM Exercise e WHERE e.ExerciseKey='raisedForelimbSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'This is similar to a squat, with them using their hindlimbs to push themselves upwards.' FROM Exercise e WHERE e.ExerciseKey='raisedForelimbSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'If necessary, you can use wall support by placing their weaker side next to a wall to prevent splaying/rotation of hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='raisedForelimbSitToStands';

-- Raised forelimb stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedForelimbStands', 'Raised forelimb stands', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Raise your dog’s forelimbs and/or hindlimbs onto a flat stable platform or cushion (as directed by your physiotherapist) and encourage them to stand.
Then let them rest before repeating the exercise.
You may have to assist by placing your hands either side of their chest but try to let them maintain their own balance.
Aim to increase the time standing by 5s each week but do not progress if your dog fatigues quickly.', NULL, NULL, NULL, TRUE, 'Incline stands_Raised FL stands - IMG_1199.jpg', NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedForelimbStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='raisedForelimbStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Raise your dog’s forelimbs and/or hindlimbs onto a flat stable platform or cushion (as directed by your physiotherapist) and encourage them to stand.' FROM Exercise e WHERE e.ExerciseKey='raisedForelimbStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Then let them rest before repeating the exercise.' FROM Exercise e WHERE e.ExerciseKey='raisedForelimbStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'You may have to assist by placing your hands either side of their chest but try to let them maintain their own balance.' FROM Exercise e WHERE e.ExerciseKey='raisedForelimbStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Aim to increase the time standing by 5s each week but do not progress if your dog fatigues quickly.' FROM Exercise e WHERE e.ExerciseKey='raisedForelimbStands';

-- Raised hindlimb and forelimb sit to stands / Balance sit to stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedHindlimbAndForelimbSitToStandsBalanceSitToStands', 'Raised hindlimb and forelimb sit to stands / Balance sit to stands', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with forelimbs and hindlimbs on 2 firm blocks/cushions a couple of inches tall.', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with forelimbs and hindlimbs on 2 firm blocks/cushions a couple of inches tall.
Once the sit has been achieved, ask them to stand with all their feet still on the blocks/cushions (you can use a treat or a toy for encouragement but only if controlled).
Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.
This is similar to a squat, with them using their hindlimbs to push themselves upwards.
If necessary, you can use wall support by placing their weaker side next to a wall to prevent splaying/rotation of hindlimbs.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedHindlimbAndForelimbSitToStandsBalanceSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with forelimbs and hindlimbs on 2 firm blocks/cushions a couple of inches tall.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbAndForelimbSitToStandsBalanceSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Once the sit has been achieved, ask them to stand with all their feet still on the blocks/cushions (you can use a treat or a toy for encouragement but only if controlled).' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbAndForelimbSitToStandsBalanceSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbAndForelimbSitToStandsBalanceSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'This is similar to a squat, with them using their hindlimbs to push themselves upwards.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbAndForelimbSitToStandsBalanceSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'If necessary, you can use wall support by placing their weaker side next to a wall to prevent splaying/rotation of hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbAndForelimbSitToStandsBalanceSitToStands';

-- Raised hindlimb lays to stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedHindlimbLaysToStands', 'Raised hindlimb lays to stands', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.
Place their forelimbs and/or hindlimbs on a block/cushion (as directed by your physiotherapist).
Once the lay has been achieved, ask them to stand (you can use a treat or a toy for encouragement but only if controlled).
Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.
Make sure your dog’s forelimbs and hindlimbs stay on the raised platform(s) at all times during this motion.
This is similar to a push-up, with them using their hindlimbs to push themselves upwards.', NULL, NULL, NULL, TRUE, 'Raised HL bows - IMG_0181.jpg', 'Raised HL bows - IMG_0181.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='general_exercise'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedHindlimbLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place their forelimbs and/or hindlimbs on a block/cushion (as directed by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Once the lay has been achieved, ask them to stand (you can use a treat or a toy for encouragement but only if controlled).' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Make sure your dog’s forelimbs and hindlimbs stay on the raised platform(s) at all times during this motion.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'This is similar to a push-up, with them using their hindlimbs to push themselves upwards.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbLaysToStands';

-- Raised hindlimb sit to stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedHindlimbSitToStands', 'Raised hindlimb sit to stands', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with hindlimb paws on a firm block/cushion a couple of inches tall and forelimbs flat on the floor.', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with hindlimb paws on a firm block/cushion a couple of inches tall and forelimbs flat on the floor.
Once the sit has been achieved, ask them to stand with their hindlimbs still on the block/cushion (you can use a treat or a toy for encouragement but only if controlled).
Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.
This is similar to a squat, with them using their hindlimbs to push themselves upwards.
If necessary, you can use wall support by backing them into a corner and placing a cushion to the side of their hindlimbs to prevent splaying/rotation of hindlimbs.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedHindlimbSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with hindlimb paws on a firm block/cushion a couple of inches tall and forelimbs flat on the floor.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Once the sit has been achieved, ask them to stand with their hindlimbs still on the block/cushion (you can use a treat or a toy for encouragement but only if controlled).' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'This is similar to a squat, with them using their hindlimbs to push themselves upwards.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'If necessary, you can use wall support by backing them into a corner and placing a cushion to the side of their hindlimbs to prevent splaying/rotation of hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbSitToStands';

-- Raised hindlimb stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedHindlimbStands', 'Raised hindlimb stands', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Raise your dog’s forelimbs and/or hindlimbs onto a flat stable platform or cushion (as directed by your physiotherapist) and encourage them to stand.
Then let them rest before repeating the exercise.
You may have to assist by placing your hands either side of their chest but try to let them maintain their own balance.
Aim to increase the time standing by 5s each week but do not progress if your dog fatigues quickly.', NULL, NULL, NULL, TRUE, 'Raised HL stands - IMG_0180.jpg', 'Raised HL stands - IMG_0180.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedHindlimbStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Raise your dog’s forelimbs and/or hindlimbs onto a flat stable platform or cushion (as directed by your physiotherapist) and encourage them to stand.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Then let them rest before repeating the exercise.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'You may have to assist by placing your hands either side of their chest but try to let them maintain their own balance.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Aim to increase the time standing by 5s each week but do not progress if your dog fatigues quickly.' FROM Exercise e WHERE e.ExerciseKey='raisedHindlimbStands';

-- Raised Lays to Stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedLaysToStands', 'Raised Lays to Stands', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.
Place their forelimbs and/or hindlimbs on a block/cushion (as directed by your physiotherapist).
Once the lay has been achieved, ask them to stand (you can use a treat or a toy for encouragement but only if controlled).
Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.
Make sure your dog’s forelimbs and hindlimbs stay on the raised platform(s) at all times during this motion.
This is similar to a push-up, with them using their hindlimbs to push themselves upwards.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='general_exercise'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.' FROM Exercise e WHERE e.ExerciseKey='raisedLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place their forelimbs and/or hindlimbs on a block/cushion (as directed by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='raisedLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Once the lay has been achieved, ask them to stand (you can use a treat or a toy for encouragement but only if controlled).' FROM Exercise e WHERE e.ExerciseKey='raisedLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.' FROM Exercise e WHERE e.ExerciseKey='raisedLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Make sure your dog’s forelimbs and hindlimbs stay on the raised platform(s) at all times during this motion.' FROM Exercise e WHERE e.ExerciseKey='raisedLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'This is similar to a push-up, with them using their hindlimbs to push themselves upwards.' FROM Exercise e WHERE e.ExerciseKey='raisedLaysToStands';

-- Raised Lays to Stands on Incline
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedLaysToStandsOnIncline', 'Raised Lays to Stands on Incline', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.
Place their forelimbs and/or hindlimbs on a block/cushion (as directed by your physiotherapist).
Encourage them to lay facing into the incline with their forelimbs at the highest point.
Once the lay has been achieved, ask them to stand with their forelimbs at the highest point (you can use a treat or a toy for encouragement but only if controlled).
Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.
Make sure your dog’s forelimbs and hindlimbs stay on the raised platforms at all times during this motion.
This is similar to a push-up, with them using their hindlimbs to push themselves upwards.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='general_exercise'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedLaysToStandsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square lay with your dog’s legs underneath themselves.' FROM Exercise e WHERE e.ExerciseKey='raisedLaysToStandsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place their forelimbs and/or hindlimbs on a block/cushion (as directed by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='raisedLaysToStandsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Encourage them to lay facing into the incline with their forelimbs at the highest point.' FROM Exercise e WHERE e.ExerciseKey='raisedLaysToStandsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Once the lay has been achieved, ask them to stand with their forelimbs at the highest point (you can use a treat or a toy for encouragement but only if controlled).' FROM Exercise e WHERE e.ExerciseKey='raisedLaysToStandsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.' FROM Exercise e WHERE e.ExerciseKey='raisedLaysToStandsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Make sure your dog’s forelimbs and hindlimbs stay on the raised platforms at all times during this motion.' FROM Exercise e WHERE e.ExerciseKey='raisedLaysToStandsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'This is similar to a push-up, with them using their hindlimbs to push themselves upwards.' FROM Exercise e WHERE e.ExerciseKey='raisedLaysToStandsOnIncline';

-- Raised limb lifts
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedLimbLifts', 'Raised limb lifts', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Ask your dog to stand square with forelimbs and/or hindlimbs raised onto flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist) then ask for their paw.
Depending on their level of training, you may have to start by picking up the limb yourself at first.
You should support the forelimb at above the wrist/carpus and the hindlimb below the ankle/hock.
Bring the paw upwards slightly to gain a little flexion from the elbow (forelimb) or stifle/knee (hindlimb) joint.
You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.
Hold the position for a few seconds before gently placing the limb back on the floor.
You may have to provide support under the stomach with your hand when lifting the hindlimbs.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='limb_lifts_and_proprioception'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask your dog to stand square with forelimbs and/or hindlimbs raised onto flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist) then ask for their paw.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Depending on their level of training, you may have to start by picking up the limb yourself at first.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'You should support the forelimb at above the wrist/carpus and the hindlimb below the ankle/hock.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Bring the paw upwards slightly to gain a little flexion from the elbow (forelimb) or stifle/knee (hindlimb) joint.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Hold the position for a few seconds before gently placing the limb back on the floor.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLifts';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'You may have to provide support under the stomach with your hand when lifting the hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLifts';

-- Raised limb lifts on incline
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedLimbLiftsOnIncline', 'Raised limb lifts on incline', 'This exercise should be performed on a non-slip surface with your dog standing square.', 'This exercise should be performed on a non-slip surface with your dog standing square.
Use a slight incline and face your dog with forelimbs at the highest point.
Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist).
Allow your dog time to balance and then gently pick up each foot one at a time, making sure to gently flex through the elbow joint (forelimb) and/or stifle joint (hindlimb).
You should support the forelimb at above the wrist/carpus and the hindlimb below the ankle/hock.
The limb should be parallel to the floor at a 90-degree angle.
If your dog is struggling, remove the incline.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='limb_lifts_and_proprioception'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedLimbLiftsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a non-slip surface with your dog standing square.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Use a slight incline and face your dog with forelimbs at the highest point.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Allow your dog time to balance and then gently pick up each foot one at a time, making sure to gently flex through the elbow joint (forelimb) and/or stifle joint (hindlimb).' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'You should support the forelimb at above the wrist/carpus and the hindlimb below the ankle/hock.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'The limb should be parallel to the floor at a 90-degree angle.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsOnIncline';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'If your dog is struggling, remove the incline.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsOnIncline';

-- Raised limb lifts – Forelimb / Give paw
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedLimbLiftsForelimbGivePaw', 'Raised limb lifts – Forelimb / Give paw', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Ask your dog to stand square with forelimbs and/or hindlimbs raised onto flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist) then ask for their paw.
Depending on their level of training, you may have to start by picking up the limb yourself at first.
You should support the forelimb at above the wrist/carpus and the hindlimb below the ankle/hock.
Bring the paw upwards slightly to gain a little flexion from the elbow (forelimb) or stifle/knee (hindlimb) joint.
You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.
Hold the position for a few seconds before gently placing the limb back on the floor.
You may have to provide support under the stomach with your hand when lifting the hindlimbs.', NULL, NULL, NULL, TRUE, 'Raised FL limb lifts_give paw - IMG_0176.jpg', 'Raised FL limb lifts_give paw - IMG_0176.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='limb_lifts_and_proprioception'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedLimbLiftsForelimbGivePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsForelimbGivePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask your dog to stand square with forelimbs and/or hindlimbs raised onto flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist) then ask for their paw.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsForelimbGivePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Depending on their level of training, you may have to start by picking up the limb yourself at first.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsForelimbGivePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'You should support the forelimb at above the wrist/carpus and the hindlimb below the ankle/hock.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsForelimbGivePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Bring the paw upwards slightly to gain a little flexion from the elbow (forelimb) or stifle/knee (hindlimb) joint.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsForelimbGivePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'You should aim for a 90-degree angle so the limb is parallel to the floor, but do not force the joint beyond the point of comfort.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsForelimbGivePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Hold the position for a few seconds before gently placing the limb back on the floor.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsForelimbGivePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'You may have to provide support under the stomach with your hand when lifting the hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='raisedLimbLiftsForelimbGivePaw';

-- Raised pivot / Around the world
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedPivotAroundTheWorld', 'Raised pivot / Around the world', 'Perform this exercise on a flat, non-slip surface with your dog standing square to start.', 'Perform this exercise on a flat, non-slip surface with your dog standing square to start.
Place your dog’s forelimbs onto a flat non-slip block/cushion (as directed by your physiotherapist) a few inches high.
Stand in front of your dog with a treat in hand and start to walk slowly in a circle around them, encouraging them to turn with you whilst keeping their forelimbs firmly on the block.
The hindlimbs will move to the side causing a pivot around the forelimbs on the block at the centre.
Make sure to perform this exercise evenly in both directions and keep to a slow and steady pace.
Once your dog becomes more proficient, you can start to speed up slightly without causing them to stumble over their hindlimbs.', NULL, NULL, NULL, TRUE, 'Around the Worlds_Pivots - 29d13df399cc4d188f4188f931f4ddcd.jpg', 'Around the Worlds_Pivots - 29d13df399cc4d188f4188f931f4ddcd.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedPivotAroundTheWorld';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this exercise on a flat, non-slip surface with your dog standing square to start.' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorld';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place your dog’s forelimbs onto a flat non-slip block/cushion (as directed by your physiotherapist) a few inches high.' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorld';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Stand in front of your dog with a treat in hand and start to walk slowly in a circle around them, encouraging them to turn with you whilst keeping their forelimbs firmly on the block.' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorld';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'The hindlimbs will move to the side causing a pivot around the forelimbs on the block at the centre.' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorld';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Make sure to perform this exercise evenly in both directions and keep to a slow and steady pace.' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorld';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Once your dog becomes more proficient, you can start to speed up slightly without causing them to stumble over their hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorld';

-- Raised pivot / Around the world with pole(s)
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedPivotAroundTheWorldWithPoleS', 'Raised pivot / Around the world with pole(s)', 'Perform this exercise on a flat, non-slip surface with your dog standing square to start.', 'Perform this exercise on a flat, non-slip surface with your dog standing square to start.
Place your dog’s forelimbs onto a flat non-slip block/cushion (as directed by your physiotherapist) a few inches high.
A flat pole should be placed on the floor in the centre of the block/cushion.
Stand in front of your dog with a treat in hand and start to walk slowly in a circle around them, encouraging them to turn with you whilst keeping their forelimbs firmly on the block.
The hindlimbs will move to the side up and over the pole, causing a pivot around the forelimbs on the block at the centre.
Make sure to perform this exercise evenly in both directions and keep to a slow and steady pace.
Once your dog becomes more proficient, you can start to speed up slightly without causing them to stumble over their hindlimbs.
Progression (only once directed by your physiotherapist):
Add more poles - distanced so they take one step in between each gap
Raise the poles onto the block/cushion – no higher than ankle/tarsus height and one at a time', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedPivotAroundTheWorldWithPoleS';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this exercise on a flat, non-slip surface with your dog standing square to start.' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorldWithPoleS';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place your dog’s forelimbs onto a flat non-slip block/cushion (as directed by your physiotherapist) a few inches high.' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorldWithPoleS';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'A flat pole should be placed on the floor in the centre of the block/cushion.' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorldWithPoleS';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Stand in front of your dog with a treat in hand and start to walk slowly in a circle around them, encouraging them to turn with you whilst keeping their forelimbs firmly on the block.' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorldWithPoleS';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'The hindlimbs will move to the side up and over the pole, causing a pivot around the forelimbs on the block at the centre.' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorldWithPoleS';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Make sure to perform this exercise evenly in both directions and keep to a slow and steady pace.' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorldWithPoleS';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Once your dog becomes more proficient, you can start to speed up slightly without causing them to stumble over their hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorldWithPoleS';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Progression (only once directed by your physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorldWithPoleS';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, 'Add more poles - distanced so they take one step in between each gap' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorldWithPoleS';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 10, 'Raise the poles onto the block/cushion – no higher than ankle/tarsus height and one at a time' FROM Exercise e WHERE e.ExerciseKey='raisedPivotAroundTheWorldWithPoleS';

-- Raised Stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedStands', 'Raised Stands', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Raise your dog’s forelimbs and/or hindlimbs onto a flat stable platform or cushion (as directed by your physiotherapist) and encourage them to stand.
Then let them rest before repeating the exercise.
You may have to assist by placing your hands either side of their chest but try to let them maintain their own balance.
Aim to increase the time standing by 5s each week but do not progress if your dog fatigues quickly.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='raisedStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Raise your dog’s forelimbs and/or hindlimbs onto a flat stable platform or cushion (as directed by your physiotherapist) and encourage them to stand.' FROM Exercise e WHERE e.ExerciseKey='raisedStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Then let them rest before repeating the exercise.' FROM Exercise e WHERE e.ExerciseKey='raisedStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'You may have to assist by placing your hands either side of their chest but try to let them maintain their own balance.' FROM Exercise e WHERE e.ExerciseKey='raisedStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Aim to increase the time standing by 5s each week but do not progress if your dog fatigues quickly.' FROM Exercise e WHERE e.ExerciseKey='raisedStands';

-- Raised Weight Shifting on incline – Craniocaudal
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedWeightShiftingOnInclineCraniocaudal', 'Raised Weight Shifting on incline – Craniocaudal', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.
Use a slight incline and face your dog with forelimbs at the highest point.
Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist).
Craniocaudal: Cup your hands over the front of your dog’s shoulders (you should feel bony prominences to hold) whilst you stand behind them and gently pull them towards you in a fore-back motion.
This should be a slow and gentle movement, just enough to feel a contraction under your hands.
We don’t want your dog moving their feet so, if they start to do this, slow the action down.
Additional techniques:
Perturbations: rhythmic gentle pressure in multiple directions
Rebounding: adding a hold time before allowing return to neutral/mid-line position
[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedWeightShiftingOnInclineCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface with your dog standing square.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Use a slight incline and face your dog with forelimbs at the highest point.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Craniocaudal: Cup your hands over the front of your dog’s shoulders (you should feel bony prominences to hold) whilst you stand behind them and gently pull them towards you in a fore-back motion.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'This should be a slow and gentle movement, just enough to feel a contraction under your hands.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'We don’t want your dog moving their feet so, if they start to do this, slow the action down.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Additional techniques:' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Perturbations: rhythmic gentle pressure in multiple directions' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, 'Rebounding: adding a hold time before allowing return to neutral/mid-line position' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 10, '[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineCraniocaudal';

-- Raised Weight Shifting on incline – Lateral Forelimbs
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedWeightShiftingOnInclineLateralForelimbs', 'Raised Weight Shifting on incline – Lateral Forelimbs', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.
Use a slight incline and face your dog with forelimbs at the highest point.
Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist).
Lateral forelimbs: Place your hands on your dog’s shoulders and gently rock them from side-to-side.
This should be a slow and gentle movement, just enough to feel a contraction under your hands.
We don’t want your dog moving their feet so, if they start to do this, slow the action down.
Additional techniques:
Perturbations: rhythmic gentle pressure in multiple directions
Rebounding: adding a hold time before allowing return to neutral/mid-line position
[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface with your dog standing square.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Use a slight incline and face your dog with forelimbs at the highest point.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Lateral forelimbs: Place your hands on your dog’s shoulders and gently rock them from side-to-side.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'This should be a slow and gentle movement, just enough to feel a contraction under your hands.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'We don’t want your dog moving their feet so, if they start to do this, slow the action down.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Additional techniques:' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Perturbations: rhythmic gentle pressure in multiple directions' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, 'Rebounding: adding a hold time before allowing return to neutral/mid-line position' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 10, '[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralForelimbs';

-- Raised Weight Shifting on incline – Lateral Hindlimbs
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedWeightShiftingOnInclineLateralHindlimbs', 'Raised Weight Shifting on incline – Lateral Hindlimbs', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.
Use a slight incline and face your dog with forelimbs at the highest point.
Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist).
Lateral hindlimbs: Place your hands over their hips and gently rock them side-to-side.
This should be a slow and gentle movement, just enough to feel a contraction under your hands.
We don’t want your dog moving their feet so, if they start to do this, slow the action down.
Additional techniques:
Perturbations: rhythmic gentle pressure in multiple directions
Rebounding: adding a hold time before allowing return to neutral/mid-line position
[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface with your dog standing square.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Use a slight incline and face your dog with forelimbs at the highest point.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Lateral hindlimbs: Place your hands over their hips and gently rock them side-to-side.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'This should be a slow and gentle movement, just enough to feel a contraction under your hands.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'We don’t want your dog moving their feet so, if they start to do this, slow the action down.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Additional techniques:' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Perturbations: rhythmic gentle pressure in multiple directions' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, 'Rebounding: adding a hold time before allowing return to neutral/mid-line position' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 10, '[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingOnInclineLateralHindlimbs';

-- Raised Weight Shifting – Craniocaudal
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedWeightShiftingCraniocaudal', 'Raised Weight Shifting – Craniocaudal', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.
Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist).
Craniocaudal: Cup your hands over the front of your dog’s shoulders (you should feel bony prominences to hold) whilst you stand behind them and gently pull them towards you in a fore-back motion.
This should be a slow and gentle movement, just enough to feel a contraction under your hands.
We don’t want your dog moving their feet so, if they start to do this, slow the action down.
Additional techniques:
Perturbations: rhythmic gentle pressure in multiple directions
Rebounding: adding a hold time before allowing return to neutral/mid-line position
[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface with your dog standing square.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Craniocaudal: Cup your hands over the front of your dog’s shoulders (you should feel bony prominences to hold) whilst you stand behind them and gently pull them towards you in a fore-back motion.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'This should be a slow and gentle movement, just enough to feel a contraction under your hands.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'We don’t want your dog moving their feet so, if they start to do this, slow the action down.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Additional techniques:' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Perturbations: rhythmic gentle pressure in multiple directions' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Rebounding: adding a hold time before allowing return to neutral/mid-line position' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, '[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingCraniocaudal';

-- Raised Weight Shifting – Lateral Forelimbs
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedWeightShiftingLateralForelimbs', 'Raised Weight Shifting – Lateral Forelimbs', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.
Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist).
Lateral forelimbs: Place your hands on your dog’s shoulders and gently rock them from side-to-side.
This should be a slow and gentle movement, just enough to feel a contraction under your hands.
We don’t want your dog moving their feet so, if they start to do this, slow the action down.
Additional techniques:
Perturbations: rhythmic gentle pressure in multiple directions
Rebounding: adding a hold time before allowing return to neutral/mid-line position
[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface with your dog standing square.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Lateral forelimbs: Place your hands on your dog’s shoulders and gently rock them from side-to-side.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'This should be a slow and gentle movement, just enough to feel a contraction under your hands.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'We don’t want your dog moving their feet so, if they start to do this, slow the action down.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Additional techniques:' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Perturbations: rhythmic gentle pressure in multiple directions' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Rebounding: adding a hold time before allowing return to neutral/mid-line position' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, '[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralForelimbs';

-- Raised Weight Shifting – Lateral Hindlimbs
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'raisedWeightShiftingLateralHindlimbs', 'Raised Weight Shifting – Lateral Hindlimbs', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.
Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist).
Lateral hindlimbs: Place your hands over their hips and gently rock them side-to-side.
This should be a slow and gentle movement, just enough to feel a contraction under your hands.
We don’t want your dog moving their feet so, if they start to do this, slow the action down.
Additional techniques:
Perturbations: rhythmic gentle pressure in multiple directions
Rebounding: adding a hold time before allowing return to neutral/mid-line position
[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='raisedWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface with your dog standing square.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Raise your dog’s forelimbs and/or hindlimbs onto a flat, stable platform/cushion/wobble cushion (as directed by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Lateral hindlimbs: Place your hands over their hips and gently rock them side-to-side.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'This should be a slow and gentle movement, just enough to feel a contraction under your hands.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'We don’t want your dog moving their feet so, if they start to do this, slow the action down.' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Additional techniques:' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Perturbations: rhythmic gentle pressure in multiple directions' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Rebounding: adding a hold time before allowing return to neutral/mid-line position' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, '[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]' FROM Exercise e WHERE e.ExerciseKey='raisedWeightShiftingLateralHindlimbs';

-- Resistance band stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'resistanceBandStands', 'Resistance band stands', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Secure a resistance band around the shoulder muscles (forelimb) and/or the thigh muscles (hindlimb).
Apply gentle, multi-directional force whilst encouraging them to remain stationary.
Do not pull too hard, we don’t want them to fall over, just resist the tension.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='resistanceBandStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='resistanceBandStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Secure a resistance band around the shoulder muscles (forelimb) and/or the thigh muscles (hindlimb).' FROM Exercise e WHERE e.ExerciseKey='resistanceBandStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Apply gentle, multi-directional force whilst encouraging them to remain stationary.' FROM Exercise e WHERE e.ExerciseKey='resistanceBandStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Do not pull too hard, we don’t want them to fall over, just resist the tension.' FROM Exercise e WHERE e.ExerciseKey='resistanceBandStands';

-- Rocks
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'rocks', 'Rocks', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Place your dog’s forelimbs and hindlimbs onto 2 blocks/cushions.
Use treats to guide your dog’s nose down and in front of the forelimb block then return to neutral head position (head raised with your dog looking at you).
After, guide your dog’s nose to behind the forelimb block, then back to neutral again and repeat.
Make sure your dog keeps their limbs in place on the blocks throughout the exercise.
This is similar to a push-up.', NULL, NULL, NULL, TRUE, 'Rocks - IMG_1535.jpg', 'Rocks - IMG_1535.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='rocks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='rocks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place your dog’s forelimbs and hindlimbs onto 2 blocks/cushions.' FROM Exercise e WHERE e.ExerciseKey='rocks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Use treats to guide your dog’s nose down and in front of the forelimb block then return to neutral head position (head raised with your dog looking at you).' FROM Exercise e WHERE e.ExerciseKey='rocks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'After, guide your dog’s nose to behind the forelimb block, then back to neutral again and repeat.' FROM Exercise e WHERE e.ExerciseKey='rocks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Make sure your dog keeps their limbs in place on the blocks throughout the exercise.' FROM Exercise e WHERE e.ExerciseKey='rocks';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'This is similar to a push-up.' FROM Exercise e WHERE e.ExerciseKey='rocks';

-- Roll over
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'rollOver', 'Roll over', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Ask your dog for a square lay then encourage them onto their side (either with a gentle push force or with a treat encouraging their head down to the floor).
Then continue to guide them over onto their back until they have rolled to their other side.
Do this slowly and with control.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='rollOver';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='rollOver';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask your dog for a square lay then encourage them onto their side (either with a gentle push force or with a treat encouraging their head down to the floor).' FROM Exercise e WHERE e.ExerciseKey='rollOver';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Then continue to guide them over onto their back until they have rolled to their other side.' FROM Exercise e WHERE e.ExerciseKey='rollOver';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Do this slowly and with control.' FROM Exercise e WHERE e.ExerciseKey='rollOver';

-- Seated baited neck stretches – Cranial
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'seatedBaitedNeckStretchesCranial', 'Seated baited neck stretches – Cranial', 'Perform this action in a sitting position on a flat, non-slip surface and with a treat in hand.', 'Perform this action in a sitting position on a flat, non-slip surface and with a treat in hand.
Cranial: Take your hand in front of your dog’s nose and guide their head forwards and hold the position.
Remember to always hold the position for at least 7-10s and return to neutral before asking for another stretch.
Do not take your dog’s head beyond point of comfort and make sure their feet stay stationary throughout.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_stretching'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='seatedBaitedNeckStretchesCranial';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this action in a sitting position on a flat, non-slip surface and with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='seatedBaitedNeckStretchesCranial';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Cranial: Take your hand in front of your dog’s nose and guide their head forwards and hold the position.' FROM Exercise e WHERE e.ExerciseKey='seatedBaitedNeckStretchesCranial';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Remember to always hold the position for at least 7-10s and return to neutral before asking for another stretch.' FROM Exercise e WHERE e.ExerciseKey='seatedBaitedNeckStretchesCranial';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Do not take your dog’s head beyond point of comfort and make sure their feet stay stationary throughout.' FROM Exercise e WHERE e.ExerciseKey='seatedBaitedNeckStretchesCranial';

-- Seated baited neck stretches – Lateral
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'seatedBaitedNeckStretchesLateral', 'Seated baited neck stretches – Lateral', 'Perform this action in a sitting position on a flat, non-slip surface and with a treat in hand.', 'Perform this action in a sitting position on a flat, non-slip surface and with a treat in hand.
Lateral: Take your hand from your dog’s nose to the side, stopping just before they start to rotate their head.
Hold this position for 7s then return to neutral head position (i.e., normal straight head position).
Repeat the action to the other side after a few seconds.
Remember to always hold the position for at least 7-10s and return to neutral before asking for another stretch.
Do not take your dog’s head beyond point of comfort and make sure their feet stay stationary throughout.', NULL, NULL, 7, TRUE, 'Flat lateral baited neck stretch - 5050789b95ea4254a2ad9a09fc351d10.jpg', 'Flat lateral baited neck stretch - 5050789b95ea4254a2ad9a09fc351d10.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_stretching'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='seatedBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this action in a sitting position on a flat, non-slip surface and with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='seatedBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Lateral: Take your hand from your dog’s nose to the side, stopping just before they start to rotate their head.' FROM Exercise e WHERE e.ExerciseKey='seatedBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Hold this position for 7s then return to neutral head position (i.e., normal straight head position).' FROM Exercise e WHERE e.ExerciseKey='seatedBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Repeat the action to the other side after a few seconds.' FROM Exercise e WHERE e.ExerciseKey='seatedBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Remember to always hold the position for at least 7-10s and return to neutral before asking for another stretch.' FROM Exercise e WHERE e.ExerciseKey='seatedBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Do not take your dog’s head beyond point of comfort and make sure their feet stay stationary throughout.' FROM Exercise e WHERE e.ExerciseKey='seatedBaitedNeckStretchesLateral';

-- Seated baited neck stretches – Ventral
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'seatedBaitedNeckStretchesVentral', 'Seated baited neck stretches – Ventral', 'Perform this action in a sitting position on a flat, non-slip surface and with a treat in hand.', 'Perform this action in a sitting position on a flat, non-slip surface and with a treat in hand.
Ventral: Encourage their head down (to the level specified by your physiotherapist) without your dog laying down to stretch the muscles on the top of the neck.
Remember to always hold the position for at least 7-10s and return to neutral before asking for another stretch.
Do not take your dog’s head beyond point of comfort and make sure their feet stay stationary throughout.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_stretching'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='seatedBaitedNeckStretchesVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this action in a sitting position on a flat, non-slip surface and with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='seatedBaitedNeckStretchesVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ventral: Encourage their head down (to the level specified by your physiotherapist) without your dog laying down to stretch the muscles on the top of the neck.' FROM Exercise e WHERE e.ExerciseKey='seatedBaitedNeckStretchesVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Remember to always hold the position for at least 7-10s and return to neutral before asking for another stretch.' FROM Exercise e WHERE e.ExerciseKey='seatedBaitedNeckStretchesVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Do not take your dog’s head beyond point of comfort and make sure their feet stay stationary throughout.' FROM Exercise e WHERE e.ExerciseKey='seatedBaitedNeckStretchesVentral';

-- Shoulder PROM
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'shoulderProm', 'Shoulder PROM', 'Try to stand your dog on a flat non-slip surface and give them plenty of support. Alternatively, ask them to lay flat on their side with their limbs in a neutral position (i.e., not tucked under themselves or rotated in any way). Keep the limb close to the body and in a straight line without any twisting. Take your dog''s joints in a forward and back motion making sure not to push them past their capabilities. Remember to do this gently as forcing a joint can cause damage. Remember, slow and gentle and watch for signs of discomfort (tensing muscles or pulling the leg away). If they do this, try to reduce the degree of motion you''re asking for (ie. move the joint less).', 'Shoulder: Place your hand along the front flat part of the shoulder and cup the other hand behind the elbow (this will be the origin of motion).
Lift the limb upwards from the elbow to flex the shoulder and then bring it downwards and forwards to extend (the limb should be pointing low and forwards).
When you swap to the other side, you will need to switch your hands over too.', NULL, NULL, NULL, TRUE, 'LR Shoulder PROM - 019c041c-3b0b-4871-a66b-15742e169522.jpg', 'LR Shoulder PROM - 019c041c-3b0b-4871-a66b-15742e169522.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='passive_range_of_motion'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='shoulderProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Shoulder: Place your hand along the front flat part of the shoulder and cup the other hand behind the elbow (this will be the origin of motion).' FROM Exercise e WHERE e.ExerciseKey='shoulderProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Lift the limb upwards from the elbow to flex the shoulder and then bring it downwards and forwards to extend (the limb should be pointing low and forwards).' FROM Exercise e WHERE e.ExerciseKey='shoulderProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'When you swap to the other side, you will need to switch your hands over too.' FROM Exercise e WHERE e.ExerciseKey='shoulderProm';

-- Side steps
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'sideSteps', 'Side steps', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Stand next to your dog with a treat in front of their nose or with them looking up at the treat in your hand.
Step towards them slowly to encourage them to step sideways away from you.
You can apply gentle pressure to encourage this movement.
Limbs should cross over.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='gait_and_walking'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='sideSteps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='sideSteps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Stand next to your dog with a treat in front of their nose or with them looking up at the treat in your hand.' FROM Exercise e WHERE e.ExerciseKey='sideSteps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Step towards them slowly to encourage them to step sideways away from you.' FROM Exercise e WHERE e.ExerciseKey='sideSteps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'You can apply gentle pressure to encourage this movement.' FROM Exercise e WHERE e.ExerciseKey='sideSteps';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Limbs should cross over.' FROM Exercise e WHERE e.ExerciseKey='sideSteps';

-- Sit to Lays to Sit to Stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'sitToLaysToSitToStands', 'Sit to Lays to Sit to Stands', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with your dog’s legs underneath themselves.', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with your dog’s legs underneath themselves.
Once the sit has been achieved, ask them into a square lay position.
Once they are in a square lay, ask them up into a sit, then ask them to stand (you can use a treat or a toy for encouragement but only if controlled).
Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.
Control the standing action by taking the treat from in front of their nose to the side of your leg.
This is similar to a push-up combined with a squat, with them using their forelimbs then hindlimbs to push themselves upwards.
If necessary, you can use wall support by backing them into a corner to allow for hip support either side of their hindlimbs or placing their weaker side next to a wall to prevent splaying/rotation of hindlimbs.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='sitToLaysToSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with your dog’s legs underneath themselves.' FROM Exercise e WHERE e.ExerciseKey='sitToLaysToSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Once the sit has been achieved, ask them into a square lay position.' FROM Exercise e WHERE e.ExerciseKey='sitToLaysToSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Once they are in a square lay, ask them up into a sit, then ask them to stand (you can use a treat or a toy for encouragement but only if controlled).' FROM Exercise e WHERE e.ExerciseKey='sitToLaysToSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.' FROM Exercise e WHERE e.ExerciseKey='sitToLaysToSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Control the standing action by taking the treat from in front of their nose to the side of your leg.' FROM Exercise e WHERE e.ExerciseKey='sitToLaysToSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'This is similar to a push-up combined with a squat, with them using their forelimbs then hindlimbs to push themselves upwards.' FROM Exercise e WHERE e.ExerciseKey='sitToLaysToSitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'If necessary, you can use wall support by backing them into a corner to allow for hip support either side of their hindlimbs or placing their weaker side next to a wall to prevent splaying/rotation of hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='sitToLaysToSitToStands';

-- Sit to Lays to Stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'sitToLaysToStands', 'Sit to Lays to Stands', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with your dog’s legs underneath themselves.', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with your dog’s legs underneath themselves.
Once the sit has been achieved, ask them into a square lay position.
Once they are in a square lay, then ask them to stand (you can use a treat or a toy for encouragement but only if controlled).
Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.
Control the standing action by taking the treat from in front of their nose to the side of your leg.
This is similar to a push-up, with them using their hindlimbs to push themselves upwards.
If necessary, you can use wall support by backing them into a corner to allow for hip support either side of their hindlimbs or placing their weaker side next to a wall to prevent splaying/rotation of hindlimbs.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='sitToLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with your dog’s legs underneath themselves.' FROM Exercise e WHERE e.ExerciseKey='sitToLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Once the sit has been achieved, ask them into a square lay position.' FROM Exercise e WHERE e.ExerciseKey='sitToLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Once they are in a square lay, then ask them to stand (you can use a treat or a toy for encouragement but only if controlled).' FROM Exercise e WHERE e.ExerciseKey='sitToLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.' FROM Exercise e WHERE e.ExerciseKey='sitToLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Control the standing action by taking the treat from in front of their nose to the side of your leg.' FROM Exercise e WHERE e.ExerciseKey='sitToLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'This is similar to a push-up, with them using their hindlimbs to push themselves upwards.' FROM Exercise e WHERE e.ExerciseKey='sitToLaysToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'If necessary, you can use wall support by backing them into a corner to allow for hip support either side of their hindlimbs or placing their weaker side next to a wall to prevent splaying/rotation of hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='sitToLaysToStands';

-- Sit to Stands
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'sitToStands', 'Sit to Stands', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with your dog’s legs underneath themselves.', 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with your dog’s legs underneath themselves.
Once the sit has been achieved, ask them to stand (you can use a treat or a toy for encouragement but only if controlled).
Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.
This is similar to a squat, with them using their hindlimbs to push themselves upwards.
If necessary, you can use wall support by backing them into a corner to prevent splaying/rotation of hindlimbs.', NULL, NULL, NULL, TRUE, 'Sit to stands - 69e563e5e9c5413cad2b22c00b306142.jpg', 'Sit to stands - 69e563e5e9c5413cad2b22c00b306142.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='sitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface and involves a controlled, square sit with your dog’s legs underneath themselves.' FROM Exercise e WHERE e.ExerciseKey='sitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Once the sit has been achieved, ask them to stand (you can use a treat or a toy for encouragement but only if controlled).' FROM Exercise e WHERE e.ExerciseKey='sitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Try to stand in front of them as this prevent them from using their forelimbs to drag themselves forwards.' FROM Exercise e WHERE e.ExerciseKey='sitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'This is similar to a squat, with them using their hindlimbs to push themselves upwards.' FROM Exercise e WHERE e.ExerciseKey='sitToStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'If necessary, you can use wall support by backing them into a corner to prevent splaying/rotation of hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='sitToStands';

-- Skin rolling massage
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'skinRollingMassage', 'Skin rolling massage', 'Make sure your dog is calm and relaxed on a comfortable surface in a relatively flat position.', 'Make sure your dog is calm and relaxed on a comfortable surface in a relatively flat position.
Apply your palms to the area following one hand with the other and gradually increasing pressure as the tissue warms up.
Travel in the direction of the hair line.
Do not rush the movement.
After about 5-10 mins of this, following the hair line, then take your dog’s skin between your thumbs and fingers and roll the skin upwards towards your dog’s head.
If you come across a section of skin that is stuck, do NOT push through it.
Just drop the skin and move to the next section above the stuck section and continue.
Repeat this action a couple of times then return to the original action of following one hand with another down your dog’s back for a few minutes to re-relax the tissue.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='massage'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='skinRollingMassage';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Make sure your dog is calm and relaxed on a comfortable surface in a relatively flat position.' FROM Exercise e WHERE e.ExerciseKey='skinRollingMassage';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Apply your palms to the area following one hand with the other and gradually increasing pressure as the tissue warms up.' FROM Exercise e WHERE e.ExerciseKey='skinRollingMassage';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Travel in the direction of the hair line.' FROM Exercise e WHERE e.ExerciseKey='skinRollingMassage';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Do not rush the movement.' FROM Exercise e WHERE e.ExerciseKey='skinRollingMassage';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'After about 5-10 mins of this, following the hair line, then take your dog’s skin between your thumbs and fingers and roll the skin upwards towards your dog’s head.' FROM Exercise e WHERE e.ExerciseKey='skinRollingMassage';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'If you come across a section of skin that is stuck, do NOT push through it.' FROM Exercise e WHERE e.ExerciseKey='skinRollingMassage';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Just drop the skin and move to the next section above the stuck section and continue.' FROM Exercise e WHERE e.ExerciseKey='skinRollingMassage';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Repeat this action a couple of times then return to the original action of following one hand with another down your dog’s back for a few minutes to re-relax the tissue.' FROM Exercise e WHERE e.ExerciseKey='skinRollingMassage';

-- Slow Lead Walking
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'slowLeadWalking', 'Slow Lead Walking', 'Walk your dog on a short lead with a raised head.', 'Walk your dog on a short lead with a raised head.
This should be slow enough to allow them to weight bear equally on each limb so do not rush them beyond their capable speed.
The goal is to keep them at heel but if your dog struggles with pulling, you may want to consider a harness or halti (but only if this does not impact movement negatively).', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='gait_and_walking'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='slowLeadWalking';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Walk your dog on a short lead with a raised head.' FROM Exercise e WHERE e.ExerciseKey='slowLeadWalking';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'This should be slow enough to allow them to weight bear equally on each limb so do not rush them beyond their capable speed.' FROM Exercise e WHERE e.ExerciseKey='slowLeadWalking';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'The goal is to keep them at heel but if your dog struggles with pulling, you may want to consider a harness or halti (but only if this does not impact movement negatively).' FROM Exercise e WHERE e.ExerciseKey='slowLeadWalking';

-- Slow Lead Walking (neuro)
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'slowLeadWalkingNeuro', 'Slow Lead Walking (neuro)', 'Walk your dog with a raised head by placing a treat in front of their nose.', 'Walk your dog with a raised head by placing a treat in front of their nose.
Make sure to use a sling to support under their stomach.
This should be slow enough to allow them to weight bear equally on each limb so do not rush them beyond their capable speed.
Try to guide your dog’s hindlimbs by placing them manually in the correct position as they walk – do NOT allow them to drag their legs (you may want to consider boots/socks to protect hindlimb digits).', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='gait_and_walking'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='slowLeadWalkingNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Walk your dog with a raised head by placing a treat in front of their nose.' FROM Exercise e WHERE e.ExerciseKey='slowLeadWalkingNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Make sure to use a sling to support under their stomach.' FROM Exercise e WHERE e.ExerciseKey='slowLeadWalkingNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'This should be slow enough to allow them to weight bear equally on each limb so do not rush them beyond their capable speed.' FROM Exercise e WHERE e.ExerciseKey='slowLeadWalkingNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Try to guide your dog’s hindlimbs by placing them manually in the correct position as they walk – do NOT allow them to drag their legs (you may want to consider boots/socks to protect hindlimb digits).' FROM Exercise e WHERE e.ExerciseKey='slowLeadWalkingNeuro';

-- Stair ascent / descent
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'stairAscentDescent', 'Stair ascent / descent', 'This exercise should be performed on flat, non-slip steps that are not too steep and have traction (i.e., not wood).', 'This exercise should be performed on flat, non-slip steps that are not too steep and have traction (i.e., not wood).
Ask your dog to walk slowly up/down the stairs (guided by physiotherapist).
As your dog becomes more proficient, you can increase the speed of the ascent/descent.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='gait_and_walking'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='stairAscentDescent';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on flat, non-slip steps that are not too steep and have traction (i.e., not wood).' FROM Exercise e WHERE e.ExerciseKey='stairAscentDescent';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask your dog to walk slowly up/down the stairs (guided by physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='stairAscentDescent';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'As your dog becomes more proficient, you can increase the speed of the ascent/descent.' FROM Exercise e WHERE e.ExerciseKey='stairAscentDescent';

-- Static Weight Shifting with Assisted Standing – Craniocaudal
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'staticWeightShiftingWithAssistedStandingCraniocaudal', 'Static Weight Shifting with Assisted Standing – Craniocaudal', 'Perform this exercise on a flat, non-slip surface.', 'Perform this exercise on a flat, non-slip surface.
Place a comfortable but supportive cushion underneath your dog’s chest and allow them to stand in a square position with all feet flat to the floor.
Remember, we want them to take most of their weight and support themselves.
Whilst your dog is supported:
Craniocaudal: Cup your hands over the front of your dog’s shoulders (you should feel bony prominences to hold) whilst you stand behind them and gently pull them towards you in a fore-back motion.
This should be a slow and gentle movement, just enough to feel a contraction under your hands.
We don’t want your dog moving their feet so, if they start to do this, slow the action down.
Only stand your dog the recommended amount of time, even if you have not completed the weight shifting reps; you can always stand them up again after they have rested to finish the reps.
If necessary, just focus on the standing element rather than the weight shifting (only complete if capable).
Additional techniques:
Perturbations: rhythmic gentle pressure in multiple directions
Rebounding: adding a hold time before allowing return to neutral/mid-line position
[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this exercise on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a comfortable but supportive cushion underneath your dog’s chest and allow them to stand in a square position with all feet flat to the floor.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Remember, we want them to take most of their weight and support themselves.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Whilst your dog is supported:' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Craniocaudal: Cup your hands over the front of your dog’s shoulders (you should feel bony prominences to hold) whilst you stand behind them and gently pull them towards you in a fore-back motion.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'This should be a slow and gentle movement, just enough to feel a contraction under your hands.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'We don’t want your dog moving their feet so, if they start to do this, slow the action down.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Only stand your dog the recommended amount of time, even if you have not completed the weight shifting reps; you can always stand them up again after they have rested to finish the reps.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, 'If necessary, just focus on the standing element rather than the weight shifting (only complete if capable).' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 10, 'Additional techniques:' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 11, 'Perturbations: rhythmic gentle pressure in multiple directions' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 12, 'Rebounding: adding a hold time before allowing return to neutral/mid-line position' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 13, '[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingCraniocaudal';

-- Static Weight Shifting with Assisted Standing – Lateral Forelimbs
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'staticWeightShiftingWithAssistedStandingLateralForelimbs', 'Static Weight Shifting with Assisted Standing – Lateral Forelimbs', 'Perform this exercise on a flat, non-slip surface.', 'Perform this exercise on a flat, non-slip surface.
Place a comfortable but supportive cushion underneath your dog’s chest and allow them to stand in a square position with all feet flat to the floor.
Remember, we want them to take most of their weight and support themselves.
Whilst your dog is supported:
Lateral forelimbs: Place your hands on your dog’s shoulders and gently rock them from side-to-side.
This should be a slow and gentle movement, just enough to feel a contraction under your hands.
We don’t want your dog moving their feet so, if they start to do this, slow the action down.
Only stand your dog the recommended amount of time, even if you have not completed the weight shifting reps; you can always stand them up again after they have rested to finish the reps.
If necessary, just focus on the standing element rather than the weight shifting (only complete if capable).
Additional techniques:
Perturbations: rhythmic gentle pressure in multiple directions
Rebounding: adding a hold time before allowing return to neutral/mid-line position
[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this exercise on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a comfortable but supportive cushion underneath your dog’s chest and allow them to stand in a square position with all feet flat to the floor.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Remember, we want them to take most of their weight and support themselves.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Whilst your dog is supported:' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Lateral forelimbs: Place your hands on your dog’s shoulders and gently rock them from side-to-side.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'This should be a slow and gentle movement, just enough to feel a contraction under your hands.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'We don’t want your dog moving their feet so, if they start to do this, slow the action down.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Only stand your dog the recommended amount of time, even if you have not completed the weight shifting reps; you can always stand them up again after they have rested to finish the reps.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, 'If necessary, just focus on the standing element rather than the weight shifting (only complete if capable).' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 10, 'Additional techniques:' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 11, 'Perturbations: rhythmic gentle pressure in multiple directions' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 12, 'Rebounding: adding a hold time before allowing return to neutral/mid-line position' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 13, '[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralForelimbs';

-- Static Weight Shifting with Assisted Standing – Lateral Hindlimbs
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'staticWeightShiftingWithAssistedStandingLateralHindlimbs', 'Static Weight Shifting with Assisted Standing – Lateral Hindlimbs', 'Perform this exercise on a flat, non-slip surface.', 'Perform this exercise on a flat, non-slip surface.
Place a comfortable but supportive cushion underneath your dog’s chest and allow them to stand in a square position with all feet flat to the floor.
Remember, we want them to take most of their weight and support themselves.
Whilst your dog is supported:
Lateral hindlimbs: Place your hands over their hips and gently rock them side-to-side.
This should be a slow and gentle movement, just enough to feel a contraction under your hands.
We don’t want your dog moving their feet so, if they start to do this, slow the action down.
Only stand your dog the recommended amount of time, even if you have not completed the weight shifting reps; you can always stand them up again after they have rested to finish the reps.
If necessary, just focus on the standing element rather than the weight shifting (only complete if capable).
Additional techniques:
Perturbations: rhythmic gentle pressure in multiple directions
Rebounding: adding a hold time before allowing return to neutral/mid-line position
[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this exercise on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a comfortable but supportive cushion underneath your dog’s chest and allow them to stand in a square position with all feet flat to the floor.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Remember, we want them to take most of their weight and support themselves.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Whilst your dog is supported:' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Lateral hindlimbs: Place your hands over their hips and gently rock them side-to-side.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'This should be a slow and gentle movement, just enough to feel a contraction under your hands.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'We don’t want your dog moving their feet so, if they start to do this, slow the action down.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Only stand your dog the recommended amount of time, even if you have not completed the weight shifting reps; you can always stand them up again after they have rested to finish the reps.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 9, 'If necessary, just focus on the standing element rather than the weight shifting (only complete if capable).' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 10, 'Additional techniques:' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 11, 'Perturbations: rhythmic gentle pressure in multiple directions' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 12, 'Rebounding: adding a hold time before allowing return to neutral/mid-line position' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 13, '[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingWithAssistedStandingLateralHindlimbs';

-- Static Weight Shifting – Craniocaudal
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'staticWeightShiftingCraniocaudal', 'Static Weight Shifting – Craniocaudal', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.
Craniocaudal: Cup your hands over the front of your dog’s shoulders (you should feel bony prominences to hold) whilst you stand behind them and gently pull them towards you in a fore-back motion.
This should be a slow and gentle movement, just enough to feel a contraction under your hands.
We don’t want your dog moving their feet so, if they start to do this, slow the action down.
Additional techniques:
Perturbations: rhythmic gentle pressure in multiple directions
Rebounding: adding a hold time before allowing return to neutral/mid-line position
[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]', NULL, NULL, NULL, TRUE, 'Craniocaudal SWS - IMG_1319.jpg', 'Craniocaudal SWS - IMG_1319.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='staticWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface with your dog standing square.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Craniocaudal: Cup your hands over the front of your dog’s shoulders (you should feel bony prominences to hold) whilst you stand behind them and gently pull them towards you in a fore-back motion.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'This should be a slow and gentle movement, just enough to feel a contraction under your hands.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'We don’t want your dog moving their feet so, if they start to do this, slow the action down.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Additional techniques:' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Perturbations: rhythmic gentle pressure in multiple directions' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Rebounding: adding a hold time before allowing return to neutral/mid-line position' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, '[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingCraniocaudal';

-- Static Weight Shifting – Lateral Forelimbs
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'staticWeightShiftingLateralForelimbs', 'Static Weight Shifting – Lateral Forelimbs', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.
Lateral forelimbs: Place your hands on your dog’s shoulders and gently rock them from side-to-side.
This should be a slow and gentle movement, just enough to feel a contraction under your hands.
We don’t want your dog moving their feet so, if they start to do this, slow the action down.
Additional techniques:
Perturbations: rhythmic gentle pressure in multiple directions
Rebounding: adding a hold time before allowing return to neutral/mid-line position
[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]', NULL, NULL, NULL, TRUE, 'Lateral FL SWS - IMG_1317.jpg', 'Lateral FL SWS - IMG_1317.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='staticWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface with your dog standing square.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Lateral forelimbs: Place your hands on your dog’s shoulders and gently rock them from side-to-side.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'This should be a slow and gentle movement, just enough to feel a contraction under your hands.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'We don’t want your dog moving their feet so, if they start to do this, slow the action down.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Additional techniques:' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Perturbations: rhythmic gentle pressure in multiple directions' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Rebounding: adding a hold time before allowing return to neutral/mid-line position' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralForelimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, '[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralForelimbs';

-- Static Weight Shifting – Lateral Hindlimbs
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'staticWeightShiftingLateralHindlimbs', 'Static Weight Shifting – Lateral Hindlimbs', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.', 'This exercise should be performed on a flat, non-slip surface with your dog standing square.
Lateral hindlimbs: Place your hands over their hips and rock them side-to-side.
This should be a slow and gentle movement, just enough to feel a contraction under your hands.
We don’t want your dog moving their feet so, if they start to do this, slow the action down.
Additional techniques:
Perturbations: rhythmic gentle pressure in multiple directions
Rebounding: adding a hold time before allowing return to neutral/mid-line position
[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]', NULL, NULL, NULL, TRUE, 'Lateral HL SWS - v24044gl0000d2rghj7og65pjbcfttk0.jpg', 'Lateral HL SWS - v24044gl0000d2rghj7og65pjbcfttk0.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='balance_and_weight_shifting'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='staticWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface with your dog standing square.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Lateral hindlimbs: Place your hands over their hips and rock them side-to-side.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'This should be a slow and gentle movement, just enough to feel a contraction under your hands.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'We don’t want your dog moving their feet so, if they start to do this, slow the action down.' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Additional techniques:' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Perturbations: rhythmic gentle pressure in multiple directions' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Rebounding: adding a hold time before allowing return to neutral/mid-line position' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralHindlimbs';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, '[Remember you can try this exercise with a lick-mat stuck to the wall in front of your dog to keep them occupied and you can provide additional support by completing this exercise next to a wall with you stood on the other side.]' FROM Exercise e WHERE e.ExerciseKey='staticWeightShiftingLateralHindlimbs';

-- Step on-off – Craniocaudal
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'stepOnOffCraniocaudal', 'Step on-off – Craniocaudal', 'Perform this exercise on a flat stable surface with your dog standing square.', 'Perform this exercise on a flat stable surface with your dog standing square.
Place a non-slip block or cushion on the floor in front of your dog.
Ask your dog to step up onto the block or cushion with the forelimbs only.
Do the same exercise in a forward-backward direction.', NULL, NULL, NULL, TRUE, 'Craniocaudal step up and off - IMG_1426.jpg', 'Craniocaudal step up and off - IMG_1426.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='step_and_pivot_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='stepOnOffCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this exercise on a flat stable surface with your dog standing square.' FROM Exercise e WHERE e.ExerciseKey='stepOnOffCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a non-slip block or cushion on the floor in front of your dog.' FROM Exercise e WHERE e.ExerciseKey='stepOnOffCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Ask your dog to step up onto the block or cushion with the forelimbs only.' FROM Exercise e WHERE e.ExerciseKey='stepOnOffCraniocaudal';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Do the same exercise in a forward-backward direction.' FROM Exercise e WHERE e.ExerciseKey='stepOnOffCraniocaudal';

-- Step on-off – Hindlimb Lateral
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'stepOnOffHindlimbLateral', 'Step on-off – Hindlimb Lateral', 'Perform this exercise on a flat stable surface with your dog standing square.', 'Perform this exercise on a flat stable surface with your dog standing square.
Place a non-slip block/cushion flat on the floor (as directed by your physiotherapist) flat on the floor level with your dog’s hindlimbs.
Ask your dog to step sideways up onto the block/cushion, with hindlimbs only, from one side and dismount onto the floor on other side of the block.
Repeat this action going in the other direction.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='step_and_pivot_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='stepOnOffHindlimbLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this exercise on a flat stable surface with your dog standing square.' FROM Exercise e WHERE e.ExerciseKey='stepOnOffHindlimbLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a non-slip block/cushion flat on the floor (as directed by your physiotherapist) flat on the floor level with your dog’s hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='stepOnOffHindlimbLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Ask your dog to step sideways up onto the block/cushion, with hindlimbs only, from one side and dismount onto the floor on other side of the block.' FROM Exercise e WHERE e.ExerciseKey='stepOnOffHindlimbLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Repeat this action going in the other direction.' FROM Exercise e WHERE e.ExerciseKey='stepOnOffHindlimbLateral';

-- Step on-off – Lateral
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'stepOnOffLateral', 'Step on-off – Lateral', 'Perform this exercise on a flat stable surface with your dog standing square.', 'Perform this exercise on a flat stable surface with your dog standing square.
Place a non-slip block or cushion on the floor in front of your dog.
Ask your dog to step up onto the block or cushion with the forelimbs only from one side and dismount onto the floor on the other side of the block.
Repeat this action going in the other direction.', NULL, NULL, NULL, TRUE, 'Lateral step up and off - IMG_1431.jpg', 'Lateral step up and off - IMG_1431.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='step_and_pivot_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='stepOnOffLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this exercise on a flat stable surface with your dog standing square.' FROM Exercise e WHERE e.ExerciseKey='stepOnOffLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place a non-slip block or cushion on the floor in front of your dog.' FROM Exercise e WHERE e.ExerciseKey='stepOnOffLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Ask your dog to step up onto the block or cushion with the forelimbs only from one side and dismount onto the floor on the other side of the block.' FROM Exercise e WHERE e.ExerciseKey='stepOnOffLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Repeat this action going in the other direction.' FROM Exercise e WHERE e.ExerciseKey='stepOnOffLateral';

-- Stifle PROM
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'stifleProm', 'Stifle PROM', 'Try to stand your dog on a flat non-slip surface and give them plenty of support. Alternatively, ask them to lay flat on their side with their limbs in a neutral position (i.e., not tucked under themselves or rotated in any way). Keep the limb close to the body and in a straight line without any twisting. Take your dog''s joints in a forward and back motion making sure not to push them past their capabilities. Remember to do this gently as forcing a joint can cause damage. Remember, slow and gentle and watch for signs of discomfort (tensing muscles or pulling the leg away). If they do this, try to reduce the degree of motion you''re asking for (ie. move the joint less).', 'Stifle: Cup your hand around the stifle joint then use your other hand to cup the back of their hock (ankle) (this will be the origin of motion).
Use the hand around the hock to lift your dog''s leg towards their bum into flexion, then take it into extension by gently pushing it down and forward, aiming to point the limb behind the forelimb on that side.
When you swap to the other side, you will need to switch your hands over too.', NULL, NULL, NULL, TRUE, 'LR stifle PROM - v24044gl0000d39u24nog65indk4ah7g.jpg', 'LR stifle PROM - v24044gl0000d39u24nog65indk4ah7g.mp4'
  FROM ExerciseCategory ec WHERE ec.CategoryKey='passive_range_of_motion'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='stifleProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Stifle: Cup your hand around the stifle joint then use your other hand to cup the back of their hock (ankle) (this will be the origin of motion).' FROM Exercise e WHERE e.ExerciseKey='stifleProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Use the hand around the hock to lift your dog''s leg towards their bum into flexion, then take it into extension by gently pushing it down and forward, aiming to point the limb behind the forelimb on that side.' FROM Exercise e WHERE e.ExerciseKey='stifleProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'When you swap to the other side, you will need to switch your hands over too.' FROM Exercise e WHERE e.ExerciseKey='stifleProm';

-- Stood baited neck stretches – Cranial
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'stoodBaitedNeckStretchesCranial', 'Stood baited neck stretches – Cranial', 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.', 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.
Cranial: Take your hand in front of your dog’s nose and guide their head forwards and hold the position.
You may need to provide support with a hand either at the sternum or in front of their hindlimbs.
Remember to always return to neutral before asking for another stretch.
Do not take your dog’s head beyond point of comfort.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_stretching'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='stoodBaitedNeckStretchesCranial';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesCranial';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Cranial: Take your hand in front of your dog’s nose and guide their head forwards and hold the position.' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesCranial';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'You may need to provide support with a hand either at the sternum or in front of their hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesCranial';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Remember to always return to neutral before asking for another stretch.' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesCranial';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Do not take your dog’s head beyond point of comfort.' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesCranial';

-- Stood baited neck stretches – Lateral
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'stoodBaitedNeckStretchesLateral', 'Stood baited neck stretches – Lateral', 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.', 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.
Lateral: Take your hand from your dog’s nose to the side, stopping just before they start to rotate their head (to the level specified by your physiotherapist).
Hold this position for 7s then return to neutral head position (i.e., normal straight head position).
Repeat the action to the other side after a few seconds.
You can stand next to them at their shoulder and encourage their head around your leg.
Remember to always return to neutral before asking for another stretch.
Do not take your dog’s head beyond point of comfort.', NULL, NULL, 7, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_stretching'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='stoodBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Lateral: Take your hand from your dog’s nose to the side, stopping just before they start to rotate their head (to the level specified by your physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Hold this position for 7s then return to neutral head position (i.e., normal straight head position).' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Repeat the action to the other side after a few seconds.' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'You can stand next to them at their shoulder and encourage their head around your leg.' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Remember to always return to neutral before asking for another stretch.' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesLateral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Do not take your dog’s head beyond point of comfort.' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesLateral';

-- Stood baited neck stretches – Ventral
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'stoodBaitedNeckStretchesVentral', 'Stood baited neck stretches – Ventral', 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.', 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.
Ventral: Encourage their head down (to the level specified by your physiotherapist) without your dog laying down to stretch the muscles on the top of the neck by holding a treat between the forelimbs.
Remember to always return to neutral before asking for another stretch.
Do not take your dog’s head beyond point of comfort.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neck_stretching'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='stoodBaitedNeckStretchesVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Perform this action in a standing position on a flat, non-slip surface and with a treat in hand.' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ventral: Encourage their head down (to the level specified by your physiotherapist) without your dog laying down to stretch the muscles on the top of the neck by holding a treat between the forelimbs.' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Remember to always return to neutral before asking for another stretch.' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesVentral';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Do not take your dog’s head beyond point of comfort.' FROM Exercise e WHERE e.ExerciseKey='stoodBaitedNeckStretchesVentral';

-- Supermans
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'supermans', 'Supermans', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Ask your dog to stand square then gently lift one forelimb and the opposite hindlimb by support the forelimb at above the wrist/carpus and the hindlimb below the ankle/hock.
Allow your dog time to balance themselves for a few seconds, then start to stretch the hindlimb out behind them and the forelimb forwards into a superman pose.
Do not force your dog beyond their capabilities and hold the position for at least 7-10s.
Once completed, allow the limbs to return to neutral position and gently place them on the floor, then switch to the other diagonal pair.
Make sure to support your dog at all times, you may need someone to help you.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='limb_lifts_and_proprioception'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='supermans';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='supermans';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask your dog to stand square then gently lift one forelimb and the opposite hindlimb by support the forelimb at above the wrist/carpus and the hindlimb below the ankle/hock.' FROM Exercise e WHERE e.ExerciseKey='supermans';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Allow your dog time to balance themselves for a few seconds, then start to stretch the hindlimb out behind them and the forelimb forwards into a superman pose.' FROM Exercise e WHERE e.ExerciseKey='supermans';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Do not force your dog beyond their capabilities and hold the position for at least 7-10s.' FROM Exercise e WHERE e.ExerciseKey='supermans';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Once completed, allow the limbs to return to neutral position and gently place them on the floor, then switch to the other diagonal pair.' FROM Exercise e WHERE e.ExerciseKey='supermans';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Make sure to support your dog at all times, you may need someone to help you.' FROM Exercise e WHERE e.ExerciseKey='supermans';

-- Surface variation
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'surfaceVariation', 'Surface variation', 'Place different textured fabrics/materials on the ground and ask your dog to stand on/walk over them slowly.', 'Place different textured fabrics/materials on the ground and ask your dog to stand on/walk over them slowly.
Make sure to do this on a non-slip floor and do not use anything sharp.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neurological_rehabilitation'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='surfaceVariation';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Place different textured fabrics/materials on the ground and ask your dog to stand on/walk over them slowly.' FROM Exercise e WHERE e.ExerciseKey='surfaceVariation';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Make sure to do this on a non-slip floor and do not use anything sharp.' FROM Exercise e WHERE e.ExerciseKey='surfaceVariation';

-- Tactile Stimulation
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'tactileStimulation', 'Tactile Stimulation', 'Tickle the hair between and around the outside of your dog’s paw pads for 5-10s, then switch to the other foot and do the same.', 'Tickle the hair between and around the outside of your dog’s paw pads for 5-10s, then switch to the other foot and do the same.
Your dog will adjust to the sensation so we want to keep them alert and responsive by switching between feet for optimal effect.
This is to help with awareness of limb placement by increasing the stimuli your dog is exposed to, firing nerves and contracting muscles in response.
Make sure your dog is laid flat on their side with limbs in a neutral position (i.e., not underneath them or too far out behind them).
If they suddenly contract the leg to avoid the sensation, we do not want them torquing a joint and causing discomfort.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neurological_rehabilitation'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='tactileStimulation';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Tickle the hair between and around the outside of your dog’s paw pads for 5-10s, then switch to the other foot and do the same.' FROM Exercise e WHERE e.ExerciseKey='tactileStimulation';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Your dog will adjust to the sensation so we want to keep them alert and responsive by switching between feet for optimal effect.' FROM Exercise e WHERE e.ExerciseKey='tactileStimulation';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'This is to help with awareness of limb placement by increasing the stimuli your dog is exposed to, firing nerves and contracting muscles in response.' FROM Exercise e WHERE e.ExerciseKey='tactileStimulation';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Make sure your dog is laid flat on their side with limbs in a neutral position (i.e., not underneath them or too far out behind them).' FROM Exercise e WHERE e.ExerciseKey='tactileStimulation';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'If they suddenly contract the leg to avoid the sensation, we do not want them torquing a joint and causing discomfort.' FROM Exercise e WHERE e.ExerciseKey='tactileStimulation';

-- Tactile Stimulation (neuro)
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'tactileStimulationNeuro', 'Tactile Stimulation (neuro)', 'Tickle the hair between and around the outside of your dog’s paw pads for 5-10s, then switch to the other foot and do the same.', 'Tickle the hair between and around the outside of your dog’s paw pads for 5-10s, then switch to the other foot and do the same.
Your dog will adjust to the sensation so we want to keep them alert and responsive by switching between feet for optimal effect.
This is to help with awareness of limb placement by increasing the stimuli your dog is exposed to, firing nerves and contracting muscles in response.
Make sure your dog is laid flat on their side with limbs in a neutral position (i.e., not underneath them or too far out behind them).
If they suddenly contract the leg to avoid the sensation, we do not want them torquing a joint and causing discomfort.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='neurological_rehabilitation'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='tactileStimulationNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Tickle the hair between and around the outside of your dog’s paw pads for 5-10s, then switch to the other foot and do the same.' FROM Exercise e WHERE e.ExerciseKey='tactileStimulationNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Your dog will adjust to the sensation so we want to keep them alert and responsive by switching between feet for optimal effect.' FROM Exercise e WHERE e.ExerciseKey='tactileStimulationNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'This is to help with awareness of limb placement by increasing the stimuli your dog is exposed to, firing nerves and contracting muscles in response.' FROM Exercise e WHERE e.ExerciseKey='tactileStimulationNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Make sure your dog is laid flat on their side with limbs in a neutral position (i.e., not underneath them or too far out behind them).' FROM Exercise e WHERE e.ExerciseKey='tactileStimulationNeuro';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'If they suddenly contract the leg to avoid the sensation, we do not want them torquing a joint and causing discomfort.' FROM Exercise e WHERE e.ExerciseKey='tactileStimulationNeuro';

-- Tarsus PROM
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'tarsusProm', 'Tarsus PROM', 'Try to stand your dog on a flat non-slip surface and give them plenty of support. Alternatively, ask them to lay flat on their side with their limbs in a neutral position (i.e., not tucked under themselves or rotated in any way). Keep the limb close to the body and in a straight line without any twisting. Take your dog''s joints in a forward and back motion making sure not to push them past their capabilities. Remember to do this gently as forcing a joint can cause damage. Remember, slow and gentle and watch for signs of discomfort (tensing muscles or pulling the leg away). If they do this, try to reduce the degree of motion you''re asking for (ie. move the joint less).', 'Tarsus: Cup your hand above the ankle/hock joint to stabilise the limb then place your other hand below and behind the joint (this will be the origin of motion).
Use the hand below the hock to move it gently in a forwards and backwards direction for flexion and extension.
When you swap to the other side, you will need to switch your hands over too.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='passive_range_of_motion'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='tarsusProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Tarsus: Cup your hand above the ankle/hock joint to stabilise the limb then place your other hand below and behind the joint (this will be the origin of motion).' FROM Exercise e WHERE e.ExerciseKey='tarsusProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Use the hand below the hock to move it gently in a forwards and backwards direction for flexion and extension.' FROM Exercise e WHERE e.ExerciseKey='tarsusProm';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'When you swap to the other side, you will need to switch your hands over too.' FROM Exercise e WHERE e.ExerciseKey='tarsusProm';

-- Trot
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'trot', 'Trot', '4 beat walking gait).', '4 beat walking gait).
Then, after a few minutes of warm up, move up to a trot (where limbs land in diagonal pairs).', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='gait_and_walking'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='trot';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, '4 beat walking gait).' FROM Exercise e WHERE e.ExerciseKey='trot';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Then, after a few minutes of warm up, move up to a trot (where limbs land in diagonal pairs).' FROM Exercise e WHERE e.ExerciseKey='trot';

-- Tug-of-War
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'tugOfWar', 'Tug-of-War', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Use a rope toy and encourage your dog to bite onto one end.
Add gentle resistance but no sudden pulls.
Gradually increase tension as your dog becomes more proficient.
Start in a straight line and deviate to different directions (as guided by physiotherapist).', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='core_and_posture'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='tugOfWar';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='tugOfWar';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Use a rope toy and encourage your dog to bite onto one end.' FROM Exercise e WHERE e.ExerciseKey='tugOfWar';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Add gentle resistance but no sudden pulls.' FROM Exercise e WHERE e.ExerciseKey='tugOfWar';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Gradually increase tension as your dog becomes more proficient.' FROM Exercise e WHERE e.ExerciseKey='tugOfWar';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Start in a straight line and deviate to different directions (as guided by physiotherapist).' FROM Exercise e WHERE e.ExerciseKey='tugOfWar';

-- Tummy Tickles / Ventral stimulation
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'tummyTicklesVentralStimulation', 'Tummy Tickles / Ventral stimulation', 'Tickle underneath your dog’s stomach starting from the top of their chest to the end of their stomach.', 'Tickle underneath your dog’s stomach starting from the top of their chest to the end of their stomach.
You are looking for their back to raise slightly.
It will be a subtle motion so make sure to monitor the motion closely.
This will engage their core and help straighten their posture.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='core_and_posture'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='tummyTicklesVentralStimulation';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Tickle underneath your dog’s stomach starting from the top of their chest to the end of their stomach.' FROM Exercise e WHERE e.ExerciseKey='tummyTicklesVentralStimulation';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'You are looking for their back to raise slightly.' FROM Exercise e WHERE e.ExerciseKey='tummyTicklesVentralStimulation';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'It will be a subtle motion so make sure to monitor the motion closely.' FROM Exercise e WHERE e.ExerciseKey='tummyTicklesVentralStimulation';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'This will engage their core and help straighten their posture.' FROM Exercise e WHERE e.ExerciseKey='tummyTicklesVentralStimulation';

-- Weaving
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'weaving', 'Weaving', 'This exercise should be performed on a flat, non-slip surface at an active walk.', 'This exercise should be performed on a flat, non-slip surface at an active walk.
Place three-four markers/cones on the floor (distance between determined by physiotherapist) and walk your dog around them in a weaving configuration.
This should be performed on a flat, non-slip surface and at an active walk without rushing your dog.
Weaving must be slow and controlled with wide turns encouraged to allow bending.
Start with a shallow turn, close to the markers and progress to a deeper turn to encourage spinal bending slowly.
Progression (never progress unless told to by a physiotherapist):
Reduce distance between markers/cones to encourage tighter turns', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='pole_and_obstacle_work'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='weaving';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface at an active walk.' FROM Exercise e WHERE e.ExerciseKey='weaving';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place three-four markers/cones on the floor (distance between determined by physiotherapist) and walk your dog around them in a weaving configuration.' FROM Exercise e WHERE e.ExerciseKey='weaving';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'This should be performed on a flat, non-slip surface and at an active walk without rushing your dog.' FROM Exercise e WHERE e.ExerciseKey='weaving';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Weaving must be slow and controlled with wide turns encouraged to allow bending.' FROM Exercise e WHERE e.ExerciseKey='weaving';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Start with a shallow turn, close to the markers and progress to a deeper turn to encourage spinal bending slowly.' FROM Exercise e WHERE e.ExerciseKey='weaving';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Progression (never progress unless told to by a physiotherapist):' FROM Exercise e WHERE e.ExerciseKey='weaving';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Reduce distance between markers/cones to encourage tighter turns' FROM Exercise e WHERE e.ExerciseKey='weaving';

-- Wheelbarrow
INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ec.ExerciseCategoryId, 'wheelbarrow', 'Wheelbarrow', 'This exercise should be performed on a flat, non-slip surface.', 'This exercise should be performed on a flat, non-slip surface.
Lift your dog under their stomach, keeping their forelimbs on the floor.
Once balanced, ask them to take a few steps forwards with their hindlimbs elevated.
Only reach an angle equivalent to the height of their wrist/carpus.
It is imperative that the spine remain neutral and that they can engage through their core.
If you notice a dipping or rotation through their spine, gently place their hindlimbs back on the floor and provide more support underneath them.', NULL, NULL, NULL, TRUE, NULL, NULL
  FROM ExerciseCategory ec WHERE ec.CategoryKey='strength_and_transitions'
  ON DUPLICATE KEY UPDATE
    ExerciseCategoryId=VALUES(ExerciseCategoryId), Title=VALUES(Title), ObjectiveSummary=VALUES(ObjectiveSummary),
    InstructionsText=VALUES(InstructionsText), DefaultReps=VALUES(DefaultReps), DefaultSets=VALUES(DefaultSets),
    DefaultHoldSeconds=VALUES(DefaultHoldSeconds), IsActive=TRUE, ImageUrl=VALUES(ImageUrl), VideoUrl=VALUES(VideoUrl);
DELETE ei FROM ExerciseInstruction ei JOIN Exercise e ON e.ExerciseId=ei.ExerciseId WHERE e.ExerciseKey='wheelbarrow';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise should be performed on a flat, non-slip surface.' FROM Exercise e WHERE e.ExerciseKey='wheelbarrow';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Lift your dog under their stomach, keeping their forelimbs on the floor.' FROM Exercise e WHERE e.ExerciseKey='wheelbarrow';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Once balanced, ask them to take a few steps forwards with their hindlimbs elevated.' FROM Exercise e WHERE e.ExerciseKey='wheelbarrow';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Only reach an angle equivalent to the height of their wrist/carpus.' FROM Exercise e WHERE e.ExerciseKey='wheelbarrow';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'It is imperative that the spine remain neutral and that they can engage through their core.' FROM Exercise e WHERE e.ExerciseKey='wheelbarrow';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'If you notice a dipping or rotation through their spine, gently place their hindlimbs back on the floor and provide more support underneath them.' FROM Exercise e WHERE e.ExerciseKey='wheelbarrow';

COMMIT;
