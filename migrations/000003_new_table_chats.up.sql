
create schema chat;
CREATE TABLE chat.rooms (
    id text PRIMARY KEY,  -- GUID for the chat room ID
    name TEXT,                                      -- Optional name for the chat room
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP, -- Timestamp with timezone
    updated_at timestamptz NULL,
	deleted_at timestamptz NULL
);


CREATE TABLE chat.messages (
    id text PRIMARY KEY ,   -- GUID for the message ID
    room_id text REFERENCES chat.rooms(id),      -- Reference to the chat room
    user_id text NOT NULL,                           -- The user who sent the message
    message TEXT NOT NULL,                           -- Content of the message
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP, -- Timestamp with timezone
    updated_at timestamptz NULL,
	deleted_at timestamptz NULL
);

CREATE TABLE chat.participants (
    id text PRIMARY KEY ,   -- GUID for the participant record
    room_id text REFERENCES chat.rooms(id),      -- Reference to the chat room
    user_id text NOT NULL,                           -- The user who is participating
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP,  -- Timestamp with timezone
    updated_at timestamptz NULL,
	deleted_at timestamptz NULL
);
