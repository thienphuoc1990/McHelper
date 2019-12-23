using AutoVPT.Libs;
using AutoVPT.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        TextBox mTextBoxStatus;

        public MainAuto(IntPtr hWnd, Character character, TextBox textBoxStatus)
        {
            mHWnd = hWnd;
            mCharacter = character;
            mWindowName = mCharacter.ID;
            mTextBoxStatus = textBoxStatus;
            mAuto = new AutoFeatures(hWnd, mWindowName, textBoxStatus, mCharacter);
            mGeneralFunctions = new GeneralFunctions(hWnd, mCharacter, textBoxStatus);
        }

        public void runEvent()
        {
            if (mCharacter.Running != 2)
            {
                MessageBox.Show("Nhân vật " + mCharacter.ID + " đang không được chạy hoặc đang chạy auto khác như: daily, ...");
                return;
            }

            List<Map> maps = new List<Map>();
            maps.Add(new Map("leduongbac", 1, 30, -20));
            maps.Add(new Map("leduongnam", 1, 10, -20));
            maps.Add(new Map("laptuyetdia", 1, 30, -20));
            maps.Add(new Map("anhvucanh", 1, 60, -20));
            maps.Add(new Map("bangtuyetnguyen", 1, 10, -20));

            List<Monster> monsters = mGeneralFunctions.initListMonsters("nguoituyetcuonghoan", 40, -40);

            startGameIfNotExists();

            mGeneralFunctions.prepareScreen();

            while (mCharacter.Running == 2)
            {
                int x = 0;
                while (x < maps.Count)
                {
                    // Di chuyển đến Map
                    if (!mAuto.moveToMap(maps[x].name, maps[x].mapIndex, maps[x].x, maps[x].y))
                    {
                        mAuto.writeStatus("Không thể di chuyển đến " + maps[x].name + ", thử lại ...");
                    }

                    List<Bitmap> mapPaths = mGeneralFunctions.collectMapMiniPath();

                    mGeneralFunctions.moveAndFindMonsters(mapPaths, monsters, "nguoituyetcuonghoan");

                    x++;
                }
            }
        }

        public void run()
        {
            if (mCharacter.Running != 1)
            {
                MessageBox.Show("Nhân vật " + mCharacter.ID + " đang không được chạy hoặc đang chạy auto khác như: event, ...");
                return;
            }

            startGameIfNotExists();

            mGeneralFunctions.prepareScreen();

            while (mCharacter.Running == 1)
            {
                var i = 0;
                // Trồng nguyên liệu
                if (mCharacter.TrongNL == 1)
                {
                    mGeneralFunctions.trongNL();
                }

                // "Nhận VIP"
                if (mCharacter.VipPromotion == 1 || mCharacter.VipPromotion == 2)
                {
                    i++;
                    mGeneralFunctions.nhanVIP();
                    mCharacter.VipPromotion = 3;
                    CharacterList.UpdateCharacterAllFields(mCharacter);
                }

                // Check to run "Nhận và Auto Phụ Bản"
                if (mCharacter.AutoPhuBan == 1 || mCharacter.AutoPhuBan == 2)
                {
                    i++;
                    string[] phuBan = mCharacter.AutoPhuBanDanhSach.Split(',');
                    mGeneralFunctions.runNhanAutoPB(phuBan);
                    mCharacter.AutoPhuBan = 3;
                    CharacterList.UpdateCharacterAllFields(mCharacter);
                }

                // "Rút bộ"
                if (mCharacter.RutBo == 1 || mCharacter.RutBo == 2)
                {
                    i++;
                    mGeneralFunctions.rutBo();
                    mCharacter.RutBo = 3;
                    CharacterList.UpdateCharacterAllFields(mCharacter);
                }

                // "Đổi thưởng Không Gian Điêu Khắc"
                if (mCharacter.DoiKGDK == 1 || mCharacter.DoiKGDK == 2)
                {
                    i++;
                    mGeneralFunctions.khongGianDieuKhac();
                    mCharacter.DoiKGDK = 3;
                    CharacterList.UpdateCharacterAllFields(mCharacter);
                }

                // "Nhận thưởng hành lang"
                if (mCharacter.NhanThuongHLVT == 1 || mCharacter.NhanThuongHLVT == 2)
                {
                    i++;
                    mGeneralFunctions.nhanThuongHanhLang();
                    mCharacter.NhanThuongHLVT = 3;
                    CharacterList.UpdateCharacterAllFields(mCharacter);
                }

                // Check to run "Rung cây"
                if (mCharacter.UocNguyen == 1 || mCharacter.UocNguyen == 2)
                {
                    i++;
                    mGeneralFunctions.rungCay();
                    mCharacter.UocNguyen = 3;
                    CharacterList.UpdateCharacterAllFields(mCharacter);
                }

                // Check to run "Chế mật bảo"
                if (mCharacter.CheMatBao == 1 || mCharacter.CheMatBao == 2)
                {
                    i++;
                    mGeneralFunctions.runCheMatBao(mCharacter.CheMatBaoLoai, mCharacter.CheMatBaoCap);
                    mCharacter.CheMatBao = 3;
                    CharacterList.UpdateCharacterAllFields(mCharacter);
                }

                // Check to run "Tu Hành"
                if (mCharacter.TuHanh == 1 || mCharacter.TuHanh == 2)
                {
                    i++;
                    mGeneralFunctions.runAutoTuHanh();
                    mCharacter.TuHanh = 3;
                    CharacterList.UpdateCharacterAllFields(mCharacter);

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
                    CharacterList.UpdateCharacterAllFields(mCharacter);
                }

                // Check to run "Đổi năng nổ"
                if (mCharacter.DoiNangNo == 1)
                {
                    mGeneralFunctions.runDoiNangNo(mCharacter.DoiNangNoNL4 == 1);
                }

                if (i == 0)
                {
                    // Check to run "Bug Online"
                    if (mCharacter.BugOnline == 1)
                    {
                        mGeneralFunctions.runBugOnline();
                        mCharacter.Running = 0;
                        break;
                    }
                }

                Helper.writeStatus(mTextBoxStatus, mCharacter.ID, "Ngừng 1 phút");
                Thread.Sleep(60 * 1000);
            }
        }

        private void startGameIfNotExists()
        {
            // Lập lại việc check và mở windows
            while (!mGeneralFunctions.checkWindowOpen())
            {
                mGeneralFunctions.openWindow();
                Thread.Sleep(5000);
            }

            // Login vào game
            while (!mGeneralFunctions.isInGame())
            {
                mGeneralFunctions.login();
                Thread.Sleep(5000);
            }

            Helper.writeStatus(mTextBoxStatus, mCharacter.ID, "Đã vào game");
        }

        public void testRightClick(string group, string name, int x, int y)
        {
            mAuto.clickRightImageByGroup(group, name, false, false, 1, x, y);
        }
    }
}
