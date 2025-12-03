using AutoVPT.Services.Orchestrator.Models;
using System;

namespace AutoVPT.Services.Orchestrator
{
    /// <summary>
    /// Tracks execution status for all characters
    /// </summary>
    public interface IStatusTracker
    {
        /// <summary>
        /// Update character status
        /// </summary>
        void UpdateCharacterStatus(string characterId, CharacterOrchestrationStatus status);

        /// <summary>
        /// Get current status for character
        /// </summary>
        CharacterOrchestrationStatus GetCharacterStatus(string characterId);

        /// <summary>
        /// Get overall statistics
        /// </summary>
        OrchestrationStatistics GetStatistics();

        /// <summary>
        /// Get completion percentage for character
        /// </summary>
        double GetCompletionPercentage(string characterId);

        /// <summary>
        /// Get overall orchestration status
        /// </summary>
        OrchestrationStatus GetOrchestrationStatus();

        /// <summary>
        /// Clear all status (reset)
        /// </summary>
        void Clear();
    }
}

