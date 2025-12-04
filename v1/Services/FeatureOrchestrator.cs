using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Services
{
    /// <summary>
    /// Orchestrates the execution of multiple features for a character.
    /// Handles feature ordering, dependency management, and execution coordination.
    /// </summary>
    public class FeatureOrchestrator
    {
        private readonly Dictionary<FeatureType, IFeatureExecutor> _executors;
        private readonly ILogger _logger;

        /// <summary>
        /// Event raised when a feature starts executing
        /// </summary>
        public event EventHandler<FeatureExecutionEventArgs> FeatureStarted;

        /// <summary>
        /// Event raised when a feature completes
        /// </summary>
        public event EventHandler<FeatureExecutionEventArgs> FeatureCompleted;

        /// <summary>
        /// Event raised when all features complete
        /// </summary>
        public event EventHandler<ExecutionSummaryEventArgs> ExecutionCompleted;

        public FeatureOrchestrator(IEnumerable<IFeatureExecutor> executors, ILogger logger)
        {
            _executors = executors?.ToDictionary(e => e.Type) 
                ?? throw new ArgumentNullException(nameof(executors));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Execute all enabled features for a character in the recommended order.
        /// </summary>
        public async Task<ExecutionSummary> ExecuteAllAsync(
            CharacterAggregate character,
            IntPtr windowHandle,
            System.Windows.Forms.TextBox statusTextBox,
            CancellationToken cancellationToken)
        {
            var summary = new ExecutionSummary
            {
                CharacterId = character.Identity.Id,
                StartTime = DateTime.Now
            };

            // Get features in execution order
            var orderedFeatures = GetExecutionOrder(character);

            _logger.LogInfo($"[Orchestrator] Starting execution of {orderedFeatures.Count} features", character.Identity.Id);

            foreach (var featureType in orderedFeatures)
            {
                // Check for cancellation
                if (cancellationToken.IsCancellationRequested || Helper.IsStoppingAll())
                {
                    _logger.LogInfo("[Orchestrator] Execution cancelled", character.Identity.Id);
                    summary.WasCancelled = true;
                    break;
                }

                // Get executor
                if (!_executors.TryGetValue(featureType, out var executor))
                {
                    _logger.LogWarning($"[Orchestrator] No executor found for {featureType}", character.Identity.Id);
                    summary.Results[featureType] = FeatureResult.Failed("No executor available");
                    continue;
                }

                // Create execution context
                var context = CreateContext(character, windowHandle, featureType, statusTextBox, cancellationToken);

                // Check if feature can execute
                if (!executor.CanExecute(context))
                {
                    _logger.LogDebug($"[Orchestrator] Skipping {featureType} - cannot execute", character.Identity.Id);
                    summary.Results[featureType] = FeatureResult.Failed("Skipped - conditions not met");
                    continue;
                }

                // Execute feature
                try
                {
                    OnFeatureStarted(character.Identity.Id, featureType);
                    _logger.LogInfo($"[Orchestrator] Executing {featureType}", character.Identity.Id);

                    var result = await executor.ExecuteAsync(context);

                    summary.Results[featureType] = result;
                    OnFeatureCompleted(character.Identity.Id, featureType, result);

                    if (result.Success)
                    {
                        _logger.LogInfo($"[Orchestrator] {featureType} completed successfully", character.Identity.Id);
                        // Mark as completed in runtime state
                        character.RuntimeState.MarkCompleted(featureType);
                    }
                    else
                    {
                        _logger.LogWarning($"[Orchestrator] {featureType} failed: {result.Message}", character.Identity.Id);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInfo($"[Orchestrator] {featureType} was cancelled", character.Identity.Id);
                    summary.Results[featureType] = FeatureResult.Failed("Cancelled");
                    summary.WasCancelled = true;
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"[Orchestrator] {featureType} threw exception", ex, character.Identity.Id);
                    summary.Results[featureType] = FeatureResult.Failed(ex.Message, ex);
                }
            }

            summary.EndTime = DateTime.Now;
            OnExecutionCompleted(summary);

            _logger.LogInfo($"[Orchestrator] Execution completed - {summary.SuccessCount}/{summary.TotalCount} features succeeded", 
                character.Identity.Id);

            return summary;
        }

        /// <summary>
        /// Execute a single feature for a character.
        /// </summary>
        public async Task<FeatureResult> ExecuteSingleAsync(
            CharacterAggregate character,
            IntPtr windowHandle,
            FeatureType featureType,
            System.Windows.Forms.TextBox statusTextBox,
            CancellationToken cancellationToken)
        {
            if (!_executors.TryGetValue(featureType, out var executor))
            {
                return FeatureResult.Failed($"No executor found for {featureType}");
            }

            var context = CreateContext(character, windowHandle, featureType, statusTextBox, cancellationToken);

            if (!executor.CanExecute(context))
            {
                return FeatureResult.Failed("Feature cannot execute - conditions not met");
            }

            try
            {
                OnFeatureStarted(character.Identity.Id, featureType);
                var result = await executor.ExecuteAsync(context);
                OnFeatureCompleted(character.Identity.Id, featureType, result);

                if (result.Success)
                {
                    character.RuntimeState.MarkCompleted(featureType);
                }

                return result;
            }
            catch (OperationCanceledException)
            {
                return FeatureResult.Failed("Cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[Orchestrator] {featureType} threw exception", ex, character.Identity.Id);
                return FeatureResult.Failed(ex.Message, ex);
            }
        }

        /// <summary>
        /// Get the recommended execution order for enabled features.
        /// Features are ordered to maximize efficiency (e.g., VIP rewards first).
        /// </summary>
        private List<FeatureType> GetExecutionOrder(CharacterAggregate character)
        {
            // Define optimal execution order
            var orderedTypes = new[]
            {
                // 1. Quick rewards first (no navigation)
                FeatureType.VipPromotion,
                FeatureType.NhanHoiPhuc,
                FeatureType.NhanThuongHLVT,

                // 2. Daily exchanges
                FeatureType.DoiNangNo,
                FeatureType.DoiNangNoNL4,
                FeatureType.DoiKGDK,
                FeatureType.RutBo,

                // 3. Crafting/farming
                FeatureType.TrongNL,
                FeatureType.TrongCay,
                FeatureType.CheMatBao,
                FeatureType.HaiThuoc,

                // 4. Mini-games
                FeatureType.LatTheBai,
                FeatureType.UocNguyen,
                FeatureType.CauCa,

                // 5. Quests (can take longer)
                FeatureType.TuHanh,
                FeatureType.TriAn,
                FeatureType.TruMa,
                FeatureType.AutoThanTu,

                // 6. Combat/dungeons (longest)
                FeatureType.DauPet,
                FeatureType.AoMaThap,
                FeatureType.MeTran,
                FeatureType.AutoPhuBan,

                // 7. Background tasks
                FeatureType.BugOnline
            };

            // Filter to only enabled features
            return orderedTypes
                .Where(t => character.FeatureConfig.IsEnabled(t))
                .Where(t => !character.RuntimeState.IsCompleted(t))
                .ToList();
        }

        private ExecutionContext CreateContext(
            CharacterAggregate character,
            IntPtr windowHandle,
            FeatureType featureType,
            System.Windows.Forms.TextBox statusTextBox,
            CancellationToken cancellationToken)
        {
            return new ExecutionContext
            {
                Character = character,
                WindowHandle = windowHandle,
                Config = character.FeatureConfig,
                StatusTextBox = statusTextBox,
                CancellationToken = cancellationToken
            };
        }

        #region Event Handlers

        protected virtual void OnFeatureStarted(string characterId, FeatureType featureType)
        {
            FeatureStarted?.Invoke(this, new FeatureExecutionEventArgs
            {
                CharacterId = characterId,
                FeatureType = featureType
            });
        }

        protected virtual void OnFeatureCompleted(string characterId, FeatureType featureType, FeatureResult result)
        {
            FeatureCompleted?.Invoke(this, new FeatureExecutionEventArgs
            {
                CharacterId = characterId,
                FeatureType = featureType,
                Result = result
            });
        }

        protected virtual void OnExecutionCompleted(ExecutionSummary summary)
        {
            ExecutionCompleted?.Invoke(this, new ExecutionSummaryEventArgs { Summary = summary });
        }

        #endregion
    }

    #region Event Args and Summary Classes

    public class FeatureExecutionEventArgs : EventArgs
    {
        public string CharacterId { get; set; }
        public FeatureType FeatureType { get; set; }
        public FeatureResult Result { get; set; }
    }

    public class ExecutionSummaryEventArgs : EventArgs
    {
        public ExecutionSummary Summary { get; set; }
    }

    public class ExecutionSummary
    {
        public string CharacterId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool WasCancelled { get; set; }
        public Dictionary<FeatureType, FeatureResult> Results { get; set; } = new Dictionary<FeatureType, FeatureResult>();

        public TimeSpan Duration => EndTime - StartTime;
        public int TotalCount => Results.Count;
        public int SuccessCount => Results.Count(r => r.Value.Success);
        public int FailedCount => Results.Count(r => !r.Value.Success);
    }

    #endregion
}

