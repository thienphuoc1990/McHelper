using System;
using System.Threading;
using System.Collections.Generic;
using System.Drawing;
using AutoVPT.Objects;

namespace AutoVPT.Libs
{
    class AutoTruMa
    {
        public IntPtr mHWnd;
        public string mWindowName;
        public AutoFeatures mAuto;
        List<Monster> mMonstersCuma;
        List<Monster> mMonstersCuthu;
        List<Monster> mMonstersPhima;

        public AutoTruMa(IntPtr hWnd, string windowName, AutoFeatures auto)
        {
            mHWnd = hWnd;
            mWindowName = windowName;
            mAuto = auto;
            mMonstersCuma = initListMonsters("cuma");
            mMonstersCuthu = initListMonsters("cuthu");
            mMonstersPhima = initListMonsters("phima");
        }

        /*
         * Function: auto
         * Description: Start the "Trừ Ma" flow.
         * Current steps:
         *  1. Ensure dialog state is clean
         *  2. Open quest by NVHN ("truma") until NPC 'quanquannhu' is reachable
         *  3. Talk with NPC 'quanquannhu'
         *
         * Additional steps can be added later.
         */
        public bool auto()
        {
            if (mAuto == null)
            {
                return false;
            }

            mAuto.writeStatus("Bắt đầu \"Trừ Ma\"");
            string quai = "phima";
            int loop = 0;
            
            do
            {
                if (!isDangCoNhiemVu())
                {
                    nhanTraNhiemVu();
                }

                if (!isHoanThanhNV())
                {
                    mAuto.writeStatus("Chưa đánh quái");
                    quai = xacDinhNhiemVu();
                    diChuyenDenQuai(quai);
                    findMonstersOnMap(getMonstersByQuai(quai), quai);
                    diChuyenDenNhanTraQ(quai);
                }

                nhanTraNhiemVu(true);
                loop++;
            } while (isDangCoNhiemVu() && loop < Constant.MaxLoop) ;

            mAuto.writeStatus("Xong \"Trừ Ma\"");
            return true;
        }

        private List<Monster> getMonstersByQuai(string quai)
        {
            if (quai == "cuma")
            {
                return mMonstersCuma;
            }
            else if (quai == "cuthu")
            {
                return mMonstersCuthu;
            }
            else
            {
                return mMonstersPhima;
            }
        }

        private void diChuyenDenNhanTraQ(string quai)
        {
            string npc = "tinhlinhchuyendich";
            string location = "dichchuyentinhlinhthanh";
            string dichuyen = "tinhlinhthanhchuyendich_dht";
            string map_check = "donghuyenthanh_check";
            if (quai != "phima")
            {
                npc = "tramthuylamchuyendich";
                location = "dichchuyentramthuylam";
                dichuyen = "tramthuylamchuyendich_dht";
            }

            while (!mAuto.findImageByGroup("maps", map_check))
            {
                mAuto.closeAllDialog();
                mAuto.moveToNPC(npc, location);
                mAuto.talkToNPC(npc);
                mAuto.writeStatus("Đã nói chuyện dc với NPC dịch chuyển");
                mAuto.clickImageByGroup("doi_thoai", dichuyen);
                Thread.Sleep(Constant.TimeMedium);
            }
            mAuto.writeStatus("Đã di chuyển tới map");
        }

        private void diChuyenDenQuai(string quai)
        {
            string npc = "dhtmapchuyendich";
            string location = "dichchuyendht";
            string dichuyen = "dhtmapchuyendich_tramthuylam";
            string map_check = "tramthuylam_check";
            if (quai == "phima")
            {
                dichuyen = "dhtmapchuyendich_tinhlinhthanh";
                map_check = "tinhlinhthanh_check";
            }
            
            while (!mAuto.findImageByGroup("maps", map_check))
            {
                mAuto.closeAllDialog();
                int loop = 0;
                while (!mAuto.isTalkWithNPC("quanquannhudht") && loop < Constant.MaxLoop)
                {
                    mAuto.writeStatus("Mở NVHN truma để tìm NPC quanquannhu ...");
                    mAuto.openQuestByNVHN("truma");
                    Thread.Sleep(Constant.TimeShort);
                    loop++;
                }

                mAuto.writeStatus("Di chuyển đến " + quai);
                while (!mAuto.talkToNPC(npc))
                {
                    mAuto.moveToNPC(npc, location);
                }
                mAuto.writeStatus("Đã nói chuyện dc với NPC dịch chuyển");
                mAuto.clickImageByGroup("doi_thoai", dichuyen);
                Thread.Sleep(Constant.TimeMedium);
            }
            mAuto.writeStatus("Đã di chuyển tới map");
        }

        private bool isDangCoNhiemVu()
        {
            mAuto.closeAllDialog();
            while (!mAuto.findImageByGroup("global", "nhiemvu_check"))
            {
                mAuto.clickImageByGroup("global", "nhiemvu");
            }
            while (!mAuto.findImageByGroup("tru_ma", "bangnhiemvu_nvvongopened", true) && mAuto.findImageByGroup("tru_ma", "bangnhiemvu_nvvong", true))
            {
                mAuto.clickImageByGroup("tru_ma", "bangnhiemvu_nvvong", true);
            }

            return mAuto.findImageByGroup("tru_ma", "nhiemvutruma", true, true);
        }

        private bool isHoanThanhNV()
        {
            while (!mAuto.findImageByGroup("tru_ma", "nhiemvutruma", true, true))
            {
                isDangCoNhiemVu();
            }
            return mAuto.findImageByGroup("tru_ma", "bangnhiemvutrumadaxong", true, true);
        }

        private string xacDinhNhiemVu()
        {
            mAuto.closeAllDialog();
            // Mở nhiệm vụ hàng ngày    
            while (!mAuto.findImageByGroup("nvhn", "bang_check"))
            {
                mAuto.writeStatus("Mở bảng nhiệm vụ hàng ngày");
                mAuto.clickImageByGroup("nvhn", "bang");
                Thread.Sleep(Constant.TimeShort);
            }
            while (!mAuto.findImageByGroup("global", "nhiemvu_check"))
            {
                mAuto.clickImageByGroup("global", "nhiemvu");
            }
            while (!mAuto.findImageByGroup("tru_ma", "bangnhiemvu_nvvongopened", true))
            {
                mAuto.clickImageByGroup("tru_ma", "bangnhiemvu_nvvong", true);
            }
            mAuto.clickImageByGroup("tru_ma", "nhiemvutruma", true, true);

            if (mAuto.findImageByGroup("tru_ma", "nhiemvucuma", false, false))
            {
                mAuto.writeStatus("Nhiệm vụ cự ma");
                return "cuma";
            }
            else if (mAuto.findImageByGroup("tru_ma", "nhiemvuphima", false, false))
            {
                mAuto.writeStatus("Nhiệm vụ phi ma");
                return "phima";
            }
            else
            {
                mAuto.writeStatus("Nhiệm vụ cự thú");
                return "cuthu";
            }
        }

        private void nhanTraNhiemVu(bool traNv = false)
        {
            // Try to bring up the quest/interaction until the NPC is available
            int loop = 0;
            while (!mAuto.isTalkWithNPC("quanquannhudht") && loop < Constant.MaxLoop)
            {
                mAuto.writeStatus("Mở NVHN truma để tìm NPC quanquannhu ...");
                mAuto.openQuestByNVHN("truma");
                Thread.Sleep(Constant.TimeShort);
                loop++;
            }

            mAuto.writeStatus("Đã nói chuyện với quanquannhu");
            mAuto.clickImageByGroup("doi_thoai", "quanquannhudht_ndvapt");
            if (!traNv)
            {
                mAuto.clickImageByGroup("doi_thoai", "quanquannhudht_truma");
            }
            else
            {
                mAuto.clickImageByGroup("doi_thoai", "quanquannhudht_truma_phima", false, true);
                mAuto.clickImageByGroup("doi_thoai", "quanquannhudht_truma_phongan", false, true);
                mAuto.clickImageByGroup("global", "xong", false, true);
            }
        }

        // New helper: call AutoFeatures helpers to search monsters on map
        private void findMonstersOnMap(List<Monster> monsters, string monsterName)
        {
            if ( monsters == null || monsters.Count == 0)
            {
                mAuto.writeStatus("No monsters provided to findMonstersOnMap.");
                return;
            }

            var mapPoints = mAuto.collectMapMiniPoints();
            if (mapPoints == null || mapPoints.Count == 0)
            {
                mAuto.writeStatus("No map points found, aborting monster search.");
                return;
            }

            mAuto.moveAndFindMonsters(mapPoints, monsters, monsterName);
        }
        private List<Monster> initListMonsters(string monster_name)
        {
            List<Monster> monsters = new List<Monster>();
            int number = 2;
            if (monster_name == "cuma")
            {
                number = 8;
            }
            if (monster_name == "cuthu")
            {
                number = 4;
            }

            for (int i = 0; i < number; i++)
            {
                monsters.Add(new Monster(Constant.ImagePathTruMaFolder + monster_name + (i + 1).ToString() + ".png", 0, -20));
            }

            return monsters;
        }
    }
}