# AGENTS.md – Hướng dẫn cho người và AI làm việc trong repo này

File này là **luật chung** của dự án. Mọi người, và mọi công cụ AI (Claude Code, Copilot, Cursor, Codex...), phải đọc hết file này trước khi viết code. Mục tiêu: mỗi người làm một phần riêng, ghép lại vẫn build và chạy được, code đọc như một người viết.

Nếu một yêu cầu trong issue mâu thuẫn với file này, **làm theo file này** và ghi chú mâu thuẫn trong PR.

---

## 1. Dự án

- Đề tài: Hệ thống quản lý tòa khách sạn. Đặc tả gồm sơ đồ BFD 6 nhóm chức năng (1.1 → 6.5) và 13 lớp dữ liệu, trong đó `LuuTru` là lớp trung tâm.
- Công nghệ: **C# 12, .NET 8, WinForms** (`net8.0-windows`), cơ sở dữ liệu **SQL Server** (LocalDB khi phát triển).
- Solution: `HotelManagement.sln`, project duy nhất: `src/HotelManagement/HotelManagement.csproj`.
- Giao diện, thông báo, tên cột hiển thị: **tiếng Việt có dấu**. Tên định danh trong code: **tiếng Việt không dấu** (xem mục 4).

## 2. Kiến trúc: 4 tầng, gọi một chiều

```
Views / Forms  ──►  Services  ──►  Repositories  ──►  Db (SQL Server)
   (UI)            (nghiệp vụ)      (truy cập dữ liệu)
         └──────────── Models (dùng chung cho mọi tầng) ───────────┘
```

| Tầng | Thư mục | Được làm | Không được làm |
|---|---|---|---|
| Models | `Models/` | Lớp dữ liệu (13 lớp theo đặc tả) và hằng số trạng thái (`Constants.cs`) | Chứa logic, truy vấn, gọi UI |
| Repositories | `Data/Repositories/` | Đọc/ghi **một bảng**, trả về Model | Chứa quy tắc nghiệp vụ, gọi repository khác, gọi UI |
| Services | `Services/` | Quy tắc nghiệp vụ, kiểm tra hợp lệ, gọi một hoặc nhiều repository | Tạo control, gọi `MessageBox`, viết SQL |
| Views / Forms | `Views/`, `Forms/`, `UI/` | Hiển thị, nhận thao tác, gọi **Service** | Viết SQL, gọi repository trực tiếp, truy cập `MockDb` |

Quy tắc gọi giữa các module:
- Module A cần dữ liệu của module B thì gọi **Service public của B**, không gọi repository của B.
- Được **thêm** method mới vào Service của người khác nếu cần, nhưng **không đổi tên, không đổi tham số** method đã có. Ghi rõ trong PR và tag chủ module.

### Giai đoạn chuyển đổi Mock → SQL

Bản demo hiện tại đọc dữ liệu từ `Data/MockDb.cs` và `Data/HotelService.cs`. Khi chuyển sang SQL:
1. Issue nền tảng tạo `Db`, script CSDL, interface repository, và **bản Mock của mọi repository** (bọc `MockDb`).
2. Mỗi người viết bản **Sql** cho repository của module mình, chuyển View của mình sang gọi Service.
3. Chọn nguồn dữ liệu bằng biến môi trường `HOTEL_DATA=Mock|Sql` (mặc định `Mock`). Nhờ vậy app **luôn chạy được** dù các module khác chưa xong.
4. Khi mọi module đã sang Sql, xóa `MockDb.cs` và `HotelService.cs` (issue cuối).

## 3. Cấu trúc thư mục đích

```
src/HotelManagement/
  Program.cs
  Models/
    Entities.cs            13 lớp dữ liệu (CHUNG – chỉ issue nền tảng sửa)
    Constants.cs           Hằng số trạng thái, vai trò (CHUNG)
  Data/
    Db.cs                  Mở kết nối, helper Query/Execute (CHUNG)
    DataSource.cs          Chọn Mock/Sql, nơi lấy các repository (CHUNG)
    Sql/
      01_schema.sql        Tạo bảng, khóa chính, khóa ngoại (CHUNG)
      02_seed.sql          Dữ liệu mẫu
    Repositories/
      I<Ten>Repository.cs          Interface
      Mock<Ten>Repository.cs       Bản dùng MockDb
      Sql<Ten>Repository.cs        Bản dùng SQL Server
  Services/
    <Module>Service.cs     Mỗi module một file
  UI/                      Theme, UiKit, Components (CHUNG)
  Forms/                   LoginForm, MainForm
  Views/                   <Module>Views.cs – mỗi module một file
```

File đánh dấu **CHUNG** do issue nền tảng sở hữu. Người khác chỉ sửa khi thật cần, sửa nhỏ, và ghi rõ trong PR.

## 4. Quy ước đặt tên

Tên lấy từ đặc tả, tiếng Việt **không dấu**, viết liền.

| Thứ | Kiểu | Ví dụ |
|---|---|---|
| Lớp, interface, enum, file | PascalCase, interface có tiền tố `I` | `DatPhong`, `IPhongRepository`, `DatPhongService.cs` |
| Thuộc tính, method public | PascalCase | `MaPhong`, `NgayNhanDuKien`, `LayTheoMa()` |
| Field private | `_camelCase` | `_grid`, `_repo` |
| Biến cục bộ, tham số | camelCase | `maPhong`, `ngayTra`, `soDem` |
| Hằng số | PascalCase trong lớp static | `TrangThaiPhong.DangO` |
| Bảng SQL | Trùng tên lớp | `DatPhong`, `LuuTru` |
| Cột SQL | Trùng tên thuộc tính | `MaPhong`, `NgayNhanDuKien` |
| Mã (khóa chính) | `string`, tiền tố cố định + số | `P101`, `KH001`, `DP001`, `LT0001`, `HD0001` |

Tên method dùng **động từ tiếng Việt đứng đầu**, thống nhất bộ từ sau:

| Việc | Repository | Service |
|---|---|---|
| Lấy tất cả | `LayTatCa()` | `LayDanhSach(...)` |
| Lấy một bản ghi | `LayTheoMa(ma)` | `LayTheoMa(ma)` |
| Tìm theo điều kiện | `TimKiem(...)` | `TimKiem(...)` |
| Thêm | `Them(entity)` | `Them(...)` → trả về entity đã có mã |
| Sửa | `CapNhat(entity)` | `CapNhat(...)` |
| Xóa | `Xoa(ma)` | `Xoa(ma)` |
| Sinh mã mới | `TaoMaMoi()` | – |
| Nghiệp vụ | – | Theo BFD: `NhanPhong`, `TraPhong`, `LapHoaDon`, `GhiNhanThanhToan`, `HuyDatPhong`... |

## 5. Quy ước code

- Bật `Nullable`. Không để cảnh báo build mới (CI build ở Release).
- Tiền: `decimal`. Ngày giờ: `DateTime`. Số đêm: `HotelService.SoDem` (sau này `LuuTruService.SoDem`), không tự tính lại.
- Trạng thái: **luôn dùng hằng số** trong `Models/Constants.cs` (`TrangThaiPhong.Trong`...), không gõ chuỗi trực tiếp. Cần trạng thái mới thì thêm vào `Constants.cs`.
- Lỗi nghiệp vụ (trùng phòng, phòng đang ở, sai mật khẩu...): Service **ném `InvalidOperationException` với thông báo tiếng Việt**. View bắt lỗi và gọi `UiKit.Warn(ex.Message)`.
- Kiểm tra dữ liệu nhập ở View (`FormDialog.Validator`) cho lỗi nhập liệu. Kiểm tra quy tắc nghiệp vụ ở Service. Service **không tin** dữ liệu từ View.
- SQL: **luôn dùng tham số** (`@MaPhong`), không ghép chuỗi. Một thao tác nhiều bảng (nhận phòng, thanh toán) chạy trong **một transaction**.
- Mật khẩu: lưu hash (SHA-256 kèm salt, xem issue đăng nhập), không lưu chữ thường.
- Comment ngắn, tiếng Việt, chỉ giải thích "vì sao". Mỗi lớp public có `/// <summary>` ghi mã BFD, ví dụ `/// <summary>3.2 Tiếp nhận đặt phòng.</summary>`.

## 6. Quy ước giao diện

- Mỗi màn hình là một `UserControl` kế thừa `BaseView` (tiêu đề, mô tả, `Toolbar`, `Body`).
- Dựng control bằng code qua `UiKit` (`Btn`, `Grid().Columns(...)`, `Search`, `Combo`, `Card`) và `FormDialog` cho hộp thoại thêm/sửa. **Không dùng file `.Designer.cs`**, không tự chọn màu: dùng `Theme`.
- Lưới: `AutoGenerateColumns = false`, khai báo cột bằng `new Col(...)`, tiền có `Format: "N0"` và `Right: true`, cột trạng thái có `Status: true`.
- Màn hình mới đăng ký bằng **một dòng** trong `Data/Menu.cs` (key `snake_case`, kèm mã BFD). Không sửa dòng của người khác.
- Thông báo: `UiKit.Info`, `UiKit.Warn`, `UiKit.Confirm`. Không gọi `MessageBox` trực tiếp.

## 7. Thư viện

Chỉ dùng:
- Thư viện có sẵn của .NET 8 / WinForms.
- `Microsoft.Data.SqlClient` (truy cập SQL Server – thêm trong issue nền tảng).
- `ClosedXML` (chỉ để xuất Excel ở module thống kê).
- `xunit` cho project kiểm thử.

Muốn thêm thư viện khác: mở issue hỏi trước. Không dùng Entity Framework, Dapper, DevExpress, Guna, MaterialSkin... để cả nhóm dùng chung một cách.

## 8. Quy trình Git

- **Không bao giờ push thẳng lên `main`.** Mọi thay đổi đi qua Pull Request, CI (build trên Windows) phải xanh mới merge.
- Mỗi issue một nhánh: `issue-<số>-<mo-ta-ngan>`, ví dụ `issue-7-dat-phong`.
- Tiêu đề issue và PR bắt đầu bằng số thứ tự task: `[07] Đặt phòng (3.2–3.5)`. Người nhận task gán mình vào issue (Assignees).
- Mô tả PR có `Closes #<số issue>`, liệt kê file đã sửa ngoài phạm vi của mình (nếu có) và lý do.
- Trước khi mở PR: `git pull origin main`, build lại, chạy thử màn hình của mình và **ít nhất một màn hình của module khác** để chắc không làm hỏng.
- Commit message tiếng Việt, ngắn, bắt đầu bằng động từ: `Thêm kiểm tra trùng phòng khi sửa đặt phòng`.

## 9. Dành riêng cho AI agent

1. Đọc file này, `README.md` và issue được giao trước khi sửa code.
2. Chỉ sửa file thuộc phạm vi issue (bảng "Phạm vi file" trong issue). Cần sửa file CHUNG hoặc file module khác thì sửa tối thiểu và ghi rõ trong PR.
3. Làm trên nhánh riêng, mở PR vào `main`, **không push lên `main`, không tự merge**.
4. Không đổi tên, không xóa method public mà module khác đang gọi. Tìm nơi gọi trước (`grep`) khi sửa chữ ký hàm.
5. Không thêm thư viện ngoài danh sách mục 7.
6. Sau khi sửa phải build được: `dotnet build HotelManagement.sln -c Release` (trên Windows). Nếu môi trường không có Windows, nói rõ trong PR là chưa chạy thử giao diện.
7. Nếu yêu cầu không rõ hoặc đụng đến kiến trúc, dừng lại và hỏi trong issue thay vì tự đoán.

## 10. Lệnh thường dùng

```bash
dotnet build HotelManagement.sln -c Release      # build như CI
dotnet run --project src/HotelManagement          # chạy app (Windows)
set HOTEL_DATA=Sql && dotnet run --project src/HotelManagement   # chạy với SQL Server
sqlcmd -S "(localdb)\MSSQLLocalDB" -i src/HotelManagement/Data/Sql/01_schema.sql   # tạo CSDL
```

Tài khoản demo: `admin`, `quanly`, `letan`, `buongphong` – mật khẩu `123`.
