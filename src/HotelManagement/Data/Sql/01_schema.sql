-- =============================================================
-- Hệ thống quản lý khách sạn – tạo CSDL HotelManagement (13 bảng).
-- Tên bảng/cột trùng tên lớp/thuộc tính trong Models/Entities.cs.
-- Chạy lại được nhiều lần: XÓA và tạo lại toàn bộ bảng (mất dữ liệu cũ).
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -f 65001 -i 01_schema.sql
-- Giá trị trạng thái trong các ràng buộc CHECK phải khớp Models/Constants.cs.
-- =============================================================
-- sqlcmd mặc định tắt QUOTED_IDENTIFIER, bật lại cho giống SSMS.
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO
IF DB_ID(N'HotelManagement') IS NULL
    CREATE DATABASE HotelManagement;
GO
USE HotelManagement;
GO

DROP TABLE IF EXISTS ThanhToan, HoaDon, SuDungDichVu, DichVu, ChiTietLuuTru, LuuTru,
                     DatPhong, KhachHang, Phong, BangGia, LoaiPhong, TaiKhoan, NhanVien;
GO

-- ---------- 1. Hệ thống ----------
CREATE TABLE NhanVien (
    MaNV        VARCHAR(10)   NOT NULL CONSTRAINT PK_NhanVien PRIMARY KEY,
    HoTen       NVARCHAR(100) NOT NULL,
    NgaySinh    DATE          NOT NULL,
    SDT         VARCHAR(20)   NOT NULL DEFAULT '',
    Email       NVARCHAR(100) NOT NULL DEFAULT N'',
    ChucVu      NVARCHAR(50)  NOT NULL DEFAULT N''
);

CREATE TABLE TaiKhoan (
    MaTK        VARCHAR(10)   NOT NULL CONSTRAINT PK_TaiKhoan PRIMARY KEY,
    TenDangNhap NVARCHAR(50)  NOT NULL CONSTRAINT UQ_TaiKhoan_TenDangNhap UNIQUE,
    MatKhau     NVARCHAR(200) NOT NULL,  -- [02] lưu hash SHA-256 kèm salt
    VaiTro      NVARCHAR(50)  NOT NULL
        CONSTRAINT CK_TaiKhoan_VaiTro CHECK (VaiTro IN (N'Quản trị viên', N'Quản lý', N'Lễ tân', N'Nhân viên buồng phòng')),
    TrangThai   NVARCHAR(30)  NOT NULL DEFAULT N'Hoạt động'
        CONSTRAINT CK_TaiKhoan_TrangThai CHECK (TrangThai IN (N'Hoạt động', N'Đã khóa')),
    MaNV        VARCHAR(10)   NULL CONSTRAINT FK_TaiKhoan_NhanVien REFERENCES NhanVien(MaNV)
);

-- ---------- 2. Phòng ----------
CREATE TABLE LoaiPhong (
    MaLoai      VARCHAR(10)   NOT NULL CONSTRAINT PK_LoaiPhong PRIMARY KEY,
    TenLoai     NVARCHAR(50)  NOT NULL,
    GiaCoBan    DECIMAL(18,2) NOT NULL CONSTRAINT CK_LoaiPhong_GiaCoBan CHECK (GiaCoBan >= 0),
    SucChua     INT           NOT NULL CONSTRAINT CK_LoaiPhong_SucChua CHECK (SucChua > 0),
    MoTa        NVARCHAR(255) NOT NULL DEFAULT N''
);

CREATE TABLE BangGia (
    MaGia       VARCHAR(10)   NOT NULL CONSTRAINT PK_BangGia PRIMARY KEY,
    MaLoai      VARCHAR(10)   NOT NULL CONSTRAINT FK_BangGia_LoaiPhong REFERENCES LoaiPhong(MaLoai),
    TuNgay      DATE          NOT NULL,
    DenNgay     DATE          NOT NULL,
    GiaApDung   DECIMAL(18,2) NOT NULL CONSTRAINT CK_BangGia_GiaApDung CHECK (GiaApDung >= 0),
    GhiChu      NVARCHAR(255) NOT NULL DEFAULT N'',
    CONSTRAINT CK_BangGia_Ngay CHECK (DenNgay >= TuNgay)
);

CREATE TABLE Phong (
    MaPhong     VARCHAR(10)   NOT NULL CONSTRAINT PK_Phong PRIMARY KEY,
    SoPhong     VARCHAR(10)   NOT NULL CONSTRAINT UQ_Phong_SoPhong UNIQUE,
    Tang        INT           NOT NULL,
    TrangThai   NVARCHAR(30)  NOT NULL DEFAULT N'Trống'
        CONSTRAINT CK_Phong_TrangThai CHECK (TrangThai IN (N'Trống', N'Đã đặt', N'Đang ở', N'Đang dọn', N'Bảo trì')),
    MaLoai      VARCHAR(10)   NOT NULL CONSTRAINT FK_Phong_LoaiPhong REFERENCES LoaiPhong(MaLoai)
);

-- ---------- 3. Khách hàng & đặt phòng ----------
CREATE TABLE KhachHang (
    MaKH         VARCHAR(10)   NOT NULL CONSTRAINT PK_KhachHang PRIMARY KEY,
    HoTen        NVARCHAR(100) NOT NULL,
    CCCD_HoChieu VARCHAR(20)   NOT NULL DEFAULT '',
    SDT          VARCHAR(30)   NOT NULL DEFAULT '',
    Email        NVARCHAR(100) NOT NULL DEFAULT N'',
    DiaChi       NVARCHAR(255) NOT NULL DEFAULT N''
);

CREATE TABLE DatPhong (
    MaDat          VARCHAR(10)   NOT NULL CONSTRAINT PK_DatPhong PRIMARY KEY,
    NgayDat        DATETIME2(0)  NOT NULL,
    NgayNhanDuKien DATE          NOT NULL,
    NgayTraDuKien  DATE          NOT NULL,
    TrangThai      NVARCHAR(30)  NOT NULL DEFAULT N'Chờ nhận phòng'
        CONSTRAINT CK_DatPhong_TrangThai CHECK (TrangThai IN (N'Chờ nhận phòng', N'Đã nhận phòng', N'Hoàn tất', N'Đã hủy')),
    MaKH           VARCHAR(10)   NOT NULL CONSTRAINT FK_DatPhong_KhachHang REFERENCES KhachHang(MaKH),
    MaPhong        VARCHAR(10)   NOT NULL CONSTRAINT FK_DatPhong_Phong REFERENCES Phong(MaPhong),
    GhiChu         NVARCHAR(255) NOT NULL DEFAULT N'',
    CONSTRAINT CK_DatPhong_Ngay CHECK (NgayTraDuKien > NgayNhanDuKien)
);
CREATE INDEX IX_DatPhong_MaPhong ON DatPhong(MaPhong, TrangThai);

-- ---------- 4. Lưu trú & dịch vụ ----------
CREATE TABLE LuuTru (
    MaLuuTru       VARCHAR(10)   NOT NULL CONSTRAINT PK_LuuTru PRIMARY KEY,
    NgayNhan       DATETIME2(0)  NOT NULL,
    NgayTra        DATETIME2(0)  NOT NULL,  -- dự kiến khi đang ở, thực tế khi đã trả phòng
    GiaPhongApDung DECIMAL(18,2) NOT NULL CONSTRAINT CK_LuuTru_Gia CHECK (GiaPhongApDung >= 0),
    TrangThai      NVARCHAR(30)  NOT NULL DEFAULT N'Đang ở'
        CONSTRAINT CK_LuuTru_TrangThai CHECK (TrangThai IN (N'Đang ở', N'Đã trả phòng')),
    MaPhong        VARCHAR(10)   NOT NULL CONSTRAINT FK_LuuTru_Phong REFERENCES Phong(MaPhong),
    MaDat          VARCHAR(10)   NULL     CONSTRAINT FK_LuuTru_DatPhong REFERENCES DatPhong(MaDat)  -- NULL: khách vãng lai
);
CREATE INDEX IX_LuuTru_MaPhong ON LuuTru(MaPhong, TrangThai);

CREATE TABLE ChiTietLuuTru (
    MaCTLT      VARCHAR(10)   NOT NULL CONSTRAINT PK_ChiTietLuuTru PRIMARY KEY,
    MaLuuTru    VARCHAR(10)   NOT NULL CONSTRAINT FK_ChiTietLuuTru_LuuTru REFERENCES LuuTru(MaLuuTru),
    MaKH        VARCHAR(10)   NOT NULL CONSTRAINT FK_ChiTietLuuTru_KhachHang REFERENCES KhachHang(MaKH),
    VaiTro      NVARCHAR(30)  NOT NULL DEFAULT N'Khách chính'
        CONSTRAINT CK_ChiTietLuuTru_VaiTro CHECK (VaiTro IN (N'Khách chính', N'Khách đi cùng')),
    GhiChu      NVARCHAR(255) NOT NULL DEFAULT N'',
    CONSTRAINT UQ_ChiTietLuuTru UNIQUE (MaLuuTru, MaKH)
);

CREATE TABLE DichVu (
    MaDV        VARCHAR(10)   NOT NULL CONSTRAINT PK_DichVu PRIMARY KEY,
    TenDV       NVARCHAR(100) NOT NULL,
    DonGia      DECIMAL(18,2) NOT NULL CONSTRAINT CK_DichVu_DonGia CHECK (DonGia >= 0),
    DonViTinh   NVARCHAR(30)  NOT NULL DEFAULT N'',
    TrangThai   NVARCHAR(30)  NOT NULL DEFAULT N'Đang kinh doanh'
        CONSTRAINT CK_DichVu_TrangThai CHECK (TrangThai IN (N'Đang kinh doanh', N'Ngừng kinh doanh'))
);

CREATE TABLE SuDungDichVu (
    MaSD         VARCHAR(10)   NOT NULL CONSTRAINT PK_SuDungDichVu PRIMARY KEY,
    MaLuuTru     VARCHAR(10)   NOT NULL CONSTRAINT FK_SuDungDichVu_LuuTru REFERENCES LuuTru(MaLuuTru),
    MaDV         VARCHAR(10)   NOT NULL CONSTRAINT FK_SuDungDichVu_DichVu REFERENCES DichVu(MaDV),
    SoLuong      INT           NOT NULL CONSTRAINT CK_SuDungDichVu_SoLuong CHECK (SoLuong > 0),
    DonGiaApDung DECIMAL(18,2) NOT NULL CONSTRAINT CK_SuDungDichVu_DonGia CHECK (DonGiaApDung >= 0),
    ThanhTien    AS (SoLuong * DonGiaApDung),  -- khớp thuộc tính chỉ đọc SuDungDichVu.ThanhTien
    ThoiGian     DATETIME2(0)  NOT NULL
);
CREATE INDEX IX_SuDungDichVu_MaLuuTru ON SuDungDichVu(MaLuuTru);

-- ---------- 5. Thanh toán ----------
CREATE TABLE HoaDon (
    MaHD        VARCHAR(10)   NOT NULL CONSTRAINT PK_HoaDon PRIMARY KEY,
    MaLuuTru    VARCHAR(10)   NOT NULL CONSTRAINT FK_HoaDon_LuuTru REFERENCES LuuTru(MaLuuTru)
                              CONSTRAINT UQ_HoaDon_MaLuuTru UNIQUE,  -- mỗi lượt lưu trú một hóa đơn
    NgayLap     DATETIME2(0)  NOT NULL,
    TienPhong   DECIMAL(18,2) NOT NULL DEFAULT 0,
    TienDV      DECIMAL(18,2) NOT NULL DEFAULT 0,
    GiamGia     DECIMAL(18,2) NOT NULL DEFAULT 0,
    TongTien    DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThai   NVARCHAR(30)  NOT NULL DEFAULT N'Chưa thanh toán'
        CONSTRAINT CK_HoaDon_TrangThai CHECK (TrangThai IN (N'Chưa thanh toán', N'Thanh toán một phần', N'Đã thanh toán'))
);
CREATE INDEX IX_HoaDon_NgayLap ON HoaDon(NgayLap);

CREATE TABLE ThanhToan (
    MaTT        VARCHAR(10)   NOT NULL CONSTRAINT PK_ThanhToan PRIMARY KEY,
    MaHD        VARCHAR(10)   NOT NULL CONSTRAINT FK_ThanhToan_HoaDon REFERENCES HoaDon(MaHD),
    PhuongThuc  NVARCHAR(30)  NOT NULL,
    SoTien      DECIMAL(18,2) NOT NULL CONSTRAINT CK_ThanhToan_SoTien CHECK (SoTien > 0),
    ThoiGian    DATETIME2(0)  NOT NULL,
    TrangThai   NVARCHAR(30)  NOT NULL DEFAULT N'Thành công'
);
CREATE INDEX IX_ThanhToan_MaHD ON ThanhToan(MaHD);
GO

PRINT N'Đã tạo 13 bảng của CSDL HotelManagement.';
GO
