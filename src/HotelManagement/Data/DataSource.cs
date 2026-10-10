using HotelManagement.Data.Repositories;

namespace HotelManagement.Data;

/// <summary>
/// Chọn nguồn dữ liệu theo biến môi trường <c>HOTEL_DATA=Mock|Sql</c> (mặc định Mock) và là nơi duy nhất
/// Service lấy repository. Ở chế độ Sql, bảng nào chưa có <c>Sql*Repository</c> thì tạm dùng bản Mock
/// để app vẫn chạy được trong lúc chuyển đổi.
/// </summary>
public static class DataSource
{
    // Trim vì `set HOTEL_DATA=Sql && ...` trong cmd giữ lại dấu cách cuối.
    public static bool LaSql { get; } =
        string.Equals(Environment.GetEnvironmentVariable("HOTEL_DATA")?.Trim(), "Sql", StringComparison.OrdinalIgnoreCase);

    public static string Ten => LaSql ? "Sql" : "Mock";

    // Viết xong Sql<Ten>Repository thì sửa đúng dòng của module mình: thay `null` bằng `() => new Sql<Ten>Repository()`.
    public static ITaiKhoanRepository TaiKhoan { get; } = Chon<ITaiKhoanRepository>(() => new MockTaiKhoanRepository(), null);                // [02]
    public static INhanVienRepository NhanVien { get; } = Chon<INhanVienRepository>(() => new MockNhanVienRepository(), null);                // [03]
    public static ILoaiPhongRepository LoaiPhong { get; } = Chon<ILoaiPhongRepository>(() => new MockLoaiPhongRepository(), () => new SqlLoaiPhongRepository()); // [04]
    public static IBangGiaRepository BangGia { get; } = Chon<IBangGiaRepository>(() => new MockBangGiaRepository(), null);                    // [04]
    public static IPhongRepository Phong { get; } = Chon<IPhongRepository>(() => new MockPhongRepository(), null);                            // [05]
    public static IKhachHangRepository KhachHang { get; } = Chon<IKhachHangRepository>(() => new MockKhachHangRepository(), null);            // [06]
    public static IDatPhongRepository DatPhong { get; } = Chon<IDatPhongRepository>(() => new MockDatPhongRepository(), null);                // [07]
    public static ILuuTruRepository LuuTru { get; } = Chon<ILuuTruRepository>(() => new MockLuuTruRepository(), null);                        // [08]
    public static IChiTietLuuTruRepository ChiTietLuuTru { get; } = Chon<IChiTietLuuTruRepository>(() => new MockChiTietLuuTruRepository(), null); // [08]
    public static IDichVuRepository DichVu { get; } = Chon<IDichVuRepository>(() => new MockDichVuRepository(), null);                        // [09]
    public static ISuDungDichVuRepository SuDungDichVu { get; } = Chon<ISuDungDichVuRepository>(() => new MockSuDungDichVuRepository(), null); // [09]
    public static IHoaDonRepository HoaDon { get; } = Chon<IHoaDonRepository>(() => new MockHoaDonRepository(), null);                        // [10]
    public static IThanhToanRepository ThanhToan { get; } = Chon<IThanhToanRepository>(() => new MockThanhToanRepository(), null);            // [10]

    private static T Chon<T>(Func<T> mock, Func<T>? sql) => LaSql && sql != null ? sql() : mock();
}
