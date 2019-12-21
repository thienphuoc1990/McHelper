using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.CheckedListBox;

namespace AutoVPT.Libs
{
    class MainAuto
    {
        private IntPtr mHWnd;
        private string mWindowName;
        public AutoFeatures mAuto;
        private Character mCharacter;
        public GeneralFunctions mGeneralFunctions;
        public FormManageAuto mForm;
        TextBox mTextBoxStatus;

        public MainAuto(IntPtr hWnd, Character character, TextBox textBoxStatus, FormManageAuto form)
        {
            mHWnd = hWnd;
            mCharacter = character;
            mWindowName = character.ID;
            mTextBoxStatus = textBoxStatus;
            mAuto = new AutoFeatures(hWnd, mWindowName, textBoxStatus, mCharacter);
            mGeneralFunctions = new GeneralFunctions (hWnd, character, textBoxStatus);
            mForm = form;
        }

        public void run()
        {
            if(mCharacter.Running == 0)
            {
                return;
            }

            startGameIfNotExists();

            mGeneralFunctions.prepareScreen();

            while(mCharacter.Running == 1)
            {
                var i = 0;
                // Trồng nguyên liệu
                if (mCharacter.TrongNL == 1)
                {
                    i++;
                    mGeneralFunctions.trongNL();
                }

                // "Nhận VIP"
                if (mCharacter.VipPromotion == 1 || mCharacter.VipPromotion == 2)
                {
                    i++;
                    mGeneralFunctions.nhanVIP();
                    mCharacter.VipPromotion = 3;
                    mForm.updateStatusFeatures();
                }

                // Check to run "Nhận và Auto Phụ Bản"
                if (mCharacter.AutoPhuBan == 1 || mCharacter.AutoPhuBan == 2)
                {
                    i++;
                    string[] phuBan = mCharacter.AutoPhuBanDanhSach.Split(',');
                    mGeneralFunctions.runNhanAutoPB(phuBan);
                    mCharacter.AutoPhuBan = 3;
                    mForm.updateStatusFeatures();
                }

                // "Rút bộ"
                if (mCharacter.RutBo == 1 || mCharacter.RutBo == 2)
                {
                    i++;
                    mGeneralFunctions.rutBo();
                    mCharacter.RutBo = 3;
                    mForm.updateStatusFeatures();
                }

                // "Đổi thưởng Không Gian Điêu Khắc"
                if (mCharacter.DoiKGDK == 1 || mCharacter.DoiKGDK == 2)
                {
                    i++;
                    mGeneralFunctions.khongGianDieuKhac();
                    mCharacter.DoiKGDK = 3;
                    mForm.updateStatusFeatures();
                }

                // "Nhận thưởng hành lang"
                if (mCharacter.NhanThuongHLVT == 1 || mCharacter.NhanThuongHLVT == 2)
                {
                    i++;
                    mGeneralFunctions.nhanThuongHanhLang();
                    mCharacter.NhanThuongHLVT = 3;
                    mForm.updateStatusFeatures();
                }

                // Check to run "Rung cây"
                if (mCharacter.UocNguyen == 1 || mCharacter.UocNguyen == 2)
                {
                    i++;
                    mGeneralFunctions.rungCay();
                    mCharacter.UocNguyen = 3;
                    mForm.updateStatusFeatures();
                }

                // Check to run "Chế mật bảo"
                if (mCharacter.CheMatBao == 1 || mCharacter.CheMatBao == 2)
                {
                    i++;
                    mGeneralFunctions.runCheMatBao(mCharacter.CheMatBaoLoai, mCharacter.CheMatBaoCap);
                    mCharacter.CheMatBao = 3;
                    mForm.updateStatusFeatures();
                }

                // Check to run "Tu Hành"
                if (mCharacter.TuHanh == 1 || mCharacter.TuHanh == 2)
                {
                    i++;
                    mGeneralFunctions.runAutoTuHanh();
                    mCharacter.TuHanh = 3;
                    mForm.updateStatusFeatures();

                    // Bug online sau khi tu hành
                    mGeneralFunctions.runBugOnline();

                    // Ngủ 30p sau khi auto tu hành
                    Thread.Sleep(60 * 30 * 1000);

                    startGameIfNotExists();

                    mGeneralFunctions.prepareScreen();
                }

                // Check to run "Chạy Trị An"
                if (mCharacter.TriAn == 1 || mCharacter.TriAn == 2)
                {
                    i++;
                    mGeneralFunctions.runTriAn();
                    mCharacter.TriAn = 3;
                    mForm.updateStatusFeatures();
                }

                // Check to run "Đổi năng nổ"
                if (mCharacter.DoiNangNo == 1)
                {
                    i++;
                    mGeneralFunctions.runDoiNangNo(mCharacter.DoiNangNoNL4 == 1);
                }

                // Check to run "Bug Online"
                if (mCharacter.BugOnline == 1)
                {
                    mGeneralFunctions.runBugOnline();
                    if(i <= 2)
                    {
                        mCharacter.Running = 0;
                        break;
                    }
                }

                Helper.writeStatus(mTextBoxStatus, "Ngừng 1 phút");
                Thread.Sleep(60*1000);
            }
        }

        private void startGameIfNotExists()
        {
            // Lập lại việc check và mở windows
            while(!mGeneralFunctions.checkWindowOpen())
            {
                mGeneralFunctions.openWindow();
                Thread.Sleep(5000);
            }

            // Login vào game
            while(!mGeneralFunctions.isInGame())
            {
                mGeneralFunctions.login();
                Thread.Sleep(5000);
            }

            Helper.writeStatus(mTextBoxStatus, "Đã vào game");
        }

        public void testRightClick(string group, string name, int x, int y)
        {
            mAuto.clickRightImageByGroup(group, name, false, false, 1, x, y);
        }
    }
}
