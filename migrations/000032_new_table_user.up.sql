ALTER TABLE auth.user_role_permissions DROP CONSTRAINT user_role_permissions_users_fk;
ALTER TABLE auth.user_role_permissions DROP COLUMN user_id;
