using HotelManagement.Models;
using static HotelManagement.Data.MockDb;

namespace HotelManagement.Data;

/// <summary>Nghiệp vụ chính: tra cứu, đặt phòng, nhận/trả phòng, tính tiền, hóa đơn, thanh toán.</summary>
public static class HotelService
{
    // ---------- Tra cứu nhanh ----------
    public static KhachHang? Khach(string? maKH) => KhachHangs.FirstOrDefault(k => k.MaKH == maKH);
    public static Phong? PhongTheoMa(string? maPhong) => Phongs.FirstOrDefault(p => p.MaPhong == maPhong);
    public static LoaiPhong? Loai(string? maLoai) => LoaiPhongs.FirstOrDefault(l => l.MaLoai == maLoai);
    public static DichVu? DV(string? maDV) => DichVus.FirstOrDefault(d => d.MaDV == maDV);
    public static NhanVien? NV(string? maNV) => NhanViens.FirstOrDefault(n => n.MaNV == maNV);
    public static LuuTru? LT(string? ma) => LuuTrus.FirstOrDefault(l => l.MaLuuTru == ma);

    public static KhachHang? KhachChinh(LuuTru lt) =>
        Khach(ChiTietLuuTrus.Where(c => c.MaLuuTru == lt.MaLuuTru).OrderBy(c => c.VaiTro == "Khách chính" ? 0 : 1).FirstOrDefault()?.MaKH);

    public static LuuTru? LuuTruDangO(string maPhong) =>
        LuuTrus.FirstOrDefault(l => l.MaPhong == maPhong && l.TrangThai == TrangThaiLuuTru.DangO);

    public static HoaDon? HoaDonCua(LuuTru lt) => HoaDons.FirstOrDefault(h => h.MaLuuTru == lt.MaLuuTru);

    // ---------- Giá phòng (2.1 bảng giá) ----------
    public static decimal GiaPhong(string maLoai, DateTime ngay)
    {
        var bg = BangGias.FirstOrDefault(b => b.MaLoai == maLoai && ngay.Date >= b.TuNgay.Date && ngay.Date <= b.DenNgay.Date);
        return bg?.GiaApDung ?? Loai(maLoai)?.GiaCoBan ?? 0;
    }

    // ---------- Kiểm tra trùng phòng (3.2, 3.3) ----------
    public static bool TrungLich(string maPhong, DateTime tu, DateTime den, string? boQuaMaDat = null)
    {
        bool overlap(DateTime a1, DateTime a2) => tu.Date < a2.Date && a1.Date < den.Date;

        var trungDat = DatPhongs.Any(d => d.MaPhong == maPhong && d.MaDat != boQuaMaDat
            && d.TrangThai == TrangThaiDatPhong.ChoNhanPhong && overlap(d.NgayNhanDuKien, d.NgayTraDuKien));
        var trungO = LuuTrus.Any(l => l.MaPhong == maPhong && l.TrangThai == TrangThaiLuuTru.DangO
            && (boQuaMaDat == null || l.MaDat != boQuaMaDat) && overlap(l.NgayNhan, l.NgayTra));
        return trungDat || trungO;
    }

    /// <summary>2.4 Tra cứu phòng trống theo khoảng ngày và loại phòng.</summary>
    public static List<Phong> PhongTrong(DateTime tu, DateTime den, string? maLoai = null, string? boQuaMaDat = null) =>
        Phongs.Where(p => p.TrangThai != TrangThaiPhong.BaoTri
                       && (maLoai == null || p.MaLoai == maLoai)
                       && !TrungLich(p.MaPhong, tu, den, boQuaMaDat))
              .OrderBy(p => p.SoPhong).ToList();

    public static int SoDem(DateTime nhan, DateTime tra) => Math.Max(1, (tra.Date - nhan.Date).Days);

    // ---------- Nhận phòng (4.1) ----------
    public static LuuTru NhanPhong(string maPhong, string maKhachChinh, DateTime ngayTraDuKien, DatPhong? dp, IEnumerable<string> khachDiCung)
    {
        var phong = PhongTheoMa(maPhong)!;
        var lt = new LuuTru
        {
            MaLuuTru = NextId(LuuTrus, l => l.MaLuuTru, "LT", 4),
            NgayNhan = DateTime.Now,
            NgayTra = ngayTraDuKien.Date.AddHours(12),
            GiaPhongApDung = GiaPhong(phong.MaLoai, DateTime.Today),
            MaPhong = maPhong,
            MaDat = dp?.MaDat,
        };
        LuuTrus.Add(lt);
        ChiTietLuuTrus.Add(new ChiTietLuuTru { MaCTLT = NextId(ChiTietLuuTrus, c => c.MaCTLT, "CT", 4), MaLuuTru = lt.MaLuuTru, MaKH = maKhachChinh, VaiTro = "Khách chính" });
        foreach (var ma in khachDiCung.Where(m => m != maKhachChinh).Distinct())
            ChiTietLuuTrus.Add(new ChiTietLuuTru { MaCTLT = NextId(ChiTietLuuTrus, c => c.MaCTLT, "CT", 4), MaLuuTru = lt.MaLuuTru, MaKH = ma, VaiTro = "Khách đi cùng" });

        if (dp != null) dp.TrangThai = TrangThaiDatPhong.DaNhanPhong;
        phong.TrangThai = TrangThaiPhong.DangO;
        return lt;
    }

    // ---------- Tính tiền (5.2, 5.3) ----------
    public static decimal TinhTienPhong(LuuTru lt, DateTime ngayTra) => SoDem(lt.NgayNhan, ngayTra) * lt.GiaPhongApDung;

    public static decimal TinhTienDichVu(LuuTru lt) =>
        SuDungDichVus.Where(s => s.MaLuuTru == lt.MaLuuTru).Sum(s => s.ThanhTien);

    // ---------- Lập hóa đơn (5.4) ----------
    public static HoaDon LapHoaDon(LuuTru lt, decimal giamGia, DateTime ngayTra)
    {
        var hd = HoaDonCua(lt);
        if (hd == null)
        {
            hd = new HoaDon { MaHD = NextId(HoaDons, h => h.MaHD, "HD", 4), MaLuuTru = lt.MaLuuTru };
            HoaDons.Add(hd);
        }
        hd.NgayLap = ngayTra;
        hd.TienPhong = TinhTienPhong(lt, ngayTra);
        hd.TienDV = TinhTienDichVu(lt);
        hd.GiamGia = Math.Min(giamGia, hd.TienPhong + hd.TienDV);
        hd.TongTien = hd.TienPhong + hd.TienDV - hd.GiamGia;
        return hd;
    }

    public static decimal DaThanhToan(HoaDon hd) => ThanhToans.Where(t => t.MaHD == hd.MaHD).Sum(t => t.SoTien);

    // ---------- Ghi nhận thanh toán (5.5) + hoàn tất trả phòng (5.1) ----------
    public static ThanhToan GhiNhanThanhToan(HoaDon hd, string phuongThuc, decimal soTien)
    {
        var tt = new ThanhToan
        {
            MaTT = NextId(ThanhToans, t => t.MaTT, "TT", 4), MaHD = hd.MaHD,
            PhuongThuc = phuongThuc, SoTien = soTien, ThoiGian = DateTime.Now,
        };
        ThanhToans.Add(tt);

        var daTra = DaThanhToan(hd);
        hd.TrangThai = daTra >= hd.TongTien ? TrangThaiHoaDon.DaThanhToan
                     : daTra > 0 ? TrangThaiHoaDon.ThanhToanMotPhan : TrangThaiHoaDon.ChuaThanhToan;

        if (hd.TrangThai == TrangThaiHoaDon.DaThanhToan)
        {
            var lt = LT(hd.MaLuuTru)!;
            if (lt.TrangThai == TrangThaiLuuTru.DangO)
            {
                lt.TrangThai = TrangThaiLuuTru.DaTraPhong;
                lt.NgayTra = hd.NgayLap;
                var phong = PhongTheoMa(lt.MaPhong);
                if (phong != null) phong.TrangThai = TrangThaiPhong.DangDon;
                var dp = DatPhongs.FirstOrDefault(d => d.MaDat == lt.MaDat);
                if (dp != null) dp.TrangThai = TrangThaiDatPhong.HoanTat;
            }
        }
        return tt;
    }

    // ---------- Doanh thu (6.3) ----------
    /// <summary>Doanh thu được ghi nhận theo ngày lập hóa đơn đã thanh toán.</summary>
    public static IEnumerable<HoaDon> HoaDonDaThu(DateTime tu, DateTime den) =>
        HoaDons.Where(h => h.TrangThai == TrangThaiHoaDon.DaThanhToan && h.NgayLap.Date >= tu.Date && h.NgayLap.Date <= den.Date);
}
