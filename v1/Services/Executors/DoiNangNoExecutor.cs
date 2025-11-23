using AutoVPT.Configuration;
using AutoVPT.Domain;
using AutoVPT.Interfaces;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Feature executor for DoiNangNo (Exchange Resources) feature.
    /// Demonstrates the Strategy pattern for feature execution.
    /// </summary>
    public class DoiNangNoExecutor : BaseFeatureExecutor
    {
        public override FeatureType Type => FeatureType.DoiNangNo;

        public DoiNangNoExecutor(
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
                LogInfo("Starting DoiNangNo feature", context);

                var config = ConfigurationManager.Instance;
                var resourceType = context.Config.GetParameter("Loai", "Kim Loai");

                LogInfo($"Exchange type: {resourceType}", context);

                // Step 1: Open minimap
                await _inputSimulator.SendKeyAsync(Keys.Oemtilde, delayAfterMs: config.Timing.ShortDelayMs);

                // Step 2: Navigate to NPC
                var npcImagePath = config.Paths.GetImagePath("/in_map/npc_exchange.png", context.Character.Identity.IsChinese);
                var npcLocation = await _imageRecognition.FindImageAsync(npcImagePath);

                if (!npcLocation.HasValue)
                {
                    LogWarning("NPC not found on map", context);
                    return FeatureResult.Failed("NPC not found");
                }

                // Click NPC location
                await _inputSimulator.ClickAsync(npcLocation.Value, delayAfterMs: config.Timing.MediumDelayMs);

                // Step 3: Wait for dialog
                var dialogImagePath = config.Paths.GetImagePath("/global/exchange_dialog.png", context.Character.Identity.IsChinese);
                bool dialogAppeared = await _imageRecognition.WaitForImageAsync(
                    dialogImagePath,
                    TimeSpan.FromSeconds(5),
                    context.CancellationToken
                );

                if (!dialogAppeared)
                {
                    LogWarning("Exchange dialog did not appear", context);
                    return FeatureResult.Failed("Dialog timeout");
                }

                // Step 4: Select resource type
                // (Implementation would find and click the resource type button based on parameter)

                // Step 5: Confirm exchange
                var confirmButtonPath = config.Paths.GetImagePath("/global/confirm_button.png", context.Character.Identity.IsChinese);
                var confirmLocation = await _imageRecognition.FindImageAsync(confirmButtonPath);

                if (confirmLocation.HasValue)
                {
                    await _inputSimulator.ClickAsync(confirmLocation.Value, delayAfterMs: config.Timing.ShortDelayMs);
                }

                // Step 6: Close dialogs
                for (int i = 0; i < 3; i++)
                {
                    await _inputSimulator.SendKeyAsync(Keys.Escape, delayAfterMs: 300);
                }

                LogInfo("DoiNangNo completed successfully", context);
                return FeatureResult.Successful($"Exchanged {resourceType}");
            }
            catch (Exception ex)
            {
                LogError("Error during DoiNangNo execution", ex, context);
                return FeatureResult.Failed("Exception occurred", ex);
            }
        }

        public override bool CanExecute(ExecutionContext context)
        {
            // Check base conditions
            if (!base.CanExecute(context))
                return false;

            // Check feature-specific requirements
            // For example: character level, inventory space, etc.

            return true;
        }
    }
}
