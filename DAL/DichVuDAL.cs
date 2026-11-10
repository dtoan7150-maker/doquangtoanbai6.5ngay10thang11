using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using KaraokeManagement.DTO;

namespace KaraokeManagement.DAL
{
    public class DichVuDAL : DatabaseAccess
    {
        public List<DichVuDTO> LayDanhSachDichVu()
        {
            List<DichVuDTO> list = new List<DichVuDTO>();
            using (SqlConnection conn = GetConnection())
            {
                string query = "SELECT MaDV, TenDV, DonGia, TonKho FROM DichVu";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new DichVuDTO
                    {
                        MaDV = Convert.ToInt32(reader["MaDV"]),
                        TenDV = reader["TenDV"].ToString(),
                        DonGia = Convert.ToDecimal(reader["DonGia"]),
                        TonKho = Convert.ToInt32(reader["TonKho"])
                    });
                }
            }
            return list;
        }
    }
}
