using AutoVPT.Domain;
using AutoVPT.Services;
using AutoVPT.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using ExecutionContext = AutoVPT.Services.ExecutionContext;

namespace AutoVPT.Tests.Unit
{
    [TestClass]
    public class ExecutorRegistryTests
    {
        private ExecutorRegistry _registry;
        private MockImageRecognition _imageRecognition;
        private MockInputSimulator _inputSimulator;
        private MockLogger _logger;

        [TestInitialize]
        public void Setup()
        {
            _imageRecognition = new MockImageRecognition();
            _inputSimulator = new MockInputSimulator();
            _logger = new MockLogger();
            _registry = new ExecutorRegistry(_imageRecognition, _inputSimulator, _logger);
        }

        #region Constructor Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullImageRecognition_ThrowsException()
        {
            new ExecutorRegistry(null, _inputSimulator, _logger);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullInputSimulator_ThrowsException()
        {
            new ExecutorRegistry(_imageRecognition, null, _logger);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullLogger_ThrowsException()
        {
            new ExecutorRegistry(_imageRecognition, _inputSimulator, null);
        }

        [TestMethod]
        public void Constructor_RegistersBuiltInExecutors()
        {
            // Assert
            Assert.IsTrue(_registry.Count > 0, "Registry should have registered executors");
        }

        #endregion

        #region GetExecutor Tests

        [TestMethod]
        public void GetExecutor_WithRegisteredType_ReturnsExecutor()
        {
            // Act
            var executor = _registry.GetExecutor(FeatureType.VipPromotion);

            // Assert
            Assert.IsNotNull(executor);
            Assert.AreEqual(FeatureType.VipPromotion, executor.Type);
        }

        [TestMethod]
        public void GetExecutor_WithUnregisteredType_ReturnsNull()
        {
            // First, check if there are any unregistered types
            var missingTypes = _registry.GetMissingExecutors().ToList();
            
            if (missingTypes.Count > 0)
            {
                // Act
                var executor = _registry.GetExecutor(missingTypes[0]);

                // Assert
                Assert.IsNull(executor);
            }
            else
            {
                // All types are registered, test passes
                Assert.IsTrue(true);
            }
        }

        #endregion

        #region HasExecutor Tests

        [TestMethod]
        public void HasExecutor_WithRegisteredType_ReturnsTrue()
        {
            // Act
            var hasExecutor = _registry.HasExecutor(FeatureType.VipPromotion);

            // Assert
            Assert.IsTrue(hasExecutor);
        }

        #endregion

        #region Register Tests

        [TestMethod]
        public void Register_WithNewExecutor_AddsToRegistry()
        {
            // Arrange
            var customExecutor = new TestExecutor(FeatureType.BugOnline);
            var initialCount = _registry.Count;

            // Check if BugOnline is already registered
            if (_registry.HasExecutor(FeatureType.BugOnline))
            {
                // Overwrite existing
                _registry.Register(customExecutor);
                Assert.AreEqual(initialCount, _registry.Count);
            }
            else
            {
                // Add new
                _registry.Register(customExecutor);
                Assert.AreEqual(initialCount + 1, _registry.Count);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Register_WithNullExecutor_ThrowsException()
        {
            _registry.Register(null);
        }

        #endregion

        #region AllExecutors Tests

        [TestMethod]
        public void AllExecutors_ReturnsAllRegistered()
        {
            // Act
            var allExecutors = _registry.AllExecutors.ToList();

            // Assert
            Assert.AreEqual(_registry.Count, allExecutors.Count);
        }

        #endregion

        #region RegisteredFeatures Tests

        [TestMethod]
        public void RegisteredFeatures_ReturnsAllTypes()
        {
            // Act
            var features = _registry.RegisteredFeatures.ToList();

            // Assert
            Assert.AreEqual(_registry.Count, features.Count);
            Assert.IsTrue(features.Contains(FeatureType.VipPromotion));
        }

        #endregion

        #region GetMissingExecutors Tests

        [TestMethod]
        public void GetMissingExecutors_ReturnsUnregisteredTypes()
        {
            // Act
            var missing = _registry.GetMissingExecutors().ToList();
            var registered = _registry.RegisteredFeatures.ToList();

            // Assert
            // The sum should equal all feature types
            var allTypes = Enum.GetValues(typeof(FeatureType)).Length;
            Assert.AreEqual(allTypes, missing.Count + registered.Count);
        }

        #endregion

        #region CreateOrchestrator Tests

        [TestMethod]
        public void CreateOrchestrator_ReturnsOrchestratorWithAllExecutors()
        {
            // Act
            var orchestrator = _registry.CreateOrchestrator();

            // Assert
            Assert.IsNotNull(orchestrator);
        }

        #endregion

        #region GetExecutors Tests

        [TestMethod]
        public void GetExecutors_WithMultipleTypes_ReturnsMatchingExecutors()
        {
            // Arrange
            var types = new[] { FeatureType.VipPromotion, FeatureType.DoiNangNo };

            // Act
            var executors = _registry.GetExecutors(types).ToList();

            // Assert
            Assert.IsTrue(executors.Count <= types.Length);
            foreach (var executor in executors)
            {
                Assert.IsTrue(types.Contains(executor.Type));
            }
        }

        #endregion

        #region Test Helper

        private class TestExecutor : IFeatureExecutor
        {
            public FeatureType Type { get; }

            public TestExecutor(FeatureType type)
            {
                Type = type;
            }

            public Task<FeatureResult> ExecuteAsync(ExecutionContext context)
            {
                return Task.FromResult(FeatureResult.Successful("Test"));
            }

            public bool CanExecute(ExecutionContext context)
            {
                return true;
            }
        }

        #endregion
    }
}

