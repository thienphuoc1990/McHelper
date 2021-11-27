using System;

namespace AutoVPT.Libs
{
    class DoiNangNo
    {
        public IntPtr mHWnd;
        public string mWindowName;
        public AutoFeatures mAuto;
        public string mLoaiNL;

        public DoiNangNo(IntPtr hWnd, string windowName, AutoFeatures auto)
        {
            mHWnd = hWnd;
            mWindowName = windowName;
            mAuto = auto;
        }

        /*
         * Function: doiNangNo
         * Description: Chạy Đổi Năng Nổ
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-16 - Updated At: 2019-11-16
         */
        public bool doiNangNo(int level)
        {
            if (mLoaiNL == null)
            {
                mAuto.writeStatus("Phải chọn loại nguyên liệu để đổi");
                return false;
            }

            string npc = "thuonghoitruongbbt";
            if (level == 4)
            {
                npc = "thuonghoitruongtlt";
            }

            if (mAuto.talkToNPC(npc))
            {
                // Click vào nhiệm vụ được trả hoặc chưa nhận
                mAuto.clickImageByGroup("global", "nvunangnochuanhan" + mLoaiNL);
                mAuto.clickImageByGroup("global", "nvunangnoduoctra" + mLoaiNL);

                // Click nhận nhiệm vụ và trả nhiệm vụ
                mAuto.clickImageByGroup("global", "nhannhiemvu", false, true);
                mAuto.clickImageByGroup("global", "xong", false, true);

                if (mAuto.findImageByGroup("global", "khongxong"))
                {
                    mAuto.writeStatus("Không đủ điều kiện để đổi năng nổ bằng nguyên liệu " + mLoaiNL);
                    return false;
                }

                mAuto.writeStatus("Đổi năng nổ bằng " + mLoaiNL + " thành công ...");
                return true;
            }

            return false;
        }

        /*
         * Function: Di chuyển đến vị trí đổi năng nổ
         * Description: Chạy Đổi Năng Nổ
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-16 - Updated At: 2019-11-16
         */
        public bool diChuyenDenDoiNN(int level = 5)
        {
            string npc = "thuonghoitruongbbt";
            string location = "doinangnobbt";
            string map = "bangboithanh";
            if (level == 4)
            {
                npc = "thuonghoitruongtlt";
                location = "doinangnotlt";
                map = "tienlapthanh";
            }
            mAuto.writeStatus("Di chuyển đến vị trí đổi năng nổ cấp " + level);

            // Tắt hết cửa sổ đang có trong game
            mAuto.closeAllDialog();

            // Di chuyển đến bản đồ đổi năng nổ
            if(!mAuto.moveToMap(map))
            {
                mAuto.writeStatus("Không thể di chuyển đến map đổi năng nổ cấp " + level);
                return false;
            }

            // Bay lên
            mAuto.bay();

            // Di chuyển đến vị trí đổi năng nổ
            if(!mAuto.moveToNPC(npc, location))
            {
                mAuto.writeStatus("Không thể di chuyển đến vị trí đổi năng nổ cấp " + level);
                return false;
            }

            return true;
        }

        public void setLoaiNL(string loaiNL)
        {
            switch (loaiNL)
            {
                case Constant.NameNguyenLieuDaThu:
                    mLoaiNL = "dathu";
                    break;
                case Constant.NameNguyenLieuPhaLe:
                    mLoaiNL = "phale";
                    break;
                case Constant.NameNguyenLieuKimLoaiHiem:
                    mLoaiNL = "kimloaihiem";
                    break;
                case Constant.NameNguyenLieuGoTot:
                    mLoaiNL = "gotot";
                    break;
                case Constant.NameNguyenLieuGamVoc:
                default:
                    mLoaiNL = "gamvoc";
                    break;
            }
        }

    }
}
