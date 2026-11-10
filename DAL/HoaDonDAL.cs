using System;
using System.Data;
using Microsoft.Data.SqlClient;
using KaraokeManagement.DTO;

namespace KaraokeManagement.DAL
{
    public class HoaDonDAL : DatabaseAccess
    {
        // Lấy hóa đơn chưa thanh toán của phòng
        public HoaDonDTO LayHoaDonHienTaiCuaPhong(int maPhong)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "SELECT TOP 1 MaHD, MaPhong, GioVao, GioRa, TongTienPhong, TongTienDichVu, TrangThai " +
                               "FROM HoaDon WHERE MaPhong = @MaPhong AND TrangThai = 0 ORDER BY MaHD DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    return new HoaDonDTO
                    {
                        MaHD = Convert.ToInt32(reader["MaHD"]),
                        MaPhong = Convert.ToInt32(reader["MaPhong"]),
                        GioVao = Convert.ToDateTime(reader["GioVao"]),
                        GioRa = reader["GioRa"] != DBNull.Value ? (DateTime?)reader["GioRa"] : null,
                        TongTienPhong = Convert.ToDecimal(reader["TongTienPhong"]),
                        TongTienDichVu = Convert.ToDecimal(reader["TongTienDichVu"]),
                        TrangThai = Convert.ToBoolean(reader["TrangThai"])
                    };
                }
            }
            return null;
        }

        // Mở phòng: Thêm HoaDon mới (TrangThai = 0) và đổi trạng thái PhongHat sang 'Đang hát'
        public bool MoPhong(int maPhong, out int maHDTao)
        {
            maHDTao = 0;
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlHD = @"INSERT INTO HoaDon (MaPhong, GioVao, TrangThai) 
                                         VALUES (@MaPhong, GETDATE(), 0);
                                         SELECT SCOPE_IDENTITY();";
                        SqlCommand cmdHD = new SqlCommand(sqlHD, conn, trans);
                        cmdHD.Parameters.AddWithValue("@MaPhong", maPhong);
                        maHDTao = Convert.ToInt32(cmdHD.ExecuteScalar());

                        string sqlPhong = "UPDATE PhongHat SET TrangThai = N'Đang hát' WHERE MaPhong = @MaPhong";
                        SqlCommand cmdPhong = new SqlCommand(sqlPhong, conn, trans);
                        cmdPhong.Parameters.AddWithValue("@MaPhong", maPhong);
                        cmdPhong.ExecuteNonQuery();

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        // Đọc danh sách chi tiết dịch vụ gọi qua SqlDataAdapter + DataTable (Disconnected Model)
        public DataTable LayChiTietDichVuByMaHD(int maHD)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = GetConnection())
            {
                string query = @"SELECT ct.MaHD, ct.MaDV, dv.TenDV, ct.SoLuong, ct.DonGia, 
                                        (ct.SoLuong * ct.DonGia) AS ThanhTien
                                 FROM ChiTietDichVu ct
                                 JOIN DichVu dv ON ct.MaDV = dv.MaDV
                                 WHERE ct.MaHD = @MaHD";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaHD", maHD);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dt);
            }
            return dt;
        }

        // Thêm dịch vụ vào chi tiết hóa đơn
        public bool ThemDichVuVaoHoaDon(int maHD, int maDV, int soLuong, decimal donGia)
        {
            using (SqlConnection conn = GetConnection())
            {
                string sqlCheck = "SELECT COUNT(1) FROM ChiTietDichVu WHERE MaHD = @MaHD AND MaDV = @MaDV";
                SqlCommand cmdCheck = new SqlCommand(sqlCheck, conn);
                cmdCheck.Parameters.AddWithValue("@MaHD", maHD);
                cmdCheck.Parameters.AddWithValue("@MaDV", maDV);
                conn.Open();
                int count = Convert.ToInt32(cmdCheck.ExecuteScalar());

                string query = "";
                if (count > 0)
                {
                    query = "UPDATE ChiTietDichVu SET SoLuong = SoLuong + @SoLuong WHERE MaHD = @MaHD AND MaDV = @MaDV";
                }
                else
                {
                    query = "INSERT INTO ChiTietDichVu (MaHD, MaDV, SoLuong, DonGia) VALUES (@MaHD, @MaDV, @SoLuong, @DonGia)";
                }

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@MaHD", maHD);
                cmd.Parameters.AddWithValue("@MaDV", maDV);
                cmd.Parameters.AddWithValue("@SoLuong", soLuong);
                cmd.Parameters.AddWithValue("@DonGia", donGia);

                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Thanh toán & tính tiền bằng SqlTransaction
        public bool ThanhToanHoaDon(int maHD, int maPhong, decimal giaTheoGio, out decimal tongTienPhong, out decimal tongTienDV, out string errorMsg)
        {
            errorMsg = string.Empty;
            tongTienPhong = 0;
            tongTienDV = 0;

            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Lấy giờ vào
                        string sqlGioVao = "SELECT GioVao FROM HoaDon WHERE MaHD = @MaHD";
                        SqlCommand cmdGioVao = new SqlCommand(sqlGioVao, conn, trans);
                        cmdGioVao.Parameters.AddWithValue("@MaHD", maHD);
                        DateTime gioVao = Convert.ToDateTime(cmdGioVao.ExecuteScalar());
                        DateTime gioRa = DateTime.Now;

                        // Tính tiền giờ (Số giờ = Math.Max(0.5, số giờ thực tế làm tròn 2 chữ số thập phân))
                        double soGio = (gioRa - gioVao).TotalHours;
                        if (soGio < 0.1) soGio = 0.1; // Tối thiểu 0.1 giờ cho demo
                        tongTienPhong = Math.Round((decimal)soGio * giaTheoGio, 0);

                        // 2. Tính tổng tiền dịch vụ
                        string sqlSumDV = "SELECT ISNULL(SUM(SoLuong * DonGia), 0) FROM ChiTietDichVu WHERE MaHD = @MaHD";
                        SqlCommand cmdSumDV = new SqlCommand(sqlSumDV, conn, trans);
                        cmdSumDV.Parameters.AddWithValue("@MaHD", maHD);
                        tongTienDV = Convert.ToDecimal(cmdSumDV.ExecuteScalar());

                        // 3. Kiểm tra & Trừ tồn kho từng dịch vụ trong bảng DichVu
                        string sqlGetCT = "SELECT MaDV, SoLuong FROM ChiTietDichVu WHERE MaHD = @MaHD";
                        SqlCommand cmdGetCT = new SqlCommand(sqlGetCT, conn, trans);
                        cmdGetCT.Parameters.AddWithValue("@MaHD", maHD);
                        
                        List<Tuple<int, int>> dsDV = new List<Tuple<int, int>>();
                        using (SqlDataReader rd = cmdGetCT.ExecuteReader())
                        {
                            while (rd.Read())
                            {
                                dsDV.Add(new Tuple<int, int>(Convert.ToInt32(rd["MaDV"]), Convert.ToInt32(rd["SoLuong"])));
                            }
                        }

                        foreach (var item in dsDV)
                        {
                            int maDV = item.Item1;
                            int soLuong = item.Item2;

                            string sqlUpdateKho = @"UPDATE DichVu 
                                                    SET TonKho = TonKho - @SoLuong 
                                                    WHERE MaDV = @MaDV AND TonKho >= @SoLuong;";
                            SqlCommand cmdKho = new SqlCommand(sqlUpdateKho, conn, trans);
                            cmdKho.Parameters.AddWithValue("@SoLuong", soLuong);
                            cmdKho.Parameters.AddWithValue("@MaDV", maDV);

                            int rows = cmdKho.ExecuteNonQuery();
                            if (rows == 0)
                            {
                                trans.Rollback();
                                errorMsg = $"Dịch vụ mã #{maDV} không đủ số lượng tồn kho!";
                                return false;
                            }
                        }

                        // 4. Cập nhật HoaDon (GioRa, TongTienPhong, TongTienDichVu, TrangThai = 1)
                        string sqlUpdateHD = @"UPDATE HoaDon 
                                               SET GioRa = @GioRa, 
                                                   TongTienPhong = @TongTienPhong, 
                                                   TongTienDichVu = @TongTienDichVu, 
                                                   TrangThai = 1 
                                               WHERE MaHD = @MaHD;";
                        SqlCommand cmdUpdateHD = new SqlCommand(sqlUpdateHD, conn, trans);
                        cmdUpdateHD.Parameters.AddWithValue("@GioRa", gioRa);
                        cmdUpdateHD.Parameters.AddWithValue("@TongTienPhong", tongTienPhong);
                        cmdUpdateHD.Parameters.AddWithValue("@TongTienDichVu", tongTienDV);
                        cmdUpdateHD.Parameters.AddWithValue("@MaHD", maHD);
                        cmdUpdateHD.ExecuteNonQuery();

                        // 5. Cập nhật trạng thái PhongHat về 'Trống'
                        string sqlPhong = "UPDATE PhongHat SET TrangThai = N'Trống' WHERE MaPhong = @MaPhong";
                        SqlCommand cmdPhong = new SqlCommand(sqlPhong, conn, trans);
                        cmdPhong.Parameters.AddWithValue("@MaPhong", maPhong);
                        cmdPhong.ExecuteNonQuery();

                        trans.Commit();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        errorMsg = ex.Message;
                        return false;
                    }
                }
            }
        }
    }
}
