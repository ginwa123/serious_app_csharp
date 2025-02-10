CREATE TABLE auth.user_role_assignees (
    id text not null,
	user_id  text not null,
	user_role_id text not null,
	created_at timestamptz NOT NULL,
	updated_at timestamptz NOT NULL,
	deleted_at timestamptz NULL,
	CONSTRAINT user_role_assignees_pk PRIMARY KEY (id)
);

ALTER TABLE auth.user_role_assignees ADD CONSTRAINT user_role_assignees_user_id_fk
FOREIGN KEY (user_id) REFERENCES auth.users(id) ON UPDATE CASCADE;

ALTER TABLE auth.user_role_assignees ADD CONSTRAINT user_role_assignees_user_role_id_fk
FOREIGN KEY (user_role_id) REFERENCES auth.user_roles(id) ON UPDATE CASCADE;