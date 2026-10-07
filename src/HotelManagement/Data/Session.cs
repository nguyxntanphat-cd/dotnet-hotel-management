using HotelManagement.Models;

namespace HotelManagement.Data;

public static class Session
{
    public static TaiKhoan? TaiKhoan { get; private set; }
    public static NhanVien? NhanVien => HotelService.NV(TaiKhoan?.MaNV);
    public static string VaiTro => TaiKhoan?.VaiTro ?? "";

    /// <summary>1.1 Đăng nhập. Trả về thông báo lỗi, hoặc null nếu thành công.</summary>
    public static string? DangNhap(string tenDangNhap, string matKhau)
    {
        if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrEmpty(matKhau))
            return "Vui lòng nhập tên đăng nhập và mật khẩu.";
        var tk = MockDb.TaiKhoans.FirstOrDefault(t => t.TenDangNhap.Equals(tenDangNhap.Trim(), StringComparison.OrdinalIgnoreCase));
        if (tk == null || tk.MatKhau != matKhau) return "Tên đăng nhập hoặc mật khẩu không đúng.";
        if (tk.TrangThai == TrangThaiTaiKhoan.DaKhoa) return "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên.";
        TaiKhoan = tk;
        return null;
    }

    /// <summary>1.2 Đăng xuất.</summary>
    public static void DangXuat() => TaiKhoan = null;

    public static bool CoQuyen(string maChucNang) =>
        MockDb.PhanQuyen.TryGetValue(VaiTro, out var q) && q.Contains(maChucNang);
}
