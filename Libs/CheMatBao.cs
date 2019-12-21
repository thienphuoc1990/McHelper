using System;
using System.Data;
using System.Threading;
using static System.Windows.Forms.CheckedListBox;

namespace AutoVPT.Libs
{
    class CheMatBao
    {
        public IntPtr mHWnd;
        public string mWindowName;
        public AutoFeatures mAuto;
        public string mLoaiMB;
        public int mCapMB;

        public CheMatBao(IntPtr hWnd, string windowName, AutoFeatures auto)
        {
            mHWnd = hWnd;
            mWindowName = windowName;
            mAuto = auto;
        }

        public bool che()
        {
            int loop = 1;

            // Mở bảng theo cấp mật bảo cần chế
            mAuto.clickImageByGroup("mat_bao", "tieudecapmatbao", false, false, 1, 20, -20 + (mCapMB * 25));

            // Click điểm an toàn
            mAuto.clickImageByGroup("mat_bao", "clickantoan");

            // Mở bảng theo loại mật bảo cần chế
            mAuto.clickImageByGroup("mat_bao", mLoaiMB);

            // Chế mật bảo
            //!mAuto.findImageByGroup("mat_bao", "hetluotche") &&
            while (loop <= Constant.MaxLoopQ)
            {
                // Click tự đặt nguyên liệu
                mAuto.clickImageByGroup("mat_bao", "tudongdatnguyenlieu");

                // Click chế tạo
                mAuto.clickImageByGroup("mat_bao", "chetaomatbao", false, false);

                // Click điểm an toàn
                mAuto.clickImageByGroup("mat_bao", "clickantoan");
                Thread.Sleep(2000);
                loop++;
            }

            return true;
        }

        public bool moBangCheMB()
        {
            mAuto.closeAllDialog();

            // Mở bảng nhân vật
            mAuto.clickImageByGroup("global", "nhanvat", false, false);

            // Mở bảng hồn khí
            mAuto.clickImageByGroup("mat_bao", "honkhi", false, false);

            // Chờ 5s
            Thread.Sleep(5000);

            // Mở bảng mật bảo
            mAuto.clickImageByGroup("mat_bao", "matbao", true, true);

            // Mở bảng chế tạo
            mAuto.clickImageByGroup("mat_bao", "chetao", true, true);

            // Kiểm tra đã mở dc bảng chế tao mật bảo chưa ?
            if(!mAuto.findImageByGroup("mat_bao", "chetaomatbao", false, true))
            {
                moBangCheMB();
            }

            return true;
        }

        public void setLoaiMB(string loaiMB)
        {
            switch (loaiMB)
            {
                case "Pháp Sức":
                    mLoaiMB = "phapsuc";
                    break;
                case "Vô Ưu":
                    mLoaiMB = "vouu";
                    break;
                case "Thánh Điện":
                    mLoaiMB = "thanhdien";
                    break;
                case "Hang Động":
                    mLoaiMB = "hangdong";
                    break;
                case "Đại Mạc":
                    mLoaiMB = "daimac";
                    break;
                case "Di Cảnh":
                    mLoaiMB = "dicanh";
                    break;
                case "Liệt Diễm":
                    mLoaiMB = "lietdiem";
                    break;
                case "Lang Huyệt":
                    mLoaiMB = "langhuyet";
                    break;
                case "Lạc Viên":
                    mLoaiMB = "lacvien";
                    break;
                case "Chiến Trang":
                    mLoaiMB = "chientrang";
                    break;
                case "Thần Binh":
                default:
                    mLoaiMB = "thanbinh";
                    break;
            }
        }

        public void setCapMB(int capMB)
        {
            mCapMB = capMB;
        }
    }
}
