using AutoVPT.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Services
{
    /// <summary>
    /// Result of feature execution
    /// </summary>
    public class FeatureResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Exception Error { get; set; }

        public static FeatureResult Successful(string message = "")
        {
            return new FeatureResult { Success = true, Message = message };
        }

        public static FeatureResult Failed(string message, Exception ex = null)
        {
            return new FeatureResult { Success = false, Message = message, Error = ex };
        }
    }

    /// <summary>
    /// Execution context passed to feature executors
    /// </summary>
    public class ExecutionContext
    {
        /// <summary>
        /// Character aggregate being automated
        /// </summary>
        public CharacterAggregate Character { get; set; }

        /// <summary>
        /// Window handle for automation
        /// </summary>
        public IntPtr WindowHandle { get; set; }

        /// <summary>
        /// Feature configuration
        /// </summary>
        public FeatureConfig Config { get; set; }

        /// <summary>
        /// Cancellation token
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// Status text box for logging (optional, for legacy compatibility)
        /// </summary>
        public System.Windows.Forms.TextBox StatusTextBox { get; set; }
    }

    /// <summary>
    /// Interface for feature-specific execution logic.
    /// Implements Strategy pattern for different automation features.
    /// </summary>
    public interface IFeatureExecutor
    {
        /// <summary>
        /// The feature type this executor handles
        /// </summary>
        FeatureType Type { get; }

        /// <summary>
        /// Execute the feature
        /// </summary>
        Task<FeatureResult> ExecuteAsync(ExecutionContext context);

        /// <summary>
        /// Check if this feature can be executed in the current context
        /// </summary>
        bool CanExecute(ExecutionContext context);
    }
}
