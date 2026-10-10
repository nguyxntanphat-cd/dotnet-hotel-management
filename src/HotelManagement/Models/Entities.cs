namespace HotelManagement.Models;

// 13 lớp dữ liệu theo mục 5 của đặc tả. LuuTru là lớp trung tâm.

public class TaiKhoan
{
    public string MaTK { get; set; } = "";
    public string TenDangNhap { get; set; } = "";
    public string MatKhau { get; set; } = "";
    public string VaiTro { get; set; } = "";
    public string TrangThai { get; set; } = TrangThaiTaiKhoan.HoatDong;
    public string? MaNV { get; set; }
}

public class NhanVien
{
    public string MaNV { get; set; } = "";
    public string HoTen { get; set; } = "";
    public DateTime NgaySinh { get; set; }
    public string SDT { get; set; } = "";
    public string Email { get; set; } = "";
    public string ChucVu { get; set; } = "";
}

public class LoaiPhong
{
    public string MaLoai { get; set; } = "";
    public string TenLoai { get; set; } = "";
    public decimal GiaCoBan { get; set; }
    public int SucChua { get; set; }
    public string MoTa { get; set; } = "";
}

public class BangGia
{
    public string MaGia { get; set; } = "";
    public string MaLoai { get; set; } = "";
    public DateTime TuNgay { get; set; }
    public DateTime DenNgay { get; set; }
    public decimal GiaApDung { get; set; }
    public string GhiChu { get; set; } = "";
}

public class Phong
{
    public string MaPhong { get; set; } = "";
    public string SoPhong { get; set; } = "";
    public int Tang { get; set; }
    public string TrangThai { get; set; } = TrangThaiPhong.Trong;
    public string MaLoai { get; set; } = "";
}

public class KhachHang
{
    public string MaKH { get; set; } = "";
    public string HoTen { get; set; } = "";
    public string CCCD_HoChieu { get; set; } = "";
    public string SDT { get; set; } = "";
    public string Email { get; set; } = "";
    public string DiaChi { get; set; } = "";
}

public class DatPhong
{
    public string MaDat { get; set; } = "";
    public DateTime NgayDat { get; set; }
    public DateTime NgayNhanDuKien { get; set; }
    public DateTime NgayTraDuKien { get; set; }
    public string TrangThai { get; set; } = TrangThaiDatPhong.ChoNhanPhong;
    public string MaKH { get; set; } = "";
    public string MaPhong { get; set; } = "";
    public string GhiChu { get; set; } = "";
}

public class LuuTru
{
    public string MaLuuTru { get; set; } = "";
    public DateTime NgayNhan { get; set; }
    /// <summary>Ngày trả dự kiến khi đang ở, ngày trả thực tế khi đã trả phòng.</summary>
    public DateTime NgayTra { get; set; }
    public decimal GiaPhongApDung { get; set; }
    public string TrangThai { get; set; } = TrangThaiLuuTru.DangO;
    public string MaPhong { get; set; } = "";
    public string? MaDat { get; set; }
}

public class ChiTietLuuTru
{
    public string MaCTLT { get; set; } = "";
    public string MaLuuTru { get; set; } = "";
    public string MaKH { get; set; } = "";
    public string VaiTro { get; set; } = VaiTroLuuTru.KhachChinh;
    public string GhiChu { get; set; } = "";
}

public class DichVu
{
    public string MaDV { get; set; } = "";
    public string TenDV { get; set; } = "";
    public decimal DonGia { get; set; }
    public string DonViTinh { get; set; } = "";
    public string TrangThai { get; set; } = TrangThaiDichVu.DangKinhDoanh;
}

public class SuDungDichVu
{
    public string MaSD { get; set; } = "";
    public string MaLuuTru { get; set; } = "";
    public string MaDV { get; set; } = "";
    public int SoLuong { get; set; }
    public decimal DonGiaApDung { get; set; }
    public decimal ThanhTien => SoLuong * DonGiaApDung;
    public DateTime ThoiGian { get; set; }
}

public class HoaDon
{
    public string MaHD { get; set; } = "";
    public string MaLuuTru { get; set; } = "";
    public DateTime NgayLap { get; set; }
    public decimal TienPhong { get; set; }
    public decimal TienDV { get; set; }
    public decimal GiamGia { get; set; }
    public decimal TongTien { get; set; }
    public string TrangThai { get; set; } = TrangThaiHoaDon.ChuaThanhToan;
}

public class ThanhToan
{
    public string MaTT { get; set; } = "";
    public string MaHD { get; set; } = "";
    public string PhuongThuc { get; set; } = "";
    public decimal SoTien { get; set; }
    public DateTime ThoiGian { get; set; }
    public string TrangThai { get; set; } = TrangThaiThanhToan.ThanhCong;
}
