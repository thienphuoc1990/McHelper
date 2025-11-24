using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using System;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for DoiKGDK (Space-Time Carving Exchange) feature.
    /// Automates exchanging space-time carving rewards.
    /// REFACTORED: Native async/await implementation (no legacy dependencies).
    /// </summary>
    public class DoiKGDKExecutor : BaseFeatureExecutor
    {
        public override FeatureType Type => FeatureType.DoiKGDK;

        public DoiKGDKExecutor(
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
                LogInfo("Starting DoiKGDK (Space-Time Exchange) feature", context);

                // Step 1: Close all dialogs first
                LogInfo("Closing all dialogs...", context);
                await CloseAllDialogsAsync(context);

                // Step 2: Open Space-Time Carving panel
                LogInfo("Opening Space-Time Carving panel...", context);
                bool panelOpened = await OpenFeatureFromQuickListAsync("khonggiandieukhac", context);

                if (!panelOpened)
                {
                    LogWarning("Failed to open Space-Time Carving panel", context);
                    return FeatureResult.Failed("Failed to open panel");
                }

                await Task.Delay(2000); // Wait for panel to fully load

                // Step 3: Click exchange button
                LogInfo("Clicking exchange button...", context);
                var exchangeButtonLocation = await _imageRecognition.FindImageAsync(
                    Constant.ImagePathGlobalFolder + "khonggiandieukhacdoi.png",
                    threshold: 0.8);

                if (!exchangeButtonLocation.HasValue)
                {
                    LogWarning("Exchange button not found", context);
                    return FeatureResult.Failed("Exchange button not found");
                }

                await _inputSimulator.ClickAsync(exchangeButtonLocation.Value);
                await Task.Delay(Constant.TimeShort);

                // Step 4: Click confirm button
                LogInfo("Confirming exchange...", context);
                var confirmButtonLocation = await _imageRecognition.FindImageAsync(
                    Constant.ImagePathGlobalFolder + "luachonco.png",
                    threshold: 0.8);

                if (confirmButtonLocation.HasValue)
                {
                    await _inputSimulator.ClickAsync(confirmButtonLocation.Value);
                    await Task.Delay(Constant.TimeShort);
                }

                // Step 5: Close panel
                LogInfo("Closing panel...", context);
                await CloseAllDialogsAsync(context);

                LogInfo("DoiKGDK completed successfully", context);
                return FeatureResult.Successful("Space-time exchange completed");
            }
            catch (Exception ex)
            {
                LogError($"DoiKGDK feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
        }

        public override bool CanExecute(ExecutionContext context)
        {
            // Check if feature is enabled
            if (!context.Character.FeatureConfig.IsEnabled(FeatureType.DoiKGDK))
                return false;

            // Check if already completed today
            if (context.Character.RuntimeState.IsCompleted(FeatureType.DoiKGDK))
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
        /// Open a feature from the quick features list by scrolling and clicking
        /// </summary>
        private async Task<bool> OpenFeatureFromQuickListAsync(string featureName, ExecutionContext context)
        {
            int loop = 0;

            while (loop < Constant.MaxLoopShort)
            {
                // Check if feature panel is already open
                var featureCheckLocation = await _imageRecognition.FindImageAsync(
                    Constant.ImagePathGlobalFolder + featureName + "_check.png",
                    threshold: 0.8);

                if (featureCheckLocation.HasValue)
                {
                    LogInfo($"Feature panel '{featureName}' is open", context);
                    return true;
                }

                // Scroll to top of quick features list first
                while (true)
                {
                    var upArrowLocation = await _imageRecognition.FindImageAsync(
                        Constant.ImagePathGlobalFolder + "quickFeatureListUpArrow.png",
                        threshold: 0.8);

                    var featureLocation = await _imageRecognition.FindImageAsync(
                        Constant.ImagePathGlobalFolder + featureName + ".png",
                        threshold: 0.8);

                    if (!upArrowLocation.HasValue || featureLocation.HasValue)
                    {
                        break; // Reached top or found feature
                    }

                    LogInfo("Scrolling to top of quick feature list", context);
                    await _inputSimulator.ClickAsync(upArrowLocation.Value);
                    await Task.Delay(Constant.TimeShort);
                }

                // Scroll down to find the feature
                while (true)
                {
                    var featureLocation = await _imageRecognition.FindImageAsync(
                        Constant.ImagePathGlobalFolder + featureName + ".png",
                        threshold: 0.8);

                    if (featureLocation.HasValue)
                    {
                        // Found the feature, click it
                        LogInfo($"Found feature '{featureName}', clicking...", context);
                        await _inputSimulator.ClickAsync(featureLocation.Value);
                        await Task.Delay(Constant.TimeMedium);
                        break;
                    }

                    var downArrowLocation = await _imageRecognition.FindImageAsync(
                        Constant.ImagePathGlobalFolder + "quickFeatureListDownArrow.png",
                        threshold: 0.8);

                    if (!downArrowLocation.HasValue)
                    {
                        // Reached bottom without finding feature
                        LogWarning($"Feature '{featureName}' not found in quick list", context);
                        return false;
                    }

                    LogInfo("Scrolling down to find feature", context);
                    await _inputSimulator.ClickAsync(downArrowLocation.Value);
                    await Task.Delay(Constant.TimeMedium);
                }

                loop++;
            }

            return false;
        }

        #endregion
    }
}
