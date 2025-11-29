using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Shared helper methods for executors to reduce code duplication.
    /// Provides common patterns like dialog closing, panel waiting, image clicking, etc.
    /// </summary>
    public static class ExecutorHelpers
    {
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

            while (attempts < maxAttempts)
            {
                // Check if panel is already open
                var checkLocation = await imageRecognition.FindImageAsync(checkImagePath, threshold: 0.8);

                if (checkLocation.HasValue)
                {
                    return true;
                }

                // Click button to open panel
                var buttonLocation = await imageRecognition.FindImageAsync(buttonImagePath, threshold: 0.8);

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
                    var upArrowLocation = await imageRecognition.FindImageAsync(upArrowPath, threshold: 0.8);
                    var featureLocation = await imageRecognition.FindImageAsync(featureImagePath, threshold: 0.8);

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
                    var featureLocation = await imageRecognition.FindImageAsync(featureImagePath, threshold: 0.8);

                    if (featureLocation.HasValue)
                    {
                        await inputSimulator.ClickAsync(featureLocation.Value);
                        await Task.Delay(Constant.TimeMedium);
                        break;
                    }

                    var downArrowLocation = await imageRecognition.FindImageAsync(downArrowPath, threshold: 0.8);

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

            int attempts = 0;

            while (attempts < maxAttempts)
            {
                var imageLocation = await imageRecognition.FindImageAsync(imagePath, threshold: 0.8);

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

            bool foundAny = false;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                var imageLocation = await imageRecognition.FindImageAsync(imagePath, threshold: 0.8);

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
            return await imageRecognition.FindImageAsync(imagePath, threshold: 0.8);
        }

        /// <summary>
        /// Click an image by group and name.
        /// TIER 3 helper - used by 2+ executors.
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

            var location = await imageRecognition.FindImageAsync(imagePath, threshold: 0.8);

            if (location.HasValue)
            {
                await inputSimulator.ClickAsync(location.Value);
                await Task.Delay(waitAfter ? 1000 : 200);
                return true;
            }

            return false;
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
    }
}
