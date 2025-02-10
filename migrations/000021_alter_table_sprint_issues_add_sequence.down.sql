-- Down Migration: Remove sequence column from sprint_issue table
ALTER TABLE projecttracking.sprint_issues
DROP COLUMN IF EXISTS sequence;