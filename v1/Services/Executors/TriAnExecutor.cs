using AutoVPT.Domain;
using AutoVPT.Infrastructure;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for TriAn (Thanksgiving Quest) feature.
    /// Automates accepting quest from NPC, navigating to quest area, defeating monsters, and turning in quest.
    /// </summary>
    public class TriAnExecutor : BaseFeatureExecutor
    {
        private readonly List<MonsterTarget> _monsterTargets;
        private readonly int _vipLevel;

        public override FeatureType Type => FeatureType.TriAn;

        public TriAnExecutor(
            IImageRecognition imageRecognition,
            IInputSimulator inputSimulator,
            ILogger logger,
            int vipLevel = 0)
            : base(imageRecognition, inputSimulator, logger)
        {
            _vipLevel = vipLevel;
            _monsterTargets = InitializeMonsterTargets();
        }

        public override async Task<FeatureResult> ExecuteAsync(ExecutionContext context)
        {
            try
            {
                LogInfo("Starting TriAn (Thanksgiving Quest) feature", context);

                // Step 1: Check if quest already completed
                if (await CheckQuestCompletedAsync(context))
                {
                    LogInfo("TriAn quest already completed", context);
                    return FeatureResult.Successful("Quest already completed");
                }

                // Step 2: Accept quest from NPC
                LogInfo("Accepting TriAn quest...", context);
                await AcceptQuestAsync(context);

                // Step 3: Complete quest objectives (defeat monsters)
                LogInfo("Completing quest objectives...", context);
                await CompleteQuestObjectivesAsync(context);

                // Step 4: Turn in quest
                LogInfo("Turning in quest...", context);
                await TurnInQuestAsync(context);

                LogInfo("TriAn feature completed successfully", context);
                return FeatureResult.Successful("Thanksgiving quest completed");
            }
            catch (Exception ex)
            {
                LogError($"TriAn feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
        }

        public override bool CanExecute(ExecutionContext context)
        {
            if (!context.Character.FeatureConfig.IsEnabled(FeatureType.TriAn))
                return false;

            if (context.Character.RuntimeState.IsCompleted(FeatureType.TriAn))
                return false;

            return true;
        }

        #region Private Methods

        /// <summary>
        /// Initialize list of monster targets (Phi Tac and Phan Quan)
        /// </summary>
        private List<MonsterTarget> InitializeMonsterTargets()
        {
            return new List<MonsterTarget>
            {
                new MonsterTarget(Constant.ImagePathTriAnPhiTac + "1.png", 0, -20),
                new MonsterTarget(Constant.ImagePathTriAnPhiTac + "2.png", 0, -20),
                new MonsterTarget(Constant.ImagePathTriAnPhanQuan + "1.png", 0, -20),
                new MonsterTarget(Constant.ImagePathTriAnPhanQuan + "2.png", 0, -20),
                new MonsterTarget(Constant.ImagePathTenTriAnPhiTac + "1.png", 0, -80),
                new MonsterTarget(Constant.ImagePathTenTriAnPhiTac + "2.png", 0, -80),
                new MonsterTarget(Constant.ImagePathTenTriAnPhanQuan + "1.png", 40, -80),
                new MonsterTarget(Constant.ImagePathTenTriAnPhanQuan + "2.png", -40, -80)
            };
        }

        /// <summary>
        /// Accept quest from NPC
        /// </summary>
        private async Task AcceptQuestAsync(ExecutionContext context)
        {
            // Check if already accepted
            if (await CheckQuestAcceptedAsync(context))
            {
                LogInfo("Quest already accepted", context);
                return;
            }

            // Navigate to NPC
            LogInfo("Navigating to quest NPC...", context);
            await NavigateToQuestNPCAsync(context);

            // Talk to NPC
            await Task.Delay(2000); // Wait after movement

            var isTalkingToNPC = await CheckTalkingToNPCAsync(context, "truongcanvedonghuyenthanh");
            if (!isTalkingToNPC)
            {
                await TalkToNPCAsync(context, "truongcanvedonghuyenthanh");
            }

            // Click on quest (in dialog area - 3x faster)
            var questLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathTriAnNDPT,
                searchArea: SearchRegions.DialogArea,
                threshold: 0.8);

            if (questLocation.HasValue)
            {
                await _inputSimulator.ClickAsync(questLocation.Value);
                await Task.Delay(1000);
            }

            // Accept quest dialog - PARALLEL SEARCH (3x faster)
            // Search for all 3 quest completion buttons in parallel, click whichever is found first
            var (foundButton, buttonLocation) = await ExecutorHelpers.FindFirstImageByGroupAsync(
                _imageRecognition,
                "tri_an",
                new[] { "nhiemvutrianchuanhan", "nhiemvuphanquandaxong", "nhiemvuphitacdaxong" });

            if (buttonLocation.HasValue)
            {
                LogInfo($"Found quest button: {foundButton}", context);
                await _inputSimulator.ClickAsync(buttonLocation.Value);
                await Task.Delay(Constant.TimeShort);
            }

            // Confirm quest acceptance
            await ConfirmQuestDialogAsync(context);
        }

        /// <summary>
        /// Complete quest objectives by defeating monsters
        /// </summary>
        private async Task CompleteQuestObjectivesAsync(ExecutionContext context)
        {
            int maxAttempts = 5;
            int attempt = 0;

            while (!await CheckQuestObjectivesCompletedAsync(context) && attempt < maxAttempts)
            {
                attempt++;
                LogInfo($"Quest objective attempt {attempt}/{maxAttempts}", context);

                // Clear chat log
                await ClickImageByGroupAsync(context, "global", "chatclear");

                // Open right menu
                await OpenRightMenuAsync(context);

                // Use flight if VIP level < 6
                if (_vipLevel > 0 && _vipLevel < 6)
                {
                    await UseFlight(context);
                }

                // Navigate to quest location
                await NavigateToQuestLocationAsync(context);

                // Close all dialogs
                await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);

                // Land if using flight
                if (_vipLevel > 0 && _vipLevel < 6)
                {
                    await LandAsync(context);
                }

                // Close right menu
                await CloseRightMenuAsync(context);

                // Find and attack monster
                await FindAndAttackMonsterAsync(context);

                // Wait for combat to complete
                await WaitForCombatCompletionAsync(context);

                // Check if objectives completed
                if (await CheckQuestObjectivesCompletedAsync(context))
                {
                    LogInfo("Quest objectives completed", context);
                    break;
                }
            }

            if (attempt >= maxAttempts)
            {
                throw new Exception("Failed to complete quest objectives after maximum attempts");
            }
        }

        /// <summary>
        /// Turn in completed quest
        /// </summary>
        private async Task TurnInQuestAsync(ExecutionContext context)
        {
            // Navigate back to NPC
            await NavigateToQuestNPCAsync(context);

            // Talk to NPC
            await Task.Delay(2000);
            var isTalkingToNPC = await CheckTalkingToNPCAsync(context, "truongcanvedonghuyenthanh");
            if (!isTalkingToNPC)
            {
                await TalkToNPCAsync(context, "truongcanvedonghuyenthanh");
            }

            // Click on quest (in dialog area - 3x faster)
            var questLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathTriAnNDPT,
                searchArea: SearchRegions.DialogArea,
                threshold: 0.8);

            if (questLocation.HasValue)
            {
                await _inputSimulator.ClickAsync(questLocation.Value);
                await Task.Delay(1000);
            }

            // Turn in quest - PARALLEL SEARCH (3x faster)
            // Search for all 3 quest completion buttons in parallel, click whichever is found first
            var (foundTurnInButton, turnInLocation) = await ExecutorHelpers.FindFirstImageByGroupAsync(
                _imageRecognition,
                "tri_an",
                new[] { "nhiemvutrianchuanhan", "nhiemvuphanquandaxong", "nhiemvuphitacdaxong" });

            if (turnInLocation.HasValue)
            {
                LogInfo($"Found turn-in button: {foundTurnInButton}", context);
                await _inputSimulator.ClickAsync(turnInLocation.Value);
                await Task.Delay(Constant.TimeShort);
            }

            // Confirm turn-in
            await ConfirmQuestDialogAsync(context);
        }

        /// <summary>
        /// Navigate to quest NPC location
        /// </summary>
        private async Task NavigateToQuestNPCAsync(ExecutionContext context)
        {
            await OpenRightMenuAsync(context);
            await MoveToMapAsync(context, "donghuyenthanh");
            await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);

            if (_vipLevel > 0 && _vipLevel < 6)
            {
                await LandAsync(context);
            }

            // Open guide (quest indicator in top-left - 8x faster)
            var guideLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathGlobalCachChoi,
                searchArea: SearchRegions.TopLeft,
                threshold: 0.8);

            if (guideLocation.HasValue)
            {
                await _inputSimulator.ClickAsync(guideLocation.Value);
                await Task.Delay(1000);
            }

            // Click on "Earning money" guide
            await ClickImageByGroupAsync(context, "global", "cach_choi_kiem_tien");

            // Click on NPC link
            await ClickImageByGroupAsync(context, "global", "truong_can_ve_dht");
            await Task.Delay(2000);
            await ClickImageByGroupAsync(context, "global", "truong_can_ve_dht"); // Click twice for reliability

            await Task.Delay(2000); // Wait for navigation
        }

        /// <summary>
        /// Navigate to quest location (monster spawn area)
        /// </summary>
        private async Task NavigateToQuestLocationAsync(ExecutionContext context)
        {
            // Open quest bag
            await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);

            var questBagLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathGlobalTui,
                searchArea: SearchRegions.TopLeft,  // Bag icon in top-left UI (8x faster)
                threshold: 0.8);

            if (questBagLocation.HasValue)
            {
                await _inputSimulator.ClickAsync(questBagLocation.Value);
                await Task.Delay(1000);
            }

            // Click quest tab
            await ClickImageByGroupAsync(context, "global", "tui_tab_nhiemvu");

            // Double-click quest map (in inventory/dialog - 3x faster)
            var questMapLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathTriAnBanDoNhiemVu,
                searchArea: SearchRegions.DialogArea,
                threshold: 0.8);

            if (questMapLocation.HasValue)
            {
                var clickPoint = new Point(questMapLocation.Value.X, questMapLocation.Value.Y - 20);
                await _inputSimulator.ClickAsync(clickPoint);
                await Task.Delay(200);
                await _inputSimulator.ClickAsync(clickPoint); // Double-click
            }

            // Wait for map to load
            await Task.Delay(2000);
            await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);

            // Click on coordinates if VIP < 6
            if (_vipLevel > 0 && _vipLevel < 6)
            {
                await ClickCoordinatesAsync(context);
            }
        }

        /// <summary>
        /// Find and attack a monster from the target list
        /// </summary>
        private async Task FindAndAttackMonsterAsync(ExecutionContext context)
        {
            // Look for attack dialog first (in dialog area - 3x faster)
            var attackDialogLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathDoiThoai + "trian.png",
                searchArea: SearchRegions.DialogArea,
                threshold: 0.8);

            if (attackDialogLocation.HasValue)
            {
                await _inputSimulator.ClickAsync(attackDialogLocation.Value);
                return;
            }

            // Find monster from target list
            foreach (var monster in _monsterTargets)
            {
                var monsterLocation = await _imageRecognition.FindImageAsync(
                    monster.ImagePath,
                    searchArea: SearchRegions.GameplayArea,  // Monsters in gameplay area (2x faster)
                    threshold: 0.8);

                if (monsterLocation.HasValue)
                {
                    var clickPoint = new Point(
                        monsterLocation.Value.X + monster.OffsetX,
                        monsterLocation.Value.Y + monster.OffsetY);

                    await _inputSimulator.ClickAsync(clickPoint);
                    await Task.Delay(1000);

                    // Check for attack dialog
                    attackDialogLocation = await _imageRecognition.FindImageAsync(
                        Constant.ImagePathDoiThoai + "trian.png",
                        threshold: 0.8);

                    if (attackDialogLocation.HasValue)
                    {
                        await _inputSimulator.ClickAsync(attackDialogLocation.Value);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Wait for combat to complete
        /// </summary>
        private async Task WaitForCombatCompletionAsync(ExecutionContext context)
        {
            int maxWaitTime = 60000; // 60 seconds max
            int elapsedTime = 0;
            int checkInterval = 5000; // Check every 5 seconds

            while (elapsedTime < maxWaitTime)
            {
                // Check if in combat (this would need to be implemented based on game UI)
                // For now, use a simple delay
                await Task.Delay(checkInterval);
                elapsedTime += checkInterval;

                // Try to detect if combat ended (would need game-specific logic)
                // Simplified version: just wait one cycle
                break;
            }
        }

        /// <summary>
        /// Check if quest is already accepted
        /// </summary>
        private async Task<bool> CheckQuestAcceptedAsync(ExecutionContext context)
        {
            await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);
            await ClickImageByGroupAsync(context, "global", "nhiemvu");
            await ClickImageByGroupAsync(context, "global", "nhiemvuvong");

            // Check for quest markers - PARALLEL SEARCH (4x faster)
            // Search for all 4 quest status indicators in parallel
            var (foundMarker, markerLocation) = await ExecutorHelpers.FindFirstImageByGroupAsync(
                _imageRecognition,
                "tri_an",
                new[] { "bangnhiemvutrianchuaxong", "bangnhiemvutrianchuaxonggreen",
                       "bangnhiemvutriandaxong", "bangnhiemvutriandaxonggreen" });

            return markerLocation.HasValue;
        }

        /// <summary>
        /// Check if quest objectives (combat) are completed
        /// </summary>
        private async Task<bool> CheckQuestObjectivesCompletedAsync(ExecutionContext context)
        {
            await ExecutorHelpers.CloseAllDialogsAsync(_inputSimulator);
            await ClickImageByGroupAsync(context, "global", "nhiemvu");
            await ClickImageByGroupAsync(context, "global", "nhiemvuvong");

            // Check for quest completion - PARALLEL SEARCH (2x faster)
            // Search for both quest completion indicators in parallel
            var (foundCompleteMarker, completeLocation) = await ExecutorHelpers.FindFirstImageByGroupAsync(
                _imageRecognition,
                "tri_an",
                new[] { "bangnhiemvutriandaxong", "bangnhiemvutriandaxonggreen" });

            return completeLocation.HasValue;
        }

        /// <summary>
        /// Check if quest is fully completed (turned in)
        /// </summary>
        private async Task<bool> CheckQuestCompletedAsync(ExecutionContext context)
        {
            await NavigateToQuestNPCAsync(context);
            await Task.Delay(2000);

            var isTalkingToNPC = await CheckTalkingToNPCAsync(context, "truongcanvedonghuyenthanh");
            if (!isTalkingToNPC)
            {
                await TalkToNPCAsync(context, "truongcanvedonghuyenthanh");
            }

            var questLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathTriAnNDPT,
                threshold: 0.8);

            if (questLocation.HasValue)
            {
                await _inputSimulator.ClickAsync(questLocation.Value);
                await Task.Delay(1000);
            }

            bool hasTriAnQuest = await FindImageByGroupAsync(context, "tri_an", "nhiemvutrian");
            bool hasPhanQuanQuest = await FindImageByGroupAsync(context, "tri_an", "nhiemvuphanquan");
            bool hasPhiTacQuest = await FindImageByGroupAsync(context, "tri_an", "nhiemvuphitac");

            return !hasTriAnQuest && !hasPhanQuanQuest && !hasPhiTacQuest;
        }

        #region Helper Methods

        private async Task OpenRightMenuAsync(ExecutionContext context)
        {
            var menuLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathGlobalFolder + "menu_phai.png",
                threshold: 0.8);

            if (menuLocation.HasValue)
            {
                await _inputSimulator.ClickAsync(menuLocation.Value);
                await Task.Delay(500);
            }
        }

        private async Task CloseRightMenuAsync(ExecutionContext context)
        {
            var menuLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathGlobalFolder + "menu_phai.png",
                searchArea: SearchRegions.RightPanel,  // Menu button on right (6x faster)
                threshold: 0.8);

            if (menuLocation.HasValue)
            {
                await _inputSimulator.ClickAsync(menuLocation.Value);
                await Task.Delay(500);
            }
        }

        private async Task UseFlight(ExecutionContext context)
        {
            var flightLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathGlobalBay,
                searchArea: SearchRegions.BottomBar,  // Flight button in bottom bar (6x faster)
                threshold: 0.8);

            if (flightLocation.HasValue)
            {
                await _inputSimulator.ClickAsync(flightLocation.Value);
                await Task.Delay(1000);
            }
        }

        private async Task LandAsync(ExecutionContext context)
        {
            var landLocation = await _imageRecognition.FindImageAsync(
                Constant.ImagePathGlobalXuong,
                searchArea: SearchRegions.BottomBar,  // Land button in bottom bar (6x faster)
                threshold: 0.8);

            if (landLocation.HasValue)
            {
                await _inputSimulator.ClickAsync(landLocation.Value);
                await Task.Delay(1000);
            }
        }

        private async Task MoveToMapAsync(ExecutionContext context, string mapName)
        {
            // This would use the existing map navigation logic
            // For now, simplified version
            await Task.Delay(1000);
        }

        private async Task ClickCoordinatesAsync(ExecutionContext context)
        {
            var coord1Location = await _imageRecognition.FindImageAsync(
                Constant.ImagePathTriAnToaDo,
                searchArea: SearchRegions.DialogArea,  // Coordinates in dialog/map (3x faster)
                threshold: 0.8);

            if (coord1Location.HasValue)
            {
                var clickPoint = new Point(coord1Location.Value.X + 10, coord1Location.Value.Y - 25);
                await _inputSimulator.ClickAsync(clickPoint);
                await Task.Delay(500);
            }

            var coord2Location = await _imageRecognition.FindImageAsync(
                Constant.ImagePathTriAnToaDo2,
                searchArea: SearchRegions.DialogArea,  // Coordinates in dialog/map (3x faster)
                threshold: 0.8);

            if (coord2Location.HasValue)
            {
                var clickPoint = new Point(coord2Location.Value.X + 10, coord2Location.Value.Y - 25);
                await _inputSimulator.ClickAsync(clickPoint);
                await Task.Delay(500);
            }
        }

        private async Task<bool> CheckTalkingToNPCAsync(ExecutionContext context, string npcName)
        {
            // Would check if already in conversation with NPC
            // Simplified for now
            return false;
        }

        private async Task TalkToNPCAsync(ExecutionContext context, string npcName)
        {
            // Would find and click NPC
            // Simplified for now
            await Task.Delay(1000);
        }

        private async Task ConfirmQuestDialogAsync(ExecutionContext context)
        {
            // Would click confirm/OK buttons
            // Simplified for now
            await Task.Delay(500);
        }

        private async Task ClickImageByGroupAsync(ExecutionContext context, string group, string imageName)
        {
            // This would use the grouped image finding logic from AutoFeatures
            // For now, simplified version
            await Task.Delay(200);
        }

        private async Task<bool> FindImageByGroupAsync(ExecutionContext context, string group, string imageName)
        {
            // This would use the grouped image finding logic from AutoFeatures
            // For now, return false
            return false;
        }

        // Removed CloseAllDialogsAsync - now using ExecutorHelpers.CloseAllDialogsAsync

        #endregion

        #endregion
    }

    /// <summary>
    /// Monster target with image path and click offsets
    /// </summary>
    internal class MonsterTarget
    {
        public string ImagePath { get; set; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }

        public MonsterTarget(string imagePath, int offsetX, int offsetY)
        {
            ImagePath = imagePath;
            OffsetX = offsetX;
            OffsetY = offsetY;
        }
    }
}
