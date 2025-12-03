using AutoVPT.Domain;
using AutoVPT.Services.Orchestrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoVPT.Services.Orchestrator
{
    /// <summary>
    /// Decision engine that determines the next action for a character
    /// </summary>
    public class DecisionEngine : IDecisionEngine
    {
        /// <summary>
        /// Determine next action for character
        /// </summary>
        public NextAction DetermineNextAction(CharacterAggregate character)
        {
            if (character == null)
            {
                return NextAction.Stop("", "Character is null");
            }

            // Check if character is running
            if (!character.RuntimeState.IsRunning)
            {
                return NextAction.Stop(character.Id, "Character is not running");
            }

            // Check daily reset
            character.BeforeFeatureExecution();

            // Get all enabled features
            var enabledFeatures = character.GetEnabledFeatures().ToList();
            
            if (enabledFeatures.Count == 0)
            {
                return NextAction.Skip(character.Id, "No features enabled");
            }

            // Filter out completed features
            var pendingFeatures = character.GetPendingFeatures().ToList();

            if (pendingFeatures.Count == 0)
            {
                return NextAction.Complete(character.Id);
            }

            // Get features that can execute (dependencies met)
            var executableFeatures = pendingFeatures
                .Where(f => CanExecuteFeature(character, f))
                .ToList();

            if (executableFeatures.Count == 0)
            {
                // Find which dependencies are missing
                var missingDeps = pendingFeatures
                    .SelectMany(f => GetDependencies(f)
                        .Where(dep => !character.RuntimeState.IsCompleted(dep)))
                    .FirstOrDefault();

                if (missingDeps != default(FeatureType))
                {
                    return NextAction.Wait(character.Id, $"Waiting for dependency: {missingDeps}");
                }

                return NextAction.Wait(character.Id, "Dependencies not met");
            }

            // Sort by priority (lower number = higher priority)
            var sortedFeatures = executableFeatures
                .Select(f => new { Feature = f, Priority = GetPriority(f) })
                .OrderBy(x => x.Priority)
                .ToList();

            var nextFeature = sortedFeatures.First();
            
            return NextAction.Execute(
                character.Id,
                nextFeature.Feature,
                nextFeature.Priority,
                $"Next: {nextFeature.Feature} (Priority {nextFeature.Priority})"
            );
        }

        /// <summary>
        /// Check if feature can execute (dependencies met)
        /// </summary>
        public bool CanExecuteFeature(CharacterAggregate character, FeatureType feature)
        {
            if (character == null)
                return false;

            // Check if feature is enabled
            if (!character.FeatureConfig.IsEnabled(feature))
                return false;

            // Check if already completed
            if (character.RuntimeState.IsCompleted(feature))
                return false;

            // Check dependencies
            var featureStatuses = character.RuntimeState.FeatureStatuses;
            return DependencyRules.CanExecute(feature, featureStatuses);
        }

        /// <summary>
        /// Get execution priority for feature
        /// </summary>
        public int GetPriority(FeatureType feature)
        {
            return PriorityRules.GetPriority(feature);
        }

        /// <summary>
        /// Get feature dependencies
        /// </summary>
        public IEnumerable<FeatureType> GetDependencies(FeatureType feature)
        {
            return DependencyRules.GetDependencies(feature);
        }
    }
}

