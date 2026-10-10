using HotelManagement.Models;

namespace HotelManagement.UI;

public static class Theme
{
    public static readonly Color Primary = Color.FromArgb(29, 78, 216);
    public static readonly Color PrimaryHover = Color.FromArgb(30, 64, 175);
    public static readonly Color PrimarySoft = Color.FromArgb(219, 234, 254);
    public static readonly Color Sidebar = Color.FromArgb(15, 23, 42);
    public static readonly Color SidebarHover = Color.FromArgb(30, 41, 59);
    public static readonly Color SidebarText = Color.FromArgb(203, 213, 225);
    public static readonly Color SidebarMuted = Color.FromArgb(100, 116, 139);
    public static readonly Color Background = Color.FromArgb(241, 245, 249);
    public static readonly Color Surface = Color.White;
    public static readonly Color Border = Color.FromArgb(226, 232, 240);
    public static readonly Color Text = Color.FromArgb(15, 23, 42);
    public static readonly Color Muted = Color.FromArgb(100, 116, 139);
    public static readonly Color Success = Color.FromArgb(22, 163, 74);
    public static readonly Color Warning = Color.FromArgb(217, 119, 6);
    public static readonly Color Danger = Color.FromArgb(220, 38, 38);
    public static readonly Color Info = Color.FromArgb(8, 145, 178);
    public static readonly Color Violet = Color.FromArgb(124, 58, 237);
    public static readonly Color Gray = Color.FromArgb(100, 116, 139);

    public const string HotelName = "SÀI GÒN HOTEL";
    public const string FontName = "Segoe UI";
    public static readonly Font Base = new(FontName, 9.75f);
    public static readonly Font Bold = new(FontName, 9.75f, FontStyle.Bold);
    public static readonly Font Small = new(FontName, 8.5f);
    public static readonly Font H1 = new(FontName, 17f, FontStyle.Bold);
    public static readonly Font H2 = new(FontName, 12f, FontStyle.Bold);
    public static readonly Font Big = new(FontName, 20f, FontStyle.Bold);

    /// <summary>Màu theo trạng thái, dùng chung cho sơ đồ phòng, lưới dữ liệu và nhãn.</summary>
    public static Color StatusColor(string? trangThai) => trangThai switch
    {
        TrangThaiPhong.Trong => Success,
        TrangThaiPhong.DaDat => Warning,
        TrangThaiPhong.DangO => Primary,
        TrangThaiPhong.DangDon => Info,
        TrangThaiPhong.BaoTri => Danger,
        TrangThaiDatPhong.ChoNhanPhong => Warning,
        TrangThaiDatPhong.DaNhanPhong => Primary,
        TrangThaiDatPhong.HoanTat => Success,
        TrangThaiDatPhong.DaHuy => Gray,
        TrangThaiLuuTru.DaTraPhong => Gray,
        TrangThaiHoaDon.DaThanhToan => Success,
        TrangThaiHoaDon.ChuaThanhToan => Danger,
        TrangThaiHoaDon.ThanhToanMotPhan => Warning,
        TrangThaiTaiKhoan.HoatDong => Success,
        TrangThaiTaiKhoan.DaKhoa => Danger,
        TrangThaiDichVu.DangKinhDoanh => Success,
        TrangThaiDichVu.NgungKinhDoanh => Gray,
        _ => Muted,
    };

    public static Color Lighten(Color c, double amount) => Color.FromArgb(
        (int)(c.R + (255 - c.R) * amount), (int)(c.G + (255 - c.G) * amount), (int)(c.B + (255 - c.B) * amount));

    public static string Money(decimal v) => v.ToString("N0") + " đ";
}
