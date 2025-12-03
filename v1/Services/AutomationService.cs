using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Objects;
using AutoVPT.Repositories;
using AutoVPT.Services.Executors;
using AutoVPT.DependencyInjection;
using AutoVPT.Infrastructure;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Services
{
    /// <summary>
    /// Main automation service implementation.
    /// Coordinates feature execution and manages character automation lifecycle.
    /// </summary>
    public class AutomationService : IAutomationService
    {
        private readonly ICharacterRepository _characterRepo;
        private readonly IWindowManager _windowManager;
        private readonly ILogger _logger;
        private readonly Dictionary<FeatureType, IFeatureExecutor> _executors;
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _runningTasks;

        public AutomationService(
            ICharacterRepository characterRepo,
            IWindowManager windowManager,
            ILogger logger,
            IEnumerable<IFeatureExecutor> executors)
        {
            _characterRepo = characterRepo ?? throw new ArgumentNullException(nameof(characterRepo));
            _windowManager = windowManager ?? throw new ArgumentNullException(nameof(windowManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _executors = new Dictionary<FeatureType, IFeatureExecutor>();
            if (executors != null)
            {
                foreach (var executor in executors)
                {
                    _executors[executor.Type] = executor;
                }
            }

            _runningTasks = new ConcurrentDictionary<string, CancellationTokenSource>();
        }

        public async Task<AutomationResult> RunDailyTasksAsync(string characterId, CancellationToken ct = default)
        {
            var startTime = DateTime.Now;
            int completed = 0;
            int failed = 0;

            try
            {
                _logger.LogInfo($"Starting daily tasks for {characterId}", characterId);

                // Load character
                var legacyChar = _characterRepo.GetById(characterId);
                if (legacyChar == null)
                {
                    return AutomationResult.Failed($"Character {characterId} not found");
                }

                // Convert to aggregate
                var character = CharacterAdapter.ToAggregate(legacyChar);

                // Check and reset daily status
                character.BeforeFeatureExecution();

                // Find game window
                var windowHandle = _windowManager.FindWindow(characterId);
                if (windowHandle == IntPtr.Zero)
                {
                    _logger.LogWarning($"Game window not found for {characterId}", characterId);
                    return AutomationResult.Failed("Game window not found");
                }

                // Start character automation
                character.RuntimeState.Start();

                // Execute all pending features
                var pendingFeatures = character.GetPendingFeatures().ToList();
                _logger.LogInfo($"Found {pendingFeatures.Count} pending features", characterId);

                foreach (var feature in pendingFeatures)
                {
                    if (ct.IsCancellationRequested)
                        break;

                    var result = await ExecuteFeatureInternal(character, feature, windowHandle, ct);

                    if (result.Success)
                    {
                        completed++;
                        character.CompleteFeature(feature);
                        _logger.LogInfo($"Completed {feature}", characterId);
                    }
                    else
                    {
                        failed++;
                        character.FailFeature(feature);
                        _logger.LogError($"Failed {feature}: {result.Message}", result.Error, characterId);
                    }

                    // Save progress
                    var updatedLegacy = CharacterAdapter.ToLegacy(character);
                    _characterRepo.Save(updatedLegacy);
                }

                var duration = DateTime.Now - startTime;
                _logger.LogInfo($"Daily tasks completed: {completed} succeeded, {failed} failed", characterId);

                return AutomationResult.Successful(completed, duration);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running daily tasks for {characterId}", ex);
                return AutomationResult.Failed(ex.Message);
            }
        }

        public async Task<AutomationResult> RunFeatureAsync(string characterId, FeatureType feature, CancellationToken ct = default)
        {
            try
            {
                _logger.LogInfo($"Starting feature {feature} for {characterId}", characterId);

                var legacyChar = _characterRepo.GetById(characterId);
                if (legacyChar == null)
                {
                    return AutomationResult.Failed($"Character {characterId} not found");
                }

                var character = CharacterAdapter.ToAggregate(legacyChar);
                var windowHandle = _windowManager.FindWindow(characterId);

                if (windowHandle == IntPtr.Zero)
                {
                    return AutomationResult.Failed("Game window not found");
                }

                var result = await ExecuteFeatureInternal(character, feature, windowHandle, ct);

                if (result.Success)
                {
                    character.CompleteFeature(feature);
                }
                else
                {
                    character.FailFeature(feature);
                }

                // Save status
                var updatedLegacy = CharacterAdapter.ToLegacy(character);
                _characterRepo.Save(updatedLegacy);

                return result.Success
                    ? AutomationResult.Successful(1, TimeSpan.Zero)
                    : AutomationResult.Failed(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error running feature {feature} for {characterId}", ex);
                return AutomationResult.Failed(ex.Message);
            }
        }

        public async Task<CharacterRuntimeState> GetStatusAsync(string characterId)
        {
            return await Task.Run(() =>
            {
                var legacyChar = _characterRepo.GetById(characterId);
                if (legacyChar == null)
                    return null;

                var character = CharacterAdapter.ToAggregate(legacyChar);
                return character.RuntimeState;
            });
        }

        public async Task StopAsync(string characterId)
        {
            if (_runningTasks.TryRemove(characterId, out var cts))
            {
                cts.Cancel();
                _logger.LogInfo($"Stopped automation for {characterId}", characterId);
            }

            // Update character status
            await Task.Run(() =>
            {
                var legacyChar = _characterRepo.GetById(characterId);
                if (legacyChar != null)
                {
                    var character = CharacterAdapter.ToAggregate(legacyChar);
                    character.RuntimeState.Stop();

                    var updatedLegacy = CharacterAdapter.ToLegacy(character);
                    _characterRepo.Save(updatedLegacy);
                }
            });
        }

        public async Task StopAllAsync()
        {
            var stopTasks = _runningTasks.Keys.Select(id => StopAsync(id));
            await Task.WhenAll(stopTasks);
            _logger.LogInfo("All automations stopped");
        }

        private async Task<FeatureResult> ExecuteFeatureInternal(
            CharacterAggregate character,
            FeatureType feature,
            IntPtr windowHandle,
            CancellationToken ct)
        {
            // Try to get executor from registered list first
            IFeatureExecutor executor = null;
            if (_executors.ContainsKey(feature))
            {
                executor = _executors[feature];
            }
            else
            {
                // Create executor on-demand using window services
                executor = CreateExecutorForFeature(feature, windowHandle, character);
                if (executor == null)
                {
                    return FeatureResult.Failed($"No executor found for {feature}");
                }
            }

            var config = character.FeatureConfig.GetConfig(feature);

            var context = new ExecutionContext
            {
                Character = character,
                WindowHandle = windowHandle,
                Config = config,
                CancellationToken = ct
            };

            if (!executor.CanExecute(context))
            {
                return FeatureResult.Failed($"Cannot execute {feature} - preconditions not met");
            }

            try
            {
                return await executor.ExecuteAsync(context);
            }
            catch (Exception ex)
            {
                return FeatureResult.Failed($"Exception during {feature} execution", ex);
            }
        }

        /// <summary>
        /// Create executor for a feature on-demand using window services
        /// </summary>
        private IFeatureExecutor CreateExecutorForFeature(FeatureType feature, IntPtr windowHandle, CharacterAggregate character)
        {
            try
            {
                // Get character for window services
                var legacyChar = _characterRepo.GetById(character.Id);
                if (legacyChar == null)
                    return null;

                // Create window services
                var windowServices = ServiceContainer.CreateWindowServices(windowHandle, legacyChar);

                // Create image recognition and input simulator
                var imageRecognition = windowServices.GetService<IImageRecognition>();
                var inputSimulator = windowServices.GetService<IInputSimulator>();
                var vipLevel = character.Identity.VipLevel;

                // Create executor based on feature type
                switch (feature)
                {
                    case FeatureType.DoiNangNo:
                        return new DoiNangNoExecutor(imageRecognition, inputSimulator, _logger);
                    case FeatureType.TrongNL:
                        return new TrongNLExecutor(imageRecognition, inputSimulator, _logger);
                    case FeatureType.TriAn:
                        return new TriAnExecutor(imageRecognition, inputSimulator, _logger, vipLevel);
                    case FeatureType.CheMatBao:
                        return new CheMatBaoExecutor(imageRecognition, inputSimulator, _logger);
                    case FeatureType.AutoPhuBan:
                        return new AutoPhuBanExecutor(imageRecognition, inputSimulator, _logger);
                    case FeatureType.TruMa:
                        return new TruMaExecutor(imageRecognition, inputSimulator, _logger);
                    case FeatureType.VipPromotion:
                        return new VipPromotionExecutor(imageRecognition, inputSimulator, _logger);
                    case FeatureType.NhanThuongHLVT:
                        return new NhanThuongHLVTExecutor(imageRecognition, inputSimulator, _logger);
                    case FeatureType.RutBo:
                        return new RutBoExecutor(imageRecognition, inputSimulator, _logger);
                    case FeatureType.DoiKGDK:
                        return new DoiKGDKExecutor(imageRecognition, inputSimulator, _logger);
                    case FeatureType.NhanHoiPhuc:
                        return new NhanHoiPhucExecutor(imageRecognition, inputSimulator, _logger);
                    case FeatureType.TuHanh:
                        return new TuHanhExecutor(imageRecognition, inputSimulator, _logger);
                    case FeatureType.AutoThanTu:
                        return new AutoThanTuExecutor(imageRecognition, inputSimulator, _logger);
                    default:
                        return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating executor for {feature}", ex, character.Id);
                return null;
            }
        }
    }
}
