using HotelManagement.Models;

namespace HotelManagement.Data.Repositories;

/// <summary>
/// 2.1 Loại phòng – bản SQL Server. Đây là mẫu cho mọi <c>Sql*Repository</c>:
/// SQL viết thẳng, tham số <c>@Ten</c> lấy từ thuộc tính cùng tên, cột trùng tên thuộc tính để <see cref="Db.Query{T}(string, object?)"/> tự gán.
/// </summary>
public class SqlLoaiPhongRepository : ILoaiPhongRepository
{
    public List<LoaiPhong> LayTatCa() =>
        Db.Query<LoaiPhong>("SELECT MaLoai, TenLoai, GiaCoBan, SucChua, MoTa FROM LoaiPhong ORDER BY MaLoai");

    public LoaiPhong? LayTheoMa(string ma) =>
        Db.Query<LoaiPhong>("SELECT MaLoai, TenLoai, GiaCoBan, SucChua, MoTa FROM LoaiPhong WHERE MaLoai = @MaLoai",
            new { MaLoai = ma }).FirstOrDefault();

    public void Them(LoaiPhong entity) =>
        Db.Execute(@"INSERT INTO LoaiPhong (MaLoai, TenLoai, GiaCoBan, SucChua, MoTa)
                     VALUES (@MaLoai, @TenLoai, @GiaCoBan, @SucChua, @MoTa)", entity);

    public void CapNhat(LoaiPhong entity) =>
        Db.Execute(@"UPDATE LoaiPhong SET TenLoai = @TenLoai, GiaCoBan = @GiaCoBan, SucChua = @SucChua, MoTa = @MoTa
                     WHERE MaLoai = @MaLoai", entity);

    public void Xoa(string ma) =>
        Db.Execute("DELETE FROM LoaiPhong WHERE MaLoai = @MaLoai", new { MaLoai = ma });

    public string TaoMaMoi() => Db.TaoMa("LoaiPhong", "MaLoai", "LP", 2);
}
