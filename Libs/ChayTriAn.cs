using AutoVPT.Objects;
using KAutoHelper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
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
        public List<Monster> lPTPQ = new List<Monster>();

        public ChayTriAn(IntPtr hWnd, string windowName, Character character, AutoFeatures auto)
        {
            mHWnd = hWnd;
            mWindowName = windowName;
            mAuto = auto;
            mCharacter = character;
            initListPTPQ();
        }

        public void initListPTPQ()
        {
            lPTPQ.Add(new Monster(Constant.ImagePathTriAnPhiTac + "1" + ".png", 0, -20));
            lPTPQ.Add(new Monster(Constant.ImagePathTriAnPhiTac + "2" + ".png", 0, -20));
            lPTPQ.Add(new Monster(Constant.ImagePathTriAnPhanQuan + "1" + ".png", 0, -20));
            lPTPQ.Add(new Monster(Constant.ImagePathTriAnPhanQuan + "2" + ".png", 0, -20));
            lPTPQ.Add(new Monster(Constant.ImagePathTenTriAnPhiTac + "1" + ".png", 0, -80));
            lPTPQ.Add(new Monster(Constant.ImagePathTenTriAnPhiTac + "2" + ".png", 0, -80));
            lPTPQ.Add(new Monster(Constant.ImagePathTenTriAnPhanQuan + "1" + ".png", 40, -80));
            lPTPQ.Add(new Monster(Constant.ImagePathTenTriAnPhanQuan + "2" + ".png", -40, -80));
        }

        public void chayQ()
        {
            mAuto.writeStatus("Đã nhận được nhiệm vụ, bắt đầu tìm quái đánh ...");

            // Xóa ghi chép chat
            mAuto.writeStatus("Xóa ghi chép chat ...");
            mAuto.clickImageByGroup("global", "chatclear", false, true);

            List<Bitmap> pos = new List<Bitmap>();

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

                // Tìm phản quân hoặc phi tặc
                mAuto.writeStatus("Tìm phản quân hoặc phi tặc ...");
                int x = 0;
                while (!mAuto.findImage(Constant.ImagePathDoiThoai + "trian" + ".png") && x < lPTPQ.Count)
                {
                    if (mAuto.findImage(lPTPQ[x].imagePath))
                    {
                        mAuto.clickToImage(lPTPQ[x].imagePath, lPTPQ[x].x, lPTPQ[x].y);
                    }

                    x++;
                }

                // Đánh
                mAuto.clickToImage(Constant.ImagePathDoiThoai + "trian" + ".png");

                // Nghỉ 5s nếu nhân vật đang trong trận đấu
                while (mAuto.dangTrongTranDau())
                {
                    Thread.Sleep(5000);
                }

                if (checkDaDanhNhiemVu())
                {
                    break;
                }

                // Lưu 4 vị trí xung quanh vị trí hiện tại và tracking vị trí pqpt hiện tại
                mAuto.writeStatus("Lưu 4 vị trí xung quanh vị trí hiện tại và tracking vị trí pqpt hiện tại ...");
                if (pos.Count <= 0)
                {
                    // Mở bảng đồ mini
                    mAuto.clickToImage(Constant.ImagePathMiniMap);

                    Thread.Sleep(1000);

                    var full_screen = CaptureHelper.CaptureWindow(mHWnd);

                    // Tắt các bảng nổi
                    mAuto.closeAllDialog();

                    // Lưu tracking
                    mAuto.writeStatus("Lưu tracking ...");
                    Bitmap bChuyenKenh = ImageScanOpenCV.GetImage(Constant.ImagePathGlobalChuyenKenh);
                    var pBChuyenKenh = ImageScanOpenCV.FindOutPoint((Bitmap)full_screen, bChuyenKenh);

                    if (pBChuyenKenh != null)
                    {
                        Bitmap tracking = CaptureHelper.CropImage((Bitmap)full_screen, new Rectangle(pBChuyenKenh.Value.X, pBChuyenKenh.Value.Y, 180, 20));
                        tracking.Save("tracking/trian_" + mCharacter.ID + ".png", ImageFormat.Png);
                    }

                    // Lưu 4 vị trí
                    mAuto.writeStatus("Lưu 4 vị trí ...");
                    Bitmap iBtn = ImageScanOpenCV.GetImage(Constant.ImagePathInMapChar);
                    var pBtn = ImageScanOpenCV.FindOutPoint((Bitmap)full_screen, iBtn);

                    if (pBtn != null)
                    {
                        pos.Add(CaptureHelper.CropImage((Bitmap)full_screen, new Rectangle(pBtn.Value.X + (-15), pBtn.Value.Y + (-30), 30, 30)));
                        pos.Add(CaptureHelper.CropImage((Bitmap)full_screen, new Rectangle(pBtn.Value.X + (11), pBtn.Value.Y + (0), 30, 30)));
                        pos.Add(CaptureHelper.CropImage((Bitmap)full_screen, new Rectangle(pBtn.Value.X + (-15), pBtn.Value.Y + (20), 30, 30)));
                        pos.Add(CaptureHelper.CropImage((Bitmap)full_screen, new Rectangle(pBtn.Value.X + (-32), pBtn.Value.Y + (0), 30, 30)));
                    }
                }

                // Di chuyển đến vị trí quanh ptpq
                mAuto.writeStatus("Di chuyển đến vị trí quanh ptpq ...");

                int i = 0;
                while (!mAuto.findImage(Constant.ImagePathDoiThoai + "trian" + ".png") && i < pos.Count)
                {
                    // Mở menu phải
                    mAuto.moMenuPhai();

                    // Bay lên
                    mAuto.bay();

                    // Mở bảng đồ mini
                    mAuto.clickToImage(Constant.ImagePathMiniMap);

                    // Nhấp vào vị trí xung quanh ptpq
                    mAuto.clickImage(pos[i], 15, -15);

                    // Tắt các bảng nổi
                    mAuto.closeAllDialog();

                    // Đóng menu phải
                    mAuto.dongMenuPhai();

                    // Tìm phản quân hoặc phi tặc
                    mAuto.writeStatus("Tìm phản quân hoặc phi tặc ...");
                    int y = 0;
                    while (!mAuto.findImage(Constant.ImagePathDoiThoai + "trian" + ".png") && y < lPTPQ.Count)
                    {
                        if (mAuto.findImage(lPTPQ[y].imagePath))
                        {
                            // Mở menu phải
                            mAuto.moMenuPhai();

                            // Bay xuống
                            if (mAuto.findImage(Constant.ImagePathGlobalXuong))
                            {
                                mAuto.bayXuong();
                                Thread.Sleep(3000);
                            }

                            // Đóng menu phải
                            mAuto.dongMenuPhai();

                            mAuto.clickToImage(lPTPQ[y].imagePath, lPTPQ[y].x, lPTPQ[y].y);
                            Thread.Sleep(1000);
                        }

                        y++;
                    }

                    i++;
                }

                // Đánh
                mAuto.clickToImage(Constant.ImagePathDoiThoai + "trian" + ".png");

                // Nghỉ 5s nếu nhân vật đang trong trận đấu
                while (mAuto.dangTrongTranDau())
                {
                    Thread.Sleep(5000);
                }
            }

            if (!checkHoanThanhNhiemVu())
            {
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
