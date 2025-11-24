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
    /// NOTE: Uses legacy AutoFeatures for NVHN quest navigation (openQuestByNVHN).
    /// TODO: Refactor navigation to native async when quest system is available.
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

                // Hybrid approach: Uses AutoFeatures for NVHN navigation
                await Task.Run(() =>
                {
                    var legacyCharacter = CharacterAdapter.ToLegacy(context.Character);
                    var autoFeatures = new AutoFeatures(
                        context.WindowHandle,
                        context.Character.Identity.Id,
                        context.StatusTextBox,
                        legacyCharacter
                    );

                    // Step 1: Close all dialogs
                    LogInfo("Closing all dialogs...", context);
                    autoFeatures.closeAllDialog();

                    // Step 2: Fly up
                    LogInfo("Flying up...", context);
                    autoFeatures.bay();

                    // Step 3: Use NVHN quest helper to navigate to cultivation NPC
                    LogInfo("Using NVHN quest helper to navigate...", context);
                    while (!autoFeatures.isTalkWithNPC("truonglaovouutoc"))
                    {
                        autoFeatures.openQuestByNVHN("tuhanh");
                    }

                    // Step 4: Start auto cultivation
                    LogInfo("Starting auto cultivation...", context);
                    StartAutoCultivation(autoFeatures);

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

        /// <summary>
        /// Start auto cultivation by clicking the necessary buttons
        /// </summary>
        private void StartAutoCultivation(AutoFeatures autoFeatures)
        {
            while (!autoFeatures.findImageByGroup("global", "autotuhanh_check", false, false))
            {
                autoFeatures.clickImageByGroup("global", "autotuhanh", false, true);
                autoFeatures.clickImageByGroup("global", "batdauautotuhanh", false, false);
                autoFeatures.clickImageByGroup("global", "luachonco", false, true);
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
