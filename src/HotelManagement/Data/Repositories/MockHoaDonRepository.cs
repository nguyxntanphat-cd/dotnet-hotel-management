using HotelManagement.Models;

namespace HotelManagement.Data.Repositories;

/// <summary>5.4, 5.6 Hóa đơn. Bản Mock trên <see cref="MockDb.HoaDons"/>.</summary>
public class MockHoaDonRepository : MockRepository<HoaDon>, IHoaDonRepository
{
    public MockHoaDonRepository() : base(MockDb.HoaDons, x => x.MaHD, "HD", 4) { }
}
