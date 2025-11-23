using AutoVPT.Interfaces;
using System;
using System.Collections.Generic;

namespace AutoVPT.Infrastructure
{
    /// <summary>
    /// Composite logger that forwards log messages to multiple logger implementations.
    /// Implements Composite pattern for flexible logging configuration.
    /// </summary>
    public class CompositeLogger : ILogger
    {
        private readonly List<ILogger> _loggers = new List<ILogger>();
        private readonly object _lock = new object();

        public void AddLogger(ILogger logger)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            lock (_lock)
            {
                _loggers.Add(logger);
            }
        }

        public void RemoveLogger(ILogger logger)
        {
            lock (_lock)
            {
                _loggers.Remove(logger);
            }
        }

        public void LogInfo(string message, string context = null)
        {
            lock (_lock)
            {
                foreach (var logger in _loggers)
                {
                    try
                    {
                        logger.LogInfo(message, context);
                    }
                    catch
                    {
                        // Ignore individual logger failures to prevent cascade
                    }
                }
            }
        }

        public void LogWarning(string message, string context = null)
        {
            lock (_lock)
            {
                foreach (var logger in _loggers)
                {
                    try
                    {
                        logger.LogWarning(message, context);
                    }
                    catch
                    {
                        // Ignore individual logger failures
                    }
                }
            }
        }

        public void LogError(string message, Exception ex = null, string context = null)
        {
            lock (_lock)
            {
                foreach (var logger in _loggers)
                {
                    try
                    {
                        logger.LogError(message, ex, context);
                    }
                    catch
                    {
                        // Ignore individual logger failures
                    }
                }
            }
        }

        public void LogDebug(string message, string context = null)
        {
            lock (_lock)
            {
                foreach (var logger in _loggers)
                {
                    try
                    {
                        logger.LogDebug(message, context);
                    }
                    catch
                    {
                        // Ignore individual logger failures
                    }
                }
            }
        }
    }
}
