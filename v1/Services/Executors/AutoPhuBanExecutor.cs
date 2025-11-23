using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
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

                // Get dungeon array
                string[] dungeons = dungeonList.Split(',');
                if (dungeons.Length == 0)
                {
                    LogWarning("Empty dungeon list", context);
                    return FeatureResult.Failed("Empty dungeon list");
                }

                // Call legacy implementation
                // TODO: Replace with native async implementation
                await Task.Run(() =>
                {
                    // Convert to legacy Character object
                    var legacyCharacter = CharacterAdapter.ToLegacy(context.Character);

                    // Create AutoFeatures instance for legacy code
                    var autoFeatures = new AutoFeatures(
                        context.WindowHandle,
                        context.Character.Identity.Id,
                        null, // StatusTextBox not available in ExecutionContext
                        legacyCharacter
                    );

                    // Create AutoPhuBan instance
                    var autoPhuBan = new AutoPhuBan(
                        context.WindowHandle,
                        context.Character.Identity.Id,
                        autoFeatures
                    );

                    // Set dungeon list
                    autoPhuBan.mPhuBan = dungeons;

                    // Run dungeon automation
                    autoPhuBan.auto();

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
