CREATE TABLE projecttracking.sprint_issue_comment_content_types (
	name text NOT NULL,
	created_at timestamptz NOT NULL,
	updated_at timestamptz NOT NULL,
	deleted_at timestamptz NULL,
	CONSTRAINT sprint_issue_comment_content_types_pk PRIMARY KEY (name)
);

insert into projecttracking.sprint_issue_comment_content_types
(name, created_at, updated_at)
values
('TEXT', now(), now()),
('IMAGE', now(), now()),
('IMAGE_BASE64', now(), now());

ALTER TABLE projecttracking.sprint_issue_comment_contents
ADD CONSTRAINT sprint_issue_comment_contents_sprint_issue_comment_content_types_fk FOREIGN KEY (sprint_issue_comment_content_type_name) REFERENCES projecttracking.sprint_issue_comment_content_types("name") ON UPDATE CASCADE;




