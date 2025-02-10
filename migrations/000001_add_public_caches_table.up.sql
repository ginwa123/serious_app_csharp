CREATE TABLE public.caches (
	"key" text NOT NULL,
	value text NULL,
	expiration timestamptz NULL,
    created_at timestamptz NULL,
	CONSTRAINT caches_pk PRIMARY KEY ("key")
);
