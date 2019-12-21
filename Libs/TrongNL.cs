using System;
using System.Threading;

namespace AutoVPT.Libs
{
    class TrongNL
    {
        public IntPtr mHWnd;
        public string mWindowName;
        public AutoFeatures mAuto;
        public string mLoaiNL;

        public TrongNL(IntPtr hWnd, string windowName, AutoFeatures auto)
        {
            mHWnd = hWnd;
            mWindowName = windowName;
            mAuto = auto;
        }

        public void thuHoach()
        {
            mAuto.writeStatus("Thu hoạch nguyên liệu ...");
            mAuto.clickToImage(Constant.ImagePathThuHoachButton);
            Thread.Sleep(Constant.TimeShort);
            mAuto.clickToImage(Constant.ImagePathDiemThuHoachButton);
            mAuto.writeStatus("Đã trồng xong nguyên liệu ...");
        }

        public void trong()
        {
            mAuto.writeStatus("Trồng Nguyên Liệu ...");
            bool trong = true;
            int i = 0;
            while (trong && i < Constant.MaxLoop)
            {
                trong = mAuto.clickToImage(Constant.ImagePathDatTrong, 0, -25);
                i++;
            }
        }

        public void chonNL()
        {
            mAuto.writeStatus("Chọn nguyên liệu để trồng ...");
            string pathImage;
            switch (mLoaiNL)
            {
                case Constant.NameNguyenLieuKimLoai:
                    pathImage = Constant.ImagePathNguyenLieuKimLoai;
                    break;
                case Constant.NameNguyenLieuGo:
                    pathImage = Constant.ImagePathNguyenLieuGo;
                    break;
                case Constant.NameNguyenLieuNgoc:
                    pathImage = Constant.ImagePathNguyenLieuNgoc;
                    break;
                case Constant.NameNguyenLieuVai:
                    pathImage = Constant.ImagePathNguyenLieuVai;
                    break;
                case Constant.NameNguyenLieuLongThu:
                    pathImage = Constant.ImagePathNguyenLieuLongThu;
                    break;
                case Constant.NameNguyenLieuGamVoc:
                    pathImage = Constant.ImagePathNguyenLieuGamVoc;
                    break;
                case Constant.NameNguyenLieuDaThu:
                    pathImage = Constant.ImagePathNguyenLieuDaThu;
                    break;
                case Constant.NameNguyenLieuPhaLe:
                    pathImage = Constant.ImagePathNguyenLieuPhaLe;
                    break;
                case Constant.NameNguyenLieuKimLoaiHiem:
                    pathImage = Constant.ImagePathNguyenLieuKimLoaiHiem;
                    break;
                case Constant.NameNguyenLieuGoTot:
                    pathImage = Constant.ImagePathNguyenLieuGoTot;
                    break;
                default:
                    pathImage = Constant.ImagePathNguyenLieuKimLoai;
                    break;
            }

            mAuto.clickToImage(pathImage);
            Thread.Sleep(Constant.TimeShort);
        }

        public bool kiemTraSoDatTrong()
        {
            mAuto.writeStatus("Kiểm tra còn đất trống để trồng hay không ?");
            return mAuto.findImage(Constant.ImagePathDatTrong);
        }

        public void moNuoiTrong()
        {
            mAuto.writeStatus("Mở bảng nuôi trồng nguyên liệu ...");
            mAuto.clickToImage(Constant.ImagePathNuoiTrongButton);
            Thread.Sleep(Constant.TimeShort);
        }

        public void dongTrangVien()
        {
            mAuto.writeStatus("Đóng trang viên ...");
            mAuto.clickToImage(Constant.ImagePathTrangVienButton);
        }

        public void moTrangVien()
        {
            mAuto.writeStatus("Mở trang viên ...");
            mAuto.closeAllDialog();
            // Mở menu phải
            mAuto.writeStatus("Mở menu phải ...");
            mAuto.moMenuPhai();
            mAuto.clickToImage(Constant.ImagePathTrangVienButton);
            mAuto.writeStatus("Chờ 1 thời gian để flash tải thông tin mở trang viên ...");
            Thread.Sleep(Constant.TimeMedium);
        }

        public void setLoaiNL(string loaiNL)
        {
            mLoaiNL = loaiNL;
        }
    }
}
