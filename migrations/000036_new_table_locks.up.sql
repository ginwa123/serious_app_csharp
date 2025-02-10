CREATE TABLE public.locks (
    lock_key TEXT PRIMARY KEY,
    locked_at TIMESTAMP WITH TIME ZONE DEFAULT now(),
    owner_id TEXT
);

ALTER TABLE auth.api_url_permissions DROP CONSTRAINT api_url_permissions_api_url_id_fk;
ALTER TABLE auth.api_url_permissions ADD CONSTRAINT api_url_permissions_api_url_id_fk FOREIGN KEY (api_url_id) REFERENCES auth.api_urls(id) ON DELETE CASCADE ON UPDATE CASCADE;
ALTER TABLE auth.users DROP CONSTRAINT users_unique;


ALTER TABLE auth.workspace_participants DROP CONSTRAINT workspace_participants_workspace_participant_roles_fk;
ALTER TABLE auth.workspace_participants DROP COLUMN workspace_participant_role_id;



CREATE TABLE auth.workspace_participant_assignees (
	id text NOT NULL,
	workspace_participant_role_id text NOT NULL,
	workspace_participant_id text NOT NULL,
	created_at timestamp with time zone NOT NULL,
	updated_at timestamp with time zone NOT NULL,
	deleted_at timestamp with time zone NULL,
	CONSTRAINT workspace_participant_assignees_pk PRIMARY KEY (id),
	CONSTRAINT workspace_participant_assignees_workspace_participant_roles_fk FOREIGN KEY (workspace_participant_role_id) REFERENCES auth.workspace_participant_roles(id),
	CONSTRAINT workspace_participant_assignees_workspace_participants_fk FOREIGN KEY (workspace_participant_id) REFERENCES auth.workspace_participants(id)
);
