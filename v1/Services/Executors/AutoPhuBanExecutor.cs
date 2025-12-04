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
    /// Uses async/await with cancellable delays for responsive "Stop All" handling.
    /// NOTE: Uses legacy AutoFeatures for complex navigation and dungeon mechanics.
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

                // Check for cancellation at start
                ThrowIfCancelled(context);

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

                LogInfo($"Configured {dungeons.Length} dungeons for automation", context);

                // Setup legacy objects
                var legacyCharacter = CharacterAdapter.ToLegacy(context.Character);
                var autoFeatures = new AutoFeatures(
                    context.WindowHandle,
                    context.Character.Identity.Id,
                    context.StatusTextBox,
                    legacyCharacter
                );

                // Create AutoPhuBan instance and set dungeon codes
                var autoPhuBan = new AutoPhuBan(
                    context.WindowHandle,
                    context.Character.Identity.Id,
                    autoFeatures
                );
                
                // Set dungeon codes for legacy methods (must be done before receiving quests)
                autoPhuBan.setPhuBan(dungeons);

                // Step 1: Receive quests at TLT (Tiên Lập Thành) using legacy method
                // This matches the working flow in runNhanAutoPB
                if (!ShouldContinue(context))
                {
                    LogInfo("AutoPhuBan cancelled before quest pickup", context);
                    return FeatureResult.Failed("Cancelled");
                }

                LogInfo("Receiving quests at TLT (Tiên Lập Thành)...", context);
                await Task.Run(() => autoPhuBan.nhanPhuBanTLTByNVHN(), context.CancellationToken);
                
                if (!ShouldContinue(context))
                {
                    LogInfo("AutoPhuBan cancelled after TLT quests", context);
                    return FeatureResult.Failed("Cancelled");
                }
                LogInfo("Completed receiving quests at TLT", context);

                // Step 2: Receive quest at Cổ Đạo if "Thám Hiểm" is configured
                if (dungeonList.Contains("Thám Hiểm"))
                {
                    if (!ShouldContinue(context))
                    {
                        LogInfo("AutoPhuBan cancelled before Cổ Đạo", context);
                        return FeatureResult.Failed("Cancelled");
                    }

                    LogInfo("Receiving Thám Hiểm quest at Cổ Đạo...", context);
                    bool navigated = await Task.Run(() => autoPhuBan.diChuyenDenNhanPhuBan("codao"), context.CancellationToken);
                    
                    if (navigated && ShouldContinue(context))
                    {
                        await Task.Run(() => autoPhuBan.nhanPhuBan("codao"), context.CancellationToken);
                        LogInfo("Completed receiving Thám Hiểm quest at Cổ Đạo", context);
                    }
                    else if (!ShouldContinue(context))
                    {
                        LogInfo("AutoPhuBan cancelled at Cổ Đạo", context);
                        return FeatureResult.Failed("Cancelled");
                    }
                    else
                    {
                        LogWarning("Failed to navigate to Cổ Đạo, skipping Thám Hiểm quest", context);
                    }
                }

                // Step 3: Run dungeon automation using legacy method
                // Note: auto() internally collects rewards first, then runs dungeons
                if (!ShouldContinue(context))
                {
                    LogInfo("AutoPhuBan cancelled before dungeon run", context);
                    return FeatureResult.Failed("Cancelled");
                }

                LogInfo("Starting dungeon run automation...", context);
                await Task.Run(() => autoPhuBan.auto(), context.CancellationToken);

                LogInfo($"AutoPhuBan completed successfully - {dungeons.Length} dungeons processed", context);
                return FeatureResult.Successful($"Completed {dungeons.Length} dungeons: {dungeonList}");
            }
            catch (OperationCanceledException)
            {
                // Feature was cancelled - this is expected when Stop All is pressed
                LogInfo("AutoPhuBan was cancelled", context);
                return FeatureResult.Failed("Cancelled");
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
