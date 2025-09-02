using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoVPT.Libs
{
    class DanhSTMT
    {
        public IntPtr mHWnd;
        public string mWindowName;
        public AutoFeatures mAuto;
        public string[] mStmt = new string[16];

        public DanhSTMT(IntPtr hWnd, string windowName, AutoFeatures auto)
        {
            mHWnd = hWnd;
            mWindowName = windowName;
            mAuto = auto;
        }

        public bool danhSTMT()
        {
            mAuto.writeStatus("danhSTMT");
            mAuto.closeAllDialog();
            int mtIndex = 0;
            while (mtIndex < 16)
            {
                while (!mAuto.findImageByGroup("stmt", "stmt_check"))
                {
                    while (mAuto.findImageByGroup("global", "quickFeatureListUpArrow") && !mAuto.findImageByGroup("stmt", "stmt"))
                    {
                        mAuto.writeStatus("Kéo lên đầu quick feature list");
                        mAuto.clickImageByGroup("global", "quickFeatureListUpArrow");
                        Thread.Sleep(Constant.TimeShort);
                    }

                    while (!mAuto.findImageByGroup("stmt", "stmt") && mAuto.findImageByGroup("global", "quickFeatureListDownArrow"))
                    {
                        mAuto.writeStatus("Không tìm thấy STMT, di chuyển sang trang tiếp");
                        mAuto.clickImageByGroup("global", "quickFeatureListDownArrow");
                        Thread.Sleep(Constant.TimeShort);
                    }

                    mAuto.clickImageByGroup("stmt", "stmt");
                    Thread.Sleep(Constant.TimeMedium);
                }

                int i = 0;

                // Tìm MT trong danh sách
                while (!mAuto.findImageByGroup("stmt", mStmt[mtIndex]) && i <= Constant.MaxLoopQ)
                {
                    mAuto.clickImageByGroup("stmt", "nextpage");
                    Thread.Sleep(2000);

                    i++;
                }

                if (i >= Constant.MaxLoopQ)
                {
                    mAuto.writeStatus("Không tìm thấy ma thú " + mStmt[mtIndex]);
                    return false;
                }
                mAuto.writeStatus("Tìm thấy ma thú " + mStmt[mtIndex]);
                if (mStmt[mtIndex].Contains("air"))
                {
                    mAuto.writeStatus("Đánh ma thú bay, bay lên");
                    mAuto.bay();
                }
                else
                {
                    mAuto.writeStatus("Đánh ma thú đất, bay xuống");
                    mAuto.bayXuong();
                }

                // Đánh MT
                mAuto.clickImageByGroup("stmt", mStmt[mtIndex], false, false, 1, -5, 0);
                Thread.Sleep(Constant.TimeShort);
                mAuto.clickImageByGroup("stmt", "co");
                Thread.Sleep(Constant.TimeMedium);

                bool inBattle = false;
                while (mAuto.dangTrongTranDau())
                {
                    inBattle = true;
                    mAuto.clickImageByGroup("global", "inbattleauto");
                    Thread.Sleep(Constant.TimeLong);
                }

                if (inBattle)
                {
                    Thread.Sleep(Constant.TimeMedium);
                }

                mtIndex++;
            }

            return true;
        }

        public void setSTMT(string[] stmt)
        {
            mAuto.writeStatus("setSTMT");
            int i = 0;
            foreach (string mt in stmt)
            {
                if (i >= 16) break;
                if (mt.Contains("air"))
                {
                    for (int j = 0; j < 4; j++)
                    {
                        mStmt[i] = mt;
                        i++;
                    }
                }
                else
                {
                    for (int j = 0; j < 2; j++)
                    {
                        mStmt[i] = mt;
                        i++;
                    }
                }
            }
            mAuto.writeStatus("Đánh STMT với danh sách sau: " + string.Join(",", mStmt));
        }
    }
}
