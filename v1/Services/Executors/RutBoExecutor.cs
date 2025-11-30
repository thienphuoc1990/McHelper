using AutoVPT.Domain;
using AutoVPT.Infrastructure;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using System;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for RutBo (Equipment Withdrawal) feature.
    /// Automates withdrawing equipment rewards from the wardrobe.
    /// REFACTORED: Native async/await implementation (no legacy dependencies).
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

                // Step 1: Close all dialogs first
                LogInfo("Closing all dialogs...", context);
                await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);

                // Step 2: Open character panel
                LogInfo("Opening character panel...", context);
                var characterButtonLocation = await _imageRecognition.FindImageAsync(
                    Constant.ImagePathGlobalFolder + "nhanvat.png",
                    searchArea: SearchRegions.BottomRight,  // 12x faster - character button in bottom UI
                    threshold: 0.8);

                if (!characterButtonLocation.HasValue)
                {
                    LogWarning("Character button not found", context);
                    return FeatureResult.Failed("Character button not found");
                }

                await _inputSimulator.ClickAsync(characterButtonLocation.Value);
                await Task.Delay(Constant.TimeShort);

                // Step 3: Open wardrobe
                LogInfo("Opening wardrobe...", context);
                bool wardrobeOpened = await ExecutorHelpers.ClickImageWithLoopAsync(
                    _imageRecognition,
                    _inputSimulator,
                    Constant.ImagePathGlobalFolder + "tudo.png");

                if (!wardrobeOpened)
                {
                    LogWarning("Failed to open wardrobe", context);
                    return FeatureResult.Failed("Failed to open wardrobe");
                }

                await Task.Delay(2000); // Wait for wardrobe to fully load

                // Step 4: Click withdraw button (click all instances)
                LogInfo("Clicking withdraw button...", context);
                bool withdrawClicked = await ExecutorHelpers.ClickAllImagesWithLoopAsync(
                    _imageRecognition,
                    _inputSimulator,
                    Constant.ImagePathGlobalFolder + "rutbo.png");

                if (!withdrawClicked)
                {
                    LogWarning("Withdraw button not found", context);
                    return FeatureResult.Failed("Withdraw button not found");
                }

                // Step 5: Click withdraw reward button
                LogInfo("Clicking withdraw reward button...", context);
                await ExecutorHelpers.ClickImageWithLoopAsync(
                    _imageRecognition,
                    _inputSimulator,
                    Constant.ImagePathGlobalFolder + "rutthuongbo.png");

                await Task.Delay(Constant.TimeShort);

                // Step 6: Click confirm button
                LogInfo("Clicking confirm button...", context);
                await ExecutorHelpers.ClickImageWithLoopAsync(
                    _imageRecognition,
                    _inputSimulator,
                    Constant.ImagePathGlobalFolder + "rutboxacnhan.png");

                await Task.Delay(Constant.TimeShort);

                // Step 7: Close panels
                LogInfo("Closing panels...", context);
                await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);

                LogInfo("RutBo completed successfully", context);
                return FeatureResult.Successful("Equipment withdrawal completed");
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

        #region Private Methods

        /// <summary>
        /// Close all open dialogs by pressing ESC key
        /// </summary>
        // Removed CloseAllDialogsAsync - now using ExecutorHelpers.CloseAllDialogsAsync
        // Removed ClickImageWithLoopAsync - now using ExecutorHelpers.ClickImageWithLoopAsync
        // Removed ClickAllImagesWithLoopAsync - now using ExecutorHelpers.ClickAllImagesWithLoopAsync

        #endregion
    }
}
