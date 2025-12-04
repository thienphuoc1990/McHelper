using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Services
{
    /// <summary>
    /// Base class for feature executors.
    /// Provides common functionality like logging, image recognition, input simulation,
    /// and cancellable async operations.
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

        #region Cancellable Delay Helpers

        /// <summary>
        /// Delay with cancellation support. Use this instead of Thread.Sleep().
        /// Responds immediately to cancellation requests.
        /// </summary>
        /// <param name="milliseconds">Delay duration in milliseconds</param>
        /// <param name="context">Execution context with cancellation token</param>
        /// <returns>True if delay completed, false if cancelled</returns>
        protected async Task<bool> DelayAsync(int milliseconds, ExecutionContext context)
        {
            try
            {
                // Also check global stop flag
                if (Helper.IsStoppingAll())
                {
                    return false;
                }

                await Task.Delay(milliseconds, context.CancellationToken);
                return true;
            }
            catch (OperationCanceledException)
            {
                return false;
            }
        }

        /// <summary>
        /// Delay with cancellation support using predefined time constants.
        /// </summary>
        protected Task<bool> DelayShortAsync(ExecutionContext context) => DelayAsync(Constant.TimeShort, context);
        protected Task<bool> DelayMediumAsync(ExecutionContext context) => DelayAsync(Constant.TimeMedium, context);
        protected Task<bool> DelayLongAsync(ExecutionContext context) => DelayAsync(Constant.TimeLong, context);

        /// <summary>
        /// Check if execution should continue (not cancelled and not stopping all).
        /// Call this frequently in loops.
        /// </summary>
        /// <param name="context">Execution context</param>
        /// <returns>True if should continue, false if should stop</returns>
        protected bool ShouldContinue(ExecutionContext context)
        {
            return !context.CancellationToken.IsCancellationRequested 
                && !Helper.IsStoppingAll();
        }

        /// <summary>
        /// Throw if cancellation has been requested.
        /// Use at the start of long operations.
        /// </summary>
        protected void ThrowIfCancelled(ExecutionContext context)
        {
            if (Helper.IsStoppingAll())
            {
                throw new OperationCanceledException("Stop All requested");
            }
            context.CancellationToken.ThrowIfCancellationRequested();
        }

        #endregion

        #region Wait Helpers

        /// <summary>
        /// Wait for a condition to become true with timeout and cancellation support.
        /// </summary>
        /// <param name="condition">Function that returns true when condition is met</param>
        /// <param name="context">Execution context</param>
        /// <param name="timeoutMs">Maximum time to wait (default: 30 seconds)</param>
        /// <param name="checkIntervalMs">Interval between checks (default: 500ms)</param>
        /// <param name="operationName">Operation name for logging</param>
        /// <returns>True if condition was met, false if timeout or cancelled</returns>
        protected async Task<bool> WaitForConditionAsync(
            Func<bool> condition,
            ExecutionContext context,
            int timeoutMs = 30000,
            int checkIntervalMs = 500,
            string operationName = "operation")
        {
            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromMilliseconds(timeoutMs);

            while (DateTime.Now - startTime < timeout)
            {
                // Check for cancellation
                if (!ShouldContinue(context))
                {
                    LogDebug($"{operationName}: Cancelled", context);
                    return false;
                }

                // Check condition
                if (condition())
                {
                    return true;
                }

                // Wait before next check
                if (!await DelayAsync(checkIntervalMs, context))
                {
                    return false; // Cancelled during delay
                }
            }

            LogDebug($"{operationName}: Timeout after {timeoutMs}ms", context);
            return false;
        }

        /// <summary>
        /// Wait for an async condition to become true with timeout and cancellation support.
        /// </summary>
        protected async Task<bool> WaitForConditionAsync(
            Func<Task<bool>> asyncCondition,
            ExecutionContext context,
            int timeoutMs = 30000,
            int checkIntervalMs = 500,
            string operationName = "operation")
        {
            var startTime = DateTime.Now;
            var timeout = TimeSpan.FromMilliseconds(timeoutMs);

            while (DateTime.Now - startTime < timeout)
            {
                // Check for cancellation
                if (!ShouldContinue(context))
                {
                    LogDebug($"{operationName}: Cancelled", context);
                    return false;
                }

                // Check condition
                if (await asyncCondition())
                {
                    return true;
                }

                // Wait before next check
                if (!await DelayAsync(checkIntervalMs, context))
                {
                    return false; // Cancelled during delay
                }
            }

            LogDebug($"{operationName}: Timeout after {timeoutMs}ms", context);
            return false;
        }

        /// <summary>
        /// Retry an operation until it succeeds or max retries reached.
        /// </summary>
        /// <param name="operation">Operation that returns true on success</param>
        /// <param name="context">Execution context</param>
        /// <param name="maxRetries">Maximum retry attempts</param>
        /// <param name="delayBetweenRetriesMs">Delay between retries</param>
        /// <param name="operationName">Operation name for logging</param>
        /// <returns>True if operation succeeded, false if failed or cancelled</returns>
        protected async Task<bool> RetryAsync(
            Func<bool> operation,
            ExecutionContext context,
            int maxRetries = 3,
            int delayBetweenRetriesMs = 1000,
            string operationName = "operation")
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                // Check for cancellation
                if (!ShouldContinue(context))
                {
                    LogDebug($"{operationName}: Cancelled on attempt {attempt}", context);
                    return false;
                }

                // Try the operation
                if (operation())
                {
                    return true;
                }

                // Log retry
                if (attempt < maxRetries)
                {
                    LogDebug($"{operationName}: Retry {attempt}/{maxRetries}", context);
                    if (!await DelayAsync(delayBetweenRetriesMs, context))
                    {
                        return false; // Cancelled during delay
                    }
                }
            }

            LogDebug($"{operationName}: Failed after {maxRetries} attempts", context);
            return false;
        }

        /// <summary>
        /// Retry an async operation until it succeeds or max retries reached.
        /// </summary>
        protected async Task<bool> RetryAsync(
            Func<Task<bool>> asyncOperation,
            ExecutionContext context,
            int maxRetries = 3,
            int delayBetweenRetriesMs = 1000,
            string operationName = "operation")
        {
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                // Check for cancellation
                if (!ShouldContinue(context))
                {
                    LogDebug($"{operationName}: Cancelled on attempt {attempt}", context);
                    return false;
                }

                // Try the operation
                if (await asyncOperation())
                {
                    return true;
                }

                // Log retry
                if (attempt < maxRetries)
                {
                    LogDebug($"{operationName}: Retry {attempt}/{maxRetries}", context);
                    if (!await DelayAsync(delayBetweenRetriesMs, context))
                    {
                        return false; // Cancelled during delay
                    }
                }
            }

            LogDebug($"{operationName}: Failed after {maxRetries} attempts", context);
            return false;
        }

        #endregion

        #region Logging Helpers

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

        #endregion
    }
}
