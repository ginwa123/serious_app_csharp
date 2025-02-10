-- Down Migration

-- Drop tables in reverse order of creation to respect dependencies
DROP TABLE IF EXISTS projecttracking.sprint_issue_assignees;
DROP TABLE IF EXISTS projecttracking.sprint_issue_comments;
DROP TABLE IF EXISTS projecttracking.sprint_issues;
DROP TABLE IF EXISTS projecttracking.sprints;
DROP TABLE IF EXISTS projecttracking.project_participants;
DROP TABLE IF EXISTS projecttracking.projects;

-- Finally, drop the schema
DROP SCHEMA IF EXISTS projecttracking;
