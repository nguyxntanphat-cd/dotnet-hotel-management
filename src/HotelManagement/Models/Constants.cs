namespace HotelManagement.Models;

public static class VaiTro
{
    public const string QuanTriVien = "Quản trị viên";
    public const string QuanLy = "Quản lý";
    public const string LeTan = "Lễ tân";
    public const string BuongPhong = "Nhân viên buồng phòng";
    public static readonly string[] TatCa = { QuanTriVien, QuanLy, LeTan, BuongPhong };
}

public static class TrangThaiTaiKhoan
{
    public const string HoatDong = "Hoạt động";
    public const string DaKhoa = "Đã khóa";
}

public static class TrangThaiPhong
{
    public const string Trong = "Trống";
    public const string DaDat = "Đã đặt";
    public const string DangO = "Đang ở";
    public const string DangDon = "Đang dọn";
    public const string BaoTri = "Bảo trì";
    public static readonly string[] TatCa = { Trong, DaDat, DangO, DangDon, BaoTri };
    /// <summary>Trạng thái nhân viên được phép cập nhật tay; "Đã đặt" và "Đang ở" do hệ thống tự cập nhật.</summary>
    public static readonly string[] CapNhatTay = { Trong, DangDon, BaoTri };
}

public static class TrangThaiDatPhong
{
    public const string ChoNhanPhong = "Chờ nhận phòng";
    public const string DaNhanPhong = "Đã nhận phòng";
    public const string HoanTat = "Hoàn tất";
    public const string DaHuy = "Đã hủy";
    public static readonly string[] TatCa = { ChoNhanPhong, DaNhanPhong, HoanTat, DaHuy };
}

public static class TrangThaiLuuTru
{
    public const string DangO = "Đang ở";
    public const string DaTraPhong = "Đã trả phòng";
}

/// <summary>Vai trò của khách trong một lượt lưu trú (<see cref="ChiTietLuuTru.VaiTro"/>).</summary>
public static class VaiTroLuuTru
{
    public const string KhachChinh = "Khách chính";
    public const string KhachDiCung = "Khách đi cùng";
}

public static class TrangThaiDichVu
{
    public const string DangKinhDoanh = "Đang kinh doanh";
    public const string NgungKinhDoanh = "Ngừng kinh doanh";
}

public static class TrangThaiHoaDon
{
    public const string ChuaThanhToan = "Chưa thanh toán";
    public const string ThanhToanMotPhan = "Thanh toán một phần";
    public const string DaThanhToan = "Đã thanh toán";
    public static readonly string[] TatCa = { ChuaThanhToan, ThanhToanMotPhan, DaThanhToan };
}

public static class TrangThaiThanhToan
{
    public const string ThanhCong = "Thành công";
}

public static class PhuongThucThanhToan
{
    public static readonly string[] TatCa = { "Tiền mặt", "Chuyển khoản", "Thẻ ngân hàng", "Ví điện tử" };
}
