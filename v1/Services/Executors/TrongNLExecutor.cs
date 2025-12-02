using AutoVPT.Domain;
using AutoVPT.Infrastructure;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for TrongNL (Planting Materials) feature.
    /// Automates opening the farm, selecting material type, planting, and harvesting.
    /// </summary>
    public class TrongNLExecutor : BaseFeatureExecutor
    {
        public override FeatureType Type => FeatureType.TrongNL;

        public TrongNLExecutor(
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
                LogInfo("Starting TrongNL (Planting Materials) feature", context);

                // Clear image cache to ensure fresh, uncompressed images
                _imageRecognition.ClearCache();
                LogInfo("Cleared image recognition cache", context);

                // Get material type from configuration
                string materialType = context.Config.GetParameter("Loai", Constant.NameNguyenLieuKimLoai);
                LogInfo($"Material type: {materialType}", context);

                // Step 1: Open farm
                LogInfo("Opening farm...", context);
                await OpenFarmAsync(context);

                // Step 2: Open farming panel
                LogInfo("Opening farming panel...", context);
                await OpenFarmingPanelAsync(context);

                // Step 3: Check if there are empty plots
                LogInfo("Checking for empty plots...", context);
                bool hasEmptyPlots = await CheckEmptyPlotsAsync(context);

                if (hasEmptyPlots)
                {
                    // Step 4: Select material type
                    LogInfo("Selecting material type...", context);
                    await SelectMaterialTypeAsync(context, materialType);

                    // Step 5: Plant on all empty plots
                    LogInfo("Planting materials...", context);
                    await PlantMaterialsAsync(context);
                }
                else
                {
                    LogInfo("No empty plots available, skipping planting", context);
                }

                // Step 6: Harvest materials
                LogInfo("Harvesting materials...", context);
                await HarvestMaterialsAsync(context);

                // Step 7: Close farm
                LogInfo("Closing farm...", context);
                await CloseFarmAsync(context);

                LogInfo("TrongNL feature completed successfully", context);
                return FeatureResult.Successful($"Planted and harvested {materialType}");
            }
            catch (Exception ex)
            {
                LogError($"TrongNL feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
        }

        public override bool CanExecute(ExecutionContext context)
        {
            // Check if feature is enabled
            if (!context.Character.FeatureConfig.IsEnabled(FeatureType.TrongNL))
                return false;

            // Check if already completed today
            if (context.Character.RuntimeState.IsCompleted(FeatureType.TrongNL))
                return false;

            return true;
        }

        #region Private Methods

        /// <summary>
        /// Open the farm interface
        /// </summary>
        private async Task OpenFarmAsync(ExecutionContext context)
        {
            // Close all dialogs first
            LogDebug("Closing all dialogs...", context);
            await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);

            // Wait after closing dialogs for UI to settle
            await Task.Delay(1000);

            // Step 1: Check if menu is already open by looking for farm button
            LogDebug("Checking if right menu is already open...", context);
            var farmButtonCheck = await _imageRecognition.FindImageAsync(
                Constant.ImagePathTrangVienButton,
                searchArea: SearchRegions.RightPanel,  // Farm button in right panel (6x faster)
                threshold: 0.95);  // Use same threshold as legacy code

            if (!farmButtonCheck.HasValue)
            {
                // Menu is not open, need to open it
                LogInfo("Right menu not open, attempting to open...", context);
                
                // Try to open menu using the menu button (similar to legacy moMenuPhai)
                int menuOpenRetries = 0;
                const int maxMenuOpenRetries = 5;
                bool menuButtonClicked = false;

                while (!menuButtonClicked && menuOpenRetries < maxMenuOpenRetries)
                {
                    // Look for menu open button
                    var menuOpenLocation = await _imageRecognition.FindImageAsync(
                        Constant.ImagePathGlobalFolder + "momenuphai.png",
                        searchArea: SearchRegions.RightPanel,  // Menu button on right side (6x faster)
                        threshold: 0.95);  // Use same threshold as legacy (0.95)

                    if (menuOpenLocation.HasValue)
                    {
                        await _inputSimulator.ClickAsync(menuOpenLocation.Value);
                        await Task.Delay(Constant.TimeShort);
                        menuButtonClicked = true;
                        LogInfo("Menu open button clicked", context);
                    }
                    else
                    {
                        // Also try menu_phai.png (alternative button name)
                        var menuPhaiLocation = await _imageRecognition.FindImageAsync(
                            Constant.ImagePathGlobalFolder + "menu_phai.png",
                            searchArea: SearchRegions.RightPanel,  // Menu button on right side (6x faster)
                            threshold: 0.95);

                        if (menuPhaiLocation.HasValue)
                        {
                            await _inputSimulator.ClickAsync(menuPhaiLocation.Value);
                            await Task.Delay(Constant.TimeShort);
                            menuButtonClicked = true;
                            LogInfo("Menu button (menu_phai) clicked", context);
                        }
                    }

                    if (!menuButtonClicked)
                    {
                        menuOpenRetries++;
                        if (menuOpenRetries < maxMenuOpenRetries)
                        {
                            LogInfo($"Menu button not found, retrying ({menuOpenRetries}/{maxMenuOpenRetries})...", context);
                            await Task.Delay(500);
                        }
                    }
                }

                // Wait for menu to fully open
                if (menuButtonClicked)
                {
                    LogInfo("Waiting for right menu to open...", context);
                    await Task.Delay(Constant.TimeMedium); // Use longer delay - menu needs time to animate
                }
                else
                {
                    LogWarning("Failed to find and click menu button", context);
                }
            }
            else
            {
                LogInfo("Right menu is already open", context);
            }

            // Step 2: Click farm button with retry logic
            LogInfo($"Searching for farm button: {Constant.ImagePathTrangVienButton}", context);
            bool farmButtonFound = false;
            int buttonRetries = 0;
            const int maxButtonRetries = 10;

            while (!farmButtonFound && buttonRetries < maxButtonRetries)
            {
                var farmButtonLocation = await _imageRecognition.FindImageAsync(
                    Constant.ImagePathTrangVienButton,
                    searchArea: SearchRegions.RightPanel,  // Farm button in right panel (6x faster)
                    threshold: 0.95);  // Use same threshold as legacy code

                if (farmButtonLocation.HasValue)
                {
                    await _inputSimulator.ClickAsync(farmButtonLocation.Value);
                    farmButtonFound = true;
                    LogInfo("Farm button clicked successfully", context);
                    break;
                }

                buttonRetries++;
                if (buttonRetries < maxButtonRetries)
                {
                    LogInfo($"Farm button not found, retrying ({buttonRetries}/{maxButtonRetries})...", context);

                    // If we've tried a few times and still no button, try closing and reopening menu
                    if (buttonRetries == 3 || buttonRetries == 6)
                    {
                        LogDebug("Attempting to close and reopen menu...", context);

                        // Close all dialogs first (in case menu is already open)
                        await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);
                        await Task.Delay(500);

                        // Try to reopen menu
                        var reopenMenuLocation = await _imageRecognition.FindImageAsync(
                            Constant.ImagePathGlobalFolder + "momenuphai.png",
                            searchArea: SearchRegions.RightPanel,  // Menu button on right side (6x faster)
                            threshold: 0.95);  // Use same threshold as legacy

                        if (!reopenMenuLocation.HasValue)
                        {
                            // Try alternative button name
                            reopenMenuLocation = await _imageRecognition.FindImageAsync(
                                Constant.ImagePathGlobalFolder + "menu_phai.png",
                                searchArea: SearchRegions.RightPanel,  // Menu button on right side (6x faster)
                                threshold: 0.95);
                        }

                        if (reopenMenuLocation.HasValue)
                        {
                            await _inputSimulator.ClickAsync(reopenMenuLocation.Value);
                            await Task.Delay(Constant.TimeShort);
                        }
                    }

                    await Task.Delay(500);
                }
            }

            if (!farmButtonFound)
            {
                throw new Exception("Farm button not found after multiple attempts. Please ensure the right menu is open and the farm button is visible.");
            }

            // Wait for flash to load farm information
            LogInfo("Waiting for farm interface to load...", context);
            await Task.Delay(Constant.TimeMedium);
        }

        /// <summary>
        /// Close the farm interface
        /// </summary>
        private async Task CloseFarmAsync(ExecutionContext context)
        {
            var farmButtonLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathTrangVienButton,
                searchArea: SearchRegions.RightPanel,  // Farm button in right panel (6x faster)
                threshold: 0.95);  // Use same threshold as legacy code

            if (farmButtonLocation.HasValue)
            {
                await _inputSimulator.ClickAsync(farmButtonLocation.Value);
                await Task.Delay(Constant.TimeShort);
            }
        }

        /// <summary>
        /// Open the farming panel (nuôi trồng)
        /// </summary>
        private async Task OpenFarmingPanelAsync(ExecutionContext context)
        {
            var farmingButtonLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathNuoiTrongButton,
                searchArea: SearchRegions.DialogArea,  // 3x faster - button in farm dialog
                threshold: 0.8);

            if (!farmingButtonLocation.HasValue)
            {
                throw new Exception("Farming panel button not found");
            }

            await _inputSimulator.ClickAsync(farmingButtonLocation.Value);
            await Task.Delay(Constant.TimeShort);
        }

        /// <summary>
        /// Check if there are empty plots available
        /// </summary>
        private async Task<bool> CheckEmptyPlotsAsync(ExecutionContext context)
        {
            var emptyPlotLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathDatTrong,
                searchArea: SearchRegions.GameplayArea,  // 2x faster - plots in gameplay area
                threshold: 0.8);

            return emptyPlotLocation.HasValue;
        }

        /// <summary>
        /// Select material type to plant
        /// </summary>
        private async Task SelectMaterialTypeAsync(ExecutionContext context, string materialType)
        {
            string imagePath = GetMaterialImagePath(materialType);

            var materialLocation = await _imageRecognition.FindImageAsync(
                imagePath,
                searchArea: SearchRegions.DialogArea,  // 3x faster - material selection in dialog
                threshold: 0.8);

            if (!materialLocation.HasValue)
            {
                throw new Exception($"Material type '{materialType}' not found");
            }

            await _inputSimulator.ClickAsync(materialLocation.Value);
            await Task.Delay(Constant.TimeShort);
        }

        /// <summary>
        /// Plant materials on all empty plots
        /// </summary>
        private async Task PlantMaterialsAsync(ExecutionContext context)
        {
            int planted = 0;
            int maxAttempts = Constant.MaxLoop;

            for (int i = 0; i < maxAttempts; i++)
            {
                // Find empty plot
                var emptyPlotLocation = await _imageRecognition.FindImageAsync(
                    Constant.ImagePathDatTrong,
                    searchArea: SearchRegions.GameplayArea,  // 2x faster - plots in gameplay area
                    threshold: 0.8);

                if (!emptyPlotLocation.HasValue)
                {
                    // No more empty plots
                    break;
                }

                // Click slightly above the plot (offset -25 pixels on Y axis)
                var clickPoint = new Point(emptyPlotLocation.Value.X, emptyPlotLocation.Value.Y - 25);
                await _inputSimulator.ClickAsync(clickPoint);
                await Task.Delay(500); // Short delay between planting

                planted++;
            }

            LogInfo($"Planted {planted} plots", context);
        }

        /// <summary>
        /// Harvest all mature materials
        /// </summary>
        private async Task HarvestMaterialsAsync(ExecutionContext context)
        {
            // Click harvest button
            var harvestButtonLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathThuHoachButton,
                searchArea: SearchRegions.DialogArea,  // 3x faster - harvest button in dialog
                threshold: 0.8);

            if (harvestButtonLocation.HasValue)
            {
                await _inputSimulator.ClickAsync(harvestButtonLocation.Value);
                await Task.Delay(Constant.TimeShort);

                // Click collect button
                var collectButtonLocation = await _imageRecognition.FindImageAsync(
                    Constant.ImagePathDiemThuHoachButton,
                    searchArea: SearchRegions.DialogArea,  // 3x faster - collect button in dialog
                    threshold: 0.8);

                if (collectButtonLocation.HasValue)
                {
                    await _inputSimulator.ClickAsync(collectButtonLocation.Value);
                    await Task.Delay(Constant.TimeShort);
                    LogInfo("Harvested materials successfully", context);
                }
            }
            else
            {
                LogInfo("No materials ready to harvest", context);
            }
        }

        /// <summary>
        /// Close all open dialogs by pressing ESC key
        /// </summary>
        // Removed CloseAllDialogsAsync - now using ExecutorHelpers.CloseAllDialogsAsync

        /// <summary>
        /// Get image path for material type
        /// </summary>
        private string GetMaterialImagePath(string materialType)
        {
            switch (materialType)
            {
                case Constant.NameNguyenLieuKimLoai:
                    return Constant.ImagePathNguyenLieuKimLoai;
                case Constant.NameNguyenLieuGo:
                    return Constant.ImagePathNguyenLieuGo;
                case Constant.NameNguyenLieuNgoc:
                    return Constant.ImagePathNguyenLieuNgoc;
                case Constant.NameNguyenLieuVai:
                    return Constant.ImagePathNguyenLieuVai;
                case Constant.NameNguyenLieuLongThu:
                    return Constant.ImagePathNguyenLieuLongThu;
                case Constant.NameNguyenLieuGamVoc:
                    return Constant.ImagePathNguyenLieuGamVoc;
                case Constant.NameNguyenLieuDaThu:
                    return Constant.ImagePathNguyenLieuDaThu;
                case Constant.NameNguyenLieuPhaLe:
                    return Constant.ImagePathNguyenLieuPhaLe;
                case Constant.NameNguyenLieuKimLoaiHiem:
                    return Constant.ImagePathNguyenLieuKimLoaiHiem;
                case Constant.NameNguyenLieuGoTot:
                    return Constant.ImagePathNguyenLieuGoTot;
                default:
                    return Constant.ImagePathNguyenLieuKimLoai; // Default to metal
            }
        }

        #endregion
    }
}
