using HotelManagement.Models;

namespace HotelManagement.Data.Repositories;

/// <summary>4.4 Sử dụng dịch vụ. Bản Mock trên <see cref="MockDb.SuDungDichVus"/>.</summary>
public class MockSuDungDichVuRepository : MockRepository<SuDungDichVu>, ISuDungDichVuRepository
{
    public MockSuDungDichVuRepository() : base(MockDb.SuDungDichVus, x => x.MaSD, "SD", 4) { }
}
