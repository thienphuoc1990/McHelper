using AutoVPT.Domain;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Services
{
    /// <summary>
    /// Result of automation execution
    /// </summary>
    public class AutomationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int FeaturesCompleted { get; set; }
        public int FeaturesFailed { get; set; }
        public TimeSpan Duration { get; set; }

        public static AutomationResult Successful(int completed, TimeSpan duration)
        {
            return new AutomationResult
            {
                Success = true,
                FeaturesCompleted = completed,
                Duration = duration,
                Message = $"Completed {completed} features in {duration.TotalSeconds:F1}s"
            };
        }

        public static AutomationResult Failed(string message)
        {
            return new AutomationResult
            {
                Success = false,
                Message = message
            };
        }
    }

    /// <summary>
    /// Main automation orchestration service.
    /// Coordinates feature execution for characters.
    /// </summary>
    public interface IAutomationService
    {
        /// <summary>
        /// Run all enabled daily tasks for a character
        /// </summary>
        Task<AutomationResult> RunDailyTasksAsync(string characterId, CancellationToken ct = default);

        /// <summary>
        /// Run a specific feature for a character
        /// </summary>
        Task<AutomationResult> RunFeatureAsync(string characterId, FeatureType feature, CancellationToken ct = default);

        /// <summary>
        /// Get current automation status for a character
        /// </summary>
        Task<CharacterRuntimeState> GetStatusAsync(string characterId);

        /// <summary>
        /// Stop automation for a character
        /// </summary>
        Task StopAsync(string characterId);

        /// <summary>
        /// Stop all running automations
        /// </summary>
        Task StopAllAsync();
    }
}
