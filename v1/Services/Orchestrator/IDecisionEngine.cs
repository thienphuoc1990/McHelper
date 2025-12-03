using AutoVPT.Domain;
using AutoVPT.Services.Orchestrator.Models;
using System.Collections.Generic;

namespace AutoVPT.Services.Orchestrator
{
    /// <summary>
    /// Decision engine that determines the next action for a character
    /// </summary>
    public interface IDecisionEngine
    {
        /// <summary>
        /// Determine next action for character
        /// </summary>
        NextAction DetermineNextAction(CharacterAggregate character);

        /// <summary>
        /// Check if feature can execute (dependencies met)
        /// </summary>
        bool CanExecuteFeature(CharacterAggregate character, FeatureType feature);

        /// <summary>
        /// Get execution priority for feature
        /// </summary>
        int GetPriority(FeatureType feature);

        /// <summary>
        /// Get feature dependencies
        /// </summary>
        IEnumerable<FeatureType> GetDependencies(FeatureType feature);
    }
}

