using AutoVPT.Interfaces;
using System;
using System.IO;

namespace AutoVPT.Infrastructure
{
    /// <summary>
    /// File-based logger that writes log entries to a text file.
    /// Thread-safe implementation with automatic directory creation.
    /// </summary>
    public class FileLogger : ILogger
    {
        private readonly string _logFilePath;
        private readonly object _fileLock = new object();
        private readonly LogLevel _minimumLevel;

        public FileLogger(string logFilePath, LogLevel minimumLevel = LogLevel.Info)
        {
            if (string.IsNullOrWhiteSpace(logFilePath))
                throw new ArgumentNullException(nameof(logFilePath));

            _logFilePath = logFilePath;
            _minimumLevel = minimumLevel;

            // Ensure directory exists
            var directory = Path.GetDirectoryName(_logFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public void LogInfo(string message, string context = null)
        {
            if (_minimumLevel <= LogLevel.Info)
            {
                WriteLog(LogLevel.Info, message, context);
            }
        }

        public void LogWarning(string message, string context = null)
        {
            if (_minimumLevel <= LogLevel.Warning)
            {
                WriteLog(LogLevel.Warning, message, context);
            }
        }

        public void LogError(string message, Exception ex = null, string context = null)
        {
            if (_minimumLevel <= LogLevel.Error)
            {
                var fullMessage = ex != null
                    ? $"{message}\nException: {ex.GetType().Name}: {ex.Message}\nStackTrace: {ex.StackTrace}"
                    : message;
                WriteLog(LogLevel.Error, fullMessage, context);
            }
        }

        public void LogDebug(string message, string context = null)
        {
            if (_minimumLevel <= LogLevel.Debug)
            {
                WriteLog(LogLevel.Debug, message, context);
            }
        }

        private void WriteLog(LogLevel level, string message, string context)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var contextPart = !string.IsNullOrEmpty(context) ? $"[{context}] " : "";
            var logEntry = $"{timestamp} [{level}] {contextPart}{message}\n";

            lock (_fileLock)
            {
                try
                {
                    File.AppendAllText(_logFilePath, logEntry);
                }
                catch
                {
                    // Silently fail - logging should not break application
                }
            }
        }
    }
}
