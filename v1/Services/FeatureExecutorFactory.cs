using AutoVPT.Domain;
using System;
using System.Collections.Generic;

namespace AutoVPT.Services
{
    /// <summary>
    /// Factory for creating and managing feature executors.
    /// Implements Factory pattern for executor creation.
    /// </summary>
    public class FeatureExecutorFactory
    {
        private readonly Dictionary<FeatureType, IFeatureExecutor> _executors;
        private readonly IServiceProvider _serviceProvider;

        public FeatureExecutorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _executors = new Dictionary<FeatureType, IFeatureExecutor>();
        }

        /// <summary>
        /// Register an executor for a feature type
        /// </summary>
        public void Register(IFeatureExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException(nameof(executor));

            _executors[executor.Type] = executor;
        }

        /// <summary>
        /// Get executor for a feature type
        /// </summary>
        public IFeatureExecutor GetExecutor(FeatureType featureType)
        {
            if (_executors.ContainsKey(featureType))
            {
                return _executors[featureType];
            }

            return null;
        }

        /// <summary>
        /// Get all registered executors
        /// </summary>
        public IEnumerable<IFeatureExecutor> GetAllExecutors()
        {
            return _executors.Values;
        }

        /// <summary>
        /// Check if executor exists for feature type
        /// </summary>
        public bool HasExecutor(FeatureType featureType)
        {
            return _executors.ContainsKey(featureType);
        }
    }
}
