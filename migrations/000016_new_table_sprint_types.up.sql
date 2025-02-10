
CREATE TABLE projecttracking.sprint_types (
	id text primary key  NOT NULL,
	sprint_id text REFERENCES projecttracking.sprints(id) not null,
	name text not null,
	created_at timestamptz NULL,
	updated_at timestamptz NULL,
	deleted_at timestamptz NULL
);