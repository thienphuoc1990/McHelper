using AutoVPT.Libs;
using AutoVPT.Objects;
using KAutoHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoVPT
{
    public partial class FormManageAuto : Form
    {
        public Character character;
        public bool renewConfig = false;
        private MainAuto mMainAuto;

        public FormManageAuto()
        {
            InitializeComponent();
        }

        public void loadData()
        {
            initConfigs();
            checkRenewConfig();

            this.numericUpDownVIPLevel.Value = (character.VipLevel > 1) ? character.VipLevel : 0;
            this.numericUpDownIncreaseFPS.Value = (character.IncreaseFPS != "") ? decimal.Parse(character.IncreaseFPS) : 0;
            character.VipPromotion = setStateFeature(this.checkBoxNhanVIP, character.VipPromotion);
            character.DoiNangNo = setStateFeature(this.checkBoxDoiNN, character.DoiNangNo);
            character.DoiNangNoNL4 = setStateFeature(this.checkBoxDoiNLCap4, character.DoiNangNoNL4);
            character.TrongNL = setStateFeature(this.checkBoxTrongNL, character.TrongNL);
            character.TriAn = setStateFeature(this.checkBoxTriAn, character.TriAn);
            character.LatTheBai = setStateFeature(this.checkBoxLatTheBai, character.LatTheBai);
            character.RutBo = setStateFeature(this.checkBoxRutBo, character.RutBo);
            character.DoiKGDK = setStateFeature(this.checkBoxDoiKGDK, character.DoiKGDK);
            character.TuHanh = setStateFeature(this.checkBoxTuHanh, character.TuHanh);
            character.TruMa = setStateFeature(this.checkBoxTruMa, character.TruMa);
            character.AoMaThap = setStateFeature(this.checkBoxAoMaThap, character.AoMaThap);
            character.TrongCay = setStateFeature(this.checkBoxTrongCay, character.TrongCay);
            character.CheMatBao = setStateFeature(this.checkBoxCheMatBao, character.CheMatBao);
            character.AutoPhuBan = setStateFeature(this.checkBoxAutoPhuBan, character.AutoPhuBan);
            character.UocNguyen = setStateFeature(this.checkBoxRungCay, character.UocNguyen);
            character.DauPet = setStateFeature(this.checkBoxDauPet, character.DauPet);
            character.NhanThuongHLVT = setStateFeature(this.checkBoxNhanThuongHanhLang, character.NhanThuongHLVT);
            character.BugOnline = setStateFeature(this.checkBoxBugOnline, character.BugOnline);
            character.MeTran = setStateFeature(this.checkBoxMeTran, character.MeTran);
            character.HaiThuoc = setStateFeature(this.checkBoxHaiThuoc, character.HaiThuoc);
            character.CauCa = setStateFeature(this.checkBoxCauCa, character.CauCa);
            this.comboBoxTrongNL.SelectedIndex = this.comboBoxTrongNL.FindStringExact(character.TrongNLLoai);
            this.comboBoxChonNLDoiNN.SelectedIndex = this.comboBoxChonNLDoiNN.FindStringExact(character.DoiNangNoLoai);
            this.comboBoxNguyenLieuMB.SelectedIndex = this.comboBoxNguyenLieuMB.FindStringExact(character.CheMatBaoLoai);
            this.numericUpDownCapMB.Value = (character.CheMatBaoCap > 1) ? character.CheMatBaoCap : 1;
            this.numericUpDownViTriNhanVat.Value = (character.ViTriNhanVat > 1) ? character.ViTriNhanVat : 1;

            this.setDanhSachPhuBan();
        }

        public void updateStatusFeatures()
        {
            character.VipPromotion = setStateFeature(this.checkBoxNhanVIP, character.VipPromotion);
            character.DoiNangNo = setStateFeature(this.checkBoxDoiNN, character.DoiNangNo);
            character.DoiNangNoNL4 = setStateFeature(this.checkBoxDoiNLCap4, character.DoiNangNoNL4);
            character.TrongNL = setStateFeature(this.checkBoxTrongNL, character.TrongNL);
            character.TriAn = setStateFeature(this.checkBoxTriAn, character.TriAn);
            character.LatTheBai = setStateFeature(this.checkBoxLatTheBai, character.LatTheBai);
            character.RutBo = setStateFeature(this.checkBoxRutBo, character.RutBo);
            character.DoiKGDK = setStateFeature(this.checkBoxDoiKGDK, character.DoiKGDK);
            character.TuHanh = setStateFeature(this.checkBoxTuHanh, character.TuHanh);
            character.TruMa = setStateFeature(this.checkBoxTruMa, character.TruMa);
            character.AoMaThap = setStateFeature(this.checkBoxAoMaThap, character.AoMaThap);
            character.TrongCay = setStateFeature(this.checkBoxTrongCay, character.TrongCay);
            character.CheMatBao = setStateFeature(this.checkBoxCheMatBao, character.CheMatBao);
            character.AutoPhuBan = setStateFeature(this.checkBoxAutoPhuBan, character.AutoPhuBan);
            character.UocNguyen = setStateFeature(this.checkBoxRungCay, character.UocNguyen);
            character.DauPet = setStateFeature(this.checkBoxDauPet, character.DauPet);
            character.NhanThuongHLVT = setStateFeature(this.checkBoxNhanThuongHanhLang, character.NhanThuongHLVT);
            character.BugOnline = setStateFeature(this.checkBoxBugOnline, character.BugOnline);
            character.MeTran = setStateFeature(this.checkBoxMeTran, character.MeTran);
            character.HaiThuoc = setStateFeature(this.checkBoxHaiThuoc, character.HaiThuoc);
            character.CauCa = setStateFeature(this.checkBoxCauCa, character.CauCa);

            try
            {
                CharacterList.UpdateCharacterAllFields(character);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.ToString());
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
            }
        }

        private int setStateFeature(CheckBox checkBox, int status)
        {
            checkBox.BeginInvoke(new Action(() => checkBox.Checked = false));

            if (renewConfig && status >= 1)
            {
                status = 1;
            }

            if (status >= 1)
            {
                checkBox.BeginInvoke(new Action(() => checkBox.Checked = true));
                checkBox.BeginInvoke(new Action(() => checkBox.Text = checkBox.Text.Replace(" - Đang chờ", "")));
                checkBox.BeginInvoke(new Action(() => checkBox.Text = checkBox.Text.Replace(" - Đang chạy", "")));
                checkBox.BeginInvoke(new Action(() => checkBox.Text = checkBox.Text.Replace(" - Đã xong", "")));

                switch (status)
                {
                    case 1:
                        checkBox.BeginInvoke(new Action(() => checkBox.Text = checkBox.Text + " - Đang chờ"));
                        break;
                    case 2:
                        checkBox.BeginInvoke(new Action(() => checkBox.Text = checkBox.Text + " - Đang chạy"));
                        break;
                    case 3:
                        checkBox.BeginInvoke(new Action(() => checkBox.Text = checkBox.Text + " - Đã xong"));
                        break;
                    default:
                        break;
                }
            }

            return status;
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
        }

        private void checkRenewConfig()
        {
            try
            {
                DateTime yesterday = DateTime.ParseExact(character.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                DateTime today = DateTime.Today;
                int compareDate = DateTime.Compare(yesterday, today);
                if (compareDate < 0)
                {
                    renewConfig = true;
                    Helper.writeStatus(textBoxStatus, "Trạng thái và cài đặt của ngày cũ, làm mới trạng thái và cài đặt");
                }
                else
                {
                    Helper.writeStatus(textBoxStatus, "Trạng thái và cài đặt mới nhất, không cần tự làm mới.");
                }
            }
            catch
            {
                Helper.writeStatus(textBoxStatus, "Không thể kiểm tra phải cài đặt mới nhất không nên giữ nguyên cài đăt và trạng thái.");
            }
        }

        private void FormManageAuto_Load(object sender, EventArgs e)
        {
            labelAuthorVersion.Text = "AutoVPT Version 1.0 - Tử La Lan - https://www.facebook.com/Tu.La.Lan.NT";

            loadData();

            IntPtr hWnd = IntPtr.Zero;
            // Find define handle of project
            hWnd = AutoControl.FindWindowHandle(null, character.ID);

            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
            }
            mMainAuto = new MainAuto(hWnd, character, textBoxStatus, this);

            if (renewConfig)
            {
                parsingAndUpdateCharacter();
            }
        }

        private void parsingAndUpdateCharacter()
        {
            DateTime today = DateTime.Today;

            // Update character config from form settings
            character.Date = today.ToString("dd/MM/yyyy");
            character.VipLevel = int.Parse(this.numericUpDownVIPLevel.Value.ToString());
            character.IncreaseFPS = this.numericUpDownIncreaseFPS.Value.ToString();
            character.VipPromotion = (this.checkBoxNhanVIP.Checked) ? ((character.VipPromotion >= 1) ? character.VipPromotion : 1) : 0;
            character.DoiNangNo = (this.checkBoxDoiNN.Checked) ? ((character.DoiNangNo >= 1) ? character.DoiNangNo : 1) : 0;
            character.DoiNangNoNL4 = (this.checkBoxDoiNLCap4.Checked) ? ((character.DoiNangNoNL4 >= 1) ? character.DoiNangNoNL4 : 1) : 0;
            character.TrongNL = (this.checkBoxTrongNL.Checked) ? ((character.TrongNL >= 1) ? character.TrongNL : 1) : 0;
            character.TriAn = (this.checkBoxTriAn.Checked) ? ((character.TriAn >= 1) ? character.TriAn : 1) : 0;
            character.LatTheBai = (this.checkBoxLatTheBai.Checked) ? ((character.LatTheBai >= 1) ? character.LatTheBai : 1) : 0;
            character.RutBo = (this.checkBoxRutBo.Checked) ? ((character.RutBo >= 1) ? character.RutBo : 1) : 0;
            character.DoiKGDK = (this.checkBoxDoiKGDK.Checked) ? ((character.DoiKGDK >= 1) ? character.DoiKGDK : 1) : 0;
            character.TuHanh = (this.checkBoxTuHanh.Checked) ? ((character.TuHanh >= 1) ? character.TuHanh : 1) : 0;
            character.TruMa = (this.checkBoxTruMa.Checked) ? ((character.TruMa >= 1) ? character.TruMa : 1) : 0;
            character.AoMaThap = (this.checkBoxAoMaThap.Checked) ? ((character.AoMaThap >= 1) ? character.AoMaThap : 1) : 0;
            character.TrongCay = (this.checkBoxTrongCay.Checked) ? ((character.TrongCay >= 1) ? character.TrongCay : 1) : 0;
            character.CheMatBao = (this.checkBoxCheMatBao.Checked) ? ((character.CheMatBao >= 1) ? character.CheMatBao : 1) : 0;
            character.AutoPhuBan = (this.checkBoxAutoPhuBan.Checked) ? ((character.AutoPhuBan >= 1) ? character.AutoPhuBan : 1) : 0;
            character.UocNguyen = (this.checkBoxRungCay.Checked) ? ((character.UocNguyen >= 1) ? character.UocNguyen : 1) : 0;
            character.DauPet = (this.checkBoxDauPet.Checked) ? ((character.DauPet >= 1) ? character.DauPet : 1) : 0;
            character.NhanThuongHLVT = (this.checkBoxNhanThuongHanhLang.Checked) ? ((character.NhanThuongHLVT >= 1) ? character.NhanThuongHLVT : 1) : 0;
            character.BugOnline = (this.checkBoxBugOnline.Checked) ? ((character.BugOnline >= 1) ? character.BugOnline : 1) : 0;
            character.MeTran = (this.checkBoxMeTran.Checked) ? ((character.MeTran >= 1) ? character.MeTran : 1) : 0;
            character.HaiThuoc = (this.checkBoxHaiThuoc.Checked) ? ((character.HaiThuoc >= 1) ? character.HaiThuoc : 1) : 0;
            character.CauCa = (this.checkBoxCauCa.Checked) ? ((character.CauCa >= 1) ? character.CauCa : 1) : 0;
            character.DoiNangNoLoai = this.comboBoxChonNLDoiNN.Text;
            character.TrongNLLoai = this.comboBoxTrongNL.Text;
            character.CheMatBaoLoai = this.comboBoxNguyenLieuMB.Text;
            character.CheMatBaoCap = int.Parse(this.numericUpDownCapMB.Value.ToString());
            character.ViTriNhanVat = int.Parse(this.numericUpDownViTriNhanVat.Value.ToString());

            // Lấy thông tin danh sách phụ bản
            this.getDanhSachPhuBan();

            updateCharacter();
        }

        private void updateCharacter()
        {
            try
            {
                CharacterList.UpdateCharacterAllFields(character);
                Helper.writeStatus(textBoxStatus, "Cập nhật character " + character.ID + " thành công.");
            }
            catch (Exception exp)
            {
                Helper.writeStatus(textBoxStatus, exp.ToString());
                Helper.writeStatus(textBoxStatus, "Cập nhật character " + character.ID + " không thành công.");
            }
        }

        private void buttonSaveConfig_Click(object sender, EventArgs e)
        {
            parsingAndUpdateCharacter();
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

        private void buttonStartAuto_Click(object sender, EventArgs e)
        {
            character.Running = 1;
            updateCharacter();

            Helper.threadList.Add(new Thread(mMainAuto.run));
            int index = Helper.threadList.Count() - 1;
            Helper.threadList[index].Name = character.ID + "mainauto";
            Helper.threadList[index].Start();
        }

        private void buttonStopAuto_Click(object sender, EventArgs e)
        {
            character.Running = 0;
            updateCharacter();

            foreach (var thread in Helper.threadList)
            {
                if(thread.Name == (character.ID + "mainauto"))
                {
                    Helper.writeStatus(textBoxStatus, "Đã ngừng auto");
                    thread.Abort();
                }
            }
        }

        private void buttonRightClick_Click(object sender, EventArgs e)
        {
            mMainAuto.testRightClick(textBoxRightClickGroup.Text, textBoxRightClickName.Text, int.Parse(numericUpDownX.Value.ToString()), int.Parse(numericUpDownY.Value.ToString()));
        }
    }
}
