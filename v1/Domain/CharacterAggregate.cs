using System;
using System.Collections.Generic;

namespace AutoVPT.Domain
{
    /// <summary>
    /// Character aggregate root that composes identity, configuration, and runtime state.
    /// This is the new recommended way to work with characters.
    /// Provides clean separation of concerns while maintaining backward compatibility.
    /// </summary>
    [Serializable]
    public class CharacterAggregate
    {
        /// <summary>
        /// Core character identity
        /// </summary>
        public CharacterIdentity Identity { get; set; }

        /// <summary>
        /// Feature configuration (what's enabled and settings)
        /// </summary>
        public CharacterFeatureConfig FeatureConfig { get; set; }

        /// <summary>
        /// Runtime state (current status, feature completion)
        /// </summary>
        public CharacterRuntimeState RuntimeState { get; set; }

        public CharacterAggregate()
        {
            Identity = new CharacterIdentity();
            FeatureConfig = new CharacterFeatureConfig();
            RuntimeState = new CharacterRuntimeState();
        }

        public CharacterAggregate(string id, string link)
        {
            Identity = new CharacterIdentity(id, link);
            FeatureConfig = new CharacterFeatureConfig(id);
            RuntimeState = new CharacterRuntimeState(id);
        }

        /// <summary>
        /// Character ID (convenience property)
        /// </summary>
        public string Id
        {
            get => Identity.Id;
            set => Identity.Id = value;
        }

        /// <summary>
        /// Check if a feature should run (enabled and not completed)
        /// </summary>
        public bool ShouldRunFeature(FeatureType feature)
        {
            return FeatureConfig.IsEnabled(feature)
                && !RuntimeState.IsCompleted(feature)
                && RuntimeState.IsRunning;
        }

        /// <summary>
        /// Execute feature lifecycle (check daily reset, mark status)
        /// </summary>
        public void BeforeFeatureExecution()
        {
            RuntimeState.CheckAndResetDaily();
        }

        /// <summary>
        /// Mark feature as completed after successful execution
        /// </summary>
        public void CompleteFeature(FeatureType feature)
        {
            RuntimeState.MarkCompleted(feature);
        }

        /// <summary>
        /// Mark feature as failed after unsuccessful execution
        /// </summary>
        public void FailFeature(FeatureType feature)
        {
            RuntimeState.MarkFailed(feature);
        }

        /// <summary>
        /// Get all enabled features
        /// </summary>
        public IEnumerable<FeatureType> GetEnabledFeatures()
        {
            foreach (var kvp in FeatureConfig.Features)
            {
                if (kvp.Value.Enabled)
                {
                    yield return kvp.Key;
                }
            }
        }

        /// <summary>
        /// Get all pending features (enabled but not completed)
        /// </summary>
        public IEnumerable<FeatureType> GetPendingFeatures()
        {
            foreach (var feature in GetEnabledFeatures())
            {
                if (!RuntimeState.IsCompleted(feature))
                {
                    yield return feature;
                }
            }
        }

        public override string ToString()
        {
            return Identity.ToString();
        }
    }
}
