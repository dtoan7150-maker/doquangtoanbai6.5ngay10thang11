namespace KaraokeManagement.DTO
{
    public class ChiTietDichVuDTO
    {
        public int MaHD { get; set; }
        public int MaDV { get; set; }
        public string TenDV { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }
}
