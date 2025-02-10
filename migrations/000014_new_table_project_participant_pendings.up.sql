
CREATE TABLE projecttracking.project_participant_pendings (
	id text primary key  NOT NULL,
	project_id text REFERENCES projecttracking.projects(id),
	user_id text NOT NULL,
	status TEXT CHECK (status IN ('pending', 'accepted', 'rejected')) DEFAULT 'pending',
	created_at timestamptz NULL,
	updated_at timestamptz NULL,
	deleted_at timestamptz NULL
);