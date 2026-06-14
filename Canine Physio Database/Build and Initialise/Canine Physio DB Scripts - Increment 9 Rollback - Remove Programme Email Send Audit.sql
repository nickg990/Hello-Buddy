-- =============================================================================
-- CANINE PHYSIOTHERAPY - INCREMENT 9 ROLLBACK: REMOVE PROGRAMME EMAIL SEND AUDIT
-- =============================================================================
-- Safe to run against an EXISTING database (idempotent DROP TABLE IF EXISTS).
-- Removes the ProgrammeEmailSend audit table now that Email PDF functionality is withdrawn.
-- =============================================================================

USE canine_physiotherapy;
SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

DROP TABLE IF EXISTS ProgrammeEmailSend;

SET FOREIGN_KEY_CHECKS = 1;
