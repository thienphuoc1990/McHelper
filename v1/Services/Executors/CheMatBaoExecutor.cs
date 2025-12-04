using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for CheMatBao (Secret Manual Crafting) feature.
    /// Automates crafting secret manuals of specified type and tier.
    /// </summary>
    public class CheMatBaoExecutor : BaseFeatureExecutor
    {
        public override FeatureType Type => FeatureType.CheMatBao;

        public CheMatBaoExecutor(
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
                LogInfo("Starting CheMatBao (Secret Manual Crafting) feature", context);

                // Get configuration parameters
                string manualType = context.Config.GetParameter(Type, "Loai", "Thần Binh");
                int manualTier = int.Parse(context.Config.GetParameter(Type, "Cap", "1"));

                LogInfo($"Crafting {manualType} manual, Tier {manualTier}", context);

                // Step 1: Open crafting panel
                LogInfo("Opening crafting panel...", context);
                await OpenCraftingPanelAsync(context);

                // Step 2: Craft manuals
                LogInfo("Crafting manuals...", context);
                int craftedCount = await CraftManualsAsync(context, manualType, manualTier);

                LogInfo($"CheMatBao feature completed successfully. Crafted {craftedCount} manuals", context);
                return FeatureResult.Successful($"Crafted {craftedCount} {manualType} manuals (Tier {manualTier})");
            }
            catch (Exception ex)
            {
                LogError($"CheMatBao feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
        }

        public override bool CanExecute(ExecutionContext context)
        {
            if (!context.Character.FeatureConfig.IsEnabled(FeatureType.CheMatBao))
                return false;

            if (context.Character.RuntimeState.IsCompleted(FeatureType.CheMatBao))
                return false;

            return true;
        }

        #region Private Methods

        /// <summary>
        /// Open the secret manual crafting panel
        /// </summary>
        private async Task OpenCraftingPanelAsync(ExecutionContext context)
        {
            int maxAttempts = 3;
            int attempt = 0;

            while (attempt < maxAttempts)
            {
                attempt++;

                // Close all dialogs
                await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);

                // Open character panel
                LogInfo("Opening character panel...", context);
                await ExecutorHelpers.ClickImageByGroupAsync(_imageRecognition, _inputSimulator, "global", "nhanvat");

                // Open soul/spirit panel (hồn khí)
                LogInfo("Opening soul panel...", context);
                await ExecutorHelpers.ClickImageByGroupAsync(_imageRecognition, _inputSimulator, "mat_bao", "honkhi");

                // Wait for panel to load
                await Task.Delay(5000);

                // Open secret manual panel (mật bảo)
                LogInfo("Opening secret manual panel...", context);
                await ExecutorHelpers.ClickImageByGroupAsync(_imageRecognition, _inputSimulator, "mat_bao", "matbao");

                // Open crafting tab
                LogInfo("Opening crafting tab...", context);
                await ExecutorHelpers.ClickImageByGroupAsync(_imageRecognition, _inputSimulator, "mat_bao", "chetao");

                await Task.Delay(1000);

                // Verify panel opened
                var panelLocation = await ExecutorHelpers.FindImageByGroupAsync(_imageRecognition, "mat_bao", "chetaomatbao");
                bool panelOpened = panelLocation.HasValue;

                if (panelOpened)
                {
                    LogInfo("Crafting panel opened successfully", context);
                    return;
                }

                LogInfo($"Failed to open crafting panel, attempt {attempt}/{maxAttempts}", context);
            }

            throw new Exception("Failed to open crafting panel after maximum attempts");
        }

        /// <summary>
        /// Craft secret manuals of specified type and tier
        /// </summary>
        private async Task<int> CraftManualsAsync(ExecutionContext context, string manualType, int manualTier)
        {
            // Select tier (level/cap)
            LogInfo($"Selecting tier {manualTier}...", context);
            await SelectManualTierAsync(context, manualTier);

            // Click safe area
            await ExecutorHelpers.ClickImageByGroupAsync(_imageRecognition, _inputSimulator, "mat_bao", "clickantoan");

            // Select manual type
            LogInfo($"Selecting manual type: {manualType}...", context);
            string manualTypeKey = GetManualTypeKey(manualType);
            await ExecutorHelpers.ClickImageByGroupAsync(_imageRecognition, _inputSimulator, "mat_bao", manualTypeKey);

            // Craft loop
            int craftedCount = 0;
            int maxCraftAttempts = Constant.MaxLoopQ;

            LogInfo($"Starting craft loop (max {maxCraftAttempts} attempts)...", context);

            for (int i = 0; i < maxCraftAttempts; i++)
            {
                // Click auto-place materials
                await ExecutorHelpers.ClickImageByGroupAsync(_imageRecognition, _inputSimulator, "mat_bao", "tudongdatnguyenlieu");

                // Click craft button
                bool crafted = await ExecutorHelpers.ClickImageByGroupAsync(_imageRecognition, _inputSimulator, "mat_bao", "chetaomatbao");

                if (crafted)
                {
                    craftedCount++;

                    // Click safe area to clear any popups
                    await ExecutorHelpers.ClickImageByGroupAsync(_imageRecognition, _inputSimulator, "mat_bao", "clickantoan");

                    await Task.Delay(2000); // Wait for crafting animation

                    // Check if out of crafting attempts
                    var outOfAttemptsLocation = await ExecutorHelpers.FindImageByGroupAsync(_imageRecognition, "mat_bao", "hetluotche");
                    bool outOfAttempts = outOfAttemptsLocation.HasValue;
                    if (outOfAttempts)
                    {
                        LogInfo("Out of crafting attempts", context);
                        break;
                    }
                }
                else
                {
                    // Could not find craft button, might be finished
                    LogInfo("Craft button not found, stopping", context);
                    break;
                }
            }

            return craftedCount;
        }

        /// <summary>
        /// Select manual tier/level
        /// </summary>
        private async Task SelectManualTierAsync(ExecutionContext context, int tier)
        {
            // Click on tier header with offset based on tier level
            // Each tier is 25 pixels apart
            int yOffset = -20 + (tier * 25);

            var tierHeaderLocation = await ExecutorHelpers.FindImageByGroupAsync(
                _imageRecognition,
                "mat_bao",
                "tieudecapmatbao");

            if (tierHeaderLocation.HasValue)
            {
                var clickPoint = new Point(
                    tierHeaderLocation.Value.X + 20,
                    tierHeaderLocation.Value.Y + yOffset);

                await _inputSimulator.ClickAsync(clickPoint);
                await Task.Delay(500);
            }
            else
            {
                throw new Exception("Tier header not found");
            }
        }

        /// <summary>
        /// Get manual type key for image lookup
        /// </summary>
        private string GetManualTypeKey(string manualType)
        {
            switch (manualType)
            {
                case "Pháp Sức":
                    return "phapsuc";
                case "Vô Ưu":
                    return "vouu";
                case "Thánh Điện":
                    return "thanhdien";
                case "Hang Động":
                    return "hangdong";
                case "Đại Mạc":
                    return "daimac";
                case "Di Cảnh":
                    return "dicanh";
                case "Liệt Diễm":
                    return "lietdiem";
                case "Lang Huyệt":
                    return "langhuyet";
                case "Lạc Viên":
                    return "lacvien";
                case "Chiến Trang":
                    return "chientrang";
                case "Thần Binh":
                default:
                    return "thanbinh";
            }
        }

        #region Helper Methods

        /// <summary>
        /// Click on an image in a group (simplified version)
        /// </summary>
        // Removed ClickImageByGroupAsync - now using ExecutorHelpers.ClickImageByGroupAsync
        // Removed FindImageByGroupAsync - now using ExecutorHelpers.FindImageByGroupAsync
        // Removed FindImageByGroupLocationAsync - now using ExecutorHelpers.FindImageByGroupAsync
        // Removed GetGroupPath - now using ExecutorHelpers.GetGroupPath
        // Removed CloseAllDialogsAsync - now using ExecutorHelpers.CloseAllDialogsAsync

        #endregion

        #endregion
    }
}
