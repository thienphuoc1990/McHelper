using AutoVPT.Interfaces;
using AutoVPT.Services;
using AutoVPT.Services.Orchestrator.Models;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Services.Orchestrator
{
    /// <summary>
    /// Manages execution of actions for characters
    /// </summary>
    public class ExecutionManager : IExecutionManager
    {
        private readonly IAutomationService _automationService;
        private readonly ICharacterRepository _characterRepository;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeExecutions;
        private int _maxConcurrentExecutions;

        public ExecutionManager(
            IAutomationService automationService,
            ICharacterRepository characterRepository,
            ILogger logger,
            int maxConcurrentExecutions = 5)
        {
            _automationService = automationService ?? throw new ArgumentNullException(nameof(automationService));
            _characterRepository = characterRepository ?? throw new ArgumentNullException(nameof(characterRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _maxConcurrentExecutions = maxConcurrentExecutions;
            _activeExecutions = new ConcurrentDictionary<string, CancellationTokenSource>();
        }

        public int MaxConcurrentExecutions
        {
            get => _maxConcurrentExecutions;
            set => _maxConcurrentExecutions = Math.Max(1, value); // Ensure at least 1
        }

        /// <summary>
        /// Execute next action for character
        /// </summary>
        public async Task<ExecutionResult> ExecuteActionAsync(string characterId, NextAction action, CancellationToken ct)
        {
            if (action == null)
            {
                return ExecutionResult.Failed("Action is null");
            }

            if (action.ActionType != Models.ActionType.ExecuteFeature)
            {
                return ExecutionResult.Failed($"Cannot execute action type: {action.ActionType}");
            }

            if (!action.Feature.HasValue)
            {
                return ExecutionResult.Failed("No feature specified in action");
            }

            var feature = action.Feature.Value;
            var startTime = DateTime.Now;

            try
            {
                _logger.LogInfo($"Executing {feature} for {characterId}", characterId);

                // Create cancellation token source for this execution
                var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                _activeExecutions[characterId] = cts;

                // Execute the feature
                var result = await _automationService.RunFeatureAsync(characterId, feature, cts.Token);

                var duration = DateTime.Now - startTime;

                if (result.Success)
                {
                    _logger.LogInfo($"Completed {feature} for {characterId} in {duration.TotalSeconds:F1}s", characterId);
                    return ExecutionResult.Successful($"Feature {feature} completed successfully", duration);
                }
                else
                {
                    _logger.LogWarning($"Failed {feature} for {characterId}: {result.Message}", characterId);
                    return ExecutionResult.Failed($"Feature {feature} failed: {result.Message}");
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInfo($"Cancelled {feature} for {characterId}", characterId);
                return ExecutionResult.Failed("Execution was cancelled");
            }
            catch (Exception ex)
            {
                var duration = DateTime.Now - startTime;
                _logger.LogError($"Exception executing {feature} for {characterId}", ex, characterId);
                return ExecutionResult.Failed($"Exception: {ex.Message}");
            }
            finally
            {
                // Remove from active executions
                _activeExecutions.TryRemove(characterId, out _);
            }
        }

        /// <summary>
        /// Check if character can execute (not already running)
        /// </summary>
        public bool CanExecute(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
                return false;

            // Check if already executing
            if (_activeExecutions.ContainsKey(characterId))
                return false;

            // Check concurrent execution limit
            if (_activeExecutions.Count >= MaxConcurrentExecutions)
                return false;

            return true;
        }

        /// <summary>
        /// Get active execution count
        /// </summary>
        public int GetActiveExecutionCount()
        {
            return _activeExecutions.Count;
        }

        /// <summary>
        /// Cancel execution for character
        /// </summary>
        public async Task CancelExecutionAsync(string characterId)
        {
            if (_activeExecutions.TryRemove(characterId, out var cts))
            {
                try
                {
                    // Cancel the cancellation token
                    cts.Cancel();
                    cts.Dispose();
                    
                    // Stop the automation service
                    await _automationService.StopAsync(characterId);
                    
                    _logger.LogInfo($"Cancelled execution for {characterId}", characterId);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error cancelling execution for {characterId}", ex, characterId);
                }
            }
        }

        /// <summary>
        /// Cancel all active executions
        /// </summary>
        public async Task CancelAllExecutionsAsync()
        {
            var characterIds = _activeExecutions.Keys.ToList();
            var cancelTasks = characterIds.Select(id => CancelExecutionAsync(id));
            await Task.WhenAll(cancelTasks);
        }
    }
}

