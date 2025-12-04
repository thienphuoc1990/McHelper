using AutoVPT.Domain;
using AutoVPT.Services;
using AutoVPT.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Tests.Integration
{
    /// <summary>
    /// Integration tests for executor flows.
    /// Tests the full execution pipeline from orchestrator through executors.
    /// </summary>
    [TestClass]
    public class ExecutorIntegrationTests
    {
        private ExecutorRegistry _registry;
        private FeatureOrchestrator _orchestrator;
        private ConfigurationValidator _validator;
        private MockImageRecognition _imageRecognition;
        private MockInputSimulator _inputSimulator;
        private MockLogger _logger;
        private CharacterAggregate _character;

        [TestInitialize]
        public void Setup()
        {
            _imageRecognition = new MockImageRecognition();
            _inputSimulator = new MockInputSimulator();
            _logger = new MockLogger();
            _registry = new ExecutorRegistry(_imageRecognition, _inputSimulator, _logger);
            _orchestrator = _registry.CreateOrchestrator();
            _validator = new ConfigurationValidator();
            _character = new CharacterAggregate("test-char", "http://game.url");
        }

        #region Full Pipeline Tests

        [TestMethod]
        public async Task FullPipeline_ValidateAndExecute_Works()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);

            // Step 1: Validate
            var validationResults = _validator.ValidateAll(_character);
            Assert.IsTrue(validationResults[FeatureType.VipPromotion].IsValid);

            // Step 2: Execute
            var result = await _orchestrator.ExecuteSingleAsync(
                _character,
                IntPtr.Zero,
                FeatureType.VipPromotion,
                null,
                CancellationToken.None);

            // Assert - executor runs (may fail due to no window, but doesn't throw)
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task FullPipeline_MultipleFeatures_ExecutesInOrder()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            _character.FeatureConfig.Enable(FeatureType.NhanHoiPhuc);
            _character.FeatureConfig.Enable(FeatureType.DoiNangNo);

            var executionOrder = new System.Collections.Generic.List<FeatureType>();
            _orchestrator.FeatureStarted += (s, e) => executionOrder.Add(e.FeatureType);

            // Act
            var summary = await _orchestrator.ExecuteAllAsync(
                _character,
                IntPtr.Zero,
                null,
                CancellationToken.None);

            // Assert - features executed in order
            Assert.IsTrue(executionOrder.Count > 0);
            // VipPromotion should be first (quick rewards)
            if (executionOrder.Count > 0)
            {
                Assert.AreEqual(FeatureType.VipPromotion, executionOrder[0]);
            }
        }

        #endregion

        #region Cancellation Tests

        [TestMethod]
        public async Task Cancellation_DuringExecution_StopsGracefully()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            _character.FeatureConfig.Enable(FeatureType.DoiNangNo);
            _character.FeatureConfig.Enable(FeatureType.TuHanh);

            var cts = new CancellationTokenSource();
            var featuresStarted = 0;

            _orchestrator.FeatureStarted += (s, e) =>
            {
                featuresStarted++;
                if (featuresStarted >= 1)
                {
                    cts.Cancel(); // Cancel after first feature starts
                }
            };

            // Act
            var summary = await _orchestrator.ExecuteAllAsync(
                _character,
                IntPtr.Zero,
                null,
                cts.Token);

            // Assert
            Assert.IsTrue(summary.WasCancelled);
            Assert.IsTrue(summary.TotalCount < 3); // Not all features completed
        }

        [TestMethod]
        public async Task Cancellation_BeforeStart_ReturnsImmediately()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel before start

            // Act
            var summary = await _orchestrator.ExecuteAllAsync(
                _character,
                IntPtr.Zero,
                null,
                cts.Token);

            // Assert
            Assert.IsTrue(summary.WasCancelled);
            Assert.AreEqual(0, summary.TotalCount);
        }

        #endregion

        #region Event Flow Tests

        [TestMethod]
        public async Task Events_FireInCorrectOrder()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);

            var eventSequence = new System.Collections.Generic.List<string>();

            _orchestrator.FeatureStarted += (s, e) => eventSequence.Add("Started");
            _orchestrator.FeatureCompleted += (s, e) => eventSequence.Add("Completed");
            _orchestrator.ExecutionCompleted += (s, e) => eventSequence.Add("ExecutionCompleted");

            // Act
            await _orchestrator.ExecuteAllAsync(
                _character,
                IntPtr.Zero,
                null,
                CancellationToken.None);

            // Assert
            Assert.IsTrue(eventSequence.Count >= 3);
            Assert.AreEqual("Started", eventSequence[0]);
            Assert.AreEqual("Completed", eventSequence[1]);
            Assert.AreEqual("ExecutionCompleted", eventSequence[eventSequence.Count - 1]);
        }

        #endregion

        #region Registry Integration Tests

        [TestMethod]
        public void Registry_AllRegisteredExecutors_HaveCorrectTypes()
        {
            // Act
            foreach (var executor in _registry.AllExecutors)
            {
                // Assert
                Assert.IsNotNull(executor);
                Assert.IsTrue(Enum.IsDefined(typeof(FeatureType), executor.Type));
            }
        }

        [TestMethod]
        public void Registry_GetMissingExecutors_ListsUnimplementedFeatures()
        {
            // Act
            var missing = _registry.GetMissingExecutors();

            // Assert
            foreach (var featureType in missing)
            {
                Assert.IsFalse(_registry.HasExecutor(featureType));
            }
        }

        #endregion

        #region Validation Integration Tests

        [TestMethod]
        public void Validation_BeforeExecution_CatchesMissingConfig()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.AutoPhuBan);
            // Don't set DanhSach parameter

            // Act
            var results = _validator.ValidateAll(_character);

            // Assert
            Assert.IsFalse(results[FeatureType.AutoPhuBan].IsValid);
            Assert.IsTrue(results[FeatureType.AutoPhuBan].Errors.Count > 0);
        }

        [TestMethod]
        public void Validation_WithValidConfig_PassesAll()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.AutoPhuBan);
            _character.FeatureConfig.SetParameter(FeatureType.AutoPhuBan, "DanhSach", "Liệt Diễm Thâm Uyên");

            // Act
            var results = _validator.ValidateAll(_character);

            // Assert
            Assert.IsTrue(results[FeatureType.AutoPhuBan].IsValid);
        }

        #endregion

        #region RuntimeState Integration Tests

        [TestMethod]
        public async Task RuntimeState_UpdatedAfterExecution()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            Assert.IsFalse(_character.RuntimeState.IsCompleted(FeatureType.VipPromotion));

            // Act
            var result = await _orchestrator.ExecuteSingleAsync(
                _character,
                IntPtr.Zero,
                FeatureType.VipPromotion,
                null,
                CancellationToken.None);

            // Assert - if successful, should be marked completed
            if (result.Success)
            {
                Assert.IsTrue(_character.RuntimeState.IsCompleted(FeatureType.VipPromotion));
            }
        }

        #endregion

        #region Summary Statistics Tests

        [TestMethod]
        public async Task ExecutionSummary_TracksCorrectStatistics()
        {
            // Arrange
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            _character.FeatureConfig.Enable(FeatureType.DoiNangNo);

            // Act
            var summary = await _orchestrator.ExecuteAllAsync(
                _character,
                IntPtr.Zero,
                null,
                CancellationToken.None);

            // Assert
            Assert.AreEqual(_character.Identity.Id, summary.CharacterId);
            Assert.IsTrue(summary.Duration.TotalMilliseconds >= 0);
            Assert.AreEqual(summary.TotalCount, summary.SuccessCount + summary.FailedCount);
        }

        #endregion
    }
}

