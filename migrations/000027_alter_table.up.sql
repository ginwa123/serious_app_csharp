
ALTER TABLE projecttracking.sprint_issue_comments RENAME COLUMN "content" TO sprint_issue_comment_content_id;
ALTER TABLE projecttracking.sprint_issue_comments ADD CONSTRAINT sprint_issue_comments_sprint_issue_comment_contents_fk FOREIGN KEY (sprint_issue_comment_content_id) REFERENCES projecttracking.sprint_issue_comment_contents(id) ON UPDATE CASCADE;

