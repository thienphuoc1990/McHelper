using AutoVPT.Objects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

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

        public void runEventWithCode()
        {
            if (mCharacter.Running != 2)
            {
                MessageBox.Show("Nhân vật " + mCharacter.ID + " đang không được chạy hoặc đang chạy auto khác như: daily, ...");
                return;
            }

            startGameIfNotExists();

            mGeneralFunctions.prepareScreen();

            findMonsterByCode("nguoituyetcuonghoan");
        }

        public void runEventBugFlight()
        {
            if (mCharacter.Running != 2)
            {
                MessageBox.Show("Nhân vật " + mCharacter.ID + " đang không được chạy hoặc đang chạy auto khác như: daily, ...");
                return;
            }

            startGameIfNotExists();

            mGeneralFunctions.prepareScreen();

            findMonstersInMap("nguoituyetcuonghoan", true);
            //findMonsterByCode("nguoituyetcuonghoan");
        }

        public void runXuQue()
        {
            if (mCharacter.Running != 2)
            {
                MessageBox.Show("Nhân vật " + mCharacter.ID + " đang không được chạy hoặc đang chạy auto khác như: daily, ...");
                return;
            }

            startGameIfNotExists();

            mGeneralFunctions.prepareScreen();
            mGeneralFunctions.xuQue();
        }

        public void runEvent()
        {
            if (mCharacter.Running != 2)
            {
                MessageBox.Show("Nhân vật " + mCharacter.ID + " đang không được chạy hoặc đang chạy auto khác như: daily, ...");
                return;
            }

            startGameIfNotExists();

            mGeneralFunctions.prepareScreen();

            findMonstersInMap("nguoituyetcuonghoan");
            //findMonsterByCode("nguoituyetcuonghoan");
        }

        public void findMonsterByCode(string monster_name)
        {
            string imageTalkMonster = Constant.ImagePathDoiThoai + monster_name + ".png";
            int x = 0;
            int y = 0;

            List<Map> maps = new List<Map>();
            maps.Add(new Map("leduongbac", 1, 30, -20));
            maps.Add(new Map("leduongnam", 1, 10, -20));
            maps.Add(new Map("laptuyetdia", 1, 30, -20));
            maps.Add(new Map("anhvucanh", 1, 60, -20));
            maps.Add(new Map("bangtuyetnguyen", 1, 10, -20));

            List<string> tmpPaths = new List<string>();
            tmpPaths.Add("resources/event/ldb1.png");
            tmpPaths.Add("resources/event/ldb2.png");
            tmpPaths.Add("resources/event/ldb3.png");
            tmpPaths.Add("resources/event/ldb4.png");
            tmpPaths.Add("resources/event/ldb5.png");
            maps[0].setPosPaths(tmpPaths);

            List<string> tmpPathsLDN = new List<string>();
            tmpPathsLDN.Add("resources/event/ldn1.png");
            tmpPathsLDN.Add("resources/event/ldn2.png");
            tmpPathsLDN.Add("resources/event/ldn3.png");
            tmpPathsLDN.Add("resources/event/ldn4.png");
            tmpPathsLDN.Add("resources/event/ldn5.png");
            maps[1].setPosPaths(tmpPathsLDN);

            List<string> tmpPathsLTD = new List<string>();
            tmpPathsLTD.Add("resources/event/ltd1.png");
            tmpPathsLTD.Add("resources/event/ltd2.png");
            tmpPathsLTD.Add("resources/event/ltd3.png");
            tmpPathsLTD.Add("resources/event/ltd4.png");
            tmpPathsLTD.Add("resources/event/ltd5.png");
            maps[2].setPosPaths(tmpPathsLTD);

            List<string> tmpPathsAVC = new List<string>();
            tmpPathsAVC.Add("resources/event/avc1.png");
            tmpPathsAVC.Add("resources/event/avc2.png");
            tmpPathsAVC.Add("resources/event/avc3.png");
            tmpPathsAVC.Add("resources/event/avc4.png");
            tmpPathsAVC.Add("resources/event/avc5.png");
            maps[3].setPosPaths(tmpPathsAVC);

            List<string> tmpPathsBTN = new List<string>();
            tmpPathsBTN.Add("resources/event/btn1.png");
            tmpPathsBTN.Add("resources/event/btn2.png");
            tmpPathsBTN.Add("resources/event/btn3.png");
            tmpPathsBTN.Add("resources/event/btn4.png");
            tmpPathsBTN.Add("resources/event/btn5.png");
            maps[4].setPosPaths(tmpPathsBTN);

            while (mCharacter.Running == 2)
            {
                x = 0;

                while (x < maps.Count)
                {

                    // Di chuyển đến Map
                    if (!mAuto.moveToMapNhom(maps[x].name, maps[x].mapIndex, maps[x].x, maps[x].y))
                    {
                        mAuto.writeStatus("Không thể di chuyển đến " + maps[x].name + ", thử lại ...");
                    }

                    y = 0;

                    while (y < maps[x].posPaths.Count)
                    {
                        // Check đang di chuyển
                        do
                        {
                            // Nhấp vào code
                            mAuto.clickToImage(maps[x].posPaths[y], -20);
                            if (mAuto.findImageByGroup("global", "movenhom"))
                            {
                                // Tắt các bảng nổi
                                mAuto.closeAllDialog();
                            }
                        } while (mAuto.isMoving());

                        Thread.Sleep(1000);

                        // Check có bảng đối thoại ko ?
                        if (mAuto.findImage(imageTalkMonster))
                        {
                            // Đánh quái
                            mAuto.clickToImage(imageTalkMonster);

                            Thread.Sleep(2000);

                            // Nghỉ 5s nếu nhân vật đang trong trận đấu
                            bool inBattle = false;
                            while (mAuto.dangTrongTranDau())
                            {
                                inBattle = true;
                                mAuto.clickImageByGroup("global", "inbattleauto");
                                Thread.Sleep(2000);
                            }

                            if (inBattle)
                            {
                                Thread.Sleep(2000);
                            }
                        }

                        y++;

                    }

                    x++;
                }
            }
        }

        public void findMonstersInMap(string monster_name, bool is_bug_flight = false)
        {
            List<Map> maps = new List<Map>();
            maps.Add(new Map("leduongbac", 1, 30, -20));
            maps.Add(new Map("leduongnam", 1, 10, -20));
            maps.Add(new Map("laptuyetdia", 1, 30, -20));
            maps.Add(new Map("anhvucanh", 1, 60, -20));
            maps.Add(new Map("bangtuyetnguyen", 1, 10, -20));

            List<Monster> monsters = mGeneralFunctions.initListMonsters(monster_name, 40, -40);

            List<Point> mapPoints = mGeneralFunctions.collectMapMiniPoints();

            while (mCharacter.Running == 2)
            {
                int x = 0;
                while (x < maps.Count)
                {
                    // Di chuyển đến Map
                    if (!mAuto.moveToMapNhom(maps[x].name, maps[x].mapIndex, maps[x].x, maps[x].y))
                    {
                        mAuto.writeStatus("Không thể di chuyển đến " + maps[x].name + ", thử lại ...");
                    }

                    mGeneralFunctions.moveAndFindMonsters(mapPoints, monsters, monster_name, is_bug_flight);

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
                if (mCharacter.VipPromotion == 1 && mCharacter.StatusVipPromotion == 0)
                {
                    i++;
                    mGeneralFunctions.nhanVIP();
                    mCharacter.StatusVipPromotion = 1;
                    Helper.saveSettingsToXML(mCharacter);
                }

                // Check to run "Nhận và Auto Phụ Bản"
                if (mCharacter.AutoPhuBan == 1 && mCharacter.StatusAutoPhuBan == 0)
                {
                    i++;
                    string[] phuBan = mCharacter.AutoPhuBanDanhSach.Split(',');
                    mGeneralFunctions.runNhanAutoPB(phuBan);
                    mCharacter.StatusAutoPhuBan = 1;
                    Helper.saveSettingsToXML(mCharacter);
                }

                // "Rút bộ"
                if (mCharacter.RutBo == 1 && mCharacter.StatusRutBo == 0)
                {
                    i++;
                    mGeneralFunctions.rutBo();
                    mCharacter.StatusRutBo = 1;
                    Helper.saveSettingsToXML(mCharacter);
                }

                // "Đổi thưởng Không Gian Điêu Khắc"
                if (mCharacter.DoiKGDK == 1 && mCharacter.StatusDoiKGDK == 0)
                {
                    i++;
                    mGeneralFunctions.khongGianDieuKhac();
                    mCharacter.StatusDoiKGDK = 1;
                    Helper.saveSettingsToXML(mCharacter);
                }

                // "Nhận thưởng hành lang"
                if (mCharacter.NhanThuongHLVT == 1 && mCharacter.StatusNhanThuongHLVT == 0)
                {
                    i++;
                    mGeneralFunctions.nhanThuongHanhLang();
                    mCharacter.StatusNhanThuongHLVT = 1;
                    Helper.saveSettingsToXML(mCharacter);
                }

                // Check to run "Rung cây"
                if (mCharacter.UocNguyen == 1 && mCharacter.StatusUocNguyen == 0)
                {
                    i++;
                    mGeneralFunctions.rungCay();
                    mCharacter.StatusUocNguyen = 1;
                    Helper.saveSettingsToXML(mCharacter);
                }

                // Check to run "Chế mật bảo"
                if (mCharacter.CheMatBao == 1 && mCharacter.StatusCheMatBao == 0)
                {
                    i++;
                    mGeneralFunctions.runCheMatBao(mCharacter.CheMatBaoLoai, mCharacter.CheMatBaoCap);
                    mCharacter.StatusCheMatBao = 1;
                    Helper.saveSettingsToXML(mCharacter);
                }

                // Check to run "Tu Hành"
                if (mCharacter.TuHanh == 1 && mCharacter.StatusTuHanh == 0)
                {
                    i++;
                    mGeneralFunctions.runAutoTuHanh();
                    mCharacter.StatusTuHanh = 1;
                    Helper.saveSettingsToXML(mCharacter);

                    //// Bug online sau khi tu hành
                    //mGeneralFunctions.runNhanHoiPhuc();

                    //if (mCharacter.RunToLast == 1)
                    //{
                    //    // Ngủ 30p sau khi auto tu hành
                    //    Thread.Sleep(60 * 30 * 1000);

                    //    startGameIfNotExists();

                    //    mGeneralFunctions.prepareScreen();
                    //}
                    //else
                    //{
                    //    Thread.CurrentThread.Abort();
                    //}
                }

                // Check to run "Auto Thần tu"
                if (mCharacter.AutoThanTu == 1 && mCharacter.StatusAutoThanTu == 0)
                {
                    i++;
                    mGeneralFunctions.runAutoThanTu();
                    mCharacter.StatusAutoThanTu = 1;
                    Helper.saveSettingsToXML(mCharacter);

                    //// Bug online sau khi tu hành
                    //mGeneralFunctions.runNhanHoiPhuc();

                    //if (mCharacter.RunToLast == 1)
                    //{
                    //    // Ngủ 30p sau khi auto tu hành
                    //    Thread.Sleep(60 * 15 * 1000);

                    //    startGameIfNotExists();

                    //    mGeneralFunctions.prepareScreen();
                    //}
                    //else
                    //{
                    //    Thread.CurrentThread.Abort();
                    //}
                }

                // Check to run "Chạy Trị An"
                if (mCharacter.TriAn == 1 && mCharacter.StatusTriAn == 0)
                {
                    i++;
                    mGeneralFunctions.runTriAn();
                    mCharacter.StatusTriAn = 1;
                    Helper.saveSettingsToXML(mCharacter);
                }

                // Check to run "Đổi năng nổ"
                if (mCharacter.DoiNangNo == 1)
                {
                    mGeneralFunctions.nhanThuongAutoPhuBan();
                    mGeneralFunctions.runDoiNangNo(mCharacter.DoiNangNoNL4 == 1);
                }

                if (mCharacter.NhanHoiPhuc == 1)
                {
                    mGeneralFunctions.hoiPhuc();
                }

                if (i == 0)
                {
                    mCharacter.Running = 0;
                    Helper.saveSettingsToXML(mCharacter);
                    Thread.CurrentThread.Abort();
                    break;
                }
                else
                {
                    // Run ảo ma & đá pet
                    var numberOfLoop = 0;
                    while (numberOfLoop <= Constant.MaxLoop)
                    {
                        if (numberOfLoop < 3)
                        {
                            mGeneralFunctions.aoMa();
                        }

                        mGeneralFunctions.dauPet();

                        numberOfLoop++;
                        Thread.Sleep(60 * 10 * 1000);
                    }
                }

                Helper.writeStatus(mTextBoxStatus, mCharacter.ID, "Ngừng 1 phút");
                Thread.Sleep(10 * 1000);
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

        public void loginToGame()
        {
            runAction("logintogame", () => startGameIfNotExists());
        }

        public void daPet()
        {
            runAction("dapet", () => mGeneralFunctions.dauPet());
        }

        public void autoPhuBan()
        {
            runAction("phuban", () => mGeneralFunctions.runNhanAutoPB(mCharacter.AutoPhuBanDanhSach.Split(',')));
        }

        public void captureImage()
        {
            startGameIfNotExists();
            mAuto.captureImage();
        }

        public void aoMa()
        {
            runAction("aoma", () => mGeneralFunctions.aoMa());
        }

        public void hoiPhuc()
        {
            runAction("hoiphuc", () => mGeneralFunctions.hoiPhuc());
        }

        public void nhanThuongAutoPhuBan()
        {
            runAction("nhanThuongAutoPhuBan", () => mGeneralFunctions.nhanThuongAutoPhuBan());
        }

        public void nhanThuongHanhLang()
        {
            runAction("nhanThuongHanhLang", () => mGeneralFunctions.nhanThuongHanhLang());
        }

        public void runAutoTuHanh()
        {
            runAction("runAutoTuHanh", () => mGeneralFunctions.runAutoTuHanh());
        }

        public void khongGianDieuKhac()
        {
            runAction("khongGianDieuKhac", () => mGeneralFunctions.khongGianDieuKhac());
        }

        public void trongNL()
        {
            runAction("trongNL", () => mGeneralFunctions.trongNL());
        }

        public void doiNangNo()
        {
            runAction("doinangno", () => mGeneralFunctions.runDoiNangNo(mCharacter.DoiNangNoNL4 == 1));
        }

        private void runAction(String actionName, Action action)
        {
            if (mCharacter.Running != 1)
            {
                Helper.writeStatus(mTextBoxStatus, mCharacter.ID, "Nhân vật " + mCharacter.ID + " đang không được chạy hoặc đang chạy auto khác như: event, ...");
                return;
            }
            startGameIfNotExists();
            mGeneralFunctions.prepareScreen();
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Helper.writeStatus(mTextBoxStatus, mCharacter.ID, $"Lỗi khi thực hiện hành động {actionName}: {ex.Message}");
            }
            mCharacter.Running = 0;
            Helper.saveSettingsToXML(mCharacter);
            foreach (var thread in Helper.threadList)
            {
                if (thread.Name == (mCharacter.ID + actionName))
                {
                    Helper.writeStatus(mTextBoxStatus, mCharacter.ID, "Đã ngừng auto");
                    thread.Abort();
                    Helper.writeStatus(mTextBoxStatus, mCharacter.ID, "Đã ngừng auto sau khi abort");
                }
            }
        }
    }
}
