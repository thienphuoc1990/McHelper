using AutoVPT.Domain;
using AutoVPT.Services;
using AutoVPT.Tests.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExecutionContext = AutoVPT.Services.ExecutionContext;

namespace AutoVPT.Tests.Unit
{
    [TestClass]
    public class FeatureOrchestratorTests
    {
        private FeatureOrchestrator _orchestrator;
        private MockLogger _logger;
        private List<IFeatureExecutor> _executors;
        private CharacterAggregate _character;

        [TestInitialize]
        public void Setup()
        {
            _logger = new MockLogger();
            _executors = new List<IFeatureExecutor>();
            _character = new CharacterAggregate("test-char", "http://game.url");
        }

        private void CreateOrchestrator()
        {
            _orchestrator = new FeatureOrchestrator(_executors, _logger);
        }

        #region Constructor Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullExecutors_ThrowsException()
        {
            new FeatureOrchestrator(null, _logger);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullLogger_ThrowsException()
        {
            new FeatureOrchestrator(_executors, null);
        }

        #endregion

        #region ExecuteAllAsync Tests

        [TestMethod]
        public async Task ExecuteAllAsync_WithNoEnabledFeatures_ReturnsEmptySummary()
        {
            // Arrange
            CreateOrchestrator();

            // Act
            var summary = await _orchestrator.ExecuteAllAsync(
                _character, 
                IntPtr.Zero, 
                null, 
                CancellationToken.None);

            // Assert
            Assert.IsNotNull(summary);
            Assert.AreEqual(0, summary.TotalCount);
            Assert.AreEqual(_character.Identity.Id, summary.CharacterId);
        }

        [TestMethod]
        public async Task ExecuteAllAsync_WithCancellation_SetsCancelledFlag()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately
            
            _executors.Add(new TestExecutor(FeatureType.VipPromotion, true));
            CreateOrchestrator();
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);

            // Act
            var summary = await _orchestrator.ExecuteAllAsync(
                _character,
                IntPtr.Zero,
                null,
                cts.Token);

            // Assert
            Assert.IsTrue(summary.WasCancelled);
        }

        [TestMethod]
        public async Task ExecuteAllAsync_WithSuccessfulExecutor_RecordsSuccess()
        {
            // Arrange
            _executors.Add(new TestExecutor(FeatureType.VipPromotion, true));
            CreateOrchestrator();
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);

            // Act
            var summary = await _orchestrator.ExecuteAllAsync(
                _character,
                IntPtr.Zero,
                null,
                CancellationToken.None);

            // Assert
            Assert.AreEqual(1, summary.SuccessCount);
            Assert.AreEqual(0, summary.FailedCount);
            Assert.IsTrue(summary.Results[FeatureType.VipPromotion].Success);
        }

        [TestMethod]
        public async Task ExecuteAllAsync_WithFailingExecutor_RecordsFailure()
        {
            // Arrange
            _executors.Add(new TestExecutor(FeatureType.VipPromotion, false, "Test failure"));
            CreateOrchestrator();
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);

            // Act
            var summary = await _orchestrator.ExecuteAllAsync(
                _character,
                IntPtr.Zero,
                null,
                CancellationToken.None);

            // Assert
            Assert.AreEqual(0, summary.SuccessCount);
            Assert.AreEqual(1, summary.FailedCount);
            Assert.IsFalse(summary.Results[FeatureType.VipPromotion].Success);
        }

        [TestMethod]
        public async Task ExecuteAllAsync_RaisesFeatureStartedEvent()
        {
            // Arrange
            var eventRaised = false;
            _executors.Add(new TestExecutor(FeatureType.VipPromotion, true));
            CreateOrchestrator();
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            _orchestrator.FeatureStarted += (s, e) => eventRaised = true;

            // Act
            await _orchestrator.ExecuteAllAsync(
                _character,
                IntPtr.Zero,
                null,
                CancellationToken.None);

            // Assert
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public async Task ExecuteAllAsync_RaisesFeatureCompletedEvent()
        {
            // Arrange
            var eventRaised = false;
            FeatureResult capturedResult = null;
            _executors.Add(new TestExecutor(FeatureType.VipPromotion, true));
            CreateOrchestrator();
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            _orchestrator.FeatureCompleted += (s, e) => 
            {
                eventRaised = true;
                capturedResult = e.Result;
            };

            // Act
            await _orchestrator.ExecuteAllAsync(
                _character,
                IntPtr.Zero,
                null,
                CancellationToken.None);

            // Assert
            Assert.IsTrue(eventRaised);
            Assert.IsNotNull(capturedResult);
            Assert.IsTrue(capturedResult.Success);
        }

        [TestMethod]
        public async Task ExecuteAllAsync_RaisesExecutionCompletedEvent()
        {
            // Arrange
            var eventRaised = false;
            ExecutionSummary capturedSummary = null;
            _executors.Add(new TestExecutor(FeatureType.VipPromotion, true));
            CreateOrchestrator();
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);
            _orchestrator.ExecutionCompleted += (s, e) =>
            {
                eventRaised = true;
                capturedSummary = e.Summary;
            };

            // Act
            await _orchestrator.ExecuteAllAsync(
                _character,
                IntPtr.Zero,
                null,
                CancellationToken.None);

            // Assert
            Assert.IsTrue(eventRaised);
            Assert.IsNotNull(capturedSummary);
        }

        #endregion

        #region ExecuteSingleAsync Tests

        [TestMethod]
        public async Task ExecuteSingleAsync_WithMissingExecutor_ReturnsFailed()
        {
            // Arrange
            CreateOrchestrator();
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);

            // Act
            var result = await _orchestrator.ExecuteSingleAsync(
                _character,
                IntPtr.Zero,
                FeatureType.VipPromotion,
                null,
                CancellationToken.None);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.Message.Contains("No executor"));
        }

        [TestMethod]
        public async Task ExecuteSingleAsync_WithSuccessfulExecutor_ReturnsSuccess()
        {
            // Arrange
            _executors.Add(new TestExecutor(FeatureType.VipPromotion, true));
            CreateOrchestrator();
            _character.FeatureConfig.Enable(FeatureType.VipPromotion);

            // Act
            var result = await _orchestrator.ExecuteSingleAsync(
                _character,
                IntPtr.Zero,
                FeatureType.VipPromotion,
                null,
                CancellationToken.None);

            // Assert
            Assert.IsTrue(result.Success);
        }

        #endregion

        #region Test Helper

        /// <summary>
        /// Test executor for unit testing
        /// </summary>
        private class TestExecutor : IFeatureExecutor
        {
            private readonly bool _succeeds;
            private readonly string _message;

            public FeatureType Type { get; }

            public TestExecutor(FeatureType type, bool succeeds, string message = "Test completed")
            {
                Type = type;
                _succeeds = succeeds;
                _message = message;
            }

            public Task<FeatureResult> ExecuteAsync(ExecutionContext context)
            {
                if (_succeeds)
                    return Task.FromResult(FeatureResult.Successful(_message));
                else
                    return Task.FromResult(FeatureResult.Failed(_message));
            }

            public bool CanExecute(ExecutionContext context)
            {
                return context.Config.IsEnabled(Type);
            }
        }

        #endregion
    }
}

