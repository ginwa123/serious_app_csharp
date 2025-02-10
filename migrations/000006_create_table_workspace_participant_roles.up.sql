CREATE TABLE auth.workspace_participant_roles (
	id text NOT NULL,
    name text not null unique,
	created_at timestamptz NULL,
	updated_at timestamptz NULL,
	deleted_at timestamptz NULL,
	CONSTRAINT workspace_participant_roles_pk PRIMARY KEY (id)
);
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
