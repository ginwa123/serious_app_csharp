-- Delete inserted data from the table
DELETE FROM projecttracking.sprint_issue_comment_content_types
WHERE name IN ('TEXT', 'IMAGE');

-- Drop the table
DROP TABLE IF EXISTS projecttracking.sprint_issue_comment_content_types;