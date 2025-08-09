using AutoVPT.Libs;
using AutoVPT.Objects;
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
using System.Windows.Forms;
using System.IO;

namespace AutoVPT
{
    public partial class MainForm : Form
    {
        public Character character;
        public bool renewConfig = false;
        public string current_selected;

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

            Process.Start("flash.exe", character.Link);

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
            character.BugOnline = (this.checkBoxBugOnline.Checked) ? 1 : 0;
            character.MeTran = (this.checkBoxMeTran.Checked) ? 1 : 0;
            character.HaiThuoc = (this.checkBoxHaiThuoc.Checked) ? 1 : 0;
            character.CauCa = (this.checkBoxCauCa.Checked) ? 1 : 0;
            character.AutoThanTu = (this.checkBoxAutoThanTu.Checked) ? 1 : 0;
            character.RunToLast = (this.checkBoxRunAutoToLast.Checked) ? 1 : 0;
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

            // Lấy thông tin danh sách phụ bản
            this.getDanhSachPhuBan();

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
            checkBoxBugOnline.Checked = (character.BugOnline >= 1) ? true : false;
            checkBoxMeTran.Checked = (character.MeTran >= 1) ? true : false;
            checkBoxHaiThuoc.Checked = (character.HaiThuoc >= 1) ? true : false;
            checkBoxCauCa.Checked = (character.CauCa >= 1) ? true : false;
            checkBoxAutoThanTu.Checked = (character.AutoThanTu >= 1) ? true : false;
            checkBoxRunAutoToLast.Checked = (character.RunToLast >= 1) ? true : false;

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

            this.setDanhSachPhuBan();
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

            character.Running = 1;
            updateCharacter();

            // Mở game
            openWindow();

            IntPtr hWnd = IntPtr.Zero;
            // Find define handle of project
            hWnd = AutoControl.FindWindowHandle(null, character.ID);

            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
            }
            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);

            Helper.threadList.Add(new Thread(mMainAuto.run));
            int index = Helper.threadList.Count() - 1;
            Helper.threadList[index].Name = character.ID + "mainauto";
            Helper.threadList[index].Start();
        }

        private void buttonStopAuto_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            character.Running = 0;
            updateCharacter();

            foreach (var thread in Helper.threadList)
            {
                if (thread.Name == (character.ID + "mainauto"))
                {
                    Helper.writeStatus(textBoxStatus, character.ID, "Đã ngừng auto");
                    thread.Abort();
                }

                if (thread.Name == (character.ID + "autoevent"))
                {
                    Helper.writeStatus(textBoxStatus, character.ID, "Đã ngừng auto event");
                    thread.Abort();
                }
            }
        }

        private void buttonOpenTestForm_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            TestForm testForm = new TestForm();

            testForm.Show();
        }

        private void buttonRunEvent_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            character.Running = 2;
            updateCharacter();

            // Mở game
            openWindow();

            IntPtr hWnd = IntPtr.Zero;
            // Find define handle of project
            hWnd = AutoControl.FindWindowHandle(null, character.ID);

            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
            }
            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);

            Helper.threadList.Add(new Thread(mMainAuto.runEvent));
            int index = Helper.threadList.Count() - 1;
            Helper.threadList[index].Name = character.ID + "autoevent";
            Helper.threadList[index].Start();
        }

        private void buttonRunEventBugBay_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            character.Running = 2;
            updateCharacter();

            // Mở game
            openWindow();

            IntPtr hWnd = IntPtr.Zero;
            // Find define handle of project
            hWnd = AutoControl.FindWindowHandle(null, character.ID);

            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
            }
            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);

            Helper.threadList.Add(new Thread(mMainAuto.runEventBugFlight));
            int index = Helper.threadList.Count() - 1;
            Helper.threadList[index].Name = character.ID + "autoevent";
            Helper.threadList[index].Start();
        }

        private void buttonRunEventWithCode_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            character.Running = 2;
            updateCharacter();

            // Mở game
            openWindow();

            IntPtr hWnd = IntPtr.Zero;
            // Find define handle of project
            hWnd = AutoControl.FindWindowHandle(null, character.ID);

            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
            }
            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);

            Helper.threadList.Add(new Thread(mMainAuto.runEventWithCode));
            int index = Helper.threadList.Count() - 1;
            Helper.threadList[index].Name = character.ID + "autoevent";
            Helper.threadList[index].Start();
        }

        private void buttonLoginToGame_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            character.Running = 1;
            updateCharacter();

            // Mở game
            openWindow();

            IntPtr hWnd = IntPtr.Zero;
            // Find define handle of project
            hWnd = AutoControl.FindWindowHandle(null, character.ID);

            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
            }
            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);

            Helper.threadList.Add(new Thread(mMainAuto.loginToGame));
            int index = Helper.threadList.Count() - 1;
            Helper.threadList[index].Name = character.ID + "logintogame";
            Helper.threadList[index].Start();
        }

        private void buttonOpenGameInForm_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            FormVPT formVPT = new FormVPT();
            formVPT.setCharacter(character);

            formVPT.Show();
        }

        private void buttonStopAllAuto_Click(object sender, EventArgs e)
        {
            foreach (var thread in Helper.threadList)
            {
                thread.Abort();
                Helper.writeStatus(textBoxStatus, character.ID, "Đã ngừng " + thread.Name);
            }
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
            } catch {
                Helper.writeStatus(textBoxStatus, character.ID, "Không tạo được thư mục cần thiết!");
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
        }

        private void buttonChayXuQue_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            character.Running = 2;
            updateCharacter();

            // Mở game
            openWindow();

            IntPtr hWnd = IntPtr.Zero;
            // Find define handle of project
            hWnd = AutoControl.FindWindowHandle(null, character.ID);

            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
            }
            MainAuto mMainAuto = new MainAuto(hWnd, character, textBoxStatus);

            Helper.threadList.Add(new Thread(mMainAuto.runXuQue));
            int index = Helper.threadList.Count() - 1;
            Helper.threadList[index].Name = character.ID + "chayxuque";
            Helper.threadList[index].Start();
        }

        private void buttonStopXuQue_Click(object sender, EventArgs e)
        {
            if (!checkSelectCharacter()) { return; }

            character.Running = 0;
            updateCharacter();

            foreach (var thread in Helper.threadList)
            {
                if (thread.Name == (character.ID + "chayxuque"))
                {
                    Helper.writeStatus(textBoxStatus, character.ID, "Đã ngừng auto Xủ Quẻ");
                    thread.Abort();
                }
            }
        }

        private void buttonVaoAllGame_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow item in dataGridViewCharacters.Rows)
            {
                Character currentCharacter = Helper.loadSettingsFromXML(item.Cells[0].Value.ToString());

                // Mở game
                openWindow();

                IntPtr hWnd = IntPtr.Zero;
                // Find define handle of project
                hWnd = AutoControl.FindWindowHandle(null, currentCharacter.ID);

                if (hWnd == IntPtr.Zero)
                {
                    MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                }
                MainAuto mMainAuto = new MainAuto(hWnd, currentCharacter, textBoxStatus);

                Helper.threadList.Add(new Thread(mMainAuto.loginToGame));
                int index = Helper.threadList.Count() - 1;
                Helper.threadList[index].Name = currentCharacter.ID + "logintogame";
                Helper.threadList[index].Start();
            }

            
        }
    }
}
