using HotelManagement.Data;
using HotelManagement.Models;
using HotelManagement.UI;
using static HotelManagement.Data.MockDb;

namespace HotelManagement.Views;

/// <summary>2.1 Quản lý loại phòng và bảng giá theo mùa.</summary>
public class LoaiPhongView : BaseView
{
    private record LoaiRow(string MaLoai, string TenLoai, decimal GiaCoBan, int SucChua, int SoPhong, string MoTa, LoaiPhong E);
    private record GiaRow(string MaGia, DateTime TuNgay, DateTime DenNgay, decimal GiaApDung, string GhiChu, BangGia E);

    private readonly DataGridView _loai = UiKit.Grid();
    private readonly DataGridView _gia = UiKit.Grid();
    private readonly Label _giaTitle = UiKit.Lbl("", Theme.H2);

    public LoaiPhongView() : base("Loại phòng & bảng giá", "Cấu hình loại phòng, giá cơ bản và giá áp dụng theo khoảng thời gian (lễ, Tết, cao điểm).")
    {
        var add = UiKit.Btn("+ Thêm loại phòng");
        var edit = UiKit.Btn("Sửa", BtnKind.Secondary);
        var del = UiKit.Btn("Xóa", BtnKind.Danger);
        Toolbar.Controls.AddRange(new Control[] { add, edit, del });

        _loai.Columns(
            new Col("MaLoai", "Mã loại", 80),
            new Col("TenLoai", "Tên loại", 130),
            new Col("GiaCoBan", "Giá cơ bản / đêm", 150, "N0", Right: true),
            new Col("SucChua", "Sức chứa", 90, Right: true),
            new Col("SoPhong", "Số phòng", 90, Right: true),
            new Col("MoTa", "Mô tả", 300, Fill: true));
        var top = UiKit.Card(padding: new Padding(1));
        top.Dock = DockStyle.Top;
        top.Height = 240;
        top.Controls.Add(_loai);

        // Bảng giá của loại đang chọn
        var bottom = UiKit.Card(padding: new Padding(1, 52, 1, 1));
        bottom.Dock = DockStyle.Fill;
        _giaTitle.Location = new Point(16, 14);
        var addGia = UiKit.Btn("+ Thêm giá áp dụng", BtnKind.Secondary);
        var delGia = UiKit.Btn("Xóa", BtnKind.Secondary);
        var giaBtns = UiKit.Row(addGia, delGia);
        bottom.Resize += (_, _) => giaBtns.Location = new Point(bottom.Width - giaBtns.Width - 12, 9);
        _gia.Columns(
            new Col("MaGia", "Mã giá", 80),
            new Col("TuNgay", "Từ ngày", 120, "dd/MM/yyyy"),
            new Col("DenNgay", "Đến ngày", 120, "dd/MM/yyyy"),
            new Col("GiaApDung", "Giá áp dụng / đêm", 160, "N0", Right: true),
            new Col("GhiChu", "Ghi chú", 200, Fill: true));
        bottom.Controls.Add(_gia);
        bottom.Controls.Add(_giaTitle);
        bottom.Controls.Add(giaBtns);

        Body.Controls.Add(bottom);
        Body.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 14 });
        Body.Controls.Add(top);

        _loai.SelectionChanged += (_, _) => LoadGia();
        add.Click += (_, _) => EditLoai(null);
        edit.Click += (_, _) => { if (_loai.Selected<LoaiRow>() is { } r) EditLoai(r.E); };
        _loai.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0 && _loai.Selected<LoaiRow>() is { } r) EditLoai(r.E); };
        del.Click += (_, _) =>
        {
            if (_loai.Selected<LoaiRow>() is not { } r) return;
            if (r.SoPhong > 0) { UiKit.Warn("Không thể xóa loại phòng đang có phòng sử dụng."); return; }
            if (!UiKit.Confirm($"Xóa loại phòng \"{r.TenLoai}\"?")) return;
            LoaiPhongs.Remove(r.E);
            BangGias.RemoveAll(b => b.MaLoai == r.MaLoai);
            Reload();
        };
        addGia.Click += (_, _) => AddGia();
        delGia.Click += (_, _) =>
        {
            if (_gia.Selected<GiaRow>() is not { } g || !UiKit.Confirm("Xóa mức giá này?")) return;
            BangGias.Remove(g.E);
            LoadGia();
        };
        Reload();
    }

    public override void Reload()
    {
        _loai.Bind(LoaiPhongs.Select(l => new LoaiRow(l.MaLoai, l.TenLoai, l.GiaCoBan, l.SucChua, Phongs.Count(p => p.MaLoai == l.MaLoai), l.MoTa, l)));
        LoadGia();
    }

    private void LoadGia()
    {
        var l = _loai.Selected<LoaiRow>();
        _giaTitle.Text = l == null ? "Bảng giá" : $"Bảng giá theo thời gian - {l.TenLoai}";
        _gia.Bind(BangGias.Where(b => b.MaLoai == l?.MaLoai).OrderBy(b => b.TuNgay)
            .Select(b => new GiaRow(b.MaGia, b.TuNgay, b.DenNgay, b.GiaApDung, b.GhiChu, b)));
    }

    private void EditLoai(LoaiPhong? lp)
    {
        using var d = new FormDialog(lp == null ? "Thêm loại phòng" : "Sửa loại phòng");
        var ten = d.TextField("Tên loại", lp?.TenLoai ?? "");
        var gia = d.NumberField("Giá cơ bản / đêm", lp?.GiaCoBan ?? 500_000, 0, 100_000_000, 50_000);
        var sc = d.NumberField("Sức chứa (người)", lp?.SucChua ?? 2, 1, 20);
        var mota = d.Field("Mô tả", new TextBox { Text = lp?.MoTa ?? "", Multiline = true, Font = Theme.Base }, 70);
        d.Validator = () => string.IsNullOrWhiteSpace(ten.Text) ? "Tên loại không được để trống."
            : gia.Value <= 0 ? "Giá phải lớn hơn 0." : null;
        if (d.ShowDialog(this) != DialogResult.OK) return;
        if (lp == null)
        {
            lp = new LoaiPhong { MaLoai = NextId(LoaiPhongs, l => l.MaLoai, "LP", 2) };
            LoaiPhongs.Add(lp);
        }
        lp.TenLoai = ten.Text.Trim();
        lp.GiaCoBan = gia.Value;
        lp.SucChua = (int)sc.Value;
        lp.MoTa = mota.Text.Trim();
        Reload();
    }

    private void AddGia()
    {
        if (_loai.Selected<LoaiRow>() is not { } l) return;
        using var d = new FormDialog($"Thêm giá áp dụng - {l.TenLoai}");
        var tu = d.DateField("Từ ngày", DateTime.Today);
        var den = d.DateField("Đến ngày", DateTime.Today.AddDays(7));
        var gia = d.NumberField("Giá áp dụng / đêm", l.GiaCoBan, 0, 100_000_000, 50_000);
        var note = d.TextField("Ghi chú", "");
        d.Validator = () =>
        {
            if (den.Value.Date < tu.Value.Date) return "Ngày kết thúc phải sau ngày bắt đầu.";
            if (BangGias.Any(b => b.MaLoai == l.MaLoai && tu.Value.Date <= b.DenNgay && b.TuNgay <= den.Value.Date))
                return "Khoảng thời gian bị trùng với một mức giá đã có.";
            return null;
        };
        if (d.ShowDialog(this) != DialogResult.OK) return;
        BangGias.Add(new BangGia
        {
            MaGia = NextId(BangGias, b => b.MaGia, "BG"), MaLoai = l.MaLoai,
            TuNgay = tu.Value.Date, DenNgay = den.Value.Date, GiaApDung = gia.Value, GhiChu = note.Text.Trim(),
        });
        LoadGia();
    }
}

/// <summary>2.2 Quản lý phòng, 2.3 Cập nhật trạng thái phòng, 2.4 Tra cứu phòng.</summary>
public class PhongView : BaseView
{
    private readonly FlowLayoutPanel _map = new() { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Theme.Background, Padding = new Padding(0, 0, 0, 12) };
    private readonly ComboBox _tang = UiKit.Combo(new[] { "Tất cả tầng" }.Concat(Phongs.Select(p => p.Tang).Distinct().OrderBy(t => t).Select(t => $"Tầng {t}")), 120);
    private readonly ComboBox _loai = UiKit.Combo(new[] { "Tất cả loại" }.Concat(LoaiPhongs.Select(l => l.TenLoai)), 140);
    private readonly ComboBox _tt = UiKit.Combo(new[] { "Tất cả trạng thái" }.Concat(TrangThaiPhong.TatCa), 150);
    private readonly TextBox _search = UiKit.Search("Số phòng...", 120);
    private readonly FlowLayoutPanel _legend = new() { Dock = DockStyle.Top, Height = 34, BackColor = Theme.Background };
    private Phong? _selected;
    private readonly Dictionary<Phong, BorderPanel> _tiles = new();

    public PhongView() : base("Sơ đồ phòng", "Danh sách phòng theo tầng và trạng thái thời gian thực. Nhấp chuột phải vào phòng để cập nhật trạng thái.")
    {
        bool laBuongPhong = Session.VaiTro == VaiTro.BuongPhong;
        var find = UiKit.Btn("Tra cứu phòng trống", BtnKind.Secondary);
        var add = UiKit.Btn("+ Thêm phòng");
        var edit = UiKit.Btn("Sửa", BtnKind.Secondary);
        var status = UiKit.Btn("Cập nhật trạng thái", BtnKind.Secondary);
        Toolbar.Controls.AddRange(new Control[] { _search, _tang, _loai, _tt });
        Spacer();
        if (!laBuongPhong) Toolbar.Controls.Add(find);
        Toolbar.Controls.Add(status);
        if (Session.VaiTro == VaiTro.QuanTriVien) Toolbar.Controls.AddRange(new Control[] { add, edit });

        Body.Controls.Add(_map);
        Body.Controls.Add(_legend);

        foreach (var c in new Control[] { _tang, _loai, _tt }) ((ComboBox)c).SelectedIndexChanged += (_, _) => Reload();
        _search.TextChanged += (_, _) => Reload();
        find.Click += (_, _) => { using var f = new TraCuuPhongDialog(); f.ShowDialog(this); };
        add.Click += (_, _) => EditPhong(null);
        edit.Click += (_, _) => { if (_selected != null) EditPhong(_selected); else UiKit.Warn("Hãy chọn một phòng trên sơ đồ."); };
        status.Click += (_, _) => { if (_selected != null) ChangeStatusDialog(_selected); else UiKit.Warn("Hãy chọn một phòng trên sơ đồ."); };
        _map.Resize += (_, _) => { foreach (Control c in _map.Controls) if (c is Label) c.Width = _map.ClientSize.Width - 30; };
        Reload();
    }

    public override void Reload()
    {
        // Chú thích màu + số lượng theo trạng thái
        _legend.Controls.Clear();
        foreach (var tt in TrangThaiPhong.TatCa)
        {
            var dot = new Panel { Size = new Size(12, 12), BackColor = Theme.StatusColor(tt), Margin = new Padding(0, 9, 6, 0) };
            var l = UiKit.Lbl($"{tt} ({Phongs.Count(p => p.TrangThai == tt)})", Theme.Base, Theme.Text);
            l.Margin = new Padding(0, 6, 22, 0);
            _legend.Controls.AddRange(new Control[] { dot, l });
        }

        var q = _search.Text.Trim();
        var list = Phongs.Where(p =>
            (_tang.SelectedIndex <= 0 || $"Tầng {p.Tang}" == (string)_tang.SelectedItem!) &&
            (_loai.SelectedIndex <= 0 || HotelService.Loai(p.MaLoai)?.TenLoai == (string)_loai.SelectedItem!) &&
            (_tt.SelectedIndex <= 0 || p.TrangThai == (string)_tt.SelectedItem!) &&
            (q == "" || p.SoPhong.Contains(q))).OrderBy(p => p.SoPhong).ToList();

        _map.SuspendLayout();
        var old = _map.Controls.Cast<Control>().ToList();
        _map.Controls.Clear();
        old.ForEach(c => c.Dispose());
        _tiles.Clear();
        foreach (var g in list.GroupBy(p => p.Tang))
        {
            var header = UiKit.Lbl($"TẦNG {g.Key}", Theme.Bold, Theme.Muted);
            header.AutoSize = false;
            header.Size = new Size(Math.Max(300, _map.ClientSize.Width - 30), 26);
            header.Margin = new Padding(0, 6, 0, 4);
            header.TextAlign = ContentAlignment.BottomLeft;
            _map.Controls.Add(header);
            _map.SetFlowBreak(header, true);
            Control? last = null;
            foreach (var p in g) _map.Controls.Add(last = Tile(p));
            if (last != null) _map.SetFlowBreak(last, true);
        }
        if (list.Count == 0) _map.Controls.Add(UiKit.Lbl("Không có phòng phù hợp điều kiện lọc.", Theme.Base, Theme.Muted));
        _map.ResumeLayout();
    }

    private Control Tile(Phong p)
    {
        var color = Theme.StatusColor(p.TrangThai);
        var tile = new BorderPanel
        {
            Size = new Size(176, 112),
            Margin = new Padding(0, 0, 12, 12),
            BackColor = Theme.Lighten(color, 0.9),
            BorderColor = p == _selected ? Theme.Text : Theme.Lighten(color, 0.55),
            Cursor = Cursors.Hand,
        };
        var band = new Panel { Dock = DockStyle.Top, Height = 5, BackColor = color };
        var so = UiKit.Lbl(p.SoPhong, new Font(Theme.FontName, 16f, FontStyle.Bold), Theme.Text);
        so.Location = new Point(10, 12);
        var loai = HotelService.Loai(p.MaLoai);
        var lo = UiKit.Lbl(loai?.TenLoai ?? "", Theme.Small, Theme.Muted);
        lo.Location = new Point(12, 44);
        var st = UiKit.Lbl(p.TrangThai, Theme.Bold, color);
        st.Location = new Point(10, 64);
        string extra = "";
        if (p.TrangThai == TrangThaiPhong.DangO && HotelService.LuuTruDangO(p.MaPhong) is { } lt)
            extra = $"{HotelService.KhachChinh(lt)?.HoTen}  •  trả {lt.NgayTra:dd/MM}";
        else if (p.TrangThai == TrangThaiPhong.DaDat && DatPhongs.FirstOrDefault(d => d.MaPhong == p.MaPhong && d.TrangThai == TrangThaiDatPhong.ChoNhanPhong) is { } dp)
            extra = $"{HotelService.Khach(dp.MaKH)?.HoTen}  •  đến {dp.NgayNhanDuKien:dd/MM}";
        else if (loai != null)
            extra = $"{HotelService.GiaPhong(loai.MaLoai, DateTime.Today):N0} đ/đêm";
        var ex = UiKit.Lbl(extra, Theme.Small, Theme.Text);
        ex.Location = new Point(12, 88);
        ex.AutoSize = false;
        ex.Size = new Size(160, 18);
        ex.AutoEllipsis = true;
        tile.Controls.AddRange(new Control[] { band, so, lo, st, ex });

        var menu = new ContextMenuStrip();
        var header = new ToolStripMenuItem($"Phòng {p.SoPhong}") { Enabled = false };
        menu.Items.Add(header);
        menu.Items.Add(new ToolStripSeparator());
        foreach (var tt in TrangThaiPhong.CapNhatTay)
        {
            var mi = new ToolStripMenuItem("Chuyển sang: " + tt) { Enabled = p.TrangThai != tt && CanChange(p) };
            mi.Click += (_, _) => SetStatus(p, tt);
            menu.Items.Add(mi);
        }
        void select(object? s, EventArgs e) => SelectRoom(p);
        foreach (Control c in tile.Controls) { c.Click += select; c.ContextMenuStrip = menu; c.Cursor = Cursors.Hand; }
        tile.Click += select;
        tile.DoubleClick += (_, _) => ChangeStatusDialog(p);
        foreach (Control c in tile.Controls) c.DoubleClick += (_, _) => ChangeStatusDialog(p);
        tile.ContextMenuStrip = menu;
        _tiles[p] = tile;
        return tile;
    }

    private void SelectRoom(Phong p)
    {
        if (_selected != null && _tiles.TryGetValue(_selected, out var old))
        {
            old.BorderColor = Theme.Lighten(Theme.StatusColor(_selected.TrangThai), 0.55);
            old.Invalidate();
        }
        _selected = p;
        if (_tiles.TryGetValue(p, out var t))
        {
            t.BorderColor = Theme.Text;
            t.Invalidate();
        }
    }

    private static bool CanChange(Phong p) => p.TrangThai != TrangThaiPhong.DangO && p.TrangThai != TrangThaiPhong.DaDat;

    private void SetStatus(Phong p, string tt)
    {
        if (!CanChange(p))
        {
            UiKit.Warn($"Phòng {p.SoPhong} đang \"{p.TrangThai}\". Trạng thái này do hệ thống cập nhật khi nhận/trả phòng.");
            return;
        }
        p.TrangThai = tt;
        BeginInvoke(new Action(Reload));
    }

    private void ChangeStatusDialog(Phong p)
    {
        if (!CanChange(p)) { SetStatus(p, p.TrangThai); return; }
        using var d = new FormDialog($"Cập nhật trạng thái phòng {p.SoPhong}");
        d.Note($"Trạng thái hiện tại: {p.TrangThai}", Theme.StatusColor(p.TrangThai));
        var tt = d.ComboField("Trạng thái mới", TrangThaiPhong.CapNhatTay, p.TrangThai);
        d.Note("Dọn dẹp xong → \"Trống\" (sẵn sàng đón khách).");
        if (d.ShowDialog(this) == DialogResult.OK) SetStatus(p, (string)tt.SelectedItem!);
    }

    private void EditPhong(Phong? p)
    {
        using var d = new FormDialog(p == null ? "Thêm phòng" : $"Sửa phòng {p.SoPhong}");
        var so = d.TextField("Số phòng", p?.SoPhong ?? "");
        var tang = d.NumberField("Tầng", p?.Tang ?? 1, 1, 50);
        var loai = d.ComboField("Loại phòng", LoaiPhongs.Select(l => l.TenLoai), HotelService.Loai(p?.MaLoai)?.TenLoai);
        d.Validator = () => string.IsNullOrWhiteSpace(so.Text) ? "Số phòng không được để trống."
            : Phongs.Any(x => x != p && x.SoPhong == so.Text.Trim()) ? "Số phòng đã tồn tại." : null;
        if (d.ShowDialog(this) != DialogResult.OK) return;
        if (p == null)
        {
            p = new Phong { MaPhong = "P" + so.Text.Trim() };
            Phongs.Add(p);
        }
        p.SoPhong = so.Text.Trim();
        p.Tang = (int)tang.Value;
        p.MaLoai = LoaiPhongs.First(l => l.TenLoai == (string)loai.SelectedItem!).MaLoai;
        _selected = p;
        Reload();
    }
}

/// <summary>2.4 Tra cứu phòng trống theo khoảng ngày, loại phòng và số khách.</summary>
public class TraCuuPhongDialog : Form
{
    private record Row(string SoPhong, int Tang, string TenLoai, int SucChua, decimal Gia, string TrangThai);

    public TraCuuPhongDialog()
    {
        Text = "Tra cứu phòng trống";
        Font = Theme.Base;
        BackColor = Theme.Background;
        ClientSize = new Size(820, 520);
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        ShowInTaskbar = false;
        Padding = new Padding(16);

        var tu = UiKit.Date(DateTime.Today);
        var den = UiKit.Date(DateTime.Today.AddDays(1));
        var loai = UiKit.Combo(new[] { "Tất cả loại" }.Concat(LoaiPhongs.Select(l => l.TenLoai)), 140);
        var khach = new NumericUpDown { Minimum = 1, Maximum = 10, Value = 2, Width = 60, Margin = new Padding(0, 4, 8, 0) };
        var btn = UiKit.Btn("Tìm");
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 46, WrapContents = false };
        Label L(string s) { var l = UiKit.Lbl(s, Theme.Base, Theme.Muted); l.Margin = new Padding(0, 8, 6, 0); return l; }
        bar.Controls.AddRange(new Control[] { L("Từ"), tu, L("đến"), den, L("Loại"), loai, L("Số khách"), khach, btn });

        var result = UiKit.Lbl("", Theme.Bold, Theme.Success);
        result.Dock = DockStyle.Bottom;
        result.AutoSize = false;
        result.Height = 30;
        result.TextAlign = ContentAlignment.MiddleLeft;

        var grid = UiKit.Grid().Columns(
            new Col("SoPhong", "Phòng", 80), new Col("Tang", "Tầng", 60, Right: true), new Col("TenLoai", "Loại", 120),
            new Col("SucChua", "Sức chứa", 90, Right: true), new Col("Gia", "Giá / đêm", 140, "N0", Right: true),
            new Col("TrangThai", "Hiện trạng", 140, Fill: true, Status: true));
        var card = UiKit.Card(padding: new Padding(1));
        card.Dock = DockStyle.Fill;
        card.Controls.Add(grid);

        Controls.Add(card);
        Controls.Add(result);
        Controls.Add(bar);

        void search()
        {
            if (den.Value.Date <= tu.Value.Date) { UiKit.Warn("Ngày trả phải sau ngày nhận."); return; }
            var maLoai = loai.SelectedIndex > 0 ? LoaiPhongs.First(l => l.TenLoai == (string)loai.SelectedItem!).MaLoai : null;
            var rows = HotelService.PhongTrong(tu.Value, den.Value, maLoai)
                .Select(p => (p, l: HotelService.Loai(p.MaLoai)!))
                .Where(x => x.l.SucChua >= khach.Value)
                .Select(x => new Row(x.p.SoPhong, x.p.Tang, x.l.TenLoai, x.l.SucChua, HotelService.GiaPhong(x.l.MaLoai, tu.Value), x.p.TrangThai))
                .ToList();
            grid.Bind(rows);
            result.Text = $"  Tìm thấy {rows.Count} phòng trống từ {tu.Value:dd/MM/yyyy} đến {den.Value:dd/MM/yyyy} ({HotelService.SoDem(tu.Value, den.Value)} đêm).";
        }
        btn.Click += (_, _) => search();
        Load += (_, _) => search();
    }
}
