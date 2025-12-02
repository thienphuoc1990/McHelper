using AutoVPT.Domain;
using AutoVPT.Infrastructure;
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
                await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);

                // Step 2: Open quick features list first
                LogInfo("Opening quick features list...", context);
                var quickFeatureButton = await _imageRecognition.FindImageAsync(
                    Constant.ImagePathGlobalFolder + "quickFeatureButton.png",
                    searchArea: SearchRegions.RightPanel,  // Quick features button on right (6x faster)
                    threshold: 0.8);

                if (quickFeatureButton.HasValue)
                {
                    await _inputSimulator.ClickAsync(quickFeatureButton.Value);
                    await Task.Delay(Constant.TimeShort);
                }
                else
                {
                    // Quick features list might already be open, continue
                    LogInfo("Quick features button not found, assuming list is already open", context);
                }

                // Step 3: Open Space-Time Carving panel from quick features list
                LogInfo("Opening Space-Time Carving panel...", context);
                bool panelOpened = await ExecutorHelpers.OpenFeatureFromQuickListAsync(
                    _imageRecognition, _inputSimulator, "khonggiandieukhac");

                if (!panelOpened)
                {
                    // Verify panel is not already open
                    var checkLocation = await _imageRecognition.FindImageAsync(
                        Constant.ImagePathGlobalFolder + "khonggiandieukhac_check.png",
                        searchArea: SearchRegions.DialogArea,  // Panel check in dialog area (3x faster)
                        threshold: 0.8);

                    if (!checkLocation.HasValue)
                    {
                        LogWarning("Failed to open Space-Time Carving panel. The feature may not be available or the quick features list may not be accessible.", context);
                        return FeatureResult.Failed("Failed to open Space-Time Carving panel. Please ensure the feature is available in the quick features list.");
                    }
                    else
                    {
                        LogInfo("Panel appears to be already open", context);
                        panelOpened = true;
                    }
                }

                await Task.Delay(2000); // Wait for panel to fully load

                // Step 4: Click exchange button
                LogInfo("Clicking exchange button...", context);
                var exchangeButtonLocation = await _imageRecognition.FindImageAsync(
                    Constant.ImagePathGlobalFolder + "khonggiandieukhacdoi.png",
                    searchArea: SearchRegions.DialogArea,  // 3x faster - exchange button in panel dialog
                    threshold: 0.8);

                if (!exchangeButtonLocation.HasValue)
                {
                    LogWarning("Exchange button not found", context);
                    return FeatureResult.Failed("Exchange button not found");
                }

                await _inputSimulator.ClickAsync(exchangeButtonLocation.Value);
                await Task.Delay(Constant.TimeShort);

                // Step 5: Click confirm button
                LogInfo("Confirming exchange...", context);
                var confirmButtonLocation = await _imageRecognition.FindImageAsync(
                    Constant.ImagePathGlobalFolder + "luachonco.png",
                    searchArea: SearchRegions.DialogArea,  // 3x faster - confirm button in dialog
                    threshold: 0.8);

                if (confirmButtonLocation.HasValue)
                {
                    await _inputSimulator.ClickAsync(confirmButtonLocation.Value);
                    await Task.Delay(Constant.TimeShort);
                }

                // Step 6: Close panel
                LogInfo("Closing panel...", context);
                await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);

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
        // Removed CloseAllDialogsAsync - now using ExecutorHelpers.CloseAllDialogsAsync
        // Removed OpenFeatureFromQuickListAsync - now using ExecutorHelpers.OpenFeatureFromQuickListAsync

        #endregion
    }
}
