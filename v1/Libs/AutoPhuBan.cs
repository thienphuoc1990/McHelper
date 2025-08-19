using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Libs
{
    class AutoPhuBan
    {
        public IntPtr mHWnd;
        public string mWindowName;
        public AutoFeatures mAuto;
        public string[] mPhuBan = new string[8];

        public AutoPhuBan(IntPtr hWnd, string windowName, AutoFeatures auto)
        {
            mHWnd = hWnd;
            mWindowName = windowName;
            mAuto = auto;
        }

        /*
         * Function: auto
         * Description: Chạy auto
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-17 - Updated At: 2019-11-17
         */
        public bool auto()
        {
            nhanThuongPhuBan();

            // Chuyển trang 1
            mAuto.clickImageByGroup("phu_ban", "prevpage");

            int i = 0;

            while(mPhuBan[i] != null && i <= Constant.MaxLoopQ)
            {
                // Chuyển trang
                if(mPhuBan[i] == "thamhiem")
                {
                    mAuto.clickImageByGroup("phu_ban", "nextpage");
                }
                else
                {
                    mAuto.clickImageByGroup("phu_ban", "prevpage");
                }


                // Chọn độ khó
                if (mPhuBan[i] == "lietdiemthamuyen" 
                    || mPhuBan[i] == "trolailanghuyet"
                    || mPhuBan[i] == "quyhutmau")
                {
                    mAuto.clickImageByGroup("phu_ban", "showchondokho" + mPhuBan[i], false, false, 1, 120);
                    Thread.Sleep(Constant.TimeShort);
                    mAuto.clickImageByGroup("phu_ban", "chonkho");
                }

                // Auto phụ bản
                mAuto.clickImageByGroup("phu_ban", "batdau" + mPhuBan[i], false, false, 1, 60, 40);
                mAuto.clickImageByGroup("global", "luachonco", false, true);
                i++;
            }

            if (i >= Constant.MaxLoopQ)
            {
                return false;
            }

            return true;
        }

        /*
         * Function: nhanThuongPhuBan
         * Description: Chạy nhận thưởng phụ bản
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2025-08-15 - Updated At: 2025-08-15
         */
        public bool nhanThuongPhuBan()
        {
            mAuto.closeAllDialog();

            // Mở auto phụ bản
            while (!mAuto.findImageByGroup("phu_ban", "hoanthanhphuban_check"))
            {
                // Kéo quick feature list lên trên cùng
                while (mAuto.findImageByGroup("global", "quickFeatureListUpArrow") && !mAuto.findImageByGroup("phu_ban", "hoanthanhphuban"))
                {
                    mAuto.writeStatus("Kéo lên đầu quick feature list");
                    mAuto.clickImageByGroup("global", "quickFeatureListUpArrow");
                    Thread.Sleep(Constant.TimeShort);
                }

                // Kéo quick feature list xuống để tìm Auto Phụ bản
                while (!mAuto.findImageByGroup("phu_ban", "hoanthanhphuban") && mAuto.findImageByGroup("global", "quickFeatureListDownArrow"))
                {
                    mAuto.writeStatus("Không tìm thấy Auto Phụ bản, di chuyển sang trang tiếp");
                    mAuto.clickImageByGroup("global", "quickFeatureListDownArrow");
                    Thread.Sleep(Constant.TimeShort);
                }

                // Mở auto phụ bản
                mAuto.clickImageByGroup("phu_ban", "hoanthanhphuban");
                Thread.Sleep(Constant.TimeMedium);
            }

            int i = 0;
            // Nhận hoàn thành phụ bản nếu có
            while (mAuto.findImageByGroup("phu_ban", "nhanthuong") && i <= Constant.MaxLoopQ)
            {
                mAuto.clickImageByGroup("phu_ban", "nhanthuong");
                Thread.Sleep(Constant.TimeMedium);
                i++;
            }

            // Chuyển trang 2 và nhận hoàn thành phụ bản nếu có
            mAuto.clickImageByGroup("phu_ban", "nextpage");
            while (mAuto.findImageByGroup("phu_ban", "nhanthuong") && i <= Constant.MaxLoopQ + 5)
            {
                mAuto.clickImageByGroup("phu_ban", "nhanthuong");
                Thread.Sleep(Constant.TimeMedium);
                i++;
            }

            return true;
        }

        /*
         * Function: nhanPhuBan
         * Description: Nhận Phụ Bản
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-17 - Updated At: 2019-11-17
         */
        public void nhanPhuBan(string m)
        {
            string npc = "sugiamophuban";
            if (m == "codao")
            {
                npc = "nguoikhaiquat";
            }
            int i = 0;
            while(mPhuBan[i] != null && i <= Constant.MaxLoopQ)
            {
                if(m == "codao" && mPhuBan[i] == "thamhiem" || m != "codao" && mPhuBan[i] != "thamhiem")
                {
                    if (mAuto.talkToNPC(npc, 0, 0, -40))
                    {
                        if (mPhuBan[i] == "thamhiem")
                        {
                            mAuto.clickImageByGroup("phu_ban", "nhanbando", false, true);
                        }

                        // Click vào nhiệm vụ được trả hoặc chưa nhận
                        mAuto.clickImageByGroup("phu_ban", mPhuBan[i], false, true);

                        // Click nhận nhiệm vụ và trả nhiệm vụ
                        mAuto.clickImageByGroup("global", "nhannhiemvu", false, true);
                        mAuto.clickImageByGroup("global", "xong", false, true);

                        mAuto.writeStatus("Nhận phụ bản thành công ...");
                        Thread.Sleep(2000);
                    }
                }
                i++;
            }
        }

        /*
         * Function: Di chuyển đến vị trí đổi năng nổ
         * Description: Chạy Đổi Năng Nổ
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-17 - Updated At: 2019-11-17
         */
        public bool diChuyenDenNhanPhuBan(string m = "tienlapthanh")
        {
            string npc = "sugiamophuban";
            string location = "nhanphubantlt";
            string map = "tienlapthanh";
            int x = 0;
            int y = -20;
            if (m == "codao")
            {
                npc = "nguoikhaiquat";
                location = "nhanphubancd";
                map = "codao";
                x = 38;
            }
            mAuto.writeStatus("Di chuyển đến vị trí nhận phụ bản ở " + m);

            mAuto.closeAllDialog();

            if (!mAuto.moveToMap(map, 1, x, y))
            {
                mAuto.writeStatus("Không thể di chuyển đến map nhận phụ bản ở " + m);
                return false;
            }

            // Bay lên
            mAuto.bay();

            if (!mAuto.moveToNPC(npc, location))
            {
                mAuto.writeStatus("Không thể di chuyển đến vị trí nhận phụ bản ở " + m);
                return false;
            }
            mAuto.bayXuong();

            return true;
        }

        /*
         * Function: setPhuBan
         * Description: Set Phụ Bản sẽ Auto
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-17 - Updated At: 2019-11-17
         */
        public void setPhuBan(string[] phuBan)
        {
            mAuto.writeStatus("Lấy thông tin các phụ bản sẽ auto ...");
            int i = 0;
            foreach (string phuban in phuBan)
            {
                switch (phuban)
                {
                    case "Mê Huyễn Động":
                        mPhuBan[i] = "mehuyendong";
                        break;
                    case "Kho Báu Đại Mạc":
                        mPhuBan[i] = "khobaudaimac";
                        break;
                    case "Lục Tiên Cảnh":
                        mPhuBan[i] = "luctiencanh";
                        break;
                    case "Liệt Diễm Thâm Uyên":
                        mPhuBan[i] = "lietdiemthamuyen";
                        break;
                    case "Trở Lại Lang Huyệt":
                        mPhuBan[i] = "trolailanghuyet";
                        break;
                    case "Quỷ Hút Máu":
                        mPhuBan[i] = "quyhutmau";
                        break;
                    case "Thế Giới Số":
                        mPhuBan[i] = "thegioiso";
                        break;
                    case "Thám Hiểm":
                        mPhuBan[i] = "thamhiem";
                        break;
                    default:
                        break;
                }
                i++;
            }
        }
    }
}
