CREATE DATABASE KaraokeDB;
GO
USE KaraokeDB;
GO

-- 1. Bảng PhongHat
CREATE TABLE PhongHat (
    MaPhong INT PRIMARY KEY IDENTITY(1,1),
    TenPhong NVARCHAR(50) NOT NULL,
    LoaiPhong NVARCHAR(30),
    GiaTheoGio DECIMAL(18,2),
    TrangThai NVARCHAR(20) DEFAULT N'Trống'
);

-- 2. Bảng DichVu
CREATE TABLE DichVu (
    MaDV INT PRIMARY KEY IDENTITY(1,1),
    TenDV NVARCHAR(100) NOT NULL,
    DonGia DECIMAL(18,2),
    TonKho INT CHECK (TonKho >= 0)
);

-- 3. Bảng HoaDon
CREATE TABLE HoaDon (
    MaHD INT PRIMARY KEY IDENTITY(1,1),
    MaPhong INT FOREIGN KEY REFERENCES PhongHat(MaPhong),
    GioVao DATETIME NOT NULL DEFAULT GETDATE(),
    GioRa DATETIME NULL,
    TongTienPhong DECIMAL(18,2) DEFAULT 0,
    TongTienDichVu DECIMAL(18,2) DEFAULT 0,
    TrangThai BIT DEFAULT 0 -- 0: Chưa thanh toán, 1: Đã thanh toán
);

-- 4. Bảng ChiTietDichVu
CREATE TABLE ChiTietDichVu (
    MaHD INT FOREIGN KEY REFERENCES HoaDon(MaHD),
    MaDV INT FOREIGN KEY REFERENCES DichVu(MaDV),
    SoLuong INT CHECK (SoLuong > 0),
    DonGia DECIMAL(18,2),
    PRIMARY KEY (MaHD, MaDV)
);

-- Dữ liệu mẫu
INSERT INTO PhongHat (TenPhong, LoaiPhong, GiaTheoGio, TrangThai) VALUES
(N'Phòng VIP 01', N'VIP', 200000, N'Trống'),
(N'Phòng VIP 02', N'VIP', 200000, N'Trống'),
(N'Phòng Thường 01', N'Thường', 120000, N'Trống'),
(N'Phòng Thường 02', N'Thường', 120000, N'Trống'),
(N'Phòng Thường 03', N'Thường', 120000, N'Trống'),
(N'Phòng Super VIP', N'VIP', 300000, N'Trống');

INSERT INTO DichVu (TenDV, DonGia, TonKho) VALUES
(N'Bia Heineken (Lon)', 25000, 100),
(N'Bia Tiger (Lon)', 22000, 120),
(N'Nước ngọt Cocacola', 15000, 150),
(N'Trái cây dĩa lớn', 150000, 30),
(N'Hạt điều rang muối', 50000, 50),
(N'Bò khô đĩa', 80000, 40);
