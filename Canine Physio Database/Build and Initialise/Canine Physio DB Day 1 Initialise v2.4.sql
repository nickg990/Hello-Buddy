USE canine_physiotherapy;

SET NAMES utf8mb4;
START TRANSACTION;

-- CANINE PHYSIOTHERAPY - DAY 1 SEED DATA V2.4 FULL

INSERT INTO Practitioner (FirstName, LastName, Email, PhoneNumber, IsActive) VALUES
('Amelia', 'Carter', 'amelia.carter@caninephysio.local', '07700 900101', TRUE),
('James', 'Holloway', 'james.holloway@caninephysio.local', '07700 900102', TRUE);

INSERT INTO ExerciseCategory (CategoryKey, CategoryName, Description, IsActive) VALUES
('mobility', 'Mobility', 'Exercises focused on joint mobility, flexibility, and controlled range of motion.', TRUE),
('strengthening', 'Strengthening', 'Exercises intended to improve muscular strength, endurance, and stability.', TRUE),
('balance_proprioception', 'Balance and Proprioception', 'Exercises designed to improve balance, coordination, and limb awareness.', TRUE),
('functional_rehab', 'Functional Rehabilitation', 'Exercises supporting return to normal movement and day-to-day canine function.', TRUE);

INSERT INTO SessionContentType (ContentKey, DisplayName, MobileDescription, IsActive) VALUES
('hindlimbCore', 'Hind Limb and Core', 'These canine physiotherapy videos explain how to strengthen your dogs hindlimbs and core muscles using sit to stand exercises, and forelimbs, hindlimbs and core muscles.', TRUE),
('mobilityFlexibility', 'Mobility and Flexibility', 'Improve your dog''s mobility, balance and spinal flexibility through controlled walking exercises, weaving patterns and targeted limb movements.', TRUE),
('singleRecovery', 'Single Recovery Session', 'A single guided recovery session combining carefully selected strengthening and mobility exercises for home physiotherapy.', TRUE);

INSERT INTO TermsDocument (DocumentType, VersionNumber, Title, ContentText, EffectiveFrom, IsActive) VALUES
('terms_of_service', '1.0', 'TERMS OF SERVICE', 'By using Hello Buddy Canine Physiotherapy, you agree to use the app responsibly and follow all exercise guidance provided. This app provides general physiotherapy exercises for dogs and is not a substitute for professional veterinary care. Always consult your veterinarian before starting any new exercise programme. You acknowledge that exercise results may vary and that you are responsible for monitoring your dog''s comfort and safety during all activities.', CURRENT_TIMESTAMP, TRUE),
('privacy_policy', '1.0', 'PRIVACY POLICY', 'We collect and store your email address and exercise progress data to provide our service. Your dog''s exercise history is stored securely and used solely to track progress within the app. We do not sell or share your personal information with third parties. Data may be stored locally on your device and synchronised with our secure servers. You may request deletion of your data at any time by contacting support.', CURRENT_TIMESTAMP, TRUE),
('acceptable_use', '1.0', 'ACCEPTABLE USE POLICY', 'You agree not to misuse the app or attempt to access it using unauthorized methods. The app should only be used for its intended purpose of canine physiotherapy guidance. You must not share your account credentials with others. Any suspected security issues should be reported immediately. We reserve the right to suspend accounts that violate these terms.', CURRENT_TIMESTAMP, TRUE);

INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ExerciseCategoryId, 'baitedBackStretch', 'Baited back stretch',
'Neck and spine stretches on block – 2-3 stretches per direction at 7-15s hold twice daily (following heat packs where possible). Perform this action in a standing position and with a treat in hand.',
'Raise your dog''s forelimbs onto a flat, stable platform a couple of inches high and stretch their neck upwards then forwards/to the side.
Place your hand in front of your dog''s hindlimbs and encourage their head forwards with a treat, hold for at least 7s.
Take your hand from your dog''s nose to the side, wrapping around your legs as you stand next to them, stopping just before they start to rotate their head and/or move their forelimbs.
Hold this position for 7s then return to neutral head position (i.e., normal straight head position).
Repeat the action to the other side after a few seconds.
Encourage their head down without your dog laying down to stretch the muscles on the top of the neck by holding a treat between your dog''s forelimbs.
Remember to always return to neutral before asking for another stretch.
Do not take your dog''s head beyond point of comfort.',
3,1,7,TRUE,'https://cdn.caninephysio.local/exercises/baited_back_stretch.jpg','https://cdn.caninephysio.local/exercises/baited_back_stretch_video.mp4'
FROM ExerciseCategory WHERE CategoryKey='mobility';

INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ExerciseCategoryId, 'raisedPoles', 'Raised poles',
'Five reps (there and back counts as one rep) twice daily. This exercise should be performed on a flat, non-slip surface and at an active walk. Lay X flat poles on the ground and ask your dog to walk over the centre of each.',
'The poles should be distanced so that each foot is placed in each gap between the poles only once (i.e., approximately the length of your dog from base of neck to rear).
Upon completion of the poles, make sure you turn your dog nice and wide.
If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.
If they are still scuffing poles, you should remove one pole to reduce the intensity of the exercise.
Raise the last pole on one side by a few centimetres (never higher than your dog''s hock/ankle).
Raise the first pole on the alternate side to the last pole by the same measure of height.',
5,1,NULL,TRUE,'https://cdn.caninephysio.local/exercises/raised_poles.jpg','https://cdn.caninephysio.local/exercises/raised_poles_video.mp4'
FROM ExerciseCategory WHERE CategoryKey='functional_rehab';

INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ExerciseCategoryId, 'raisedSitStands', 'Raised sit stands',
'Five reps thrice daily. This exercise involves a controlled, square sit onto a firm cushion a couple of inches tall with your dog''s legs underneath themselves.',
'Once the sit has been achieved, ask them to stand with their hindlimbs on the cushion (you can use a treat or a toy for encouragement but only if controlled).
Try to stand in front of them as this prevents them from using their forelimbs to drag themselves forwards.
This is similar to a squat, with them using their hindlimbs to push themselves upwards.
If necessary, you can use wall support by backing them into a corner and placing a cushion to the side of their hindlimbs to allow for hip support either side of their hindlimbs.',
5,2,NULL,TRUE,'https://cdn.caninephysio.local/exercises/raised_sit_stands.jpg','https://cdn.caninephysio.local/exercises/raised_sit_stands_video.mp4'
FROM ExerciseCategory WHERE CategoryKey='strengthening';

INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ExerciseCategoryId, 'stepUp', 'Step up',
'Five reps twice daily. Perform this exercise on a flat stable surface with your dog standing square.',
'Place a non-slip block flat on the floor in front of your dog.
Ask your dog to step up onto the block, with forelimbs only, from one side and dismount onto the floor on other side of the block.
Repeat this action going in the other direction.
Then do the same in a forward-backward direction.',
5,3,NULL,TRUE,'https://cdn.caninephysio.local/exercises/step_up.jpg','https://cdn.caninephysio.local/exercises/step_up_video.mp4'
FROM ExerciseCategory WHERE CategoryKey='functional_rehab';

INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ExerciseCategoryId, 'circles', 'Circles',
'Three reps twice daily. Walk your dog around four markers placed 1.5 body lengths apart in a circular path, keeping your dog on the inside. Perform clockwise and anticlockwise on a flat, non-slip surface at an active walk.',
'Place four markers or cones 1.5 body lengths apart from the centre of the circle and walk your dog around them.
Your dog should be on the inside, closest to the circle markers.
Remember to do this exercise on both sides (i.e. clockwise and anticlockwise).
This should be performed on a flat, non-slip surface and at an active walk without rushing your dog.',
3,2,NULL,TRUE,'https://cdn.caninephysio.local/exercises/circles.jpg','https://cdn.caninephysio.local/exercises/circles.mp4'
FROM ExerciseCategory WHERE CategoryKey='balance_proprioception';

INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ExerciseCategoryId, 'givePaw', 'Give paw',
'Three reps twice daily. Ask your sitting dog to give their paw, bringing it upwards slightly to gain flexion from the elbow joint. If needed, start by picking up the limb yourself.',
'Ask your dog to sit square then ask for their paw.
Depending on their level of training, you may have to start by picking up the limb yourself at first.
Bring the paw upwards slightly to gain a little flexion from the elbow joint.',
3,2,NULL,TRUE,'https://cdn.caninephysio.local/exercises/give_paw.jpg','https://cdn.caninephysio.local/exercises/give_paw.mp4'
FROM ExerciseCategory WHERE CategoryKey='mobility';

INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ExerciseCategoryId, 'slowLeadWalk', 'Slow lead walk',
'Three reps twice daily. Walk your dog on a short lead with a raised head at a slow, controlled pace to encourage equal weight bearing on each limb. Consider a harness if your dog struggles with pulling.',
'Walk your dog on a short lead with a raised head.
This should be slow enough to allow them to weight bear equally on each limb so do not rush them beyond their capable speed.
The goal is to keep them at heel but if your dog struggles with pulling, you may want to consider a harness or halti (but only if this does not impact movement negatively).',
3,2,NULL,TRUE,'https://cdn.caninephysio.local/exercises/slow_lead.jpg','https://cdn.caninephysio.local/exercises/slow_lead_walk.mp4'
FROM ExerciseCategory WHERE CategoryKey='functional_rehab';

INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ExerciseCategoryId, 'layToStand', 'Lay to stand',
'Three reps twice daily. A controlled lay-to-stand exercise using hindlimb strength, similar to a squat. Stand in front of your dog to prevent them dragging forward with their forelimbs.',
'This exercise involves a controlled, square lay with your dog''s legs underneath themselves.
Once the lay has been achieved, ask them to stand (you can use a treat or a toy for encouragement but only if controlled).
Try to stand in front of them as this prevents them from using their forelimbs to drag themselves forwards.
This is similar to a squat, with them using their hindlimbs to push themselves upwards.',
3,2,NULL,TRUE,'https://cdn.caninephysio.local/exercises/lay_to_stand.jpg','https://cdn.caninephysio.local/exercises/lay_to_stand.mp4'
FROM ExerciseCategory WHERE CategoryKey='strengthening';

INSERT INTO Exercise (ExerciseCategoryId, ExerciseKey, Title, ObjectiveSummary, InstructionsText, DefaultReps, DefaultSets, DefaultHoldSeconds, IsActive, ImageUrl, VideoUrl)
SELECT ExerciseCategoryId, 'weaving', 'Weaving',
'Three reps twice daily. Walk your dog around three markers placed 6 feet apart in a weaving pattern on a flat, non-slip surface. Maintain slow, controlled movement progressing from shallow to deeper turns to encourage spinal bending.',
'Place three markers or cones 6 feet apart and walk your dog around them in a weaving configuration.
This should be performed on a flat, non-slip surface and at an active walk without rushing your dog.
Weaving must be slow and controlled with wide turns encouraged to allow bending.
Start with a shallow turn, close to the markers and progress to a deeper turn to encourage spinal bending slowly.',
3,2,NULL,TRUE,'https://cdn.caninephysio.local/exercises/weaving.jpg','https://cdn.caninephysio.local/exercises/weaving.mp4'
FROM ExerciseCategory WHERE CategoryKey='balance_proprioception';

-- Instruction steps
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Raise your dog''s forelimbs onto a flat, stable platform a couple of inches high and stretch their neck upwards then forwards/to the side.' FROM Exercise e WHERE e.ExerciseKey='baitedBackStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Place your hand in front of your dog''s hindlimbs and encourage their head forwards with a treat, hold for at least 7s.' FROM Exercise e WHERE e.ExerciseKey='baitedBackStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Take your hand from your dog''s nose to the side, wrapping around your legs as you stand next to them, stopping just before they start to rotate their head and/or move their forelimbs.' FROM Exercise e WHERE e.ExerciseKey='baitedBackStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Hold this position for 7s then return to neutral head position (i.e., normal straight head position).' FROM Exercise e WHERE e.ExerciseKey='baitedBackStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Repeat the action to the other side after a few seconds.' FROM Exercise e WHERE e.ExerciseKey='baitedBackStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Encourage their head down without your dog laying down to stretch the muscles on the top of the neck by holding a treat between your dog''s forelimbs.' FROM Exercise e WHERE e.ExerciseKey='baitedBackStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 7, 'Remember to always return to neutral before asking for another stretch.' FROM Exercise e WHERE e.ExerciseKey='baitedBackStretch';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 8, 'Do not take your dog''s head beyond point of comfort.' FROM Exercise e WHERE e.ExerciseKey='baitedBackStretch';

INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'The poles should be distanced so that each foot is placed in each gap between the poles only once (i.e., approximately the length of your dog from base of neck to rear).' FROM Exercise e WHERE e.ExerciseKey='raisedPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Upon completion of the poles, make sure you turn your dog nice and wide.' FROM Exercise e WHERE e.ExerciseKey='raisedPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'If your dog starts to scuff a pole with their feet, you may need to alter the distance between the poles.' FROM Exercise e WHERE e.ExerciseKey='raisedPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'If they are still scuffing poles, you should remove one pole to reduce the intensity of the exercise.' FROM Exercise e WHERE e.ExerciseKey='raisedPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 5, 'Raise the last pole on one side by a few centimetres (never higher than your dog''s hock/ankle).' FROM Exercise e WHERE e.ExerciseKey='raisedPoles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 6, 'Raise the first pole on the alternate side to the last pole by the same measure of height.' FROM Exercise e WHERE e.ExerciseKey='raisedPoles';

INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Once the sit has been achieved, ask them to stand with their hindlimbs on the cushion (you can use a treat or a toy for encouragement but only if controlled).' FROM Exercise e WHERE e.ExerciseKey='raisedSitStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Try to stand in front of them as this prevents them from using their forelimbs to drag themselves forwards.' FROM Exercise e WHERE e.ExerciseKey='raisedSitStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'This is similar to a squat, with them using their hindlimbs to push themselves upwards.' FROM Exercise e WHERE e.ExerciseKey='raisedSitStands';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'If necessary, you can use wall support by backing them into a corner and placing a cushion to the side of their hindlimbs to allow for hip support either side of their hindlimbs.' FROM Exercise e WHERE e.ExerciseKey='raisedSitStands';

INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Place a non-slip block flat on the floor in front of your dog.' FROM Exercise e WHERE e.ExerciseKey='stepUp';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Ask your dog to step up onto the block, with forelimbs only, from one side and dismount onto the floor on other side of the block.' FROM Exercise e WHERE e.ExerciseKey='stepUp';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Repeat this action going in the other direction.' FROM Exercise e WHERE e.ExerciseKey='stepUp';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Then do the same in a forward-backward direction.' FROM Exercise e WHERE e.ExerciseKey='stepUp';

INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Place four markers or cones 1.5 body lengths apart from the centre of the circle and walk your dog around them.' FROM Exercise e WHERE e.ExerciseKey='circles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Your dog should be on the inside, closest to the circle markers.' FROM Exercise e WHERE e.ExerciseKey='circles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Remember to do this exercise on both sides (i.e. clockwise and anticlockwise).' FROM Exercise e WHERE e.ExerciseKey='circles';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'This should be performed on a flat, non-slip surface and at an active walk without rushing your dog.' FROM Exercise e WHERE e.ExerciseKey='circles';

INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Ask your dog to sit square then ask for their paw.' FROM Exercise e WHERE e.ExerciseKey='givePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Depending on their level of training, you may have to start by picking up the limb yourself at first.' FROM Exercise e WHERE e.ExerciseKey='givePaw';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Bring the paw upwards slightly to gain a little flexion from the elbow joint.' FROM Exercise e WHERE e.ExerciseKey='givePaw';

INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Walk your dog on a short lead with a raised head.' FROM Exercise e WHERE e.ExerciseKey='slowLeadWalk';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'This should be slow enough to allow them to weight bear equally on each limb so do not rush them beyond their capable speed.' FROM Exercise e WHERE e.ExerciseKey='slowLeadWalk';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'The goal is to keep them at heel but if your dog struggles with pulling, you may want to consider a harness or halti (but only if this does not impact movement negatively).' FROM Exercise e WHERE e.ExerciseKey='slowLeadWalk';

INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'This exercise involves a controlled, square lay with your dog''s legs underneath themselves.' FROM Exercise e WHERE e.ExerciseKey='layToStand';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'Once the lay has been achieved, ask them to stand (you can use a treat or a toy for encouragement but only if controlled).' FROM Exercise e WHERE e.ExerciseKey='layToStand';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Try to stand in front of them as this prevents them from using their forelimbs to drag themselves forwards.' FROM Exercise e WHERE e.ExerciseKey='layToStand';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'This is similar to a squat, with them using their hindlimbs to push themselves upwards.' FROM Exercise e WHERE e.ExerciseKey='layToStand';

INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 1, 'Place three markers or cones 6 feet apart and walk your dog around them in a weaving configuration.' FROM Exercise e WHERE e.ExerciseKey='weaving';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 2, 'This should be performed on a flat, non-slip surface and at an active walk without rushing your dog.' FROM Exercise e WHERE e.ExerciseKey='weaving';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 3, 'Weaving must be slow and controlled with wide turns encouraged to allow bending.' FROM Exercise e WHERE e.ExerciseKey='weaving';
INSERT INTO ExerciseInstruction (ExerciseId, StepNumber, InstructionText) SELECT e.ExerciseId, 4, 'Start with a shallow turn, close to the markers and progress to a deeper turn to encourage spinal bending slowly.' FROM Exercise e WHERE e.ExerciseKey='weaving';

INSERT INTO AppContentBlock (ContentGroup, ContentKey, HeaderText, BodyText, LinkedTermsDocumentId, SortOrder, IsActive) VALUES
('information', 'about', 'ABOUT THIS APP', 'Physiotherapy at home, with practitioner guidance, can significantly improve the recovery and well-being of your dog.

This app enables practitioners provide exercise programs that can be carried out at home.', NULL, 1, TRUE),
('information', 'physiotherapy', 'PHYSIOTHERAPY', 'Physiotherapy includes massage, exercises, and environmental modifications that can help manage pain and improve mobility.

However, it''s important to consult with a veterinarian or certified canine physiotherapist before starting any at-home physiotherapy to ensure it''s appropriate and safe for your dog.', NULL, 2, TRUE),
('information', 'pain', 'RECOGNISING PAIN', 'Key signs of pain include changes in behaviour, decreased activity, altered posture, increased vocalization, and changes in daily habits.

If you are concerned about pain or discomfort, stop immediately and contact your practitioner.', NULL, 3, TRUE);

INSERT INTO AppContentBlock (ContentGroup, ContentKey, HeaderText, BodyText, LinkedTermsDocumentId, SortOrder, IsActive)
SELECT 'termsConditions', 'termsOfService', td.Title, td.ContentText, td.TermsDocumentId, 1, TRUE
FROM TermsDocument td
WHERE td.DocumentType = 'terms_of_service' AND td.VersionNumber = '1.0';

INSERT INTO AppContentBlock (ContentGroup, ContentKey, HeaderText, BodyText, LinkedTermsDocumentId, SortOrder, IsActive)
SELECT 'termsConditions', 'privacyPolicy', td.Title, td.ContentText, td.TermsDocumentId, 2, TRUE
FROM TermsDocument td
WHERE td.DocumentType = 'privacy_policy' AND td.VersionNumber = '1.0';

INSERT INTO AppContentBlock (ContentGroup, ContentKey, HeaderText, BodyText, LinkedTermsDocumentId, SortOrder, IsActive)
SELECT 'termsConditions', 'acceptableUse', td.Title, td.ContentText, td.TermsDocumentId, 3, TRUE
FROM TermsDocument td
WHERE td.DocumentType = 'acceptable_use' AND td.VersionNumber = '1.0';

INSERT INTO AppContentBlock (ContentGroup, ContentKey, HeaderText, BodyText, LinkedTermsDocumentId, SortOrder, IsActive) VALUES
('warnings', 'exerciseDisclaimer', NULL, 'If you are concerned about pain or discomfort, stop immediately and contact your practitioner.', NULL, 1, TRUE);

INSERT INTO SessionSkipReason (ReasonName, Description, IsActive) VALUES
('Pet tired', 'The pet appeared too tired or fatigued to complete the planned session safely.', TRUE),
('Pain flare-up', 'The session was skipped because pain, discomfort, or sensitivity increased.', TRUE),
('Owner unavailable', 'The owner was unable to complete the scheduled session.', TRUE),
('Pet non-compliant', 'The pet was unwilling, distressed, or unable to cooperate sufficiently.', TRUE),
('Practitioner advice', 'Session was skipped based on updated practitioner advice.', TRUE);

COMMIT;
