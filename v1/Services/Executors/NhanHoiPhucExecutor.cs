using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for NhanHoiPhuc (Receive Recovery Rewards) feature.
    /// Automates collecting recovery rewards from daily quest system.
    /// REFACTORED: Native async/await implementation (no legacy dependencies).
    /// </summary>
    public class NhanHoiPhucExecutor : BaseFeatureExecutor
    {
        public override FeatureType Type => FeatureType.NhanHoiPhuc;

        public NhanHoiPhucExecutor(
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
                LogInfo("Starting NhanHoiPhuc (Recovery Rewards) feature", context);

                // Step 1: Close all dialogs first
                LogInfo("Closing all dialogs...", context);
                await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);

                // Step 2: Open daily quest panel
                LogInfo("Opening daily quest panel...", context);
                bool dailyQuestOpened = await ExecutorHelpers.WaitForPanelAsync(
                    _imageRecognition,
                    _inputSimulator,
                    Constant.ImagePathGlobalFolder + "nhiemvuhangngay.png",
                    Constant.ImagePathGlobalFolder + "nhiemvuhangngay_check.png");

                if (!dailyQuestOpened)
                {
                    LogWarning("Failed to open daily quest panel", context);
                    return FeatureResult.Failed("Failed to open daily quest panel");
                }

                // Step 3: Open recovery panel
                LogInfo("Opening recovery panel...", context);
                bool recoveryPanelOpened = await ExecutorHelpers.WaitForPanelAsync(
                    _imageRecognition,
                    _inputSimulator,
                    Constant.ImagePathGlobalFolder + "nvhn_hoiphuc.png",
                    Constant.ImagePathGlobalFolder + "nvhn_hoiphuc_check.png");

                if (!recoveryPanelOpened)
                {
                    LogWarning("Failed to open recovery panel", context);
                    return FeatureResult.Failed("Failed to open recovery panel");
                }

                // Step 4: Collect recovery rewards for various activities
                int collectedCount = 0;

                // Nhiệm vụ nghề (Profession quest recovery)
                if (await CollectRecoveryRewardAsync("nvhp_nhiemvunghe", "profession quest", context))
                    collectedCount++;

                // Tu luyện pet (Pet training recovery)
                if (await CollectRecoveryRewardAsync("nvhp_tuluyenpet", "pet training", context))
                    collectedCount++;

                // Trị an (Gratitude quest recovery)
                if (await CollectRecoveryRewardAsync("nvhp_trian", "gratitude quest", context))
                    collectedCount++;

                // Trừ ma (Monster hunting recovery)
                if (await CollectRecoveryRewardAsync("nvhp_truma", "monster hunting", context))
                    collectedCount++;

                // Treo thưởng (Bounty recovery)
                if (await CollectRecoveryRewardAsync("nvhp_treothuong", "bounty", context))
                    collectedCount++;

                // Step 5: Close panels
                LogInfo("Closing panels...", context);
                await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);

                LogInfo($"NhanHoiPhuc completed successfully. Collected {collectedCount} recovery rewards", context);
                return FeatureResult.Successful($"Collected {collectedCount} recovery rewards");
            }
            catch (Exception ex)
            {
                LogError($"NhanHoiPhuc feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
        }

        public override bool CanExecute(ExecutionContext context)
        {
            // Check if feature is enabled
            if (!context.Character.FeatureConfig.IsEnabled(FeatureType.NhanHoiPhuc))
                return false;

            // Check if already completed today
            if (context.Character.RuntimeState.IsCompleted(FeatureType.NhanHoiPhuc))
                return false;

            return true;
        }

        #region Private Methods

        /// <summary>
        /// Close all open dialogs by pressing ESC key
        /// </summary>
        // Removed CloseAllDialogsAsync - now using ExecutorHelpers.CloseAllDialogsAsync
        // Removed WaitForPanelAsync - now using ExecutorHelpers.WaitForPanelAsync

        /// <summary>
        /// Collect recovery reward for a specific activity
        /// </summary>
        private async Task<bool> CollectRecoveryRewardAsync(string imageName, string activityDescription, ExecutionContext context)
        {
            var imageLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathGlobalFolder + imageName + ".png",
                threshold: 0.8);

            if (imageLocation.HasValue)
            {
                LogInfo($"Collecting recovery reward for {activityDescription}...", context);

                // Click with offset: +470 X, -10 Y (same as legacy code)
                var clickPoint = new Point(imageLocation.Value.X + 470, imageLocation.Value.Y - 10);
                await _inputSimulator.ClickAsync(clickPoint);
                await Task.Delay(Constant.TimeShort);

                return true;
            }

            return false;
        }

        #endregion
    }
}
