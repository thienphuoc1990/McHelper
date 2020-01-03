using AutoVPT.Objects;
using KAutoHelper;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace AutoVPT.Libs
{
    class AutoFeatures
    {
        public AutoIT au3 = new AutoIT();
        public IntPtr mHWnd;
        public string mWindowName;
        TextBox mTextBoxStatus;
        private Character mCharacter;
        public Random random = new Random();

        public AutoFeatures(IntPtr hWnd, string windowName, TextBox textBoxStatus, Character character)
        {
            mHWnd = hWnd;
            mWindowName = windowName;
            mTextBoxStatus = textBoxStatus;
            mCharacter = character;
        }

        public void closeFlash()
        {
            au3.winclose(mWindowName);
        }

        public void sendKey(string key, int wait = 1000)
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            au3.controlsend(mWindowName, key);
            Thread.Sleep(wait);
        }

        public void closeAllDialog()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            // Đóng tất cả hộp thoại đang có
            for (int i = 0; i <= 3; i++)
            {
                au3.controlsend(mWindowName, "{ESC}");
            }
        }

        /*
         * Function: Move To Map
         * Description: Tìm đến vị trí trên bản đồ nhỏ và click vào
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-16 - Updated At: 2019-11-16
         * Flows:
         *  1. Mở bản đồ nhỏ
         *  2. Nhấn vào vị trí trên bản đồ
         */
        public bool moveToNPC(string npc, string locationName)
        {
            if (mCharacter.Running == 0)
            {
                return false;
            }

            int loop = 1;
            do
            {
                closeAllDialog();

                // Mở bản đồ nhỏ
                clickToImage(Constant.ImagePathMiniMap);

                // Click vào vị trí cần đến
                clickImageByGroup("in_map", locationName);

                // Check còn đang di chuyển không ?
                while (isMoving())
                {
                    Thread.Sleep(2000);
                }
                loop++;
            } while (!findNPC(npc) && loop <= Constant.MaxLoop && mCharacter.Running == 1);

            if (loop >= Constant.MaxLoop)
            {
                writeStatus("Không thể di chuyển đến NPC " + npc);
                return false;
            }

            return true;
        }

        /*
         * Function: findNPC
         * Description: Tìm NPC
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-16 - Updated At: 2019-11-16
         */
        public bool findNPC(string npc)
        {
            if (mCharacter.Running == 0)
            {
                return false;
            }

            string npcViTriTenImagePath1 = Constant.ImagePathViTriNPC + "ten" + npc + "1.png";
            string npcViTriTenImagePath2 = Constant.ImagePathViTriNPC + "ten" + npc + "2.png";
            string npcViTriImagePath1 = Constant.ImagePathViTriNPC + npc + "1.png";
            string npcViTriImagePath2 = Constant.ImagePathViTriNPC + npc + "2.png";

            closeAllDialog();

            if (findImage(npcViTriImagePath1)
                || findImage(npcViTriImagePath2)
                || findImage(npcViTriTenImagePath1)
                || findImage(npcViTriTenImagePath2)) 
            { 
                return true;
            }

            return false;
        }

        /*
         * Function: Move To Map
         * Description: Tìm đến vị trí của map trên bản đồ lớn và click vào
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-10 - Updated At: 2019-11-10
         * Flows:
         *  1. Mở bản đồ nhỏ
         *  2. Mở bản đồ thế giới
         *  3. Nhấn vào vị trí của map
         */
        public bool moveToMapNhom(string mapName, int worldMapIndex = 1, int x = 0, int y = -20)
        {
            if (mCharacter.Running == 0)
            {
                return false;
            }

            string mapPath = Constant.ImagePathMapsFolder + mapName + ".png";
            string mapActivePath = Constant.ImagePathMapsFolder + mapName + "_active.png";
            string mapCheckPath = Constant.ImagePathMapsFolder + mapName + "_check.png";
            int loop = 1;

            while (!findImage(mapCheckPath) && loop <= Constant.MaxLoop && mCharacter.Running != 0)
            {
                closeAllDialog();

                // Mở bản đồ nhỏ
                clickToImage(Constant.ImagePathMiniMap);

                // Mở bản đồ thể giới
                clickToImage(Constant.ImagePathWorldMap);

                // Nếu worldMapIndex == 2 thì mở sang bản đồ 2
                if (worldMapIndex == 2)
                {
                    clickToImage(Constant.ImagePathSecondWorldMap);
                }

                clickToImage(mapPath, x, y);
                clickToImage(mapActivePath, x, y);
                clickImageByGroup("global", "movenhom");
                loop++;
                Thread.Sleep(3000);

                closeAllDialog();
            }

            if (loop >= Constant.MaxLoop)
            {
                writeStatus("Không thể di chuyển đến " + mapName);
                return false;
            }

            return true;

        }

        /*
         * Function: Move To Map
         * Description: Tìm đến vị trí của map trên bản đồ lớn và click vào
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-10 - Updated At: 2019-11-10
         * Flows:
         *  1. Mở bản đồ nhỏ
         *  2. Mở bản đồ thế giới
         *  3. Nhấn vào vị trí của map
         */
        public bool moveToMap(string mapName, int worldMapIndex = 1, int x = 0, int y = -20)
        {
            if (mCharacter.Running == 0)
            {
                return false;
            }

            string mapPath = Constant.ImagePathMapsFolder + mapName + ".png";
            string mapActivePath = Constant.ImagePathMapsFolder + mapName + "_active.png";
            string mapCheckPath = Constant.ImagePathMapsFolder + mapName + "_check.png";
            int loop = 1;

            while(!findImage(mapCheckPath) && loop <= Constant.MaxLoop && mCharacter.Running != 0)
            {
                closeAllDialog();

                // Mở bản đồ nhỏ
                clickToImage(Constant.ImagePathMiniMap);

                // Mở bản đồ thể giới
                clickToImage(Constant.ImagePathWorldMap);

                // Nếu worldMapIndex == 2 thì mở sang bản đồ 2
                if (worldMapIndex == 2)
                {
                    clickToImage(Constant.ImagePathSecondWorldMap);
                }

                clickToImage(mapPath, x, y);
                clickToImage(mapActivePath, x, y);
                loop++;
                Thread.Sleep(3000);

                closeAllDialog();
            }

            if (loop >= Constant.MaxLoop)
            {
                writeStatus("Không thể di chuyển đến " + mapName);
                return false;
            }

            return true;
            
        }

        /*
         * Function: Move mouse
         * Description: Find position of image on window and move mouse to it
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-09 - Updated At: 2019-11-09
         */
        public bool moveToImage(string imagePath, int xRange = 0, int yRange = -20)
        {
            if (mCharacter.Running == 0)
            {
                return false;
            }

            var screen = CaptureHelper.CaptureWindow(mHWnd);
            Bitmap iBtn = ImageScanOpenCV.GetImage(imagePath);
            var pBtn = ImageScanOpenCV.FindOutPoint((Bitmap)screen, iBtn);
            if (pBtn != null)
            {
                au3.move(mWindowName, pBtn.Value.X + xRange, pBtn.Value.Y + yRange);
                return true;
            }
            return false;
        }

        /*
         * Function: Click to Image
         * Description: Find position of image on window and click it
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-09 - Updated At: 2019-11-09
         */
        public bool clickRightToImage(string imagePath, int xRange = 0, int yRange = -20)
        {
            if (mCharacter.Running == 0)
            {
                return false;
            }

            var screen = CaptureHelper.CaptureWindow(mHWnd);
            Bitmap iBtn = ImageScanOpenCV.GetImage(imagePath);
            var pBtn = ImageScanOpenCV.FindOutPoint((Bitmap)screen, iBtn);
            if (pBtn != null)
            {
                au3.clickRight(mWindowName, 1, pBtn.Value.X + xRange, pBtn.Value.Y + yRange);
                Thread.Sleep(Constant.TimeShort);
                return true;
            }
            return false;
        }

        /*
         * Function: Click to Window
         * Description: Find position of image on window and click it
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-09 - Updated At: 2019-11-09
         */
        public void clickToWindow(int xRange = 0, int yRange = -20, int numClick = 1, int wait = Constant.TimeShort)
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            au3.click(mWindowName, numClick, xRange, yRange);
            Thread.Sleep(wait);
        }

        /*
         * Function: Click to Point
         * Description: Click on the point
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-09 - Updated At: 2019-11-09
         */
        public void clickPoint(int x = 0, int y = 0, int numClick = 1, int wait = Constant.TimeShort)
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            au3.click(mWindowName, numClick, x, y);
            Thread.Sleep(wait);
        }

        /*
         * Function: Click to Image
         * Description: Find position of image on window and click it
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-09 - Updated At: 2019-11-09
         */
        public bool clickImage(Bitmap image, int xRange = 0, int yRange = -20, int numClick = 1, int wait = Constant.TimeShort)
        {
            if (mCharacter.Running == 0)
            {
                return false;
            }

            var screen = CaptureHelper.CaptureWindow(mHWnd);
            var pBtn = ImageScanOpenCV.FindOutPoint((Bitmap)screen, image);
            if (pBtn != null)
            {
                au3.click(mWindowName, numClick, pBtn.Value.X + xRange, pBtn.Value.Y + yRange);
                Thread.Sleep(wait);
                return true;
            }
            return false;
        }

        /*
         * Function: Click to Image
         * Description: Find position of image on window and click it
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-09 - Updated At: 2019-11-09
         */
        public bool clickToImage(string imagePath, int xRange = 0, int yRange = -20, int numClick = 1, int wait = Constant.TimeShort)
        {
            if (mCharacter.Running == 0)
            {
                return false;
            }

            var screen = CaptureHelper.CaptureWindow(mHWnd);
            Bitmap iBtn = ImageScanOpenCV.GetImage(imagePath);
            var pBtn = ImageScanOpenCV.FindOutPoint((Bitmap)screen, iBtn);
            if (pBtn != null)
            {
                au3.click(mWindowName, numClick, pBtn.Value.X + xRange, pBtn.Value.Y + yRange);
                Thread.Sleep(wait);
                return true;
            }
            return false;
        }

        /*
         * Function: Find Image
         * Description: Find position of image on window
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-09 - Updated At: 2019-11-09
         */
        public bool findImage(string imagePath)
        {
            if (mCharacter.Running == 0)
            {
                return false;
            }

            var screen = CaptureHelper.CaptureWindow(mHWnd);
            Bitmap iBtn = ImageScanOpenCV.GetImage(imagePath);
            var pBtn = ImageScanOpenCV.FindOutPoint((Bitmap)screen, iBtn);
            if (pBtn != null)
            {
                return true;
            }

            return false;
        }

        public void login(IntPtr hWnd, string windowName)
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            // Click to "Bắt đầu" button
            this.clickToImage(Constant.ImagePathStartButton);
        }

        public void writeStatus(string statusText)
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            mTextBoxStatus.BeginInvoke(new Action(() => mTextBoxStatus.AppendText(mCharacter.ID + ": " + statusText + Environment.NewLine)));
            //mTextBoxStatus.AppendText(statusText + Environment.NewLine);
        }

        public void increaseFPS(int numberIncrease)
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            writeStatus("Tăng FPS ...");
            clickToImage(Constant.ImagePathGlobalFPS, 0, -20, numberIncrease);
        }

        public void dongMenuPhai()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            clickImageByGroup("global", "dongmenuphai");
        }

        public void batAuto()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            clickImageByGroup("global", "moauto");
        }

        public void moMenuPhai()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            clickImageByGroup("global", "momenuphai");
        }

        public bool findImageByGroup(string group, string name, bool active = false, bool hover = false)
        {
            if (mCharacter.Running == 0)
            {
                return false;
            }

            string groupPath = Constant.ImagePathGlobalFolder;
            switch (group)
            {
                case "maps":
                    groupPath = Constant.ImagePathMapsFolder;
                    break;
                case "phu_ban":
                    groupPath = Constant.ImagePathPhuBanFolder;
                    break;
                case "mat_bao":
                    groupPath = Constant.ImagePathMatBaoFolder;
                    break;
                case "char_name":
                    groupPath = Constant.ImagePathCharNameFolder;
                    break;
                case "tri_an":
                    groupPath = Constant.ImagePathTriAnFolder;
                    break;
                case "in_map":
                    groupPath = Constant.ImagePathInMapFolder;
                    break;
                case "global":
                default:
                    groupPath = Constant.ImagePathGlobalFolder;
                    break;
            }
            if (findImage(groupPath + name + ".png"))
            {
                return true;
            }

            if (active)
            {
                if (findImage(groupPath + name + "_active.png"))
                {
                    return true;
                }
            }

            if (hover)
            {
                if (findImage(groupPath + name + "_hover.png"))
                {
                    return true;
                }
            }

            return false;
        }

        public void clickRightImageByGroup(string group, string name, bool active = false, bool hover = false, int numClick = 1, int x = 0, int y = -20)
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            string groupPath = Constant.ImagePathGlobalFolder;
            switch (group)
            {
                case "maps":
                    groupPath = Constant.ImagePathMapsFolder;
                    break;
                case "phu_ban":
                    groupPath = Constant.ImagePathPhuBanFolder;
                    break;
                case "mat_bao":
                    groupPath = Constant.ImagePathMatBaoFolder;
                    break;
                case "char_name":
                    groupPath = Constant.ImagePathCharNameFolder;
                    break;
                case "tri_an":
                    groupPath = Constant.ImagePathTriAnFolder;
                    break;
                case "in_map":
                    groupPath = Constant.ImagePathInMapFolder;
                    break;
                case "global":
                default:
                    groupPath = Constant.ImagePathGlobalFolder;
                    break;
            }
            clickRightToImage(groupPath + name + ".png", x, y);
            if (active)
            {
                clickRightToImage(groupPath + name + "_active.png", x, y);
            }
            if (hover)
            {
                clickRightToImage(groupPath + name + "_hover.png", x, y);
            }
        }

        public void clickImageByGroup(string group, string name, bool active = false, bool hover = false, int numClick = 1, int x = 0, int y = -20)
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            string groupPath = Constant.ImagePathGlobalFolder;
            switch (group)
            {
                case "maps":
                    groupPath = Constant.ImagePathMapsFolder;
                    break;
                case "phu_ban":
                    groupPath = Constant.ImagePathPhuBanFolder;
                    break;
                case "mat_bao":
                    groupPath = Constant.ImagePathMatBaoFolder;
                    break;
                case "char_name":
                    groupPath = Constant.ImagePathCharNameFolder;
                    break;
                case "tri_an":
                    groupPath = Constant.ImagePathTriAnFolder;
                    break;
                case "in_map":
                    groupPath = Constant.ImagePathInMapFolder;
                    break;
                case "global":
                default:
                    groupPath = Constant.ImagePathGlobalFolder;
                    break;
            }
            clickToImage(groupPath + name + ".png", x, y, numClick);
            if (active)
            {
                clickToImage(groupPath + name + "_active.png", x, y, numClick);
            }
            if (hover)
            {
                clickToImage(groupPath + name + "_hover.png", x, y, numClick);
            }
        }

        public void bay()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            clickToImage(Constant.ImagePathGlobalBay);
        }

        public void bayXuong()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            clickToImage(Constant.ImagePathGlobalXuong);
        }

        public void traNhiemVu()
        {
            if (mCharacter.Running == 0)
            {
                return;
            }

            clickImageByGroup("global", "xong", false, true);
        }

        /*
         * Function: talkToNPC
         * Description: Kiểm tra nhân vật có đang nói chuyện với NPC
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-10 - Updated At: 2019-11-10
         */
        public bool talkToNPC(string npc, int loopTime = 0, int x = 0, int y = -20)
        {
            if (mCharacter.Running == 0)
            {
                return false;
            }

            loopTime++;
            closeAllDialog();

            string npcViTriTenImagePath1 = Constant.ImagePathViTriNPC + "ten" + npc + "1.png";
            string npcViTriTenImagePath2 = Constant.ImagePathViTriNPC + "ten" + npc + "2.png";
            string npcViTriImagePath1 = Constant.ImagePathViTriNPC + npc + "1.png";
            string npcViTriImagePath2 = Constant.ImagePathViTriNPC + npc + "2.png";

            // Click vào NPC
            writeStatus("Click vào vị trí NPC ...");
            clickToImage(npcViTriImagePath1, x, y);
            clickToImage(npcViTriImagePath2, x, y);

            if (!isTalkWithNPC(npc) && loopTime < Constant.MaxLoop && mCharacter.Running == 1)
            {
                // Click vào vị trí khác bên cạnh NPC
                writeStatus("Click vào vị trí khác bên cạnh NPC ...");
                clickToImage(npcViTriTenImagePath1, random.Next(-100, 100), random.Next(-100, 100));
                clickToImage(npcViTriTenImagePath2, random.Next(-100, 100), random.Next(-100, 100));

                talkToNPC(npc, loopTime, x, y);
            }

            if(loopTime >= Constant.MaxLoop)
            {
                writeStatus("Không nói chuyện được với NPC ...");
                return false;
            }

            writeStatus("Đang nói chuyện được với NPC ...");
            return true;
        }

        /*
         * Function: isTalkWithNPC
         * Description: Kiểm tra nhân vật có đang nói chuyện với NPC
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-10 - Updated At: 2019-11-10
         */
        public bool isTalkWithNPC(string npc)
        {
            if (mCharacter.Running == 0)
            {
                return false;
            }

            string npcDoiThoaiImagePath = Constant.ImagePathDoiThoai + npc + ".png";
            if (findImage(npcDoiThoaiImagePath))
            {
                writeStatus("Nhân vật đang đối thoại với " + npc + " ...");
                return true;
            }
            writeStatus("Nhân vật đang không đối thoại với " + npc + " ...");
            return false;
        }

        /*
         * Function: isMoving
         * Description: Kiểm tra nhân vật đang di chuyển
         * Author: Tử La Lan - Facebook: https://www.facebook.com/Tu.La.Lan.NT
         * Created At: 2019-11-10 - Updated At: 2019-11-10
         */
        public bool isMoving()
        {
            if (mCharacter.Running == 0)
            {
                return false;
            }

            int i = 0;
            bool moving = true;
            while (moving && i < Constant.MaxLoop && mCharacter.Running != 0)
            {
                // Chụp màn hình
                var screen_first = CaptureHelper.CaptureWindow(mHWnd);
                screen_first = CaptureHelper.CropImage((Bitmap)screen_first, new Rectangle(180, 0, 250, 250));

                // Chờ 3s
                Thread.Sleep(1500);
                var screen_second = CaptureHelper.CaptureWindow(mHWnd);

                // Kiểm tra hình trước có trong hình sau hay không
                var p = ImageScanOpenCV.FindOutPoint((Bitmap)screen_second, (Bitmap)screen_first);
                if (p != null)
                {
                    moving = false;
                    return moving;
                }
            }

            return moving;
        }

        public bool dangTrongTranDau()
        {
            if (mCharacter.Running == 0)
            {
                return false;
            }

            if (!findImage(Constant.ImagePathKhongTrongTranDau))
            {
                //writeStatus("Nhân vật đang trong trận đấu ...");
                return true;
            }
            //writeStatus("Nhân vật đang không trong trận đấu ...");
            return false;
        }
    }
}
