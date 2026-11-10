using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using KaraokeManagement.BUS;
using KaraokeManagement.DTO;

namespace KaraokeManagement.GUI
{
    public partial class FormMain : Form
    {
        private PhongHatBUS phongBUS = new PhongHatBUS();
        private DichVuBUS dvBUS = new DichVuBUS();
        private HoaDonBUS hdBUS = new HoaDonBUS();

        private FlowLayoutPanel pnlRoomGrid;
        private DataGridView dgvDichVuPhong;
        private ComboBox cboDichVu;
        private NumericUpDown numSoLuong;
        private Button btnMoPhong;
        private Button btnThemDV;
        private Button btnThanhToan;
        private Label lblThongTinPhong;
        private Label lblGioVao;
        private Label lblTongTien;

        private PhongHatDTO selectedRoom = null;
        private HoaDonDTO currentHoaDon = null;

        public FormMain()
        {
            InitializeComponent();
            LoadRoomGrid();
            LoadDanhSachDichVu();
        }

        private void InitializeComponent()
        {
            this.Text = "Hệ thống Quản lý Đặt phòng & Dịch vụ Karaoke";
            this.Size = new Size(1000, 650);
            this.StartPosition = FormStartPosition.CenterScreen;

            // GroupBox Sơ đồ phòng hát (Visual Room Grid)
            GroupBox gbPhong = new GroupBox()
            {
                Text = "Sơ đồ phòng hát (Visual Room Grid)",
                Left = 15,
                Top = 15,
                Width = 450,
                Height = 570
            };

            pnlRoomGrid = new FlowLayoutPanel()
            {
                Dock = DockStyle.Fill,
                AutoScroll = true
            };
            gbPhong.Controls.Add(pnlRoomGrid);

            // GroupBox Chi tiết & Dịch vụ
            GroupBox gbNghiepVu = new GroupBox()
            {
                Text = "Chi tiết phòng & Gọi dịch vụ",
                Left = 480,
                Top = 15,
                Width = 480,
                Height = 570
            };

            lblThongTinPhong = new Label() { Text = "Chưa chọn phòng", Left = 20, Top = 25, Width = 440, Font = new Font("Arial", 11, FontStyle.Bold) };
            lblGioVao = new Label() { Text = "Giờ vào: --:--:--", Left = 20, Top = 55, Width = 440 };

            btnMoPhong = new Button() { Text = "MỞ PHÒNG", Left = 20, Top = 85, Width = 130, Height = 35, BackColor = Color.LightGreen };
            btnMoPhong.Click += BtnMoPhong_Click;

            Label lblDV = new Label() { Text = "Chọn dịch vụ:", Left = 20, Top = 135, Width = 90 };
            cboDichVu = new ComboBox() { Left = 110, Top = 132, Width = 180, DropDownStyle = ComboBoxStyle.DropDownList };

            Label lblSL = new Label() { Text = "Số lượng:", Left = 300, Top = 135, Width = 60 };
            numSoLuong = new NumericUpDown() { Left = 365, Top = 132, Width = 50, Minimum = 1, Maximum = 100, Value = 1 };

            btnThemDV = new Button() { Text = "Thêm DV", Left = 420, Top = 130, Width = 50, Height = 26 };
            btnThemDV.Click += BtnThemDV_Click;

            dgvDichVuPhong = new DataGridView()
            {
                Left = 20,
                Top = 170,
                Width = 440,
                Height = 280,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false
            };

            lblTongTien = new Label() { Text = "Tổng tiền tạm tính: 0 VNĐ", Left = 20, Top = 465, Width = 440, Font = new Font("Arial", 10, FontStyle.Bold), ForeColor = Color.DarkRed };

            btnThanhToan = new Button() { Text = "THANH TOÁN HÓA ĐƠN", Left = 20, Top = 500, Width = 440, Height = 45, BackColor = Color.Orange, Font = new Font("Arial", 11, FontStyle.Bold) };
            btnThanhToan.Click += BtnThanhToan_Click;

            gbNghiepVu.Controls.Add(lblThongTinPhong);
            gbNghiepVu.Controls.Add(lblGioVao);
            gbNghiepVu.Controls.Add(btnMoPhong);
            gbNghiepVu.Controls.Add(lblDV);
            gbNghiepVu.Controls.Add(cboDichVu);
            gbNghiepVu.Controls.Add(lblSL);
            gbNghiepVu.Controls.Add(numSoLuong);
            gbNghiepVu.Controls.Add(btnThemDV);
            gbNghiepVu.Controls.Add(dgvDichVuPhong);
            gbNghiepVu.Controls.Add(lblTongTien);
            gbNghiepVu.Controls.Add(btnThanhToan);

            this.Controls.Add(gbPhong);
            this.Controls.Add(gbNghiepVu);
        }

        private void LoadRoomGrid()
        {
            pnlRoomGrid.Controls.Clear();
            List<PhongHatDTO> dsPhong = phongBUS.LayDanhSachPhong();

            foreach (var phong in dsPhong)
            {
                Button btn = new Button()
                {
                    Width = 125,
                    Height = 90,
                    Text = $"{phong.TenPhong}\n({phong.LoaiPhong})\n{phong.TrangThai}",
                    Tag = phong,
                    Margin = new Padding(8),
                    Font = new Font("Arial", 9, FontStyle.Bold)
                };

                if (phong.TrangThai == "Đang hát")
                {
                    btn.BackColor = Color.IndianRed;
                    btn.ForeColor = Color.White;
                }
                else
                {
                    btn.BackColor = Color.LightGreen;
                    btn.ForeColor = Color.Black;
                }

                btn.Click += RoomButton_Click;
                pnlRoomGrid.Controls.Add(btn);
            }
        }

        private void LoadDanhSachDichVu()
        {
            List<DichVuDTO> dsDV = dvBUS.LayDanhSachDichVu();
            cboDichVu.DataSource = dsDV;
            cboDichVu.DisplayMember = "TenDV";
            cboDichVu.ValueMember = "MaDV";
        }

        private void RoomButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            selectedRoom = btn.Tag as PhongHatDTO;

            lblThongTinPhong.Text = $"{selectedRoom.TenPhong} - Loại: {selectedRoom.LoaiPhong} - Đơn giá: {selectedRoom.GiaTheoGio:N0} đ/giờ";

            if (selectedRoom.TrangThai == "Đang hát")
            {
                btnMoPhong.Enabled = false;
                btnThemDV.Enabled = true;
                btnThanhToan.Enabled = true;

                currentHoaDon = hdBUS.LayHoaDonHienTaiCuaPhong(selectedRoom.MaPhong);
                if (currentHoaDon != null)
                {
                    lblGioVao.Text = $"Giờ vào: {currentHoaDon.GioVao:HH:mm:ss dd/MM/yyyy}";
                    LoadChiTietDichVu(currentHoaDon.MaHD);
                }
            }
            else
            {
                btnMoPhong.Enabled = true;
                btnThemDV.Enabled = false;
                btnThanhToan.Enabled = false;
                lblGioVao.Text = "Giờ vào: --:--:--";
                dgvDichVuPhong.DataSource = null;
                lblTongTien.Text = "Tổng tiền tạm tính: 0 VNĐ";
                currentHoaDon = null;
            }
        }

        private void LoadChiTietDichVu(int maHD)
        {
            DataTable dt = hdBUS.LayChiTietDichVuByMaHD(maHD);
            dgvDichVuPhong.DataSource = dt;

            decimal tongDV = 0;
            foreach (DataRow row in dt.Rows)
            {
                tongDV += Convert.ToDecimal(row["ThanhTien"]);
            }
            lblTongTien.Text = $"Tổng tiền dịch vụ: {tongDV:N0} VNĐ";
        }

        private void BtnMoPhong_Click(object sender, EventArgs e)
        {
            if (selectedRoom == null)
            {
                MessageBox.Show("Vui lòng chọn phòng từ sơ đồ!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (hdBUS.MoPhong(selectedRoom.MaPhong, out int maHD))
            {
                MessageBox.Show($"Mở {selectedRoom.TenPhong} thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadRoomGrid();
                selectedRoom.TrangThai = "Đang hát";
                RoomButton_Click(GetButtonByRoom(selectedRoom.MaPhong), null);
            }
        }

        private Button GetButtonByRoom(int maPhong)
        {
            foreach (Control c in pnlRoomGrid.Controls)
            {
                if (c is Button btn && btn.Tag is PhongHatDTO p && p.MaPhong == maPhong)
                    return btn;
            }
            return null;
        }

        private void BtnThemDV_Click(object sender, EventArgs e)
        {
            if (currentHoaDon == null || cboDichVu.SelectedItem == null) return;

            DichVuDTO dv = cboDichVu.SelectedItem as DichVuDTO;
            int soLuong = (int)numSoLuong.Value;

            if (hdBUS.ThemDichVuVaoHoaDon(currentHoaDon.MaHD, dv.MaDV, soLuong, dv.DonGia))
            {
                LoadChiTietDichVu(currentHoaDon.MaHD);
            }
        }

        private void BtnThanhToan_Click(object sender, EventArgs e)
        {
            if (selectedRoom == null || currentHoaDon == null) return;

            DialogResult result = MessageBox.Show($"Xác nhận thanh toán cho {selectedRoom.TenPhong}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                if (hdBUS.ThanhToanHoaDon(currentHoaDon.MaHD, selectedRoom.MaPhong, selectedRoom.GiaTheoGio, out decimal tongPhong, out decimal tongDV, out string errorMsg))
                {
                    decimal tongCong = tongPhong + tongDV;
                    MessageBox.Show($"THANH TOÁN THÀNH CÔNG!\n\n- Tiền giờ: {tongPhong:N0} VNĐ\n- Tiền dịch vụ: {tongDV:N0} VNĐ\n------------------------\nTỔNG CỘNG: {tongCong:N0} VNĐ", "Hóa đơn thanh toán", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    LoadRoomGrid();
                    selectedRoom = null;
                    currentHoaDon = null;
                    lblThongTinPhong.Text = "Chưa chọn phòng";
                    lblGioVao.Text = "Giờ vào: --:--:--";
                    dgvDichVuPhong.DataSource = null;
                    lblTongTien.Text = "Tổng tiền tạm tính: 0 VNĐ";
                }
                else
                {
                    MessageBox.Show($"Thanh toán thất bại (Rollback Transaction): {errorMsg}", "Lỗi Transaction", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
