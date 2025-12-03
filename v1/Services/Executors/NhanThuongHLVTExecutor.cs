using AutoVPT.Domain;
using AutoVPT.Interfaces;
using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Threading.Tasks;

namespace AutoVPT.Services.Executors
{
    /// <summary>
    /// Executor for NhanThuongHLVT (Corridor Rewards) feature.
    /// Automates collecting daily corridor rewards from NPC at Quyền Cô Thành.
    /// NOTE: Uses legacy AutoFeatures for complex navigation (moveToMap, moveToNPC, bay).
    /// TODO: Refactor navigation to native async when navigation service is available.
    /// </summary>
    public class NhanThuongHLVTExecutor : BaseFeatureExecutor
    {
        public override FeatureType Type => FeatureType.NhanThuongHLVT;

        public NhanThuongHLVTExecutor(
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
                // Check global stop flag before starting (avoid throwing exceptions)
                if (Libs.Helper.IsStoppingAll())
                {
                    LogInfo("NhanThuongHLVT cancelled before starting", context);
                    return FeatureResult.Failed("Cancelled");
                }

                LogInfo("Starting NhanThuongHLVT (Corridor Rewards) feature", context);

                // For complex navigation features, we still need legacy AutoFeatures
                // This is a hybrid approach until navigation is fully refactored
                // Note: We don't pass cancellation token to Task.Run because we check it manually
                // This allows the exception to be properly caught by the outer try-catch
                await Task.Run(() =>
                {
                    // Check global stop flag before starting (avoid throwing exceptions)
                    if (Libs.Helper.IsStoppingAll())
                        return;

                    var legacyCharacter = CharacterAdapter.ToLegacy(context.Character);
                    var autoFeatures = new AutoFeatures(
                        context.WindowHandle,
                        context.Character.Identity.Id,
                        context.StatusTextBox,
                        legacyCharacter
                    );

                    // Step 1: Close all dialogs
                    LogInfo("Closing all dialogs...", context);
                    if (Libs.Helper.IsStoppingAll()) return;
                    autoFeatures.closeAllDialog();

                    // Step 2: Navigate to Quyền Cô Thành
                    LogInfo("Navigating to Quyền Cô Thành...", context);
                    if (Libs.Helper.IsStoppingAll()) return;
                    if (!autoFeatures.moveToMap("quyencothanh", 5))
                    {
                        throw new Exception("Failed to navigate to Quyền Cô Thành");
                    }

                    // Step 3: Fly up
                    LogInfo("Flying up...", context);
                    if (Libs.Helper.IsStoppingAll()) return;
                    autoFeatures.bay();

                    // Step 4: Move to NPC
                    LogInfo("Moving to corridor NPC...", context);
                    if (Libs.Helper.IsStoppingAll()) return;
                    if (!autoFeatures.moveToNPC("conghanhlang", "nhanquahanhlang"))
                    {
                        throw new Exception("Failed to reach corridor NPC");
                    }

                    // Step 5: Fly down
                    LogInfo("Flying down...", context);
                    if (Libs.Helper.IsStoppingAll()) return;
                    autoFeatures.bayXuong();

                    // Step 6: Talk to NPC
                    LogInfo("Talking to NPC...", context);
                    if (Libs.Helper.IsStoppingAll()) return;
                    if (!autoFeatures.talkToNPC("conghanhlang", 0, 0, -40))
                    {
                        throw new Exception("Failed to talk to NPC");
                    }

                    // Step 7: Scroll down in dialog
                    LogInfo("Scrolling down...", context);
                    if (Libs.Helper.IsStoppingAll()) return;
                    autoFeatures.clickImageByGroup("global", "keoxuong", false, true, 3);

                    // Step 8: Click receive rewards button
                    LogInfo("Collecting corridor rewards...", context);
                    if (Libs.Helper.IsStoppingAll()) return;
                    autoFeatures.clickImageByGroup("global", "nhanthuonghanhlang", false, true);
                });

                LogInfo("NhanThuongHLVT completed successfully", context);
                return FeatureResult.Successful("Corridor rewards collected");
            }
            catch (OperationCanceledException)
            {
                // Feature was cancelled - this is expected when Stop All is pressed
                LogInfo("NhanThuongHLVT was cancelled", context);
                return FeatureResult.Failed("Cancelled");
            }
            catch (Exception ex)
            {
                LogError($"NhanThuongHLVT feature failed: {ex.Message}", ex, context);
                return FeatureResult.Failed(ex.Message);
            }
        }

        public override bool CanExecute(ExecutionContext context)
        {
            // Check if feature is enabled
            if (!context.Character.FeatureConfig.IsEnabled(FeatureType.NhanThuongHLVT))
                return false;

            // Check if already completed today
            if (context.Character.RuntimeState.IsCompleted(FeatureType.NhanThuongHLVT))
                return false;

            return true;
        }
    }
}
