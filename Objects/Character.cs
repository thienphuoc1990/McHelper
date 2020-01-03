using AutoVPT.DML;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;
using static System.Net.Mime.MediaTypeNames;

namespace AutoVPT.Objects
{
    public class Character
    {
        private string id;
        private string link;
        private int vip_level = 0;
        private int increase_fps = 0;
        private string date = "";
        private int vip_promotion = 0;
        private int doi_nang_no = 0;
        private int doi_nang_no_nl4 = 0;
        private string doi_nang_no_loai = "";
        private int trong_nl = 0;
        private string trong_nl_loai = "";
        private int tri_an = 0;
        private int lat_the_bai = 0;
        private int rut_bo = 0;
        private int doi_kgdk = 0;
        private int tu_hanh = 0;
        private int tru_ma = 0;
        private int ao_ma_thap = 0;
        private int trong_cay = 0;
        private int che_mat_bao = 0;
        private string che_mat_bao_loai = "";
        private int che_mat_bao_cap = 0;
        private int auto_phu_ban = 0;
        private string auto_phu_ban_danh_sach = "";
        private int uoc_nguyen = 0;
        private int dau_pet = 0;
        private int nhan_thuong_hlvt = 0;
        private int bug_online = 0;
        private int me_tran = 0;
        private int hai_thuoc = 0;
        private int cau_ca = 0;
        private int vi_tri_nhan_vat = 1;
        private int running = 0;
        private int auto_than_tu = 0;
        private int run_to_last = 0;

        private int status_vip_promotion = 0;
        private int status_doi_nang_no = 0;
        private int status_tri_an = 0;
        private int status_lat_the_bai = 0;
        private int status_rut_bo = 0;
        private int status_doi_kgdk = 0;
        private int status_tu_hanh = 0;
        private int status_tru_ma = 0;
        private int status_ao_ma_thap = 0;
        private int status_trong_cay = 0;
        private int status_che_mat_bao = 0;
        private int status_auto_phu_ban = 0;
        private int status_uoc_nguyen = 0;
        private int status_nhan_thuong_hlvt = 0;
        private int status_me_tran = 0;
        private int status_hai_thuoc = 0;
        private int status_cau_ca = 0;
        private int status_auto_than_tu = 0;

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

        public int IncreaseFPS
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

        public int AutoThanTu
        {
            get { return auto_than_tu; }
            set { auto_than_tu = value; }
        }

        public int RunToLast
        {
            get { return run_to_last; }
            set { run_to_last = value; }
        }

        public int StatusVipPromotion
        {
            get { return status_vip_promotion; }
            set { status_vip_promotion = value; }
        }

        public int StatusDoiNangNo
        {
            get { return status_doi_nang_no; }
            set { status_doi_nang_no = value; }
        }

        public int StatusTriAn
        {
            get { return status_tri_an; }
            set { status_tri_an = value; }
        }

        public int StatusLatTheBai
        {
            get { return status_lat_the_bai; }
            set { status_lat_the_bai = value; }
        }

        public int StatusRutBo
        {
            get { return status_rut_bo; }
            set { status_rut_bo = value; }
        }

        public int StatusDoiKGDK
        {
            get { return status_doi_kgdk; }
            set { status_doi_kgdk = value; }
        }

        public int StatusTuHanh
        {
            get { return status_tu_hanh; }
            set { status_tu_hanh = value; }
        }

        public int StatusTruMa
        {
            get { return status_tru_ma; }
            set { status_tru_ma = value; }
        }

        public int StatusAoMaThap
        {
            get { return status_ao_ma_thap; }
            set { status_ao_ma_thap = value; }
        }

        public int StatusTrongCay
        {
            get { return status_trong_cay; }
            set { status_trong_cay = value; }
        }

        public int StatusCheMatBao
        {
            get { return status_che_mat_bao; }
            set { status_che_mat_bao = value; }
        }

        public int StatusAutoPhuBan
        {
            get { return status_auto_phu_ban; }
            set { status_auto_phu_ban = value; }
        }

        public int StatusUocNguyen
        {
            get { return status_uoc_nguyen; }
            set { status_uoc_nguyen = value; }
        }

        public int StatusNhanThuongHLVT
        {
            get { return status_nhan_thuong_hlvt; }
            set { status_nhan_thuong_hlvt = value; }
        }

        public int StatusMeTran
        {
            get { return status_me_tran; }
            set { status_me_tran = value; }
        }

        public int StatusHaiThuoc
        {
            get { return status_hai_thuoc; }
            set { status_hai_thuoc = value; }
        }

        public int StatusCauCa
        {
            get { return status_cau_ca; }
            set { status_cau_ca = value; }
        }

        public int StatusAutoThanTu
        {
            get { return status_auto_than_tu; }
            set { status_auto_than_tu = value; }
        }
    }

    public class CharacterList
    {
        public static Character GetCharacterByRowIndex(int index)
        {
            DataRow iDr = null;
            iDr = XMLCharacter.SelectByRowIndex(index);
            Character character = null;
            if (iDr != null)
            {
                character = new Character();
                character.ID = iDr[0] != DBNull.Value ? iDr[0].ToString() : string.Empty;
                character.Link = iDr[1] != DBNull.Value ? iDr[1].ToString() : string.Empty;
            }
            return character;
        }

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

        public static void DeleteCharacter(string characterID)
        {
            XMLCharacter.Delete(characterID);
        }
    }
}
