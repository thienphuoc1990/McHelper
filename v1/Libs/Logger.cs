using System;
using System.IO;
using System.Windows.Forms;

namespace AutoVPT.Libs
{
    public static class Logger
    {
        private static string _logPath = Path.Combine(Application.StartupPath, "logs");
        private static object _logLock = new object();

        static Logger()
        {
            try
            {
                Directory.CreateDirectory(_logPath);
            }
            catch
            {
                // If we can't create the log directory, we'll fail silently
            }
        }

        public static void LogError(string characterId, string context, Exception ex)
        {
            try
            {
                lock (_logLock)
                {
                    var logFile = Path.Combine(_logPath, $"{DateTime.Now:yyyy-MM-dd}.log");
                    var entry = $"[{DateTime.Now:HH:mm:ss}] [ERROR] [{characterId}] {context}: {ex.Message}\n{ex.StackTrace}\n";
                    File.AppendAllText(logFile, entry + Environment.NewLine);
                }
            }
            catch
            {
                // If logging fails, don't crash the application
            }
        }

        public static void LogWarning(string characterId, string context, string message)
        {
            try
            {
                lock (_logLock)
                {
                    var logFile = Path.Combine(_logPath, $"{DateTime.Now:yyyy-MM-dd}.log");
                    var entry = $"[{DateTime.Now:HH:mm:ss}] [WARNING] [{characterId}] {context}: {message}";
                    File.AppendAllText(logFile, entry + Environment.NewLine);
                }
            }
            catch
            {
                // If logging fails, don't crash the application
            }
        }

        public static void LogInfo(string characterId, string context, string message)
        {
            try
            {
                lock (_logLock)
                {
                    var logFile = Path.Combine(_logPath, $"{DateTime.Now:yyyy-MM-dd}.log");
                    var entry = $"[{DateTime.Now:HH:mm:ss}] [INFO] [{characterId}] {context}: {message}";
                    File.AppendAllText(logFile, entry + Environment.NewLine);
                }
            }
            catch
            {
                // If logging fails, don't crash the application
            }
        }

        public static void ClearOldLogs(int daysToKeep = 7)
        {
            try
            {
                if (!Directory.Exists(_logPath))
                    return;

                var cutoffDate = DateTime.Now.AddDays(-daysToKeep);
                foreach (var file in Directory.GetFiles(_logPath, "*.log"))
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                    }
                }
            }
            catch
            {
                // If cleaning fails, don't crash the application
            }
        }
    }
}
