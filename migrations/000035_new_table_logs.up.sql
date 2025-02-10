CREATE TABLE public.logs (
    id text not null,
    message text NULL,
    method text NULL,
    url text NULL,
	created_at timestamptz NOT NULL
--	CONSTRAINT logs_pk PRIMARY KEY (id)
);
