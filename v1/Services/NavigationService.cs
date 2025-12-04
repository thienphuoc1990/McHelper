using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Threading;
using System.Windows.Forms;

namespace AutoVPT.Services
{
    /// <summary>
    /// Service for character navigation - moving to maps, NPCs, and coordinates.
    /// Wraps navigation methods from AutoFeatures to provide a cleaner interface.
    /// </summary>
    internal class NavigationService : INavigationService
    {
        private readonly AutoFeatures _auto;
        private readonly Character _character;
        private readonly Func<bool> _isRunning;

        /// <summary>
        /// Create a new NavigationService instance.
        /// </summary>
        /// <param name="auto">AutoFeatures instance for image and movement operations</param>
        /// <param name="character">Character settings</param>
        /// <param name="isRunningCheck">Optional function to check if character is still running</param>
        public NavigationService(AutoFeatures auto, Character character, Func<bool> isRunningCheck = null)
        {
            _auto = auto ?? throw new ArgumentNullException(nameof(auto));
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _isRunning = isRunningCheck ?? (() => _character.Running != 0);
        }

        #region Map Navigation

        /// <summary>
        /// Move to a map by its name using world map navigation.
        /// </summary>
        /// <param name="mapName">Map name (must match image file name)</param>
        /// <param name="x">X offset for click position</param>
        /// <param name="y">Y offset for click position</param>
        /// <returns>True if successfully moved to map, false otherwise</returns>
        public bool MoveToMap(string mapName, int x = 0, int y = -20)
        {
            if (!_isRunning() || Helper.IsStoppingAll())
                return false;

            return _auto.moveToMap(mapName, x, y);
        }

        /// <summary>
        /// Move to a map using group/party navigation.
        /// </summary>
        /// <param name="mapName">Map name</param>
        /// <param name="worldMapIndex">World map index (1 or 2)</param>
        /// <param name="x">X offset</param>
        /// <param name="y">Y offset</param>
        /// <returns>True if successfully moved, false otherwise</returns>
        public bool MoveToMapAsGroup(string mapName, int worldMapIndex = 1, int x = 0, int y = -20)
        {
            if (!_isRunning() || Helper.IsStoppingAll())
                return false;

            return _auto.moveToMapNhom(mapName, worldMapIndex, x, y);
        }

        #endregion

        #region NPC Navigation

        /// <summary>
        /// Move to and find an NPC by its name.
        /// </summary>
        /// <param name="npcName">NPC name (must match image file names)</param>
        /// <param name="locationName">Location marker name on mini-map</param>
        /// <returns>True if NPC found, false otherwise</returns>
        public bool MoveToNPC(string npcName, string locationName)
        {
            if (!_isRunning() || Helper.IsStoppingAll())
                return false;

            return _auto.moveToNPC(npcName, locationName);
        }

        /// <summary>
        /// Find an NPC (check if NPC is visible on screen).
        /// </summary>
        /// <param name="npcName">NPC name</param>
        /// <returns>True if NPC is visible, false otherwise</returns>
        public bool FindNPC(string npcName)
        {
            if (!_isRunning() || Helper.IsStoppingAll())
                return false;

            return _auto.findNPC(npcName);
        }

        /// <summary>
        /// Talk to an NPC (initiate conversation).
        /// </summary>
        /// <param name="npcName">NPC name</param>
        /// <param name="x">X offset for click position</param>
        /// <param name="y">Y offset for click position</param>
        /// <returns>True if successfully talking to NPC, false otherwise</returns>
        public bool TalkToNPC(string npcName, int x = 0, int y = -20)
        {
            if (!_isRunning() || Helper.IsStoppingAll())
                return false;

            return _auto.talkToNPC(npcName, 0, x, y);
        }

        /// <summary>
        /// Check if currently talking with an NPC.
        /// </summary>
        /// <param name="npcName">NPC name to check</param>
        /// <returns>True if in conversation with NPC, false otherwise</returns>
        public bool IsTalkingWithNPC(string npcName)
        {
            if (!_isRunning())
                return false;

            return _auto.isTalkWithNPC(npcName);
        }

        #endregion

        #region Movement State

        /// <summary>
        /// Check if character is currently moving.
        /// </summary>
        /// <returns>True if character is moving, false if stationary or stopped</returns>
        public bool IsMoving()
        {
            if (!_isRunning())
                return false;

            return _auto.isMoving();
        }

        /// <summary>
        /// Wait until character stops moving.
        /// </summary>
        /// <param name="timeoutMs">Maximum wait time in milliseconds</param>
        /// <returns>True if stopped moving, false if timeout</returns>
        public bool WaitUntilStopped(int timeoutMs = 30000)
        {
            var endTime = DateTime.Now.AddMilliseconds(timeoutMs);

            while (DateTime.Now < endTime)
            {
                if (!_isRunning() || Helper.IsStoppingAll())
                    return false;

                if (!IsMoving())
                    return true;

                Thread.Sleep(500);
            }

            return false;
        }

        /// <summary>
        /// Check if character is in a battle.
        /// </summary>
        /// <returns>True if in battle, false otherwise</returns>
        public bool IsInBattle()
        {
            if (!_isRunning())
                return false;

            return _auto.dangTrongTranDau();
        }

        #endregion

        #region Flight Controls

        /// <summary>
        /// Make character fly up.
        /// </summary>
        public void FlyUp()
        {
            if (!_isRunning() || Helper.IsStoppingAll())
                return;

            _auto.bay();
        }

        /// <summary>
        /// Make character land/fly down.
        /// </summary>
        public void FlyDown()
        {
            if (!_isRunning() || Helper.IsStoppingAll())
                return;

            _auto.bayXuong();
        }

        #endregion

        #region Dialog/UI Controls

        /// <summary>
        /// Close all open dialogs.
        /// </summary>
        public void CloseAllDialogs()
        {
            if (!_isRunning())
                return;

            _auto.closeAllDialog();
        }

        /// <summary>
        /// Open the mini-map.
        /// </summary>
        public void OpenMiniMap()
        {
            if (!_isRunning())
                return;

            _auto.sendKey(Keys.Oemtilde);
        }

        #endregion

        #region Composite Operations

        /// <summary>
        /// Navigate to map and then to a specific NPC location.
        /// </summary>
        /// <param name="mapName">Map name to move to</param>
        /// <param name="npcName">NPC name to find</param>
        /// <param name="locationName">Location marker name</param>
        /// <param name="maxRetries">Maximum retry attempts</param>
        /// <returns>True if successfully reached NPC, false otherwise</returns>
        public bool NavigateToNPC(string mapName, string npcName, string locationName, int maxRetries = 3)
        {
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                if (!_isRunning() || Helper.IsStoppingAll())
                    return false;

                // Move to map first
                if (!MoveToMap(mapName))
                {
                    _auto.writeStatus($"Không thể di chuyển đến {mapName}, thử lại...");
                    continue;
                }

                // Fly up for better view
                FlyUp();

                // Move to NPC
                if (MoveToNPC(npcName, locationName))
                {
                    return true;
                }

                _auto.writeStatus($"Không tìm thấy {npcName}, thử lại...");
            }

            _auto.writeStatus($"Thất bại khi tìm {npcName} sau {maxRetries} lần thử");
            return false;
        }

        /// <summary>
        /// Navigate to NPC, fly down, and talk to them.
        /// </summary>
        /// <param name="mapName">Map name</param>
        /// <param name="npcName">NPC name</param>
        /// <param name="locationName">Location marker</param>
        /// <returns>True if successfully talking to NPC, false otherwise</returns>
        public bool GoToAndTalkToNPC(string mapName, string npcName, string locationName)
        {
            if (!NavigateToNPC(mapName, npcName, locationName))
                return false;

            // Land before talking
            FlyDown();
            Thread.Sleep(Constant.TimeShort);

            // Talk to NPC
            return TalkToNPC(npcName);
        }

        #endregion
    }

    /// <summary>
    /// Interface for navigation operations.
    /// </summary>
    public interface INavigationService
    {
        // Map Navigation
        bool MoveToMap(string mapName, int x = 0, int y = -20);
        bool MoveToMapAsGroup(string mapName, int worldMapIndex = 1, int x = 0, int y = -20);

        // NPC Navigation
        bool MoveToNPC(string npcName, string locationName);
        bool FindNPC(string npcName);
        bool TalkToNPC(string npcName, int x = 0, int y = -20);
        bool IsTalkingWithNPC(string npcName);

        // Movement State
        bool IsMoving();
        bool WaitUntilStopped(int timeoutMs = 30000);
        bool IsInBattle();

        // Flight
        void FlyUp();
        void FlyDown();

        // Dialog
        void CloseAllDialogs();
        void OpenMiniMap();

        // Composite
        bool NavigateToNPC(string mapName, string npcName, string locationName, int maxRetries = 3);
        bool GoToAndTalkToNPC(string mapName, string npcName, string locationName);
    }
}

