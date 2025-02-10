BEGIN;

ALTER TABLE projecttracking.sprints
DROP COLUMN status;

COMMIT;