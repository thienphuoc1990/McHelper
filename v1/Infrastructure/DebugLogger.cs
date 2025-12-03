using AutoVPT.Interfaces;
using System;
using System.Diagnostics;

namespace AutoVPT.Infrastructure
{
    /// <summary>
    /// Debug logger that writes to Debug output window.
    /// Useful for development and debugging scenarios.
    /// </summary>
    public class DebugLogger : ILogger
    {
        private readonly LogLevel _minimumLevel;

        public DebugLogger(LogLevel minimumLevel = LogLevel.Debug)
        {
            _minimumLevel = minimumLevel;
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
                    ? $"{message}\n{ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}"
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
            var logEntry = $"{timestamp} [{level}] {contextPart}{message}";

            Debug.WriteLine(logEntry);
        }
    }
}
