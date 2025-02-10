-- Drop the sequences
DROP SEQUENCE IF EXISTS projecttracking.sprint_issue_comments_id_seq;
DROP SEQUENCE IF EXISTS projecttracking.sprint_issue_comment_contents_id_seq;

-- Drop the table
DROP TABLE IF EXISTS projecttracking.sprint_issue_comment_contents;