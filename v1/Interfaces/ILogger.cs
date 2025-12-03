using System;

namespace AutoVPT.Interfaces
{
    /// <summary>
    /// Logging abstraction to decouple business logic from UI.
    /// Supports multiple logging targets (file, UI, debug output).
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log informational message
        /// </summary>
        /// <param name="message">Message to log</param>
        /// <param name="context">Optional context (e.g., character ID, feature name)</param>
        void LogInfo(string message, string context = null);

        /// <summary>
        /// Log warning message
        /// </summary>
        void LogWarning(string message, string context = null);

        /// <summary>
        /// Log error message
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="ex">Optional exception</param>
        /// <param name="context">Optional context</param>
        void LogError(string message, Exception ex = null, string context = null);

        /// <summary>
        /// Log debug message (only in debug builds or when debug logging is enabled)
        /// </summary>
        void LogDebug(string message, string context = null);
    }

    /// <summary>
    /// Log levels for filtering
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }
}
