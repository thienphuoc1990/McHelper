using AutoVPT.Libs;
using AutoVPT.Objects;
using AutoVPT.Interfaces;
using KAutoHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Emgu.CV.Tracking;

namespace AutoVPT
{
    public partial class MainForm : Form
    {
        public Character character;
        public bool renewConfig = false;
        public string current_selected;
        private RateLimiter batchRateLimiter = new RateLimiter(3); // Max 3 concurrent batch operations

        public MainForm()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Ansi)]
        public static extern bool SetWindowText(IntPtr hWnd, String strNewWindowName);

        // Utilities Functions
        void populate()
        {
            IList list = CharacterList.GetCharacterList();

            this.dataGridViewCharacters.DataSource = list;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            labelAuthorVersion.Text = Constant.Version;

            // Initialize ServiceContainer for dependency injection
            AutoVPT.DependencyInjection.ServiceContainer.Initialize(textBoxStatus);

            populate();
            initConfigs();
        }

        private void buttonXoaNhanVat_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }
            if (current_selected != null)
            {
                CharacterList.DeleteCharacter(current_selected);
            }
            else
            {
                MessageBox.Show("Chưa chọn nhân vật, không thể xóa nhân vật không xác định.");
            }
        }

        private void buttonThemNhanVat_Click(object sender, EventArgs e)
        {
            FormAddCharacter formAddCharacter = new FormAddCharacter();

            formAddCharacter.Show();
        }

        private void buttonSuaNhanVat_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }
            FormAddCharacter formAddCharacter = new FormAddCharacter();
            formAddCharacter.item = current_selected;
            formAddCharacter.loadData();

            formAddCharacter.Show();
        }

        void getCurrentSelectedRow()
        {
            current_selected = dataGridViewCharacters.SelectedRows[0].Cells[0].Value.ToString();

            try
            {
                character = Helper.loadSettingsFromXML(current_selected);
            }
            catch
            {
                character = CharacterList.GetCharacterByRowIndex(dataGridViewCharacters.SelectedRows[0].Index);
            }
        }

        bool checkWindowOpen()
        {
            if (!checkSelectCharacter()) { return false; }
            IntPtr targetHWnd = IntPtr.Zero;

            string targetWindowName = character.ID;

            // Find define handle of project
            targetHWnd = AutoControl.FindWindowHandle(null, targetWindowName);

            if ((targetHWnd != IntPtr.Zero))
            {
                return true;
            }

            return false;
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        void openWindow()
        {
            if (!checkSelectCharacter()) { return; }

            if (checkWindowOpen())
            {
                return;
            }

            IntPtr defaultHWnd = IntPtr.Zero;

            string defaultWindowName = "Adobe Flash Player 10";

            if (character.IsChinese != 1)
            {
                Process.Start("flash.exe", character.Link);
            }
            else
            {
                Process.Start("ForceBindIP.exe", $"{textBoxVpnIp.Text} \"flash.exe\" {character.Link}");
            }
            do
            {
                // Find define handle of project
                defaultHWnd = AutoControl.FindWindowHandle(null, defaultWindowName);

                SetWindowText(defaultHWnd, character.ID);
            } while (defaultHWnd == IntPtr.Zero);

            MoveWindow(defaultHWnd, 0, 0, 500, 500, true);
        }

        private void buttonOpenGame_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }
            openWindow();
        }

        private bool checkSelectCharacter()
        {
            if (character == null)
            {
                MessageBox.Show("Chưa chọn nhân vật, không thể mở ứng dụng");
                return false;
            }

            return true;
        }

        private void buttonSaveConfigAuto_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }
            parsingAndUpdateCharacter();
        }

        private void parsingAndUpdateCharacter()
        {
            DateTime today = DateTime.Today;

            // Update character config from form settings
            character.Date = today.ToString("dd/MM/yyyy");
            character.VipLevel = int.Parse(this.numericUpDownVIPLevel.Value.ToString());
            //character.IncreaseFPS = this.numericUpDownIncreaseFPS.Value.ToString();
            character.VipPromotion = (this.checkBoxNhanVIP.Checked) ? 1 : 0;
            character.DoiNangNo = (this.checkBoxDoiNN.Checked) ? 1 : 0;
            character.DoiNangNoNL4 = (this.checkBoxDoiNLCap4.Checked) ? 1 : 0;
            character.TrongNL = (this.checkBoxTrongNL.Checked) ? 1 : 0;
            character.TriAn = (this.checkBoxTriAn.Checked) ? 1 : 0;
            character.LatTheBai = (this.checkBoxLatTheBai.Checked) ? 1 : 0;
            character.RutBo = (this.checkBoxRutBo.Checked) ? 1 : 0;
            character.DoiKGDK = (this.checkBoxDoiKGDK.Checked) ? 1 : 0;
            character.TuHanh = (this.checkBoxTuHanh.Checked) ? 1 : 0;
            character.TruMa = (this.checkBoxTruMa.Checked) ? 1 : 0;
            character.AoMaThap = (this.checkBoxAoMaThap.Checked) ? 1 : 0;
            character.TrongCay = (this.checkBoxTrongCay.Checked) ? 1 : 0;
            character.CheMatBao = (this.checkBoxCheMatBao.Checked) ? 1 : 0;
            character.AutoPhuBan = (this.checkBoxAutoPhuBan.Checked) ? 1 : 0;
            character.UocNguyen = (this.checkBoxRungCay.Checked) ? 1 : 0;
            character.DauPet = (this.checkBoxDauPet.Checked) ? 1 : 0;
            character.NhanThuongHLVT = (this.checkBoxNhanThuongHanhLang.Checked) ? 1 : 0;
            character.NhanHoiPhuc = (this.checkBoxNhanHoiPhuc.Checked) ? 1 : 0;
            character.MeTran = (this.checkBoxMeTran.Checked) ? 1 : 0;
            character.HaiThuoc = (this.checkBoxHaiThuoc.Checked) ? 1 : 0;
            character.CauCa = (this.checkBoxCauCa.Checked) ? 1 : 0;
            character.AutoThanTu = (this.checkBoxAutoThanTu.Checked) ? 1 : 0;
            character.RunToLast = (this.checkBoxRunAutoToLast.Checked) ? 1 : 0;
            character.IsChinese = (this.checkBoxIsChinese.Checked) ? 1 : 0;
            character.DoiNangNoLoai = this.comboBoxChonNLDoiNN.Text;
            character.TrongNLLoai = this.comboBoxTrongNL.Text;
            character.CheMatBaoLoai = this.comboBoxNguyenLieuMB.Text;
            character.CheMatBaoCap = int.Parse(this.numericUpDownCapMB.Value.ToString());
            character.ViTriNhanVat = int.Parse(this.numericUpDownViTriNhanVat.Value.ToString());

            // Status
            character.StatusCheMatBao = (this.checkBoxStatusCheMB.Checked) ? 1 : 0;
            character.StatusAutoPhuBan = (this.checkBoxStatusNhanVaAutoPB.Checked) ? 1 : 0;
            character.StatusAutoThanTu = (this.checkBoxStatusAutoThanTu.Checked) ? 1 : 0;
            character.StatusTriAn = (this.checkBoxStatusTriAn.Checked) ? 1 : 0;
            character.StatusVipPromotion = (this.checkBoxStatusVipPromotion.Checked) ? 1 : 0;
            character.StatusUocNguyen = (this.checkBoxStatusRungCay.Checked) ? 1 : 0;
            character.StatusTuHanh = (this.checkBoxStatusTuHanh.Checked) ? 1 : 0;
            character.StatusRutBo = (this.checkBoxStatusRutBo.Checked) ? 1 : 0;
            character.StatusNhanThuongHLVT = (this.checkBoxStatusNhanThuongHL.Checked) ? 1 : 0;
            character.StatusDoiKGDK = (this.checkBoxStatusDoiKGDK.Checked) ? 1 : 0;
            character.StatusNhanHoiPhuc = (this.checkBoxStatusNhanHoiPhuc.Checked) ? 1 : 0;

            // Lấy thông tin danh sách phụ bản
            this.getDanhSachPhuBan();
            // Lấy thông tin danh sách STMT
            this.getDanhSachSTMT();

            updateCharacter();
        }

        private void getDanhSachPhuBan()
        {
            string[] phuBan = new string[9];
            int i = 0;
            foreach (string item in checkedListBoxPhuBan.CheckedItems)
            {
                phuBan[i] = item;
                i++;
            }

            character.AutoPhuBanDanhSach = string.Join(",", phuBan.Where(x => !string.IsNullOrEmpty(x)).ToArray());
        }

        private void getDanhSachSTMT()
        {
            string[] stmt = new string[16];
            int i = 0;
            foreach (string item in checkedListBoxSTMT.CheckedItems)
            {
                stmt[i] = item;
                i++;
            }

            character.DanhSTMTDanhSach = string.Join(",", stmt.Where(x => !string.IsNullOrEmpty(x)).ToArray());
        }

        private void updateCharacter()
        {
            Helper.saveSettingsToXML(character);
            //CharacterList.UpdateCharacter(character);
        }

        public void loadData()
        {
            loadCharacterSettings();
        }

        private void checkRenewConfig()
        {
            renewConfig = false;
            try
            {
                DateTime yesterday = DateTime.ParseExact(character.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime today = DateTime.Today;
                int compareDate = DateTime.Compare(yesterday, today);
                if (compareDate < 0)
                {
                    renewConfig = true;
                    Helper.writeStatus(textBoxStatus, character.ID, "Trạng thái và cài đặt của ngày cũ, làm mới trạng thái và cài đặt");
                }
                else
                {
                    Helper.writeStatus(textBoxStatus, character.ID, "Trạng thái và cài đặt mới nhất, không cần tự làm mới.");
                }
            }
            catch
            {
                Helper.writeStatus(textBoxStatus, character.ID, "Không thể kiểm tra phải cài đặt mới nhất không nên giữ nguyên cài đăt và trạng thái.");
            }
        }

        /// <summary>
        /// Automatically check if configuration needs renewal and renew if necessary
        /// This is called before running automation to ensure status flags are reset daily
        /// </summary>
        private void autoRenewConfigIfNeeded()
        {
            try
            {
                // Check if date is empty or invalid
                if (string.IsNullOrEmpty(character.Date))
                {
                    Helper.writeStatus(textBoxStatus, character.ID, "Ngày không hợp lệ, làm mới cấu hình");
                    resetAllStatusFlags();
                    character.Date = DateTime.Today.ToString("dd/MM/yyyy");
                    Helper.saveSettingsToXML(character);
                    return;
                }

                // Parse and compare dates
                DateTime lastRunDate = DateTime.ParseExact(character.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime today = DateTime.Today;
                int compareDate = DateTime.Compare(lastRunDate, today);

                if (compareDate < 0)
                {
                    // Date is old, need to renew
                    Helper.writeStatus(textBoxStatus, character.ID, "Ngày mới, tự động làm mới trạng thái");
                    resetAllStatusFlags();
                    character.Date = today.ToString("dd/MM/yyyy");
                    Helper.saveSettingsToXML(character);
                }
                else
                {
                    Helper.writeStatus(textBoxStatus, character.ID, "Trạng thái đã được cập nhật hôm nay");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(character.ID, "autoRenewConfigIfNeeded", ex);
                Helper.writeStatus(textBoxStatus, character.ID, "Lỗi khi kiểm tra ngày, làm mới cấu hình để an toàn");
                resetAllStatusFlags();
                character.Date = DateTime.Today.ToString("dd/MM/yyyy");
                Helper.saveSettingsToXML(character);
            }
        }

        /// <summary>
        /// Reset all status flags to 0 (not completed)
        /// </summary>
        private void resetAllStatusFlags()
        {
            character.StatusVipPromotion = 0;
            character.StatusDoiNangNo = 0;
            character.StatusTriAn = 0;
            character.StatusLatTheBai = 0;
            character.StatusRutBo = 0;
            character.StatusDoiKGDK = 0;
            character.StatusTuHanh = 0;
            character.StatusTruMa = 0;
            character.StatusAoMaThap = 0;
            character.StatusTrongCay = 0;
            character.StatusCheMatBao = 0;
            character.StatusAutoPhuBan = 0;
            character.StatusUocNguyen = 0;
            character.StatusNhanThuongHLVT = 0;
            character.StatusNhanHoiPhuc = 0;
            character.StatusMeTran = 0;
            character.StatusHaiThuoc = 0;
            character.StatusCauCa = 0;
            character.StatusAutoThanTu = 0;
        }

        private void loadCharacterSettings()
        {
            if (!checkSelectCharacter()) { return; }

            checkRenewConfig();

            this.numericUpDownVIPLevel.Value = character.VipLevel;
            this.numericUpDownIncreaseFPS.Value = decimal.Parse(character.IncreaseFPS.ToString());
            this.comboBoxTrongNL.SelectedIndex = this.comboBoxTrongNL.FindStringExact(character.TrongNLLoai);
            this.comboBoxChonNLDoiNN.SelectedIndex = this.comboBoxChonNLDoiNN.FindStringExact(character.DoiNangNoLoai);
            this.comboBoxNguyenLieuMB.SelectedIndex = this.comboBoxNguyenLieuMB.FindStringExact(character.CheMatBaoLoai);
            this.numericUpDownCapMB.Value = (character.CheMatBaoCap > 1) ? character.CheMatBaoCap : 1;
            this.numericUpDownViTriNhanVat.Value = (character.ViTriNhanVat > 1) ? character.ViTriNhanVat : 1;

            checkBoxNhanVIP.Checked = (character.VipPromotion >= 1) ? true : false;
            checkBoxDoiNN.Checked = (character.DoiNangNo >= 1) ? true : false;
            checkBoxDoiNLCap4.Checked = (character.DoiNangNoNL4 >= 1) ? true : false;
            checkBoxTrongNL.Checked = (character.TrongNL >= 1) ? true : false;
            checkBoxTriAn.Checked = (character.TriAn >= 1) ? true : false;
            checkBoxLatTheBai.Checked = (character.LatTheBai >= 1) ? true : false;
            checkBoxRutBo.Checked = (character.RutBo >= 1) ? true : false;
            checkBoxDoiKGDK.Checked = (character.DoiKGDK >= 1) ? true : false;
            checkBoxTuHanh.Checked = (character.TuHanh >= 1) ? true : false;
            checkBoxTruMa.Checked = (character.TruMa >= 1) ? true : false;
            checkBoxAoMaThap.Checked = (character.AoMaThap >= 1) ? true : false;
            checkBoxTrongCay.Checked = (character.TrongCay >= 1) ? true : false;
            checkBoxCheMatBao.Checked = (character.CheMatBao >= 1) ? true : false;
            checkBoxAutoPhuBan.Checked = (character.AutoPhuBan >= 1) ? true : false;
            checkBoxRungCay.Checked = (character.UocNguyen >= 1) ? true : false;
            checkBoxDauPet.Checked = (character.DauPet >= 1) ? true : false;
            checkBoxNhanThuongHanhLang.Checked = (character.NhanThuongHLVT >= 1) ? true : false;
            checkBoxNhanHoiPhuc.Checked = (character.NhanHoiPhuc >= 1) ? true : false;
            checkBoxMeTran.Checked = (character.MeTran >= 1) ? true : false;
            checkBoxHaiThuoc.Checked = (character.HaiThuoc >= 1) ? true : false;
            checkBoxCauCa.Checked = (character.CauCa >= 1) ? true : false;
            checkBoxAutoThanTu.Checked = (character.AutoThanTu >= 1) ? true : false;
            checkBoxRunAutoToLast.Checked = (character.RunToLast >= 1) ? true : false;
            checkBoxIsChinese.Checked = (character.IsChinese >= 1) ? true : false;

            // Status
            character.StatusCheMatBao = setStateFeature(checkBoxStatusCheMB, character.StatusCheMatBao);
            character.StatusAutoPhuBan = setStateFeature(checkBoxStatusNhanVaAutoPB, character.StatusAutoPhuBan);
            character.StatusAutoThanTu = setStateFeature(checkBoxStatusAutoThanTu, character.StatusAutoThanTu);
            character.StatusTriAn = setStateFeature(checkBoxStatusTriAn, character.StatusTriAn);
            character.StatusVipPromotion = setStateFeature(checkBoxStatusVipPromotion, character.StatusVipPromotion);
            character.StatusUocNguyen = setStateFeature(checkBoxStatusRungCay, character.StatusUocNguyen);
            character.StatusTuHanh = setStateFeature(checkBoxStatusTuHanh, character.StatusTuHanh);
            character.StatusRutBo = setStateFeature(checkBoxStatusRutBo, character.StatusRutBo);
            character.StatusNhanThuongHLVT = setStateFeature(checkBoxStatusNhanThuongHL, character.StatusNhanThuongHLVT);
            character.StatusDoiKGDK = setStateFeature(checkBoxStatusDoiKGDK, character.StatusDoiKGDK);
            character.StatusNhanHoiPhuc = setStateFeature(checkBoxStatusNhanHoiPhuc, character.StatusNhanHoiPhuc);

            this.setDanhSachPhuBan();
            this.setDanhSachSTMT();

            if (renewConfig)
            {
                parsingAndUpdateCharacter();
            }
        }

        private void setDanhSachSTMT()
        {
            string[] list = character.DanhSTMTDanhSach.Split(',');
            for (int count = 0; count < checkedListBoxSTMT.Items.Count; count++)
            {
                if (list.Contains(checkedListBoxSTMT.Items[count].ToString()))
                {
                    checkedListBoxSTMT.SetItemChecked(count, true);
                }
                else
                {
                    checkedListBoxSTMT.SetItemChecked(count, false);
                }
            }
        }

        private void setDanhSachPhuBan()
        {
            string[] list = character.AutoPhuBanDanhSach.Split(',');
            for (int count = 0; count < checkedListBoxPhuBan.Items.Count; count++)
            {
                if (list.Contains(checkedListBoxPhuBan.Items[count].ToString()))
                {
                    checkedListBoxPhuBan.SetItemChecked(count, true);
                }
                else
                {
                    checkedListBoxPhuBan.SetItemChecked(count, false);
                }
            }
        }

        private int setStateFeature(CheckBox checkBox, int status)
        {
            checkBox.Checked = false;

            if (renewConfig)
            {
                status = 0;
            }

            if (status == 1)
            {
                checkBox.Checked = true;
            }

            return status;
        }

        private void buttonRunAuto_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            // Automatically check and renew configuration if needed
            autoRenewConfigIfNeeded();

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.run, "mainauto");
        }

        private void buttonStopAuto_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            character.Running = 0;
            updateCharacter();

            // Cancel all tokens for this character
            var keysToCancel = Helper.cancellationTokens.Keys
                .Where(k => k.StartsWith(character.ID))
                .ToList();

            foreach (var key in keysToCancel)
            {
                Helper.CancelToken(key);
            }

            Helper.writeStatus(textBoxStatus, character.ID, "Đã ngừng auto");
        }

        private void buttonOpenTestForm_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            TestForm testForm = new TestForm();

            testForm.Show();
        }

        private void buttonLoginToGame_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.loginToGame, "logintogame");
        }

        private void buttonStopAllAuto_Click(object sender, EventArgs e)
        {
            // Step 1: Stop all running characters by setting their Running flag to 0
            // This updates the actual Character objects being used by the threads
            // This also sets the _isStoppingAll flag to prevent new features from starting
            Helper.StopAllRunningCharacters();

            // Step 2: Cancel all cancellation tokens
            var allKeys = Helper.cancellationTokens.Keys.ToList();
            foreach (var key in allKeys)
            {
                Helper.CancelToken(key);
            }

            // Step 3: Abort all threads (interrupt sleeping threads and abort if necessary)
            Helper.AbortAllThreads();

            // Step 4: Clear any remaining registered characters
            Helper.runningCharacters.Clear();

            // Step 5: Reset the stopping flag after a short delay to allow threads to stop
            // This prevents new features from starting immediately after stop
            Task.Run(async () =>
            {
                await Task.Delay(2000); // Wait 2 seconds for threads to stop
                Helper.ResetStopAllFlag();
            });

            Helper.writeStatus(textBoxStatus, "ALL", "Đã ngừng tất cả auto");
        }

        private void checkBoxFilterSelectedChar_CheckedChanged(object sender, EventArgs e)
        {
            var logger = DependencyInjection.ServiceContainer.GetService<ILogger>();
            if (logger is Infrastructure.CompositeLogger compositeLogger)
            {
                // Find the UI logger
                var uiLogger = compositeLogger.GetLoggers()
                    .OfType<Infrastructure.UiLogger>()
                    .FirstOrDefault();

                if (uiLogger != null)
                {
                    if (checkBoxFilterSelectedChar.Checked)
                    {
                        // Filter by selected character
                        if (character != null && !string.IsNullOrEmpty(character.ID))
                        {
                            uiLogger.SetCharacterFilter(character.ID);
                            Helper.writeStatus(textBoxStatus, character.ID, $"Filtering logs for character: {character.ID}");
                        }
                        else
                        {
                            MessageBox.Show("Please select a character first.");
                            checkBoxFilterSelectedChar.Checked = false;
                        }
                    }
                    else
                    {
                        // Clear filter (show all)
                        uiLogger.ClearCharacterFilter();
                        Helper.writeStatus(textBoxStatus, "ALL", "Showing logs for all characters");
                    }
                }
            }
        }

        private void buttonClearLog_Click(object sender, EventArgs e)
        {
            textBoxStatus.Clear();
        }

        private void dataGridViewCharacters_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            getCurrentSelectedRow();
            loadData();
        }

        private void initConfigs()
        {
            // Thêm data cho Trồng NL Combo Box
            comboBoxTrongNL.Items.Add(Constant.NameNguyenLieuKimLoai);
            comboBoxTrongNL.Items.Add(Constant.NameNguyenLieuGo);
            comboBoxTrongNL.Items.Add(Constant.NameNguyenLieuLongThu);
            comboBoxTrongNL.Items.Add(Constant.NameNguyenLieuNgoc);
            comboBoxTrongNL.Items.Add(Constant.NameNguyenLieuVai);
            comboBoxTrongNL.Items.Add(Constant.NameNguyenLieuKimLoaiHiem);
            comboBoxTrongNL.Items.Add(Constant.NameNguyenLieuGoTot);
            comboBoxTrongNL.Items.Add(Constant.NameNguyenLieuGamVoc);
            comboBoxTrongNL.Items.Add(Constant.NameNguyenLieuDaThu);
            comboBoxTrongNL.Items.Add(Constant.NameNguyenLieuPhaLe);

            // Thêm data cho Đổi năng nổ Combo Box
            comboBoxChonNLDoiNN.Items.Add(Constant.NameNguyenLieuKimLoaiHiem);
            comboBoxChonNLDoiNN.Items.Add(Constant.NameNguyenLieuGoTot);
            comboBoxChonNLDoiNN.Items.Add(Constant.NameNguyenLieuGamVoc);
            comboBoxChonNLDoiNN.Items.Add(Constant.NameNguyenLieuDaThu);
            comboBoxChonNLDoiNN.Items.Add(Constant.NameNguyenLieuPhaLe);

            // Thêm data cho Loại mật bảo Combo Box
            comboBoxNguyenLieuMB.Items.Add(Constant.NameLoaiMatBaoThanBinh);
            comboBoxNguyenLieuMB.Items.Add(Constant.NameLoaiMatBaoChienTrang);
            comboBoxNguyenLieuMB.Items.Add(Constant.NameLoaiMatBaoPhapSuc);
            comboBoxNguyenLieuMB.Items.Add(Constant.NameLoaiMatBaoVoUu);
            comboBoxNguyenLieuMB.Items.Add(Constant.NameLoaiMatBaoThanhDien);
            comboBoxNguyenLieuMB.Items.Add(Constant.NameLoaiMatBaoHangDong);
            comboBoxNguyenLieuMB.Items.Add(Constant.NameLoaiMatBaoDaiMac);
            comboBoxNguyenLieuMB.Items.Add(Constant.NameLoaiMatBaoDiCanh);
            comboBoxNguyenLieuMB.Items.Add(Constant.NameLoaiMatBaoLietDiem);
            comboBoxNguyenLieuMB.Items.Add(Constant.NameLoaiMatBaoLangHuyet);
            comboBoxNguyenLieuMB.Items.Add(Constant.NameLoaiMatBaoLacVien);

            // Create folder if not exists
            try
            {
                Directory.CreateDirectory(Application.StartupPath + "\\tracking\\");
            }
            catch (Exception ex)
            {
                Logger.LogError("System", "initConfigs - Create tracking directory", ex);
                Helper.writeStatus(textBoxStatus, "System", "Không tạo được thư mục cần thiết!");
            }
        }

        private void buttonResetStatus_Click(object sender, EventArgs e)
        {
            renewConfig = true;

            character.StatusCheMatBao = setStateFeature(checkBoxStatusCheMB, character.StatusCheMatBao);
            character.StatusAutoPhuBan = setStateFeature(checkBoxStatusNhanVaAutoPB, character.StatusAutoPhuBan);
            character.StatusAutoThanTu = setStateFeature(checkBoxStatusAutoThanTu, character.StatusAutoThanTu);
            character.StatusTriAn = setStateFeature(checkBoxStatusTriAn, character.StatusTriAn);
            character.StatusVipPromotion = setStateFeature(checkBoxStatusVipPromotion, character.StatusVipPromotion);
            character.StatusUocNguyen = setStateFeature(checkBoxStatusRungCay, character.StatusUocNguyen);
            character.StatusTuHanh = setStateFeature(checkBoxStatusTuHanh, character.StatusTuHanh);
            character.StatusRutBo = setStateFeature(checkBoxStatusRutBo, character.StatusRutBo);
            character.StatusNhanThuongHLVT = setStateFeature(checkBoxStatusNhanThuongHL, character.StatusNhanThuongHLVT);
            character.StatusDoiKGDK = setStateFeature(checkBoxStatusDoiKGDK, character.StatusDoiKGDK);
            character.StatusNhanHoiPhuc = setStateFeature(checkBoxStatusNhanHoiPhuc, character.StatusNhanHoiPhuc);
        }

        private void buttonChayXuQue_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.runXuQue, "chayxuque");
        }

        private void buttonStopXuQue_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            character.Running = 0;
            updateCharacter();

            string threadKey = character.ID + "chayxuque";
            Helper.CancelToken(threadKey);
            Helper.writeStatus(textBoxStatus, character.ID, "Đã ngừng auto Xủ Quẻ");
        }

        private void buttonVaoAllGame_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
            {
                character = Helper.loadSettingsFromXML(item.Cells[0].Value.ToString());

                if (character.ID != null &&  character.ID != "")
                {
                    IntPtr hWnd = getHandledWindow();
                    if (hWnd == IntPtr.Zero)
                    {
                        MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                        return;
                    }

                    MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
                    runTaskInThread(mMainAuto.loginToGame, "logintogame", character);
                    Thread.Sleep(Constant.VeryTimeShort);
                }
            }
        }

        private void buttonDaPetAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
            {
                character = Helper.loadSettingsFromXML(item.Cells[0].Value.ToString());

                if (character.ID != null && character.ID != "" && checkWindowOpen())
                {
                    IntPtr hWnd = getHandledWindow();
                    if (hWnd == IntPtr.Zero)
                    {
                        MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                        return;
                    }

                    MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
                    runTaskInThread(mMainAuto.daPet, "dapet", character);
                    Thread.Sleep(Constant.VeryTimeShort);
                }
            }
        }

        private void buttonDaPet_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            // Auto-renew config if needed
            autoRenewConfigIfNeeded();

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.daPet, "dapet");
        }

        private void buttonAutoPhuBan_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.autoPhuBan, "autoPhuBan");
        }

        private void buttonCaptureImage_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.captureImage, "captureImage");
        }

        private void buttonAoMa_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.aoMa, "aoma");
        }

        private void buttonAoMaAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
            {
                character = Helper.loadSettingsFromXML(item.Cells[0].Value.ToString());

                if (character.ID != null && character.ID != "" && checkWindowOpen())
                {
                    IntPtr hWnd = getHandledWindow();
                    if (hWnd == IntPtr.Zero)
                    {
                        MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                        return;
                    }

                    MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
                    runTaskInThread(mMainAuto.aoMa, "aoma", character);
                    Thread.Sleep(Constant.VeryTimeShort);
                }
            }
        }

        private void buttonNhanHoiPhuc_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.hoiPhuc, "hoiphuc");
        }

        private void buttonNhanHoiPhucAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
            {
                character = Helper.loadSettingsFromXML(item.Cells[0].Value.ToString());

                if (character.ID != null && character.ID != "" && checkWindowOpen())
                {
                    IntPtr hWnd = getHandledWindow();
                    if (hWnd == IntPtr.Zero)
                    {
                        MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                        return;
                    }

                    MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
                    runTaskInThread(mMainAuto.hoiPhuc, "hoiphuc", character);
                    Thread.Sleep(Constant.VeryTimeShort);
                }
            }
        }

        private void buttonNhanThuongAutoPB_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }
            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.nhanThuongAutoPhuBan, "nhanThuongAutoPhuBan");
        }

        private void buttonDoiNangNo_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }
            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.doiNangNo, "doinangno");
        }

        private void buttonDoiNangNoAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
            {
                var charId = item.Cells[0].Value.ToString();

                // Use rate limiter to prevent overwhelming the system
                batchRateLimiter.Execute(() =>
                {
                    character = Helper.loadSettingsFromXML(charId);

                    if (character.ID != null && character.ID != "" && checkWindowOpen())
                    {
                        // Auto-renew config if needed (for each character)
                        autoRenewConfigIfNeeded();

                        IntPtr hWnd = getHandledWindow();
                        if (hWnd == IntPtr.Zero)
                        {
                            Helper.writeStatus(textBoxStatus, character.ID, "Không tìm thấy nhân vật này đang được chạy.");
                            return;
                        }

                        MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
                        runTaskInThread(mMainAuto.doiNangNo, "doinangno", character);
                        Thread.Sleep(Constant.VeryTimeShort);
                    }
                });
            }
        }

        private void buttonNhanThuongAutoPBAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
            {
                character = Helper.loadSettingsFromXML(item.Cells[0].Value.ToString());

                if (character.ID != null && character.ID != "" && checkWindowOpen())
                {
                    IntPtr hWnd = getHandledWindow();
                    if (hWnd == IntPtr.Zero)
                    {
                        MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                        return;
                    }

                    MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
                    runTaskInThread(mMainAuto.nhanThuongAutoPhuBan, "nhanThuongAutoPhuBan", character);
                    Thread.Sleep(Constant.VeryTimeShort);
                }
            }
        }

        private void buttonNhanHL_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.nhanThuongHanhLang, "nhanThuongHanhLang");
        }

        private void runTaskInThread(ThreadStart action, String actionName, Character customCharacter = null)
        {
            var thread = new Thread(action);
            thread.Name = (customCharacter != null ? customCharacter.ID : character.ID) + actionName;

            lock (Helper.threadList)
            {
                Helper.threadList.Add(thread);
            }

            thread.Start();
        }

        private IntPtr getHandledWindow(Character member = null)
        {
            Character lCharacter = (member != null) ? member : character;
            checkRenewConfig();
            lCharacter.Running = 1;
            updateCharacter();
            openWindow();

            IntPtr hWnd = IntPtr.Zero;
            return AutoControl.FindWindowHandle(null, lCharacter.ID);
        }

        private void buttonAutoTuHanh_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.runAutoTuHanh, "runAutoTuHanh");
        }

        private void buttonNhanThuongKGDK_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.khongGianDieuKhac, "khongGianDieuKhac");
        }

        private void buttonTrongNL_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.trongNL, "trongNL");
        }

        private void buttonChayAutoAllAcc_Click(object sender, EventArgs e)
        {
            Helper.threadList.Add(new Thread(runAllAcc));
            int index = Helper.threadList.Count() - 1;
            Helper.threadList[index].Name = "runAllAcc";
            Helper.threadList[index].Start();
        }

        /// <summary>
        /// Runs automation for all accounts, respecting the maximum concurrent limit.
        /// Handles ThreadInterruptedException gracefully when "Stop All" is pressed.
        /// NOTE: If you see ThreadInterruptedException in the debugger, it's a "first-chance exception"
        /// which is normal - the exception is caught and handled properly. You can continue execution.
        /// </summary>
        private void runAllAcc()
        {
            int rowIndex = 0;
            int soLuongChay = int.Parse(textBoxSoLuongAcc.Text);

            try
            {
                while (rowIndex < dataGridViewCharacters.Rows.Count)
                {
                    // Check if thread should continue (for graceful shutdown)
                    Thread currentThread = Thread.CurrentThread;
                    if (currentThread == null || !currentThread.IsAlive)
                    {
                        break;
                    }

                    DataGridViewRow item = dataGridViewCharacters.Rows[rowIndex];
                    int runningCount = getRunningThreadsWithNameContaining("mainauto").Count;
                    Helper.writeStatus(textBoxStatus, "All acc", "Đang chạy auto " + runningCount.ToString() + " acc");

                    if (runningCount < soLuongChay)
                    {
                        character = Helper.loadSettingsFromXML(item.Cells[0].Value.ToString());

                        if (character.ID != null && character.ID != "")
                        {
                            IntPtr hWnd = getHandledWindow();
                            if (hWnd == IntPtr.Zero)
                            {
                                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                                return;
                            }

                            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
                            runTaskInThread(mMainAuto.run, "mainauto", character);
                            
                            try
                            {
                                Thread.Sleep(Constant.VeryTimeShort);
                            }
                            catch (ThreadInterruptedException)
                            {
                                // Thread was interrupted, exit gracefully
                                Helper.writeStatus(textBoxStatus, "All acc", "Đã dừng chạy auto all");
                                return;
                            }
                        }

                        rowIndex++;
                    }
                    else
                    {
                        // Wait for a slot to become available, but check more frequently for cancellation
                        // Sleep in smaller increments to be more responsive to thread interruption
                        int waitTimeMs = 0;
                        const int checkIntervalMs = 5000; // Check every 5 seconds
                        const int maxWaitMs = 60 * 1000; // Maximum 60 seconds total

                        while (waitTimeMs < maxWaitMs && getRunningThreadsWithNameContaining("mainauto").Count >= soLuongChay)
                        {
                            try
                            {
                                Thread.Sleep(checkIntervalMs);
                                waitTimeMs += checkIntervalMs;
                            }
                            catch (ThreadInterruptedException)
                            {
                                // Thread was interrupted (e.g., by Stop All), exit gracefully
                                Helper.writeStatus(textBoxStatus, "All acc", "Đã dừng chạy auto all");
                                return;
                            }
                        }
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                // Thread was interrupted - this is expected and normal when Stop All is pressed
                // Catching this exception clears the interrupted status automatically
                Helper.writeStatus(textBoxStatus, "All acc", "Đã dừng chạy auto all");
                // Exit gracefully - exception is handled, no need to rethrow
            }
            catch (Exception ex)
            {
                // Only log non-interruption exceptions
                // ThreadInterruptedException is already handled above and should not reach here
                Helper.writeStatus(textBoxStatus, "All acc", $"Lỗi khi chạy auto all: {ex.Message}");
                Logger.LogError("All acc", "runAllAcc", ex);
            }
        }

        private List<Thread> getRunningThreadsWithNameContaining(string searchText)
        {
            // Lấy danh sách các thread đang chạy và có tên chứa chuỗi tìm kiếm
            var runningThreads = Helper.threadList
                .Where(thread =>
                    thread.IsAlive &&  // Kiểm tra thread có đang chạy không
                    !string.IsNullOrEmpty(thread.Name) && // Đảm bảo tên thread không rỗng
                    thread.Name.Contains(searchText))    // Kiểm tra tên thread có chứa chuỗi
                .ToList(); // Chuyển kết quả thành List

            return runningThreads;
        }

        private void buttonAmDpAllToEnd_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
            {
                character = Helper.loadSettingsFromXML(item.Cells[0].Value.ToString());

                if (character.ID != null && character.ID != "" && checkWindowOpen())
                {
                    IntPtr hWnd = getHandledWindow();
                    if (hWnd == IntPtr.Zero)
                    {
                        MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                        return;
                    }

                    MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
                    runTaskInThread(mMainAuto.aoMaDaPetAllToEnd, "aoMaDaPetAllToEnd", character);
                    Thread.Sleep(Constant.VeryTimeShort);
                }
            }
        }

        private void buttonDanhSTMT_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.danhSTMT, "danhSTMT");
        }

        private void buttonNhanKNVU_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.nhanKNVU, "nhanKNVU");
        }

        private void buttonNhanKnvuAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
            {
                character = Helper.loadSettingsFromXML(item.Cells[0].Value.ToString());

                if (character.ID != null && character.ID != "" && checkWindowOpen())
                {
                    IntPtr hWnd = getHandledWindow();
                    if (hWnd == IntPtr.Zero)
                    {
                        MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                        return;
                    }

                    MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
                    runTaskInThread(mMainAuto.nhanKNVU, "nhanKNVU", character);
                    Thread.Sleep(Constant.VeryTimeShort);
                }
            }
        }

        private void buttonDapetAllToEnd_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
            {
                character = Helper.loadSettingsFromXML(item.Cells[0].Value.ToString());

                if (character.ID != null && character.ID != "" && checkWindowOpen())
                {
                    IntPtr hWnd = getHandledWindow();
                    if (hWnd == IntPtr.Zero)
                    {
                        MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                        return;
                    }

                    MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
                    runTaskInThread(mMainAuto.daPetAllToEnd, "daPetAllToEnd", character);
                    Thread.Sleep(Constant.VeryTimeShort);
                }
            }
        }

        private void buttonAllCanhKyTruong_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
            {
                character = Helper.loadSettingsFromXML(item.Cells[0].Value.ToString());

                if (character.ID != null && character.ID != "" && checkWindowOpen())
                {
                    IntPtr hWnd = getHandledWindow();
                    if (hWnd == IntPtr.Zero)
                    {
                        MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                        return;
                    }

                    MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
                    runTaskInThread(mMainAuto.goCanhKyTruong, "goCanhKyTruong", character);
                    Thread.Sleep(Constant.VeryTimeShort);
                }
            }
        }

        private void buttonTaoNhom_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);

            string[] memberList = null;

            foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
            {
                Character member = Helper.loadSettingsFromXML(item.Cells[0].Value.ToString());

                if (character.Group == member.Group && character.ID != member.ID)
                {
                    if (memberList == null)
                    {
                        memberList = new string[] { member.ID };
                    }
                    else
                    {
                        Array.Resize(ref memberList, memberList.Length + 1);
                        memberList[memberList.Length - 1] = member.ID;
                    }

                    if (member.ID != null && member.ID != "")
                    {
                        IntPtr hWndMember = getHandledWindow(member);
                        if (hWndMember == IntPtr.Zero)
                        {
                            MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                            return;
                        }

                        MainAuto mMainAutoMember = new MainAuto(hWndMember, member, textBoxStatus);
                        runTaskInThread(mMainAutoMember.dongYVaoNhom, "dongYVaoNhom", member);
                        Thread.Sleep(Constant.VeryTimeShort);
                    }
                }
            }
            Helper.writeStatus(textBoxStatus, "All", "Đang chạy tạo nhóm cho: " + string.Join(",", memberList));
            mMainAuto.setMembers(memberList);

            runTaskInThread(mMainAuto.taoNhom, "taoNhom");
        }

        private void buttonTrainQuai_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.trainQuai, "trainQuai");
        }

        private void buttonBatPet_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.batPet, "batPet");
        }

        private void buttonTrainByMap_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.trainByMap, "trainByMap");
        }

        private void buttonCheMatBao_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.cheMatBao, "cheMatBao");
        }

        private void buttonCheMbAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
            {
                character = Helper.loadSettingsFromXML(item.Cells[0].Value.ToString());

                if (character.ID != null && character.ID != "" && checkWindowOpen())
                {
                    IntPtr hWnd = getHandledWindow();
                    if (hWnd == IntPtr.Zero)
                    {
                        MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                        return;
                    }

                    MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
                    runTaskInThread(mMainAuto.cheMatBao, "cheMatBao", character);
                    Thread.Sleep(Constant.VeryTimeShort);
                }
            }
        }

        private void buttonTrongNLAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
            {
                character = Helper.loadSettingsFromXML(item.Cells[0].Value.ToString());

                if (character.ID != null && character.ID != "" && checkWindowOpen())
                {
                    IntPtr hWnd = getHandledWindow();
                    if (hWnd == IntPtr.Zero)
                    {
                        MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                        return;
                    }

                    MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
                    runTaskInThread(mMainAuto.trongNL, "trongNL", character);
                    Thread.Sleep(Constant.VeryTimeShort);
                }
            }
        }

        private void buttonTruMa_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            IntPtr hWnd = getHandledWindow();
            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);
            runTaskInThread(mMainAuto.truMa, "truMa");
        }
    }
}
