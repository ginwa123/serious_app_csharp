ALTER TABLE projecttracking.sprint_issue_assignees DROP CONSTRAINT sprint_issue_assignees_users_fk;

ALTER TABLE projecttracking.sprint_issue_assignees ADD CONSTRAINT sprint_issue_assignees_users_fk FOREIGN KEY (user_id) REFERENCES auth.users(id) ON UPDATE CASCADE