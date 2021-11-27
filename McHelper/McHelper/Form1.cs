using McHelper.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace McHelper
{
    public partial class Form1 : Form
    {
        SQLiteHelper sQLiteHelper = new SQLiteHelper(Constant.SQLITE_FILE_NAME);
        public Form1()
        {
            InitializeComponent();
            Initial();
        }

        private void ButtonOpenWindow_Click(object sender, EventArgs e)
        {

            //IntPtr defaultHWnd = IntPtr.Zero;

            //string defaultWindowName = "Adobe Flash Player 10";

            //Process.Start("flash.exe", "https://magicx.xcloudgame.com/s/s6/GameLoader.swf?user=2021112704225851451&pass=5cc66a6b4086693e45e24824f14d7e72&isExpand=true");
            MessageBox.Show("Updated");
        }

        private void ButtonReloadDatabase_Click(object sender, EventArgs e)
        {
            LoadDataToGridLink();
        }

        private void Initial()
        {
            sQLiteHelper.InitialDatabase();
            sQLiteHelper.InitialTable(Constant.SQLITE_TABLE_LINK_NAME, Constant.SQLITE_TABLE_LINK_CREATE_SQL);
            LoadDataToGridLink();
        }

        private void LoadDataToGridLink()
        {
            DataSet dataSet = sQLiteHelper.LoadData(Constant.SQLITE_TABLE_LINK_GET_ALL_SQL);
            Console.WriteLine(dataSet.Tables.Count);

            dataGridViewLinks.DataSource = dataSet.Tables[0];
        }
    }
}
