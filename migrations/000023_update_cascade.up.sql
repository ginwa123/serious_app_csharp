ALTER TABLE projecttracking.project_participants ADD CONSTRAINT project_participants_users_fk FOREIGN KEY (user_id) REFERENCES auth.users(id) ON UPDATE CASCADE;

ALTER TABLE projecttracking.project_participant_requests ADD CONSTRAINT project_participant_requests_users_fk FOREIGN KEY (user_id) REFERENCES auth.users(id) ON UPDATE CASCADE;

ALTER TABLE projecttracking.sprint_issue_comments ADD CONSTRAINT sprint_issue_comments_users_fk FOREIGN KEY (user_id) REFERENCES auth.users(id) ON UPDATE CASCADE;

ALTER TABLE projecttracking.sprint_issue_assignees ADD CONSTRAINT sprint_issue_assignees_users_fk FOREIGN KEY (id) REFERENCES auth.users(id) ON UPDATE CASCADE;
