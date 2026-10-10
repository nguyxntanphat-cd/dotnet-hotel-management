using System.Collections.Concurrent;
using System.Data;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace HotelManagement.Data;

/// <summary>
/// Kết nối SQL Server và các hàm dùng chung cho mọi <c>Sql*Repository</c>:
/// <see cref="Query{T}(string, object?)"/>, <see cref="Execute"/>, <see cref="Scalar{T}"/>, <see cref="Transaction(Action)"/>.
/// Tham số truyền bằng đối tượng: mỗi thuộc tính thành một tham số cùng tên, ví dụ
/// <c>Db.Query&lt;Phong&gt;("SELECT ... WHERE MaPhong = @MaPhong", new { MaPhong = ma })</c>.
/// </summary>
public static class Db
{
    public const string ChuoiKetNoiMacDinh =
        @"Server=(localdb)\MSSQLLocalDB;Database=HotelManagement;Integrated Security=True;TrustServerCertificate=True";

    /// <summary>Đọc từ biến môi trường <c>HOTEL_DB</c>, không đặt thì dùng LocalDB.</summary>
    public static string ChuoiKetNoi { get; set; } =
        Environment.GetEnvironmentVariable("HOTEL_DB") is { Length: > 0 } s ? s : ChuoiKetNoiMacDinh;

    // Kết nối và transaction đang mở của luồng hiện tại; các lệnh gọi bên trong Transaction(...) tự dùng chung.
    [ThreadStatic] private static SqlConnection? _ketNoi;
    [ThreadStatic] private static SqlTransaction? _giaoDich;

    // ---------- Đọc ----------

    /// <summary>Chạy SELECT, tự gán cột vào thuộc tính trùng tên của <typeparamref name="T"/>.</summary>
    public static List<T> Query<T>(string sql, object? thamSo = null) where T : new()
    {
        var thuocTinh = ThuocTinhGhiDuoc(typeof(T));
        return Query(sql, r =>
        {
            var obj = new T();
            for (int i = 0; i < r.FieldCount; i++)
            {
                if (!thuocTinh.TryGetValue(r.GetName(i), out var p) || r.IsDBNull(i)) continue;
                p.SetValue(obj, DoiKieu(r.GetValue(i), p.PropertyType));
            }
            return obj;
        }, thamSo);
    }

    /// <summary>Chạy SELECT, tự đọc từng dòng bằng <paramref name="docDong"/> (dùng khi kết quả không trùng một lớp Model).</summary>
    public static List<T> Query<T>(string sql, Func<SqlDataReader, T> docDong, object? thamSo = null) =>
        DungKetNoi(cmd =>
        {
            using var r = cmd.ExecuteReader();
            var ds = new List<T>();
            while (r.Read()) ds.Add(docDong(r));
            return ds;
        }, sql, thamSo);

    /// <summary>Lấy giá trị cột đầu của dòng đầu (COUNT, SUM, MAX...). Trả về default nếu không có hoặc NULL.</summary>
    public static T? Scalar<T>(string sql, object? thamSo = null) =>
        DungKetNoi(cmd =>
        {
            var v = cmd.ExecuteScalar();
            return v is null or DBNull ? default : (T?)DoiKieu(v, typeof(T));
        }, sql, thamSo);

    // ---------- Ghi ----------

    /// <summary>Chạy INSERT/UPDATE/DELETE, trả về số dòng bị ảnh hưởng.</summary>
    /// <exception cref="InvalidOperationException">Trùng khóa hoặc vi phạm khóa ngoại, kèm thông báo tiếng Việt.</exception>
    public static int Execute(string sql, object? thamSo = null) =>
        DungKetNoi(cmd =>
        {
            try { return cmd.ExecuteNonQuery(); }
            catch (SqlException ex) when (ex.Number is 2627 or 2601)
            {
                throw new InvalidOperationException("Dữ liệu bị trùng: mã hoặc giá trị không được trùng đã tồn tại.", ex);
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                throw new InvalidOperationException("Không thực hiện được vì dữ liệu đang được dùng ở nơi khác hoặc tham chiếu tới mã không tồn tại.", ex);
            }
        }, sql, thamSo);

    // ---------- Transaction ----------

    /// <summary>
    /// Chạy nhiều thao tác trong một transaction: mọi lệnh Db bên trong dùng chung một kết nối,
    /// lỗi thì hoàn tác hết. Gọi lồng nhau thì dùng transaction ngoài cùng.
    /// Ở chế độ Mock chỉ chạy <paramref name="congViec"/> (dữ liệu trong bộ nhớ không hoàn tác được).
    /// </summary>
    public static void Transaction(Action congViec) => Transaction(() => { congViec(); return 0; });

    public static T Transaction<T>(Func<T> congViec)
    {
        if (!DataSource.LaSql || _giaoDich != null) return congViec();

        using var cn = new SqlConnection(ChuoiKetNoi);
        cn.Open();
        using var tx = cn.BeginTransaction();
        _ketNoi = cn;
        _giaoDich = tx;
        try
        {
            var kq = congViec();
            tx.Commit();
            return kq;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
        finally
        {
            _ketNoi = null;
            _giaoDich = null;
        }
    }

    // ---------- Tiện ích ----------

    /// <summary>
    /// Sinh mã mới dạng tiền tố + số, ví dụ <c>TaoMa("DatPhong", "MaDat", "DP", 3)</c> → <c>DP042</c>.
    /// Cùng cách đánh số với <see cref="MockDb.NextId"/> để hai nguồn dữ liệu cho mã giống nhau.
    /// </summary>
    public static string TaoMa(string bang, string cot, string tienTo, int doDai)
    {
        // Tên bảng/cột không truyền bằng tham số được, nên chỉ nhận tên định danh hợp lệ do code truyền vào.
        if (!TenHopLe.IsMatch(bang) || !TenHopLe.IsMatch(cot))
            throw new ArgumentException("Tên bảng hoặc cột không hợp lệ.");
        var max = Scalar<int?>(
            $"SELECT MAX(TRY_CAST(SUBSTRING({cot}, LEN(@TienTo) + 1, 20) AS INT)) FROM {bang} WHERE {cot} LIKE @TienTo + '%'",
            new { TienTo = tienTo }) ?? 0;
        return tienTo + (max + 1).ToString().PadLeft(doDai, '0');
    }

    /// <summary>Mở thử kết nối, ném lỗi nếu không kết nối được (gọi khi khởi động ở chế độ Sql).</summary>
    public static void KiemTraKetNoi()
    {
        using var cn = new SqlConnection(ChuoiKetNoi);
        cn.Open();
    }

    private static readonly Regex TenHopLe = new(@"^[A-Za-z_][A-Za-z0-9_]*$");

    private static T DungKetNoi<T>(Func<SqlCommand, T> chay, string sql, object? thamSo)
    {
        if (_ketNoi != null)
        {
            using var cmdGd = TaoLenh(_ketNoi, sql, thamSo);
            cmdGd.Transaction = _giaoDich;
            return chay(cmdGd);
        }
        using var cn = new SqlConnection(ChuoiKetNoi);
        cn.Open();
        using var cmd = TaoLenh(cn, sql, thamSo);
        return chay(cmd);
    }

    private static SqlCommand TaoLenh(SqlConnection cn, string sql, object? thamSo)
    {
        var cmd = new SqlCommand(sql, cn);
        if (thamSo == null) return cmd;
        foreach (var p in thamSo.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!p.CanRead || p.GetIndexParameters().Length > 0) continue;
            var giaTri = p.GetValue(thamSo);
            var sp = cmd.Parameters.AddWithValue("@" + p.Name, giaTri ?? DBNull.Value);
            // DateTime mặc định ánh xạ sang kiểu datetime cũ (mất độ chính xác, không nhận năm < 1753).
            if (giaTri is DateTime) sp.SqlDbType = SqlDbType.DateTime2;
        }
        return cmd;
    }

    private static readonly ConcurrentDictionary<Type, Dictionary<string, PropertyInfo>> _thuocTinh = new();

    private static Dictionary<string, PropertyInfo> ThuocTinhGhiDuoc(Type t) =>
        _thuocTinh.GetOrAdd(t, k => k.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite)
            .ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase));

    private static object? DoiKieu(object giaTri, Type kieu)
    {
        var dich = Nullable.GetUnderlyingType(kieu) ?? kieu;
        if (dich.IsInstanceOfType(giaTri)) return giaTri;
        if (giaTri is DateOnly d && dich == typeof(DateTime)) return d.ToDateTime(TimeOnly.MinValue);
        return Convert.ChangeType(giaTri, dich);
    }
}
