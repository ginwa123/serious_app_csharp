-- Up Migration: Add sequence column to sprint_issue table
ALTER TABLE projecttracking.sprint_issues
ADD COLUMN sequence INT NULL;