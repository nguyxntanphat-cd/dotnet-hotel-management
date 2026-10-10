using HotelManagement.Models;

namespace HotelManagement.Data.Repositories;

/// <summary>5.5 Thanh toán. Bản Mock trên <see cref="MockDb.ThanhToans"/>.</summary>
public class MockThanhToanRepository : MockRepository<ThanhToan>, IThanhToanRepository
{
    public MockThanhToanRepository() : base(MockDb.ThanhToans, x => x.MaTT, "TT", 4) { }
}
