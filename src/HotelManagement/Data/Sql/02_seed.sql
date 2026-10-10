-- =============================================================
-- Dữ liệu mẫu tối thiểu, cùng mã với Data/MockDb.cs để hai chế độ Mock/Sql khớp nhau.
-- Chạy sau 01_schema.sql:
--   sqlcmd -S "(localdb)\MSSQLLocalDB" -f 65001 -i 02_seed.sql
-- Ngày đặt phòng/lưu trú tính theo ngày chạy script.
-- =============================================================
USE HotelManagement;
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DECLARE @HomNay DATE = CAST(GETDATE() AS DATE);
DECLARE @Nam INT = YEAR(@HomNay);
DECLARE @Tet DATE = DATEFROMPARTS(@Nam + CASE WHEN MONTH(@HomNay) > 2 THEN 1 ELSE 0 END, 1, 25);

-- ---------- Nhân viên, tài khoản (mật khẩu demo 123; [02] đổi sang hash) ----------
INSERT INTO NhanVien (MaNV, HoTen, NgaySinh, SDT, Email, ChucVu) VALUES
('NV001', N'Nguyễn Tấn Phát', '1990-03-12', '0901234567', N'phat.nt@hotel.vn', N'Quản trị viên'),
('NV002', N'Trần Thị Mai',    '1985-07-04', '0912345678', N'mai.tt@hotel.vn',  N'Quản lý'),
('NV003', N'Lê Hoàng Nam',    '1998-11-20', '0987654321', N'nam.lh@hotel.vn',  N'Lễ tân'),
('NV004', N'Phạm Ngọc Anh',   '2000-01-15', '0934567890', N'anh.pn@hotel.vn',  N'Lễ tân'),
('NV005', N'Võ Văn Hùng',     '1992-05-30', '0978123456', N'hung.vv@hotel.vn', N'Nhân viên buồng phòng'),
('NV006', N'Đặng Thu Hà',     '1995-09-09', '0965432187', N'ha.dt@hotel.vn',   N'Nhân viên buồng phòng'),
('NV007', N'Bùi Minh Tuấn',   '1997-02-18', '0944556677', N'tuan.bm@hotel.vn', N'Lễ tân');

INSERT INTO TaiKhoan (MaTK, TenDangNhap, MatKhau, VaiTro, TrangThai, MaNV) VALUES
('TK001', N'admin',      N'123', N'Quản trị viên',         N'Hoạt động', 'NV001'),
('TK002', N'quanly',     N'123', N'Quản lý',               N'Hoạt động', 'NV002'),
('TK003', N'letan',      N'123', N'Lễ tân',                N'Hoạt động', 'NV003'),
('TK004', N'letan2',     N'123', N'Lễ tân',                N'Đã khóa',   'NV004'),
('TK005', N'buongphong', N'123', N'Nhân viên buồng phòng', N'Hoạt động', 'NV005');

-- ---------- Loại phòng, bảng giá ----------
INSERT INTO LoaiPhong (MaLoai, TenLoai, GiaCoBan, SucChua, MoTa) VALUES
('LP01', N'Standard', 500000,  2, N'Phòng tiêu chuẩn, 1 giường đôi, 20m²'),
('LP02', N'Superior', 750000,  2, N'2 giường đơn, cửa sổ hướng phố, 25m²'),
('LP03', N'Deluxe',   1100000, 3, N'Giường King, ban công, bồn tắm, 32m²'),
('LP04', N'Family',   1500000, 4, N'2 giường đôi, phù hợp gia đình, 40m²'),
('LP05', N'Suite',    2500000, 4, N'Phòng khách riêng, view toàn cảnh, 60m²');

INSERT INTO BangGia (MaGia, MaLoai, TuNgay, DenNgay, GiaApDung, GhiChu) VALUES
('BG001', 'LP03', @Tet, DATEADD(DAY, 10, @Tet), 1450000, N'Tết Nguyên đán'),
('BG002', 'LP04', @Tet, DATEADD(DAY, 10, @Tet), 1950000, N'Tết Nguyên đán'),
('BG003', 'LP05', @Tet, DATEADD(DAY, 10, @Tet), 3200000, N'Tết Nguyên đán'),
('BG004', 'LP01', DATEFROMPARTS(@Nam, 12, 24), DATEFROMPARTS(@Nam + 1, 1, 2), 650000, N'Giáng sinh & Năm mới');

-- ---------- Phòng: 5 tầng × 6 phòng ----------
INSERT INTO Phong (MaPhong, SoPhong, Tang, TrangThai, MaLoai) VALUES
('P101', '101', 1, N'Đã đặt',   'LP01'), ('P102', '102', 1, N'Đang ở',  'LP01'), ('P103', '103', 1, N'Trống',    'LP01'),
('P104', '104', 1, N'Trống',    'LP01'), ('P105', '105', 1, N'Đang dọn','LP01'), ('P106', '106', 1, N'Trống',    'LP01'),
('P201', '201', 2, N'Trống',    'LP02'), ('P202', '202', 2, N'Trống',   'LP02'), ('P203', '203', 2, N'Đang ở',   'LP02'),
('P204', '204', 2, N'Trống',    'LP02'), ('P205', '205', 2, N'Trống',   'LP01'), ('P206', '206', 2, N'Đang dọn', 'LP01'),
('P301', '301', 3, N'Trống',    'LP03'), ('P302', '302', 3, N'Trống',   'LP03'), ('P303', '303', 3, N'Trống',    'LP03'),
('P304', '304', 3, N'Trống',    'LP03'), ('P305', '305', 3, N'Trống',   'LP03'), ('P306', '306', 3, N'Bảo trì',  'LP03'),
('P401', '401', 4, N'Trống',    'LP04'), ('P402', '402', 4, N'Trống',   'LP04'), ('P403', '403', 4, N'Đang dọn', 'LP04'),
('P404', '404', 4, N'Trống',    'LP04'), ('P405', '405', 4, N'Trống',   'LP04'), ('P406', '406', 4, N'Trống',    'LP04'),
('P501', '501', 5, N'Trống',    'LP05'), ('P502', '502', 5, N'Trống',   'LP05'), ('P503', '503', 5, N'Trống',    'LP05'),
('P504', '504', 5, N'Bảo trì',  'LP04'), ('P505', '505', 5, N'Trống',   'LP04'), ('P506', '506', 5, N'Trống',    'LP04');

-- ---------- Khách hàng ----------
INSERT INTO KhachHang (MaKH, HoTen, CCCD_HoChieu, SDT, Email, DiaChi) VALUES
('KH001', N'Nguyễn Văn An',    '079201000101', '0903111001', N'nguyen.van.an@gmail.com',    N'Q.1, TP.HCM'),
('KH002', N'Trần Thị Bích',    '079301000102', '0903111002', N'tran.thi.bich@gmail.com',    N'Q.3, TP.HCM'),
('KH003', N'Lê Minh Châu',     '001201000103', '0903111003', N'le.minh.chau@gmail.com',     N'Hà Nội'),
('KH004', N'Phạm Quốc Dũng',   '048201000104', '0903111004', N'pham.quoc.dung@gmail.com',   N'Đà Nẵng'),
('KH005', N'Hoàng Thị Em',     '092301000105', '0903111005', N'hoang.thi.em@gmail.com',     N'Cần Thơ'),
('KH006', N'Vũ Đức Giang',     '031201000106', '0903111006', N'vu.duc.giang@gmail.com',     N'Hải Phòng'),
('KH007', N'Đỗ Thanh Hương',   '079301000107', '0903111007', N'do.thanh.huong@gmail.com',   N'Q.7, TP.HCM'),
('KH008', N'Bùi Văn Khoa',     '075201000108', '0903111008', N'bui.van.khoa@gmail.com',     N'Biên Hòa, Đồng Nai'),
('KH009', N'Ngô Thị Lan',      '046301000109', '0903111009', N'ngo.thi.lan@gmail.com',      N'Huế'),
('KH010', N'Dương Quang Minh', '056201000110', '0903111010', N'duong.quang.minh@gmail.com', N'Nha Trang'),
('KH011', N'Lý Thu Ngân',      '077301000111', '0903111011', N'ly.thu.ngan@gmail.com',      N'Vũng Tàu'),
('KH012', N'Trịnh Công Phú',   '079201000112', '0903111012', N'trinh.cong.phu@gmail.com',   N'Thủ Đức, TP.HCM'),
('KH013', N'John Smith',       'P5432101',     '+44 7700900123', N'john.smith@gmail.com',   N'London, UK'),
('KH014', N'Kim Min-jun',      'M1234567',     '+82 1012345678', N'kim.min-jun@gmail.com',  N'Seoul, Hàn Quốc');

-- ---------- Dịch vụ ----------
INSERT INTO DichVu (MaDV, TenDV, DonGia, DonViTinh, TrangThai) VALUES
('DV01', N'Ăn sáng buffet',  120000, N'suất',  N'Đang kinh doanh'),
('DV02', N'Giặt ủi',         35000,  N'kg',    N'Đang kinh doanh'),
('DV03', N'Nước suối',       15000,  N'chai',  N'Đang kinh doanh'),
('DV04', N'Bia lon',         30000,  N'lon',   N'Đang kinh doanh'),
('DV05', N'Đưa đón sân bay', 250000, N'lượt',  N'Đang kinh doanh'),
('DV06', N'Thuê xe máy',     150000, N'ngày',  N'Đang kinh doanh'),
('DV07', N'Spa & massage',   400000, N'giờ',   N'Đang kinh doanh'),
('DV08', N'Snack minibar',   25000,  N'gói',   N'Đang kinh doanh'),
('DV09', N'Trang trí phòng', 500000, N'lần',   N'Ngừng kinh doanh');

-- ---------- Một lượt lưu trú đã trả phòng, có hóa đơn và thanh toán ----------
INSERT INTO DatPhong (MaDat, NgayDat, NgayNhanDuKien, NgayTraDuKien, TrangThai, MaKH, MaPhong, GhiChu) VALUES
('DP001', DATEADD(DAY, -15, @HomNay), DATEADD(DAY, -10, @HomNay), DATEADD(DAY, -8, @HomNay), N'Hoàn tất', 'KH004', 'P301', N'');
INSERT INTO LuuTru (MaLuuTru, NgayNhan, NgayTra, GiaPhongApDung, TrangThai, MaPhong, MaDat) VALUES
('LT0001', DATEADD(HOUR, 14, CAST(DATEADD(DAY, -10, @HomNay) AS DATETIME2)), DATEADD(HOUR, 12, CAST(DATEADD(DAY, -8, @HomNay) AS DATETIME2)), 1100000, N'Đã trả phòng', 'P301', 'DP001');
INSERT INTO ChiTietLuuTru (MaCTLT, MaLuuTru, MaKH, VaiTro) VALUES ('CT0001', 'LT0001', 'KH004', N'Khách chính');
INSERT INTO SuDungDichVu (MaSD, MaLuuTru, MaDV, SoLuong, DonGiaApDung, ThoiGian) VALUES
('SD0001', 'LT0001', 'DV01', 2, 120000, DATEADD(HOUR, 7, CAST(DATEADD(DAY, -9, @HomNay) AS DATETIME2)));
INSERT INTO HoaDon (MaHD, MaLuuTru, NgayLap, TienPhong, TienDV, GiamGia, TongTien, TrangThai) VALUES
('HD0001', 'LT0001', DATEADD(HOUR, 12, CAST(DATEADD(DAY, -8, @HomNay) AS DATETIME2)), 2200000, 240000, 0, 2440000, N'Đã thanh toán');
INSERT INTO ThanhToan (MaTT, MaHD, PhuongThuc, SoTien, ThoiGian, TrangThai) VALUES
('TT0001', 'HD0001', N'Chuyển khoản', 2440000, DATEADD(MINUTE, 10, DATEADD(HOUR, 12, CAST(DATEADD(DAY, -8, @HomNay) AS DATETIME2))), N'Thành công');

-- ---------- Hai phòng đang có khách ----------
INSERT INTO DatPhong (MaDat, NgayDat, NgayNhanDuKien, NgayTraDuKien, TrangThai, MaKH, MaPhong, GhiChu) VALUES
('DP002', DATEADD(DAY, -7, @HomNay), DATEADD(DAY, -2, @HomNay), DATEADD(DAY, 1, @HomNay), N'Đã nhận phòng', 'KH001', 'P102', N'');
INSERT INTO LuuTru (MaLuuTru, NgayNhan, NgayTra, GiaPhongApDung, TrangThai, MaPhong, MaDat) VALUES
('LT0002', DATEADD(HOUR, 14, CAST(DATEADD(DAY, -2, @HomNay) AS DATETIME2)), DATEADD(HOUR, 12, CAST(DATEADD(DAY, 1, @HomNay) AS DATETIME2)), 500000, N'Đang ở', 'P102', 'DP002'),
('LT0003', DATEADD(HOUR, 20, CAST(DATEADD(DAY, -1, @HomNay) AS DATETIME2)), DATEADD(HOUR, 12, CAST(DATEADD(DAY, 2, @HomNay) AS DATETIME2)), 750000, N'Đang ở', 'P203', NULL);
INSERT INTO ChiTietLuuTru (MaCTLT, MaLuuTru, MaKH, VaiTro) VALUES
('CT0002', 'LT0002', 'KH001', N'Khách chính'),
('CT0003', 'LT0002', 'KH008', N'Khách đi cùng'),
('CT0004', 'LT0003', 'KH013', N'Khách chính');
INSERT INTO SuDungDichVu (MaSD, MaLuuTru, MaDV, SoLuong, DonGiaApDung, ThoiGian) VALUES
('SD0002', 'LT0002', 'DV03', 2, 15000, DATEADD(HOUR, 21, CAST(DATEADD(DAY, -2, @HomNay) AS DATETIME2))),
('SD0003', 'LT0003', 'DV05', 1, 250000, DATEADD(HOUR, 19, CAST(DATEADD(DAY, -1, @HomNay) AS DATETIME2)));

-- ---------- Đặt phòng sắp tới (P101 nhận hôm nay nên đang "Đã đặt") ----------
INSERT INTO DatPhong (MaDat, NgayDat, NgayNhanDuKien, NgayTraDuKien, TrangThai, MaKH, MaPhong, GhiChu) VALUES
('DP003', DATEADD(DAY, -5, @HomNay), @HomNay,                  DATEADD(DAY, 2, @HomNay), N'Chờ nhận phòng', 'KH003', 'P101', N'Khách đến khoảng 14h'),
('DP004', DATEADD(DAY, -3, @HomNay), DATEADD(DAY, 1, @HomNay), DATEADD(DAY, 3, @HomNay), N'Chờ nhận phòng', 'KH009', 'P401', N''),
('DP005', DATEADD(DAY, -1, @HomNay), DATEADD(DAY, 3, @HomNay), DATEADD(DAY, 7, @HomNay), N'Chờ nhận phòng', 'KH012', 'P304', N''),
('DP006', DATEADD(DAY, -6, @HomNay), DATEADD(DAY, 1, @HomNay), DATEADD(DAY, 3, @HomNay), N'Đã hủy',         'KH014', 'P302', N'Khách đổi lịch trình');

COMMIT;
PRINT N'Đã nạp dữ liệu mẫu.';
GO
