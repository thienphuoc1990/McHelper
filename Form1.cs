using AutoVPT.Libs;
using AutoVPT.Objects;
using KAutoHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

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
        }

        private void buttonXoaNhanVat_Click(object sender, EventArgs e)
        {
            getCurrentSelectedRow();
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
            getCurrentSelectedRow();
            FormAddCharacter formAddCharacter = new FormAddCharacter();
            formAddCharacter.item = current_selected;
            formAddCharacter.loadData();

            formAddCharacter.Show();
        }

        void getCurrentSelectedRow()
        {
            current_selected = dataGridViewCharacters.SelectedRows[0].Cells[0].Value.ToString();
            character = CharacterList.GetCharacter(current_selected);
        }

        bool checkWindowOpen()
        {
            IntPtr targetHWnd = IntPtr.Zero;

            string targetWindowName = character.ID;

            // Find define handle of project
            targetHWnd = AutoControl.FindWindowHandle(null, targetWindowName);

            if((targetHWnd != IntPtr.Zero))
            {
                return true;
            }

            return false;
        }

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        void openWindow()
        {
            if (character == null)
            {
                MessageBox.Show("Chưa chọn nhân vật, không thể mở ứng dụng");
                return;
            }

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
            getCurrentSelectedRow();
            openWindow();
        }

        private void buttonConfigAuto_Click(object sender, EventArgs e)
        {
            getCurrentSelectedRow();
            FormManageAuto formManageAuto = new FormManageAuto(textBoxStatus);

            formManageAuto.Text = "Config Auto " + character.ID;
            formManageAuto.character = character;

            formManageAuto.Show();
        }

        private void updateCharacter()
        {
            try
            {
                CharacterList.UpdateCharacterAllFields(character);
            }
            catch (Exception exp)
            {
                MessageBox.Show("Cập nhật character " + character.ID + " không thành công: " + exp.ToString());
            }
        }

        private void setStateFeatures()
        {
            checkRenewConfig();

            DateTime today = DateTime.Today;

            character.Date = today.ToString("dd/MM/yyyy");
            character.VipPromotion = setStateFeature(character.VipPromotion);
            character.DoiNangNo = setStateFeature(character.DoiNangNo);
            character.DoiNangNoNL4 = setStateFeature(character.DoiNangNoNL4);
            character.TrongNL = setStateFeature(character.TrongNL);
            character.TriAn = setStateFeature(character.TriAn);
            character.LatTheBai = setStateFeature(character.LatTheBai);
            character.RutBo = setStateFeature(character.RutBo);
            character.DoiKGDK = setStateFeature(character.DoiKGDK);
            character.TuHanh = setStateFeature(character.TuHanh);
            character.TruMa = setStateFeature(character.TruMa);
            character.AoMaThap = setStateFeature(character.AoMaThap);
            character.TrongCay = setStateFeature(character.TrongCay);
            character.CheMatBao = setStateFeature(character.CheMatBao);
            character.AutoPhuBan = setStateFeature(character.AutoPhuBan);
            character.UocNguyen = setStateFeature(character.UocNguyen);
            character.DauPet = setStateFeature(character.DauPet);
            character.NhanThuongHLVT = setStateFeature(character.NhanThuongHLVT);
            character.BugOnline = setStateFeature(character.BugOnline);
            character.MeTran = setStateFeature(character.MeTran);
            character.HaiThuoc = setStateFeature(character.HaiThuoc);
            character.CauCa = setStateFeature(character.CauCa);
        }

        private int setStateFeature(int status)
        {
            if (renewConfig && status >= 1)
            {
                status = 1;
            }

            return status;
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

        private void buttonRunAuto_Click(object sender, EventArgs e)
        {
            getCurrentSelectedRow();

            setStateFeatures();

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
            getCurrentSelectedRow();

            character.Running = 0;
            updateCharacter();

            foreach (var thread in Helper.threadList)
            {
                if (thread.Name == (character.ID + "mainauto"))
                {
                    Helper.writeStatus(textBoxStatus, character.ID, "Đã ngừng auto");
                    thread.Abort();
                }
            }
        }
    }
}
