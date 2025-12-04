using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Threading;

namespace AutoVPT.Services
{
    /// <summary>
    /// Service for daily tasks and rewards - VIP benefits, recovery, wishing tree, etc.
    /// Extracted from GeneralFunctions to provide single-responsibility daily task management.
    /// </summary>
    internal class DailyTaskService : IDailyTaskService
    {
        private readonly AutoFeatures _auto;
        private readonly Character _character;
        private readonly Func<bool> _isRunning;

        /// <summary>
        /// Create a new DailyTaskService instance.
        /// </summary>
        /// <param name="auto">AutoFeatures instance for image operations</param>
        /// <param name="character">Character settings</param>
        /// <param name="isRunningCheck">Optional function to check if character is still running</param>
        public DailyTaskService(AutoFeatures auto, Character character, Func<bool> isRunningCheck = null)
        {
            _auto = auto ?? throw new ArgumentNullException(nameof(auto));
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _isRunning = isRunningCheck ?? (() => _character.Running != 0);
        }

        #region VIP Rewards

        /// <summary>
        /// Collect VIP rewards.
        /// </summary>
        public void CollectVIPRewards()
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;

            _auto.writeStatus("Bắt đầu \"Nhận VIP\"");
            _auto.closeAllDialog();

            // Open VIP panel
            _auto.clickImageByGroup("global", "vip");
            Thread.Sleep(Constant.TimeShort);

            // Collect VIP benefits (click multiple times)
            for (int i = 0; i < 4; i++)
            {
                _auto.clickImageByGroup("global", "nhanvip");
                Thread.Sleep(300);
            }

            // Scroll down to get more rewards
            _auto.clickImageByGroup("global", "xuongvip", false, false, 15);
            Thread.Sleep(Constant.TimeShort);

            // Collect more VIP benefits
            for (int i = 0; i < 2; i++)
            {
                _auto.clickImageByGroup("global", "nhanvip");
                Thread.Sleep(300);
            }

            _auto.closeAllDialog();
            _auto.writeStatus("Hoàn thành \"Nhận VIP\"");
        }

        #endregion

        #region Recovery/Restoration

        /// <summary>
        /// Collect recovery rewards from daily tasks.
        /// </summary>
        public void CollectRecoveryRewards()
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;

            _auto.writeStatus("Bắt đầu \"Nhận hồi phục\"");
            _auto.closeAllDialog();

            // Open daily tasks panel
            OpenDailyTasksPanel();

            // Open recovery tab
            OpenRecoveryTab();

            // Collect each type of recovery
            CollectRecoveryItem("nvhp_nhiemvunghe", "Nhiệm vụ nghề");
            CollectRecoveryItem("nvhp_tuluyenpet", "Tu luyện pet");
            CollectRecoveryItem("nvhp_trian", "Trị an");
            CollectRecoveryItem("nvhp_truma", "Trừ ma");
            CollectRecoveryItem("nvhp_treothuong", "Treo thưởng");

            _auto.closeAllDialog();
            _auto.writeStatus("Hoàn thành \"Nhận hồi phục\"");
        }

        private void OpenDailyTasksPanel()
        {
            while (!_auto.findImageByGroup("global", "nhiemvuhangngay_check") && _isRunning() && !Helper.IsStoppingAll())
            {
                _auto.writeStatus("Mở bảng nhiệm vụ hàng ngày");
                _auto.clickImageByGroup("global", "nhiemvuhangngay");
                Thread.Sleep(Constant.TimeShort);
            }
        }

        private void OpenRecoveryTab()
        {
            while (!_auto.findImageByGroup("global", "nvhn_hoiphuc_check") && _isRunning() && !Helper.IsStoppingAll())
            {
                _auto.writeStatus("Mở bảng nhận hồi phục");
                _auto.clickImageByGroup("global", "nvhn_hoiphuc");
                Thread.Sleep(Constant.TimeShort);
            }
        }

        private void CollectRecoveryItem(string itemName, string displayName)
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;

            if (_auto.findImageByGroup("global", itemName))
            {
                _auto.writeStatus($"Nhận hồi phục {displayName}");
                _auto.clickImageByGroup("global", itemName, false, false, 1, 470, -10);
                Thread.Sleep(Constant.TimeShort);
            }
        }

        #endregion

        #region Wishing Tree

        /// <summary>
        /// Shake the wishing tree for rewards.
        /// </summary>
        /// <param name="maxShakes">Maximum number of shakes</param>
        public void ShakeWishingTree(int maxShakes = 6)
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;

            _auto.writeStatus("Bắt đầu \"Rung cây\"");
            _auto.closeAllDialog();

            // Fly up
            _auto.bay();

            // Click on wishing tree
            _auto.clickImageByGroup("global", "cayuocnguyen", false, false);
            Thread.Sleep(3000);

            // Click again
            _auto.clickImageByGroup("global", "cayuocnguyen2", false, false);

            // Select heartfelt wish
            _auto.clickImageByGroup("global", "uocnguyenthanhtam", true, true);

            // Shake tree
            int shakeCount = 0;
            while (shakeCount < maxShakes && _isRunning() && !Helper.IsStoppingAll())
            {
                _auto.clickImageByGroup("global", "uocnguyenmienphi", false, true);
                Thread.Sleep(1000);
                shakeCount++;
            }

            _auto.closeAllDialog();
            _auto.writeStatus("Hoàn thành \"Rung cây\"");
        }

        #endregion

        #region Space Carving (KGDK)

        /// <summary>
        /// Exchange rewards from Space Carving (Không Gian Điêu Khắc).
        /// </summary>
        public void ExchangeSpaceCarving()
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;

            _auto.writeStatus("Bắt đầu \"Đổi không gian điêu khắc\"");
            _auto.closeAllDialog();

            // Open KGDK panel via quick features
            FindFeatureFromQuickFeatures("khonggiandieukhac");
            Thread.Sleep(2000);

            // Click exchange
            _auto.clickImageByGroup("global", "khonggiandieukhacdoi", false, false);
            Thread.Sleep(Constant.TimeShort);

            // Confirm
            _auto.clickImageByGroup("global", "luachonco", false, true);

            _auto.closeAllDialog();
            _auto.writeStatus("Hoàn thành \"Đổi không gian điêu khắc\"");
        }

        #endregion

        #region Equipment Set

        /// <summary>
        /// Draw equipment set rewards.
        /// </summary>
        public void DrawEquipmentSet()
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;

            _auto.writeStatus("Bắt đầu \"Rút bộ\"");
            _auto.closeAllDialog();

            // Open character panel
            _auto.clickImageByGroup("global", "nhanvat", false, false);
            Thread.Sleep(Constant.TimeShort);

            // Open wardrobe
            _auto.clickImageByGroup("global", "tudo", false, true);
            Thread.Sleep(2000);

            // Open draw panel
            _auto.clickImageByGroup("global", "rutbo", true, true);
            Thread.Sleep(Constant.TimeShort);

            // Draw reward
            _auto.clickImageByGroup("global", "rutthuongbo", false, true);
            Thread.Sleep(Constant.TimeShort);

            // Confirm
            _auto.clickImageByGroup("global", "rutboxacnhan", false, true);

            _auto.closeAllDialog();
            _auto.writeStatus("Hoàn thành \"Rút bộ\"");
        }

        #endregion

        #region KNVU (Kiếp Nạn Vô Ưu)

        /// <summary>
        /// Collect KNVU rewards.
        /// </summary>
        public void CollectKNVURewards()
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;

            _auto.writeStatus("Bắt đầu \"Nhận KNVU\"");
            _auto.closeAllDialog();

            FindFeatureFromQuickFeatures("knvu_kiepnanvouu");

            if (_auto.findImageByGroup("global", "knvu_nhan"))
            {
                _auto.writeStatus("Đang có phần thưởng KNVU, nhận phần thưởng");
                _auto.clickImageByGroup("global", "knvu_nhan", false, false);
                Thread.Sleep(Constant.TimeShort);
                _auto.clickImageByGroup("global", "knvu_co", false, false);
                Thread.Sleep(Constant.TimeShort);
            }

            _auto.closeAllDialog();
            _auto.writeStatus("Hoàn thành \"Nhận KNVU\"");
        }

        #endregion

        #region Ảo Ma (Illusion Training)

        /// <summary>
        /// Perform Ảo Ma (illusion training).
        /// </summary>
        public void PerformAoMa()
        {
            if (!_isRunning() || Helper.IsStoppingAll()) return;

            _auto.writeStatus("Bắt đầu \"Tu luyện ảo ma\"");
            _auto.closeAllDialog();

            // Open Ảo Ma feature
            FindFeatureFromQuickFeatures("aoma_tuluyenaoma");

            // Cancel existing training if any
            if (_auto.findImageByGroup("global", "aoma_huybotuluyen"))
            {
                _auto.writeStatus("Đang có tu luyện ảo ma, hủy bỏ tu luyện");
                _auto.clickImageByGroup("global", "aoma_huybotuluyen");
                Thread.Sleep(Constant.TimeMedium);
                _auto.clickImageByGroup("global", "aoma_co");
            }

            // Collect rewards if available
            if (_auto.findImageByGroup("global", "aoma_nhanphanthuong", true))
            {
                _auto.writeStatus("Đang có phần thưởng tu luyện, nhận phần thưởng");
                _auto.clickImageByGroup("global", "aoma_nhanphanthuong", true);
                Thread.Sleep(Constant.TimeMedium);
                _auto.clickImageByGroup("global", "aoma_co");
            }

            // Start new training
            if (_auto.findImageByGroup("global", "aoma_tuluyen"))
            {
                _auto.writeStatus("Tu luyện");
                _auto.clickImageByGroup("global", "aoma_tuluyen");
                Thread.Sleep(Constant.TimeMedium);
                _auto.clickImageByGroup("global", "aoma_co");
            }

            _auto.closeAllDialog();
            _auto.writeStatus("Hoàn thành \"Tu luyện ảo ma\"");
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Find a feature from the quick features list and open it.
        /// </summary>
        /// <param name="featureName">Feature name to find</param>
        private void FindFeatureFromQuickFeatures(string featureName)
        {
            int loop = 0;
            while (!_auto.findImageByGroup("global", featureName + "_check") && loop < Constant.MaxLoopShort && _isRunning() && !Helper.IsStoppingAll())
            {
                // Scroll to top first
                while (_auto.findImageByGroup("global", "quickFeatureListUpArrow") && !_auto.findImageByGroup("global", featureName) && _isRunning() && !Helper.IsStoppingAll())
                {
                    _auto.writeStatus("Kéo lên đầu quick feature list");
                    _auto.clickImageByGroup("global", "quickFeatureListUpArrow");
                    Thread.Sleep(Constant.TimeShort);
                }

                // Scroll down to find feature
                while (!_auto.findImageByGroup("global", featureName) && _auto.findImageByGroup("global", "quickFeatureListDownArrow") && _isRunning() && !Helper.IsStoppingAll())
                {
                    _auto.writeStatus("Không tìm thấy tính năng, di chuyển sang trang tiếp");
                    _auto.clickImageByGroup("global", "quickFeatureListDownArrow");
                    Thread.Sleep(Constant.TimeMedium);
                }

                // Click on feature if found
                if (_auto.findImageByGroup("global", featureName))
                {
                    _auto.writeStatus("Tìm thấy tính năng, mở tính năng...");
                    _auto.clickImageByGroup("global", featureName);
                    Thread.Sleep(Constant.TimeMedium);
                }
                else
                {
                    _auto.writeStatus("Không tìm thấy tính năng " + featureName);
                }

                loop++;
            }
        }

        #endregion
    }

    /// <summary>
    /// Interface for daily task operations.
    /// </summary>
    public interface IDailyTaskService
    {
        // VIP
        void CollectVIPRewards();

        // Recovery
        void CollectRecoveryRewards();

        // Wishing Tree
        void ShakeWishingTree(int maxShakes = 6);

        // Space Carving
        void ExchangeSpaceCarving();

        // Equipment Set
        void DrawEquipmentSet();

        // KNVU
        void CollectKNVURewards();

        // Ảo Ma
        void PerformAoMa();
    }
}

