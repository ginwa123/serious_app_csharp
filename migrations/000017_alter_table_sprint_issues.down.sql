-- Remove the foreign key constraint
ALTER TABLE projecttracking.sprint_issues
DROP CONSTRAINT IF EXISTS fk_sprint_type;

-- Revert the type column back to text
ALTER TABLE projecttracking.sprint_issues
ALTER COLUMN type TYPE text USING type::text;
