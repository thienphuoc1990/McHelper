using AutoVPT.Domain;
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
                await CloseAllDialogsAsync(context);

                // Step 2: Open character panel
                LogInfo("Opening character panel...", context);
                var characterButtonLocation = await _imageRecognition.FindImageAsync(
                    Constant.ImagePathGlobalFolder + "nhanvat.png",
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
                bool wardrobeOpened = await ClickImageWithLoopAsync(
                    "tudo",
                    "Opening wardrobe",
                    context);

                if (!wardrobeOpened)
                {
                    LogWarning("Failed to open wardrobe", context);
                    return FeatureResult.Failed("Failed to open wardrobe");
                }

                await Task.Delay(2000); // Wait for wardrobe to fully load

                // Step 4: Click withdraw button (click all instances)
                LogInfo("Clicking withdraw button...", context);
                bool withdrawClicked = await ClickAllImagesWithLoopAsync(
                    "rutbo",
                    "Clicking withdraw button",
                    context);

                if (!withdrawClicked)
                {
                    LogWarning("Withdraw button not found", context);
                    return FeatureResult.Failed("Withdraw button not found");
                }

                // Step 5: Click withdraw reward button
                LogInfo("Clicking withdraw reward button...", context);
                await ClickImageWithLoopAsync(
                    "rutthuongbo",
                    "Clicking withdraw reward",
                    context);

                await Task.Delay(Constant.TimeShort);

                // Step 6: Click confirm button
                LogInfo("Clicking confirm button...", context);
                await ClickImageWithLoopAsync(
                    "rutboxacnhan",
                    "Confirming withdrawal",
                    context);

                await Task.Delay(Constant.TimeShort);

                // Step 7: Close panels
                LogInfo("Closing panels...", context);
                await CloseAllDialogsAsync(context);

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
        private async Task CloseAllDialogsAsync(ExecutionContext context)
        {
            // Press ESC key multiple times to close dialogs
            for (int i = 0; i < 3; i++)
            {
                await _inputSimulator.SendKeyAsync(System.Windows.Forms.Keys.Escape);
                await Task.Delay(500);
            }
        }

        /// <summary>
        /// Click an image repeatedly until it's found (with loop)
        /// </summary>
        private async Task<bool> ClickImageWithLoopAsync(string imageName, string actionDescription, ExecutionContext context)
        {
            int attempts = 0;
            int maxAttempts = Constant.MaxLoop;

            while (attempts < maxAttempts)
            {
                var imageLocation = await _imageRecognition.FindImageAsync(
                    Constant.ImagePathGlobalFolder + imageName + ".png",
                    threshold: 0.8);

                if (imageLocation.HasValue)
                {
                    LogInfo($"{actionDescription}...", context);
                    await _inputSimulator.ClickAsync(imageLocation.Value);
                    await Task.Delay(500);
                    return true;
                }

                attempts++;
                await Task.Delay(300);
            }

            return false;
        }

        /// <summary>
        /// Click all instances of an image (clickImageByGroup with clickAll=true)
        /// </summary>
        private async Task<bool> ClickAllImagesWithLoopAsync(string imageName, string actionDescription, ExecutionContext context)
        {
            bool foundAny = false;
            int attempts = 0;
            int maxAttempts = Constant.MaxLoop;

            while (attempts < maxAttempts)
            {
                var imageLocation = await _imageRecognition.FindImageAsync(
                    Constant.ImagePathGlobalFolder + imageName + ".png",
                    threshold: 0.8);

                if (imageLocation.HasValue)
                {
                    LogInfo($"{actionDescription}...", context);
                    await _inputSimulator.ClickAsync(imageLocation.Value);
                    await Task.Delay(500);
                    foundAny = true;
                    // Continue clicking until no more instances found
                }
                else if (foundAny)
                {
                    // Found some before but not now, we're done
                    return true;
                }
                else
                {
                    // Never found any
                    attempts++;
                    await Task.Delay(300);
                }
            }

            return foundAny;
        }

        #endregion
    }
}
