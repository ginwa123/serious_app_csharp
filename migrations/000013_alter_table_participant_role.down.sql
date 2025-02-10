ALTER TABLE projecttracking.project_participants
DROP CONSTRAINT IF EXISTS fk_project_participant_role_id,
DROP COLUMN IF EXISTS project_participant_role_id;
