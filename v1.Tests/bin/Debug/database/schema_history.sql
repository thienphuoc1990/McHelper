-- Historical Tracking Schema Extension
-- Tracks completion of tasks over time for reporting and analytics

-- Task completion history table
CREATE TABLE IF NOT EXISTS TaskHistory (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    CharacterID TEXT NOT NULL,
    TaskName TEXT NOT NULL,
    CompletedDate TEXT NOT NULL,
    CompletedTime TEXT DEFAULT CURRENT_TIMESTAMP,

    -- Foreign key to Characters table
    FOREIGN KEY (CharacterID) REFERENCES Characters(ID) ON DELETE CASCADE
);

-- Index for fast lookups
CREATE INDEX IF NOT EXISTS idx_taskhistory_character ON TaskHistory(CharacterID);
CREATE INDEX IF NOT EXISTS idx_taskhistory_date ON TaskHistory(CompletedDate);
CREATE INDEX IF NOT EXISTS idx_taskhistory_task ON TaskHistory(TaskName);
CREATE INDEX IF NOT EXISTS idx_taskhistory_chardate ON TaskHistory(CharacterID, CompletedDate);

-- Daily summary table (aggregated view for faster reporting)
CREATE TABLE IF NOT EXISTS DailySummary (
    ID INTEGER PRIMARY KEY AUTOINCREMENT,
    Date TEXT NOT NULL UNIQUE,
    TotalCharacters INTEGER DEFAULT 0,
    CompletedCharacters INTEGER DEFAULT 0,
    TotalTasks INTEGER DEFAULT 0,
    CompletedTasks INTEGER DEFAULT 0,
    CompletionPercentage INTEGER DEFAULT 0,
    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT DEFAULT CURRENT_TIMESTAMP
);

-- Index for daily summary
CREATE INDEX IF NOT EXISTS idx_dailysummary_date ON DailySummary(Date);

-- Trigger to update UpdatedAt on DailySummary
CREATE TRIGGER IF NOT EXISTS update_dailysummary_timestamp
AFTER UPDATE ON DailySummary
BEGIN
    UPDATE DailySummary SET UpdatedAt = CURRENT_TIMESTAMP WHERE ID = NEW.ID;
END;

-- View: Task completion statistics by character
CREATE VIEW IF NOT EXISTS v_CharacterTaskStats AS
SELECT
    c.ID as CharacterID,
    c.GroupName,
    COUNT(DISTINCT th.TaskName) as UniqueTasksCompleted,
    COUNT(th.ID) as TotalCompletions,
    MAX(th.CompletedDate) as LastActiveDate
FROM Characters c
LEFT JOIN TaskHistory th ON c.ID = th.CharacterID
GROUP BY c.ID, c.GroupName;

-- View: Daily completion trends (last 30 days)
CREATE VIEW IF NOT EXISTS v_DailyTrends AS
SELECT
    Date,
    TotalCharacters,
    CompletedCharacters,
    TotalTasks,
    CompletedTasks,
    CompletionPercentage
FROM DailySummary
ORDER BY Date DESC
LIMIT 30;

-- View: Most popular tasks (by completion count)
CREATE VIEW IF NOT EXISTS v_PopularTasks AS
SELECT
    TaskName,
    COUNT(*) as CompletionCount,
    COUNT(DISTINCT CharacterID) as UniqueCharacters,
    MAX(CompletedDate) as LastCompleted
FROM TaskHistory
GROUP BY TaskName
ORDER BY CompletionCount DESC;

-- View: Character performance over time
CREATE VIEW IF NOT EXISTS v_CharacterPerformance AS
SELECT
    CharacterID,
    CompletedDate as Date,
    COUNT(*) as TasksCompletedThatDay
FROM TaskHistory
GROUP BY CharacterID, CompletedDate
ORDER BY CompletedDate DESC, TasksCompletedThatDay DESC;
