USE canine_physiotherapy;

SET NAMES utf8mb4;
START TRANSACTION;

INSERT INTO Practitioner (FirstName, LastName, Email, PhoneNumber, IsActive) VALUES
('Amelia','Carter','amelia.carter@caninephysio.local','07700 900101',TRUE),
('James','Holloway','james.holloway@caninephysio.local','07700 900102',TRUE);

INSERT INTO ExerciseCategory (CategoryKey, CategoryName, Description, IsActive) VALUES
('mobility','Mobility','Exercises focused on joint mobility, flexibility, and controlled range of motion.',TRUE),
('strengthening','Strengthening','Exercises intended to improve muscular strength, endurance, and stability.',TRUE),
('balance_proprioception','Balance and Proprioception','Exercises designed to improve balance, coordination, and limb awareness.',TRUE),
('functional_rehab','Functional Rehabilitation','Exercises supporting return to normal movement and day-to-day canine function.',TRUE);

INSERT INTO SessionContentType (ContentKey, DisplayName, MobileDescription, IsActive) VALUES
('hindlimbCore','Hind Limb and Core','These canine physiotherapy videos explain how to strengthen your dogs hindlimbs and core muscles using sit to stand exercises, and forelimbs, hindlimbs and core muscles.',TRUE),
('mobilityFlexibility','Mobility and Flexibility','Improve your dog''s mobility, balance and spinal flexibility through controlled walking exercises, weaving patterns and targeted limb movements.',TRUE),
('singleRecovery','Single Recovery Session','A single guided recovery session combining carefully selected strengthening and mobility exercises for home physiotherapy.',TRUE);

INSERT INTO TermsDocument (DocumentType, VersionNumber, Title, ContentText, EffectiveFrom, IsActive) VALUES
('terms_of_service','1.0','TERMS OF SERVICE','By using Hello Buddy Canine Physiotherapy, you agree to use the app responsibly and follow all exercise guidance provided. This app provides general physiotherapy exercises for dogs and is not a substitute for professional veterinary care. Always consult your veterinarian before starting any new exercise programme. You acknowledge that exercise results may vary and that you are responsible for monitoring your dog''s comfort and safety during all activities.',CURRENT_TIMESTAMP,TRUE),
('privacy_policy','1.0','PRIVACY POLICY','We collect and store your email address and exercise progress data to provide our service. Your dog''s exercise history is stored securely and used solely to track progress within the app. We do not sell or share your personal information with third parties. Data may be stored locally on your device and synchronised with our secure servers. You may request deletion of your data at any time by contacting support.',CURRENT_TIMESTAMP,TRUE),
('acceptable_use','1.0','ACCEPTABLE USE POLICY','You agree not to misuse the app or attempt to access it using unauthorized methods. The app should only be used for its intended purpose of canine physiotherapy guidance. You must not share your account credentials with others. Any suspected security issues should be reported immediately. We reserve the right to suspend accounts that violate these terms.',CURRENT_TIMESTAMP,TRUE);

INSERT INTO AppContentBlock (ContentGroup, ContentKey, HeaderText, BodyText, LinkedTermsDocumentId, SortOrder, IsActive) VALUES
('information','about','ABOUT THIS APP','Physiotherapy at home, with practitioner guidance, can significantly improve the recovery and well-being of your dog.\n\nThis app enables practitioners provide exercise programs that can be carried out at home.',NULL,1,TRUE),
('information','physiotherapy','PHYSIOTHERAPY','Physiotherapy includes massage, exercises, and environmental modifications that can help manage pain and improve mobility.\n\nHowever, it''s important to consult with a veterinarian or certified canine physiotherapist before starting any at-home physiotherapy to ensure it''s appropriate and safe for your dog.',NULL,2,TRUE),
('information','pain','RECOGNISING PAIN','Key signs of pain include changes in behaviour, decreased activity, altered posture, increased vocalization, and changes in daily habits.\n\nIf you are concerned about pain or discomfort, stop immediately and contact your practitioner.',NULL,3,TRUE),
('warnings','exerciseDisclaimer',NULL,'If you are concerned about pain or discomfort, stop immediately and contact your practitioner.',NULL,1,TRUE);

INSERT INTO SessionSkipReason (ReasonName, Description, IsActive) VALUES
('Pet tired', 'The pet appeared too tired or fatigued to complete the planned session safely.', TRUE),
('Pain flare-up', 'The session was skipped because pain, discomfort, or sensitivity increased.', TRUE),
('Owner unavailable', 'The owner was unable to complete the scheduled session.', TRUE),
('Pet non-compliant', 'The pet was unwilling, distressed, or unable to cooperate sufficiently.', TRUE),
('Practitioner advice', 'Session was skipped based on updated practitioner advice.', TRUE);

COMMIT;
