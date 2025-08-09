using System;
using System.Threading;

namespace AutoVPT.Libs
{
    class AutoXuQue
    {
        public IntPtr mHWnd;
        public string mWindowName;
        public AutoFeatures mAuto;

        public AutoXuQue(IntPtr hWnd, string windowName, AutoFeatures auto)
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
            mAuto.writeStatus("Bắt đầu \"Xủ Quẻ\" ...");
            mAuto.closeAllDialog();

            // Mở xủ quẻ
            while (!mAuto.findImageByGroup("global", "xuQueTui"))
            {
                // Kéo quick feature list lên trên cùng
                while (mAuto.findImageByGroup("global", "quickFeatureListUpArrow") && !mAuto.findImageByGroup("global", "quickFeatureXuQue"))
                {
                    mAuto.writeStatus("Kéo lên đầu quick feature list");
                    mAuto.clickImageByGroup("global", "quickFeatureListUpArrow");
                    Thread.Sleep(Constant.TimeShort);
                }

                // Kéo quick feature list xuống để tìm xủ quẻ
                while (!mAuto.findImageByGroup("global", "quickFeatureXuQue") && mAuto.findImageByGroup("global", "quickFeatureListDownArrow"))
                {
                    mAuto.writeStatus("Không tìm thấy Xủ Quẻ, di chuyển sang trang tiếp");
                    mAuto.clickImageByGroup("global", "quickFeatureListDownArrow");
                    Thread.Sleep(Constant.TimeShort);
                }

                // Mở xủ quẻ
                mAuto.clickImageByGroup("global", "quickFeatureXuQue");
                Thread.Sleep(Constant.TimeMedium);
            }

            while(true)
            {
                // Phân giải nếu thấy
                while (mAuto.findImageByGroup("global", "xuQueGiai"))
                {
                    mAuto.clickImageByGroup("global", "xuQueGiai");
                    while (mAuto.findImageByGroup("global", "xuQueXacNhanPhanGiai"))
                    {
                        mAuto.clickImageByGroup("global", "xuQuePhanGiai");
                    }
                }

                // Xủ quẻ :D
                int i = 0;
                while (i < 3)
                {
                    i++;
                    mAuto.clickImageByGroup("global", "xuQue3Sao");
                    mAuto.clickImageByGroup("global", "xuQue4Sao");
                    mAuto.clickImageByGroup("global", "xuQue5Sao");

                    int j = 0;
                    while (j < 2)
                    {
                        j++;
                        mAuto.clickImageByGroup("global", "xuQue1Sao");
                        mAuto.clickImageByGroup("global", "xuQue2Sao");
                    }
                }
            }    


            return true;
        }
    }
}
