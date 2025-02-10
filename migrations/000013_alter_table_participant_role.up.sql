ALTER TABLE projecttracking.project_participants
ADD COLUMN project_participant_role_id text,
ADD CONSTRAINT fk_project_participant_role_id
    FOREIGN KEY (project_participant_role_id)
    REFERENCES projecttracking.project_participant_roles (id);