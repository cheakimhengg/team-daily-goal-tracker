-- Enable foreign key constraints (required for SQLite)
PRAGMA foreign_keys = ON;

-- Create TeamMembers table
CREATE TABLE IF NOT EXISTS TeamMembers (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    Name            TEXT NOT NULL CHECK(length(Name) > 0 AND length(Name) <= 100),
    CurrentMood     TEXT CHECK(CurrentMood IN ('Great', 'Good', 'Okay', 'Struggling', 'Overwhelmed')),
    MoodUpdatedAt   TEXT
);

-- Create Goals table
CREATE TABLE IF NOT EXISTS Goals (
    Id              INTEGER PRIMARY KEY AUTOINCREMENT,
    TeamMemberId    INTEGER NOT NULL,
    GoalText        TEXT NOT NULL CHECK(length(GoalText) > 0 AND length(GoalText) <= 500),
    CreatedAt       TEXT NOT NULL,
    IsCompleted     INTEGER NOT NULL DEFAULT 0 CHECK(IsCompleted IN (0, 1)),

    FOREIGN KEY (TeamMemberId) REFERENCES TeamMembers(Id) ON DELETE CASCADE
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_goals_team_member ON Goals(TeamMemberId);
CREATE INDEX IF NOT EXISTS idx_goals_created_at ON Goals(CreatedAt);

-- Seed initial team members (for development/testing)
INSERT OR IGNORE INTO TeamMembers (Id, Name, CurrentMood, MoodUpdatedAt) VALUES
(1, 'Alice Johnson', NULL, NULL),
(2, 'Bob Smith', NULL, NULL),
(3, 'Charlie Davis', NULL, NULL),
(4, 'Diana Chen', NULL, NULL);
