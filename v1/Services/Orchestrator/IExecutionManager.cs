using AutoVPT.Services.Orchestrator.Models;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Services.Orchestrator
{
    /// <summary>
    /// Manages execution of actions for characters
    /// </summary>
    public interface IExecutionManager
    {
        /// <summary>
        /// Execute next action for character
        /// </summary>
        Task<ExecutionResult> ExecuteActionAsync(string characterId, NextAction action, CancellationToken ct);

        /// <summary>
        /// Check if character can execute (not already running)
        /// </summary>
        bool CanExecute(string characterId);

        /// <summary>
        /// Get active execution count
        /// </summary>
        int GetActiveExecutionCount();

        /// <summary>
        /// Get maximum concurrent executions
        /// </summary>
        int MaxConcurrentExecutions { get; set; }

        /// <summary>
        /// Cancel execution for character
        /// </summary>
        Task CancelExecutionAsync(string characterId);
    }

    /// <summary>
    /// Result of an execution
    /// </summary>
    public class ExecutionResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public System.TimeSpan Duration { get; set; }

        public static ExecutionResult Successful(string message = null, System.TimeSpan? duration = null)
        {
            return new ExecutionResult
            {
                Success = true,
                Message = message ?? "Execution completed successfully",
                Duration = duration ?? System.TimeSpan.Zero
            };
        }

        public static ExecutionResult Failed(string message)
        {
            return new ExecutionResult
            {
                Success = false,
                Message = message,
                Duration = System.TimeSpan.Zero
            };
        }
    }
}

