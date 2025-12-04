using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace AutoVPT.Services
{
    /// <summary>
    /// Service for combat-related operations - pet battles, monster fighting, training.
    /// Extracted from GeneralFunctions to provide single-responsibility combat management.
    /// </summary>
    internal class CombatService : ICombatService
    {
        private readonly AutoFeatures _auto;
        private readonly Character _character;
        private readonly Func<bool> _isRunning;

        /// <summary>
        /// Create a new CombatService instance.
        /// </summary>
        /// <param name="auto">AutoFeatures instance for image operations</param>
        /// <param name="character">Character settings</param>
        /// <param name="isRunningCheck">Optional function to check if character is still running</param>
        public CombatService(AutoFeatures auto, Character character, Func<bool> isRunningCheck = null)
        {
            _auto = auto ?? throw new ArgumentNullException(nameof(auto));
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _isRunning = isRunningCheck ?? (() => _character.Running != 0);
        }

        #region Pet Battles

        /// <summary>
        /// Start a pet battle (Đấu Pet).
        /// </summary>
        public void StartPetBattle()
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;

            _auto.writeStatus("Bắt đầu \"Đấu Pet\"");
            _auto.closeAllDialog();

            // Open pet battle panel
            _auto.writeStatus("Mở bảng đấu pet");
            _auto.clickImageByGroup("global", "daupet", false, false);
            Thread.Sleep(2000);

            // Click challenge button
            _auto.writeStatus("Bấm khiêu chiến");
            _auto.clickImageByGroup("global", "daupetkhieuchien", false, true);
            Thread.Sleep(1000);
            
            // Click again to confirm
            _auto.writeStatus("Bấm khiêu chiến lần 2");
            _auto.clickImageByGroup("global", "daupetkhieuchien", false, true);
            _auto.closeAllDialog();
        }

        /// <summary>
        /// Run pet battles continuously until max loops reached.
        /// </summary>
        /// <param name="maxLoops">Maximum number of battle loops</param>
        /// <param name="intervalMinutes">Minutes between battles</param>
        public void RunPetBattlesToEnd(int maxLoops = 20, int intervalMinutes = 11)
        {
            int numberOfLoop = 0;
            while (numberOfLoop <= maxLoops && _isRunning() && !Helper.IsStoppingAll())
            {
                StartPetBattle();
                numberOfLoop++;
                
                if (numberOfLoop <= maxLoops)
                {
                    Thread.Sleep(intervalMinutes * 60 * 1000);
                }
            }
        }

        /// <summary>
        /// Run both Ảo Ma (illusion training) and Pet battles alternately.
        /// </summary>
        /// <param name="maxLoops">Maximum number of loops</param>
        /// <param name="aoMaLoops">Number of loops to include Ảo Ma</param>
        /// <param name="intervalMinutes">Minutes between cycles</param>
        /// <param name="aoMaAction">Action to perform Ảo Ma</param>
        public void RunAoMaAndPetBattles(int maxLoops = 20, int aoMaLoops = 3, int intervalMinutes = 11, Action aoMaAction = null)
        {
            int numberOfLoop = 0;
            while (numberOfLoop <= maxLoops && _isRunning() && !Helper.IsStoppingAll())
            {
                StartPetBattle();
                
                if (numberOfLoop < aoMaLoops && aoMaAction != null)
                {
                    aoMaAction();
                }

                numberOfLoop++;
                
                if (numberOfLoop <= maxLoops)
                {
                    Thread.Sleep(intervalMinutes * 60 * 1000);
                }
            }
        }

        #endregion

        #region Pet Management

        /// <summary>
        /// Fuse pets by color.
        /// </summary>
        /// <param name="color">Pet color: "trang" (white), "luc" (green), "lam" (blue)</param>
        public void FusePetsByColor(string color)
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;

            int colorPosition = 355 - (color == "trang" ? 0 : color == "luc" ? 40 : 70);
            int petNumber = 0;

            do
            {
                _auto.closeAllDialog();

                // Open pet panel
                _auto.clickImageByGroup("bat_pet", "eppet_bang");
                Thread.Sleep(Constant.TimeShort);

                // Click fusion tab
                _auto.clickImageByGroup("bat_pet", "eppet_bang_check", false, false, 1, 140, 0);
                Thread.Sleep(Constant.TimeShort);

                // Select color
                _auto.clickImageByGroup("bat_pet", "eppet_bang_check", false, false, 1, colorPosition, 50);
                Thread.Sleep(Constant.TimeShort);

                petNumber = 0;
                int loop = 0;
                do
                {
                    // Check if have enough pets
                    if (_auto.findImageByGroup("bat_pet", "eppet_pet"))
                    {
                        petNumber++;
                    }

                    // Scroll down
                    _auto.clickImageByGroup("bat_pet", "eppet_bang_check", false, false, 1, 320, 110);
                    loop++;
                } while (petNumber < 5 && loop < 5);
                
            } while (petNumber >= 5 && _isRunning() && !Helper.IsStoppingAll());
        }

        /// <summary>
        /// Fuse all pet colors.
        /// </summary>
        public void FuseAllPets()
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;

            FusePetsByColor("trang"); // White
            FusePetsByColor("luc");   // Green
            FusePetsByColor("lam");   // Blue
        }

        /// <summary>
        /// Catch pets on the current map.
        /// </summary>
        /// <param name="mapPoints">List of points to check on mini-map</param>
        public void CatchPets(List<Point> mapPoints)
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;

            _auto.writeStatus("Bắt đầu tìm pet để bắt");

            foreach (var point in mapPoints)
            {
                if (!_isRunning() || Helper.IsStoppingAll()) break;

                _auto.closeAllDialog();
                _auto.sendKey(System.Windows.Forms.Keys.Oemtilde);
                _auto.clickPoint(point.X, point.Y);
                Thread.Sleep(Constant.TimeMedium);

                // Wait for movement to complete
                while (_auto.isMoving() && _isRunning() && !Helper.IsStoppingAll())
                {
                    Thread.Sleep(1000);
                }

                // Try to catch pets
                int catchLoop = 0;
                while (catchLoop < 10 && _isRunning() && !Helper.IsStoppingAll())
                {
                    _auto.closeAllDialog();
                    
                    if (_auto.findImageByGroup("bat_pet", "pet"))
                    {
                        _auto.writeStatus("Tìm thấy pet, bắt pet");
                        _auto.clickImageByGroup("bat_pet", "pet");
                        Thread.Sleep(Constant.TimeMedium);
                        _auto.clickImageByGroup("bat_pet", "batpet");
                        Thread.Sleep(Constant.TimeMedium);
                    }
                    
                    catchLoop++;
                }
            }
        }

        #endregion

        #region Monster Training

        /// <summary>
        /// Train by killing monsters at specific coordinates.
        /// </summary>
        /// <param name="x">X coordinate offset</param>
        /// <param name="y">Y coordinate offset</param>
        public void TrainAtPosition(int x, int y)
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;

            _auto.closeAllDialog();
            _auto.clickPoint(x, y);
            Thread.Sleep(Constant.TimeMedium);
        }

        /// <summary>
        /// Train monsters with default coordinates based on character's Chinese setting.
        /// </summary>
        public void TrainMonsters()
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;

            int x = _character.IsChinese == 1 ? 120 : 160;
            int y = _character.IsChinese == 1 ? 460 : 550;

            TrainAtPosition(x, y);
        }

        #endregion

        #region Battle State

        /// <summary>
        /// Check if character is currently in battle.
        /// </summary>
        /// <returns>True if in battle, false otherwise</returns>
        public bool IsInBattle()
        {
            if (!_isRunning()) return false;
            return _auto.dangTrongTranDau();
        }

        /// <summary>
        /// Wait until battle ends.
        /// </summary>
        /// <param name="timeoutMs">Maximum wait time</param>
        /// <returns>True if battle ended, false if timeout</returns>
        public bool WaitForBattleEnd(int timeoutMs = 300000)
        {
            var endTime = DateTime.Now.AddMilliseconds(timeoutMs);

            while (DateTime.Now < endTime)
            {
                if (!_isRunning() || Helper.IsStoppingAll())
                    return false;

                if (!IsInBattle())
                    return true;

                Thread.Sleep(1000);
            }

            return false;
        }

        /// <summary>
        /// Enable auto-battle mode.
        /// </summary>
        public void EnableAutoBattle()
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;
            _auto.batAuto();
        }

        #endregion
    }

    /// <summary>
    /// Interface for combat operations.
    /// </summary>
    public interface ICombatService
    {
        // Pet Battles
        void StartPetBattle();
        void RunPetBattlesToEnd(int maxLoops = 20, int intervalMinutes = 11);
        void RunAoMaAndPetBattles(int maxLoops = 20, int aoMaLoops = 3, int intervalMinutes = 11, Action aoMaAction = null);

        // Pet Management
        void FusePetsByColor(string color);
        void FuseAllPets();
        void CatchPets(List<Point> mapPoints);

        // Monster Training
        void TrainAtPosition(int x, int y);
        void TrainMonsters();

        // Battle State
        bool IsInBattle();
        bool WaitForBattleEnd(int timeoutMs = 300000);
        void EnableAutoBattle();
    }
}

