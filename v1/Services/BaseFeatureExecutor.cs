using AutoVPT.Domain;
using AutoVPT.Interfaces;
using System;
using System.Threading.Tasks;

namespace AutoVPT.Services
{
    /// <summary>
    /// Base class for feature executors.
    /// Provides common functionality like logging, image recognition, and input simulation.
    /// </summary>
    public abstract class BaseFeatureExecutor : IFeatureExecutor
    {
        protected readonly IImageRecognition _imageRecognition;
        protected readonly IInputSimulator _inputSimulator;
        protected readonly ILogger _logger;

        public abstract FeatureType Type { get; }

        protected BaseFeatureExecutor(
            IImageRecognition imageRecognition,
            IInputSimulator inputSimulator,
            ILogger logger)
        {
            _imageRecognition = imageRecognition ?? throw new ArgumentNullException(nameof(imageRecognition));
            _inputSimulator = inputSimulator ?? throw new ArgumentNullException(nameof(inputSimulator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public abstract Task<FeatureResult> ExecuteAsync(ExecutionContext context);

        public virtual bool CanExecute(ExecutionContext context)
        {
            // Default implementation - check if feature is enabled
            return context.Character.FeatureConfig.IsEnabled(Type)
                && context.WindowHandle != IntPtr.Zero;
        }

        /// <summary>
        /// Log informational message (shown in UI if level >= Info)
        /// Use for important status updates
        /// </summary>
        protected void LogInfo(string message, ExecutionContext context)
        {
            _logger.LogInfo($"[{Type}] {message}", context.Character.Id);
        }

        /// <summary>
        /// Log debug message (only in log file, not in UI)
        /// Use for verbose details like retry attempts, image searches, etc.
        /// </summary>
        protected void LogDebug(string message, ExecutionContext context)
        {
            _logger.LogDebug($"[{Type}] {message}", context.Character.Id);
        }

        /// <summary>
        /// Log error with feature context (always shown in UI)
        /// </summary>
        protected void LogError(string message, Exception ex, ExecutionContext context)
        {
            _logger.LogError($"[{Type}] {message}", ex, context.Character.Id);
        }

        /// <summary>
        /// Log warning with feature context (always shown in UI)
        /// </summary>
        protected void LogWarning(string message, ExecutionContext context)
        {
            _logger.LogWarning($"[{Type}] {message}", context.Character.Id);
        }
    }
}
