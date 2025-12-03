using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for TruMa (Monster Hunting) feature.
    /// Automates the daily monster hunting quest: accept quest, find monsters, complete, turn in.
    /// NOTE: Uses legacy AutoFeatures for complex navigation and combat mechanics.
    /// TODO: Refactor navigation to native async when navigation service is available.
    /// </summary>
    public class TruMaExecutor : BaseFeatureExecutor
    {
        public override FeatureType Type => FeatureType.TruMa;

        private List<Monster> _monstersCuma;
        private List<Monster> _monstersCuthu;
        private List<Monster> _monstersPhima;

        public TruMaExecutor(
            IImageRecognition imageRecognition,
            IInputSimulator inputSimulator,
            ILogger logger)
            : base(imageRecognition, inputSimulator, logger)
        {
            // Initialize monster lists for three quest types
            _monstersCuma = InitializeMonsterList("cuma", 8);
            _monstersCuthu = InitializeMonsterList("cuthu", 4);
            _monstersPhima = InitializeMonsterList("phima", 2);
        }

        public override async Task<FeatureResult> ExecuteAsync(ExecutionContext context)
        {
            try
            {
                LogInfo("Starting TruMa (Monster Hunting) feature", context);

                int questCount = 0;
                string currentMonsterType = "phima";

                await Task.Run(() =>
                {
                    // Check global stop flag before starting
                    if (Libs.Helper.IsStoppingAll())
                        return;

                    var legacyCharacter = CharacterAdapter.ToLegacy(context.Character);
                    var autoFeatures = new AutoFeatures(
                        context.WindowHandle,
                        context.Character.Identity.Id,
                        context.StatusTextBox,
                        legacyCharacter
                    );

                    // Main quest loop
                    int loopCount = 0;
                    do
                    {
                        // Check for stop request in loop
                        if (Libs.Helper.IsStoppingAll())
                        {
                            LogInfo("TruMa cancelled during quest loop", context);
                            return;
                        }

                        // Step 1: Accept quest if not already active
                        if (!IsQuestActive(autoFeatures))
                        {
                            LogInfo("No active quest found, accepting new quest...", context);
                            AcceptQuest(autoFeatures);
                        }

                        // Step 2: Check if quest is complete, if not hunt monsters
                        if (!IsQuestCompleted(autoFeatures))
                        {
                            LogInfo("Quest not completed, hunting monsters...", context);

                            // Determine which monster type to hunt
                            currentMonsterType = DetermineMonsterType(autoFeatures);
                            LogInfo($"Monster type identified: {currentMonsterType}", context);

                            // Navigate to monster location
                            NavigateToMonsterMap(autoFeatures, currentMonsterType);

                            // Hunt monsters
                            HuntMonsters(autoFeatures, currentMonsterType);

                            // Return to quest NPC
                            NavigateToQuestNPC(autoFeatures, currentMonsterType);
                        }

                        // Step 3: Turn in quest
                        LogInfo("Turning in quest...", context);
                        TurnInQuest(autoFeatures);
                        questCount++;

                        loopCount++;
                    } while (IsQuestActive(autoFeatures) && loopCount < Constant.MaxLoop);

                }, context.CancellationToken);

                if (questCount > 0)
                {
                    LogInfo($"TruMa completed successfully - {questCount} quest(s) completed", context);
                    return FeatureResult.Successful($"Completed {questCount} monster hunting quest(s)");
                }
                else
                {
                    LogWarning("No quests were completed", context);
                    return FeatureResult.Failed("No quests completed");
                }
            }
            catch (OperationCanceledException)
            {
                // Feature was cancelled - this is expected when Stop All is pressed
                LogInfo("TruMa was cancelled", context);
                return FeatureResult.Failed("Cancelled");
            }
            catch (Exception ex)
            {
                LogError($"TruMa feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
        }

        /// <summary>
        /// Initialize monster image list for a specific monster type
        /// </summary>
        private List<Monster> InitializeMonsterList(string monsterType, int count)
        {
            List<Monster> monsters = new List<Monster>();
            for (int i = 0; i < count; i++)
            {
                string imagePath = Constant.ImagePathTruMaFolder + monsterType + (i + 1).ToString() + ".png";
                monsters.Add(new Monster(imagePath, 0, -20));
            }
            return monsters;
        }

        /// <summary>
        /// Get monster list based on quest type
        /// </summary>
        private List<Monster> GetMonstersByType(string monsterType)
        {
            if (monsterType == "cuma")
                return _monstersCuma;
            else if (monsterType == "cuthu")
                return _monstersCuthu;
            else
                return _monstersPhima;
        }

        /// <summary>
        /// Check if TruMa quest is currently active
        /// </summary>
        private bool IsQuestActive(AutoFeatures autoFeatures)
        {
            autoFeatures.closeAllDialog();

            // Open quest panel
            while (!autoFeatures.findImageByGroup("global", "nhiemvu_check"))
            {
                // Check for stop request
                if (Libs.Helper.IsStoppingAll())
                    return false;
                autoFeatures.clickImageByGroup("global", "nhiemvu");
            }

            // Expand daily quest section
            while (!autoFeatures.findImageByGroup("tru_ma", "bangnhiemvu_nvvongopened", true) &&
                   autoFeatures.findImageByGroup("tru_ma", "bangnhiemvu_nvvong", true))
            {
                // Check for stop request
                if (Libs.Helper.IsStoppingAll())
                    return false;
                autoFeatures.clickImageByGroup("tru_ma", "bangnhiemvu_nvvong", true);
            }

            // Check if TruMa quest exists
            return autoFeatures.findImageByGroup("tru_ma", "nhiemvutruma", true, true);
        }

        /// <summary>
        /// Check if TruMa quest is completed
        /// </summary>
        private bool IsQuestCompleted(AutoFeatures autoFeatures)
        {
            // Ensure quest is visible
            while (!autoFeatures.findImageByGroup("tru_ma", "nhiemvutruma", true, true))
            {
                // Check for stop request
                if (Libs.Helper.IsStoppingAll())
                    return false;
                IsQuestActive(autoFeatures);
            }

            // Check for completion marker
            return autoFeatures.findImageByGroup("tru_ma", "bangnhiemvutrumadaxong", true, true);
        }

        /// <summary>
        /// Determine which monster type to hunt (cuma, cuthu, or phima)
        /// </summary>
        private string DetermineMonsterType(AutoFeatures autoFeatures)
        {
            autoFeatures.closeAllDialog();

            // Open daily quest panel
            while (!autoFeatures.findImageByGroup("nvhn", "bang_check"))
            {
                // Check for stop request
                if (Libs.Helper.IsStoppingAll())
                    return "cuma"; // Default fallback
                autoFeatures.writeStatus("Opening daily quest panel");
                autoFeatures.clickImageByGroup("nvhn", "bang");
                Thread.Sleep(Constant.TimeShort);
            }

            // Open quest list
            while (!autoFeatures.findImageByGroup("global", "nhiemvu_check"))
            {
                // Check for stop request
                if (Libs.Helper.IsStoppingAll())
                    return "cuma"; // Default fallback
                autoFeatures.clickImageByGroup("global", "nhiemvu");
            }

            // Expand daily quest section
            while (!autoFeatures.findImageByGroup("tru_ma", "bangnhiemvu_nvvongopened", true))
            {
                // Check for stop request
                if (Libs.Helper.IsStoppingAll())
                    return "cuma"; // Default fallback
                autoFeatures.clickImageByGroup("tru_ma", "bangnhiemvu_nvvong", true);
            }

            // Click on TruMa quest to see details
            autoFeatures.clickImageByGroup("tru_ma", "nhiemvutruma", true, true);

            // Check monster type icons
            if (autoFeatures.findImageByGroup("tru_ma", "nhiemvucuma", false, false))
            {
                autoFeatures.writeStatus("Quest type: Cự Ma (Giant Demon)");
                return "cuma";
            }
            else if (autoFeatures.findImageByGroup("tru_ma", "nhiemvuphima", false, false))
            {
                autoFeatures.writeStatus("Quest type: Phi Ma (Flying Demon)");
                return "phima";
            }
            else
            {
                autoFeatures.writeStatus("Quest type: Cự Thú (Giant Beast)");
                return "cuthu";
            }
        }

        /// <summary>
        /// Accept TruMa quest from NPC
        /// </summary>
        private void AcceptQuest(AutoFeatures autoFeatures)
        {
            // Navigate to quest NPC using NVHN helper
            int loop = 0;
            while (!autoFeatures.isTalkWithNPC("quanquannhudht") && loop < Constant.MaxLoop)
            {
                autoFeatures.writeStatus("Opening NVHN to find quest NPC...");
                autoFeatures.openQuestByNVHN("truma");
                Thread.Sleep(Constant.TimeShort);
                loop++;
            }

            autoFeatures.writeStatus("Talking with quest NPC");
            autoFeatures.clickImageByGroup("doi_thoai", "quanquannhudht_ndvapt");
            autoFeatures.clickImageByGroup("doi_thoai", "quanquannhudht_truma");
        }

        /// <summary>
        /// Turn in completed quest
        /// </summary>
        private void TurnInQuest(AutoFeatures autoFeatures)
        {
            // Navigate to quest NPC
            int loop = 0;
            while (!autoFeatures.isTalkWithNPC("quanquannhudht") && loop < Constant.MaxLoop)
            {
                autoFeatures.writeStatus("Opening NVHN to find quest NPC...");
                autoFeatures.openQuestByNVHN("truma");
                Thread.Sleep(Constant.TimeShort);
                loop++;
            }

            autoFeatures.writeStatus("Turning in quest");
            autoFeatures.clickImageByGroup("doi_thoai", "quanquannhudht_ndvapt");
            autoFeatures.clickImageByGroup("doi_thoai", "quanquannhudht_truma_phima", false, true);
            autoFeatures.clickImageByGroup("doi_thoai", "quanquannhudht_truma_phongan", false, true);
            autoFeatures.clickImageByGroup("global", "xong", false, true);
        }

        /// <summary>
        /// Navigate to monster hunting map
        /// </summary>
        private void NavigateToMonsterMap(AutoFeatures autoFeatures, string monsterType)
        {
            string npc = "dhtmapchuyendich";
            string location = "dichchuyendht";
            string teleportOption = monsterType == "phima" ?
                "dhtmapchuyendich_tinhlinhthanh" : "dhtmapchuyendich_tramthuylam";
            string mapCheck = monsterType == "phima" ?
                "tinhlinhthanh_check" : "tramthuylam_check";

            // Navigate until we reach the correct map
            while (!autoFeatures.findImageByGroup("maps", mapCheck))
            {
                autoFeatures.closeAllDialog();

                // Navigate to quest NPC first
                int loop = 0;
                while (!autoFeatures.isTalkWithNPC("quanquannhudht") && loop < Constant.MaxLoop)
                {
                    autoFeatures.writeStatus("Opening NVHN to find quest NPC...");
                    autoFeatures.openQuestByNVHN("truma");
                    Thread.Sleep(Constant.TimeShort);
                    loop++;
                }

                // Move to teleport NPC
                autoFeatures.writeStatus($"Navigating to {monsterType} hunting ground");
                while (!autoFeatures.talkToNPC(npc))
                {
                    autoFeatures.moveToNPC(npc, location);
                }

                autoFeatures.writeStatus("Talking with teleport NPC");
                autoFeatures.clickImageByGroup("doi_thoai", teleportOption);
                Thread.Sleep(Constant.TimeMedium);
            }

            autoFeatures.writeStatus("Arrived at monster hunting map");
        }

        /// <summary>
        /// Navigate back to quest NPC from monster map
        /// </summary>
        private void NavigateToQuestNPC(AutoFeatures autoFeatures, string monsterType)
        {
            string npc = monsterType != "phima" ?
                "tramthuylamchuyendich" : "tinhlinhchuyendich";
            string location = monsterType != "phima" ?
                "dichchuyentramthuylam" : "dichchuyentinhlinhthanh";
            string teleportOption = monsterType != "phima" ?
                "tramthuylamchuyendich_dht" : "tinhlinhthanhchuyendich_dht";
            string mapCheck = "donghuyenthanh_check";

            // Navigate back to quest hub
            while (!autoFeatures.findImageByGroup("maps", mapCheck))
            {
                autoFeatures.closeAllDialog();
                autoFeatures.moveToNPC(npc, location);
                autoFeatures.talkToNPC(npc);
                autoFeatures.writeStatus("Talking with return teleport NPC");
                autoFeatures.clickImageByGroup("doi_thoai", teleportOption);
                Thread.Sleep(Constant.TimeMedium);
            }

            autoFeatures.writeStatus("Returned to quest hub");
        }

        /// <summary>
        /// Hunt monsters on the current map
        /// </summary>
        private void HuntMonsters(AutoFeatures autoFeatures, string monsterType)
        {
            List<Monster> monsters = GetMonstersByType(monsterType);
            if (monsters == null || monsters.Count == 0)
            {
                autoFeatures.writeStatus("No monster list available for hunting");
                return;
            }

            // Collect minimap points for navigation
            var mapPoints = autoFeatures.collectMapMiniPoints();
            if (mapPoints == null || mapPoints.Count == 0)
            {
                autoFeatures.writeStatus("No map points found, aborting monster hunt");
                return;
            }

            autoFeatures.writeStatus($"Hunting {monsterType} monsters across {mapPoints.Count} locations");
            autoFeatures.moveAndFindMonsters(mapPoints, monsters, monsterType);
        }

        public override bool CanExecute(ExecutionContext context)
        {
            // Check if feature is enabled
            if (!context.Character.FeatureConfig.IsEnabled(FeatureType.TruMa))
                return false;

            // Check if already completed today
            if (context.Character.RuntimeState.IsCompleted(FeatureType.TruMa))
                return false;

            return true;
        }
    }
}
