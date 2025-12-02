using AutoVPT.Objects;
using AutoVPT.Domain;
using AutoVPT.DependencyInjection;
using AutoVPT.Services;
using AutoVPT.Services.Executors;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AutoVPT.Libs
{
    class MainAuto
    {
        private IntPtr mHWnd;
        private string mWindowName;
        public AutoFeatures mAuto;
        private Character mCharacter;
        private string [] mMembers;
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

        public void setMembers(string[] members)
        {
            mMembers = members;
        }

        #region Executor Integration Helpers

        /// <summary>
        /// Execute a feature using the new executor pattern
        /// </summary>
        private bool ExecuteFeature<TExecutor>(FeatureType featureType, Dictionary<string, string> parameters = null)
            where TExecutor : class, IFeatureExecutor
        {
            try
            {
                // Check GLOBAL stop flag first (takes precedence over individual character state)
                // This prevents any features from starting after Stop All is pressed
                if (Helper.IsStoppingAll())
                {
                    Helper.writeStatus(mTextBoxStatus, mCharacter.ID, $"Đã dừng tất cả - bỏ qua {featureType}");
                    return false;
                }

                // Check if we should stop before executing
                if (mCharacter.Running != 1)
                {
                    Helper.writeStatus(mTextBoxStatus, mCharacter.ID, $"Đã dừng trước khi thực hiện {featureType}");
                    return false;
                }

                // Ensure ServiceContainer is initialized (idempotent operation)
                ServiceContainer.Initialize(mTextBoxStatus);

                // Convert legacy Character to new CharacterAggregate
                var characterAggregate = CharacterAdapter.ToAggregate(mCharacter);

                // Create window services for this character
                var windowServices = ServiceContainer.CreateWindowServices(mHWnd, mCharacter);

                // Create the executor
                var executor = windowServices.CreateExecutor<TExecutor>(mCharacter.VipLevel) as IFeatureExecutor;

                // Get or create cancellation token for this feature
                string tokenKey = mCharacter.ID + featureType.ToString();
                var cancellationToken = Helper.GetCancellationToken(tokenKey);

                // Check if cancellation was already requested
                if (cancellationToken.IsCancellationRequested)
                {
                    Helper.writeStatus(mTextBoxStatus, mCharacter.ID, $"Đã hủy {featureType} (cancellation requested)");
                    Helper.RemoveToken(tokenKey);
                    windowServices.Dispose();
                    return false;
                }

                // Create execution context
                var context = new Services.ExecutionContext
                {
                    Character = characterAggregate,
                    WindowHandle = mHWnd,
                    Config = CreateFeatureConfig(featureType, parameters),
                    CancellationToken = cancellationToken,
                    StatusTextBox = mTextBoxStatus
                };

                // Execute with cancellation support
                // Execute the async method and wait for it synchronously
                // The executor's ExecuteAsync method already handles cancellation properly
                FeatureResult result;
                try
                {
                    result = executor.ExecuteAsync(context).GetAwaiter().GetResult();

                    // Log result
                    if (result.Success)
                    {
                        Helper.writeStatus(mTextBoxStatus, mCharacter.ID, $"✓ {featureType}: {result.Message}");
                    }
                    else
                    {
                        Helper.writeStatus(mTextBoxStatus, mCharacter.ID, $"✗ {featureType}: {result.Message}");
                    }

                    // Clean up
                    Helper.RemoveToken(tokenKey);
                    windowServices.Dispose();

                    return result.Success;
                }
                catch (OperationCanceledException)
                {
                    // Task was cancelled
                    Helper.writeStatus(mTextBoxStatus, mCharacter.ID, $"Đã hủy {featureType} (task cancelled)");
                    Helper.RemoveToken(tokenKey);
                    windowServices.Dispose();
                    return false;
                }
            }
            catch (OperationCanceledException)
            {
                // Feature was cancelled - this is expected when Stop All is pressed
                Helper.writeStatus(mTextBoxStatus, mCharacter.ID, $"Đã hủy {featureType}");
                Helper.RemoveToken(mCharacter.ID + featureType.ToString());
                return false;
            }
            catch (ThreadInterruptedException)
            {
                // Thread was interrupted - this is expected when Stop All is pressed
                Helper.writeStatus(mTextBoxStatus, mCharacter.ID, $"Đã dừng {featureType} (interrupted)");
                Helper.RemoveToken(mCharacter.ID + featureType.ToString());
                return false;
            }
            catch (Exception ex)
            {
                Helper.writeStatus(mTextBoxStatus, mCharacter.ID, $"Error in {featureType}: {ex.Message}");
                Logger.LogError(mCharacter.ID, $"ExecuteFeature<{typeof(TExecutor).Name}>", ex);
                Helper.RemoveToken(mCharacter.ID + featureType.ToString());
                return false;
            }
        }

        /// <summary>
        /// Create feature configuration from legacy character properties
        /// </summary>
        private FeatureConfig CreateFeatureConfig(FeatureType featureType, Dictionary<string, string> parameters = null)
        {
            var config = new FeatureConfig(featureType);

            // Add parameters if provided
            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    config.SetParameter(param.Key, param.Value);
                }
            }

            // Map legacy character properties to feature config based on feature type
            switch (featureType)
            {
                case FeatureType.CheMatBao:
                    config.SetParameter("Loai", mCharacter.CheMatBaoLoai);
                    config.SetParameter("Cap", mCharacter.CheMatBaoCap.ToString());
                    break;

                case FeatureType.TrongNL:
                    config.SetParameter("Loai", mCharacter.TrongNLLoai);
                    break;

                case FeatureType.TriAn:
                    config.SetParameter("VipLevel", mCharacter.VipLevel.ToString());
                    break;

                case FeatureType.DoiNangNo:
                    config.SetParameter("UseLevel4", mCharacter.DoiNangNoNL4 == 1 ? "true" : "false");
                    break;

                case FeatureType.AutoPhuBan:
                    config.SetParameter("DanhSach", mCharacter.AutoPhuBanDanhSach);
                    break;
            }

            return config;
        }

        #endregion

        public void runEventWithCode()
        {
            if (mCharacter.Running != 2)
            {
                MessageBox.Show("Nhân vật " + mCharacter.ID + " đang không được chạy hoặc đang chạy auto khác như: daily, ...");
                return;
            }

            startGameIfNotExists();

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

            findMonstersInMap("nguoituyetcuonghoan");
            //findMonsterByCode("nguoituyetcuonghoan");
        }

        public void findMonsterByCode(string monster_name)
        {
            // Register character for Stop All functionality
            Helper.RegisterRunningCharacter(mCharacter);

            try
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
            finally
            {
                Helper.UnregisterRunningCharacter(mCharacter.ID);
            }
        }

        public void findMonstersInMap(string monster_name, bool is_bug_flight = false)
        {
            // Register character for Stop All functionality
            Helper.RegisterRunningCharacter(mCharacter);

            try
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
            finally
            {
                Helper.UnregisterRunningCharacter(mCharacter.ID);
            }
        }

        public void run()
        {
            if (mCharacter.Running != 1)
            {
                MessageBox.Show("Nhân vật " + mCharacter.ID + " đang không được chạy hoặc đang chạy auto khác như: event, ...");
                return;
            }

            // Register this character as running so Stop All can find it
            Helper.RegisterRunningCharacter(mCharacter);

            try
            {
                startGameIfNotExists(true);

                while (mCharacter.Running == 1)
                {
                    // Check GLOBAL stop flag first (takes priority over individual character state)
                    // This ensures ALL characters stop immediately when Stop All is pressed
                    if (Helper.IsStoppingAll())
                    {
                        Helper.writeStatus(mTextBoxStatus, mCharacter.ID, "Đã dừng tất cả automation");
                        break;
                    }

                    // Check if we should stop before each iteration
                    // The Running flag is checked here and before each feature execution
                    // to ensure we stop immediately when StopAllRunningCharacters is called
                    if (mCharacter.Running != 1)
                    {
                        break;
                    }

                    var i = 0;

                    // "Nhận thưởng hành lang" - Using new executor pattern
                    if (mCharacter.Running == 1 && mCharacter.NhanThuongHLVT == 1 && mCharacter.StatusNhanThuongHLVT == 0)
                    {
                        i++;
                        ExecuteFeature<NhanThuongHLVTExecutor>(FeatureType.NhanThuongHLVT);
                        mCharacter.StatusNhanThuongHLVT = 1;
                        Helper.saveSettingsToXML(mCharacter);
                    }

                    // Check if we should stop before next feature (check global flag first)
                    if (Helper.IsStoppingAll() || mCharacter.Running != 1)
                    {
                        break;
                    }

                    // Check to run "Nhận và Auto Phụ Bản" - Using new executor pattern
                    if (mCharacter.Running == 1 && mCharacter.AutoPhuBan == 1 && mCharacter.StatusAutoPhuBan == 0)
                    {
                        i++;
                        ExecuteFeature<AutoPhuBanExecutor>(FeatureType.AutoPhuBan);
                        mCharacter.StatusAutoPhuBan = 1;
                        Helper.saveSettingsToXML(mCharacter);
                    }

                    // Check if we should stop before next feature (check global flag first)
                    if (Helper.IsStoppingAll() || mCharacter.Running != 1)
                    {
                        break;
                    }

                    // Check to run "Tu Hành" - Using new executor pattern
                    if (mCharacter.Running == 1 && mCharacter.TuHanh == 1 && mCharacter.StatusTuHanh == 0)
                    {
                        i++;
                        ExecuteFeature<TuHanhExecutor>(FeatureType.TuHanh);
                        mCharacter.StatusTuHanh = 1;
                        Helper.saveSettingsToXML(mCharacter);

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

                    // Check if we should stop before next feature (check global flag first)
                    if (Helper.IsStoppingAll() || mCharacter.Running != 1)
                    {
                        break;
                    }

                    // Trồng nguyên liệu - Using legacy implementation (works reliably)
                    if (mCharacter.Running == 1 && mCharacter.TrongNL == 1)
                    {
                        i++;
                        mGeneralFunctions.trongNL();
                        // Note: TrongNL doesn't update status, it can run multiple times per day
                    }

                    // Check if we should stop before next feature (check global flag first)
                    if (Helper.IsStoppingAll() || mCharacter.Running != 1)
                    {
                        break;
                    }

                    // "Nhận VIP" - Using new executor pattern
                    if (mCharacter.Running == 1 && mCharacter.VipPromotion == 1 && mCharacter.StatusVipPromotion == 0)
                    {
                        i++;
                        ExecuteFeature<VipPromotionExecutor>(FeatureType.VipPromotion);
                        mCharacter.StatusVipPromotion = 1;
                        Helper.saveSettingsToXML(mCharacter);
                    }

                    // Check if we should stop before next feature (check global flag first)
                    if (Helper.IsStoppingAll() || mCharacter.Running != 1)
                    {
                        break;
                    }

                    // "Rút bộ" - Using new executor pattern
                    if (mCharacter.Running == 1 && mCharacter.RutBo == 1 && mCharacter.StatusRutBo == 0)
                    {
                        i++;
                        ExecuteFeature<RutBoExecutor>(FeatureType.RutBo);
                        mCharacter.StatusRutBo = 1;
                        Helper.saveSettingsToXML(mCharacter);
                    }

                    // Check if we should stop before next feature (check global flag first)
                    if (Helper.IsStoppingAll() || mCharacter.Running != 1)
                    {
                        break;
                    }

                    // "Đổi thưởng Không Gian Điêu Khắc" - Using new executor pattern
                    if (mCharacter.Running == 1 && mCharacter.DoiKGDK == 1 && mCharacter.StatusDoiKGDK == 0)
                    {
                        i++;
                        ExecuteFeature<DoiKGDKExecutor>(FeatureType.DoiKGDK);
                        mCharacter.StatusDoiKGDK = 1;
                        Helper.saveSettingsToXML(mCharacter);
                    }

                    // Check if we should stop before next feature (check global flag first)
                    if (Helper.IsStoppingAll() || mCharacter.Running != 1)
                    {
                        break;
                    }

                    // "Nhận hồi phục" - Using new executor pattern
                    if (mCharacter.Running == 1 && mCharacter.NhanHoiPhuc == 1 && mCharacter.StatusNhanHoiPhuc == 0)
                    {
                        i++;
                        ExecuteFeature<NhanHoiPhucExecutor>(FeatureType.NhanHoiPhuc);
                        mCharacter.StatusNhanHoiPhuc = 1;
                        Helper.saveSettingsToXML(mCharacter);
                    }

                    // Check to run "Rung cây"
                    //if (mCharacter.UocNguyen == 1 && mCharacter.StatusUocNguyen == 0)
                    //{
                    //    i++;
                    //    mGeneralFunctions.rungCay();
                    //    mCharacter.StatusUocNguyen = 1;
                    //    Helper.saveSettingsToXML(mCharacter);
                    //}

                    // Check if we should stop before next feature (check global flag first)
                    if (Helper.IsStoppingAll() || mCharacter.Running != 1)
                    {
                        break;
                    }

                    // Check to run "Chế mật bảo" - Using new executor pattern
                    if (mCharacter.Running == 1 && mCharacter.CheMatBao == 1 && mCharacter.StatusCheMatBao == 0)
                    {
                        i++;
                        ExecuteFeature<CheMatBaoExecutor>(FeatureType.CheMatBao);
                        mCharacter.StatusCheMatBao = 1;
                        Helper.saveSettingsToXML(mCharacter);
                    }

                    // Check if we should stop before next feature (check global flag first)
                    if (Helper.IsStoppingAll() || mCharacter.Running != 1)
                    {
                        break;
                    }

                    // Check to run "Auto Thần tu" - Using new executor pattern
                    if (mCharacter.Running == 1 && mCharacter.AutoThanTu == 1 && mCharacter.StatusAutoThanTu == 0)
                    {
                        i++;
                        ExecuteFeature<AutoThanTuExecutor>(FeatureType.AutoThanTu);
                        mCharacter.StatusAutoThanTu = 1;
                        Helper.saveSettingsToXML(mCharacter);

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

                    // Check if we should stop before next feature (check global flag first)
                    if (Helper.IsStoppingAll() || mCharacter.Running != 1)
                    {
                        break;
                    }

                    // Check to run "Chạy Trị An" - Using new executor pattern
                    if (mCharacter.Running == 1 && mCharacter.TriAn == 1 && mCharacter.StatusTriAn == 0)
                    {
                        i++;
                        ExecuteFeature<TriAnExecutor>(FeatureType.TriAn);
                        mCharacter.StatusTriAn = 1;
                        Helper.saveSettingsToXML(mCharacter);
                    }

                    // Check if we should stop before next feature (check global flag first)
                    if (Helper.IsStoppingAll() || mCharacter.Running != 1)
                    {
                        break;
                    }

                    // Check to run "Đổi năng nổ" - Using new executor pattern
                    if (mCharacter.Running == 1 && mCharacter.DoiNangNo == 1 && mCharacter.RunToLast == 1)
                    {
                        mGeneralFunctions.nhanThuongAutoPhuBan();
                        ExecuteFeature<DoiNangNoExecutor>(FeatureType.DoiNangNo);
                    }

                    if (i == 0 || mCharacter.RunToLast != 1)
                    {
                        mCharacter.Running = 0;
                        Helper.saveSettingsToXML(mCharacter);
                        Thread.CurrentThread.Abort();
                        break;
                    }

                    Helper.writeStatus(mTextBoxStatus, mCharacter.ID, "Ngừng 30 phút cho thần tu hoặc tu hành");
                    try
                    {
                        Thread.Sleep(60 * 31 * 1000);
                    }
                    catch (ThreadInterruptedException)
                    {
                        // Thread was interrupted by Stop All - exit gracefully
                        Helper.writeStatus(mTextBoxStatus, mCharacter.ID, "Đã dừng automation");
                        mCharacter.Running = 0;
                        Helper.saveSettingsToXML(mCharacter);
                        return;
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
                // Thread was interrupted during automation - this is expected when Stop All is pressed
                Helper.writeStatus(mTextBoxStatus, mCharacter.ID, "Đã dừng automation (interrupted)");
                mCharacter.Running = 0;
                Helper.saveSettingsToXML(mCharacter);
            }
            finally
            {
                // Unregister character when auto stops
                Helper.UnregisterRunningCharacter(mCharacter.ID);
            }
        }

        private void startGameIfNotExists(bool isForcePrepareScreen = false)
        {
            bool isStarting = false;
            try
            {
                // Lập lại việc check và mở windows
                while (!mGeneralFunctions.checkWindowOpen())
                {
                    isStarting = true;
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
                if (isStarting || isForcePrepareScreen)
                {
                    mGeneralFunctions.prepareScreen();
                }
            }
            catch (ThreadInterruptedException)
            {
                // Thread was interrupted during game startup - exit gracefully
                Helper.writeStatus(mTextBoxStatus, mCharacter.ID, "Đã dừng (interrupted during startup)");
                mCharacter.Running = 0;
                Helper.saveSettingsToXML(mCharacter);
                throw; // Re-throw to propagate to caller
            }
        }

        public void testRightClick(string group, string name, int x, int y)
        {
            mAuto.clickRightImageByGroup(group, name, false, false, 1, x, y);
        }

        public void loginToGame()
        {
            runAction("logintogame", () => startGameIfNotExists());
        }

        public void danhSTMT()
        {
            runAction("danhSTMT", () => mGeneralFunctions.danhSTMT(mCharacter.DanhSTMTDanhSach.Split(',')));
        }

        public void daPet()
        {
            runAction("dapet", () => mGeneralFunctions.dauPet());
        }

        public void nhanKNVU()
        {
            runAction("nhanKNVU", () => mGeneralFunctions.nhanKNVU());
        }

        public void aoMaDaPetAllToEnd()
        {
            runAction("aoMaDaPetAllToEnd", () => mGeneralFunctions.aoMaDaPetAllToEnd());
        }

        public void daPetAllToEnd()
        {
            runAction("daPetAllToEnd", () => mGeneralFunctions.daPetAllToEnd());
        }

        public void goCanhKyTruong()
        {
            runAction("goCanhKyTruong", () => mGeneralFunctions.goCanhKyTruong());
        }

        public void taoNhom()
        {
            runAction("taoNhom", () => mGeneralFunctions.taoNhom(mMembers));
        }

        public void trainQuai()
        {
            runAction("trainQuai", () => mGeneralFunctions.trainQuai());
        }

        public void trainByMap()
        {
            runAction("trainByMap", () => mGeneralFunctions.trainByMap());
        }

        public void cheMatBao()
        {
            runAction("cheMatBao", () => mGeneralFunctions.runCheMatBao(mCharacter.CheMatBaoLoai, mCharacter.CheMatBaoCap));
        }

        public void batPet()
        {
            runAction("batPet", () => mGeneralFunctions.batPet());
        }

        public void dongYVaoNhom()
        {
            runAction("dongYVaoNhom", () => mGeneralFunctions.dongYVaoNhom());
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
            runAction("runAutoTuHanh", () => mGeneralFunctions.runAutoTuHanhByNVHN());
        }

        public void khongGianDieuKhac()
        {
            runAction("khongGianDieuKhac", () => mGeneralFunctions.khongGianDieuKhac());
        }

        public void trongNL()
        {
            // Using legacy implementation until new executor is fixed
            runAction("trongNL", () => mGeneralFunctions.trongNL());
        }

        public void truMa()
        {
            // Using new executor pattern
            runAction("truMa", () => ExecuteFeature<TruMaExecutor>(FeatureType.TruMa));
        }

        public void doiNangNo()
        {
            runAction("doinangno", () => mGeneralFunctions.runDoiNangNo(mCharacter.DoiNangNoNL4 == 1));
        }

        private void runAction(String actionName, Action action)
        {
            string threadKey = mCharacter.ID + actionName;
            CancellationToken cancellationToken = Helper.GetCancellationToken(threadKey);

            if (mCharacter.Running != 1)
            {
                Helper.writeStatus(mTextBoxStatus, mCharacter.ID, "Nhân vật " + mCharacter.ID + " đang không được chạy hoặc đang chạy auto khác như: event, ...");
                Helper.RemoveToken(threadKey);
                return;
            }

            try
            {
                startGameIfNotExists();
                action();
                Helper.writeStatus(mTextBoxStatus, mCharacter.ID, $"Hoàn thành {actionName}");
            }
            catch (ThreadInterruptedException)
            {
                // Thread was interrupted - expected when Stop All is pressed
                Helper.writeStatus(mTextBoxStatus, mCharacter.ID, $"Đã dừng {actionName} (interrupted)");
            }
            catch (OperationCanceledException)
            {
                Helper.writeStatus(mTextBoxStatus, mCharacter.ID, $"Đã dừng {actionName}");
            }
            catch (Exception ex)
            {
                Helper.writeStatus(mTextBoxStatus, mCharacter.ID, $"Lỗi khi thực hiện hành động {actionName}: {ex.Message}");
                Logger.LogError(mCharacter.ID, actionName, ex);
            }
            finally
            {
                mCharacter.Running = 0;
                Helper.saveSettingsToXML(mCharacter);
                Helper.RemoveToken(threadKey);
            }
        }
    }
}
