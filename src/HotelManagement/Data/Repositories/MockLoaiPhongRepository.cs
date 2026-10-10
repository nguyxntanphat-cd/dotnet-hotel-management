using HotelManagement.Models;

namespace HotelManagement.Data.Repositories;

/// <summary>2.1 Loại phòng. Bản Mock trên <see cref="MockDb.LoaiPhongs"/>.</summary>
public class MockLoaiPhongRepository : MockRepository<LoaiPhong>, ILoaiPhongRepository
{
    public MockLoaiPhongRepository() : base(MockDb.LoaiPhongs, x => x.MaLoai, "LP", 2) { }
}
