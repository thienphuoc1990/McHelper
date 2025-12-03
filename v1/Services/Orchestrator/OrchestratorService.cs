using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Services.Orchestrator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Services.Orchestrator
{
    /// <summary>
    /// Main orchestrator service that coordinates feature execution across all characters
    /// </summary>
    public class OrchestratorService : IOrchestratorService
    {
        private readonly IDecisionEngine _decisionEngine;
        private readonly IStatusTracker _statusTracker;
        private readonly IExecutionManager _executionManager;
        private readonly ICharacterRepository _characterRepository;
        private readonly ILogger _logger;
        private readonly HashSet<string> _registeredCharacters;
        private readonly object _lock = new object();
        private CancellationTokenSource _orchestrationCts;
        private Task _orchestrationTask;
        private bool _isRunning;

        public bool IsRunning
        {
            get
            {
                lock (_lock)
                {
                    return _isRunning;
                }
            }
        }

        public OrchestratorService(
            IDecisionEngine decisionEngine,
            IStatusTracker statusTracker,
            IExecutionManager executionManager,
            ICharacterRepository characterRepository,
            ILogger logger)
        {
            _decisionEngine = decisionEngine ?? throw new ArgumentNullException(nameof(decisionEngine));
            _statusTracker = statusTracker ?? throw new ArgumentNullException(nameof(statusTracker));
            _executionManager = executionManager ?? throw new ArgumentNullException(nameof(executionManager));
            _characterRepository = characterRepository ?? throw new ArgumentNullException(nameof(characterRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _registeredCharacters = new HashSet<string>();
        }

        /// <summary>
        /// Start orchestration for all active characters
        /// </summary>
        public async Task StartOrchestrationAsync(CancellationToken ct = default)
        {
            lock (_lock)
            {
                if (_isRunning)
                {
                    _logger.LogWarning("Orchestration is already running");
                    return;
                }

                _isRunning = true;
                _orchestrationCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            }

            _logger.LogInfo("Starting orchestration", "Orchestrator");

            _orchestrationTask = Task.Run(async () => await OrchestrationLoopAsync(_orchestrationCts.Token), _orchestrationCts.Token);

            await Task.CompletedTask;
        }

        /// <summary>
        /// Stop orchestration and all running characters
        /// </summary>
        public async Task StopOrchestrationAsync()
        {
            lock (_lock)
            {
                if (!_isRunning)
                {
                    return;
                }

                _isRunning = false;
            }

            _logger.LogInfo("Stopping orchestration", "Orchestrator");

            // Cancel orchestration loop first
            _orchestrationCts?.Cancel();

            // Stop all active executions
            var characterIds = new List<string>();
            lock (_lock)
            {
                characterIds.AddRange(_registeredCharacters);
            }

            var stopTasks = new List<Task>();
            foreach (var characterId in characterIds)
            {
                stopTasks.Add(_executionManager.CancelExecutionAsync(characterId));
            }

            // Wait for all stop operations to complete (with timeout)
            try
            {
                var allStopped = Task.WhenAll(stopTasks);
                if (!allStopped.Wait(TimeSpan.FromSeconds(10)))
                {
                    _logger.LogWarning("Timeout waiting for all executions to stop", "Orchestrator");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Error stopping executions", ex, "Orchestrator");
            }

            // Wait for orchestration task to complete (with timeout)
            if (_orchestrationTask != null)
            {
                try
                {
                    if (!_orchestrationTask.Wait(TimeSpan.FromSeconds(5)))
                    {
                        _logger.LogWarning("Timeout waiting for orchestration loop to stop", "Orchestrator");
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when stopping
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error waiting for orchestration task", ex, "Orchestrator");
                }
            }

            // Dispose cancellation token source
            _orchestrationCts?.Dispose();
            _orchestrationCts = null;
            _orchestrationTask = null;

            _logger.LogInfo("Orchestration stopped", "Orchestrator");
        }

        /// <summary>
        /// Get next action for a character
        /// </summary>
        public async Task<NextAction> GetNextActionAsync(string characterId)
        {
            return await Task.Run(() =>
            {
                var character = _characterRepository.GetById(characterId);
                if (character == null)
                {
                    return NextAction.Stop(characterId, "Character not found");
                }

                var aggregate = CharacterAdapter.ToAggregate(character);
                return _decisionEngine.DetermineNextAction(aggregate);
            });
        }

        /// <summary>
        /// Get overall status
        /// </summary>
        public async Task<OrchestrationStatus> GetStatusAsync()
        {
            return await Task.Run(() => _statusTracker.GetOrchestrationStatus());
        }

        /// <summary>
        /// Get character-specific status
        /// </summary>
        public async Task<CharacterOrchestrationStatus> GetCharacterStatusAsync(string characterId)
        {
            return await Task.Run(() =>
            {
                var status = _statusTracker.GetCharacterStatus(characterId);
                if (status == null)
                {
                    // Create initial status
                    status = new CharacterOrchestrationStatus
                    {
                        CharacterId = characterId,
                        IsActive = false
                    };
                }
                return status;
            });
        }

        /// <summary>
        /// Register character for orchestration
        /// </summary>
        public void RegisterCharacter(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
                return;

            lock (_lock)
            {
                _registeredCharacters.Add(characterId);
            }

            _logger.LogInfo($"Registered character: {characterId}", "Orchestrator");
        }

        /// <summary>
        /// Unregister character
        /// </summary>
        public void UnregisterCharacter(string characterId)
        {
            if (string.IsNullOrWhiteSpace(characterId))
                return;

            lock (_lock)
            {
                _registeredCharacters.Remove(characterId);
            }

            _logger.LogInfo($"Unregistered character: {characterId}", "Orchestrator");
        }

        /// <summary>
        /// Main orchestration loop
        /// </summary>
        private async Task OrchestrationLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var characterIds = new List<string>();
                    lock (_lock)
                    {
                        characterIds.AddRange(_registeredCharacters);
                    }

                    // Process each character
                    foreach (var characterId in characterIds)
                    {
                        if (ct.IsCancellationRequested)
                            break;

                        await ProcessCharacterAsync(characterId, ct);
                    }

                    // Update status
                    UpdateStatuses(characterIds);

                    // Wait before next iteration
                    await Task.Delay(1000, ct); // Check every second
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error in orchestration loop", ex, "Orchestrator");
                    await Task.Delay(5000, ct); // Wait longer on error
                }
            }
        }

        /// <summary>
        /// Process a single character
        /// </summary>
        private async Task ProcessCharacterAsync(string characterId, CancellationToken ct)
        {
            // Check cancellation before processing
            if (ct.IsCancellationRequested)
                return;

            try
            {
                // Check if can execute
                if (!_executionManager.CanExecute(characterId))
                {
                    return; // Already running or at limit
                }

                // Check cancellation again before getting next action
                if (ct.IsCancellationRequested)
                    return;

                // Get next action
                var nextAction = await GetNextActionAsync(characterId);

                // Check cancellation before executing
                if (ct.IsCancellationRequested)
                    return;

                if (nextAction.ActionType == Models.ActionType.ExecuteFeature && nextAction.Feature.HasValue)
                {
                    // Execute the action
                    var result = await _executionManager.ExecuteActionAsync(characterId, nextAction, ct);

                    // Only update status if not cancelled
                    if (!ct.IsCancellationRequested)
                    {
                        UpdateCharacterStatus(characterId, nextAction.Feature.Value, result);
                    }
                }
                else if (nextAction.ActionType == Models.ActionType.Complete)
                {
                    // Character is done
                    if (!ct.IsCancellationRequested)
                    {
                        UpdateCharacterStatus(characterId, null, null);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping - don't log as error
                _logger.LogInfo($"Processing cancelled for {characterId}", characterId);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error processing character {characterId}", ex, characterId);
            }
        }

        /// <summary>
        /// Update status for all characters
        /// </summary>
        private void UpdateStatuses(List<string> characterIds)
        {
            foreach (var characterId in characterIds)
            {
                try
                {
                    var character = _characterRepository.GetById(characterId);
                    if (character == null)
                        continue;

                    var aggregate = CharacterAdapter.ToAggregate(character);
                    var nextAction = _decisionEngine.DetermineNextAction(aggregate);

                    var status = new CharacterOrchestrationStatus
                    {
                        CharacterId = characterId,
                        IsActive = aggregate.RuntimeState.IsRunning,
                        CurrentFeature = nextAction.Feature,
                        CompletedFeatures = aggregate.RuntimeState.FeatureStatuses.Values.Count(s => s == FeatureExecutionStatus.Completed),
                        TotalEnabledFeatures = aggregate.GetEnabledFeatures().Count(),
                        CompletionPercentage = CalculateCompletionPercentage(aggregate),
                        NextAction = nextAction,
                        LastUpdate = DateTime.Now
                    };

                    _statusTracker.UpdateCharacterStatus(characterId, status);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error updating status for {characterId}", ex, characterId);
                }
            }
        }

        /// <summary>
        /// Update character status after execution
        /// </summary>
        private void UpdateCharacterStatus(string characterId, FeatureType? feature, ExecutionResult result)
        {
            try
            {
                var character = _characterRepository.GetById(characterId);
                if (character == null)
                    return;

                var aggregate = CharacterAdapter.ToAggregate(character);

                if (feature.HasValue)
                {
                    var log = new FeatureExecutionLog
                    {
                        Feature = feature.Value,
                        Status = result.Success ? FeatureExecutionStatus.Completed : FeatureExecutionStatus.Failed,
                        StartTime = DateTime.Now.Add(-result.Duration),
                        EndTime = DateTime.Now,
                        Message = result.Message
                    };

                    var status = _statusTracker.GetCharacterStatus(characterId);
                    if (status == null)
                    {
                        status = new CharacterOrchestrationStatus { CharacterId = characterId };
                    }

                    status.ExecutionLog.Add(log);
                    status.CurrentFeature = feature;
                    status.CurrentFeatureStatus = log.Status;
                    status.LastUpdate = DateTime.Now;

                    _statusTracker.UpdateCharacterStatus(characterId, status);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating character status for {characterId}", ex, characterId);
            }
        }

        /// <summary>
        /// Calculate completion percentage for character
        /// </summary>
        private double CalculateCompletionPercentage(CharacterAggregate aggregate)
        {
            var enabledFeatures = aggregate.GetEnabledFeatures().ToList();
            if (enabledFeatures.Count == 0)
                return 100;

            var completedCount = enabledFeatures.Count(f => aggregate.RuntimeState.IsCompleted(f));
            return (double)completedCount / enabledFeatures.Count * 100;
        }
    }
}

