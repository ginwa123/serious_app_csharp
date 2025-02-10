CREATE SEQUENCE projecttracking.sprint_issue_comments_id_seq;

CREATE TABLE projecttracking.sprint_issue_comment_contents (
	id text NOT NULL,
	sprint_issue_comment_id text NOT NULL,
	sprint_issue_comment_content_type_name text NOT NULL,
	"content" text NULL,
	created_at timestamptz NOT NULL,
	updated_at timestamptz NOT NULL,
	deleted_at timestamptz NULL,
	CONSTRAINT sprint_issue_comment_contents_pk PRIMARY KEY (id)
);


CREATE SEQUENCE projecttracking.sprint_issue_comment_contents_id_seq;
