using HotelManagement.Data;
using HotelManagement.Models;
using HotelManagement.UI;
using static HotelManagement.Data.MockDb;

namespace HotelManagement.Views;

/// <summary>1.3 Quản lý tài khoản.</summary>
public class TaiKhoanView : BaseView
{
    private record Row(string MaTK, string TenDangNhap, string HoTen, string ChucVu, string VaiTro, string TrangThai, TaiKhoan E);

    private readonly DataGridView _grid = UiKit.Grid();
    private readonly TextBox _search = UiKit.Search("Tìm theo tên đăng nhập, họ tên...");
    private readonly ComboBox _role = UiKit.Combo(new[] { "Tất cả vai trò" }.Concat(VaiTro.TatCa), 180);

    public TaiKhoanView() : base("Quản lý tài khoản", "Tạo tài khoản đăng nhập cho nhân viên, gán vai trò, khóa/mở khóa và đặt lại mật khẩu.")
    {
        var add = UiKit.Btn("+ Thêm tài khoản");
        var edit = UiKit.Btn("Sửa", BtnKind.Secondary);
        var lockBtn = UiKit.Btn("Khóa / Mở khóa", BtnKind.Secondary);
        var reset = UiKit.Btn("Đặt lại mật khẩu", BtnKind.Secondary);
        var del = UiKit.Btn("Xóa", BtnKind.Danger);
        Toolbar.Controls.AddRange(new Control[] { _search, _role });
        Spacer();
        Toolbar.Controls.AddRange(new Control[] { add, edit, lockBtn, reset, del });

        _grid.Columns(
            new Col("MaTK", "Mã TK", 80),
            new Col("TenDangNhap", "Tên đăng nhập", 150),
            new Col("HoTen", "Nhân viên", 200, Fill: true),
            new Col("ChucVu", "Chức vụ", 170),
            new Col("VaiTro", "Vai trò", 180),
            new Col("TrangThai", "Trạng thái", 120, Status: true));
        var card = UiKit.Card(padding: new Padding(1));
        card.Dock = DockStyle.Fill;
        card.Controls.Add(_grid);
        Body.Controls.Add(card);

        _search.TextChanged += (_, _) => Reload();
        _role.SelectedIndexChanged += (_, _) => Reload();
        add.Click += (_, _) => Edit(null);
        edit.Click += (_, _) => { if (_grid.Selected<Row>() is { } r) Edit(r.E); };
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0 && _grid.Selected<Row>() is { } r) Edit(r.E); };
        lockBtn.Click += (_, _) =>
        {
            if (_grid.Selected<Row>() is not { } r) return;
            if (r.E == Session.TaiKhoan) { UiKit.Warn("Không thể khóa tài khoản đang đăng nhập."); return; }
            r.E.TrangThai = r.E.TrangThai == TrangThaiTaiKhoan.HoatDong ? TrangThaiTaiKhoan.DaKhoa : TrangThaiTaiKhoan.HoatDong;
            Reload();
        };
        reset.Click += (_, _) =>
        {
            if (_grid.Selected<Row>() is not { } r) return;
            if (!UiKit.Confirm($"Đặt lại mật khẩu của \"{r.TenDangNhap}\" về mặc định 123?")) return;
            r.E.MatKhau = "123";
            UiKit.Info("Đã đặt lại mật khẩu về 123.");
        };
        del.Click += (_, _) =>
        {
            if (_grid.Selected<Row>() is not { } r) return;
            if (r.E == Session.TaiKhoan) { UiKit.Warn("Không thể xóa tài khoản đang đăng nhập."); return; }
            if (!UiKit.Confirm($"Xóa tài khoản \"{r.TenDangNhap}\"?")) return;
            TaiKhoans.Remove(r.E);
            Reload();
        };
        Reload();
    }

    public override void Reload()
    {
        var q = _search.Text.Trim().ToLower();
        var role = _role.SelectedIndex > 0 ? (string)_role.SelectedItem! : null;
        _grid.Bind(TaiKhoans
            .Select(t => { var nv = HotelService.NV(t.MaNV); return new Row(t.MaTK, t.TenDangNhap, nv?.HoTen ?? "", nv?.ChucVu ?? "", t.VaiTro, t.TrangThai, t); })
            .Where(r => (role == null || r.VaiTro == role) && (q == "" || r.TenDangNhap.ToLower().Contains(q) || r.HoTen.ToLower().Contains(q))));
    }

    private void Edit(TaiKhoan? tk)
    {
        using var d = new FormDialog(tk == null ? "Thêm tài khoản" : "Sửa tài khoản");
        var user = d.TextField("Tên đăng nhập", tk?.TenDangNhap ?? "");
        var pass = tk == null ? d.TextField("Mật khẩu", "", password: true) : null;
        var nvItems = NhanViens.Select(n => $"{n.MaNV} - {n.HoTen}").ToList();
        var nv = d.ComboField("Nhân viên", nvItems, nvItems.FirstOrDefault(s => s.StartsWith(tk?.MaNV ?? "~")));
        var role = d.ComboField("Vai trò", VaiTro.TatCa, tk?.VaiTro);
        var status = d.ComboField("Trạng thái", new[] { TrangThaiTaiKhoan.HoatDong, TrangThaiTaiKhoan.DaKhoa }, tk?.TrangThai);
        d.Validator = () =>
        {
            if (string.IsNullOrWhiteSpace(user.Text)) return "Tên đăng nhập không được để trống.";
            if (TaiKhoans.Any(t => t != tk && t.TenDangNhap.Equals(user.Text.Trim(), StringComparison.OrdinalIgnoreCase))) return "Tên đăng nhập đã tồn tại.";
            if (pass != null && pass.Text.Length < 3) return "Mật khẩu tối thiểu 3 ký tự.";
            return null;
        };
        if (d.ShowDialog(this) != DialogResult.OK) return;
        tk ??= AddNew();
        tk.TenDangNhap = user.Text.Trim();
        if (pass != null) tk.MatKhau = pass.Text;
        tk.MaNV = ((string)nv.SelectedItem!).Split(" - ")[0];
        tk.VaiTro = (string)role.SelectedItem!;
        tk.TrangThai = (string)status.SelectedItem!;
        Reload();
    }

    private static TaiKhoan AddNew()
    {
        var tk = new TaiKhoan { MaTK = NextId(TaiKhoans, t => t.MaTK, "TK") };
        TaiKhoans.Add(tk);
        return tk;
    }
}

/// <summary>1.4 Quản lý nhân viên.</summary>
public class NhanVienView : BaseView
{
    private readonly DataGridView _grid = UiKit.Grid();
    private readonly TextBox _search = UiKit.Search("Tìm theo mã, họ tên, SĐT...");

    public NhanVienView() : base("Quản lý nhân viên", "Hồ sơ nhân viên khách sạn: lễ tân, buồng phòng, quản lý.")
    {
        var add = UiKit.Btn("+ Thêm nhân viên");
        var edit = UiKit.Btn("Sửa", BtnKind.Secondary);
        var del = UiKit.Btn("Xóa", BtnKind.Danger);
        Toolbar.Controls.Add(_search);
        Spacer();
        Toolbar.Controls.AddRange(new Control[] { add, edit, del });

        _grid.Columns(
            new Col("MaNV", "Mã NV", 80),
            new Col("HoTen", "Họ tên", 200, Fill: true),
            new Col("NgaySinh", "Ngày sinh", 110, "dd/MM/yyyy"),
            new Col("SDT", "Số điện thoại", 130),
            new Col("Email", "Email", 200),
            new Col("ChucVu", "Chức vụ", 180));
        var card = UiKit.Card(padding: new Padding(1));
        card.Dock = DockStyle.Fill;
        card.Controls.Add(_grid);
        Body.Controls.Add(card);

        _search.TextChanged += (_, _) => Reload();
        add.Click += (_, _) => Edit(null);
        edit.Click += (_, _) => { if (_grid.Selected<NhanVien>() is { } n) Edit(n); };
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0 && _grid.Selected<NhanVien>() is { } n) Edit(n); };
        del.Click += (_, _) =>
        {
            if (_grid.Selected<NhanVien>() is not { } n) return;
            if (TaiKhoans.Any(t => t.MaNV == n.MaNV)) { UiKit.Warn("Nhân viên đang có tài khoản đăng nhập. Hãy xóa tài khoản trước."); return; }
            if (!UiKit.Confirm($"Xóa nhân viên \"{n.HoTen}\"?")) return;
            NhanViens.Remove(n);
            Reload();
        };
        Reload();
    }

    public override void Reload()
    {
        var q = _search.Text.Trim().ToLower();
        _grid.Bind(NhanViens.Where(n => q == "" || n.MaNV.ToLower().Contains(q) || n.HoTen.ToLower().Contains(q) || n.SDT.Contains(q)));
    }

    private void Edit(NhanVien? nv)
    {
        using var d = new FormDialog(nv == null ? "Thêm nhân viên" : "Sửa nhân viên");
        var ten = d.TextField("Họ tên", nv?.HoTen ?? "");
        var ns = d.DateField("Ngày sinh", nv?.NgaySinh ?? new DateTime(2000, 1, 1));
        var sdt = d.TextField("Số điện thoại", nv?.SDT ?? "");
        var email = d.TextField("Email", nv?.Email ?? "");
        var cv = d.ComboField("Chức vụ", VaiTro.TatCa, nv?.ChucVu);
        d.Validator = () =>
        {
            if (string.IsNullOrWhiteSpace(ten.Text)) return "Họ tên không được để trống.";
            if (sdt.Text.Trim().Length < 9 || !sdt.Text.Trim().All(char.IsDigit)) return "Số điện thoại không hợp lệ.";
            if (email.Text.Length > 0 && !email.Text.Contains('@')) return "Email không hợp lệ.";
            if (DateTime.Today.Year - ns.Value.Year < 18) return "Nhân viên phải từ 18 tuổi trở lên.";
            return null;
        };
        if (d.ShowDialog(this) != DialogResult.OK) return;
        if (nv == null)
        {
            nv = new NhanVien { MaNV = NextId(NhanViens, n => n.MaNV, "NV") };
            NhanViens.Add(nv);
        }
        nv.HoTen = ten.Text.Trim();
        nv.NgaySinh = ns.Value.Date;
        nv.SDT = sdt.Text.Trim();
        nv.Email = email.Text.Trim();
        nv.ChucVu = (string)cv.SelectedItem!;
        Reload();
    }
}

/// <summary>1.5 Phân quyền người dùng: ma trận chức năng × vai trò.</summary>
public class PhanQuyenView : BaseView
{
    private readonly DataGridView _grid = UiKit.Grid();

    public PhanQuyenView() : base("Phân quyền người dùng", "Chọn chức năng mà mỗi vai trò được sử dụng. Thay đổi có hiệu lực từ lần đăng nhập tiếp theo.")
    {
        var save = UiKit.Btn("Lưu phân quyền", BtnKind.Success);
        var reset = UiKit.Btn("Khôi phục", BtnKind.Secondary);
        Toolbar.Controls.AddRange(new Control[] { save, reset });

        _grid.ReadOnly = false;
        _grid.SelectionMode = DataGridViewSelectionMode.CellSelect;
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nhom", HeaderText = "Nhóm chức năng", Width = 230, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ma", HeaderText = "BFD", Width = 70, ReadOnly = true });
        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Ten", HeaderText = "Chức năng", Width = 220, ReadOnly = true, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        foreach (var vt in VaiTro.TatCa)
        {
            var c = new DataGridViewCheckBoxColumn { Name = vt, HeaderText = vt, Width = 150 };
            c.ReadOnly = vt == VaiTro.QuanTriVien; // Quản trị viên luôn có toàn quyền
            _grid.Columns.Add(c);
        }
        var card = UiKit.Card(padding: new Padding(1));
        card.Dock = DockStyle.Fill;
        card.Controls.Add(_grid);
        Body.Controls.Add(card);

        save.Click += (_, _) =>
        {
            foreach (DataGridViewRow r in _grid.Rows)
            {
                var key = (string)r.Tag!;
                foreach (var vt in VaiTro.TatCa)
                {
                    var on = r.Cells[vt].Value is true;
                    if (on) PhanQuyen[vt].Add(key); else PhanQuyen[vt].Remove(key);
                }
            }
            UiKit.Info("Đã lưu phân quyền.");
        };
        reset.Click += (_, _) => Reload();
        Reload();
    }

    public override void Reload()
    {
        _grid.Rows.Clear();
        foreach (var m in Menu.Items)
        {
            var values = new object[] { m.Nhom, m.MaBFD, m.Ten }
                .Concat(VaiTro.TatCa.Select(vt => (object)PhanQuyen[vt].Contains(m.Key))).ToArray();
            var i = _grid.Rows.Add(values);
            _grid.Rows[i].Tag = m.Key;
        }
    }
}
