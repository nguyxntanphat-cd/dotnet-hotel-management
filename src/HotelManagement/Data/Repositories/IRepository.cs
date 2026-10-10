namespace HotelManagement.Data.Repositories;

/// <summary>
/// Bộ method chuẩn của mọi repository (mục 4 của AGENTS.md). Repository chỉ đọc/ghi một bảng,
/// không chứa quy tắc nghiệp vụ. Method riêng của từng bảng (TimKiem...) khai báo ở interface con.
/// </summary>
public interface IRepository<T> where T : class
{
    List<T> LayTatCa();
    T? LayTheoMa(string ma);
    void Them(T entity);
    void CapNhat(T entity);
    void Xoa(string ma);
    string TaoMaMoi();
}
