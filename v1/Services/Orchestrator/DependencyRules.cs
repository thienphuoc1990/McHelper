using AutoVPT.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoVPT.Services.Orchestrator
{
    /// <summary>
    /// Defines feature dependencies - which features must complete before others can run
    /// </summary>
    public static class DependencyRules
    {
        private static readonly Dictionary<FeatureType, List<FeatureType>> _dependencies = new Dictionary<FeatureType, List<FeatureType>>
        {
            // Most features have no dependencies - they can run independently
            // Add specific dependencies here as needed
            
            // Example: AutoPhuBan might benefit from having VipPromotion done first (to get rewards)
            // But we'll keep it flexible - dependencies are optional unless explicitly required
            
            // TriAn might need energy from DoiNangNo, but it's not a hard requirement
            // { FeatureType.TriAn, new List<FeatureType> { FeatureType.DoiNangNo } },
        };

        /// <summary>
        /// Get all dependencies for a feature
        /// </summary>
        public static IEnumerable<FeatureType> GetDependencies(FeatureType feature)
        {
            if (_dependencies.TryGetValue(feature, out var deps))
            {
                return deps;
            }
            return Enumerable.Empty<FeatureType>();
        }

        /// <summary>
        /// Check if a feature can execute (all dependencies are completed)
        /// </summary>
        public static bool CanExecute(FeatureType feature, Dictionary<FeatureType, FeatureExecutionStatus> featureStatuses)
        {
            var dependencies = GetDependencies(feature);
            
            foreach (var dep in dependencies)
            {
                if (!featureStatuses.TryGetValue(dep, out var status) || 
                    status != FeatureExecutionStatus.Completed)
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Add a dependency rule
        /// </summary>
        public static void AddDependency(FeatureType feature, FeatureType dependency)
        {
            if (!_dependencies.ContainsKey(feature))
            {
                _dependencies[feature] = new List<FeatureType>();
            }
            
            if (!_dependencies[feature].Contains(dependency))
            {
                _dependencies[feature].Add(dependency);
            }
        }

        /// <summary>
        /// Remove a dependency rule
        /// </summary>
        public static void RemoveDependency(FeatureType feature, FeatureType dependency)
        {
            if (_dependencies.TryGetValue(feature, out var deps))
            {
                deps.Remove(dependency);
                if (deps.Count == 0)
                {
                    _dependencies.Remove(feature);
                }
            }
        }
    }
}

