CREATE TABLE auth.api_urls (
    id text not null,
	url text NOT NULL,
	created_at timestamptz NOT NULL,
	updated_at timestamptz NOT NULL,
	deleted_at timestamptz NULL,
	method text NULL,
	CONSTRAINT api_urls_pk PRIMARY KEY (id)
);

CREATE TABLE auth.api_url_permissions (
    id text not null,
    api_url_id text NOT NULL,
	user_permission_id text NOT NULL ,
	created_at timestamptz NOT NULL,
	updated_at timestamptz NOT NULL,
	deleted_at timestamptz NULL,
	CONSTRAINT api_url_permissions_pk PRIMARY KEY (id)
);

ALTER TABLE auth.api_url_permissions ADD CONSTRAINT api_url_permissions_user_permission_id_fk
FOREIGN KEY (user_permission_id) REFERENCES auth.user_permissions(id) ON UPDATE CASCADE;

ALTER TABLE auth.api_url_permissions ADD CONSTRAINT api_url_permissions_api_url_id_fk
FOREIGN KEY (api_url_id) REFERENCES auth.api_urls(id) ON UPDATE CASCADE;


