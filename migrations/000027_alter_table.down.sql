-- Revert the foreign key constraint
ALTER TABLE projecttracking.sprint_issue_comments DROP CONSTRAINT sprint_issue_comments_sprint_issue_comment_contents_fk;

-- Rename the column back to its original name
ALTER TABLE projecttracking.sprint_issue_comments RENAME COLUMN sprint_issue_comment_content_id TO content;