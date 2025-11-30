using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using AutoVPT.Infrastructure;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Shared helper methods for executors to reduce code duplication.
    /// Provides common patterns like dialog closing, panel waiting, image clicking, etc.
    ///
    /// OPTIMIZATIONS APPLIED:
    /// - Regional search support for 2-3x speedup
    /// - Smart region detection based on image group
    /// - Parallel search support for multiple images
    /// </summary>
    public static class ExecutorHelpers
    {
        /// <summary>
        /// Get the appropriate search region for an image group.
        /// OPTIMIZATION 1: Smart regional search for 2-20x speedup.
        /// </summary>
        private static Rectangle? GetSearchRegionForGroup(string group)
        {
            switch (group)
            {
                case "global":
                    return null; // Global elements can be anywhere, search full screen

                case "nvhn":
                case "quickFeatureList":
                    return SearchRegions.RightPanel; // Quick features on right side (6x faster)

                case "mat_bao":
                case "tri_an":
                case "phu_ban":
                case "tru_ma":
                case "stmt":
                    return SearchRegions.DialogArea; // Feature dialogs in center (3x faster)

                case "in_map":
                case "maps":
                    return SearchRegions.TopRight; // Minimap on top-right (16x faster)

                case "bat_pet":
                    return SearchRegions.BottomRight; // Pet panel on bottom-right (12x faster)

                case "event":
                    return SearchRegions.Center; // Event dialogs in center (4x faster)

                default:
                    return null; // Unknown group, search full screen
            }
        }

        /// <summary>
        /// Get search region for common UI elements by name pattern.
        /// </summary>
        private static Rectangle? GetSearchRegionForImageName(string imageName)
        {
            string lower = imageName?.ToLower() ?? "";

            // Menu and system buttons (top-right)
            if (lower.Contains("menu") || lower.Contains("vip") || lower.Contains("setting"))
                return SearchRegions.TopRight; // 16x faster

            // Dialog buttons (center)
            if (lower.Contains("confirm") || lower.Contains("cancel") || lower.Contains("close") ||
                lower.Contains("xong") || lower.Contains("huy") || lower.Contains("dong"))
                return SearchRegions.DialogArea; // 3x faster

            // Quick feature list (right panel)
            if (lower.Contains("quickfeature") || lower.Contains("arrow"))
                return SearchRegions.RightPanel; // 6x faster

            // Character/quest indicators (top-left)
            if (lower.Contains("quest") || lower.Contains("nhiemvu") || lower.Contains("portrait"))
                return SearchRegions.TopLeft; // 16x faster

            // Skills and inventory (bottom)
            if (lower.Contains("skill") || lower.Contains("inventory") || lower.Contains("bag"))
                return SearchRegions.BottomBar; // 12x faster

            // Minimap (top-right)
            if (lower.Contains("map") || lower.Contains("minimap"))
                return SearchRegions.TopRight; // 16x faster

            return null; // Unknown, search full screen
        }

        /// <summary>
        /// Close all dialogs by pressing ESC key multiple times.
        /// TIER 1 helper - used by 11+ executors.
        /// </summary>
        /// <param name="inputSimulator">Input simulator for key presses</param>
        /// <param name="count">Number of times to press ESC (default: 3)</param>
        /// <param name="delayMs">Delay between presses in milliseconds (default: 500)</param>
        public static async Task CloseAllDialogsAsync(
            IInputSimulator inputSimulator,
            int count = 3,
            int delayMs = 500)
        {
            for (int i = 0; i < count; i++)
            {
                await inputSimulator.SendKeyAsync(Keys.Escape);
                await Task.Delay(delayMs);
            }
        }

        /// <summary>
        /// Wait for a panel to open by clicking a button until the check image appears.
        /// TIER 2 helper - used by 3+ executors.
        /// </summary>
        /// <param name="imageRecognition">Image recognition service</param>
        /// <param name="inputSimulator">Input simulator for clicking</param>
        /// <param name="buttonImagePath">Full path to button image</param>
        /// <param name="checkImagePath">Full path to check image (indicates panel is open)</param>
        /// <param name="maxAttempts">Maximum number of attempts (default: MaxLoop)</param>
        /// <param name="delayMs">Delay between attempts in milliseconds (default: TimeShort)</param>
        /// <returns>True if panel opened successfully, false otherwise</returns>
        public static async Task<bool> WaitForPanelAsync(
            IImageRecognition imageRecognition,
            IInputSimulator inputSimulator,
            string buttonImagePath,
            string checkImagePath,
            int maxAttempts = -1,
            int delayMs = -1)
        {
            if (maxAttempts == -1) maxAttempts = Constant.MaxLoop;
            if (delayMs == -1) delayMs = Constant.TimeShort;

            int attempts = 0;

            // OPTIMIZATION 1: Use smart regional search based on image names
            var checkRegion = GetSearchRegionForImageName(System.IO.Path.GetFileNameWithoutExtension(checkImagePath));
            var buttonRegion = GetSearchRegionForImageName(System.IO.Path.GetFileNameWithoutExtension(buttonImagePath));

            while (attempts < maxAttempts)
            {
                // Check if panel is already open (with regional search)
                var checkLocation = await imageRecognition.FindImageAsync(checkImagePath, searchArea: checkRegion, threshold: 0.8);

                if (checkLocation.HasValue)
                {
                    return true;
                }

                // Click button to open panel (with regional search)
                var buttonLocation = await imageRecognition.FindImageAsync(buttonImagePath, searchArea: buttonRegion, threshold: 0.8);

                if (buttonLocation.HasValue)
                {
                    await inputSimulator.ClickAsync(buttonLocation.Value);
                    await Task.Delay(delayMs);
                }

                attempts++;
                await Task.Delay(500);
            }

            return false;
        }

        /// <summary>
        /// Open a feature from the quick features list by scrolling to find it.
        /// TIER 2 helper - used by 2+ executors.
        /// </summary>
        /// <param name="imageRecognition">Image recognition service</param>
        /// <param name="inputSimulator">Input simulator for clicking</param>
        /// <param name="featureName">Name of the feature to find</param>
        /// <param name="maxLoops">Maximum number of outer loops (default: MaxLoopShort)</param>
        /// <returns>True if feature was opened successfully, false otherwise</returns>
        public static async Task<bool> OpenFeatureFromQuickListAsync(
            IImageRecognition imageRecognition,
            IInputSimulator inputSimulator,
            string featureName,
            int maxLoops = -1)
        {
            if (maxLoops == -1) maxLoops = Constant.MaxLoopShort;

            string featureImagePath = Constant.ImagePathGlobalFolder + featureName + ".png";
            string featureCheckPath = Constant.ImagePathGlobalFolder + featureName + "_check.png";
            string upArrowPath = Constant.ImagePathGlobalFolder + "quickFeatureListUpArrow.png";
            string downArrowPath = Constant.ImagePathGlobalFolder + "quickFeatureListDownArrow.png";

            // OPTIMIZATION 1: Quick features list is always on right panel (6x faster)
            var quickListRegion = SearchRegions.RightPanel;

            int loop = 0;

            while (loop < maxLoops)
            {
                // Check if feature panel is already open
                var featureCheckLocation = await imageRecognition.FindImageAsync(featureCheckPath, threshold: 0.8);

                if (featureCheckLocation.HasValue)
                {
                    return true;
                }

                // Scroll to top of quick features list first
                while (true)
                {
                    var upArrowLocation = await imageRecognition.FindImageAsync(upArrowPath, searchArea: quickListRegion, threshold: 0.8);
                    var featureLocation = await imageRecognition.FindImageAsync(featureImagePath, searchArea: quickListRegion, threshold: 0.8);

                    if (!upArrowLocation.HasValue || featureLocation.HasValue)
                    {
                        break; // Reached top or found feature
                    }

                    await inputSimulator.ClickAsync(upArrowLocation.Value);
                    await Task.Delay(Constant.TimeShort);
                }

                // Scroll down to find the feature
                while (true)
                {
                    var featureLocation = await imageRecognition.FindImageAsync(featureImagePath, searchArea: quickListRegion, threshold: 0.8);

                    if (featureLocation.HasValue)
                    {
                        await inputSimulator.ClickAsync(featureLocation.Value);
                        await Task.Delay(Constant.TimeMedium);
                        break;
                    }

                    var downArrowLocation = await imageRecognition.FindImageAsync(downArrowPath, searchArea: quickListRegion, threshold: 0.8);

                    if (!downArrowLocation.HasValue)
                    {
                        return false; // Feature not found in list
                    }

                    await inputSimulator.ClickAsync(downArrowLocation.Value);
                    await Task.Delay(Constant.TimeMedium);
                }

                loop++;
            }

            return false;
        }

        /// <summary>
        /// Click an image with retry logic until found or max attempts reached.
        /// TIER 3 helper - used by 2+ executors.
        /// </summary>
        /// <param name="imageRecognition">Image recognition service</param>
        /// <param name="inputSimulator">Input simulator for clicking</param>
        /// <param name="imagePath">Full path to image to click</param>
        /// <param name="maxAttempts">Maximum number of attempts (default: MaxLoop)</param>
        /// <param name="delayMs">Delay between attempts in milliseconds (default: 300)</param>
        /// <param name="clickDelayMs">Delay after clicking in milliseconds (default: 500)</param>
        /// <returns>True if image was found and clicked, false otherwise</returns>
        public static async Task<bool> ClickImageWithLoopAsync(
            IImageRecognition imageRecognition,
            IInputSimulator inputSimulator,
            string imagePath,
            int maxAttempts = -1,
            int delayMs = 300,
            int clickDelayMs = 500)
        {
            if (maxAttempts == -1) maxAttempts = Constant.MaxLoop;

            // OPTIMIZATION 1: Use smart regional search based on image name
            var searchRegion = GetSearchRegionForImageName(System.IO.Path.GetFileNameWithoutExtension(imagePath));

            int attempts = 0;

            while (attempts < maxAttempts)
            {
                var imageLocation = await imageRecognition.FindImageAsync(imagePath, searchArea: searchRegion, threshold: 0.8);

                if (imageLocation.HasValue)
                {
                    await inputSimulator.ClickAsync(imageLocation.Value);
                    await Task.Delay(clickDelayMs);
                    return true;
                }

                attempts++;
                await Task.Delay(delayMs);
            }

            return false;
        }

        /// <summary>
        /// Click all instances of an image with retry logic.
        /// TIER 3 helper - used by RutBoExecutor.
        /// </summary>
        /// <param name="imageRecognition">Image recognition service</param>
        /// <param name="inputSimulator">Input simulator for clicking</param>
        /// <param name="imagePath">Full path to image to click</param>
        /// <param name="maxAttempts">Maximum number of attempts (default: MaxLoop)</param>
        /// <param name="delayMs">Delay between attempts in milliseconds (default: 300)</param>
        /// <param name="clickDelayMs">Delay after clicking in milliseconds (default: 500)</param>
        /// <returns>True if at least one image was found and clicked, false otherwise</returns>
        public static async Task<bool> ClickAllImagesWithLoopAsync(
            IImageRecognition imageRecognition,
            IInputSimulator inputSimulator,
            string imagePath,
            int maxAttempts = -1,
            int delayMs = 300,
            int clickDelayMs = 500)
        {
            if (maxAttempts == -1) maxAttempts = Constant.MaxLoop;

            // OPTIMIZATION 1: Use smart regional search based on image name
            var searchRegion = GetSearchRegionForImageName(System.IO.Path.GetFileNameWithoutExtension(imagePath));

            bool foundAny = false;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                var imageLocation = await imageRecognition.FindImageAsync(imagePath, searchArea: searchRegion, threshold: 0.8);

                if (imageLocation.HasValue)
                {
                    await inputSimulator.ClickAsync(imageLocation.Value);
                    await Task.Delay(clickDelayMs);
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
                    await Task.Delay(delayMs);
                }
            }

            return foundAny;
        }

        /// <summary>
        /// Get the folder path for an image group.
        /// TIER 3 helper - used by 3+ executors.
        /// </summary>
        /// <param name="group">Group name (e.g., "global", "mat_bao", "tri_an", etc.)</param>
        /// <param name="isChinese">Whether to use Chinese resource folder</param>
        /// <returns>Full path to the image group folder</returns>
        public static string GetGroupPath(string group, bool isChinese = false)
        {
            switch (group)
            {
                case "global":
                    return Constant.ImagePathGlobalFolder;
                case "mat_bao":
                    return Constant.ImagePathMatBaoFolder;
                case "tri_an":
                    return Constant.ImagePathTriAnFolder;
                case "phu_ban":
                    return Constant.ImagePathPhuBanFolder;
                case "tru_ma":
                    return Constant.ImagePathTruMaFolder;
                case "nvhn":
                    return Constant.ImagePathNVHNFolder;
                case "in_map":
                    return Constant.ImagePathInMapFolder;
                case "maps":
                    return Constant.ImagePathMapsFolder;
                case "event":
                    return Constant.ImagePathEventFolder;
                case "bat_pet":
                    return Constant.ImagePathBatPetFolder;
                case "stmt":
                    return Constant.ImagePathSTMTFolder;
                default:
                    return Constant.ImagePathGlobalFolder;
            }
        }

        /// <summary>
        /// Find an image by group and name.
        /// TIER 3 helper - used by 3+ executors.
        /// OPTIMIZATION 1: Uses smart regional search based on group (3-16x faster).
        /// </summary>
        /// <param name="imageRecognition">Image recognition service</param>
        /// <param name="group">Group name</param>
        /// <param name="imageName">Image name (without .png extension)</param>
        /// <param name="isChinese">Whether to use Chinese resources</param>
        /// <returns>Location of the image if found, null otherwise</returns>
        public static async Task<Point?> FindImageByGroupAsync(
            IImageRecognition imageRecognition,
            string group,
            string imageName,
            bool isChinese = false)
        {
            string groupPath = GetGroupPath(group, isChinese);
            string imagePath = groupPath + imageName + ".png";

            // OPTIMIZATION 1: Use group-based regional search
            var searchRegion = GetSearchRegionForGroup(group);

            return await imageRecognition.FindImageAsync(imagePath, searchArea: searchRegion, threshold: 0.8);
        }

        /// <summary>
        /// Click an image by group and name.
        /// TIER 3 helper - used by 2+ executors.
        /// OPTIMIZATION 1: Uses smart regional search based on group (3-16x faster).
        /// </summary>
        /// <param name="imageRecognition">Image recognition service</param>
        /// <param name="inputSimulator">Input simulator for clicking</param>
        /// <param name="group">Group name</param>
        /// <param name="imageName">Image name (without .png extension)</param>
        /// <param name="isChinese">Whether to use Chinese resources</param>
        /// <param name="waitAfter">Whether to wait longer after clicking (default: false)</param>
        /// <returns>True if image was found and clicked, false otherwise</returns>
        public static async Task<bool> ClickImageByGroupAsync(
            IImageRecognition imageRecognition,
            IInputSimulator inputSimulator,
            string group,
            string imageName,
            bool isChinese = false,
            bool waitAfter = false)
        {
            string groupPath = GetGroupPath(group, isChinese);
            string imagePath = groupPath + imageName + ".png";

            // OPTIMIZATION 1: Use group-based regional search
            var searchRegion = GetSearchRegionForGroup(group);

            var location = await imageRecognition.FindImageAsync(imagePath, searchArea: searchRegion, threshold: 0.8);

            if (location.HasValue)
            {
                await inputSimulator.ClickAsync(location.Value);
                await Task.Delay(waitAfter ? 1000 : 200);
                return true;
            }

            return false;
        }

        /// <summary>
        /// OPTIMIZATION 2: Find multiple images in parallel for Nx speedup.
        /// Returns as soon as first image is found.
        /// </summary>
        /// <param name="imageRecognition">Image recognition service (must support FindFirstMatchAsync)</param>
        /// <param name="group">Group name for all images</param>
        /// <param name="imageNames">Array of image names (without .png extension)</param>
        /// <param name="isChinese">Whether to use Chinese resources</param>
        /// <returns>Tuple of (imageName, location) for first match, or (null, null) if none found</returns>
        public static async Task<(string imageName, Point? location)> FindFirstImageByGroupAsync(
            IImageRecognition imageRecognition,
            string group,
            string[] imageNames,
            bool isChinese = false)
        {
            if (!(imageRecognition is EmguCvImageRecognition emguRecognition))
            {
                // Fallback: sequential search if not EmguCvImageRecognition
                foreach (var imageName in imageNames)
                {
                    var foundLocation = await FindImageByGroupAsync(imageRecognition, group, imageName, isChinese);
                    if (foundLocation.HasValue)
                        return (imageName, foundLocation);
                }
                return (null, null);
            }

            // Build full paths
            string groupPath = GetGroupPath(group, isChinese);
            string[] imagePaths = new string[imageNames.Length];
            for (int i = 0; i < imageNames.Length; i++)
            {
                imagePaths[i] = groupPath + imageNames[i] + ".png";
            }

            // OPTIMIZATION 1 + 2: Use parallel search with regional search
            var searchRegion = GetSearchRegionForGroup(group);
            var (matchedPath, location) = await emguRecognition.FindFirstMatchAsync(imagePaths, searchRegion, 0.8);

            // Extract image name from matched path
            if (location.HasValue && !string.IsNullOrEmpty(matchedPath))
            {
                string matchedName = System.IO.Path.GetFileNameWithoutExtension(matchedPath);
                return (matchedName, location);
            }

            return (null, null);
        }

        /// <summary>
        /// Click a point with offset coordinates.
        /// TIER 4 helper - used by 4+ executors.
        /// </summary>
        /// <param name="inputSimulator">Input simulator for clicking</param>
        /// <param name="basePoint">Base point to click</param>
        /// <param name="offsetX">X offset in pixels</param>
        /// <param name="offsetY">Y offset in pixels</param>
        /// <param name="delayMs">Delay after clicking in milliseconds (default: TimeShort)</param>
        public static async Task ClickWithOffsetAsync(
            IInputSimulator inputSimulator,
            Point basePoint,
            int offsetX,
            int offsetY,
            int delayMs = -1)
        {
            if (delayMs == -1) delayMs = Constant.TimeShort;

            var clickPoint = new Point(basePoint.X + offsetX, basePoint.Y + offsetY);
            await inputSimulator.ClickAsync(clickPoint);
            await Task.Delay(delayMs);
        }

        /// <summary>
        /// Execute an action with a maximum number of attempts.
        /// TIER 1 helper - universal pattern used by all executors.
        /// </summary>
        /// <param name="action">Async action to execute</param>
        /// <param name="successCondition">Function to check if action succeeded</param>
        /// <param name="maxAttempts">Maximum number of attempts (default: MaxLoop)</param>
        /// <param name="delayMs">Delay between attempts in milliseconds (default: 500)</param>
        /// <returns>True if action succeeded within max attempts, false otherwise</returns>
        public static async Task<bool> RetryUntilSuccessAsync(
            Func<Task> action,
            Func<bool> successCondition,
            int maxAttempts = -1,
            int delayMs = 500)
        {
            if (maxAttempts == -1) maxAttempts = Constant.MaxLoop;

            int attempts = 0;

            while (attempts < maxAttempts)
            {
                await action();

                if (successCondition())
                {
                    return true;
                }

                attempts++;
                await Task.Delay(delayMs);
            }

            return false;
        }

        /// <summary>
        /// Find a button including its hover variant using parallel search.
        /// OPTIMIZATION 2: Searches for both normal and hover states simultaneously (2x faster).
        /// Common pattern for UI buttons that may appear in either state.
        /// </summary>
        /// <param name="imageRecognition">Image recognition service</param>
        /// <param name="group">Group name (e.g., "global", "tri_an")</param>
        /// <param name="buttonName">Button name without _hover suffix</param>
        /// <param name="isChinese">Whether to use Chinese resources</param>
        /// <returns>Tuple of (imageName, location) for first match, or (null, null) if not found</returns>
        public static async Task<(string imageName, Point? location)> FindButtonWithHoverAsync(
            IImageRecognition imageRecognition,
            string group,
            string buttonName,
            bool isChinese = false)
        {
            // Search for both normal and hover states in parallel
            return await FindFirstImageByGroupAsync(
                imageRecognition,
                group,
                new[] { buttonName, buttonName + "_hover" },
                isChinese);
        }

        /// <summary>
        /// Click a button including its hover variant using parallel search.
        /// OPTIMIZATION 2: Searches and clicks button in either normal or hover state (2x faster).
        /// </summary>
        /// <param name="imageRecognition">Image recognition service</param>
        /// <param name="inputSimulator">Input simulator for clicking</param>
        /// <param name="group">Group name (e.g., "global", "tri_an")</param>
        /// <param name="buttonName">Button name without _hover suffix</param>
        /// <param name="isChinese">Whether to use Chinese resources</param>
        /// <returns>True if button was found and clicked, false otherwise</returns>
        public static async Task<bool> ClickButtonWithHoverAsync(
            IImageRecognition imageRecognition,
            IInputSimulator inputSimulator,
            string group,
            string buttonName,
            bool isChinese = false)
        {
            var (foundName, location) = await FindButtonWithHoverAsync(
                imageRecognition, group, buttonName, isChinese);

            if (location.HasValue)
            {
                await inputSimulator.ClickAsync(location.Value);
                await Task.Delay(Constant.TimeShort);
                return true;
            }

            return false;
        }
    }
}
