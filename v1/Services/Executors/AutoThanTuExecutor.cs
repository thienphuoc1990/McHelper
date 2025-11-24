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
    /// NOTE: Uses legacy AutoFeatures for navigation (moveToMap, moveToNPC, bay).
    /// TODO: Refactor navigation to native async when navigation service is available.
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

                // Hybrid approach: Uses AutoFeatures for navigation
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

                    // Step 2: Navigate to Quyền Cô Thánh
                    LogInfo("Navigating to Quyền Cô Thánh...", context);
                    if (!autoFeatures.moveToMap("quyencothanh", 7, -18))
                    {
                        throw new Exception("Failed to navigate to Quyền Cô Thánh");
                    }

                    // Step 3: Fly up
                    LogInfo("Flying up...", context);
                    autoFeatures.bay();

                    // Step 4: Move to divine cultivation NPC
                    LogInfo("Moving to divine cultivation NPC...", context);
                    if (!autoFeatures.moveToNPC("thanhchuquyenco", "autothantu"))
                    {
                        throw new Exception("Failed to reach divine cultivation NPC");
                    }

                    // Step 5: Fly down
                    LogInfo("Flying down...", context);
                    autoFeatures.bayXuong();

                    // Step 6: Talk to NPC
                    LogInfo("Talking to NPC...", context);
                    if (!autoFeatures.talkToNPC("thanhchuquyenco"))
                    {
                        throw new Exception("Failed to talk to NPC");
                    }

                    // Step 7: Start divine cultivation
                    LogInfo("Starting divine cultivation...", context);
                    StartDivineCultivation(autoFeatures);

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

        /// <summary>
        /// Start divine cultivation by clicking the necessary buttons
        /// </summary>
        private void StartDivineCultivation(AutoFeatures autoFeatures)
        {
            // Click divine cultivation option
            autoFeatures.clickImageByGroup("global", "autothantu", false, true);

            // Click start button
            autoFeatures.clickImageByGroup("global", "batdauautotuhanh", false, false);

            // Click confirm
            autoFeatures.clickImageByGroup("global", "luachonco", false, true);
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
