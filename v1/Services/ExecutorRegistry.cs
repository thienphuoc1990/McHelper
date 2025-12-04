using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Services.Executors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoVPT.Services
{
    /// <summary>
    /// Registry for feature executors.
    /// Manages executor registration and provides executor lookup by feature type.
    /// </summary>
    public class ExecutorRegistry
    {
        private readonly Dictionary<FeatureType, IFeatureExecutor> _executors;
        private readonly IImageRecognition _imageRecognition;
        private readonly IInputSimulator _inputSimulator;
        private readonly ILogger _logger;

        /// <summary>
        /// Gets all registered executors
        /// </summary>
        public IEnumerable<IFeatureExecutor> AllExecutors => _executors.Values;

        /// <summary>
        /// Gets all registered feature types
        /// </summary>
        public IEnumerable<FeatureType> RegisteredFeatures => _executors.Keys;

        /// <summary>
        /// Gets the count of registered executors
        /// </summary>
        public int Count => _executors.Count;

        public ExecutorRegistry(
            IImageRecognition imageRecognition,
            IInputSimulator inputSimulator,
            ILogger logger)
        {
            _imageRecognition = imageRecognition ?? throw new ArgumentNullException(nameof(imageRecognition));
            _inputSimulator = inputSimulator ?? throw new ArgumentNullException(nameof(inputSimulator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _executors = new Dictionary<FeatureType, IFeatureExecutor>();

            // Register all built-in executors
            RegisterBuiltInExecutors();
        }

        /// <summary>
        /// Register all built-in executors
        /// </summary>
        private void RegisterBuiltInExecutors()
        {
            // Daily tasks
            Register(new VipPromotionExecutor(_imageRecognition, _inputSimulator, _logger));
            Register(new NhanHoiPhucExecutor(_imageRecognition, _inputSimulator, _logger));
            Register(new NhanThuongHLVTExecutor(_imageRecognition, _inputSimulator, _logger));
            Register(new DoiNangNoExecutor(_imageRecognition, _inputSimulator, _logger));
            Register(new DoiKGDKExecutor(_imageRecognition, _inputSimulator, _logger));
            Register(new RutBoExecutor(_imageRecognition, _inputSimulator, _logger));

            // Crafting/farming
            Register(new TrongNLExecutor(_imageRecognition, _inputSimulator, _logger));
            Register(new CheMatBaoExecutor(_imageRecognition, _inputSimulator, _logger));

            // Quests
            Register(new TuHanhExecutor(_imageRecognition, _inputSimulator, _logger));
            Register(new TriAnExecutor(_imageRecognition, _inputSimulator, _logger));
            Register(new TruMaExecutor(_imageRecognition, _inputSimulator, _logger));
            Register(new AutoThanTuExecutor(_imageRecognition, _inputSimulator, _logger));

            // Dungeons
            Register(new AutoPhuBanExecutor(_imageRecognition, _inputSimulator, _logger));

            _logger.LogDebug($"[ExecutorRegistry] Registered {_executors.Count} executors", "System");
        }

        /// <summary>
        /// Register an executor
        /// </summary>
        public void Register(IFeatureExecutor executor)
        {
            if (executor == null)
                throw new ArgumentNullException(nameof(executor));

            if (_executors.ContainsKey(executor.Type))
            {
                _logger.LogWarning($"[ExecutorRegistry] Overwriting executor for {executor.Type}", "System");
            }

            _executors[executor.Type] = executor;
        }

        /// <summary>
        /// Get executor for a specific feature type
        /// </summary>
        public IFeatureExecutor GetExecutor(FeatureType featureType)
        {
            if (_executors.TryGetValue(featureType, out var executor))
            {
                return executor;
            }

            return null;
        }

        /// <summary>
        /// Check if an executor exists for a feature type
        /// </summary>
        public bool HasExecutor(FeatureType featureType)
        {
            return _executors.ContainsKey(featureType);
        }

        /// <summary>
        /// Get executors for multiple feature types
        /// </summary>
        public IEnumerable<IFeatureExecutor> GetExecutors(IEnumerable<FeatureType> featureTypes)
        {
            return featureTypes
                .Where(HasExecutor)
                .Select(GetExecutor);
        }

        /// <summary>
        /// Get feature types that don't have executors yet
        /// </summary>
        public IEnumerable<FeatureType> GetMissingExecutors()
        {
            return Enum.GetValues(typeof(FeatureType))
                .Cast<FeatureType>()
                .Where(t => !HasExecutor(t));
        }

        /// <summary>
        /// Create an orchestrator with all registered executors
        /// </summary>
        public FeatureOrchestrator CreateOrchestrator()
        {
            return new FeatureOrchestrator(AllExecutors, _logger);
        }
    }
}

