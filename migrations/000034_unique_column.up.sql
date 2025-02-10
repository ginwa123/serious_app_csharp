--ALTER TABLE auth.user_permissions ADD CONSTRAINT user_permissions_unique UNIQUE ("name");
ALTER TABLE auth.user_roles ADD CONSTRAINT user_roles_unique UNIQUE ("name");
