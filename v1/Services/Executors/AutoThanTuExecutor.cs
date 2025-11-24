using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for AutoThanTu (Divine Cultivation) feature.
    /// Navigates to Quyền Cô Thánh, talks to NPC, and starts divine cultivation training.
    /// TODO: Refactor to use new architecture instead of wrapping legacy GeneralFunctions.
    /// </summary>
    public class AutoThanTuExecutor : BaseFeatureExecutor
    {
        public override FeatureType Type => FeatureType.AutoThanTu;

        public AutoThanTuExecutor(
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
                LogInfo("Starting AutoThanTu (Divine Cultivation) feature", context);

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

                        // Run divine cultivation
                        generalFunctions.runAutoThanTu();
                    }
                    finally
                    {
                        // Unregister character when done
                        Helper.UnregisterRunningCharacter(legacyCharacter.ID);
                    }

                }, context.CancellationToken);

                LogInfo("AutoThanTu completed successfully", context);
                return FeatureResult.Successful("Divine cultivation completed");
            }
            catch (Exception ex)
            {
                LogError($"AutoThanTu feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
        }

        public override bool CanExecute(ExecutionContext context)
        {
            // Check if feature is enabled
            if (!context.Character.FeatureConfig.IsEnabled(FeatureType.AutoThanTu))
                return false;

            // Check if already completed today
            if (context.Character.RuntimeState.IsCompleted(FeatureType.AutoThanTu))
                return false;

            return true;
        }
    }
}
