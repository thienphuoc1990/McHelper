using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for AutoPhuBan (Dungeon Automation) feature.
    /// Automates accepting dungeon quests, running dungeons, and collecting rewards.
    /// TODO: Refactor to use new architecture instead of wrapping legacy AutoPhuBan class.
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

                // Get dungeon array and trim whitespace from each entry
                string[] dungeons = dungeonList.Split(',')
                    .Select(d => d.Trim())
                    .Where(d => !string.IsNullOrEmpty(d))
                    .ToArray();

                if (dungeons.Length == 0)
                {
                    LogWarning("Empty dungeon list after filtering", context);
                    return FeatureResult.Failed("Empty dungeon list");
                }

                // Call legacy implementation
                // TODO: Replace with native async implementation
                await Task.Run(() =>
                {
                    // Convert to legacy Character object
                    var legacyCharacter = CharacterAdapter.ToLegacy(context.Character);

                    // Register character for Stop All functionality
                    Helper.RegisterRunningCharacter(legacyCharacter);

                    try
                    {
                        // Create AutoFeatures instance for legacy code
                        var autoFeatures = new AutoFeatures(
                            context.WindowHandle,
                            context.Character.Identity.Id,
                            context.StatusTextBox,
                            legacyCharacter
                        );

                        // Create AutoPhuBan instance
                        var autoPhuBan = new AutoPhuBan(
                            context.WindowHandle,
                            context.Character.Identity.Id,
                            autoFeatures
                        );

                        // Set dungeon list
                        autoPhuBan.setPhuBan(dungeons);

                        // Receive quests at TLT (Lap Tuyet Dia)
                        autoPhuBan.nhanPhuBanTLTByNVHN();
                        autoFeatures.writeStatus("Completed receiving quests at TLT");

                        // Receive quest at Co Dao if "Thám Hiểm" is in the list
                        if (dungeonList.Contains("Thám Hiểm"))
                        {
                            if (autoPhuBan.diChuyenDenNhanPhuBan("codao"))
                            {
                                autoPhuBan.nhanPhuBan("codao");
                                autoFeatures.writeStatus("Completed receiving Thám Hiểm quest at Cổ Đạo");
                            }
                        }

                        // Run dungeon automation
                        autoFeatures.writeStatus("Starting dungeon runs");
                        autoPhuBan.auto();
                    }
                    finally
                    {
                        // Unregister character when done
                        Helper.UnregisterRunningCharacter(legacyCharacter.ID);
                    }

                }, context.CancellationToken);

                LogInfo($"AutoPhuBan completed successfully for {dungeons.Length} dungeons", context);
                return FeatureResult.Successful($"Completed {dungeons.Length} dungeons: {dungeonList}");
            }
            catch (Exception ex)
            {
                LogError($"AutoPhuBan feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
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
