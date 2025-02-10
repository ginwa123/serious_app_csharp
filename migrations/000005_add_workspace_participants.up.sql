CREATE TABLE auth.workspace_participants (
	id text NOT NULL,
    workspace_id text REFERENCES auth.workspaces(id),
    user_id text REFERENCES auth.users(id),
	created_at timestamptz NULL,
	updated_at timestamptz NULL,
	deleted_at timestamptz NULL,
	CONSTRAINT workspace_participants_pk PRIMARY KEY (id)
);
