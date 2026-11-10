using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using KaraokeManagement.DTO;

namespace KaraokeManagement.DAL
{
    public class PhongHatDAL : DatabaseAccess
    {
        public List<PhongHatDTO> LayDanhSachPhong()
        {
            List<PhongHatDTO> list = new List<PhongHatDTO>();
            using (SqlConnection conn = GetConnection())
            {
                string query = "SELECT MaPhong, TenPhong, LoaiPhong, GiaTheoGio, TrangThai FROM PhongHat";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new PhongHatDTO
                    {
                        MaPhong = Convert.ToInt32(reader["MaPhong"]),
                        TenPhong = reader["TenPhong"].ToString(),
                        LoaiPhong = reader["LoaiPhong"].ToString(),
                        GiaTheoGio = Convert.ToDecimal(reader["GiaTheoGio"]),
                        TrangThai = reader["TrangThai"].ToString()
                    });
                }
            }
            return list;
        }

        public bool CapNhatTrangThaiPhong(int maPhong, string trangThai)
        {
            using (SqlConnection conn = GetConnection())
            {
                string query = "UPDATE PhongHat SET TrangThai = @TrangThai WHERE MaPhong = @MaPhong";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@TrangThai", trangThai);
                cmd.Parameters.AddWithValue("@MaPhong", maPhong);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
