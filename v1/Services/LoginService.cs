using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Threading;

namespace AutoVPT.Services
{
    /// <summary>
    /// Service for handling game login operations.
    /// Extracted from GeneralFunctions to provide single-responsibility login management.
    /// </summary>
    internal class LoginService : ILoginService
    {
        private readonly AutoFeatures _auto;
        private readonly Character _character;
        private readonly Func<bool> _isRunning;

        /// <summary>
        /// Create a new LoginService instance.
        /// </summary>
        /// <param name="auto">AutoFeatures instance for image operations</param>
        /// <param name="character">Character settings</param>
        /// <param name="isRunningCheck">Function to check if character is still running</param>
        public LoginService(AutoFeatures auto, Character character, Func<bool> isRunningCheck = null)
        {
            _auto = auto ?? throw new ArgumentNullException(nameof(auto));
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _isRunning = isRunningCheck ?? (() => _character.Running != 0);
        }

        /// <summary>
        /// Check if character is currently in game (logged in and playing).
        /// </summary>
        /// <returns>True if in game, false otherwise</returns>
        public bool IsInGame()
        {
            return _auto.findImageByGroup("global", "khongtrongtrandau", false, true);
        }

        /// <summary>
        /// Perform full login sequence - from launcher to in-game.
        /// </summary>
        /// <param name="channel">Channel to join (default: 5)</param>
        public void Login(int channel = 5)
        {
            if (!_isRunning()) return;

            // Wait for login screen to load
            WaitForLoginScreen();

            // Click the mandatory button
            ClickMandatoryButton();

            // Select channel
            WaitForChannelSelection();
            SelectChannel(channel);

            // Select character and enter game
            WaitForCharacterSelection();
            SelectCharacter();

            // Wait until in game
            WaitUntilInGame();
        }

        /// <summary>
        /// Wait for the login screen to appear.
        /// </summary>
        /// <param name="maxWaitMs">Maximum wait time in milliseconds</param>
        /// <returns>True if login screen appeared, false if timeout</returns>
        public bool WaitForLoginScreen(int maxWaitMs = 60000)
        {
            var endTime = DateTime.Now.AddMilliseconds(maxWaitMs);

            while (DateTime.Now < endTime)
            {
                if (!_isRunning() || Helper.IsStoppingAll())
                    return false;

                if (_auto.findImageByGroup("global", "loginbatbuoc", false, false))
                    return true;

                _auto.writeStatus("Chờ bảng login...");
                Thread.Sleep(Constant.TimeShort);
            }

            return false;
        }

        /// <summary>
        /// Click the mandatory button on login screen.
        /// </summary>
        public void ClickMandatoryButton()
        {
            if (!_isRunning()) return;

            // Wait for channel selection to appear
            while (!_auto.findImageByGroup("global", "bangchonkenh", false, false))
            {
                if (!_isRunning() || Helper.IsStoppingAll())
                    return;

                _auto.writeStatus("Click bắt buộc");
                _auto.clickImageByGroup("global", "loginbatbuoc", false, false);
                Thread.Sleep(Constant.TimeShort);
            }
        }

        /// <summary>
        /// Wait for channel selection screen.
        /// </summary>
        /// <param name="maxWaitMs">Maximum wait time in milliseconds</param>
        /// <returns>True if channel selection appeared, false if timeout</returns>
        public bool WaitForChannelSelection(int maxWaitMs = 30000)
        {
            var endTime = DateTime.Now.AddMilliseconds(maxWaitMs);

            while (DateTime.Now < endTime)
            {
                if (!_isRunning() || Helper.IsStoppingAll())
                    return false;

                if (_auto.findImageByGroup("global", "bangchonkenh", false, false))
                    return true;

                Thread.Sleep(Constant.TimeShort);
            }

            return false;
        }

        /// <summary>
        /// Select a channel to join.
        /// </summary>
        /// <param name="channel">Channel number (1-based)</param>
        public void SelectChannel(int channel)
        {
            if (!_isRunning()) return;

            // Channel 1: x = -100, y = 20; each subsequent channel adds 34 to y
            int xOffset = -100;
            int yOffset = 20 + ((channel - 1) * 34);

            _auto.writeStatus($"Chọn kênh {channel}");
            _auto.clickImageByGroup("global", "bangchonkenh", false, false, 1, xOffset, yOffset);
            Thread.Sleep(Constant.TimeShort);
        }

        /// <summary>
        /// Wait for character selection screen.
        /// </summary>
        /// <param name="maxWaitMs">Maximum wait time in milliseconds</param>
        /// <returns>True if character selection appeared, false if timeout</returns>
        public bool WaitForCharacterSelection(int maxWaitMs = 30000)
        {
            var endTime = DateTime.Now.AddMilliseconds(maxWaitMs);

            while (DateTime.Now < endTime)
            {
                if (!_isRunning() || Helper.IsStoppingAll())
                    return false;

                if (_auto.findImageByGroup("global", "bangchonnhanvat", false, false))
                    return true;

                _auto.writeStatus("Chờ bảng chọn nhân vật...");
                Thread.Sleep(Constant.TimeShort);
            }

            return false;
        }

        /// <summary>
        /// Select and enter the game with the configured character.
        /// </summary>
        public void SelectCharacter()
        {
            if (!_isRunning()) return;

            int characterPosition = _character.ViTriNhanVat;
            int positionOnPage = characterPosition % 3;
            if (positionOnPage == 0) positionOnPage = 3;

            // Navigate to the correct page if needed
            if (characterPosition > 3)
            {
                int pageCount = (characterPosition - 1) / 3;
                for (int i = 1; i <= pageCount; i++)
                {
                    if (!_isRunning() || Helper.IsStoppingAll())
                        return;

                    _auto.writeStatus($"Qua trang {i}");
                    _auto.clickImageByGroup("global", "loginQuaTrangNhanVat", false, false);
                    Thread.Sleep(Constant.TimeShort);
                }
            }

            // Click on character position
            // Position 1: x = -80, Position 2: x = 60, Position 3: x = 200
            int xOffset = -80 + ((positionOnPage - 1) * 140);
            int yOffset = 30;

            _auto.writeStatus($"Chọn nhân vật vị trí {characterPosition}");
            _auto.clickImageByGroup("global", "bangchonnhanvat", false, false, 2, xOffset, yOffset);
            Thread.Sleep(Constant.TimeShort);

            // Click enter game button
            _auto.clickImageByGroup("global", "loginvaogame", false, false);
        }

        /// <summary>
        /// Wait until character is fully loaded into game.
        /// </summary>
        /// <param name="maxWaitMs">Maximum wait time in milliseconds</param>
        /// <returns>True if entered game, false if timeout</returns>
        public bool WaitUntilInGame(int maxWaitMs = 60000)
        {
            var endTime = DateTime.Now.AddMilliseconds(maxWaitMs);

            while (DateTime.Now < endTime)
            {
                if (!_isRunning() || Helper.IsStoppingAll())
                    return false;

                if (IsInGame())
                {
                    _auto.writeStatus("Đã vào game");
                    return true;
                }

                _auto.writeStatus("Chờ vào game...");
                Thread.Sleep(Constant.TimeShort);
            }

            _auto.writeStatus("Hết thời gian chờ vào game");
            return false;
        }
    }

    /// <summary>
    /// Interface for login operations.
    /// </summary>
    public interface ILoginService
    {
        bool IsInGame();
        void Login(int channel = 5);
        bool WaitForLoginScreen(int maxWaitMs = 60000);
        void ClickMandatoryButton();
        bool WaitForChannelSelection(int maxWaitMs = 30000);
        void SelectChannel(int channel);
        bool WaitForCharacterSelection(int maxWaitMs = 30000);
        void SelectCharacter();
        bool WaitUntilInGame(int maxWaitMs = 60000);
    }
}

