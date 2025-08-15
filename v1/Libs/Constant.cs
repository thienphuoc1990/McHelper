using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoVPT.Libs
{
    static class Constant
    {
        public const string Version = "AutoVPT Version 1.0 - Tử La Lan - https://www.facebook.com/Tu.La.Lan.NT";

        public const int StatusFeatureInactive = 0;
        public const int StatusFeatureActive = 1;
        public const int StatusFeatureRunning = 2;
        public const int StatusFeatureCompleted = 3;

        public const int WindowTopBarHeight = 25; // Height of window top bar.

        public const int MaxLoopQ = 10;
        public const int MaxLoop = 20;

        public const int VeryTimeShort = 100; // Short time
        public const int TimeShort = 1000; // Short time
        public const int TimeMedium = 3000; // Medium time
        public const int TimeLong = 5000; // Long time

        public const string NameNguyenLieuKimLoai = "Kim Loai";
        public const string NameNguyenLieuGo = "Go";
        public const string NameNguyenLieuLongThu = "Long Thu";
        public const string NameNguyenLieuVai = "Vai";
        public const string NameNguyenLieuNgoc = "Ngoc";
        public const string NameNguyenLieuKimLoaiHiem = "Kim Loai Hiem";
        public const string NameNguyenLieuGoTot = "Go Tot";
        public const string NameNguyenLieuDaThu = "Da Thu";
        public const string NameNguyenLieuGamVoc = "Gam Voc";
        public const string NameNguyenLieuPhaLe = "Pha Le";

        public const string NameLoaiMatBaoThanBinh = "Thần Binh";
        public const string NameLoaiMatBaoChienTrang = "Chiến Trang";
        public const string NameLoaiMatBaoPhapSuc = "Pháp Sức";
        public const string NameLoaiMatBaoVoUu = "Vô Ưu";
        public const string NameLoaiMatBaoThanhDien = "Thánh Điện";
        public const string NameLoaiMatBaoHangDong = "Hang Động";
        public const string NameLoaiMatBaoDaiMac = "Đại Mạc";
        public const string NameLoaiMatBaoDiCanh = "Di Cảnh";
        public const string NameLoaiMatBaoLietDiem = "Liệt Diễm";
        public const string NameLoaiMatBaoLangHuyet = "Lang Huyệt";
        public const string NameLoaiMatBaoLacVien = "Lạc Viên";

        public const string ResourcePath = "resources";
        public const string ChineseResourcePath = "cn_resources";

        public const string ImagePathPhuBanFolder = "/phu_ban/";
        public const string ImagePathMatBaoFolder = "/mat_bao/";
        public const string ImagePathCharNameFolder = "/char_name/";
        public const string ImagePathInMapFolder = "/in_map/";
        public const string ImagePathGlobalFolder = "/global/";
        public const string ImagePathTriAnFolder = "/tri_an/";
        public const string ImagePathEventFolder = "/event/";

        public const string ImagePathGlobalMiniMap = "/global/minimap.png";
        public const string ImagePathGlobalChuyenKenh = "/global/doikenh.png";
        public const string ImagePathInMapChar = "/in_map/char.png";
        public const string ImagePathGlobalFPS = "/global/fps.png";
        public const string ImagePathGlobalCachChoi = "/global/cach_choi.png";
        public const string ImagePathGlobalCachChoiKiemTien = "/global/cach_choi_kiem_tien.png";
        public const string ImagePathGlobalCachChoiKiemTienActive = "/global/cach_choi_kiem_tien_active.png";
        public const string ImagePathGlobalCachChoiKiemTienHover = "/global/cach_choi_kiem_tien_hover.png";
        public const string ImagePathGlobalTat = "/global/tat.png";
        public const string ImagePathGlobalTruongCanVeDHT = "/global/truong_can_ve_dht.png";
        public const string ImagePathGlobalChatTabHeThong = "/global/chat_tab_he_thong.png";
        public const string ImagePathGlobalChatXoa = "/global/chat_clear.png";
        public const string ImagePathGlobalTui = "/global/tui.png";
        public const string ImagePathGlobalTuiNhiemVu = "/global/tui_tab_nhiemvu.png";
        public const string ImagePathGlobalTuiNhiemVuActive = "/global/tui_tab_nhiemvu_active.png";
        public const string ImagePathGlobalTuiNhiemVuHover = "/global/tui_tab_nhiemvu_hover.png";
        public const string ImagePathGlobalXuong = "/global/xuong.png";
        public const string ImagePathGlobalBay = "/global/bay.png";
        public const string ImagePathGlobalBangNhiemVu = "/global/nvu.png";
        public const string ImagePathGlobalBangNhiemVuVong = "/global/nhiemvuvong.png";
        public const string ImagePathGlobalBangNhiemVuVongActive = "/global/nhiemvuvong_active.png";
        public const string ImagePathGlobalBangNhiemVuTriAn = "/global/nhiemvutrian.png";
        public const string ImagePathGlobalBangNhiemVuTriAnActive = "/global/nhiemvutrian_active.png";
        public const string ImagePathGlobalTraNhiemVu = "/global/xong.png";
        public const string ImagePathGlobalTraNhiemVuActive = "/global/xong_active.png";

        public const string ImagePathDoiThoai = "/doi_thoai/";
        public const string ImagePathViTriNPC = "/vi_tri_npc/";
        public const string ImagePathKhongTrongTranDau = "/global/khongtrongtrandau.png";

        public const string ImagePathMapsFolder = "/maps/";
        public const string ImagePathMiniMap = "/maps/map.png";
        public const string ImagePathWorldMap = "/maps/world_map.png";
        public const string ImagePathSecondWorldMap = "/maps/second_world_map.png";

        public const string ImagePathTriAnNDPT = "/tri_an/noidungphanthuong.png";
        public const string ImagePathTriAnQChuaNhan = "/tri_an/nhiemvutrianchuanhan.png";
        public const string ImagePathTriAnQChuaNhanActive = "/tri_an/nhiemvutrianchuanhan_active.png";
        public const string ImagePathTriAnQDaXong = "/tri_an/nhiemvutriandaxong.png";
        public const string ImagePathTriAnBanDoNhiemVu = "/tri_an/bandonhiemvu.png";
        public const string ImagePathTriAnToaDo = "/tri_an/toa_do.png";
        public const string ImagePathTriAnToaDo2 = "/tri_an/toa_do2.png";
        public const string ImagePathTriAnPhanQuan = "/tri_an/phanquan";
        public const string ImagePathTriAnPhiTac = "/tri_an/phitac";
        public const string ImagePathTenTriAnPhanQuan = "/tri_an/tenphanquan";
        public const string ImagePathTenTriAnPhiTac = "/tri_an/tenphitac";
        public const string ImagePathTriAnBangNhiemVuTruongCanVeDHT = "/tri_an/bangnhiemvutruongcanvedht.png";

        public const string ImagePathChuotAnToan = "/trong_nl/chuot_an_toan.png";

        public const string ImagePathTrangVienButton = "/trong_nl/trang_vien.png"; // Image path of "Trang Viên" button.
        public const string ImagePathNuoiTrongButton = "/trong_nl/nuoi_trong.png"; // Image path of "Nuôi Trồng" button.
        public const string ImagePathThuHoachButton = "/trong_nl/thu_hoach.png"; // Image path of "Thu Hoạch" button.
        public const string ImagePathDiemThuHoachButton = "/trong_nl/diem_thu_hoach.png"; // Image path of "Điểm Thu Hoạch" button.
        public const string ImagePathDatTrong = "/trong_nl/dat_trong.png"; // Image path of "Đất Trống".

        public const string ImagePathNguyenLieuKimLoai = "/trong_nl/kim_loai.png"; // Image path of kim loại
        public const string ImagePathNguyenLieuGo = "/trong_nl/go.png"; // Image path of gỗ
        public const string ImagePathNguyenLieuLongThu = "/trong_nl/long_thu.png"; // Image path of lông thú
        public const string ImagePathNguyenLieuVai = "/trong_nl/vai.png"; // Image path of vải
        public const string ImagePathNguyenLieuNgoc = "/trong_nl/ngoc.png"; // Image path of ngọc
        public const string ImagePathNguyenLieuKimLoaiHiem = "/trong_nl/kim_loai_hiem.png"; // Image path of kim loại hiếm
        public const string ImagePathNguyenLieuGoTot = "/trong_nl/go_tot.png"; // Image path of gỗ tốt
        public const string ImagePathNguyenLieuDaThu = "/trong_nl/da_thu.png"; // Image path of da thú
        public const string ImagePathNguyenLieuGamVoc = "/trong_nl/gam_voc.png"; // Image path of gấm vóc
        public const string ImagePathNguyenLieuPhaLe = "/trong_nl/pha_le.png"; // Image path of pha mê

        public const string ImagePathStartButton = "/login/start_btn.png"; // Image path of start button.
    }
}
