using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for TuHanh (Cultivation Quest) feature.
    /// Uses NVHN quest helper to navigate to and start cultivation training.
    /// TODO: Refactor to use new architecture instead of wrapping legacy GeneralFunctions.
    /// </summary>
    public class TuHanhExecutor : BaseFeatureExecutor
    {
        public override FeatureType Type => FeatureType.TuHanh;

        public TuHanhExecutor(
            IImageRecognition imageRecognition,
            IInputSimulator inputSimulator,
            ILogger logger)
            : base(imageRecognition, inputSimulator, logger)
        {
        }

        public override async Task<FeatureResult> ExecuteAsync(ExecutionContext context)
        {
            try
            {
                LogInfo("Starting TuHanh (Cultivation Quest) feature", context);

                // Call legacy implementation
                // TODO: Replace with native async implementation
                await Task.Run(() =>
                {
                    // Convert to legacy Character object
                    var legacyCharacter = CharacterAdapter.ToLegacy(context.Character);

                    // Register character for Stop All functionality
                    Helper.RegisterRunningCharacter(legacyCharacter);

                    try
                    {
                        // Create GeneralFunctions instance for legacy code
                        var generalFunctions = new GeneralFunctions(
                            context.WindowHandle,
                            legacyCharacter,
                            context.StatusTextBox
                        );

                        // Run cultivation quest using NVHN helper
                        generalFunctions.runAutoTuHanhByNVHN();
                    }
                    finally
                    {
                        // Unregister character when done
                        Helper.UnregisterRunningCharacter(legacyCharacter.ID);
                    }

                }, context.CancellationToken);

                LogInfo("TuHanh completed successfully", context);
                return FeatureResult.Successful("Cultivation quest completed");
            }
            catch (Exception ex)
            {
                LogError($"TuHanh feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
        }

        public override bool CanExecute(ExecutionContext context)
        {
            // Check if feature is enabled
            if (!context.Character.FeatureConfig.IsEnabled(FeatureType.TuHanh))
                return false;

            // Check if already completed today
            if (context.Character.RuntimeState.IsCompleted(FeatureType.TuHanh))
                return false;

            return true;
        }
    }
}
