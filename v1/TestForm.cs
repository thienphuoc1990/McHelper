using AutoVPT.Libs;
using KAutoHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoVPT
{
    public partial class TestForm : Form
    {
        public TestForm()
        {
            InitializeComponent();
        }

        private void buttonTestCapturePosition_Click(object sender, EventArgs e)
        {
            IntPtr hWnd = IntPtr.Zero;
            // Find define handle of project
            hWnd = AutoControl.FindWindowHandle(null, textBoxTestID.Text);

            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
            }

            var imagePath = textBoxTestPath.Text;

            // Chụp màn hình
            var full_screen = CaptureHelper.CaptureWindow(hWnd);

            Bitmap iBtn = ImageScanOpenCV.GetImage(imagePath);
            var pBtn = ImageScanOpenCV.FindOutPoint((Bitmap)full_screen, iBtn);
            List<Bitmap> pos = new List<Bitmap>();

            //if (pBtn != null)
            //{
            //    pos.Add(CaptureHelper.CropImage((Bitmap)full_screen, new Rectangle(pBtn.Value.X + (-15), pBtn.Value.Y + (-30), 30, 30)));
            //    pos.Add(CaptureHelper.CropImage((Bitmap)full_screen, new Rectangle(pBtn.Value.X + (11), pBtn.Value.Y + (0), 30, 30)));
            //    pos.Add(CaptureHelper.CropImage((Bitmap)full_screen, new Rectangle(pBtn.Value.X + (-15), pBtn.Value.Y + (20), 30, 30)));
            //    pos.Add(CaptureHelper.CropImage((Bitmap)full_screen, new Rectangle(pBtn.Value.X + (-32), pBtn.Value.Y + (0), 30, 30)));
            //}
            //pictureBoxTest1.Image = pos[0];
            //pictureBoxTest2.Image = pos[1];
            //pictureBoxTest3.Image = pos[2];
            //pictureBoxTest4.Image = pos[3];

            //MessageBox.Show(pos.Count.ToString());

            // top: x -15, y -30
            // left: x -30, y 0
            // down: x -15, y 20
            // right: x 11, y 0
            // Click: x 15, y -15



            if (pBtn != null)
            {
                int x_start_point = pBtn.Value.X + 0;
                int y_start_point = pBtn.Value.Y + 60;
                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        pos.Add(CaptureHelper.CropImage((Bitmap)full_screen, new Rectangle(
                            x_start_point + (x * 100),
                            y_start_point + (y * 70),
                            100, 70)));
                    }
                }
            }

            MessageBox.Show(pos.Count.ToString());

            // screen: h 60, w 100

            // h: 280, w: 400

            // h: 40 * 7, w: 80 * 5
            for (int z = 0; z < pos.Count; z++)
            {
                var picture = new PictureBox
                {
                    Name = "pictureBox",
                    Size = new Size(100, 70),
                    Location = new Point(300 + z * 100, 100 + z * 70),
                    Image = pos[z],

                };
                this.Controls.Add(picture);
            }
        }

        private void TestForm_Load(object sender, EventArgs e)
        {
            textBoxTestPath.Text = Constant.ImagePathGlobalMiniMap;
        }

        private void buttonClickRightOnImage_Click(object sender, EventArgs e)
        {
            AutoIT au3 = new AutoIT();
            IntPtr hWnd = IntPtr.Zero;
            // Find define handle of project
            hWnd = AutoControl.FindWindowHandle(null, textBoxTestID.Text);

            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            var imagePath = textBoxTestPath.Text;

            var screen = CaptureHelper.CaptureWindow(hWnd);
            screen.Save("test.png");
            Bitmap iBtn = ImageScanOpenCV.GetImage(imagePath);
            var pBtn = ImageScanOpenCV.FindOutPoint((Bitmap)screen, iBtn);
            if (pBtn != null)
            {
                au3.clickRight(textBoxTestID.Text, 1, pBtn.Value.X + int.Parse(numericUpDownTestX.Value.ToString()), pBtn.Value.Y + int.Parse(numericUpDownTestY.Value.ToString()));
                Thread.Sleep(Constant.TimeShort);
            }
        }

        private void buttonClickRightOnPoint_Click(object sender, EventArgs e)
        {
            AutoIT au3 = new AutoIT();
            IntPtr hWnd = IntPtr.Zero;
            // Find define handle of project
            hWnd = AutoControl.FindWindowHandle(null, textBoxTestID.Text);

            if (hWnd == IntPtr.Zero)
            {
                MessageBox.Show("Không tìm thấy nhân vật này đang được chạy.");
                return;
            }

            au3.clickRight(textBoxTestID.Text, 1, int.Parse(numericUpDownTestX.Value.ToString()), int.Parse(numericUpDownTestY.Value.ToString()));
            Thread.Sleep(Constant.TimeShort);
        }
    }
}
