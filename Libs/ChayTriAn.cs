using AutoVPT.Objects;
using System;
using System.Threading;

namespace AutoVPT.Libs
{
    class ChayTriAn
    {
        public IntPtr mHWnd;
        public string mWindowName;
        public AutoFeatures mAuto;
        public bool completed = false;
        Character mCharacter;

        public ChayTriAn(IntPtr hWnd, string windowName, Character character, AutoFeatures auto)
        {
            mHWnd = hWnd;
            mWindowName = windowName;
            mAuto = auto;
            mCharacter = character;
        }

        public void chayQ()
        {
            mAuto.writeStatus("Đã nhận được nhiệm vụ, bắt đầu tìm quái đánh ...");

            // Xóa ghi chép chat
            mAuto.writeStatus("Xóa ghi chép chat ...");
            mAuto.clickImageByGroup("global", "chatclear", false, true);

            while (!checkDaDanhNhiemVu())
            {
                // Mở menu phải
                mAuto.moMenuPhai();

                // Nếu vip dưới 6 thì mới chạy cái này
                if (mCharacter.VipLevel < 6 && mCharacter.VipLevel > 0)
                {
                    // Bay
                    mAuto.bay();
                }

                // Tìm tới tọa độ và gọi quái
                while (!checkDaGoiQuai())
                {

                    // Mở túi nhiệm vụ
                    mAuto.closeAllDialog();
                    mAuto.writeStatus("Mở túi nhiệm vụ ...");
                    mAuto.clickToImage(Constant.ImagePathGlobalTui);
                    mAuto.clickImageByGroup("global", "tui_tab_nhiemvu", true, true);

                    // Nhấp đôi vào bản đồ nhiệm vụ
                    mAuto.writeStatus("Nhấp đôi vào bản đồ nhiệm vụ ...");
                    mAuto.clickToImage(Constant.ImagePathTriAnBanDoNhiemVu, 0, -20, 2);

                    // Chờ 2s để load map
                    mAuto.writeStatus("Chờ 2s để load map ...");
                    Thread.Sleep(2000);

                    mAuto.closeAllDialog();

                    // Nếu vip dưới 6 thì mới chạy cái này
                    if (mCharacter.VipLevel < 6 && mCharacter.VipLevel > 0)
                    {
                        do
                        {
                            // Nhấp vào tọa độ
                            mAuto.writeStatus("Nhấp vào tọa độ ...");
                            mAuto.clickToImage(Constant.ImagePathTriAnToaDo, 10, -25);
                            mAuto.clickToImage(Constant.ImagePathTriAnToaDo2, 10, -25);

                        } while (mAuto.isMoving());
                    }
                }

                mAuto.closeAllDialog();

                // Nếu vip dưới 6 thì mới chạy cái này
                if (mCharacter.VipLevel < 6 && mCharacter.VipLevel > 0)
                {
                    // Xuống
                    mAuto.bayXuong();
                }

                // Đóng menu phải
                mAuto.dongMenuPhai();

                // Click vào phản quân hoặc phi tặc
                mAuto.writeStatus("Click vào phản quân hoặc phi tặc ...");
                for (int i = 1; i <= 2; i++)
                {
                    mAuto.clickToImage(Constant.ImagePathTriAnPhiTac + i + ".png", 0, -20, 1, 100);
                    mAuto.clickToImage(Constant.ImagePathTriAnPhanQuan + i + ".png", 0, -20, 1, 100);
                }

                Thread.Sleep(1000);

                // Đánh
                mAuto.clickToImage(Constant.ImagePathDoiThoai + "trian" + ".png");

                for (int i = 1; i <= 2; i++)
                {
                    if (i == 1)
                    {
                        mAuto.clickToImage(Constant.ImagePathTenTriAnPhiTac + i + ".png", 0, -80, 1, 100);
                        mAuto.clickToImage(Constant.ImagePathTenTriAnPhanQuan + i + ".png", 40, -80, 1, 100);
                    }
                    else
                    {
                        mAuto.clickToImage(Constant.ImagePathTenTriAnPhiTac + i + ".png", -20, -80, 1, 100);
                        mAuto.clickToImage(Constant.ImagePathTenTriAnPhanQuan + i + ".png", -40, -80, 1, 100);
                    }

                    Thread.Sleep(1000);

                    // Đánh
                    mAuto.clickToImage(Constant.ImagePathDoiThoai + "trian" + ".png");
                }

                // Click vào 1 vị trí trên màn hình
                mAuto.writeStatus("Click vào 1 vị trí trên màn hình ...");
                //mAuto.clickToWindow(650, 450);
                mAuto.clickToImage(Constant.ImagePathCharNameFolder + mWindowName + ".png", mAuto.random.Next(-50, 50), mAuto.random.Next(-50, 50));

                // Nếu vip dưới 6 thì mới chạy cái này
                if (mCharacter.VipLevel < 6 && mCharacter.VipLevel > 0)
                {
                    // Nhấp vào tọa độ
                    mAuto.writeStatus("Nhấp vào tọa độ ...");
                    mAuto.clickToImage(Constant.ImagePathTriAnToaDo, 10, -25);
                    mAuto.clickToImage(Constant.ImagePathTriAnToaDo2, 10, -25);
                    Thread.Sleep(2000);
                }

                Thread.Sleep(2000);

                // Nghỉ 5s nếu nhân vật đang trong trận đấu
                while (mAuto.dangTrongTranDau())
                {
                    Thread.Sleep(5000);
                }
            }

            if (!checkHoanThanhNhiemVu()) {
                // Nhấn vào nhận Q Trị An
                mAuto.clickImageByGroup("tri_an", "nhiemvutrianchuanhan", false, true);
                mAuto.clickImageByGroup("tri_an", "nhiemvuphanquandaxong", false, true);
                mAuto.clickImageByGroup("tri_an", "nhiemvuphitacdaxong", false, true);
                // Trả nhiệm vụ
                mAuto.traNhiemVu();
            }
        }

        public void nhanQ()
        {
            if (checkDaNhanNhiemVu())
            {
                return;
            }

            mAuto.writeStatus("Chờ 2s sau khi đứng yên rồi kiểm tra xem có nchuyen với TCV DHT ko ?");
            // Chờ 2s sau khi đứng yên rồi kiểm tra xem có nchuyen với TCV DHT ko ?
            Thread.Sleep(2000);
            if (!mAuto.isTalkWithNPC("truongcanvedonghuyenthanh"))
            {
                mAuto.writeStatus("Nhân vật đang bị lag vị trí, bắt đầu fix lag vị trí ...");
                mAuto.talkToNPC("truongcanvedonghuyenthanh");
            }

            // Nhấn vào Q 
            mAuto.clickToImage(Constant.ImagePathTriAnNDPT);

            // Nhấn vào nhận Q Trị An
            mAuto.clickImageByGroup("tri_an", "nhiemvutrianchuanhan", false, true);
            mAuto.clickImageByGroup("tri_an", "nhiemvuphanquandaxong", false, true);
            mAuto.clickImageByGroup("tri_an", "nhiemvuphitacdaxong", false, true);
            // Trả nhiệm vụ
            mAuto.traNhiemVu();
        }

        public bool checkDaGoiQuai()
        {
            mAuto.writeStatus("Check đã gọi quái chưa ?");
            mAuto.closeAllDialog();
            // Mở túi nhiệm vụ
            mAuto.writeStatus("Mở túi nhiệm vụ ...");
            mAuto.clickToImage(Constant.ImagePathGlobalTui);
            mAuto.clickImageByGroup("global", "tui_tab_nhiemvu", true, true);
            if (!mAuto.findImageByGroup("tri_an", "bandonhiemvu"))
            {
                mAuto.writeStatus("Đã gọi quái");
                return true;
            }
            mAuto.writeStatus("Chưa gọi quái");
            return false;
        }

        public bool checkDaNhanNhiemVu()
        {
            mAuto.closeAllDialog();
            mAuto.writeStatus("Kiểm tra đã nhận nhiệm vụ chưa ?");
            mAuto.clickImageByGroup("global", "nhiemvu");
            mAuto.clickImageByGroup("global", "nhiemvuvong");
            if (mAuto.findImageByGroup("tri_an", "bangnhiemvutrianchuaxong", true, false)
                || mAuto.findImageByGroup("tri_an", "bangnhiemvutrianchuaxonggreen")
                || mAuto.findImageByGroup("tri_an", "bangnhiemvutriandaxong", true, false)
                || mAuto.findImageByGroup("tri_an", "bangnhiemvutriandaxonggreen"))
            {
                mAuto.writeStatus("Đã nhận nhiệm vụ rồi");
                return true;
            }
            mAuto.writeStatus("Chưa nhận nhiệm vụ hoặc đã hoàn thành nhiệm vụ");
            return checkHoanThanhNhiemVu();
        }

        public bool checkDaDanhNhiemVu()
        {
            mAuto.closeAllDialog();
            mAuto.writeStatus("Kiểm tra đã đánh nhiệm vụ chưa ?");
            mAuto.clickImageByGroup("global", "nhiemvu");
            mAuto.clickImageByGroup("global", "nhiemvuvong");
            if (mAuto.findImageByGroup("tri_an", "bangnhiemvutriandaxong", true, false) 
                || mAuto.findImageByGroup("tri_an", "bangnhiemvutriandaxonggreen"))
            {
                mAuto.writeStatus("Đã đánh nhiệm vụ rồi");
                return true;
            }
            mAuto.writeStatus("Chưa đánh quái");
            return false;
        }

        public bool checkHoanThanhNhiemVu()
        {
            mAuto.writeStatus("Kiểm tra đã hoàn thành nhiệm vụ chưa ?");

            mAuto.moMenuPhai();

            mAuto.moveToMap("donghuyenthanh");

            mAuto.closeAllDialog();
            mAuto.writeStatus("Tìm và nói chuyện với \"Trưởng cận vệ ĐHT\" ...");

            // Nếu vip dưới 6 thì mới chạy cái này
            if (mCharacter.VipLevel < 6 && mCharacter.VipLevel > 0)
            {
                // Xuong
                mAuto.bayXuong();
            }

            // Mở bảng "Cách chơi"
            mAuto.clickToImage(Constant.ImagePathGlobalCachChoi);

            // Mở bảng "Kiếm tiền"
            mAuto.clickImageByGroup("global", "cach_choi_kiem_tien", true, true);

            // Nhấn vào link "Trưởng cận vệ ĐHT"
            mAuto.clickImageByGroup("global", "truong_can_ve_dht", false, true);
            Thread.Sleep(2000);
            mAuto.clickImageByGroup("global", "truong_can_ve_dht", false, true);

            if (!mAuto.isMoving())
            {
                mAuto.writeStatus("Chờ 2s sau khi đứng yên rồi kiểm tra xem có nchuyen với TCV DHT ko ?");
                // Chờ 2s sau khi đứng yên rồi kiểm tra xem có nchuyen với TCV DHT ko ?
                Thread.Sleep(2000);
                if (!mAuto.isTalkWithNPC("truongcanvedonghuyenthanh"))
                {
                    mAuto.writeStatus("Nhân vật đang bị lag vị trí, bắt đầu fix lag vị trí ...");
                    mAuto.talkToNPC("truongcanvedonghuyenthanh");
                }

                // Nhấn vào Q 
                mAuto.clickToImage(Constant.ImagePathTriAnNDPT);

                if (!mAuto.findImageByGroup("tri_an", "nhiemvutrian", false, true)
                && !mAuto.findImageByGroup("tri_an", "nhiemvuphanquan", false, true)
                && !mAuto.findImageByGroup("tri_an", "nhiemvuphitac", false, true))
                {
                    mAuto.writeStatus("Đã hoàn thành nhiệm vụ trị an ...");
                    completed = true;
                    return true;
                }
            }

            mAuto.writeStatus("Chưa hoàn thành nhiệm vụ trị an ...");
            return false;
        }
    }
}
