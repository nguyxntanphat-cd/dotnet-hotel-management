using HotelManagement.Models;

namespace HotelManagement.Data.Repositories;

/// <summary>3.1 Khách hàng. Bản Mock trên <see cref="MockDb.KhachHangs"/>.</summary>
public class MockKhachHangRepository : MockRepository<KhachHang>, IKhachHangRepository
{
    public MockKhachHangRepository() : base(MockDb.KhachHangs, x => x.MaKH, "KH", 3) { }
}
