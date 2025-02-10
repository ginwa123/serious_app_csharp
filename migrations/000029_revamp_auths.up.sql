ALTER TABLE auth.users DROP CONSTRAINT users_user_roles_fk;
ALTER TABLE auth.users DROP COLUMN user_role_id;

CREATE TABLE auth.user_permissions (
	id text NOT NULL,
	"name" text NOT NULL,
	created_at timestamptz NOT NULL,
	updated_at timestamptz NOT NULL,
	deleted_at timestamptz NULL,
	CONSTRAINT user_role_granted_permissions_pk PRIMARY KEY (id)
);


CREATE TABLE auth.user_role_permissions (
	id text NOT NULL,
	user_role_id text NOT NULL,
	user_permission_id text NOT NULL,
	user_id text NOT NULL,
	created_at timestamptz NOT NULL,
	updated_at timestamptz NOT NULL,
	deleted_at timestamptz NULL,
	CONSTRAINT user_role_permissions_pk PRIMARY KEY (id),
	CONSTRAINT user_role_permissions_users_fk FOREIGN KEY (user_id) REFERENCES auth.users(id) ON UPDATE CASCADE,
	CONSTRAINT user_role_permissions_user_roles_fk FOREIGN KEY (user_role_id) REFERENCES auth.user_roles(id) ON UPDATE CASCADE
);

ALTER TABLE auth.user_role_permissions ADD CONSTRAINT user_role_permissions_user_permissions_fk FOREIGN KEY (user_permission_id) REFERENCES auth.user_permissions(id) ON UPDATE CASCADE;
