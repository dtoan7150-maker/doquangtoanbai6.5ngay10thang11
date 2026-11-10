using System.Data;
using KaraokeManagement.DAL;
using KaraokeManagement.DTO;

namespace KaraokeManagement.BUS
{
    public class HoaDonBUS
    {
        private HoaDonDAL hdDAL = new HoaDonDAL();

        public HoaDonDTO LayHoaDonHienTaiCuaPhong(int maPhong)
        {
            return hdDAL.LayHoaDonHienTaiCuaPhong(maPhong);
        }

        public bool MoPhong(int maPhong, out int maHD)
        {
            return hdDAL.MoPhong(maPhong, out maHD);
        }

        public DataTable LayChiTietDichVuByMaHD(int maHD)
        {
            return hdDAL.LayChiTietDichVuByMaHD(maHD);
        }

        public bool ThemDichVuVaoHoaDon(int maHD, int maDV, int soLuong, decimal donGia)
        {
            return hdDAL.ThemDichVuVaoHoaDon(maHD, maDV, soLuong, donGia);
        }

        public bool ThanhToanHoaDon(int maHD, int maPhong, decimal giaTheoGio, out decimal tongPhong, out decimal tongDV, out string error)
        {
            return hdDAL.ThanhToanHoaDon(maHD, maPhong, giaTheoGio, out tongPhong, out tongDV, out error);
        }
    }
}
