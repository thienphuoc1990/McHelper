using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for AutoPhuBan (Dungeon Automation) feature.
    /// Automates accepting dungeon quests, running dungeons, and collecting rewards.
    /// NOTE: Uses legacy AutoFeatures for complex navigation and dungeon mechanics.
    /// TODO: Refactor navigation to native async when navigation service is available.
    /// </summary>
    public class AutoPhuBanExecutor : BaseFeatureExecutor
    {
        public override FeatureType Type => FeatureType.AutoPhuBan;

        public AutoPhuBanExecutor(
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
                LogInfo("Starting AutoPhuBan (Dungeon Automation) feature", context);

                // Get dungeon list from configuration
                string dungeonList = context.Config.GetParameter("DanhSach", "");
                if (string.IsNullOrEmpty(dungeonList))
                {
                    LogWarning("No dungeons configured", context);
                    return FeatureResult.Failed("No dungeons configured");
                }

                LogInfo($"Dungeon list: {dungeonList}", context);

                // Parse and validate dungeons
                string[] dungeons = dungeonList.Split(',')
                    .Select(d => d.Trim())
                    .Where(d => !string.IsNullOrEmpty(d))
                    .ToArray();

                if (dungeons.Length == 0)
                {
                    LogWarning("Empty dungeon list after filtering", context);
                    return FeatureResult.Failed("Empty dungeon list");
                }

                // Convert dungeon names to codes
                string[] dungeonCodes = ConvertDungeonNamesToCodes(dungeons);
                int validDungeons = dungeonCodes.Count(d => d != null);

                if (validDungeons == 0)
                {
                    LogWarning("No valid dungeons found in list", context);
                    return FeatureResult.Failed("No valid dungeons configured");
                }

                LogInfo($"Configured {validDungeons} valid dungeons for automation", context);

                // Execute dungeon automation
                await Task.Run(() =>
                {
                    var legacyCharacter = CharacterAdapter.ToLegacy(context.Character);
                    var autoFeatures = new AutoFeatures(
                        context.WindowHandle,
                        context.Character.Identity.Id,
                        context.StatusTextBox,
                        legacyCharacter
                    );

                    // Step 1: Collect dungeon rewards
                    LogInfo("Collecting dungeon completion rewards...", context);
                    CollectDungeonRewards(autoFeatures);

                    // Step 2: Accept TLT quests
                    LogInfo("Accepting quests at TLT (Tiên Lập Thành)...", context);
                    AcceptTLTQuests(autoFeatures, dungeonCodes);

                    // Step 3: Accept Cổ Đạo quest if "Thám Hiểm" is configured
                    if (dungeonList.Contains("Thám Hiểm"))
                    {
                        LogInfo("Accepting Thám Hiểm quest at Cổ Đạo...", context);
                        if (NavigateToCoDaoNPC(autoFeatures))
                        {
                            AcceptCoDaoQuest(autoFeatures, dungeonCodes);
                        }
                        else
                        {
                            LogWarning("Failed to navigate to Cổ Đạo, skipping Thám Hiểm quest", context);
                        }
                    }

                    // Step 4: Run dungeon automation
                    LogInfo("Starting dungeon run automation...", context);
                    RunDungeons(autoFeatures, dungeonCodes);

                }, context.CancellationToken);

                LogInfo($"AutoPhuBan completed successfully - {validDungeons} dungeons processed", context);
                return FeatureResult.Successful($"Completed {validDungeons} dungeons: {dungeonList}");
            }
            catch (Exception ex)
            {
                LogError($"AutoPhuBan feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
        }

        /// <summary>
        /// Convert dungeon display names to internal codes
        /// </summary>
        private string[] ConvertDungeonNamesToCodes(string[] dungeonNames)
        {
            string[] codes = new string[8]; // Max 8 dungeons
            int index = 0;

            foreach (string name in dungeonNames)
            {
                if (index >= 8) break;

                string trimmedName = name.Trim();
                string code = null;

                switch (trimmedName)
                {
                    case "Mê Huyễn Động":
                        code = "mehuyendong";
                        break;
                    case "Kho Báu Đại Mạc":
                        code = "khobaudaimac";
                        break;
                    case "Lục Tiên Cảnh":
                        code = "luctiencanh";
                        break;
                    case "Liệt Diễm Thâm Uyên":
                        code = "lietdiemthamuyen";
                        break;
                    case "Trở Lại Lang Huyệt":
                        code = "trolailanghuyet";
                        break;
                    case "Quỷ Hút Máu":
                        code = "quyhutmau";
                        break;
                    case "Thế Giới Số":
                        code = "thegioiso";
                        break;
                    case "Thám Hiểm":
                        code = "thamhiem";
                        break;
                }

                if (code != null)
                {
                    codes[index++] = code;
                }
            }

            return codes;
        }

        /// <summary>
        /// Collect dungeon completion rewards from the dungeon panel
        /// </summary>
        private void CollectDungeonRewards(AutoFeatures autoFeatures)
        {
            autoFeatures.closeAllDialog();

            // Open dungeon panel via quick feature list
            while (!autoFeatures.findImageByGroup("phu_ban", "hoanthanhphuban_check"))
            {
                // Scroll to top of quick feature list
                while (autoFeatures.findImageByGroup("global", "quickFeatureListUpArrow") &&
                       !autoFeatures.findImageByGroup("phu_ban", "hoanthanhphuban"))
                {
                    autoFeatures.writeStatus("Scrolling to top of quick feature list");
                    autoFeatures.clickImageByGroup("global", "quickFeatureListUpArrow");
                    Thread.Sleep(Constant.TimeShort);
                }

                // Scroll down to find dungeon panel
                while (!autoFeatures.findImageByGroup("phu_ban", "hoanthanhphuban") &&
                       autoFeatures.findImageByGroup("global", "quickFeatureListDownArrow"))
                {
                    autoFeatures.writeStatus("Searching for dungeon panel in quick features");
                    autoFeatures.clickImageByGroup("global", "quickFeatureListDownArrow");
                    Thread.Sleep(Constant.TimeShort);
                }

                // Open dungeon panel
                autoFeatures.clickImageByGroup("phu_ban", "hoanthanhphuban");
                Thread.Sleep(Constant.TimeMedium);
            }

            // Collect rewards from page 1
            int rewardCount = 0;
            while (autoFeatures.findImageByGroup("phu_ban", "nhanthuong") && rewardCount < Constant.MaxLoopQ)
            {
                autoFeatures.clickImageByGroup("phu_ban", "nhanthuong");
                Thread.Sleep(Constant.TimeMedium);
                rewardCount++;
            }

            // Switch to page 2 and collect rewards
            autoFeatures.clickImageByGroup("phu_ban", "nextpage");
            while (autoFeatures.findImageByGroup("phu_ban", "nhanthuong") && rewardCount < Constant.MaxLoopQ + 5)
            {
                autoFeatures.clickImageByGroup("phu_ban", "nhanthuong");
                Thread.Sleep(Constant.TimeMedium);
                rewardCount++;
            }

            autoFeatures.writeStatus($"Collected {rewardCount} dungeon rewards");
        }

        /// <summary>
        /// Accept quests at TLT using NVHN helper
        /// </summary>
        private void AcceptTLTQuests(AutoFeatures autoFeatures, string[] dungeonCodes)
        {
            int questCount = 0;
            for (int i = 0; i < dungeonCodes.Length && dungeonCodes[i] != null; i++)
            {
                if (dungeonCodes[i] == "thamhiem") continue; // Skip Thám Hiểm (handled separately)

                // Open quest dialog via NVHN
                while (!autoFeatures.isTalkWithNPC("sugiamophuban"))
                {
                    autoFeatures.openQuestByNVHN("phuban");
                }

                // Click on the quest
                autoFeatures.clickImageByGroup("phu_ban", dungeonCodes[i], false, true);

                // Accept and complete quest
                autoFeatures.clickImageByGroup("global", "nhannhiemvu", false, true);
                autoFeatures.clickImageByGroup("global", "xong", false, true);

                autoFeatures.writeStatus($"Accepted TLT quest: {dungeonCodes[i]}");
                Thread.Sleep(Constant.TimeShort);
                questCount++;
            }

            autoFeatures.writeStatus($"Accepted {questCount} TLT quests");
        }

        /// <summary>
        /// Navigate to Cổ Đạo NPC
        /// </summary>
        private bool NavigateToCoDaoNPC(AutoFeatures autoFeatures)
        {
            autoFeatures.closeAllDialog();

            if (!autoFeatures.moveToMap("codao", 38, 0))
            {
                autoFeatures.writeStatus("Cannot navigate to Cổ Đạo map");
                return false;
            }

            autoFeatures.bay();

            if (!autoFeatures.moveToNPC("nguoikhaiquat", "nhanphubancd"))
            {
                autoFeatures.writeStatus("Cannot reach Cổ Đạo NPC");
                return false;
            }

            autoFeatures.bayXuong();
            return true;
        }

        /// <summary>
        /// Accept quest at Cổ Đạo
        /// </summary>
        private void AcceptCoDaoQuest(AutoFeatures autoFeatures, string[] dungeonCodes)
        {
            for (int i = 0; i < dungeonCodes.Length && dungeonCodes[i] != null; i++)
            {
                if (dungeonCodes[i] != "thamhiem") continue;

                if (autoFeatures.talkToNPC("nguoikhaiquat", 0, 0, -40))
                {
                    // Click map selection for Thám Hiểm
                    autoFeatures.clickImageByGroup("phu_ban", "nhanbando", false, true);

                    // Click on the quest
                    autoFeatures.clickImageByGroup("phu_ban", dungeonCodes[i], false, true);

                    // Accept and complete quest
                    autoFeatures.clickImageByGroup("global", "nhannhiemvu", false, true);
                    autoFeatures.clickImageByGroup("global", "xong", false, true);

                    autoFeatures.writeStatus("Accepted Thám Hiểm quest at Cổ Đạo");
                    Thread.Sleep(2000);
                    break;
                }
            }
        }

        /// <summary>
        /// Run automated dungeon battles
        /// </summary>
        private void RunDungeons(AutoFeatures autoFeatures, string[] dungeonCodes)
        {
            // Switch to page 1
            autoFeatures.clickImageByGroup("phu_ban", "prevpage");

            int dungeonIndex = 0;
            while (dungeonIndex < dungeonCodes.Length &&
                   dungeonCodes[dungeonIndex] != null &&
                   dungeonIndex < Constant.MaxLoopQ)
            {
                string dungeonCode = dungeonCodes[dungeonIndex];
                autoFeatures.writeStatus($"Running dungeon: {dungeonCode}");

                // Switch page if needed
                if (dungeonCode == "thamhiem")
                {
                    autoFeatures.clickImageByGroup("phu_ban", "nextpage");
                }
                else
                {
                    autoFeatures.clickImageByGroup("phu_ban", "prevpage");
                }

                // Select difficulty for specific dungeons
                if (dungeonCode == "lietdiemthamuyen" ||
                    dungeonCode == "trolailanghuyet" ||
                    dungeonCode == "quyhutmau")
                {
                    autoFeatures.clickImageByGroup("phu_ban", "showchondokho" + dungeonCode, false, false, 1, 120);
                    Thread.Sleep(Constant.TimeShort);
                    autoFeatures.clickImageByGroup("phu_ban", "chonkho", false, false, 1, 10, -25);
                }

                // Start dungeon
                string startButton = "batdau" + dungeonCode;
                autoFeatures.writeStatus($"Clicking start button: {startButton}.png");
                autoFeatures.clickImageByGroup("phu_ban", startButton, false, false, 1, 60, 40);
                autoFeatures.clickImageByGroup("global", "luachonco", false, true);
                Thread.Sleep(Constant.TimeShort);

                dungeonIndex++;
            }

            autoFeatures.writeStatus($"Completed {dungeonIndex} dungeon runs");
        }

        public override bool CanExecute(ExecutionContext context)
        {
            // Check if feature is enabled
            if (!context.Character.FeatureConfig.IsEnabled(FeatureType.AutoPhuBan))
                return false;

            // Check if already completed today
            if (context.Character.RuntimeState.IsCompleted(FeatureType.AutoPhuBan))
                return false;

            // Check if dungeon list is configured
            string dungeonList = context.Config.GetParameter("DanhSach", "");
            if (string.IsNullOrEmpty(dungeonList))
            {
                LogWarning("Cannot execute AutoPhuBan: No dungeons configured", context);
                return false;
            }

            return true;
        }
    }
}
