-- Add the foreign key constraint back
ALTER TABLE projecttracking.sprint_issue_comments
    ADD CONSTRAINT sprint_issue_comments_sprint_issue_comment_contents_fk
    FOREIGN KEY (sprint_issue_comment_content_id)
    REFERENCES projecttracking.sprint_issue_comment_contents(id)
    ON UPDATE CASCADE;

-- Add the column back
ALTER TABLE projecttracking.sprint_issue_comments
    ADD COLUMN sprint_issue_comment_content_id text; -- Make sure to match the original column type