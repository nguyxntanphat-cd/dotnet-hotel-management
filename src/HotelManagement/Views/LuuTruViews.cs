using HotelManagement.Data;
using HotelManagement.Forms;
using HotelManagement.Models;
using HotelManagement.UI;
using static HotelManagement.Data.MockDb;

namespace HotelManagement.Views;

/// <summary>4.1 Nhận phòng (theo đặt phòng hoặc khách vãng lai).</summary>
public class NhanPhongView : BaseView
{
    public static string? PreselectMaDat { get; set; }

    private record Row(string MaDat, string KhachHang, string SoPhong, string LoaiPhong, DateTime NgayNhan, DateTime NgayTra, string GhiChu, DatPhong E);

    private readonly DataGridView _grid = UiKit.Grid();
    private readonly TextBox _search = UiKit.Search("Mã đặt, tên khách, số phòng...");
    private readonly CheckBox _todayOnly = new() { Text = "Chỉ khách đến hôm nay", AutoSize = true, Checked = false, Margin = new Padding(0, 9, 16, 0) };
    private readonly Label _info = UiKit.Lbl("", Theme.Base);
    private readonly ListBox _guests = new() { Font = Theme.Base, BorderStyle = BorderStyle.FixedSingle, IntegralHeight = false };
    private readonly ComboBox _guestPick;
    private readonly Button _confirm = UiKit.Btn("✔  Xác nhận nhận phòng", BtnKind.Success, 330);

    public NhanPhongView() : base("Nhận phòng", "Chọn đặt phòng của khách đến, kiểm tra giấy tờ, bổ sung khách đi cùng và xác nhận nhận phòng.")
    {
        var walkIn = UiKit.Btn("+ Khách vãng lai (không đặt trước)", BtnKind.Secondary);
        Toolbar.Controls.AddRange(new Control[] { _search, _todayOnly, walkIn });

        _grid.Columns(
            new Col("MaDat", "Mã đặt", 80),
            new Col("KhachHang", "Khách hàng", 170, Fill: true),
            new Col("SoPhong", "Phòng", 70),
            new Col("LoaiPhong", "Loại", 90),
            new Col("NgayNhan", "Ngày nhận", 105, "dd/MM/yyyy"),
            new Col("NgayTra", "Ngày trả", 105, "dd/MM/yyyy"),
            new Col("GhiChu", "Ghi chú", 150));
        _grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is Row r && r.NgayNhan.Date <= DateTime.Today)
                e.CellStyle!.BackColor = Color.FromArgb(254, 249, 195);
        };
        var left = UiKit.Card(padding: new Padding(1, 48, 1, 1));
        left.Dock = DockStyle.Fill;
        var lt = UiKit.Lbl("Đặt phòng chờ nhận  (nền vàng: đến hạn hôm nay)", Theme.H2);
        lt.Location = new Point(16, 14);
        left.Controls.Add(_grid);
        left.Controls.Add(lt);

        // Bảng chi tiết bên phải
        var right = UiKit.Card("Thông tin nhận phòng");
        right.Dock = DockStyle.Right;
        right.Width = 370;
        _info.Location = new Point(18, 48);
        _info.MaximumSize = new Size(330, 0);
        var gl = UiKit.Lbl("Khách đi cùng", Theme.Bold);
        gl.Location = new Point(18, 236);
        _guestPick = UiKit.Combo(KhachHangs.Select(KhachHangView.KhLabel), 236);
        _guestPick.Location = new Point(18, 262);
        var addG = UiKit.Btn("Thêm", BtnKind.Secondary, 86);
        addG.Location = new Point(262, 260);
        _guests.Location = new Point(18, 302);
        _guests.Size = new Size(330, 110);
        var remG = UiKit.Btn("Bỏ khách đã chọn", BtnKind.Secondary);
        remG.Location = new Point(18, 420);
        _confirm.Location = new Point(18, 470);
        _confirm.Height = 44;
        right.Controls.AddRange(new Control[] { _info, gl, _guestPick, addG, _guests, remG, _confirm });

        Body.Controls.Add(left);
        Body.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 14 });
        Body.Controls.Add(right);

        _search.TextChanged += (_, _) => Reload();
        _todayOnly.CheckedChanged += (_, _) => Reload();
        _grid.SelectionChanged += (_, _) => ShowDetail();
        addG.Click += (_, _) =>
        {
            if (_guestPick.SelectedItem is string s && !_guests.Items.Contains(s) && _grid.Selected<Row>() is { } r && !s.StartsWith(r.E.MaKH + " "))
                _guests.Items.Add(s);
        };
        remG.Click += (_, _) => { if (_guests.SelectedItem != null) _guests.Items.Remove(_guests.SelectedItem); };
        _confirm.Click += (_, _) => Confirm();
        walkIn.Click += (_, _) => WalkIn();
        Reload();

        if (PreselectMaDat != null)
        {
            foreach (DataGridViewRow r in _grid.Rows)
                if (r.DataBoundItem is Row row && row.MaDat == PreselectMaDat) { r.Selected = true; _grid.CurrentCell = r.Cells[0]; }
            PreselectMaDat = null;
        }
    }

    public override void Reload()
    {
        var q = _search.Text.Trim().ToLower();
        _grid.Bind(DatPhongs.Where(d => d.TrangThai == TrangThaiDatPhong.ChoNhanPhong && (!_todayOnly.Checked || d.NgayNhanDuKien.Date <= DateTime.Today))
            .OrderBy(d => d.NgayNhanDuKien)
            .Select(d =>
            {
                var p = HotelService.PhongTheoMa(d.MaPhong);
                return new Row(d.MaDat, HotelService.Khach(d.MaKH)?.HoTen ?? "", p?.SoPhong ?? "", HotelService.Loai(p?.MaLoai)?.TenLoai ?? "", d.NgayNhanDuKien, d.NgayTraDuKien, d.GhiChu, d);
            })
            .Where(r => q == "" || r.MaDat.ToLower().Contains(q) || r.KhachHang.ToLower().Contains(q) || r.SoPhong.Contains(q)));
        ShowDetail();
    }

    private void ShowDetail()
    {
        _guests.Items.Clear();
        if (_grid.Selected<Row>() is not { } r)
        {
            _info.Text = "Chọn một đặt phòng ở danh sách bên trái.";
            _confirm.Enabled = false;
            return;
        }
        var k = HotelService.Khach(r.E.MaKH)!;
        var p = HotelService.PhongTheoMa(r.E.MaPhong)!;
        var l = HotelService.Loai(p.MaLoai)!;
        _info.Text =
            $"Mã đặt phòng:   {r.MaDat}\n" +
            $"Khách chính:      {k.HoTen}\n" +
            $"CCCD/Hộ chiếu:  {k.CCCD_HoChieu}\n" +
            $"Số điện thoại:    {k.SDT}\n\n" +
            $"Phòng:                {p.SoPhong} - {l.TenLoai} (tối đa {l.SucChua} người)\n" +
            $"Hiện trạng:         {p.TrangThai}\n" +
            $"Lưu trú:              {r.NgayNhan:dd/MM} → {r.NgayTra:dd/MM/yyyy}  ({HotelService.SoDem(r.NgayNhan, r.NgayTra)} đêm)\n" +
            $"Giá áp dụng:       {HotelService.GiaPhong(l.MaLoai, DateTime.Today):N0} đ / đêm";
        _confirm.Enabled = true;
    }

    private void Confirm()
    {
        if (_grid.Selected<Row>() is not { } r) return;
        var p = HotelService.PhongTheoMa(r.E.MaPhong)!;
        var l = HotelService.Loai(p.MaLoai)!;
        if (p.TrangThai is TrangThaiPhong.DangO or TrangThaiPhong.BaoTri)
        {
            UiKit.Warn($"Phòng {p.SoPhong} đang \"{p.TrangThai}\", không thể nhận phòng. Hãy sửa đặt phòng sang phòng khác.");
            return;
        }
        if (p.TrangThai == TrangThaiPhong.DangDon && !UiKit.Confirm($"Phòng {p.SoPhong} đang được dọn. Vẫn nhận phòng?")) return;
        if (r.NgayNhan.Date > DateTime.Today && !UiKit.Confirm($"Đặt phòng này nhận vào {r.NgayNhan:dd/MM/yyyy}. Cho khách nhận phòng sớm?")) return;
        if (_guests.Items.Count + 1 > l.SucChua)
        {
            UiKit.Warn($"Phòng {l.TenLoai} chỉ tối đa {l.SucChua} người.");
            return;
        }
        var diCung = _guests.Items.Cast<string>().Select(s => s.Split(" - ")[0]).ToList();
        var stay = HotelService.NhanPhong(p.MaPhong, r.E.MaKH, r.NgayTra, r.E, diCung);
        UiKit.Info($"Nhận phòng thành công!\n\nPhòng {p.SoPhong} - {r.KhachHang}\nMã lưu trú: {stay.MaLuuTru}\nTrả phòng dự kiến: {stay.NgayTra:dd/MM/yyyy HH:mm}");
        Reload();
    }

    private void WalkIn()
    {
        const string themMoi = "➕ Thêm khách hàng mới...";
        using var d = new FormDialog("Nhận phòng cho khách vãng lai");
        var khItems = new[] { themMoi }.Concat(KhachHangs.Select(KhachHangView.KhLabel)).ToList();
        var kh = d.ComboField("Khách hàng", khItems, khItems[1]);
        var tra = d.DateField("Ngày trả dự kiến", DateTime.Today.AddDays(1));
        var loai = d.ComboField("Loại phòng", LoaiPhongs.Select(l => l.TenLoai));
        var phong = d.ComboField("Phòng trống", Array.Empty<string>());
        var est = d.Note("", Theme.Primary);
        void refresh()
        {
            phong.Items.Clear();
            var lp = LoaiPhongs[loai.SelectedIndex];
            if (tra.Value.Date > DateTime.Today)
                foreach (var p in HotelService.PhongTrong(DateTime.Today, tra.Value, lp.MaLoai).Where(p => p.TrangThai == TrangThaiPhong.Trong))
                    phong.Items.Add(p.SoPhong);
            if (phong.Items.Count > 0) phong.SelectedIndex = 0;
            var dem = HotelService.SoDem(DateTime.Today, tra.Value);
            var gia = HotelService.GiaPhong(lp.MaLoai, DateTime.Today);
            est.Text = $"{phong.Items.Count} phòng sẵn sàng • {dem} đêm × {gia:N0} đ = {dem * gia:N0} đ";
        }
        kh.SelectedIndexChanged += (_, _) =>
        {
            if (kh.SelectedIndex != 0) return;
            var moi = KhachHangView.EditDialog(d, null);
            if (moi == null) { kh.SelectedIndex = 1; return; }
            kh.Items.Add(KhachHangView.KhLabel(moi));
            kh.SelectedIndex = kh.Items.Count - 1;
        };
        loai.SelectedIndexChanged += (_, _) => refresh();
        tra.ValueChanged += (_, _) => refresh();
        refresh();
        d.Validator = () => kh.SelectedIndex <= 0 ? "Vui lòng chọn khách hàng."
            : tra.Value.Date <= DateTime.Today ? "Ngày trả phải sau hôm nay."
            : phong.SelectedItem == null ? "Không còn phòng sẵn sàng thuộc loại này." : null;
        if (d.ShowDialog(this) != DialogResult.OK) return;

        var maKH = ((string)kh.SelectedItem!).Split(" - ")[0];
        var maPhong = Phongs.First(p => p.SoPhong == (string)phong.SelectedItem!).MaPhong;
        var stay = HotelService.NhanPhong(maPhong, maKH, tra.Value, null, Array.Empty<string>());
        UiKit.Info($"Nhận phòng thành công!\nPhòng {phong.SelectedItem} - mã lưu trú {stay.MaLuuTru}.");
        Reload();
    }
}

/// <summary>4.2 Quản lý lưu trú, 4.5 Theo dõi khách đang lưu trú.</summary>
public class LuuTruView : BaseView
{
    private record Row(string MaLuuTru, string SoPhong, string LoaiPhong, string KhachChinh, int SoKhach, DateTime NgayNhan, DateTime NgayTra,
        int SoDem, decimal GiaPhong, decimal TienDV, string TrangThai, LuuTru E);
    private record KhachRow(string MaKH, string HoTen, string CCCD, string SDT, string VaiTro, ChiTietLuuTru E);
    private record DvRow(DateTime ThoiGian, string TenDV, int SoLuong, decimal DonGia, decimal ThanhTien);

    private readonly DataGridView _grid = UiKit.Grid();
    private readonly DataGridView _khach = UiKit.Grid();
    private readonly DataGridView _dv = UiKit.Grid();
    private readonly TextBox _search = UiKit.Search("Số phòng, tên khách...", 200);
    private readonly ComboBox _filter = UiKit.Combo(new[] { "Đang ở", "Đã trả phòng", "Tất cả" }, 130);
    private readonly Label _summary = UiKit.Lbl("", Theme.Bold, Theme.Primary);

    public LuuTruView() : base("Khách đang lưu trú", "Theo dõi các phòng đang có khách, danh sách khách và chi phí dịch vụ phát sinh.")
    {
        var addGuest = UiKit.Btn("Thêm khách", BtnKind.Secondary);
        var extend = UiKit.Btn("Gia hạn", BtnKind.Secondary);
        var move = UiKit.Btn("Đổi phòng", BtnKind.Secondary);
        var service = UiKit.Btn("Ghi nhận dịch vụ →");
        var checkout = UiKit.Btn("Trả phòng →", BtnKind.Success);
        Toolbar.Controls.AddRange(new Control[] { _search, _filter });
        Spacer(8);
        Toolbar.Controls.AddRange(new Control[] { addGuest, extend, move });
        if (Session.CoQuyen("su_dung_dv")) Toolbar.Controls.Add(service);
        if (Session.CoQuyen("tra_phong")) Toolbar.Controls.Add(checkout);

        _grid.Columns(
            new Col("MaLuuTru", "Mã LT", 80),
            new Col("SoPhong", "Phòng", 65),
            new Col("LoaiPhong", "Loại", 85),
            new Col("KhachChinh", "Khách chính", 160, Fill: true),
            new Col("SoKhach", "Số khách", 75, Right: true),
            new Col("NgayNhan", "Nhận phòng", 125, "dd/MM/yyyy HH:mm"),
            new Col("NgayTra", "Trả (dự kiến)", 125, "dd/MM/yyyy HH:mm"),
            new Col("SoDem", "Đêm", 55, Right: true),
            new Col("GiaPhong", "Giá / đêm", 105, "N0", Right: true),
            new Col("TienDV", "Tiền DV", 100, "N0", Right: true),
            new Col("TrangThai", "Trạng thái", 110, Status: true));
        _grid.CellFormatting += (_, e) =>
        {
            if (e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is Row r && r.TrangThai == TrangThaiLuuTru.DangO && r.NgayTra.Date <= DateTime.Today)
                e.CellStyle!.BackColor = Color.FromArgb(254, 226, 226);
        };
        _khach.Columns(new Col("MaKH", "Mã KH", 70), new Col("HoTen", "Họ tên", 160, Fill: true), new Col("CCCD", "CCCD/HC", 120), new Col("SDT", "SĐT", 110), new Col("VaiTro", "Vai trò", 110));
        _dv.Columns(new Col("ThoiGian", "Thời gian", 115, "dd/MM HH:mm"), new Col("TenDV", "Dịch vụ", 150, Fill: true), new Col("SoLuong", "SL", 45, Right: true),
            new Col("DonGia", "Đơn giá", 90, "N0", Right: true), new Col("ThanhTien", "Thành tiền", 100, "N0", Right: true));

        var summaryBar = new Panel { Dock = DockStyle.Top, Height = 30 };
        _summary.Location = new Point(2, 4);
        summaryBar.Controls.Add(_summary);

        var top = UiKit.Card(padding: new Padding(1));
        top.Dock = DockStyle.Fill;
        top.Controls.Add(_grid);

        var bottom = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 230, ColumnCount = 3, RowCount = 1 };
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 14));
        bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        Panel Box(string title, Control grid)
        {
            var c = UiKit.Card(padding: new Padding(1, 46, 1, 1));
            c.Dock = DockStyle.Fill;
            c.Margin = new Padding(0);
            var t = UiKit.Lbl(title, Theme.H2);
            t.Location = new Point(14, 12);
            c.Controls.Add(grid);
            c.Controls.Add(t);
            return c;
        }
        bottom.Controls.Add(Box("Khách trong phòng", _khach), 0, 0);
        bottom.Controls.Add(Box("Dịch vụ đã sử dụng", _dv), 2, 0);

        Body.Controls.Add(top);
        Body.Controls.Add(summaryBar);
        Body.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 14 });
        Body.Controls.Add(bottom);

        _search.TextChanged += (_, _) => Reload();
        _filter.SelectedIndexChanged += (_, _) => Reload();
        _grid.SelectionChanged += (_, _) => ShowDetail();
        addGuest.Click += (_, _) => AddGuest();
        extend.Click += (_, _) => Extend();
        move.Click += (_, _) => DoiPhong();
        service.Click += (_, _) =>
        {
            if (_grid.Selected<Row>() is { } r && r.TrangThai == TrangThaiLuuTru.DangO) SuDungDichVuView.PreselectMaLuuTru = r.MaLuuTru;
            MainForm.Current?.Navigate("su_dung_dv");
        };
        checkout.Click += (_, _) =>
        {
            if (_grid.Selected<Row>() is { } r && r.TrangThai == TrangThaiLuuTru.DangO) TraPhongView.PreselectMaLuuTru = r.MaLuuTru;
            MainForm.Current?.Navigate("tra_phong");
        };
        Reload();
    }

    public override void Reload()
    {
        var q = _search.Text.Trim().ToLower();
        var f = (string)_filter.SelectedItem!;
        var rows = LuuTrus
            .Where(l => f == "Tất cả" || l.TrangThai == f)
            .Select(l =>
            {
                var p = HotelService.PhongTheoMa(l.MaPhong);
                return new Row(l.MaLuuTru, p?.SoPhong ?? "", HotelService.Loai(p?.MaLoai)?.TenLoai ?? "", HotelService.KhachChinh(l)?.HoTen ?? "",
                    ChiTietLuuTrus.Count(c => c.MaLuuTru == l.MaLuuTru), l.NgayNhan, l.NgayTra, HotelService.SoDem(l.NgayNhan, l.NgayTra),
                    l.GiaPhongApDung, HotelService.TinhTienDichVu(l), l.TrangThai, l);
            })
            .Where(r => q == "" || r.SoPhong.Contains(q) || r.KhachChinh.ToLower().Contains(q) || r.MaLuuTru.ToLower().Contains(q))
            .OrderByDescending(r => r.TrangThai == TrangThaiLuuTru.DangO).ThenBy(r => r.TrangThai == TrangThaiLuuTru.DangO ? r.SoPhong : "").ThenByDescending(r => r.NgayNhan)
            .ToList();
        _grid.Bind(rows);

        var dangO = LuuTrus.Where(l => l.TrangThai == TrangThaiLuuTru.DangO).ToList();
        _summary.Text = $"Đang lưu trú: {dangO.Count} phòng  •  {dangO.Sum(l => ChiTietLuuTrus.Count(c => c.MaLuuTru == l.MaLuuTru))} khách  •  " +
                        $"Trả phòng hôm nay: {dangO.Count(l => l.NgayTra.Date == DateTime.Today)}  •  Quá hạn: {dangO.Count(l => l.NgayTra.Date < DateTime.Today)}   (nền đỏ: đến hạn trả)";
        ShowDetail();
    }

    private void ShowDetail()
    {
        var r = _grid.Selected<Row>();
        _khach.Bind(ChiTietLuuTrus.Where(c => c.MaLuuTru == r?.MaLuuTru).Select(c =>
        {
            var k = HotelService.Khach(c.MaKH);
            return new KhachRow(c.MaKH, k?.HoTen ?? "", k?.CCCD_HoChieu ?? "", k?.SDT ?? "", c.VaiTro, c);
        }));
        _dv.Bind(SuDungDichVus.Where(s => s.MaLuuTru == r?.MaLuuTru).OrderBy(s => s.ThoiGian)
            .Select(s => new DvRow(s.ThoiGian, HotelService.DV(s.MaDV)?.TenDV ?? "", s.SoLuong, s.DonGiaApDung, s.ThanhTien)));
    }

    private Row? Active()
    {
        if (_grid.Selected<Row>() is { } r && r.TrangThai == TrangThaiLuuTru.DangO) return r;
        UiKit.Warn("Hãy chọn một lượt lưu trú đang ở.");
        return null;
    }

    private void AddGuest()
    {
        if (Active() is not { } r) return;
        var l = HotelService.Loai(HotelService.PhongTheoMa(r.E.MaPhong)?.MaLoai)!;
        if (r.SoKhach >= l.SucChua) { UiKit.Warn($"Phòng đã đủ {l.SucChua} người."); return; }
        using var d = new FormDialog($"Thêm khách vào phòng {r.SoPhong}");
        var kh = d.ComboField("Khách hàng", KhachHangs.Where(k => !ChiTietLuuTrus.Any(c => c.MaLuuTru == r.MaLuuTru && c.MaKH == k.MaKH)).Select(KhachHangView.KhLabel));
        var note = d.TextField("Ghi chú", "");
        if (d.ShowDialog(this) != DialogResult.OK || kh.SelectedItem == null) return;
        ChiTietLuuTrus.Add(new ChiTietLuuTru
        {
            MaCTLT = NextId(ChiTietLuuTrus, c => c.MaCTLT, "CT", 4), MaLuuTru = r.MaLuuTru,
            MaKH = ((string)kh.SelectedItem).Split(" - ")[0], VaiTro = "Khách đi cùng", GhiChu = note.Text.Trim(),
        });
        Reload();
    }

    private void Extend()
    {
        if (Active() is not { } r) return;
        using var d = new FormDialog($"Gia hạn lưu trú phòng {r.SoPhong}");
        d.Note($"Trả phòng dự kiến hiện tại: {r.NgayTra:dd/MM/yyyy}");
        var tra = d.DateField("Ngày trả mới", r.NgayTra.AddDays(1));
        d.Validator = () =>
        {
            if (tra.Value.Date <= r.NgayNhan.Date) return "Ngày trả phải sau ngày nhận.";
            var trung = DatPhongs.FirstOrDefault(x => x.MaPhong == r.E.MaPhong && x.TrangThai == TrangThaiDatPhong.ChoNhanPhong
                && x.NgayNhanDuKien.Date < tra.Value.Date && x.NgayNhanDuKien.Date >= r.NgayTra.Date);
            return trung != null ? $"Phòng đã có đặt phòng {trung.MaDat} từ {trung.NgayNhanDuKien:dd/MM}. Hãy đổi phòng hoặc chọn ngày sớm hơn." : null;
        };
        if (d.ShowDialog(this) != DialogResult.OK) return;
        r.E.NgayTra = tra.Value.Date.AddHours(12);
        Reload();
    }

    private void DoiPhong()
    {
        if (Active() is not { } r) return;
        using var d = new FormDialog($"Đổi phòng cho khách phòng {r.SoPhong}");
        var rooms = HotelService.PhongTrong(DateTime.Today, r.NgayTra).Where(p => p.TrangThai == TrangThaiPhong.Trong)
            .Select(p => $"{p.SoPhong} - {HotelService.Loai(p.MaLoai)?.TenLoai}").ToList();
        var phong = d.ComboField("Chuyển sang phòng", rooms);
        var giu = d.Field("", new CheckBox { Text = "Giữ nguyên giá phòng cũ", Checked = true, AutoSize = true });
        d.Validator = () => phong.SelectedItem == null ? "Không có phòng trống sẵn sàng." : null;
        if (d.ShowDialog(this) != DialogResult.OK) return;
        var moi = Phongs.First(p => p.SoPhong == ((string)phong.SelectedItem!).Split(" - ")[0]);
        var cu = HotelService.PhongTheoMa(r.E.MaPhong)!;
        cu.TrangThai = TrangThaiPhong.DangDon;
        moi.TrangThai = TrangThaiPhong.DangO;
        r.E.MaPhong = moi.MaPhong;
        if (!giu.Checked) r.E.GiaPhongApDung = HotelService.GiaPhong(moi.MaLoai, DateTime.Today);
        UiKit.Info($"Đã chuyển khách từ phòng {cu.SoPhong} sang phòng {moi.SoPhong}. Phòng {cu.SoPhong} chuyển sang \"Đang dọn\".");
        Reload();
    }
}

/// <summary>4.3 Quản lý dịch vụ.</summary>
public class DichVuView : BaseView
{
    private readonly DataGridView _grid = UiKit.Grid();
    private readonly TextBox _search = UiKit.Search("Tìm dịch vụ...");

    public DichVuView() : base("Danh mục dịch vụ", "Các dịch vụ khách sạn cung cấp và đơn giá hiện hành.")
    {
        var add = UiKit.Btn("+ Thêm dịch vụ");
        var edit = UiKit.Btn("Sửa", BtnKind.Secondary);
        var toggle = UiKit.Btn("Ngừng / Mở kinh doanh", BtnKind.Secondary);
        var del = UiKit.Btn("Xóa", BtnKind.Danger);
        Toolbar.Controls.Add(_search);
        Spacer();
        Toolbar.Controls.AddRange(new Control[] { add, edit, toggle, del });

        _grid.Columns(
            new Col("MaDV", "Mã DV", 80),
            new Col("TenDV", "Tên dịch vụ", 220, Fill: true),
            new Col("DonGia", "Đơn giá", 140, "N0", Right: true),
            new Col("DonViTinh", "Đơn vị tính", 120),
            new Col("TrangThai", "Trạng thái", 160, Status: true));
        var card = UiKit.Card(padding: new Padding(1));
        card.Dock = DockStyle.Fill;
        card.Controls.Add(_grid);
        Body.Controls.Add(card);

        _search.TextChanged += (_, _) => Reload();
        add.Click += (_, _) => Edit(null);
        edit.Click += (_, _) => { if (_grid.Selected<DichVu>() is { } x) Edit(x); };
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0 && _grid.Selected<DichVu>() is { } x) Edit(x); };
        toggle.Click += (_, _) =>
        {
            if (_grid.Selected<DichVu>() is not { } x) return;
            x.TrangThai = x.TrangThai == TrangThaiDichVu.DangKinhDoanh ? TrangThaiDichVu.NgungKinhDoanh : TrangThaiDichVu.DangKinhDoanh;
            Reload();
        };
        del.Click += (_, _) =>
        {
            if (_grid.Selected<DichVu>() is not { } x) return;
            if (SuDungDichVus.Any(s => s.MaDV == x.MaDV)) { UiKit.Warn("Dịch vụ đã phát sinh sử dụng, chỉ có thể ngừng kinh doanh."); return; }
            if (!UiKit.Confirm($"Xóa dịch vụ \"{x.TenDV}\"?")) return;
            DichVus.Remove(x);
            Reload();
        };
        Reload();
    }

    public override void Reload()
    {
        var q = _search.Text.Trim().ToLower();
        _grid.Bind(DichVus.Where(d => q == "" || d.TenDV.ToLower().Contains(q) || d.MaDV.ToLower().Contains(q)));
    }

    private void Edit(DichVu? dv)
    {
        using var d = new FormDialog(dv == null ? "Thêm dịch vụ" : "Sửa dịch vụ");
        var ten = d.TextField("Tên dịch vụ", dv?.TenDV ?? "");
        var gia = d.NumberField("Đơn giá", dv?.DonGia ?? 50_000, 0, 100_000_000, 5_000);
        var dvt = d.TextField("Đơn vị tính", dv?.DonViTinh ?? "lần");
        var tt = d.ComboField("Trạng thái", new[] { TrangThaiDichVu.DangKinhDoanh, TrangThaiDichVu.NgungKinhDoanh }, dv?.TrangThai);
        d.Validator = () => string.IsNullOrWhiteSpace(ten.Text) ? "Tên dịch vụ không được để trống."
            : string.IsNullOrWhiteSpace(dvt.Text) ? "Đơn vị tính không được để trống." : null;
        if (d.ShowDialog(this) != DialogResult.OK) return;
        if (dv == null)
        {
            dv = new DichVu { MaDV = NextId(DichVus, x => x.MaDV, "DV", 2) };
            DichVus.Add(dv);
        }
        dv.TenDV = ten.Text.Trim();
        dv.DonGia = gia.Value;
        dv.DonViTinh = dvt.Text.Trim();
        dv.TrangThai = (string)tt.SelectedItem!;
        Reload();
    }
}

/// <summary>4.4 Ghi nhận sử dụng dịch vụ.</summary>
public class SuDungDichVuView : BaseView
{
    public static string? PreselectMaLuuTru { get; set; }

    private record Row(string MaSD, DateTime ThoiGian, string TenDV, string DonViTinh, int SoLuong, decimal DonGiaApDung, decimal ThanhTien, SuDungDichVu E);

    private readonly ComboBox _stay = UiKit.Combo(Array.Empty<string>(), 340);
    private readonly DataGridView _grid = UiKit.Grid();
    private readonly Label _total = UiKit.Lbl("", Theme.H2, Theme.Primary);
    private readonly ComboBox _dv = UiKit.Combo(Array.Empty<string>(), 300);
    private readonly NumericUpDown _sl = new() { Minimum = 1, Maximum = 100, Value = 1, Width = 100, Font = Theme.Base };
    private readonly Label _preview = UiKit.Lbl("", Theme.Bold, Theme.Text);
    private List<LuuTru> _stays = new();

    public SuDungDichVuView() : base("Ghi nhận sử dụng dịch vụ", "Chọn phòng đang ở và ghi nhận dịch vụ khách sử dụng; chi phí sẽ cộng vào hóa đơn khi trả phòng.")
    {
        var l = UiKit.Lbl("Phòng đang ở:", Theme.Bold);
        l.Margin = new Padding(0, 8, 8, 0);
        var del = UiKit.Btn("Xóa dòng đã chọn", BtnKind.Danger);
        Toolbar.Controls.AddRange(new Control[] { l, _stay });
        Spacer();
        Toolbar.Controls.Add(del);

        _grid.Columns(
            new Col("MaSD", "Mã", 75),
            new Col("ThoiGian", "Thời gian", 130, "dd/MM/yyyy HH:mm"),
            new Col("TenDV", "Dịch vụ", 180, Fill: true),
            new Col("DonViTinh", "ĐVT", 70),
            new Col("SoLuong", "Số lượng", 85, Right: true),
            new Col("DonGiaApDung", "Đơn giá", 110, "N0", Right: true),
            new Col("ThanhTien", "Thành tiền", 120, "N0", Right: true));
        var left = UiKit.Card(padding: new Padding(1, 1, 1, 50));
        left.Dock = DockStyle.Fill;
        left.Controls.Add(_grid);
        left.Controls.Add(_total);
        left.Resize += (_, _) => _total.Location = new Point(left.Width - _total.Width - 18, left.Height - 38);
        _total.SizeChanged += (_, _) => _total.Location = new Point(left.Width - _total.Width - 18, left.Height - 38);

        var right = UiKit.Card("Thêm dịch vụ");
        right.Dock = DockStyle.Right;
        right.Width = 340;
        var l1 = UiKit.Lbl("Dịch vụ", Theme.Base, Theme.Muted);
        l1.Location = new Point(18, 54);
        _dv.Location = new Point(18, 78);
        var l2 = UiKit.Lbl("Số lượng", Theme.Base, Theme.Muted);
        l2.Location = new Point(18, 120);
        _sl.Location = new Point(18, 144);
        _preview.Location = new Point(18, 186);
        _preview.MaximumSize = new Size(300, 0);
        var add = UiKit.Btn("+ Ghi nhận", BtnKind.Primary, 300);
        add.Height = 40;
        add.Location = new Point(18, 236);
        right.Controls.AddRange(new Control[] { l1, _dv, l2, _sl, _preview, add });

        Body.Controls.Add(left);
        Body.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 14 });
        Body.Controls.Add(right);

        foreach (var d in DichVus.Where(d => d.TrangThai == TrangThaiDichVu.DangKinhDoanh)) _dv.Items.Add($"{d.TenDV}  ({d.DonGia:N0} đ/{d.DonViTinh})");
        if (_dv.Items.Count > 0) _dv.SelectedIndex = 0;

        _stay.SelectedIndexChanged += (_, _) => LoadUsage();
        _dv.SelectedIndexChanged += (_, _) => UpdatePreview();
        _sl.ValueChanged += (_, _) => UpdatePreview();
        add.Click += (_, _) => Add();
        del.Click += (_, _) =>
        {
            if (_grid.Selected<Row>() is not { } r || !UiKit.Confirm($"Xóa \"{r.TenDV}\" x{r.SoLuong}?")) return;
            SuDungDichVus.Remove(r.E);
            LoadUsage();
        };
        Reload();
        UpdatePreview();
    }

    private DichVu? SelectedDV => _dv.SelectedIndex < 0 ? null : DichVus.Where(d => d.TrangThai == TrangThaiDichVu.DangKinhDoanh).ElementAt(_dv.SelectedIndex);
    private LuuTru? SelectedStay => _stay.SelectedIndex < 0 ? null : _stays[_stay.SelectedIndex];

    public override void Reload()
    {
        _stays = LuuTrus.Where(l => l.TrangThai == TrangThaiLuuTru.DangO).OrderBy(l => HotelService.PhongTheoMa(l.MaPhong)?.SoPhong).ToList();
        _stay.Items.Clear();
        foreach (var l in _stays) _stay.Items.Add($"Phòng {HotelService.PhongTheoMa(l.MaPhong)?.SoPhong} - {HotelService.KhachChinh(l)?.HoTen}  ({l.MaLuuTru})");
        var idx = PreselectMaLuuTru == null ? 0 : _stays.FindIndex(l => l.MaLuuTru == PreselectMaLuuTru);
        PreselectMaLuuTru = null;
        if (_stays.Count > 0) _stay.SelectedIndex = Math.Max(0, idx);
        LoadUsage();
    }

    private void LoadUsage()
    {
        var lt = SelectedStay;
        var rows = SuDungDichVus.Where(s => s.MaLuuTru == lt?.MaLuuTru).OrderBy(s => s.ThoiGian).Select(s =>
        {
            var d = HotelService.DV(s.MaDV);
            return new Row(s.MaSD, s.ThoiGian, d?.TenDV ?? "", d?.DonViTinh ?? "", s.SoLuong, s.DonGiaApDung, s.ThanhTien, s);
        }).ToList();
        _grid.Bind(rows);
        _total.Text = $"Tổng tiền dịch vụ: {rows.Sum(r => r.ThanhTien):N0} đ";
    }

    private void UpdatePreview()
    {
        if (SelectedDV is not { } d) return;
        _preview.Text = $"Thành tiền: {_sl.Value:N0} × {d.DonGia:N0} = {_sl.Value * d.DonGia:N0} đ";
    }

    private void Add()
    {
        if (SelectedStay is not { } lt) { UiKit.Warn("Không có phòng nào đang ở."); return; }
        if (SelectedDV is not { } d) return;
        SuDungDichVus.Add(new SuDungDichVu
        {
            MaSD = NextId(SuDungDichVus, s => s.MaSD, "SD", 4), MaLuuTru = lt.MaLuuTru, MaDV = d.MaDV,
            SoLuong = (int)_sl.Value, DonGiaApDung = d.DonGia, ThoiGian = DateTime.Now,
        });
        _sl.Value = 1;
        LoadUsage();
    }
}
