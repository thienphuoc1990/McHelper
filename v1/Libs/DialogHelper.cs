using System;
using System.Threading;
using System.Windows.Forms;

namespace AutoVPT.Libs
{
    /// <summary>
    /// Helper class for common dialog operations.
    /// Centralizes dialog handling to reduce code duplication across feature classes.
    /// </summary>
    internal class DialogHelper
    {
        private readonly AutoFeatures _auto;
        private readonly Func<bool> _isRunning;

        /// <summary>
        /// Create a new DialogHelper instance.
        /// </summary>
        /// <param name="auto">AutoFeatures instance for image operations</param>
        /// <param name="isRunningCheck">Function to check if character is still running</param>
        public DialogHelper(AutoFeatures auto, Func<bool> isRunningCheck)
        {
            _auto = auto ?? throw new ArgumentNullException(nameof(auto));
            _isRunning = isRunningCheck ?? throw new ArgumentNullException(nameof(isRunningCheck));
        }

        #region Close Operations

        /// <summary>
        /// Close all open dialogs by pressing Escape multiple times.
        /// </summary>
        public void CloseAllDialogs()
        {
            if (!_isRunning()) return;
            _auto.closeAllDialog();
        }

        /// <summary>
        /// Close dialogs and ensure a clean state before starting an operation.
        /// </summary>
        /// <param name="statusMessage">Optional status message to display</param>
        public void PrepareCleanState(string statusMessage = null)
        {
            if (!_isRunning()) return;

            if (!string.IsNullOrEmpty(statusMessage))
            {
                _auto.writeStatus(statusMessage);
            }
            CloseAllDialogs();
        }

        #endregion

        #region Wait Operations

        /// <summary>
        /// Wait for a dialog to appear.
        /// </summary>
        /// <param name="group">Image group name</param>
        /// <param name="dialogName">Dialog image name</param>
        /// <param name="maxWaitMs">Maximum wait time in milliseconds</param>
        /// <param name="checkIntervalMs">Check interval in milliseconds</param>
        /// <returns>True if dialog appeared, false if timeout or stopped</returns>
        public bool WaitForDialog(string group, string dialogName, int maxWaitMs = 10000, int checkIntervalMs = 500)
        {
            var endTime = DateTime.Now.AddMilliseconds(maxWaitMs);

            while (DateTime.Now < endTime)
            {
                if (!_isRunning() || Helper.IsStoppingAll())
                {
                    return false;
                }

                if (_auto.findImageByGroup(group, dialogName))
                {
                    return true;
                }

                Thread.Sleep(checkIntervalMs);
            }

            return false;
        }

        /// <summary>
        /// Wait for a dialog to close/disappear.
        /// </summary>
        /// <param name="group">Image group name</param>
        /// <param name="dialogName">Dialog image name</param>
        /// <param name="maxWaitMs">Maximum wait time in milliseconds</param>
        /// <param name="checkIntervalMs">Check interval in milliseconds</param>
        /// <returns>True if dialog closed, false if timeout or stopped</returns>
        public bool WaitForDialogClose(string group, string dialogName, int maxWaitMs = 10000, int checkIntervalMs = 500)
        {
            var endTime = DateTime.Now.AddMilliseconds(maxWaitMs);

            while (DateTime.Now < endTime)
            {
                if (!_isRunning() || Helper.IsStoppingAll())
                {
                    return false;
                }

                if (!_auto.findImageByGroup(group, dialogName))
                {
                    return true;
                }

                Thread.Sleep(checkIntervalMs);
            }

            return false;
        }

        #endregion

        #region Click Operations

        /// <summary>
        /// Click a button and wait for expected result.
        /// </summary>
        /// <param name="group">Image group name</param>
        /// <param name="buttonName">Button image name</param>
        /// <param name="expectedResultGroup">Expected result group (null to skip wait)</param>
        /// <param name="expectedResultName">Expected result name (null to skip wait)</param>
        /// <param name="maxRetries">Maximum click attempts</param>
        /// <param name="delayMs">Delay between clicks</param>
        /// <returns>True if button clicked and expected result appeared</returns>
        public bool ClickAndWait(
            string group, 
            string buttonName,
            string expectedResultGroup = null,
            string expectedResultName = null,
            int maxRetries = 3,
            int delayMs = 1000)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                if (!_isRunning() || Helper.IsStoppingAll())
                {
                    return false;
                }

                _auto.clickImageByGroup(group, buttonName, true, true);
                Thread.Sleep(delayMs);

                // If no expected result specified, just return after click
                if (string.IsNullOrEmpty(expectedResultGroup) || string.IsNullOrEmpty(expectedResultName))
                {
                    return true;
                }

                // Check for expected result
                if (_auto.findImageByGroup(expectedResultGroup, expectedResultName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Open a dialog by clicking a button until the dialog check image appears.
        /// </summary>
        /// <param name="group">Image group name</param>
        /// <param name="buttonName">Button to click</param>
        /// <param name="checkName">Check image that confirms dialog is open</param>
        /// <param name="maxRetries">Maximum click attempts</param>
        /// <param name="delayMs">Delay between clicks</param>
        /// <returns>True if dialog opened successfully</returns>
        public bool OpenDialog(
            string group,
            string buttonName,
            string checkName,
            int maxRetries = 5,
            int delayMs = Constant.TimeShort)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                if (!_isRunning() || Helper.IsStoppingAll())
                {
                    return false;
                }

                // Check if already open
                if (_auto.findImageByGroup(group, checkName))
                {
                    return true;
                }

                // Click to open
                _auto.clickImageByGroup(group, buttonName, true, true);
                Thread.Sleep(delayMs);
            }

            // Final check
            return _auto.findImageByGroup(group, checkName);
        }

        /// <summary>
        /// Click a confirmation button (Yes/OK/Confirm).
        /// </summary>
        /// <param name="confirmType">Type: "co" (Yes), "xacnhan" (Confirm), etc.</param>
        public void ClickConfirm(string confirmType = "co")
        {
            if (!_isRunning()) return;

            string buttonName;
            switch (confirmType)
            {
                case "co": buttonName = "luachonco"; break;
                case "xacnhan": buttonName = "xacnhan"; break;
                case "dong": buttonName = "dong"; break;
                default: buttonName = confirmType; break;
            }

            _auto.clickImageByGroup("global", buttonName, true, true);
            Thread.Sleep(Constant.TimeShort);
        }

        /// <summary>
        /// Scroll down in a dialog a specified number of times.
        /// </summary>
        /// <param name="group">Image group</param>
        /// <param name="scrollButtonName">Scroll button image name</param>
        /// <param name="times">Number of times to click</param>
        /// <param name="delayMs">Delay between clicks</param>
        public void ScrollDown(string group, string scrollButtonName, int times = 3, int delayMs = 300)
        {
            if (!_isRunning()) return;

            for (int i = 0; i < times && _isRunning() && !Helper.IsStoppingAll(); i++)
            {
                _auto.clickImageByGroup(group, scrollButtonName, false, true);
                Thread.Sleep(delayMs);
            }
        }

        #endregion

        #region Composite Operations

        /// <summary>
        /// Perform a complete dialog interaction: open, interact, close.
        /// </summary>
        /// <param name="group">Image group</param>
        /// <param name="openButtonName">Button to open dialog</param>
        /// <param name="checkName">Check image to confirm open</param>
        /// <param name="interactionAction">Action to perform while dialog is open</param>
        /// <param name="closeAfter">Whether to close dialogs after interaction</param>
        /// <returns>True if completed successfully</returns>
        public bool PerformDialogInteraction(
            string group,
            string openButtonName,
            string checkName,
            Action interactionAction,
            bool closeAfter = true)
        {
            if (!_isRunning()) return false;

            // Open dialog
            if (!OpenDialog(group, openButtonName, checkName))
            {
                _auto.writeStatus($"Không thể mở hộp thoại {openButtonName}");
                return false;
            }

            // Perform interaction
            interactionAction?.Invoke();

            // Close if requested
            if (closeAfter)
            {
                CloseAllDialogs();
            }

            return true;
        }

        #endregion
    }
}

