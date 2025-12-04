using System;
using System.Diagnostics;
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
                if (!Directory.Exists(_logPath))
                {
                    Directory.CreateDirectory(_logPath);
                }
            }
            catch
            {
                // If we can't create the log directory, silently continue
                // Logging will fail but app will still run
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
            catch (Exception logEx)
            {
                // If logging fails, output to debug console instead
                Debug.WriteLine($"Logger.LogError failed: {logEx.Message} | Original error: [{characterId}] {context}: {ex.Message}");
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
            catch (Exception logEx)
            {
                // If logging fails, output to debug console instead
                Debug.WriteLine($"Logger.LogWarning failed: {logEx.Message} | Original warning: [{characterId}] {context}: {message}");
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
            catch (Exception logEx)
            {
                // If logging fails, output to debug console instead
                Debug.WriteLine($"Logger.LogInfo failed: {logEx.Message} | Original info: [{characterId}] {context}: {message}");
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
            catch (Exception ex)
            {
                // If cleaning fails, output to debug console
                Debug.WriteLine($"Logger.ClearOldLogs failed: {ex.Message}");
            }
        }
    }
}
