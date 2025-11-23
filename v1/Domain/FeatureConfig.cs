using System;
using System.Collections.Generic;

namespace AutoVPT.Domain
{
    /// <summary>
    /// Configuration for a single feature.
    /// Contains enable/disable flag and feature-specific settings.
    /// </summary>
    [Serializable]
    public class FeatureConfig
    {
        /// <summary>
        /// Feature type
        /// </summary>
        public FeatureType Type { get; set; }

        /// <summary>
        /// Whether this feature is enabled
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Feature-specific parameters (e.g., "Loai" for DoiNangNo, "Cap" for CheMatBao)
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }

        public FeatureConfig()
        {
            Parameters = new Dictionary<string, string>();
        }

        public FeatureConfig(FeatureType type, bool enabled = false)
        {
            Type = type;
            Enabled = enabled;
            Parameters = new Dictionary<string, string>();
        }

        /// <summary>
        /// Get parameter value with default
        /// </summary>
        public string GetParameter(string key, string defaultValue = "")
        {
            return Parameters.ContainsKey(key) ? Parameters[key] : defaultValue;
        }

        /// <summary>
        /// Set parameter value
        /// </summary>
        public void SetParameter(string key, string value)
        {
            Parameters[key] = value;
        }

        /// <summary>
        /// Check if parameter exists
        /// </summary>
        public bool HasParameter(string key)
        {
            return Parameters.ContainsKey(key);
        }
    }

    /// <summary>
    /// Complete feature configuration for a character.
    /// Maps feature types to their configurations.
    /// </summary>
    [Serializable]
    public class CharacterFeatureConfig
    {
        /// <summary>
        /// Character ID this config belongs to
        /// </summary>
        public string CharacterId { get; set; }

        /// <summary>
        /// Feature configurations mapped by type
        /// </summary>
        public Dictionary<FeatureType, FeatureConfig> Features { get; set; }

        public CharacterFeatureConfig()
        {
            Features = new Dictionary<FeatureType, FeatureConfig>();
            InitializeDefaults();
        }

        public CharacterFeatureConfig(string characterId)
        {
            CharacterId = characterId;
            Features = new Dictionary<FeatureType, FeatureConfig>();
            InitializeDefaults();
        }

        /// <summary>
        /// Initialize all features with default disabled state
        /// </summary>
        private void InitializeDefaults()
        {
            foreach (FeatureType featureType in Enum.GetValues(typeof(FeatureType)))
            {
                if (!Features.ContainsKey(featureType))
                {
                    Features[featureType] = new FeatureConfig(featureType, enabled: false);
                }
            }
        }

        /// <summary>
        /// Check if feature is enabled
        /// </summary>
        public bool IsEnabled(FeatureType feature)
        {
            return Features.ContainsKey(feature) && Features[feature].Enabled;
        }

        /// <summary>
        /// Enable a feature
        /// </summary>
        public void Enable(FeatureType feature, Dictionary<string, string> parameters = null)
        {
            if (!Features.ContainsKey(feature))
            {
                Features[feature] = new FeatureConfig(feature, enabled: true);
            }
            else
            {
                Features[feature].Enabled = true;
            }

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    Features[feature].SetParameter(param.Key, param.Value);
                }
            }
        }

        /// <summary>
        /// Disable a feature
        /// </summary>
        public void Disable(FeatureType feature)
        {
            if (Features.ContainsKey(feature))
            {
                Features[feature].Enabled = false;
            }
        }

        /// <summary>
        /// Get feature configuration
        /// </summary>
        public FeatureConfig GetConfig(FeatureType feature)
        {
            return Features.ContainsKey(feature) ? Features[feature] : new FeatureConfig(feature);
        }
    }
}
