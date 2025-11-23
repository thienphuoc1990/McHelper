using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for TruMa (Monster Hunting) feature.
    /// Automates the daily monster hunting quest: accept quest, find monsters, complete, turn in.
    /// TODO: Refactor to use new architecture instead of wrapping legacy AutoTruMa class.
    /// </summary>
    public class TruMaExecutor : BaseFeatureExecutor
    {
        public override FeatureType Type => FeatureType.TruMa;

        public TruMaExecutor(
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
                LogInfo("Starting TruMa (Monster Hunting) feature", context);

                // Call legacy implementation
                // TODO: Replace with native async implementation
                bool success = await Task.Run(() =>
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

                    // Create AutoTruMa instance
                    var autoTruMa = new AutoTruMa(
                        context.WindowHandle,
                        context.Character.Identity.Id,
                        autoFeatures
                    );

                    // Run monster hunting automation
                    return autoTruMa.auto();

                }, context.CancellationToken);

                if (success)
                {
                    LogInfo("TruMa completed successfully", context);
                    return FeatureResult.Successful("Monster hunting quest completed");
                }
                else
                {
                    LogWarning("TruMa completed with issues", context);
                    return FeatureResult.Failed("Monster hunting quest had issues");
                }
            }
            catch (Exception ex)
            {
                LogError($"TruMa feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
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
