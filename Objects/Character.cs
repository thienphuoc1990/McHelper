using AutoVPT.DML;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoVPT.Objects
{
    public class Character
    {
        private string id;
        private string link;
        private int vip_level;
        private string increase_fps;
        private string date;
        private int vip_promotion;
        private int doi_nang_no;
        private int doi_nang_no_nl4;
        private string doi_nang_no_loai = "";
        private int trong_nl;
        private string trong_nl_loai = "";
        private int tri_an;
        private int lat_the_bai;
        private int rut_bo;
        private int doi_kgdk;
        private int tu_hanh;
        private int tru_ma;
        private int ao_ma_thap;
        private int trong_cay;
        private int che_mat_bao;
        private string che_mat_bao_loai = "";
        private int che_mat_bao_cap;
        private int auto_phu_ban;
        private string auto_phu_ban_danh_sach = "";
        private int uoc_nguyen;
        private int dau_pet;
        private int nhan_thuong_hlvt;
        private int bug_online;
        private int me_tran;
        private int hai_thuoc;
        private int cau_ca;
        private int vi_tri_nhan_vat;
        private int running;

        public Character()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public string ID
        {
            get { return id; }
            set { id = value; }
        }

        public string Link
        {
            get { return link; }
            set { link = value; }
        }

        public int VipLevel
        {
            get { return vip_level; }
            set { vip_level = value; }
        }

        public string IncreaseFPS
        {
            get { return increase_fps; }
            set { increase_fps = value; }
        }

        public string Date
        {
            get { return date; }
            set { date = value; }
        }

        public int VipPromotion
        {
            get { return vip_promotion; }
            set { vip_promotion = value; }
        }

        public int DoiNangNo
        {
            get { return doi_nang_no; }
            set { doi_nang_no = value; }
        }

        public int DoiNangNoNL4
        {
            get { return doi_nang_no_nl4; }
            set { doi_nang_no_nl4 = value; }
        }

        public string DoiNangNoLoai
        {
            get { return doi_nang_no_loai; }
            set { doi_nang_no_loai = value; }
        }

        public int TrongNL
        {
            get { return trong_nl; }
            set { trong_nl = value; }
        }

        public string TrongNLLoai
        {
            get { return trong_nl_loai; }
            set { trong_nl_loai = value; }
        }

        public int TriAn
        {
            get { return tri_an; }
            set { tri_an = value; }
        }

        public int LatTheBai
        {
            get { return lat_the_bai; }
            set { lat_the_bai = value; }
        }

        public int RutBo
        {
            get { return rut_bo; }
            set { rut_bo = value; }
        }

        public int DoiKGDK
        {
            get { return doi_kgdk; }
            set { doi_kgdk = value; }
        }

        public int TuHanh
        {
            get { return tu_hanh; }
            set { tu_hanh = value; }
        }

        public int TruMa
        {
            get { return tru_ma; }
            set { tru_ma = value; }
        }

        public int AoMaThap
        {
            get { return ao_ma_thap; }
            set { ao_ma_thap = value; }
        }

        public int TrongCay
        {
            get { return trong_cay; }
            set { trong_cay = value; }
        }

        public int CheMatBao
        {
            get { return che_mat_bao; }
            set { che_mat_bao = value; }
        }

        public string CheMatBaoLoai
        {
            get { return che_mat_bao_loai; }
            set { che_mat_bao_loai = value; }
        }

        public int CheMatBaoCap
        {
            get { return che_mat_bao_cap; }
            set { che_mat_bao_cap = value; }
        }

        public int AutoPhuBan
        {
            get { return auto_phu_ban; }
            set { auto_phu_ban = value; }
        }

        public string AutoPhuBanDanhSach
        {
            get { return auto_phu_ban_danh_sach; }
            set { auto_phu_ban_danh_sach = value; }
        }

        public int UocNguyen
        {
            get { return uoc_nguyen; }
            set { uoc_nguyen = value; }
        }

        public int DauPet
        {
            get { return dau_pet; }
            set { dau_pet = value; }
        }

        public int NhanThuongHLVT
        {
            get { return nhan_thuong_hlvt; }
            set { nhan_thuong_hlvt = value; }
        }

        public int BugOnline
        {
            get { return bug_online; }
            set { bug_online = value; }
        }

        public int MeTran
        {
            get { return me_tran; }
            set { me_tran = value; }
        }

        public int HaiThuoc
        {
            get { return hai_thuoc; }
            set { hai_thuoc = value; }
        }

        public int CauCa
        {
            get { return cau_ca; }
            set { cau_ca = value; }
        }

        public int ViTriNhanVat
        {
            get { return vi_tri_nhan_vat; }
            set { vi_tri_nhan_vat = value; }
        }

        public int Running
        {
            get { return running; }
            set { running = value; }
        }
    }

    public class CharacterList
    {
        public static Character GetCharacter(string id)
        {
            DataRow iDr = null;
            iDr = XMLCharacter.Select(id);
            Character character = null;
            if (iDr != null)
            {
                character = new Character();
                character.ID = iDr[0] != DBNull.Value ? iDr[0].ToString() : string.Empty;
                character.Link = iDr[1] != DBNull.Value ? iDr[1].ToString() : string.Empty;
                character.Date = iDr[2] != DBNull.Value ? iDr[2].ToString() : string.Empty;
                character.VipLevel = iDr[3] != DBNull.Value ? int.Parse(iDr[3].ToString()) : 0;
                character.IncreaseFPS = iDr[4] != DBNull.Value ? iDr[4].ToString() : string.Empty;
                character.VipPromotion = iDr[5] != DBNull.Value ? int.Parse(iDr[5].ToString()) : 0;
                character.DoiNangNo = iDr[6] != DBNull.Value ? int.Parse(iDr[6].ToString()) : 0;
                character.DoiNangNoNL4 = iDr[7] != DBNull.Value ? int.Parse(iDr[7].ToString()) : 0;
                character.DoiNangNoLoai = iDr[8] != DBNull.Value ? iDr[8].ToString() : string.Empty;
                character.TrongNL = iDr[9] != DBNull.Value ? int.Parse(iDr[9].ToString()) : 0;
                character.TrongNLLoai = iDr[10] != DBNull.Value ? iDr[10].ToString() : string.Empty;
                character.TriAn = iDr[11] != DBNull.Value ? int.Parse(iDr[11].ToString()) : 0;
                character.LatTheBai = iDr[12] != DBNull.Value ? int.Parse(iDr[12].ToString()) : 0;
                character.RutBo = iDr[13] != DBNull.Value ? int.Parse(iDr[13].ToString()) : 0;
                character.DoiKGDK = iDr[14] != DBNull.Value ? int.Parse(iDr[14].ToString()) : 0;
                character.TuHanh = iDr[15] != DBNull.Value ? int.Parse(iDr[15].ToString()) : 0;
                character.TruMa = iDr[16] != DBNull.Value ? int.Parse(iDr[16].ToString()) : 0;
                character.AoMaThap = iDr[17] != DBNull.Value ? int.Parse(iDr[17].ToString()) : 0;
                character.TrongCay = iDr[18] != DBNull.Value ? int.Parse(iDr[18].ToString()) : 0;
                character.CheMatBao = iDr[19] != DBNull.Value ? int.Parse(iDr[19].ToString()) : 0;
                character.CheMatBaoLoai = iDr[20] != DBNull.Value ? iDr[20].ToString() : string.Empty;
                character.CheMatBaoCap = iDr[21] != DBNull.Value ? int.Parse(iDr[21].ToString()) : 0;
                character.AutoPhuBan = iDr[22] != DBNull.Value ? int.Parse(iDr[22].ToString()) : 0;
                character.AutoPhuBanDanhSach = iDr[23] != DBNull.Value ? iDr[23].ToString() : string.Empty;
                character.UocNguyen = iDr[24] != DBNull.Value ? int.Parse(iDr[24].ToString()) : 0;
                character.DauPet = iDr[25] != DBNull.Value ? int.Parse(iDr[25].ToString()) : 0;
                character.NhanThuongHLVT = iDr[26] != DBNull.Value ? int.Parse(iDr[26].ToString()) : 0;
                character.BugOnline = iDr[27] != DBNull.Value ? int.Parse(iDr[27].ToString()) : 0;
                character.MeTran = iDr[28] != DBNull.Value ? int.Parse(iDr[28].ToString()) : 0;
                character.HaiThuoc = iDr[29] != DBNull.Value ? int.Parse(iDr[29].ToString()) : 0;
                character.CauCa = iDr[30] != DBNull.Value ? int.Parse(iDr[30].ToString()) : 0;
                character.ViTriNhanVat = iDr[31] != DBNull.Value ? int.Parse(iDr[31].ToString()) : 0;
                character.Running = iDr[32] != DBNull.Value ? int.Parse(iDr[32].ToString()) : 0;
            }
            return character;
        }
        public static IList GetCharacterList()
        {

            return XMLCharacter.SelectAll();

        }

        public static void UpdateCharacter(Character character)
        {
            XMLCharacter.Update(character.ID, character.Link);
        }

        public static void InsertCharacter(Character character)
        {
            XMLCharacter.Insert(character.ID, character.Link);
        }

        public static void DeleteCharacter(string characteregoryID)
        {
            XMLCharacter.Delete(characteregoryID);
        }

        public static void UpdateCharacterAllFields(Character character)
        {
            XMLCharacter.UpdateAllFields(character);
        }
    }
}
