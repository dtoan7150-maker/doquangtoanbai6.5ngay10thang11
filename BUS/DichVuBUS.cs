using System.Collections.Generic;
using KaraokeManagement.DAL;
using KaraokeManagement.DTO;

namespace KaraokeManagement.BUS
{
    public class DichVuBUS
    {
        private DichVuDAL dvDAL = new DichVuDAL();

        public List<DichVuDTO> LayDanhSachDichVu()
        {
            return dvDAL.LayDanhSachDichVu();
        }
    }
}
