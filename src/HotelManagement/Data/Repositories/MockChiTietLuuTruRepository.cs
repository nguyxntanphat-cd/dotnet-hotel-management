using HotelManagement.Models;

namespace HotelManagement.Data.Repositories;

/// <summary>4.2 Khách trong một lượt lưu trú. Bản Mock trên <see cref="MockDb.ChiTietLuuTrus"/>.</summary>
public class MockChiTietLuuTruRepository : MockRepository<ChiTietLuuTru>, IChiTietLuuTruRepository
{
    public MockChiTietLuuTruRepository() : base(MockDb.ChiTietLuuTrus, x => x.MaCTLT, "CT", 4) { }
}
