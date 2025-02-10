insert into auth.user_roles(id, "name", created_at, updated_at, deleted_at)
 values ('1', 'admin', now(), now(), null),
       ('2', 'member', now(), now(), null);

 insert into auth.user_permissions (id, name, created_at, updated_at)
 values ('1', 'can_readall', now(), now()),
 ('2', 'can_createall', now(), now()),
 ('3', 'can_updateall', now(), now()),
 ('4', 'can_deleteall', now(), now());

