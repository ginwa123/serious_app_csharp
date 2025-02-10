ALTER TABLE projecttracking.sprint_issues
ALTER COLUMN type TYPE text USING type::text;

-- Then add the foreign key constraint
ALTER TABLE projecttracking.sprint_issues
ADD CONSTRAINT fk_sprint_type
FOREIGN KEY (type) REFERENCES projecttracking.sprint_types(id);