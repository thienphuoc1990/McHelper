using AutoVPT.Domain;
using AutoVPT.Infrastructure;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using System;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for VipPromotion (VIP Rewards) feature.
    /// Automates collecting daily VIP rewards from the VIP panel.
    /// </summary>
    public class VipPromotionExecutor : BaseFeatureExecutor
    {
        public override FeatureType Type => FeatureType.VipPromotion;

        public VipPromotionExecutor(
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
                LogInfo("Starting VipPromotion (VIP Rewards) feature", context);

                // Step 1: Close all dialogs first
                LogInfo("Closing all dialogs...", context);
                await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);

                // Step 2: Open VIP panel
                LogInfo("Opening VIP panel...", context);
                var vipButtonLocation = await _imageRecognition.FindImageAsync(
                    Constant.ImagePathGlobalFolder + "vip.png",
                    searchArea: SearchRegions.TopRight,  // 16x faster - VIP button is top-right
                    threshold: 0.8);

                if (!vipButtonLocation.HasValue)
                {
                    LogWarning("VIP button not found", context);
                    return FeatureResult.Failed("VIP button not found");
                }

                await _inputSimulator.ClickAsync(vipButtonLocation.Value);
                await Task.Delay(Constant.TimeShort);

                // Step 3: Click receive button multiple times to collect rewards
                LogInfo("Collecting VIP rewards (main section)...", context);
                string nhanVipImagePath = Constant.ImagePathGlobalFolder + "nhanvip.png";

                for (int i = 0; i < 4; i++)
                {
                    var nhanVipLocation = await _imageRecognition.FindImageAsync(
                        nhanVipImagePath,
                        searchArea: SearchRegions.DialogArea,  // 3x faster - receive button in VIP dialog
                        threshold: 0.8);

                    if (nhanVipLocation.HasValue)
                    {
                        await _inputSimulator.ClickAsync(nhanVipLocation.Value);
                        await Task.Delay(500);
                    }
                }

                // Step 4: Scroll down to access more rewards
                LogInfo("Scrolling down for additional rewards...", context);
                string xuongVipImagePath = Constant.ImagePathGlobalFolder + "xuongvip.png";

                for (int i = 0; i < 15; i++)
                {
                    var xuongVipLocation = await _imageRecognition.FindImageAsync(
                        xuongVipImagePath,
                        searchArea: SearchRegions.DialogArea,  // 3x faster - scroll button in VIP dialog
                        threshold: 0.8);

                    if (xuongVipLocation.HasValue)
                    {
                        await _inputSimulator.ClickAsync(xuongVipLocation.Value);
                        await Task.Delay(300);
                    }
                    else
                    {
                        // Button not found, might have reached the end
                        break;
                    }
                }

                // Step 5: Collect additional rewards after scrolling
                LogInfo("Collecting additional VIP rewards...", context);
                for (int i = 0; i < 2; i++)
                {
                    var nhanVipLocation = await _imageRecognition.FindImageAsync(
                        nhanVipImagePath,
                        searchArea: SearchRegions.DialogArea,  // 3x faster - receive button in VIP dialog
                        threshold: 0.8);

                    if (nhanVipLocation.HasValue)
                    {
                        await _inputSimulator.ClickAsync(nhanVipLocation.Value);
                        await Task.Delay(500);
                    }
                }

                // Step 6: Close VIP panel
                LogInfo("Closing VIP panel...", context);
                await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);

                LogInfo("VipPromotion completed successfully", context);
                return FeatureResult.Successful("VIP rewards collected");
            }
            catch (Exception ex)
            {
                LogError($"VipPromotion feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
        }

        public override bool CanExecute(ExecutionContext context)
        {
            // Check if feature is enabled
            if (!context.Character.FeatureConfig.IsEnabled(FeatureType.VipPromotion))
                return false;

            // Check if already completed today
            if (context.Character.RuntimeState.IsCompleted(FeatureType.VipPromotion))
                return false;

            return true;
        }

        #region Private Methods

        /// <summary>
        /// Close all open dialogs by pressing ESC key
        /// </summary>
        // Removed CloseAllDialogsAsync - now using ExecutorHelpers.CloseAllDialogsAsync

        #endregion
    }
}
