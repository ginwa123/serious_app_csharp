ALTER TABLE auth.workspace_participants DROP CONSTRAINT workspace_participants_workspace_id_fkey;
ALTER TABLE auth.workspace_participants ADD CONSTRAINT workspace_participants_workspace_id_fkey FOREIGN KEY (workspace_id) REFERENCES auth.workspaces(id) ON UPDATE CASCADE;

ALTER TABLE auth.workspace_participants DROP CONSTRAINT workspace_participants_user_id_fkey;
ALTER TABLE auth.workspace_participants ADD CONSTRAINT workspace_participants_user_id_fkey FOREIGN KEY (user_id) REFERENCES auth.users(id) ON UPDATE CASCADE;

ALTER TABLE auth.workspace_participants DROP CONSTRAINT workspace_participants_workspace_participant_roles_fk;
ALTER TABLE auth.workspace_participants ADD CONSTRAINT workspace_participants_workspace_participant_roles_fk FOREIGN KEY (workspace_participant_role_id) REFERENCES auth.workspace_participant_roles(id) ON UPDATE CASCADE;


ALTER TABLE auth.users DROP CONSTRAINT users_user_roles_fk;
ALTER TABLE auth.users ADD CONSTRAINT users_user_roles_fk FOREIGN KEY (user_role_id) REFERENCES auth.user_roles(id) ON UPDATE CASCADE;


ALTER TABLE projecttracking.project_participant_requests DROP CONSTRAINT project_participant_pendings_project_id_fkey;
ALTER TABLE projecttracking.project_participant_requests ADD CONSTRAINT project_participant_pendings_project_id_fkey FOREIGN KEY (project_id) REFERENCES projecttracking.projects(id) ON UPDATE CASCADE;


ALTER TABLE projecttracking.project_participants DROP CONSTRAINT fk_project_participant_role_id;
ALTER TABLE projecttracking.project_participants ADD CONSTRAINT fk_project_participant_role_id FOREIGN KEY (project_participant_role_id) REFERENCES projecttracking.project_participant_roles(id) ON UPDATE CASCADE;


ALTER TABLE projecttracking.project_participants DROP CONSTRAINT project_participants_project_id_fkey;
ALTER TABLE projecttracking.project_participants ADD CONSTRAINT project_participants_project_id_fkey FOREIGN KEY (project_id) REFERENCES projecttracking.projects(id) ON UPDATE CASCADE;


ALTER TABLE projecttracking.projects DROP CONSTRAINT projects_workspace_id_fkey;
ALTER TABLE projecttracking.projects ADD CONSTRAINT projects_workspace_id_fkey FOREIGN KEY (workspace_id) REFERENCES auth.workspaces(id) ON UPDATE CASCADE;


ALTER TABLE projecttracking.sprint_issue_assignees DROP CONSTRAINT sprint_issue_assignees_sprint_issue_id_fkey;
ALTER TABLE projecttracking.sprint_issue_assignees ADD CONSTRAINT sprint_issue_assignees_sprint_issue_id_fkey FOREIGN KEY (sprint_issue_id) REFERENCES projecttracking.sprint_issues(id) ON UPDATE CASCADE;


ALTER TABLE projecttracking.sprint_issue_comments DROP CONSTRAINT sprint_issue_comments_sprint_issue_id_fkey;
ALTER TABLE projecttracking.sprint_issue_comments ADD CONSTRAINT sprint_issue_comments_sprint_issue_id_fkey FOREIGN KEY (sprint_issue_id) REFERENCES projecttracking.sprint_issues(id) ON UPDATE CASCADE;


ALTER TABLE projecttracking.sprint_issues DROP CONSTRAINT fk_sprint_type;
ALTER TABLE projecttracking.sprint_issues ADD CONSTRAINT fk_sprint_type FOREIGN KEY (sprint_type_id) REFERENCES projecttracking.sprint_types(id) ON UPDATE CASCADE;


ALTER TABLE projecttracking.sprint_issues DROP CONSTRAINT sprint_issues_sprint_id_fkey;
ALTER TABLE projecttracking.sprint_issues ADD CONSTRAINT sprint_issues_sprint_id_fkey FOREIGN KEY (sprint_id) REFERENCES projecttracking.sprints(id) ON UPDATE CASCADE;


ALTER TABLE projecttracking.sprint_types DROP CONSTRAINT sprint_types_sprint_id_fkey;
ALTER TABLE projecttracking.sprint_types ADD CONSTRAINT sprint_types_sprint_id_fkey FOREIGN KEY (sprint_id) REFERENCES projecttracking.sprints(id) ON UPDATE CASCADE;


ALTER TABLE projecttracking.sprints DROP CONSTRAINT sprints_project_id_fkey;
ALTER TABLE projecttracking.sprints ADD CONSTRAINT sprints_project_id_fkey FOREIGN KEY (project_id) REFERENCES projecttracking.projects(id) ON UPDATE CASCADE;
