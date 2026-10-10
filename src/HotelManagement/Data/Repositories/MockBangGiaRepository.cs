using HotelManagement.Models;

namespace HotelManagement.Data.Repositories;

/// <summary>2.1 Bảng giá theo thời gian. Bản Mock trên <see cref="MockDb.BangGias"/>.</summary>
public class MockBangGiaRepository : MockRepository<BangGia>, IBangGiaRepository
{
    public MockBangGiaRepository() : base(MockDb.BangGias, x => x.MaGia, "BG", 3) { }
}
