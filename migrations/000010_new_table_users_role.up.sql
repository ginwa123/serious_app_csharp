CREATE TABLE auth.user_roles (
	id text NOT NULL primary key,
    name text not null ,
	created_at timestamptz NULL,
	updated_at timestamptz NULL,
	deleted_at timestamptz NULL
);

