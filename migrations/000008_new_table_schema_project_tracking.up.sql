CREATE SCHEMA projecttracking;

CREATE TABLE projecttracking.projects (
	id text PRIMARY KEY ,
	workspace_id text REFERENCES auth.workspaces(id),
	created_at timestamptz DEFAULT CURRENT_TIMESTAMP,
	updated_at timestamptz DEFAULT CURRENT_TIMESTAMP,
	deleted_at timestamptz,
	"name" TEXT
);

CREATE TABLE projecttracking.project_participants (
	id text PRIMARY KEY ,
	project_id text REFERENCES projecttracking.projects(id),
	user_id text NOT NULL,
	created_at timestamptz DEFAULT CURRENT_TIMESTAMP,
	updated_at timestamptz DEFAULT CURRENT_TIMESTAMP,
	deleted_at timestamptz
);

CREATE TABLE projecttracking.sprints (
	id text PRIMARY KEY ,
	project_id text REFERENCES projecttracking.projects(id),
	name TEXT NOT NULL,
	start_date timestamptz,
	end_date timestamptz,
	created_at timestamptz DEFAULT CURRENT_TIMESTAMP,
	updated_at timestamptz DEFAULT CURRENT_TIMESTAMP,
	deleted_at timestamptz
);

CREATE TABLE projecttracking.sprint_issues (
	id text PRIMARY KEY ,
	sprint_id text REFERENCES projecttracking.sprints(id),
	title TEXT NOT NULL,
	description TEXT,
	status TEXT,
	priority TEXT,
	type TEXT,
	created_at timestamptz DEFAULT CURRENT_TIMESTAMP,
	updated_at timestamptz DEFAULT CURRENT_TIMESTAMP,
	deleted_at timestamptz
);

CREATE TABLE projecttracking.sprint_issue_comments (
	id text PRIMARY KEY ,
	sprint_issue_id text REFERENCES projecttracking.sprint_issues(id),
	user_id text NOT NULL,
	content TEXT NOT NULL,
	created_at timestamptz DEFAULT CURRENT_TIMESTAMP,
	updated_at timestamptz DEFAULT CURRENT_TIMESTAMP,
	deleted_at timestamptz
);

CREATE TABLE projecttracking.sprint_issue_assignees (
	id text PRIMARY KEY ,
	sprint_issue_id text REFERENCES projecttracking.sprint_issues(id),
	user_id text NOT NULL,
	assigned_at timestamptz DEFAULT CURRENT_TIMESTAMP,
    created_at timestamptz DEFAULT CURRENT_TIMESTAMP,
	updated_at timestamptz DEFAULT CURRENT_TIMESTAMP,
	deleted_at timestamptz
);
