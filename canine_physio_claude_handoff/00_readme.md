# Canine Physiotherapy Admin System — Claude Handoff Pack

Prepared for Nick as a structured requirements handoff for Claude Opus 4.7.

## Purpose

This pack converts the prior canine physiotherapy design discussions into a set of markdown requirements that can be fed to Claude incrementally. It is deliberately modular to avoid overloading a single Claude chat.

## Core direction

Release 1 is an admin-first system for a canine physiotherapy practice. It manages owners, pets, cases, notes, exercise data and exercise programmes, then generates a branded PDF exercise programme with clickable video links.

The mobile app, JSON programme delivery, offline sync and owner progress capture are future-phase requirements. They should influence architectural choices but should not be built in the first release unless Nick explicitly expands the scope.

## How to use this pack with Claude

1. Start with `09_claude_prompt_pack.md`.
2. Give Claude `01_project_summary_and_release_scope.md`, `02_database_existing_design.md`, `04_admin_page_flow_and_layout.md`, `05_ui_design_direction.md`, `06_pdf_programme_requirements.md`, `07_technical_build_instructions.md`, `08_acceptance_criteria_and_tests.md`, and `10_implementation_approach.md`.
3. Add Mermaid files one at a time when asking Claude to design workflows.
4. Ask Claude to build Increment 1 only first. Do not ask for the full solution in a single response.

## Mermaid files

Each Mermaid chart is in its own markdown file as requested:

- `03a_mermaid_owner_pet_case_onboarding.md`
- `03b_mermaid_exercise_library_management.md`
- `03c_mermaid_programme_creation.md`
- `03d_mermaid_pdf_preview_publish_versioning.md`
- `03e_mermaid_gdpr_data_controls.md`
- `03f_mermaid_pdf_generation_sequence.md`
- `03g_mermaid_future_mobile_json_publication.md`
- `03h_mermaid_future_owner_progress_sync.md`

## Important constraint

The database has already been designed. Claude must not redesign it. Any schema concerns should be raised as proposed migrations or questions, not silently implemented as a different model.
