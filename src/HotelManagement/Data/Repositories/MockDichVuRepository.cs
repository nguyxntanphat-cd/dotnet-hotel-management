using HotelManagement.Models;

namespace HotelManagement.Data.Repositories;

/// <summary>4.3 Danh mục dịch vụ. Bản Mock trên <see cref="MockDb.DichVus"/>.</summary>
public class MockDichVuRepository : MockRepository<DichVu>, IDichVuRepository
{
    public MockDichVuRepository() : base(MockDb.DichVus, x => x.MaDV, "DV", 2) { }
}
