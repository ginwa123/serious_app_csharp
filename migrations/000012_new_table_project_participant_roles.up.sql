
CREATE TABLE projecttracking.project_participant_roles (
	id text NOT NULL,
	"name" text NOT NULL,
	created_at timestamptz NULL,
	updated_at timestamptz NULL,
	deleted_at timestamptz NULL,
	CONSTRAINT project_participant_roles_name_key UNIQUE (name),
	CONSTRAINT project_participant_roles_pk PRIMARY KEY (id)
);