CREATE TABLE auth.workspaces (
	id text NOT NULL,
	created_at timestamptz NULL,
	updated_at timestamptz NULL,
	deleted_at timestamptz NULL,
	"name" text,
	CONSTRAINT workspaces_pk PRIMARY KEY (id)
);
