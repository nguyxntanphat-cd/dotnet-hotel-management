using HotelManagement.Models;

namespace HotelManagement.Data;

/// <summary>
/// Cơ sở dữ liệu giả lập trong bộ nhớ, dùng cho bản demo giao diện.
/// Khi làm hệ thống chính, thay lớp này bằng tầng truy cập SQL Server (ADO.NET / EF Core).
/// </summary>
public static class MockDb
{
    public static List<TaiKhoan> TaiKhoans { get; } = new();
    public static List<NhanVien> NhanViens { get; } = new();
    public static List<LoaiPhong> LoaiPhongs { get; } = new();
    public static List<BangGia> BangGias { get; } = new();
    public static List<Phong> Phongs { get; } = new();
    public static List<KhachHang> KhachHangs { get; } = new();
    public static List<DatPhong> DatPhongs { get; } = new();
    public static List<LuuTru> LuuTrus { get; } = new();
    public static List<ChiTietLuuTru> ChiTietLuuTrus { get; } = new();
    public static List<DichVu> DichVus { get; } = new();
    public static List<SuDungDichVu> SuDungDichVus { get; } = new();
    public static List<HoaDon> HoaDons { get; } = new();
    public static List<ThanhToan> ThanhToans { get; } = new();

    /// <summary>Phân quyền: vai trò → tập mã chức năng được dùng (xem <see cref="Menu"/>).</summary>
    public static Dictionary<string, HashSet<string>> PhanQuyen { get; } = new();

    public static string NextId<T>(IEnumerable<T> list, Func<T, string> key, string prefix, int width = 3)
    {
        var max = list.Select(key)
            .Where(k => k.StartsWith(prefix))
            .Select(k => int.TryParse(k[prefix.Length..], out var n) ? n : 0)
            .DefaultIfEmpty(0).Max();
        return prefix + (max + 1).ToString().PadLeft(width, '0');
    }

    static MockDb() => Seed();

    private static void Seed()
    {
        var today = DateTime.Today;
        var rnd = new Random(2026);

        // ---- Nhân viên & tài khoản ----
        NhanViens.AddRange(new[]
        {
            new NhanVien { MaNV = "NV001", HoTen = "Nguyễn Tấn Phát", NgaySinh = new DateTime(1990, 3, 12), SDT = "0901234567", Email = "phat.nt@hotel.vn", ChucVu = "Quản trị viên" },
            new NhanVien { MaNV = "NV002", HoTen = "Trần Thị Mai", NgaySinh = new DateTime(1985, 7, 4), SDT = "0912345678", Email = "mai.tt@hotel.vn", ChucVu = "Quản lý" },
            new NhanVien { MaNV = "NV003", HoTen = "Lê Hoàng Nam", NgaySinh = new DateTime(1998, 11, 20), SDT = "0987654321", Email = "nam.lh@hotel.vn", ChucVu = "Lễ tân" },
            new NhanVien { MaNV = "NV004", HoTen = "Phạm Ngọc Anh", NgaySinh = new DateTime(2000, 1, 15), SDT = "0934567890", Email = "anh.pn@hotel.vn", ChucVu = "Lễ tân" },
            new NhanVien { MaNV = "NV005", HoTen = "Võ Văn Hùng", NgaySinh = new DateTime(1992, 5, 30), SDT = "0978123456", Email = "hung.vv@hotel.vn", ChucVu = "Nhân viên buồng phòng" },
            new NhanVien { MaNV = "NV006", HoTen = "Đặng Thu Hà", NgaySinh = new DateTime(1995, 9, 9), SDT = "0965432187", Email = "ha.dt@hotel.vn", ChucVu = "Nhân viên buồng phòng" },
            new NhanVien { MaNV = "NV007", HoTen = "Bùi Minh Tuấn", NgaySinh = new DateTime(1997, 2, 18), SDT = "0944556677", Email = "tuan.bm@hotel.vn", ChucVu = "Lễ tân" },
        });
        TaiKhoans.AddRange(new[]
        {
            new TaiKhoan { MaTK = "TK001", TenDangNhap = "admin", MatKhau = "123", VaiTro = VaiTro.QuanTriVien, MaNV = "NV001" },
            new TaiKhoan { MaTK = "TK002", TenDangNhap = "quanly", MatKhau = "123", VaiTro = VaiTro.QuanLy, MaNV = "NV002" },
            new TaiKhoan { MaTK = "TK003", TenDangNhap = "letan", MatKhau = "123", VaiTro = VaiTro.LeTan, MaNV = "NV003" },
            new TaiKhoan { MaTK = "TK004", TenDangNhap = "letan2", MatKhau = "123", VaiTro = VaiTro.LeTan, MaNV = "NV004", TrangThai = TrangThaiTaiKhoan.DaKhoa },
            new TaiKhoan { MaTK = "TK005", TenDangNhap = "buongphong", MatKhau = "123", VaiTro = VaiTro.BuongPhong, MaNV = "NV005" },
        });

        // ---- Phân quyền mặc định theo mục 3 của đặc tả ----
        PhanQuyen[VaiTro.QuanTriVien] = Menu.Items.Select(i => i.Key).ToHashSet();
        PhanQuyen[VaiTro.QuanLy] = new() { "tong_quan", "phong", "dat_phong", "luu_tru", "hoa_don", "thong_ke" };
        PhanQuyen[VaiTro.LeTan] = new() { "tong_quan", "phong", "khach_hang", "dat_phong", "nhan_phong", "luu_tru", "su_dung_dv", "tra_phong", "hoa_don" };
        PhanQuyen[VaiTro.BuongPhong] = new() { "phong" };

        // ---- Loại phòng, bảng giá ----
        LoaiPhongs.AddRange(new[]
        {
            new LoaiPhong { MaLoai = "LP01", TenLoai = "Standard", GiaCoBan = 500_000, SucChua = 2, MoTa = "Phòng tiêu chuẩn, 1 giường đôi, 20m²" },
            new LoaiPhong { MaLoai = "LP02", TenLoai = "Superior", GiaCoBan = 750_000, SucChua = 2, MoTa = "2 giường đơn, cửa sổ hướng phố, 25m²" },
            new LoaiPhong { MaLoai = "LP03", TenLoai = "Deluxe", GiaCoBan = 1_100_000, SucChua = 3, MoTa = "Giường King, ban công, bồn tắm, 32m²" },
            new LoaiPhong { MaLoai = "LP04", TenLoai = "Family", GiaCoBan = 1_500_000, SucChua = 4, MoTa = "2 giường đôi, phù hợp gia đình, 40m²" },
            new LoaiPhong { MaLoai = "LP05", TenLoai = "Suite", GiaCoBan = 2_500_000, SucChua = 4, MoTa = "Phòng khách riêng, view toàn cảnh, 60m²" },
        });
        var tet = new DateTime(today.Year + (today.Month > 2 ? 1 : 0), 1, 25);
        BangGias.AddRange(new[]
        {
            new BangGia { MaGia = "BG001", MaLoai = "LP03", TuNgay = tet, DenNgay = tet.AddDays(10), GiaApDung = 1_450_000, GhiChu = "Tết Nguyên đán" },
            new BangGia { MaGia = "BG002", MaLoai = "LP04", TuNgay = tet, DenNgay = tet.AddDays(10), GiaApDung = 1_950_000, GhiChu = "Tết Nguyên đán" },
            new BangGia { MaGia = "BG003", MaLoai = "LP05", TuNgay = tet, DenNgay = tet.AddDays(10), GiaApDung = 3_200_000, GhiChu = "Tết Nguyên đán" },
            new BangGia { MaGia = "BG004", MaLoai = "LP01", TuNgay = new DateTime(today.Year, 12, 24), DenNgay = new DateTime(today.Year + 1, 1, 2), GiaApDung = 650_000, GhiChu = "Giáng sinh & Năm mới" },
        });

        // ---- Phòng: 5 tầng × 6 phòng ----
        string[] loaiTheoTang = { "", "LP01", "LP02", "LP03", "LP04", "LP05" };
        for (int tang = 1; tang <= 5; tang++)
        {
            for (int i = 1; i <= 6; i++)
            {
                var loai = loaiTheoTang[tang];
                if (tang == 2 && i >= 5) loai = "LP01";
                if (tang == 5 && i >= 4) loai = "LP04";
                var so = $"{tang}{i:00}";
                Phongs.Add(new Phong { MaPhong = "P" + so, SoPhong = so, Tang = tang, MaLoai = loai });
            }
        }

        // ---- Khách hàng ----
        var hoTen = new[]
        {
            ("Nguyễn Văn An", "Q.1, TP.HCM"), ("Trần Thị Bích", "Q.3, TP.HCM"), ("Lê Minh Châu", "Hà Nội"),
            ("Phạm Quốc Dũng", "Đà Nẵng"), ("Hoàng Thị Em", "Cần Thơ"), ("Vũ Đức Giang", "Hải Phòng"),
            ("Đỗ Thanh Hương", "Q.7, TP.HCM"), ("Bùi Văn Khoa", "Biên Hòa, Đồng Nai"), ("Ngô Thị Lan", "Huế"),
            ("Dương Quang Minh", "Nha Trang"), ("Lý Thu Ngân", "Vũng Tàu"), ("Trịnh Công Phú", "Thủ Đức, TP.HCM"),
            ("John Smith", "London, UK"), ("Kim Min-jun", "Seoul, Hàn Quốc"), ("Tạ Hồng Quân", "Bình Dương"),
            ("Mai Anh Thư", "Q.10, TP.HCM"), ("Châu Gia Bảo", "Long An"), ("Hồ Ngọc Diệp", "Đà Lạt"),
        };
        for (int i = 0; i < hoTen.Length; i++)
        {
            var (ten, dc) = hoTen[i];
            var nuocNgoai = ten is "John Smith" or "Kim Min-jun";
            KhachHangs.Add(new KhachHang
            {
                MaKH = $"KH{i + 1:000}",
                HoTen = ten,
                CCCD_HoChieu = nuocNgoai ? $"P{rnd.Next(1000000, 9999999)}" : $"0792{rnd.Next(10000000, 99999999)}",
                SDT = nuocNgoai ? $"+82 10{rnd.Next(1000000, 9999999)}" : $"09{rnd.Next(10000000, 99999999)}",
                Email = $"{RemoveDiacritics(ten).ToLower().Replace(" ", ".")}@gmail.com",
                DiaChi = dc,
            });
        }

        // ---- Dịch vụ ----
        DichVus.AddRange(new[]
        {
            new DichVu { MaDV = "DV01", TenDV = "Ăn sáng buffet", DonGia = 120_000, DonViTinh = "suất" },
            new DichVu { MaDV = "DV02", TenDV = "Giặt ủi", DonGia = 35_000, DonViTinh = "kg" },
            new DichVu { MaDV = "DV03", TenDV = "Nước suối", DonGia = 15_000, DonViTinh = "chai" },
            new DichVu { MaDV = "DV04", TenDV = "Bia lon", DonGia = 30_000, DonViTinh = "lon" },
            new DichVu { MaDV = "DV05", TenDV = "Đưa đón sân bay", DonGia = 250_000, DonViTinh = "lượt" },
            new DichVu { MaDV = "DV06", TenDV = "Thuê xe máy", DonGia = 150_000, DonViTinh = "ngày" },
            new DichVu { MaDV = "DV07", TenDV = "Spa & massage", DonGia = 400_000, DonViTinh = "giờ" },
            new DichVu { MaDV = "DV08", TenDV = "Snack minibar", DonGia = 25_000, DonViTinh = "gói" },
            new DichVu { MaDV = "DV09", TenDV = "Trang trí phòng", DonGia = 500_000, DonViTinh = "lần", TrangThai = TrangThaiDichVu.NgungKinhDoanh },
        });
        var dvDangBan = DichVus.Where(d => d.TrangThai == TrangThaiDichVu.DangKinhDoanh).ToList();

        // ---- Lịch sử 6 tháng: lưu trú đã trả phòng + hóa đơn đã thanh toán ----
        for (int i = 0; i < 900; i++)
        {
            var phong = Phongs[rnd.Next(Phongs.Count)];
            var kh = KhachHangs[rnd.Next(KhachHangs.Count)];
            var nhan = today.AddDays(-rnd.Next(3, 182));
            var soDem = rnd.Next(1, 5);
            var lt = TaoLuuTruLichSu(phong, kh, nhan, nhan.AddDays(soDem), rnd, dvDangBan);
            var hd = HotelService.LapHoaDon(lt, rnd.Next(10) == 0 ? 100_000 : 0, lt.NgayTra);
            ThanhToans.Add(new ThanhToan
            {
                MaTT = NextId(ThanhToans, t => t.MaTT, "TT", 4),
                MaHD = hd.MaHD,
                PhuongThuc = PhuongThucThanhToan.TatCa[rnd.Next(PhuongThucThanhToan.TatCa.Length)],
                SoTien = hd.TongTien,
                ThoiGian = lt.NgayTra.AddHours(11).AddMinutes(rnd.Next(60)),
            });
            hd.TrangThai = TrangThaiHoaDon.DaThanhToan;
        }

        // ---- Khách đang lưu trú ----
        var dangO = new (string phong, int kh, int daO, int conLai)[]
        {
            ("P102", 0, 2, 1), ("P104", 3, 1, 2), ("P201", 6, 3, 1), ("P203", 12, 0, 3),
            ("P302", 1, 1, 0), ("P305", 13, 4, 2), ("P402", 4, 2, 2), ("P501", 9, 1, 3),
        };
        foreach (var (maPhong, khIdx, daO, conLai) in dangO)
        {
            var phong = Phongs.First(p => p.MaPhong == maPhong);
            var kh = KhachHangs[khIdx];
            var dp = new DatPhong
            {
                MaDat = NextId(DatPhongs, d => d.MaDat, "DP"),
                NgayDat = today.AddDays(-daO - rnd.Next(2, 15)),
                NgayNhanDuKien = today.AddDays(-daO),
                NgayTraDuKien = today.AddDays(conLai),
                TrangThai = TrangThaiDatPhong.DaNhanPhong,
                MaKH = kh.MaKH,
                MaPhong = maPhong,
            };
            DatPhongs.Add(dp);
            var lt = new LuuTru
            {
                MaLuuTru = NextId(LuuTrus, l => l.MaLuuTru, "LT", 4),
                NgayNhan = dp.NgayNhanDuKien.AddHours(14),
                NgayTra = dp.NgayTraDuKien.AddHours(12),
                GiaPhongApDung = HotelService.GiaPhong(phong.MaLoai, dp.NgayNhanDuKien),
                MaPhong = maPhong,
                MaDat = dp.MaDat,
            };
            LuuTrus.Add(lt);
            ChiTietLuuTrus.Add(new ChiTietLuuTru { MaCTLT = NextId(ChiTietLuuTrus, c => c.MaCTLT, "CT", 4), MaLuuTru = lt.MaLuuTru, MaKH = kh.MaKH, VaiTro = "Khách chính" });
            if (khIdx % 3 == 0)
            {
                var kh2 = KhachHangs[(khIdx + 7) % KhachHangs.Count];
                ChiTietLuuTrus.Add(new ChiTietLuuTru { MaCTLT = NextId(ChiTietLuuTrus, c => c.MaCTLT, "CT", 4), MaLuuTru = lt.MaLuuTru, MaKH = kh2.MaKH, VaiTro = "Khách đi cùng" });
            }
            for (int d = 0; d <= daO; d++)
            {
                if (rnd.Next(3) == 0) continue;
                var dv = dvDangBan[rnd.Next(dvDangBan.Count)];
                SuDungDichVus.Add(new SuDungDichVu
                {
                    MaSD = NextId(SuDungDichVus, s => s.MaSD, "SD", 4),
                    MaLuuTru = lt.MaLuuTru, MaDV = dv.MaDV, SoLuong = rnd.Next(1, 4),
                    DonGiaApDung = dv.DonGia, ThoiGian = lt.NgayNhan.Date.AddDays(d).AddHours(rnd.Next(8, 21)),
                });
            }
            phong.TrangThai = TrangThaiPhong.DangO;
        }

        // ---- Đặt phòng sắp tới ----
        var sapToi = new (string phong, int kh, int sau, int dem)[]
        {
            ("P101", 2, 0, 2), ("P204", 5, 0, 1), ("P303", 7, 0, 3), ("P401", 8, 1, 2),
            ("P106", 10, 2, 1), ("P304", 11, 3, 4), ("P502", 14, 5, 2), ("P205", 15, 7, 3),
        };
        foreach (var (maPhong, khIdx, sau, dem) in sapToi)
        {
            DatPhongs.Add(new DatPhong
            {
                MaDat = NextId(DatPhongs, d => d.MaDat, "DP"),
                NgayDat = today.AddDays(-rnd.Next(1, 20)),
                NgayNhanDuKien = today.AddDays(sau),
                NgayTraDuKien = today.AddDays(sau + dem),
                MaKH = KhachHangs[khIdx].MaKH,
                MaPhong = maPhong,
                GhiChu = sau == 0 ? "Khách đến khoảng 14h" : "",
            });
            if (sau == 0) Phongs.First(p => p.MaPhong == maPhong).TrangThai = TrangThaiPhong.DaDat;
        }
        DatPhongs.Add(new DatPhong
        {
            MaDat = NextId(DatPhongs, d => d.MaDat, "DP"), NgayDat = today.AddDays(-6),
            NgayNhanDuKien = today.AddDays(1), NgayTraDuKien = today.AddDays(3),
            TrangThai = TrangThaiDatPhong.DaHuy, MaKH = KhachHangs[16].MaKH, MaPhong = "P301", GhiChu = "Khách đổi lịch trình",
        });

        // ---- Trạng thái buồng phòng ----
        foreach (var (ma, tt) in new[] { ("P105", TrangThaiPhong.DangDon), ("P206", TrangThaiPhong.DangDon), ("P403", TrangThaiPhong.DangDon), ("P306", TrangThaiPhong.BaoTri), ("P504", TrangThaiPhong.BaoTri) })
            Phongs.First(p => p.MaPhong == ma).TrangThai = tt;
    }

    private static LuuTru TaoLuuTruLichSu(Phong phong, KhachHang kh, DateTime nhan, DateTime tra, Random rnd, List<DichVu> dvs)
    {
        var dp = new DatPhong
        {
            MaDat = NextId(DatPhongs, d => d.MaDat, "DP"), NgayDat = nhan.AddDays(-rnd.Next(1, 20)),
            NgayNhanDuKien = nhan, NgayTraDuKien = tra, TrangThai = TrangThaiDatPhong.HoanTat, MaKH = kh.MaKH, MaPhong = phong.MaPhong,
        };
        DatPhongs.Add(dp);
        var lt = new LuuTru
        {
            MaLuuTru = NextId(LuuTrus, l => l.MaLuuTru, "LT", 4),
            NgayNhan = nhan.AddHours(14), NgayTra = tra.AddHours(12),
            GiaPhongApDung = HotelService.GiaPhong(phong.MaLoai, nhan),
            TrangThai = TrangThaiLuuTru.DaTraPhong, MaPhong = phong.MaPhong, MaDat = dp.MaDat,
        };
        LuuTrus.Add(lt);
        ChiTietLuuTrus.Add(new ChiTietLuuTru { MaCTLT = NextId(ChiTietLuuTrus, c => c.MaCTLT, "CT", 4), MaLuuTru = lt.MaLuuTru, MaKH = kh.MaKH });
        int n = rnd.Next(0, 4);
        for (int i = 0; i < n; i++)
        {
            var dv = dvs[rnd.Next(dvs.Count)];
            SuDungDichVus.Add(new SuDungDichVu
            {
                MaSD = NextId(SuDungDichVus, s => s.MaSD, "SD", 4), MaLuuTru = lt.MaLuuTru, MaDV = dv.MaDV,
                SoLuong = rnd.Next(1, 4), DonGiaApDung = dv.DonGia, ThoiGian = nhan.AddHours(rnd.Next(15, 40)),
            });
        }
        return lt;
    }

    public static string RemoveDiacritics(string s)
    {
        var norm = s.Normalize(System.Text.NormalizationForm.FormD);
        var chars = norm.Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark);
        return new string(chars.ToArray()).Normalize(System.Text.NormalizationForm.FormC).Replace('đ', 'd').Replace('Đ', 'D');
    }
}
