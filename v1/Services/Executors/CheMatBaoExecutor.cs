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
                string manualType = context.Config.GetParameter("Loai", "Thần Binh");
                int manualTier = int.Parse(context.Config.GetParameter("Cap", "1"));

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
                await CloseAllDialogsAsync(context);

                // Open character panel
                LogInfo("Opening character panel...", context);
                await ClickImageByGroupAsync(context, "global", "nhanvat");

                // Open soul/spirit panel (hồn khí)
                LogInfo("Opening soul panel...", context);
                await ClickImageByGroupAsync(context, "mat_bao", "honkhi");

                // Wait for panel to load
                await Task.Delay(5000);

                // Open secret manual panel (mật bảo)
                LogInfo("Opening secret manual panel...", context);
                await ClickImageByGroupAsync(context, "mat_bao", "matbao", useCache: true);

                // Open crafting tab
                LogInfo("Opening crafting tab...", context);
                await ClickImageByGroupAsync(context, "mat_bao", "chetao", useCache: true);

                await Task.Delay(1000);

                // Verify panel opened
                bool panelOpened = await FindImageByGroupAsync(context, "mat_bao", "chetaomatbao");

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
            await ClickImageByGroupAsync(context, "mat_bao", "clickantoan");

            // Select manual type
            LogInfo($"Selecting manual type: {manualType}...", context);
            string manualTypeKey = GetManualTypeKey(manualType);
            await ClickImageByGroupAsync(context, "mat_bao", manualTypeKey);

            // Craft loop
            int craftedCount = 0;
            int maxCraftAttempts = Constant.MaxLoopQ;

            LogInfo($"Starting craft loop (max {maxCraftAttempts} attempts)...", context);

            for (int i = 0; i < maxCraftAttempts; i++)
            {
                // Click auto-place materials
                await ClickImageByGroupAsync(context, "mat_bao", "tudongdatnguyenlieu");

                // Click craft button
                bool crafted = await ClickImageByGroupAsync(context, "mat_bao", "chetaomatbao");

                if (crafted)
                {
                    craftedCount++;

                    // Click safe area to clear any popups
                    await ClickImageByGroupAsync(context, "mat_bao", "clickantoan");

                    await Task.Delay(2000); // Wait for crafting animation

                    // Check if out of crafting attempts
                    bool outOfAttempts = await FindImageByGroupAsync(context, "mat_bao", "hetluotche");
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

            var tierHeaderLocation = await FindImageByGroupLocationAsync(
                context,
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
        private async Task<bool> ClickImageByGroupAsync(
            ExecutionContext context,
            string group,
            string imageName,
            bool useCache = false,
            bool waitAfter = false)
        {
            // Build path using group folder constants
            string groupPath = GetGroupPath(group);
            string imagePath = groupPath + imageName + ".png";

            var location = await _imageRecognition.FindImageAsync(imagePath, threshold: 0.8);

            if (location.HasValue)
            {
                await _inputSimulator.ClickAsync(location.Value);

                if (waitAfter)
                {
                    await Task.Delay(1000);
                }
                else
                {
                    await Task.Delay(200);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Find an image in a group
        /// </summary>
        private async Task<bool> FindImageByGroupAsync(
            ExecutionContext context,
            string group,
            string imageName)
        {
            string groupPath = GetGroupPath(group);
            string imagePath = groupPath + imageName + ".png";
            var location = await _imageRecognition.FindImageAsync(imagePath, threshold: 0.8);
            return location.HasValue;
        }

        /// <summary>
        /// Find an image in a group and return its location
        /// </summary>
        private async Task<Point?> FindImageByGroupLocationAsync(
            ExecutionContext context,
            string group,
            string imageName)
        {
            string groupPath = GetGroupPath(group);
            string imagePath = groupPath + imageName + ".png";
            return await _imageRecognition.FindImageAsync(imagePath, threshold: 0.8);
        }

        /// <summary>
        /// Get folder path for a group
        /// </summary>
        private string GetGroupPath(string group)
        {
            switch (group)
            {
                case "mat_bao":
                    return Constant.ImagePathMatBaoFolder;
                case "global":
                    return Constant.ImagePathGlobalFolder;
                case "tri_an":
                    return Constant.ImagePathTriAnFolder;
                default:
                    return Constant.ImagePathGlobalFolder;
            }
        }

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

        #endregion

        #endregion
    }
}
