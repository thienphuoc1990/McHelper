using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using System;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for RutBo (Equipment Withdrawal) feature.
    /// Automates withdrawing equipment rewards from wardrobe.
    /// </summary>
    public class RutBoExecutor : BaseFeatureExecutor
    {
        public override FeatureType Type => FeatureType.RutBo;

        public RutBoExecutor(
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
                LogInfo("Starting RutBo (Equipment Withdrawal) feature", context);

                // Call legacy implementation via wrapper
                await Task.Run(() =>
                {
                    // Convert to legacy Character object
                    var legacyCharacter = CharacterAdapter.ToLegacy(context.Character);

                    // Register character for Stop All functionality
                    Helper.RegisterRunningCharacter(legacyCharacter);

                    try
                    {
                        // Create AutoFeatures instance
                        var autoFeatures = new AutoFeatures(
                            context.WindowHandle,
                            context.Character.Identity.Id,
                            context.StatusTextBox,
                            legacyCharacter
                        );

                        // Create GeneralFunctions instance
                        var generalFunctions = new GeneralFunctions(
                            context.WindowHandle,
                            legacyCharacter,
                            context.StatusTextBox
                        );

                        // Execute the feature
                        generalFunctions.rutBo();
                    }
                    finally
                    {
                        // Unregister character when done
                        Helper.UnregisterRunningCharacter(legacyCharacter.ID);
                    }

                }, context.CancellationToken);

                LogInfo("RutBo completed successfully", context);
                return FeatureResult.Successful("Equipment withdrawn successfully");
            }
            catch (Exception ex)
            {
                LogError($"RutBo feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
        }

        public override bool CanExecute(ExecutionContext context)
        {
            // Check if feature is enabled
            if (!context.Character.FeatureConfig.IsEnabled(FeatureType.RutBo))
                return false;

            // Check if already completed today
            if (context.Character.RuntimeState.IsCompleted(FeatureType.RutBo))
                return false;

            return true;
        }
    }
}
