using System.Drawing.Printing;
using HotelManagement.Data;
using HotelManagement.Models;
using HotelManagement.UI;
using static HotelManagement.Data.MockDb;

namespace HotelManagement.Views;

/// <summary>5.1 Trả phòng, 5.2 Tính tiền phòng, 5.3 Tính tiền dịch vụ, 5.4 Lập hóa đơn, 5.5 Ghi nhận thanh toán.</summary>
public class TraPhongView : BaseView
{
    public static string? PreselectMaLuuTru { get; set; }

    private record Row(string SoPhong, string KhachChinh, DateTime NgayNhan, DateTime NgayTra, string HoaDon, LuuTru E);
    private record DvRow(string TenDV, int SoLuong, decimal DonGia, decimal ThanhTien);

    private readonly DataGridView _grid = UiKit.Grid();
    private readonly DataGridView _dv = UiKit.Grid();
    private readonly Label _head = UiKit.Lbl("", Theme.Base);
    private readonly Label _tienPhong = UiKit.Lbl("", Theme.Bold);
    private readonly Label _tienPhongCt = UiKit.Lbl("", Theme.Small, Theme.Muted);
    private readonly Label _tienDV = UiKit.Lbl("", Theme.Bold);
    private readonly NumericUpDown _giam = new() { Maximum = 1_000_000_000, Increment = 50_000, ThousandsSeparator = true, Width = 150, Font = Theme.Base, TextAlign = HorizontalAlignment.Right };
    private readonly Label _tong = UiKit.Lbl("", new Font(Theme.FontName, 18f, FontStyle.Bold), Theme.Primary);
    private readonly Label _daTra = UiKit.Lbl("", Theme.Base, Theme.Muted);
    private readonly Button _lapHD = UiKit.Btn("Lập hóa đơn", BtnKind.Secondary, 160);
    private readonly Button _thanhToan = UiKit.Btn("Thanh toán & trả phòng", BtnKind.Success, 200);

    public TraPhongView() : base("Trả phòng & thanh toán", "Tính tiền phòng, tiền dịch vụ, lập hóa đơn và ghi nhận thanh toán khi khách trả phòng.")
    {
        var search = UiKit.Search("Số phòng, tên khách...", 220);
        Toolbar.Controls.Add(search);

        _grid.Columns(
            new Col("SoPhong", "Phòng", 70),
            new Col("KhachChinh", "Khách chính", 160, Fill: true),
            new Col("NgayNhan", "Nhận phòng", 110, "dd/MM HH:mm"),
            new Col("NgayTra", "Trả dự kiến", 100, "dd/MM/yyyy"),
            new Col("HoaDon", "Hóa đơn", 140, Status: true));
        var left = UiKit.Card(padding: new Padding(1, 48, 1, 1));
        left.Dock = DockStyle.Fill;
        var lt = UiKit.Lbl("Phòng đang ở", Theme.H2);
        lt.Location = new Point(16, 14);
        left.Controls.Add(_grid);
        left.Controls.Add(lt);

        // ---- Phiếu tính tiền ----
        var right = UiKit.Card("Chi tiết thanh toán");
        right.Dock = DockStyle.Right;
        right.Width = 500;
        right.AutoScroll = true;
        _head.Location = new Point(18, 46);
        var sec1 = Section("Tiền phòng", 152);
        _tienPhongCt.Location = new Point(18, 180);
        _tienPhong.Location = new Point(18, 180);
        var sec2 = Section("Tiền dịch vụ", 212);
        _dv.Columns(new Col("TenDV", "Dịch vụ", 150, Fill: true), new Col("SoLuong", "SL", 45, Right: true),
            new Col("DonGia", "Đơn giá", 90, "N0", Right: true), new Col("ThanhTien", "Thành tiền", 105, "N0", Right: true));
        _dv.Dock = DockStyle.None;
        _dv.Location = new Point(18, 240);
        _dv.Size = new Size(462, 130);
        _dv.ColumnHeadersHeight = 30;
        _dv.RowTemplate.Height = 26;
        _tienDV.Location = new Point(18, 378);
        var lg = UiKit.Lbl("Giảm giá (đ)", Theme.Base, Theme.Muted);
        lg.Location = new Point(18, 416);
        _giam.Location = new Point(330, 412);
        var line = new Panel { BackColor = Theme.Border, Location = new Point(18, 450), Size = new Size(462, 1) };
        var lt2 = UiKit.Lbl("TỔNG CỘNG", Theme.Bold, Theme.Muted);
        lt2.Location = new Point(18, 470);
        _tong.Location = new Point(18, 460);
        _daTra.Location = new Point(18, 500);
        _lapHD.Location = new Point(18, 536);
        _thanhToan.Location = new Point(186, 536);
        _lapHD.Height = _thanhToan.Height = 42;
        right.Controls.AddRange(new Control[] { _head, sec1, _tienPhongCt, _tienPhong, sec2, _dv, _tienDV, lg, _giam, line, lt2, _tong, _daTra, _lapHD, _thanhToan });

        Body.Controls.Add(left);
        Body.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 14 });
        Body.Controls.Add(right);

        search.TextChanged += (_, _) => Reload(search.Text);
        _grid.SelectionChanged += (_, _) => { _giam.Value = Selected is { } s && HotelService.HoaDonCua(s) is { } h ? h.GiamGia : 0; Calc(); };
        _giam.ValueChanged += (_, _) => Calc();
        _lapHD.Click += (_, _) => LapHoaDon(show: true);
        _thanhToan.Click += (_, _) => ThanhToan();
        Reload();

        if (PreselectMaLuuTru != null)
        {
            foreach (DataGridViewRow r in _grid.Rows)
                if (r.DataBoundItem is Row row && row.E.MaLuuTru == PreselectMaLuuTru) _grid.CurrentCell = r.Cells[0];
            PreselectMaLuuTru = null;
        }
    }

    private Label Section(string text, int y)
    {
        var l = UiKit.Lbl(text.ToUpper(), Theme.Bold, Theme.Muted);
        l.Location = new Point(18, y);
        return l;
    }

    private LuuTru? Selected => _grid.Selected<Row>()?.E;

    public override void Reload() => Reload("");

    private void Reload(string q)
    {
        q = q.Trim().ToLower();
        _grid.Bind(LuuTrus.Where(l => l.TrangThai == TrangThaiLuuTru.DangO)
            .Select(l => new Row(HotelService.PhongTheoMa(l.MaPhong)?.SoPhong ?? "", HotelService.KhachChinh(l)?.HoTen ?? "", l.NgayNhan, l.NgayTra,
                HotelService.HoaDonCua(l)?.TrangThai ?? "Chưa lập", l))
            .Where(r => q == "" || r.SoPhong.Contains(q) || r.KhachChinh.ToLower().Contains(q))
            .OrderBy(r => r.NgayTra).ThenBy(r => r.SoPhong));
        Calc();
    }

    private void Calc()
    {
        var lt = Selected;
        foreach (var c in new Control[] { _lapHD, _thanhToan, _giam }) c.Enabled = lt != null;
        if (lt == null)
        {
            _head.Text = "Chọn phòng cần trả ở danh sách bên trái.";
            _tienPhong.Text = _tienPhongCt.Text = _tienDV.Text = _daTra.Text = "";
            _tong.Text = "0 đ";
            _dv.Bind(Array.Empty<DvRow>());
            return;
        }
        var now = DateTime.Now;
        var p = HotelService.PhongTheoMa(lt.MaPhong)!;
        var k = HotelService.KhachChinh(lt);
        var dem = HotelService.SoDem(lt.NgayNhan, now);
        var tienPhong = HotelService.TinhTienPhong(lt, now);
        var tienDV = HotelService.TinhTienDichVu(lt);
        _head.Text = $"Phòng {p.SoPhong} ({HotelService.Loai(p.MaLoai)?.TenLoai})  •  {k?.HoTen}\n" +
                     $"Nhận: {lt.NgayNhan:dd/MM/yyyy HH:mm}    Trả: {now:dd/MM/yyyy HH:mm}\n" +
                     (now.Date < lt.NgayTra.Date ? $"⚠ Trả sớm so với dự kiến ({lt.NgayTra:dd/MM/yyyy})" : now.Date > lt.NgayTra.Date ? $"⚠ Trả trễ so với dự kiến ({lt.NgayTra:dd/MM/yyyy})" : "");
        _tienPhongCt.Text = $"{dem} đêm × {lt.GiaPhongApDung:N0} đ";
        _tienPhong.Text = $"{tienPhong:N0} đ";
        _tienPhong.Location = new Point(480 - _tienPhong.Width, 180);
        var dvs = SuDungDichVus.Where(s => s.MaLuuTru == lt.MaLuuTru)
            .GroupBy(s => (s.MaDV, s.DonGiaApDung))
            .Select(g => new DvRow(HotelService.DV(g.Key.MaDV)?.TenDV ?? "", g.Sum(s => s.SoLuong), g.Key.DonGiaApDung, g.Sum(s => s.ThanhTien))).ToList();
        _dv.Bind(dvs);
        _tienDV.Text = $"Cộng dịch vụ: {tienDV:N0} đ";
        _tienDV.Location = new Point(480 - _tienDV.Width, 378);
        var tong = Math.Max(0, tienPhong + tienDV - _giam.Value);
        _tong.Text = $"{tong:N0} đ";
        _tong.Location = new Point(480 - _tong.Width, 460);
        var hd = HotelService.HoaDonCua(lt);
        var daTra = hd == null ? 0 : HotelService.DaThanhToan(hd);
        _daTra.Text = hd == null ? "Chưa lập hóa đơn" : $"Hóa đơn {hd.MaHD} • Đã thanh toán {daTra:N0} đ • Còn lại {Math.Max(0, tong - daTra):N0} đ";
    }

    private HoaDon? LapHoaDon(bool show)
    {
        if (Selected is not { } lt) return null;
        var hd = HotelService.LapHoaDon(lt, _giam.Value, DateTime.Now);
        if (show)
        {
            using var f = new InvoiceForm(hd);
            f.ShowDialog(this);
        }
        Reload();
        return hd;
    }

    private void ThanhToan()
    {
        if (Selected is not { } lt) return;
        var hd = LapHoaDon(show: false)!;
        var conLai = hd.TongTien - HotelService.DaThanhToan(hd);
        using var d = new FormDialog($"Thanh toán hóa đơn {hd.MaHD}");
        d.Note($"Tổng hóa đơn: {hd.TongTien:N0} đ  •  Còn phải thu: {conLai:N0} đ", Theme.Primary);
        var pt = d.ComboField("Phương thức", PhuongThucThanhToan.TatCa);
        var tien = d.NumberField("Số tiền khách trả", conLai, 0, 10_000_000_000, 10_000);
        var thoi = d.Note("");
        void upd() => thoi.Text = tien.Value > conLai ? $"Tiền thối lại: {tien.Value - conLai:N0} đ" : tien.Value < conLai ? $"Còn thiếu: {conLai - tien.Value:N0} đ (thanh toán một phần)" : "Thanh toán đủ.";
        tien.ValueChanged += (_, _) => upd();
        upd();
        d.Validator = () => tien.Value <= 0 ? "Số tiền phải lớn hơn 0." : null;
        if (d.ShowDialog(this) != DialogResult.OK) return;

        HotelService.GhiNhanThanhToan(hd, (string)pt.SelectedItem!, Math.Min(tien.Value, conLai));
        if (hd.TrangThai == TrangThaiHoaDon.DaThanhToan)
        {
            UiKit.Info($"Thanh toán thành công. Khách đã trả phòng {HotelService.PhongTheoMa(lt.MaPhong)?.SoPhong}.\nPhòng được chuyển sang trạng thái \"Đang dọn\".");
            using var f = new InvoiceForm(hd);
            f.ShowDialog(this);
        }
        else UiKit.Info($"Đã ghi nhận thanh toán một phần. Hóa đơn {hd.MaHD} còn nợ {hd.TongTien - HotelService.DaThanhToan(hd):N0} đ.");
        Reload();
    }
}

/// <summary>5.6 Tra cứu hóa đơn.</summary>
public class HoaDonView : BaseView
{
    private record Row(string MaHD, DateTime NgayLap, string SoPhong, string KhachHang, decimal TienPhong, decimal TienDV, decimal GiamGia, decimal TongTien, decimal DaTra, string TrangThai, HoaDon E);

    private readonly DataGridView _grid = UiKit.Grid();
    private readonly DataGridView _tt = UiKit.Grid();
    private readonly TextBox _search = UiKit.Search("Mã HĐ, tên khách, số phòng...", 220);
    private readonly DateTimePicker _from = UiKit.Date(DateTime.Today.AddDays(-30));
    private readonly DateTimePicker _to = UiKit.Date(DateTime.Today);
    private readonly ComboBox _status = UiKit.Combo(new[] { "Tất cả trạng thái" }.Concat(TrangThaiHoaDon.TatCa), 160);
    private readonly Label _sum = UiKit.Lbl("", Theme.Bold, Theme.Primary);

    public HoaDonView() : base("Tra cứu hóa đơn", "Tìm hóa đơn theo mã, khách hàng, phòng, khoảng ngày lập và trạng thái thanh toán.")
    {
        var view = UiKit.Btn("Xem / In hóa đơn");
        Label L(string s) { var l = UiKit.Lbl(s, Theme.Base, Theme.Muted); l.Margin = new Padding(0, 8, 6, 0); return l; }
        Toolbar.Controls.AddRange(new Control[] { _search, L("Từ"), _from, L("đến"), _to, _status });
        Spacer(8);
        Toolbar.Controls.Add(view);

        _grid.Columns(
            new Col("MaHD", "Mã HĐ", 85),
            new Col("NgayLap", "Ngày lập", 125, "dd/MM/yyyy HH:mm"),
            new Col("SoPhong", "Phòng", 65),
            new Col("KhachHang", "Khách hàng", 160, Fill: true),
            new Col("TienPhong", "Tiền phòng", 110, "N0", Right: true),
            new Col("TienDV", "Tiền DV", 100, "N0", Right: true),
            new Col("GiamGia", "Giảm giá", 90, "N0", Right: true),
            new Col("TongTien", "Tổng tiền", 115, "N0", Right: true),
            new Col("DaTra", "Đã trả", 110, "N0", Right: true),
            new Col("TrangThai", "Trạng thái", 150, Status: true));
        _tt.Columns(new Col("MaTT", "Mã TT", 80), new Col("ThoiGian", "Thời gian", 140, "dd/MM/yyyy HH:mm"), new Col("PhuongThuc", "Phương thức", 140),
            new Col("SoTien", "Số tiền", 130, "N0", Right: true), new Col("TrangThai", "Trạng thái", 120, Fill: true));

        var sumBar = new Panel { Dock = DockStyle.Top, Height = 30 };
        _sum.Location = new Point(2, 4);
        sumBar.Controls.Add(_sum);
        var top = UiKit.Card(padding: new Padding(1));
        top.Dock = DockStyle.Fill;
        top.Controls.Add(_grid);
        var bottom = UiKit.Card(padding: new Padding(1, 46, 1, 1));
        bottom.Dock = DockStyle.Bottom;
        bottom.Height = 190;
        var bt = UiKit.Lbl("Lịch sử thanh toán của hóa đơn", Theme.H2);
        bt.Location = new Point(16, 12);
        bottom.Controls.Add(_tt);
        bottom.Controls.Add(bt);
        Body.Controls.Add(top);
        Body.Controls.Add(sumBar);
        Body.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 14 });
        Body.Controls.Add(bottom);

        _search.TextChanged += (_, _) => Reload();
        _from.ValueChanged += (_, _) => Reload();
        _to.ValueChanged += (_, _) => Reload();
        _status.SelectedIndexChanged += (_, _) => Reload();
        _grid.SelectionChanged += (_, _) => _tt.Bind(ThanhToans.Where(t => t.MaHD == _grid.Selected<Row>()?.MaHD));
        view.Click += (_, _) => { if (_grid.Selected<Row>() is { } r) { using var f = new InvoiceForm(r.E); f.ShowDialog(this); } };
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0 && _grid.Selected<Row>() is { } r) { using var f = new InvoiceForm(r.E); f.ShowDialog(this); } };
        Reload();
    }

    public override void Reload()
    {
        var q = _search.Text.Trim().ToLower();
        var st = _status.SelectedIndex > 0 ? (string)_status.SelectedItem! : null;
        var rows = HoaDons.Where(h => h.NgayLap.Date >= _from.Value.Date && h.NgayLap.Date <= _to.Value.Date && (st == null || h.TrangThai == st))
            .Select(h =>
            {
                var lt = HotelService.LT(h.MaLuuTru)!;
                return new Row(h.MaHD, h.NgayLap, HotelService.PhongTheoMa(lt.MaPhong)?.SoPhong ?? "", HotelService.KhachChinh(lt)?.HoTen ?? "",
                    h.TienPhong, h.TienDV, h.GiamGia, h.TongTien, HotelService.DaThanhToan(h), h.TrangThai, h);
            })
            .Where(r => q == "" || r.MaHD.ToLower().Contains(q) || r.KhachHang.ToLower().Contains(q) || r.SoPhong.Contains(q))
            .OrderByDescending(r => r.NgayLap).ToList();
        _grid.Bind(rows);
        _sum.Text = $"{rows.Count} hóa đơn  •  Tổng giá trị {rows.Sum(r => r.TongTien):N0} đ  •  Đã thu {rows.Sum(r => r.DaTra):N0} đ";
    }
}

/// <summary>Xem trước và in hóa đơn.</summary>
public class InvoiceForm : Form
{
    private readonly HoaDon _hd;

    public InvoiceForm(HoaDon hd)
    {
        _hd = hd;
        Text = $"Hóa đơn {hd.MaHD}";
        Font = Theme.Base;
        ClientSize = new Size(560, 720);
        StartPosition = FormStartPosition.CenterParent;
        BackColor = Theme.Background;
        MinimizeBox = false;
        ShowInTaskbar = false;

        var paper = new DoubleBufferedPanel { BackColor = Color.White, Location = new Point(20, 20), Size = new Size(520, 620) };
        paper.Paint += (_, e) => Draw(e.Graphics, new Rectangle(0, 0, paper.Width, paper.Height));
        var print = UiKit.Btn("🖨  In hóa đơn", BtnKind.Primary, 150);
        print.Location = new Point(240, 660);
        var close = UiKit.Btn("Đóng", BtnKind.Secondary, 140);
        close.Location = new Point(400, 660);
        close.Click += (_, _) => Close();
        print.Click += (_, _) =>
        {
            var doc = new PrintDocument { DocumentName = $"HoaDon_{hd.MaHD}" };
            doc.PrintPage += (_, e) => { Draw(e.Graphics!, new Rectangle(e.MarginBounds.Left, e.MarginBounds.Top, 520, 620)); e.HasMorePages = false; };
            using var pv = new PrintPreviewDialog { Document = doc, Width = 900, Height = 900 };
            pv.ShowDialog(this);
        };
        Controls.AddRange(new Control[] { paper, print, close });
        CancelButton = close;
    }

    private void Draw(Graphics g, Rectangle r)
    {
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        var lt = HotelService.LT(_hd.MaLuuTru)!;
        var p = HotelService.PhongTheoMa(lt.MaPhong);
        var k = HotelService.KhachChinh(lt);
        using var title = new Font(Theme.FontName, 16f, FontStyle.Bold);
        using var bold = new Font(Theme.FontName, 9.5f, FontStyle.Bold);
        using var normal = new Font(Theme.FontName, 9.5f);
        using var small = new Font(Theme.FontName, 8.5f);
        using var dark = new SolidBrush(Theme.Text);
        using var muted = new SolidBrush(Theme.Muted);
        using var pen = new Pen(Theme.Border);
        int x = r.Left + 28, w = r.Width - 56, y = r.Top + 24;
        var right = new StringFormat { Alignment = StringAlignment.Far };
        var center = new StringFormat { Alignment = StringAlignment.Center };

        g.DrawString(Theme.HotelName, bold, dark, new RectangleF(x, y, w, 20), center); y += 18;
        g.DrawString("TP. Hồ Chí Minh • Hotline 1900 1234", small, muted, new RectangleF(x, y, w, 18), center); y += 30;
        g.DrawString("HÓA ĐƠN THANH TOÁN", title, dark, new RectangleF(x, y, w, 30), center); y += 34;
        g.DrawString($"Số: {_hd.MaHD}   •   Ngày lập: {_hd.NgayLap:dd/MM/yyyy HH:mm}", normal, muted, new RectangleF(x, y, w, 20), center); y += 34;

        void kv(string key, string val) { g.DrawString(key, normal, muted, x, y); g.DrawString(val, normal, dark, x + 130, y); y += 22; }
        kv("Khách hàng:", k?.HoTen ?? "");
        kv("CCCD/Hộ chiếu:", k?.CCCD_HoChieu ?? "");
        kv("Phòng:", $"{p?.SoPhong} - {HotelService.Loai(p?.MaLoai)?.TenLoai}");
        kv("Thời gian ở:", $"{lt.NgayNhan:dd/MM/yyyy HH:mm}  →  {_hd.NgayLap:dd/MM/yyyy HH:mm}");
        y += 8;

        g.DrawLine(pen, x, y, x + w, y); y += 6;
        g.DrawString("Nội dung", bold, dark, x, y);
        g.DrawString("SL", bold, dark, new RectangleF(x + 240, y, 40, 20), right);
        g.DrawString("Đơn giá", bold, dark, new RectangleF(x + 280, y, 90, 20), right);
        g.DrawString("Thành tiền", bold, dark, new RectangleF(x + 370, y, w - 370, 20), right);
        y += 22;
        g.DrawLine(pen, x, y, x + w, y); y += 6;
        void line(string name, string sl, decimal dg, decimal tt)
        {
            g.DrawString(name, normal, dark, x, y);
            g.DrawString(sl, normal, dark, new RectangleF(x + 240, y, 40, 20), right);
            g.DrawString(dg.ToString("N0"), normal, dark, new RectangleF(x + 280, y, 90, 20), right);
            g.DrawString(tt.ToString("N0"), normal, dark, new RectangleF(x + 370, y, w - 370, 20), right);
            y += 22;
        }
        var dem = HotelService.SoDem(lt.NgayNhan, _hd.NgayLap);
        line($"Tiền phòng ({dem} đêm)", dem.ToString(), lt.GiaPhongApDung, _hd.TienPhong);
        foreach (var s in SuDungDichVus.Where(s => s.MaLuuTru == lt.MaLuuTru).GroupBy(s => (s.MaDV, s.DonGiaApDung)))
            line(HotelService.DV(s.Key.MaDV)?.TenDV ?? "", s.Sum(z => z.SoLuong).ToString(), s.Key.DonGiaApDung, s.Sum(z => z.ThanhTien));
        y += 4;
        g.DrawLine(pen, x, y, x + w, y); y += 10;

        void total(string key, decimal v, Font f) { g.DrawString(key, f, dark, x + 200, y); g.DrawString(v.ToString("N0") + " đ", f, dark, new RectangleF(x + 300, y, w - 300, 22), right); y += 24; }
        total("Tiền phòng", _hd.TienPhong, normal);
        total("Tiền dịch vụ", _hd.TienDV, normal);
        total("Giảm giá", -_hd.GiamGia, normal);
        total("TỔNG CỘNG", _hd.TongTien, bold);
        var daTra = HotelService.DaThanhToan(_hd);
        total("Đã thanh toán", daTra, normal);
        y += 6;
        using var stBrush = new SolidBrush(Theme.StatusColor(_hd.TrangThai));
        g.DrawString(_hd.TrangThai.ToUpper(), bold, stBrush, new RectangleF(x, y, w, 20), right);
        var pts = ThanhToans.Where(t => t.MaHD == _hd.MaHD).Select(t => t.PhuongThuc).Distinct();
        g.DrawString("Phương thức: " + string.Join(", ", pts), small, muted, x, y + 2);
        y += 40;
        g.DrawString("Cảm ơn quý khách và hẹn gặp lại!", normal, muted, new RectangleF(x, y, w, 20), center);
    }

    private class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel() => DoubleBuffered = true;
    }
}
