using AutoVPT.Services.Orchestrator.Models;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Services.Orchestrator
{
    /// <summary>
    /// Main orchestrator service that coordinates feature execution across all characters
    /// </summary>
    public interface IOrchestratorService
    {
        /// <summary>
        /// Start orchestration for all active characters
        /// </summary>
        Task StartOrchestrationAsync(CancellationToken ct = default);

        /// <summary>
        /// Stop orchestration and all running characters
        /// </summary>
        Task StopOrchestrationAsync();

        /// <summary>
        /// Get next action for a character
        /// </summary>
        Task<NextAction> GetNextActionAsync(string characterId);

        /// <summary>
        /// Get overall status
        /// </summary>
        Task<OrchestrationStatus> GetStatusAsync();

        /// <summary>
        /// Get character-specific status
        /// </summary>
        Task<CharacterOrchestrationStatus> GetCharacterStatusAsync(string characterId);

        /// <summary>
        /// Register character for orchestration
        /// </summary>
        void RegisterCharacter(string characterId);

        /// <summary>
        /// Unregister character
        /// </summary>
        void UnregisterCharacter(string characterId);

        /// <summary>
        /// Check if orchestration is running
        /// </summary>
        bool IsRunning { get; }
    }
}

