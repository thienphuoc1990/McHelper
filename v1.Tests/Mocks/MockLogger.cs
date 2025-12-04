using AutoVPT.Interfaces;
using System;
using System.Collections.Generic;

namespace AutoVPT.Tests.Mocks
{
    /// <summary>
    /// Mock implementation of ILogger for testing.
    /// Records all log messages for verification.
    /// </summary>
    public class MockLogger : ILogger
    {
        private readonly List<LogEntry> _entries = new List<LogEntry>();

        /// <summary>
        /// Gets all recorded log entries
        /// </summary>
        public IReadOnlyList<LogEntry> Entries => _entries;

        /// <summary>
        /// Clear all recorded entries
        /// </summary>
        public void Reset()
        {
            _entries.Clear();
        }

        /// <summary>
        /// Check if a message was logged at a specific level
        /// </summary>
        public bool HasMessage(LogLevel level, string messageContains)
        {
            foreach (var entry in _entries)
            {
                if (entry.Level == level && entry.Message.Contains(messageContains))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get all messages at a specific level
        /// </summary>
        public List<string> GetMessages(LogLevel level)
        {
            var messages = new List<string>();
            foreach (var entry in _entries)
            {
                if (entry.Level == level)
                    messages.Add(entry.Message);
            }
            return messages;
        }

        /// <summary>
        /// Get count of entries at a specific level
        /// </summary>
        public int CountEntries(LogLevel level)
        {
            int count = 0;
            foreach (var entry in _entries)
            {
                if (entry.Level == level)
                    count++;
            }
            return count;
        }

        // ILogger implementation

        public void LogInfo(string message, string characterId = null)
        {
            _entries.Add(new LogEntry(LogLevel.Info, message, characterId));
        }

        public void LogDebug(string message, string characterId = null)
        {
            _entries.Add(new LogEntry(LogLevel.Debug, message, characterId));
        }

        public void LogWarning(string message, string characterId = null)
        {
            _entries.Add(new LogEntry(LogLevel.Warning, message, characterId));
        }

        public void LogError(string message, Exception ex = null, string characterId = null)
        {
            _entries.Add(new LogEntry(LogLevel.Error, message, characterId, ex));
        }
    }

    /// <summary>
    /// Log levels
    /// </summary>
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// Recorded log entry
    /// </summary>
    public class LogEntry
    {
        public LogLevel Level { get; }
        public string Message { get; }
        public string CharacterId { get; }
        public Exception Exception { get; }
        public DateTime Timestamp { get; }

        public LogEntry(LogLevel level, string message, string characterId = null, Exception exception = null)
        {
            Level = level;
            Message = message;
            CharacterId = characterId;
            Exception = exception;
            Timestamp = DateTime.Now;
        }

        public override string ToString()
        {
            var id = string.IsNullOrEmpty(CharacterId) ? "" : $"[{CharacterId}] ";
            return $"{Timestamp:HH:mm:ss} [{Level}] {id}{Message}";
        }
    }
}

