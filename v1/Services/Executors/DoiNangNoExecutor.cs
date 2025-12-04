using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for DoiNangNo (Resource Exchange) feature.
    /// Automates exchanging materials for energy/currency at designated NPCs.
    /// NOTE: Uses legacy AutoFeatures for complex navigation (moveToMap, moveToNPC, bay).
    /// TODO: Refactor navigation to native async when navigation service is available.
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
                LogInfo("Starting DoiNangNo (Resource Exchange) feature", context);

                // Get configuration
                var resourceType = context.Config.GetParameter(Type, "DoiNangNoLoai", Constant.NameNguyenLieuGamVoc);
                var useLevel4 = context.Config.GetParameter(Type, "DoiNangNoNL4", "0") == "1";
                var level = useLevel4 ? 4 : 5;

                LogInfo($"Exchange settings - Resource: {resourceType}, Level: {level}", context);

                // Convert resource type to image suffix
                string resourceCode = GetResourceCode(resourceType);
                if (string.IsNullOrEmpty(resourceCode))
                {
                    LogWarning($"Unknown resource type: {resourceType}", context);
                    return FeatureResult.Failed($"Unknown resource type: {resourceType}");
                }

                int exchangeCount = 0;

                // For complex navigation features, we still need legacy AutoFeatures
                // This is a hybrid approach until navigation is fully refactored
                await Task.Run(() =>
                {
                    var legacyCharacter = CharacterAdapter.ToLegacy(context.Character);
                    var autoFeatures = new AutoFeatures(
                        context.WindowHandle,
                        context.Character.Identity.Id,
                        context.StatusTextBox,
                        legacyCharacter
                    );

                    // Step 1: Navigate to exchange location
                    LogInfo($"Navigating to level {level} exchange location...", context);
                    if (!NavigateToExchangeLocation(autoFeatures, level))
                    {
                        throw new Exception($"Failed to navigate to level {level} exchange location");
                    }

                    // Step 2: Perform exchanges in a loop
                    LogInfo("Starting exchange loop...", context);
                    while (legacyCharacter.Running != 0)
                    {
                        if (PerformExchange(autoFeatures, level, resourceCode))
                        {
                            exchangeCount++;
                            LogInfo($"Exchange #{exchangeCount} successful, waiting 1s before next exchange...", context);
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            LogInfo("Exchange failed or resources exhausted, stopping loop", context);
                            break;
                        }
                    }

                }, context.CancellationToken);

                if (exchangeCount > 0)
                {
                    LogInfo($"DoiNangNo completed successfully - Total exchanges: {exchangeCount}", context);
                    return FeatureResult.Successful($"Exchanged {resourceType} {exchangeCount} time(s)");
                }
                else
                {
                    LogWarning("No exchanges were completed", context);
                    return FeatureResult.Failed("No exchanges completed - may lack resources");
                }
            }
            catch (Exception ex)
            {
                LogError($"DoiNangNo feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
        }

        /// <summary>
        /// Navigate to the exchange location based on level
        /// </summary>
        private bool NavigateToExchangeLocation(AutoFeatures autoFeatures, int level)
        {
            string map = level == 4 ? "tienlapthanh" : "bangboithanh";
            string npc = level == 4 ? "thuonghoitruongtlt" : "thuonghoitruongbbt";
            string location = level == 4 ? "doinangnotlt" : "doinangnobbt";

            // Close all dialogs
            autoFeatures.closeAllDialog();

            // Move to the appropriate map
            if (!autoFeatures.moveToMap(map))
            {
                autoFeatures.writeStatus($"Cannot navigate to {map} map for level {level} exchange");
                return false;
            }

            // Fly up
            autoFeatures.bay();

            // Move to NPC location
            if (!autoFeatures.moveToNPC(npc, location))
            {
                autoFeatures.writeStatus($"Cannot reach NPC at {location} for level {level} exchange");
                return false;
            }

            // Fly down
            autoFeatures.bayXuong();

            return true;
        }

        /// <summary>
        /// Perform a single resource exchange
        /// </summary>
        private bool PerformExchange(AutoFeatures autoFeatures, int level, string resourceCode)
        {
            string npc = level == 4 ? "thuonghoitruongtlt" : "thuonghoitruongbbt";

            // Talk to NPC
            if (!autoFeatures.talkToNPC(npc))
            {
                autoFeatures.writeStatus("Failed to talk to exchange NPC");
                return false;
            }

            // Click on the exchange quest for the specified resource type
            autoFeatures.clickImageByGroup("global", "nvunangno" + resourceCode);

            // Accept the quest
            autoFeatures.clickImageByGroup("global", "nhannhiemvu", false, true);

            // Complete the quest
            autoFeatures.clickImageByGroup("global", "xong", false, true);

            // Check if exchange failed (not enough resources)
            if (autoFeatures.findImageByGroup("global", "khongxong"))
            {
                autoFeatures.writeStatus($"Cannot exchange - insufficient {resourceCode} resources");
                return false;
            }

            autoFeatures.writeStatus($"Successfully exchanged using {resourceCode}");
            return true;
        }

        /// <summary>
        /// Convert resource type name to image file suffix
        /// </summary>
        private string GetResourceCode(string resourceType)
        {
            switch (resourceType)
            {
                case Constant.NameNguyenLieuDaThu:
                    return "dathu";
                case Constant.NameNguyenLieuPhaLe:
                    return "phale";
                case Constant.NameNguyenLieuKimLoaiHiem:
                    return "kimloaihiem";
                case Constant.NameNguyenLieuGoTot:
                    return "gotot";
                case Constant.NameNguyenLieuGamVoc:
                default:
                    return "gamvoc";
            }
        }

        public override bool CanExecute(ExecutionContext context)
        {
            // Check if feature is enabled
            if (!context.Character.FeatureConfig.IsEnabled(FeatureType.DoiNangNo))
                return false;

            // Check if already completed today
            if (context.Character.RuntimeState.IsCompleted(FeatureType.DoiNangNo))
                return false;

            return true;
        }
    }
}
