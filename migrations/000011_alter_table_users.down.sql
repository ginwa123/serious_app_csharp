-- Down migration script
ALTER TABLE auth.users DROP CONSTRAINT IF EXISTS users_user_roles_fk;
ALTER TABLE auth.users DROP COLUMN IF EXISTS user_role_id;
