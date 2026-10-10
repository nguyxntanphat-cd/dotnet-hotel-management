using HotelManagement.Views;

namespace HotelManagement.Data;

public record MenuItemDef(string Key, string Nhom, string Ten, string MaBFD, Func<Control> TaoManHinh);

/// <summary>Danh sách chức năng theo sơ đồ BFD; dùng cho thanh menu và màn hình phân quyền.</summary>
public static class Menu
{
    public static readonly MenuItemDef[] Items =
    {
        new("tong_quan",  "Tổng quan",                    "Bảng điều khiển",          "",             () => new DashboardView()),
        new("tai_khoan",  "1. Quản lý hệ thống",          "Tài khoản",                "1.3",          () => new TaiKhoanView()),
        new("nhan_vien",  "1. Quản lý hệ thống",          "Nhân viên",                "1.4",          () => new NhanVienView()),
        new("phan_quyen", "1. Quản lý hệ thống",          "Phân quyền",               "1.5",          () => new PhanQuyenView()),
        new("loai_phong", "2. Quản lý phòng",             "Loại phòng & bảng giá",    "2.1",          () => new LoaiPhongView()),
        new("phong",      "2. Quản lý phòng",             "Sơ đồ phòng",              "2.2–2.4",      () => new PhongView()),
        new("khach_hang", "3. Khách hàng & đặt phòng",    "Khách hàng",               "3.1",          () => new KhachHangView()),
        new("dat_phong",  "3. Khách hàng & đặt phòng",    "Đặt phòng",                "3.2–3.5",      () => new DatPhongView()),
        new("nhan_phong", "4. Lưu trú & dịch vụ",         "Nhận phòng",               "4.1",          () => new NhanPhongView()),
        new("luu_tru",    "4. Lưu trú & dịch vụ",         "Khách đang lưu trú",       "4.2, 4.5",     () => new LuuTruView()),
        new("dich_vu",    "4. Lưu trú & dịch vụ",         "Danh mục dịch vụ",         "4.3",          () => new DichVuView()),
        new("su_dung_dv", "4. Lưu trú & dịch vụ",         "Ghi nhận dịch vụ",         "4.4",          () => new SuDungDichVuView()),
        new("tra_phong",  "5. Thanh toán",                "Trả phòng & thanh toán",   "5.1–5.5",      () => new TraPhongView()),
        new("hoa_don",    "5. Thanh toán",                "Tra cứu hóa đơn",          "5.6",          () => new HoaDonView()),
        new("thong_ke",   "6. Thống kê & báo cáo",        "Thống kê & báo cáo",       "6.1–6.5",      () => new ThongKeView()),
    };
}
