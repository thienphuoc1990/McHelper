using AutoVPT.Services.Orchestrator;
using AutoVPT.Services.Orchestrator.Models;
using AutoVPT.Interfaces;
using AutoVPT.Repositories;
using AutoVPT.Services;
using AutoVPT.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoVPT.Services.Orchestrator
{
    /// <summary>
    /// Example usage of the OrchestratorService
    /// This class demonstrates how to initialize and use the orchestrator
    /// </summary>
    public static class OrchestratorExample
    {
        /// <summary>
        /// Example: Initialize and start orchestrator for all characters
        /// </summary>
        public static async Task RunOrchestratorExample()
        {
            // Step 1: Ensure ServiceContainer is initialized
            // (Usually done in Form1_Load)
            // ServiceContainer.Initialize(textBoxStatus);

            // Step 2: Get dependencies
            var characterRepo = ServiceContainer.GetService<ICharacterRepository>();
            var automationService = ServiceContainer.GetService<IAutomationService>();
            var logger = ServiceContainer.GetService<ILogger>();

            // Step 3: Create orchestrator components
            var decisionEngine = new DecisionEngine();
            var statusTracker = new StatusTracker();
            var executionManager = new ExecutionManager(
                automationService,
                characterRepo,
                logger,
                maxConcurrentExecutions: 5  // Run up to 5 characters simultaneously
            );

            // Step 4: Create orchestrator service
            var orchestrator = new OrchestratorService(
                decisionEngine,
                statusTracker,
                executionManager,
                characterRepo,
                logger
            );

            // Step 5: Register all characters
            var allCharacters = characterRepo.GetAll();
            foreach (var character in allCharacters)
            {
                // Only register characters that are set to run
                if (character.Running == 1)
                {
                    orchestrator.RegisterCharacter(character.ID);
                    logger.LogInfo($"Registered character: {character.ID}", "Orchestrator");
                }
            }

            // Step 6: Start orchestration
            var cts = new CancellationTokenSource();
            logger.LogInfo("Starting orchestrator...", "Orchestrator");
            
            try
            {
                await orchestrator.StartOrchestrationAsync(cts.Token);
                logger.LogInfo("Orchestrator started successfully", "Orchestrator");

                // Step 7: Monitor status (optional)
                await MonitorOrchestrationAsync(orchestrator, logger, cts.Token);
            }
            catch (OperationCanceledException)
            {
                logger.LogInfo("Orchestrator stopped", "Orchestrator");
            }
            catch (Exception ex)
            {
                logger.LogError("Error in orchestrator", ex, "Orchestrator");
            }
        }

        /// <summary>
        /// Example: Monitor orchestration status
        /// </summary>
        private static async Task MonitorOrchestrationAsync(
            IOrchestratorService orchestrator,
            ILogger logger,
            CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && orchestrator.IsRunning)
            {
                try
                {
                    var status = await orchestrator.GetStatusAsync();
                    
                    logger.LogInfo(
                        $"Status - Active: {status.ActiveCharacters}, " +
                        $"Completed: {status.CompletedCharacters}, " +
                        $"Progress: {status.Statistics.OverallCompletionPercentage:F1}%",
                        "Orchestrator"
                    );

                    // Check each character's status
                    foreach (var charStatus in status.CharacterStatuses.Values)
                    {
                        if (charStatus.IsActive)
                        {
                            logger.LogInfo(
                                $"{charStatus.CharacterId}: " +
                                $"{charStatus.CompletedFeatures}/{charStatus.TotalEnabledFeatures} " +
                                $"({charStatus.CompletionPercentage:F1}%) - " +
                                $"Next: {charStatus.NextAction?.Feature}",
                                charStatus.CharacterId
                            );
                        }
                    }

                    await Task.Delay(5000, ct); // Update every 5 seconds
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogError("Error monitoring orchestration", ex, "Orchestrator");
                    await Task.Delay(5000, ct);
                }
            }
        }

        /// <summary>
        /// Example: Get next action for a specific character
        /// </summary>
        public static async Task<NextAction> GetNextActionExample(string characterId)
        {
            var characterRepo = ServiceContainer.GetService<ICharacterRepository>();
            var automationService = ServiceContainer.GetService<IAutomationService>();
            var logger = ServiceContainer.GetService<ILogger>();

            var decisionEngine = new DecisionEngine();
            var statusTracker = new StatusTracker();
            var executionManager = new ExecutionManager(automationService, characterRepo, logger);

            var orchestrator = new OrchestratorService(
                decisionEngine,
                statusTracker,
                executionManager,
                characterRepo,
                logger
            );

            return await orchestrator.GetNextActionAsync(characterId);
        }

        /// <summary>
        /// Example: Simple start/stop orchestrator for Form1 integration
        /// </summary>
        public static class Form1Integration
        {
            private static IOrchestratorService _orchestrator;
            private static CancellationTokenSource _cts;

            /// <summary>
            /// Initialize orchestrator (call from Form1_Load or button click)
            /// </summary>
            public static void Initialize()
            {
                if (_orchestrator != null)
                    return;

                var characterRepo = ServiceContainer.GetService<ICharacterRepository>();
                var automationService = ServiceContainer.GetService<IAutomationService>();
                var logger = ServiceContainer.GetService<ILogger>();

                var decisionEngine = new DecisionEngine();
                var statusTracker = new StatusTracker();
                var executionManager = new ExecutionManager(
                    automationService,
                    characterRepo,
                    logger,
                    maxConcurrentExecutions: 5
                );

                _orchestrator = new OrchestratorService(
                    decisionEngine,
                    statusTracker,
                    executionManager,
                    characterRepo,
                    logger
                );

                // Register all running characters
                var allCharacters = characterRepo.GetAll();
                foreach (var character in allCharacters)
                {
                    if (character.Running == 1)
                    {
                        _orchestrator.RegisterCharacter(character.ID);
                    }
                }
            }

            /// <summary>
            /// Start orchestrator (call from button click)
            /// </summary>
            public static async Task StartAsync()
            {
                if (_orchestrator == null)
                {
                    Initialize();
                }

                if (_orchestrator.IsRunning)
                {
                    MessageBox.Show("Orchestrator is already running!");
                    return;
                }

                _cts = new CancellationTokenSource();
                await _orchestrator.StartOrchestrationAsync(_cts.Token);
                MessageBox.Show("Orchestrator started!");
            }

            /// <summary>
            /// Stop orchestrator (call from button click)
            /// </summary>
            public static async Task StopAsync()
            {
                if (_orchestrator != null && _orchestrator.IsRunning)
                {
                    await _orchestrator.StopOrchestrationAsync();
                    _cts?.Cancel();
                    MessageBox.Show("Orchestrator stopped!");
                }
            }

            /// <summary>
            /// Get current status (call from button click or timer)
            /// </summary>
            public static async Task<OrchestrationStatus> GetStatusAsync()
            {
                if (_orchestrator == null)
                    return null;

                return await _orchestrator.GetStatusAsync();
            }
        }
    }
}

