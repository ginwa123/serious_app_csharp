ALTER TABLE auth.users ADD user_role_id text NULL;
ALTER TABLE auth.users ADD CONSTRAINT users_user_roles_fk FOREIGN KEY (user_role_id) REFERENCES auth.user_roles(id);
