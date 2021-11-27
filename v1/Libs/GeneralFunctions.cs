using AutoVPT.Objects;
using KAutoHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoVPT.Libs
{
    class GeneralFunctions
    {
        private IntPtr mHWnd;
        private string mWindowName;
        public AutoFeatures mAuto;
        private Character mCharacter;
        public TrongNL mTrongNL;
        public ChayTriAn mChayTriAn;
        public DoiNangNo mDoiNangNo;
        public CheMatBao mCheMatBao;
        public AutoPhuBan mAutoPhuBan;

        public GeneralFunctions(IntPtr hWnd, Character character, TextBox textBoxStatus)
        {
            mHWnd = hWnd;
            mCharacter = character;
            mWindowName = mCharacter.ID;
            mAuto = new AutoFeatures(hWnd, mWindowName, textBoxStatus, mCharacter);
            mTrongNL = new TrongNL(mHWnd, mWindowName, mAuto);
            mCheMatBao = new CheMatBao(mHWnd, mWindowName, mAuto);
            mAutoPhuBan = new AutoPhuBan(mHWnd, mWindowName, mAuto);
            mDoiNangNo = new DoiNangNo(mHWnd, mWindowName, mAuto);
            mChayTriAn = new ChayTriAn(mHWnd, mWindowName, mCharacter, mAuto);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowText", CharSet = CharSet.Ansi)]
        public static extern bool SetWindowText(IntPtr hWnd, String strNewWindowName);

        public bool isInGame()
        {
            if (mAuto.findImageByGroup("global", "khongtrongtrandau", false, true))
            {
                return true;
            }
            return false;
        }

        public void login()
        {
            // Chưa load vào bảng login
            while (!mAuto.findImageByGroup("global", "loginbatbuoc", false, false))
            {
                Thread.Sleep(5000);
            }

            // Chưa load bảng chọn kênh
            while (!mAuto.findImageByGroup("global", "bangchonkenh", false, false))
            {
                // Click bắt buộc
                mAuto.writeStatus("Click bắt buộc");
                mAuto.clickImageByGroup("global", "loginbatbuoc", false, false);

                Thread.Sleep(5000);
            }

            // Chưa load bảng chọn nhân vật
            while (!mAuto.findImageByGroup("global", "bangchonnhanvat", false, false))
            {
                mAuto.writeStatus("Chọn kênh");
                this.chonKenh(5);

                Thread.Sleep(5000);
            }

            // Chưa load vào game
            while (!mAuto.findImageByGroup("global", "khongtrongtrandau", false, false))
            {
                mAuto.writeStatus("Chọn nhân vật");
                this.chonNhanVat();

                Thread.Sleep(5000);
            }
        }

        public void chonNhanVat()
        {
            // kênh 1 x = -100; y = 20, lên 1 kênh tăng thêm 34
            mAuto.clickImageByGroup("global", "bangchonnhanvat", false, false, 2, -80 + ((mCharacter.ViTriNhanVat - 1) * 140), 30);
        }

        public void chonKenh(int kenh)
        {
            // kênh 1 x = -100; y = 20, lên 1 kênh tăng thêm 34
            mAuto.clickImageByGroup("global", "bangchonkenh", false, false, 1, -100, 20 + ((kenh - 1) * 34));
        }

        public bool checkWindowOpen()
        {
            IntPtr targetHWnd = IntPtr.Zero;

            string targetWindowName = mCharacter.ID;

            // Find define handle of project
            targetHWnd = AutoControl.FindWindowHandle(null, targetWindowName);

            if ((targetHWnd != IntPtr.Zero))
            {
                return true;
            }

            return false;
        }

        public void openWindow()
        {
            IntPtr defaultHWnd = IntPtr.Zero;

            string defaultWindowName = "Adobe Flash Player 10";

            Process.Start("flash.exe", mCharacter.Link);

            do
            {
                // Find define handle of project
                defaultHWnd = AutoControl.FindWindowHandle(null, defaultWindowName);

                SetWindowText(defaultHWnd, mCharacter.ID);
            } while (defaultHWnd == IntPtr.Zero);
        }

        public void prepareScreen()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            // Thu nhỏ khung chat
            mAuto.writeStatus("Thu nhỏ khung chat ...");
            mAuto.clickImageByGroup("global", "thunhokhungchat", false, true, 2);

            // Tắt bảng nhiệm vụ nổi
            mAuto.writeStatus("Tắt bảng nhiệm vụ nổi ...");
            mAuto.clickImageByGroup("global", "tatbangnhiemvunoi");

            // Ẩn thanh kỹ năng
            mAuto.writeStatus("Ẩn thanh kỹ năng ...");
            mAuto.clickImageByGroup("global", "anthanhkynang");

            //// Ẩn tên và nhân vật, mở mẫu
            //// Click vào cài đặt
            //mAuto.clickImageByGroup("global", "thietlap");
            //// click ẩn tên
            //mAuto.clickImageByGroup("global", "anten");
            //// click ẩn nhân vật
            //mAuto.clickImageByGroup("global", "annhanvat");
            //// click bỏ đóng mẫu
            //mAuto.clickImageByGroup("global", "dongmauchecked");

        }

        /*
         * Function: runBugOnline
         * Description: Tự động bug online
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-18 - Updated At: 2019-11-18
         */
        public void runBugOnline()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            mAuto.writeStatus("Bắt đầu \"Tự động Bug Online\"");
            mAuto.closeAllDialog();

            // Click vào cài đặt
            mAuto.clickImageByGroup("global", "thietlap", false, false);

            // Click vào ra screen chọn nhân vật
            mAuto.clickImageByGroup("global", "thietlapnhanvat", false, false);

            // Click vào nhân vật
            this.chonNhanVat();

            // Tắt Flash window
            mAuto.closeFlash();
        }

        /*
         * Function: runAutoThanTu
         * Description: Tự động auto thần tu
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-12-29 - Updated At: 2019-12-29
         */
        public void runAutoThanTu()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            string npc = "thanhchuquyenco";
            string location = "autothantu";
            string map = "quyencothanh";
            mAuto.writeStatus("Bắt đầu \"Auto Thần tu\"");
            mAuto.closeAllDialog();

            // Chạy đến map
            if (!mAuto.moveToMap(map, 1, 7, -18))
            {
                mAuto.writeStatus("Không thể di chuyển đến " + map + " , thử lại ...");
                runAutoThanTu();
            }

            // Bay lên
            mAuto.bay();

            // Chạy đến NPC
            if (!mAuto.moveToNPC(npc, location))
            {
                mAuto.writeStatus("Không thể di chuyển đến vị trí "  + location);
                runAutoThanTu();
            }

            // Bay xuống
            mAuto.bayXuong();

            // Nói chuyện với NPC
            if (mAuto.talkToNPC(npc))
            {
                // Chọn Auto Tu Hành
                mAuto.clickImageByGroup("global", "autothantu", false, true);

                // Bấm bắt đầu
                mAuto.clickImageByGroup("global", "batdauautotuhanh", false, false);

                // Bấm có
                mAuto.clickImageByGroup("global", "luachonco", false, true);
            }
        }

        /*
         * Function: runAutoTuHanh
         * Description: Tự động auto tu hành
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-18 - Updated At: 2019-11-18
         */
        public void runAutoTuHanh()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            string npc = "truonglaovouutoc";
            string location = "autotuhanh";
            string map = "thientinhdia";
            mAuto.writeStatus("Bắt đầu \"Auto Tu Hành\"");
            mAuto.closeAllDialog();

            // Chạy đến Thiên Tĩnh Địa
            if (!mAuto.moveToMap(map, 1, 7, -18))
            {
                mAuto.writeStatus("Không thể di chuyển đến Thiên Tĩnh Địa, thử lại ...");
                runAutoTuHanh();
            }

            // Bay lên
            mAuto.bay();

            // Chạy đến NPC
            if (!mAuto.moveToNPC(npc, location))
            {
                mAuto.writeStatus("Không thể di chuyển đến vị trí auto tu hành");
                runAutoTuHanh();
            }

            // Bay xuống
            mAuto.bayXuong();

            // Nói chuyện với NPC
            if (mAuto.talkToNPC(npc))
            {
                // Chọn Auto Tu Hành
                mAuto.clickImageByGroup("global", "autotuhanh", false, true);

                // Bấm bắt đầu
                mAuto.clickImageByGroup("global", "batdauautotuhanh", false, false);

                // Bấm có
                mAuto.clickImageByGroup("global", "luachonco", false, true);
            }
        }

        /*
         * Function: dauPet
         * Description: Đấu Pet
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-18 - Updated At: 2019-11-18
         */
        public void dauPet()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            mAuto.writeStatus("Bắt đầu \"Đấu Pet\"");
            mAuto.closeAllDialog();

            // Mở bảng đấu pet
            mAuto.writeStatus("Mở bảng đấu pet");
            mAuto.clickImageByGroup("global", "daupet", false, false);
            Thread.Sleep(2000);

            // Bấm khiêu chiến
            mAuto.writeStatus("Bấm khiêu chiến");
            mAuto.clickImageByGroup("global", "daupetkhieuchien", false, true);
            Thread.Sleep(1000);
            mAuto.writeStatus("Bấm khiêu chiến lần 2");
            mAuto.clickImageByGroup("global", "daupetkhieuchien", false, true);
            mAuto.closeAllDialog();
        }

        /*
         * Function: nhanThuongHanhLang
         * Description: Nhận thưởng hành lang
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-18 - Updated At: 2019-11-18
         */
        public void nhanThuongHanhLang()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            string npc = "conghanhlang";
            string location = "nhanquahanhlang";
            mAuto.writeStatus("Bắt đầu \"Nhận thưởng hành lang\"");
            mAuto.closeAllDialog();

            // Di chuyển đến Quyến Cố Thành
            if (!mAuto.moveToMap("quyencothanh", 1, 5))
            {
                mAuto.writeStatus("Không thể di chuyển đến Quyến Cố Thành, thử lại ...");
                nhanThuongHanhLang();
            }

            // Bay lên
            mAuto.bay();

            // Di chuyển đến vị trí nhận thưởng hàng ngày
            if (!mAuto.moveToNPC(npc, location))
            {
                mAuto.writeStatus("Không thể di chuyển đến vị trí nhận quà hành lang");
                nhanThuongHanhLang();
            }

            // Bay xuống
            mAuto.bayXuong();

            // Nói chuyện với NPC
            if (mAuto.talkToNPC(npc, 0, 0, -40))
            {
                // Kéo xuống dưới
                mAuto.clickImageByGroup("global", "keoxuong", false, true, 3);

                // Bấm nhận thưởng hành lang
                mAuto.clickImageByGroup("global", "nhanthuonghanhlang", false, true);
            }
        }

        /*
         * Function: rungCay
         * Description: Rung cây
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-18 - Updated At: 2019-11-18
         */
        public void rungCay()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            int i = 0;
            mAuto.writeStatus("Bắt đầu \"Rung cây\"");
            mAuto.closeAllDialog();

            // Di chuyển đến Anh Vũ Cảnh
            if (!mAuto.moveToMap("anhvucanh", 1, 60))
            {
                mAuto.writeStatus("Không thể di chuyển đến Anh Vũ Cảnh, thử lại ...");
                rungCay();
            }

            // Bay lên
            mAuto.bay();

            // Nhấp vào cây ước nguyện
            mAuto.clickImageByGroup("global", "cayuocnguyen", false, false);

            Thread.Sleep(3000);

            // Nhấp vào cây ước nguyện lần 2
            mAuto.clickImageByGroup("global", "cayuocnguyen2", false, false);

            // Nhấp vào ước nguyện thành tâm
            mAuto.clickImageByGroup("global", "uocnguyenthanhtam", true, true);

            while (i < 6 && mCharacter.Running == 1)
            {
                // Nhấp vào ước 1 lần miễn phí
                mAuto.clickImageByGroup("global", "uocnguyenmienphi", false, true);

                Thread.Sleep(1000);

                i++;
            }
        }

        /*
         * Function: khongGianDieuKhac
         * Description: Đổi thưởng không gian diêu khắc
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-18 - Updated At: 2019-11-18
         */
        public void khongGianDieuKhac()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            mAuto.writeStatus("Bắt đầu \"Đổi không gian điêu khắc\"");
            mAuto.closeAllDialog();

            // Mở bảng KGDK
            mAuto.clickImageByGroup("global", "khonggiandieukhac", false, false);

            Thread.Sleep(2000);

            // Chọn đổi
            mAuto.clickImageByGroup("global", "khonggiandieukhacdoi", false, false);

            // Chọn có
            mAuto.clickImageByGroup("global", "luachonco", false, true);
        }

        /*
         * Function: rutBo
         * Description: Rút bộ đồ
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-18 - Updated At: 2019-11-18
         */
        public void rutBo()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            mAuto.writeStatus("Bắt đầu \"Rút bộ\"");
            mAuto.closeAllDialog();

            // Mở bảng nhân vật
            mAuto.clickImageByGroup("global", "nhanvat", false, false);

            // Mở tủ đồ
            mAuto.clickImageByGroup("global", "tudo", false, true);

            Thread.Sleep(2000);

            // Mở bảng rút bộ
            mAuto.clickImageByGroup("global", "rutbo", true, true);

            // Bấm rút thưởng
            mAuto.clickImageByGroup("global", "rutthuongbo", false, true);

            // Bấm xác nhận
            mAuto.clickImageByGroup("global", "rutboxacnhan", false, true);
        }

        /*
         * Function: nhanVIP
         * Description: Nhận VIP
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-17 - Updated At: 2019-11-17
         */
        public void nhanVIP()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            mAuto.writeStatus("Bắt đầu \"Nhận VIP\"");
            mAuto.closeAllDialog();

            // Bật bảng VIP
            mAuto.clickImageByGroup("global", "vip");

            // Nhận phúc lợi VIP
            mAuto.clickImageByGroup("global", "nhanvip");
            mAuto.clickImageByGroup("global", "nhanvip");
            mAuto.clickImageByGroup("global", "nhanvip");
            mAuto.clickImageByGroup("global", "nhanvip");

            // Bấm xuống 15 lần để nhận thêm
            mAuto.clickImageByGroup("global", "xuongvip", false, false, 15);

            // Nhận phúc lợi VIP
            mAuto.clickImageByGroup("global", "nhanvip");
            mAuto.clickImageByGroup("global", "nhanvip");
        }

        /*
         * Function: runNhanAutoPB
         * Description: Nhận và Auto Phụ Bản
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-17 - Updated At: 2019-11-17
         */
        public void runNhanAutoPB(string[] phuBan)
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            mAuto.writeStatus("Bắt đầu \"Nhận và Auto Phụ Bản\"");
            // Set phụ bản sẽ nhận và auto
            mAutoPhuBan.setPhuBan(phuBan);

            // Nhận phụ bản ở Lạp Tuyết Địa
            if (mAutoPhuBan.diChuyenDenNhanPhuBan("tienlapthanh"))
            {
                mAutoPhuBan.nhanPhuBan("tienlapthanh");
            }

            // Nhận phụ bản ở Cổ đạo
            if (mAutoPhuBan.diChuyenDenNhanPhuBan("codao"))
            {
                mAutoPhuBan.nhanPhuBan("codao");
            }

            // Auto phụ bản
            if (!mAutoPhuBan.auto())
            {
                mAutoPhuBan.auto();
            }
        }

        /*
         * Function: Chế mật bảo
         * Description: Chế mật bảo
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-16 - Updated At: 2019-11-16
         */
        public void runCheMatBao(string loaiMB, int capMB)
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            mAuto.writeStatus("Bắt đầu \"Chế mật bảo\"");
            // Set cấp và loại mật bảo
            mCheMatBao.setCapMB(capMB);
            mCheMatBao.setLoaiMB(loaiMB);

            // Mở bảng chế mật bảo
            if (mCheMatBao.moBangCheMB())
            {
                // Mở bảng chế mật bảo cần chế
                mCheMatBao.che();
            }
        }

        /*
         * Function: Chạy Đổi Năng Nổ
         * Description: Chạy Đổi Năng Nổ
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-16 - Updated At: 2019-11-16
         */
        public void runDoiNangNo(bool useLevel4 = true)
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            mDoiNangNo.setLoaiNL(mCharacter.DoiNangNoLoai);

            mAuto.writeStatus("Bắt đầu \"Đổi năng nổ\"");
            if (mDoiNangNo.diChuyenDenDoiNN(5))
            {
                while (mDoiNangNo.doiNangNo(5))
                {
                    mAuto.writeStatus("Chờ 1s rồi đổi tiếp.");
                    Thread.Sleep(1000);
                }
            }

            if (useLevel4)
            {
                if (mDoiNangNo.diChuyenDenDoiNN(4))
                {
                    while (mDoiNangNo.doiNangNo(4))
                    {
                        mAuto.writeStatus("Chờ 1s rồi đổi tiếp.");
                        Thread.Sleep(1000);
                    }
                }
            }
        }

        /*
         * Function: Chạy Q Trị An
         * Description: Chạy Q Trị An
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-09 - Updated At: 2019-11-09
         */
        public void runTriAn()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            int i = 0;

            mAuto.writeStatus("Bắt đầu \"Chạy Trị An\" ...");

            // Chuyển qua bảng chat "Hệ thống"
            mAuto.writeStatus("Chuyển qua bảng chat \"Hệ thống\" ...");
            mAuto.clickToImage(Constant.ImagePathGlobalChatTabHeThong);

            // Block khung chat
            mAuto.writeStatus("Block khung chat ...");
            mAuto.clickImageByGroup("global", "blockchat");

            // Bật auto
            mAuto.writeStatus("Bật auto ...");
            mAuto.batAuto();

            // Nếu vip dưới 6 thì mới chạy cái này
            if (mCharacter.VipLevel < 6 && mCharacter.VipLevel > 0)
            {
                // Xuong
                mAuto.bayXuong();
            }

            while (!mChayTriAn.completed && i < Constant.MaxLoop && mCharacter.Running == 1)
            {
                i++;
                mChayTriAn.nhanQ();
                if (!mChayTriAn.completed)
                {
                    mChayTriAn.chayQ();
                }
            }

        }

        /*
         * Function: Trồng nguyên liệu
         * Description: Trồng nguyên liệu
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-09 - Updated At: 2019-11-09
         */
        public void trongNL()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            mAuto.writeStatus("Bắt đầu \"Trồng Nguyên Liệu\" ...");
            mTrongNL.moTrangVien();
            if (mTrongNL.kiemTraSoDatTrong())
            {
                mTrongNL.moNuoiTrong();
                mTrongNL.chonNL();
                mTrongNL.trong();
            }
            mTrongNL.thuHoach();
            mTrongNL.dongTrangVien();
        }

        public List<Point> collectMapMiniPoints()
        {
            mAuto.writeStatus("Thu thập điểm trên bản đồ");
            List<Point> mapPoints = new List<Point>();

            // Mở bảng đồ mini
            //mAuto.clickToImage(Constant.ImagePathMiniMap);
            mAuto.sendKey("~");

            var full_screen = CaptureHelper.CaptureWindow(mHWnd);

            // Tắt các bảng nổi
            mAuto.closeAllDialog();

            Bitmap iBtn = ImageScanOpenCV.GetImage(Constant.ImagePathGlobalMiniMap);
            var pBtn = ImageScanOpenCV.FindOutPoint((Bitmap)full_screen, iBtn);

            if (pBtn != null)
            {
                int x_start_point = pBtn.Value.X + 0;
                int y_start_point = pBtn.Value.Y + 60;
                for (int y = 0; y < 5; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        mapPoints.Add(new Point(x_start_point + (x * 100) + 50, y_start_point + (y * 56)));
                    }
                }
            }

            return mapPoints;
        }

        public List<Bitmap> collectMapMiniPath()
        {
            mAuto.writeStatus("Thu thập mảnh bản đồ");
            List<Bitmap> mapPaths = new List<Bitmap>();

            // Mở bảng đồ mini
            //mAuto.clickToImage(Constant.ImagePathMiniMap);
            mAuto.sendKey("~");

            var full_screen = CaptureHelper.CaptureWindow(mHWnd);

            // Tắt các bảng nổi
            mAuto.closeAllDialog();

            Bitmap iBtn = ImageScanOpenCV.GetImage(Constant.ImagePathGlobalMiniMap);
            var pBtn = ImageScanOpenCV.FindOutPoint((Bitmap)full_screen, iBtn);

            if (pBtn != null)
            {
                int x_start_point = pBtn.Value.X + 0;
                int y_start_point = pBtn.Value.Y + 60;
                for (int y = 0; y < 5; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        mapPaths.Add(CaptureHelper.CropImage((Bitmap)full_screen, new Rectangle(
                            x_start_point + (x * 100),
                            y_start_point + (y * 56),
                            100, 56)));
                    }
                }
            }

            return mapPaths;
        }

        public void moveAndFindMonsters(List<Point> mapPoints, List<Monster> monsters, string monster_name, bool is_bug_flight)
        {
            string imageTalkMonster = Constant.ImagePathDoiThoai + monster_name + ".png";
            int i = 0;
            while (i < mapPoints.Count)
            {
                // Tắt các bảng nổi
                mAuto.closeAllDialog();

                if (!is_bug_flight)
                {
                    // Mở menu phải
                    mAuto.moMenuPhai();

                    // Bay lên
                    mAuto.bay();
                }

                // Mở bảng đồ mini
                //mAuto.clickToImage(Constant.ImagePathMiniMap);
                mAuto.sendKey("~");

                // Nhấp vào vị trí map
                mAuto.clickPoint(mapPoints[i].X, mapPoints[i].Y);

                if (!is_bug_flight)
                {
                    // Đóng menu phải
                    mAuto.dongMenuPhai();
                }

                if (i % 4 == 0)
                {
                    Thread.Sleep(4000);
                }

                // Tắt các bảng nổi
                mAuto.closeAllDialog();

                Thread.Sleep(1000);

                // tìm quái vật
                while (isExistsMonster(monsters))
                {
                    int y = 0;
                    while (!mAuto.findImage(imageTalkMonster) && y < monsters.Count)
                    {
                        if (mAuto.findImage(monsters[y].imagePath))
                        {
                            if (!is_bug_flight)
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
                            }

                            mAuto.clickToImage(monsters[y].imagePath, monsters[y].x, monsters[y].y);
                            Thread.Sleep(1000);
                        }

                        y++;
                    }

                    // Đánh
                    mAuto.clickToImage(imageTalkMonster);

                    // Nghỉ 5s nếu nhân vật đang trong trận đấu
                    bool inBattle = false;
                    while (mAuto.dangTrongTranDau())
                    {
                        inBattle = true;
                        mAuto.clickImageByGroup("global", "inbattleauto");
                        Thread.Sleep(4000);
                    }

                    if (inBattle)
                    {
                        Thread.Sleep(2000);
                    }
                }

                i++;
            }
        }

        public void moveAndFindMonstersWithPathImage(List<Bitmap> mapPaths, List<Monster> monsters, string monster_name)
        {
            string imageTalkMonster = Constant.ImagePathDoiThoai + monster_name + ".png";
            int i = 0;
            while (i < mapPaths.Count)
            {
                if (i == 0 || i == 3 || i == 16 || i == 19)
                {
                    i++;
                    continue;
                }

                // Mở menu phải
                mAuto.moMenuPhai();

                // Bay lên
                mAuto.bay();

                // Tắt các bảng nổi
                mAuto.closeAllDialog();

                // Mở bảng đồ mini
                //mAuto.clickToImage(Constant.ImagePathMiniMap);
                mAuto.sendKey("~");

                // Nhấp vào vị trí map
                mAuto.clickImage(mapPaths[i], 50, 0);

                // Tắt các bảng nổi
                mAuto.closeAllDialog();

                // Đóng menu phải
                mAuto.dongMenuPhai();

                if(i%4 == 0)
                {
                    Thread.Sleep(4000);
                }

                Thread.Sleep(1000);

                // tìm quái vật
                while (isExistsMonster(monsters))
                {
                    int y = 0;
                    while (!mAuto.findImage(imageTalkMonster) && y < monsters.Count)
                    {
                        if (mAuto.findImage(monsters[y].imagePath))
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

                            mAuto.clickToImage(monsters[y].imagePath, monsters[y].x, monsters[y].y);
                            Thread.Sleep(1000);
                        }

                        y++;
                    }

                    // Đánh
                    mAuto.clickToImage(imageTalkMonster);

                    // Nghỉ 5s nếu nhân vật đang trong trận đấu
                    while (mAuto.dangTrongTranDau())
                    {
                        mAuto.clickImageByGroup("global", "inbattleauto");
                        Thread.Sleep(5000);
                    }

                    Thread.Sleep(2000);
                }

                i++;
            }
        }

        public bool isExistsMonster(List<Monster> monsters)
        {
            int y = 0;
            while (y < monsters.Count)
            {
                if (mAuto.findImage(monsters[y].imagePath))
                {
                    return true;
                }

                y++;
            }

            return false;
        }

        public List<Monster> initListMonsters(string monster_name, int left_name_pos, int right_name_pos)
        {
            List<Monster> monsters = new List<Monster>();

            monsters.Add(new Monster(Constant.ImagePathEventFolder + monster_name + "1" + ".png", 0, -20));
            monsters.Add(new Monster(Constant.ImagePathEventFolder + monster_name + "2" + ".png", 0, -20));
            monsters.Add(new Monster(Constant.ImagePathEventFolder + monster_name + "3" + ".png", 0, -20));
            monsters.Add(new Monster(Constant.ImagePathEventFolder + monster_name + "4" + ".png", 0, -20));
            monsters.Add(new Monster(Constant.ImagePathEventFolder + monster_name + "5" + ".png", 0, -20));
            monsters.Add(new Monster(Constant.ImagePathEventFolder + monster_name + "6" + ".png", 0, -20));
            monsters.Add(new Monster(Constant.ImagePathEventFolder + monster_name + "7" + ".png", 0, -20));
            monsters.Add(new Monster(Constant.ImagePathEventFolder + monster_name + "8" + ".png", 0, -20));
            monsters.Add(new Monster(Constant.ImagePathEventFolder + "ten" + monster_name + "1" + ".png", left_name_pos, -50));
            monsters.Add(new Monster(Constant.ImagePathEventFolder + "ten" + monster_name + "2" + ".png", right_name_pos, -50));

            return monsters;
        }
    }
}
