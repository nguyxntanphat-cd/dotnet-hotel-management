using HotelManagement.Models;

namespace HotelManagement.Data.Repositories;

/// <summary>1.4 Nhân viên. Bản Mock trên <see cref="MockDb.NhanViens"/>.</summary>
public class MockNhanVienRepository : MockRepository<NhanVien>, INhanVienRepository
{
    public MockNhanVienRepository() : base(MockDb.NhanViens, x => x.MaNV, "NV", 3) { }
}
