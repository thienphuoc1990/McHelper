using AutoVPT.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Windows.Forms;

namespace AutoVPT.Database
{
    /// <summary>
    /// Tracks historical task completion data for analytics and reporting
    /// </summary>
    public static class HistoryTracker
    {
        private static string _connectionString;
        private static readonly object _dbLock = new object();
        private static bool _isInitialized = false;

        /// <summary>
        /// Initialize history tracking (creates tables if needed)
        /// Call this after DatabaseHelper.Initialize()
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) return;

            var dbPath = Path.Combine(Application.StartupPath, "database", "mchelper.db");
            _connectionString = $"Data Source={dbPath};Version=3;";

            CreateHistoryTables();
            _isInitialized = true;
        }

        private static void CreateHistoryTables()
        {
            var schemaPath = Path.Combine(Application.StartupPath, "Database", "schema_history.sql");

            if (!File.Exists(schemaPath))
            {
                // Schema file not found - skip history tracking
                Console.WriteLine("Warning: schema_history.sql not found. History tracking disabled.");
                return;
            }

            var schema = File.ReadAllText(schemaPath);

            lock (_dbLock)
            {
                using (var conn = new SQLiteConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SQLiteCommand(schema, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        /// <summary>
        /// Record task completion in history
        /// Call this whenever a task status changes from 0 to 1
        /// </summary>
        public static void RecordTaskCompletion(string characterId, string taskName)
        {
            if (!_isInitialized) return;

            try
            {
                lock (_dbLock)
                {
                    using (var conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();

                        var sql = @"
                            INSERT INTO TaskHistory (CharacterID, TaskName, CompletedDate, CompletedTime)
                            VALUES (@CharacterID, @TaskName, @CompletedDate, @CompletedTime)";

                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@CharacterID", characterId);
                            cmd.Parameters.AddWithValue("@TaskName", taskName);
                            cmd.Parameters.AddWithValue("@CompletedDate", DateTime.Now.ToString("yyyy-MM-dd"));
                            cmd.Parameters.AddWithValue("@CompletedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recording task completion: {ex.Message}");
            }
        }

        /// <summary>
        /// Record multiple task completions at once
        /// </summary>
        public static void RecordTaskCompletions(string characterId, List<string> taskNames)
        {
            if (!_isInitialized || taskNames == null || taskNames.Count == 0) return;

            try
            {
                lock (_dbLock)
                {
                    using (var conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();

                        using (var transaction = conn.BeginTransaction())
                        {
                            var sql = @"
                                INSERT INTO TaskHistory (CharacterID, TaskName, CompletedDate, CompletedTime)
                                VALUES (@CharacterID, @TaskName, @CompletedDate, @CompletedTime)";

                            foreach (var taskName in taskNames)
                            {
                                using (var cmd = new SQLiteCommand(sql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@CharacterID", characterId);
                                    cmd.Parameters.AddWithValue("@TaskName", taskName);
                                    cmd.Parameters.AddWithValue("@CompletedDate", DateTime.Now.ToString("yyyy-MM-dd"));
                                    cmd.Parameters.AddWithValue("@CompletedTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                                    cmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recording task completions: {ex.Message}");
            }
        }

        /// <summary>
        /// Update daily summary with current statistics
        /// Call this at end of day or on demand
        /// </summary>
        public static void UpdateDailySummary()
        {
            if (!_isInitialized) return;

            try
            {
                var allCharacters = DatabaseHelper.LoadAllCharacters();
                var incomplete = DatabaseHelper.GetCharactersWithIncompleteTasks();

                int totalCharacters = allCharacters.Count;
                int completedCharacters = totalCharacters - incomplete.Count;

                int totalTasks = 0;
                int completedTasks = 0;

                foreach (var c in allCharacters)
                {
                    if (c.VipPromotion == 1) { totalTasks++; if (c.StatusVipPromotion == 1) completedTasks++; }
                    if (c.DoiNangNo == 1) { totalTasks++; if (c.StatusDoiNangNo == 1) completedTasks++; }
                    if (c.TriAn == 1) { totalTasks++; if (c.StatusTriAn == 1) completedTasks++; }
                    if (c.LatTheBai == 1) { totalTasks++; if (c.StatusLatTheBai == 1) completedTasks++; }
                    if (c.RutBo == 1) { totalTasks++; if (c.StatusRutBo == 1) completedTasks++; }
                    if (c.TruMa == 1) { totalTasks++; if (c.StatusTruMa == 1) completedTasks++; }
                    if (c.AutoPhuBan == 1) { totalTasks++; if (c.StatusAutoPhuBan == 1) completedTasks++; }
                    if (c.CheMatBao == 1) { totalTasks++; if (c.StatusCheMatBao == 1) completedTasks++; }
                    if (c.UocNguyen == 1) { totalTasks++; if (c.StatusUocNguyen == 1) completedTasks++; }
                    if (c.NhanThuongHLVT == 1) { totalTasks++; if (c.StatusNhanThuongHLVT == 1) completedTasks++; }
                }

                int completionPercentage = totalTasks > 0 ? (completedTasks * 100 / totalTasks) : 0;
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

                lock (_dbLock)
                {
                    using (var conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();

                        var sql = @"
                            INSERT OR REPLACE INTO DailySummary
                                (Date, TotalCharacters, CompletedCharacters, TotalTasks, CompletedTasks, CompletionPercentage)
                            VALUES
                                (@Date, @TotalCharacters, @CompletedCharacters, @TotalTasks, @CompletedTasks, @CompletionPercentage)";

                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Date", currentDate);
                            cmd.Parameters.AddWithValue("@TotalCharacters", totalCharacters);
                            cmd.Parameters.AddWithValue("@CompletedCharacters", completedCharacters);
                            cmd.Parameters.AddWithValue("@TotalTasks", totalTasks);
                            cmd.Parameters.AddWithValue("@CompletedTasks", completedTasks);
                            cmd.Parameters.AddWithValue("@CompletionPercentage", completionPercentage);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating daily summary: {ex.Message}");
            }
        }

        /// <summary>
        /// Get task completion history for a character
        /// </summary>
        public static DataTable GetCharacterHistory(string characterId, int days = 30)
        {
            if (!_isInitialized) return new DataTable();

            var dt = new DataTable();
            dt.Columns.Add("Date");
            dt.Columns.Add("Task");
            dt.Columns.Add("Time");

            try
            {
                lock (_dbLock)
                {
                    using (var conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();

                        var sql = @"
                            SELECT TaskName, CompletedDate, CompletedTime
                            FROM TaskHistory
                            WHERE CharacterID = @CharacterID
                            AND CompletedDate >= date('now', '-' || @Days || ' days')
                            ORDER BY CompletedTime DESC";

                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@CharacterID", characterId);
                            cmd.Parameters.AddWithValue("@Days", days);

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    dt.Rows.Add(
                                        reader["CompletedDate"].ToString(),
                                        reader["TaskName"].ToString(),
                                        reader["CompletedTime"].ToString()
                                    );
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting character history: {ex.Message}");
            }

            return dt;
        }

        /// <summary>
        /// Get daily trends (last N days)
        /// </summary>
        public static DataTable GetDailyTrends(int days = 30)
        {
            if (!_isInitialized) return new DataTable();

            var dt = new DataTable();

            try
            {
                lock (_dbLock)
                {
                    using (var conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();

                        var sql = @"
                            SELECT Date, TotalCharacters, CompletedCharacters, TotalTasks, CompletedTasks, CompletionPercentage
                            FROM DailySummary
                            WHERE Date >= date('now', '-' || @Days || ' days')
                            ORDER BY Date DESC";

                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Days", days);

                            using (var adapter = new SQLiteDataAdapter(cmd))
                            {
                                adapter.Fill(dt);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting daily trends: {ex.Message}");
            }

            return dt;
        }

        /// <summary>
        /// Get most popular tasks
        /// </summary>
        public static DataTable GetPopularTasks()
        {
            if (!_isInitialized) return new DataTable();

            var dt = new DataTable();

            try
            {
                lock (_dbLock)
                {
                    using (var conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();

                        var sql = @"SELECT * FROM v_PopularTasks";

                        using (var cmd = new SQLiteCommand(sql, conn))
                        using (var adapter = new SQLiteDataAdapter(cmd))
                        {
                            adapter.Fill(dt);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting popular tasks: {ex.Message}");
            }

            return dt;
        }

        /// <summary>
        /// Get character performance stats
        /// </summary>
        public static DataTable GetCharacterPerformance(string characterId = null)
        {
            if (!_isInitialized) return new DataTable();

            var dt = new DataTable();

            try
            {
                lock (_dbLock)
                {
                    using (var conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();

                        string sql;
                        if (!string.IsNullOrEmpty(characterId))
                        {
                            sql = @"
                                SELECT * FROM v_CharacterPerformance
                                WHERE CharacterID = @CharacterID
                                ORDER BY Date DESC
                                LIMIT 30";
                        }
                        else
                        {
                            sql = @"SELECT * FROM v_CharacterTaskStats ORDER BY TotalCompletions DESC";
                        }

                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            if (!string.IsNullOrEmpty(characterId))
                            {
                                cmd.Parameters.AddWithValue("@CharacterID", characterId);
                            }

                            using (var adapter = new SQLiteDataAdapter(cmd))
                            {
                                adapter.Fill(dt);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting character performance: {ex.Message}");
            }

            return dt;
        }

        /// <summary>
        /// Clear old history data (cleanup)
        /// </summary>
        public static void CleanupOldHistory(int daysToKeep = 90)
        {
            if (!_isInitialized) return;

            try
            {
                lock (_dbLock)
                {
                    using (var conn = new SQLiteConnection(_connectionString))
                    {
                        conn.Open();

                        // Delete old task history
                        var sql = @"
                            DELETE FROM TaskHistory
                            WHERE CompletedDate < date('now', '-' || @Days || ' days')";

                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@Days", daysToKeep);
                            int deleted = cmd.ExecuteNonQuery();
                            Console.WriteLine($"Cleaned up {deleted} old history records");
                        }

                        // Keep daily summaries longer (180 days)
                        sql = @"
                            DELETE FROM DailySummary
                            WHERE Date < date('now', '-180 days')";

                        using (var cmd = new SQLiteCommand(sql, conn))
                        {
                            int deleted = cmd.ExecuteNonQuery();
                            Console.WriteLine($"Cleaned up {deleted} old daily summaries");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cleaning up history: {ex.Message}");
            }
        }
    }
}
