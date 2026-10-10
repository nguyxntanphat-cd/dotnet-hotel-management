using HotelManagement.Models;

namespace HotelManagement.Data.Repositories;

/// <summary>2.2–2.4 Phòng. Bản Mock trên <see cref="MockDb.Phongs"/>.</summary>
/// <remarks>Mã phòng nên lấy theo số phòng ("P" + SoPhong); TaoMaMoi chỉ dùng khi không có số phòng.</remarks>
public class MockPhongRepository : MockRepository<Phong>, IPhongRepository
{
    public MockPhongRepository() : base(MockDb.Phongs, x => x.MaPhong, "P", 3) { }
}
