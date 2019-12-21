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
        public string current_selected;

        public MainForm()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Ansi)]
        public static extern bool SetWindowText(IntPtr hWnd, String strNewWindowName);

        private void stopAuto()
        {
            //MessageBox.Show("Đã ngừng auto");
        }

        // Utilities Functions
        void populate()
        {
            IList list = CharacterList.GetCharacterList();
            this.dataGridViewCharacters.DataSource = list;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            labelAuthorVersion.Text = "AutoVPT Version 1.0 - Tử La Lan - https://www.facebook.com/Tu.La.Lan.NT";
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
            getCurrentSelectedRow();
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

        private bool checkAutoOpen()
        {

            IntPtr targetHWnd = IntPtr.Zero;

            string targetWindowName = "Auto " + character.ID;

            // Find define handle of project
            targetHWnd = AutoControl.FindWindowHandle(null, targetWindowName);

            if ((targetHWnd != IntPtr.Zero))
            {
                return true;
            }

            return false;
        }

        private void preStartManageAutoForm()
        {
            if (checkAutoOpen())
            {
                return;
            }

            Helper.threadList.Add(new Thread(startManageAutoForm));
            int index = Helper.threadList.Count() - 1;
            Helper.threadList[index].Name = character.ID + "formauto";
            Helper.threadList[index].Start();
        }

        private void startManageAutoForm()
        {
            FormManageAuto formManageAuto = new FormManageAuto();

            formManageAuto.Text = "Auto " + character.ID;
            formManageAuto.character = character;

            formManageAuto.ShowDialog();
        }

        private void buttonOpenAuto_Click(object sender, EventArgs e)
        {
            openWindow();
            preStartManageAutoForm();
        }

        private void buttonOpenGame_Click(object sender, EventArgs e)
        {
            openWindow();
        }
    }
}
