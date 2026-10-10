using HotelManagement.Models;

namespace HotelManagement.Data.Repositories;

/// <summary>1.1–1.3 Tài khoản đăng nhập. Bản Mock trên <see cref="MockDb.TaiKhoans"/>.</summary>
public class MockTaiKhoanRepository : MockRepository<TaiKhoan>, ITaiKhoanRepository
{
    public MockTaiKhoanRepository() : base(MockDb.TaiKhoans, x => x.MaTK, "TK", 3) { }
}
