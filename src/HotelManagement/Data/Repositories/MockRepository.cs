namespace HotelManagement.Data.Repositories;

/// <summary>
/// Bản Mock dùng chung: đọc/ghi một danh sách trong <see cref="MockDb"/>.
/// Lưu ý: bản Mock trả về đúng đối tượng trong bộ nhớ, bản Sql trả về bản sao,
/// nên Service luôn phải gọi <see cref="CapNhat"/> sau khi sửa để chạy đúng ở cả hai chế độ.
/// </summary>
public abstract class MockRepository<T> : IRepository<T> where T : class
{
    private readonly Func<T, string> _ma;
    private readonly string _tienTo;
    private readonly int _doDai;

    protected MockRepository(List<T> danhSach, Func<T, string> ma, string tienTo, int doDai)
    {
        DanhSach = danhSach;
        _ma = ma;
        _tienTo = tienTo;
        _doDai = doDai;
    }

    /// <summary>Danh sách gốc trong MockDb, cho lớp con viết thêm method riêng (TimKiem...).</summary>
    protected List<T> DanhSach { get; }

    public virtual List<T> LayTatCa() => DanhSach.ToList();

    public virtual T? LayTheoMa(string ma) => DanhSach.FirstOrDefault(x => _ma(x) == ma);

    public virtual void Them(T entity)
    {
        if (LayTheoMa(_ma(entity)) != null)
            throw new InvalidOperationException("Dữ liệu bị trùng: mã hoặc giá trị không được trùng đã tồn tại.");
        DanhSach.Add(entity);
    }

    public virtual void CapNhat(T entity)
    {
        var i = DanhSach.FindIndex(x => _ma(x) == _ma(entity));
        if (i >= 0) DanhSach[i] = entity;
    }

    public virtual void Xoa(string ma) => DanhSach.RemoveAll(x => _ma(x) == ma);

    public virtual string TaoMaMoi() => MockDb.NextId(DanhSach, _ma, _tienTo, _doDai);
}
