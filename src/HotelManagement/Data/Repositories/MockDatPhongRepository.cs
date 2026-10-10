using HotelManagement.Models;

namespace HotelManagement.Data.Repositories;

/// <summary>3.2–3.5 Đặt phòng. Bản Mock trên <see cref="MockDb.DatPhongs"/>.</summary>
public class MockDatPhongRepository : MockRepository<DatPhong>, IDatPhongRepository
{
    public MockDatPhongRepository() : base(MockDb.DatPhongs, x => x.MaDat, "DP", 3) { }
}
