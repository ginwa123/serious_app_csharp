CREATE SCHEMA auth;
CREATE TABLE auth.users (
	id text NOT NULL,
	username text NOT NULL,
	"password" text NOT NULL,
	created_at timestamptz NULL,
	updated_at timestamptz NULL,
	deleted_at timestamptz NULL,
	"name" text NULL,
	CONSTRAINT users_pk PRIMARY KEY (id),
	CONSTRAINT users_unique UNIQUE (username)
);
