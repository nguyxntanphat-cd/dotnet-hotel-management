using HotelManagement.Models;

namespace HotelManagement.Data.Repositories;

/// <summary>4.1, 4.2, 4.5 Lưu trú (lớp trung tâm). Bản Mock trên <see cref="MockDb.LuuTrus"/>.</summary>
public class MockLuuTruRepository : MockRepository<LuuTru>, ILuuTruRepository
{
    public MockLuuTruRepository() : base(MockDb.LuuTrus, x => x.MaLuuTru, "LT", 4) { }
}
