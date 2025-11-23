using AutoVPT.Interfaces;
using System;
using System.Windows.Forms;

namespace AutoVPT.Infrastructure
{
    /// <summary>
    /// UI-based logger that writes log entries to a TextBox control.
    /// Thread-safe implementation with proper invoke handling.
    /// </summary>
    public class UiLogger : ILogger
    {
        private readonly TextBox _textBox;
        private readonly LogLevel _minimumLevel;
        private readonly int _maxLines;

        public UiLogger(TextBox textBox, LogLevel minimumLevel = LogLevel.Info, int maxLines = 1000)
        {
            _textBox = textBox ?? throw new ArgumentNullException(nameof(textBox));
            _minimumLevel = minimumLevel;
            _maxLines = maxLines;
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
                    ? $"{message} | {ex.GetType().Name}: {ex.Message}"
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
            if (_textBox.IsDisposed)
                return;

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            var levelTag = GetLevelTag(level);
            var contextPart = !string.IsNullOrEmpty(context) ? $"[{context}] " : "";
            var logEntry = $"[{timestamp}] {levelTag} {contextPart}{message}\r\n";

            if (_textBox.InvokeRequired)
            {
                _textBox.BeginInvoke(new Action(() => AppendToTextBox(logEntry)));
            }
            else
            {
                AppendToTextBox(logEntry);
            }
        }

        private void AppendToTextBox(string text)
        {
            try
            {
                _textBox.AppendText(text);

                // Limit number of lines to prevent memory issues
                var lines = _textBox.Lines.Length;
                if (lines > _maxLines)
                {
                    var linesToRemove = lines - _maxLines;
                    var index = 0;
                    for (int i = 0; i < linesToRemove; i++)
                    {
                        index = _textBox.Text.IndexOf("\r\n", index) + 2;
                    }
                    _textBox.Text = _textBox.Text.Substring(index);
                }

                // Auto-scroll to bottom
                _textBox.SelectionStart = _textBox.Text.Length;
                _textBox.ScrollToCaret();
            }
            catch
            {
                // Silently fail - logging should not break application
            }
        }

        private string GetLevelTag(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return "[DEBUG]";
                case LogLevel.Info:
                    return "[INFO]";
                case LogLevel.Warning:
                    return "[WARN]";
                case LogLevel.Error:
                    return "[ERROR]";
                default:
                    return "[LOG]";
            }
        }
    }
}
