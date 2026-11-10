using System.Collections.Generic;
using KaraokeManagement.DAL;
using KaraokeManagement.DTO;

namespace KaraokeManagement.BUS
{
    public class PhongHatBUS
    {
        private PhongHatDAL phongDAL = new PhongHatDAL();

        public List<PhongHatDTO> LayDanhSachPhong()
        {
            return phongDAL.LayDanhSachPhong();
        }

        public bool CapNhatTrangThaiPhong(int maPhong, string trangThai)
        {
            return phongDAL.CapNhatTrangThaiPhong(maPhong, trangThai);
        }
    }
}
