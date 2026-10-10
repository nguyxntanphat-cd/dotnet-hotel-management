using HotelManagement.Data;
using HotelManagement.Forms;
using HotelManagement.Models;
using HotelManagement.UI;
using static HotelManagement.Data.MockDb;

namespace HotelManagement.Views;

/// <summary>Bảng điều khiển: số liệu nhanh trong ngày cho lễ tân và quản lý.</summary>
public class DashboardView : BaseView
{
    private record KhachRow(string SoPhong, string KhachHang, string SDT, string GhiChu);

    public DashboardView() : base($"Xin chào, {Session.NhanVien?.HoTen ?? Session.TaiKhoan?.TenDangNhap}!", $"Tình hình khách sạn hôm nay, {DateTime.Today:dddd, dd/MM/yyyy}.")
    {
        var today = DateTime.Today;
        var cards = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 120, WrapContents = false, BackColor = Theme.Background };
        int tong = Phongs.Count;
        int trong = Phongs.Count(p => p.TrangThai == TrangThaiPhong.Trong);
        int dangO = Phongs.Count(p => p.TrangThai == TrangThaiPhong.DangO);
        var den = DatPhongs.Where(d => d.TrangThai == TrangThaiDatPhong.ChoNhanPhong && d.NgayNhanDuKien.Date == today).ToList();
        var di = LuuTrus.Where(l => l.TrangThai == TrangThaiLuuTru.DangO && l.NgayTra.Date <= today).ToList();
        var thang = HotelService.HoaDonDaThu(new DateTime(today.Year, today.Month, 1), today).Sum(h => h.TongTien);
        cards.Controls.AddRange(new Control[]
        {
            new StatCard("Phòng trống", $"{trong}/{tong}", Theme.Success, "sẵn sàng đón khách"),
            new StatCard("Đang có khách", dangO.ToString(), Theme.Primary, $"công suất {(double)dangO / tong:P0}"),
            new StatCard("Khách đến hôm nay", den.Count.ToString(), Theme.Warning, "đặt phòng chờ nhận"),
            new StatCard("Trả phòng hôm nay", di.Count.ToString(), Theme.Danger, "gồm cả trả trễ"),
            new StatCard("Doanh thu tháng này", Short(thang), Theme.Violet, $"{thang:N0} đ"),
        });

        // Biểu đồ doanh thu 14 ngày
        var chartCard = UiKit.Card("Doanh thu 14 ngày gần nhất", new Padding(12, 46, 12, 8));
        chartCard.Dock = DockStyle.Fill;
        var chart = new BarChart
        {
            Dock = DockStyle.Fill,
            SeriesNames = new[] { "Tiền phòng", "Tiền dịch vụ" },
            Data = Enumerable.Range(0, 14).Select(i => today.AddDays(i - 13)).Select(d =>
            {
                var hds = HotelService.HoaDonDaThu(d, d).ToList();
                return (d.ToString("dd/MM"), new[] { (double)hds.Sum(h => h.TienPhong - h.GiamGia), (double)hds.Sum(h => h.TienDV) });
            }).ToList(),
        };
        chartCard.Controls.Add(chart);

        // Tình trạng phòng
        var statusCard = UiKit.Card("Tình trạng phòng");
        statusCard.Dock = DockStyle.Right;
        statusCard.Width = 340;
        int y = 52;
        foreach (var tt in TrangThaiPhong.TatCa)
        {
            int n = Phongs.Count(p => p.TrangThai == tt);
            var l = UiKit.Lbl(tt, Theme.Base);
            l.Location = new Point(18, y);
            var v = UiKit.Lbl(n.ToString(), Theme.Bold);
            v.Location = new Point(300, y);
            var bg = new Panel { BackColor = Theme.Background, Location = new Point(18, y + 24), Size = new Size(300, 8) };
            var fg = new Panel { BackColor = Theme.StatusColor(tt), Location = new Point(0, 0), Size = new Size(300 * n / Math.Max(1, tong), 8) };
            bg.Controls.Add(fg);
            statusCard.Controls.AddRange(new Control[] { l, v, bg });
            y += 46;
        }
        var go = UiKit.Btn("Mở sơ đồ phòng →", BtnKind.Secondary, 300);
        go.Location = new Point(18, y + 4);
        go.Click += (_, _) => MainForm.Current?.Navigate("phong");
        statusCard.Controls.Add(go);

        var middle = new Panel { Dock = DockStyle.Fill };
        middle.Controls.Add(chartCard);
        middle.Controls.Add(new Panel { Dock = DockStyle.Right, Width = 14 });
        middle.Controls.Add(statusCard);

        // Danh sách khách đến / đi
        var lists = new TableLayoutPanel { Dock = DockStyle.Bottom, Height = 230, ColumnCount = 3 };
        lists.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        lists.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 14));
        lists.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        Panel ListCard(string title, IEnumerable<KhachRow> rows)
        {
            var c = UiKit.Card(padding: new Padding(1, 46, 1, 1));
            c.Dock = DockStyle.Fill;
            c.Margin = new Padding(0);
            var t = UiKit.Lbl(title, Theme.H2);
            t.Location = new Point(14, 12);
            var g = UiKit.Grid().Columns(new Col("SoPhong", "Phòng", 70), new Col("KhachHang", "Khách hàng", 170, Fill: true), new Col("SDT", "SĐT", 110), new Col("GhiChu", "Ghi chú", 150));
            g.Bind(rows);
            c.Controls.Add(g);
            c.Controls.Add(t);
            return c;
        }
        lists.Controls.Add(ListCard($"Khách đến hôm nay ({den.Count})", den.Select(d =>
        {
            var k = HotelService.Khach(d.MaKH);
            return new KhachRow(HotelService.PhongTheoMa(d.MaPhong)?.SoPhong ?? "", k?.HoTen ?? "", k?.SDT ?? "", d.GhiChu);
        })), 0, 0);
        lists.Controls.Add(ListCard($"Khách trả phòng hôm nay ({di.Count})", di.Select(l =>
        {
            var k = HotelService.KhachChinh(l);
            return new KhachRow(HotelService.PhongTheoMa(l.MaPhong)?.SoPhong ?? "", k?.HoTen ?? "", k?.SDT ?? "", l.NgayTra.Date < today ? $"Trễ {(today - l.NgayTra.Date).Days} ngày" : "");
        })), 2, 0);

        Body.Controls.Add(middle);
        Body.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 14 });
        Body.Controls.Add(lists);
        Body.Controls.Add(cards);
    }

    internal static string Short(decimal v) => v >= 1_000_000_000 ? $"{v / 1_000_000_000:0.##} tỷ" : v >= 1_000_000 ? $"{v / 1_000_000:0.#} tr" : v.ToString("N0");
}

/// <summary>6.1 Thống kê tình trạng phòng, 6.2 Thống kê khách hàng, 6.3 Thống kê doanh thu, 6.4 Báo cáo doanh thu, 6.5 Xuất báo cáo.</summary>
public class ThongKeView : BaseView
{
    private record LoaiRow(string TenLoai, int Tong, int Trong, int DaDat, int DangO, int DangDon, int BaoTri, int DemDaBan, string CongSuat);
    private record KhachRow(int Hang, string MaKH, string HoTen, string SDT, int SoLuot, int SoDem, decimal ChiTieu);
    private record DoanhThuRow(string KyBaoCao, int SoHoaDon, decimal TienPhong, decimal TienDV, decimal GiamGia, decimal DoanhThu);
    private record LoaiDoanhThuRow(string TenLoai, int SoLuot, int SoDem, decimal DoanhThu, string TyTrong);
    private record DichVuDoanhThuRow(string TenDV, string DonViTinh, int SoLuong, decimal DoanhThu, string TyTrong);

    private readonly DateTimePicker _from = UiKit.Date(DateTime.Today.AddDays(-29));
    private readonly DateTimePicker _to = UiKit.Date(DateTime.Today);
    private readonly ComboBox _preset = UiKit.Combo(new[] { "30 ngày qua", "7 ngày qua", "Tháng này", "Tháng trước", "6 tháng qua", "Tùy chọn" }, 130);
    private readonly TabControl _tabs = new() { Dock = DockStyle.Fill, Font = Theme.Base, Padding = new Point(16, 6) };

    private readonly DataGridView _gLoai = UiKit.Grid(), _gKhach = UiKit.Grid(), _gDoanhThu = UiKit.Grid(), _gBcLoai = UiKit.Grid(), _gBcDv = UiKit.Grid();
    private readonly FlowLayoutPanel _cPhong = Cards(), _cKhach = Cards(), _cDoanhThu = Cards();
    private readonly BarChart _chart = new() { Dock = DockStyle.Fill, SeriesNames = new[] { "Tiền phòng", "Tiền dịch vụ" } };
    private readonly BarChart _chartPhong = new() { Dock = DockStyle.Fill, ShowTotals = true, ColorPerBar = true };
    private bool _loading;

    public ThongKeView() : base("Thống kê & báo cáo", "Số liệu tổng hợp theo khoảng thời gian: tình trạng phòng, khách hàng và doanh thu.")
    {
        var view = UiKit.Btn("Xem thống kê");
        var export = UiKit.Btn("⭳  Xuất báo cáo (Excel/CSV)", BtnKind.Success);
        Label L(string s) { var l = UiKit.Lbl(s, Theme.Base, Theme.Muted); l.Margin = new Padding(0, 8, 6, 0); return l; }
        Toolbar.Controls.AddRange(new Control[] { L("Kỳ"), _preset, L("Từ"), _from, L("đến"), _to, view });
        Spacer();
        Toolbar.Controls.Add(export);

        // 6.1
        _gLoai.Columns(new Col("TenLoai", "Loại phòng", 130, Fill: true), new Col("Tong", "Tổng", 70, Right: true), new Col("Trong", "Trống", 70, Right: true),
            new Col("DaDat", "Đã đặt", 70, Right: true), new Col("DangO", "Đang ở", 70, Right: true), new Col("DangDon", "Đang dọn", 80, Right: true),
            new Col("BaoTri", "Bảo trì", 70, Right: true), new Col("DemDaBan", "Đêm phòng đã bán (kỳ)", 170, Right: true), new Col("CongSuat", "Công suất (kỳ)", 120, Right: true));
        _tabs.TabPages.Add(Page("6.1  Tình trạng phòng", _cPhong, Split(GridCard("Theo loại phòng", _gLoai), ChartCard("Số phòng theo trạng thái hiện tại", _chartPhong))));
        // 6.2
        _gKhach.Columns(new Col("Hang", "#", 45, Right: true), new Col("MaKH", "Mã KH", 80), new Col("HoTen", "Khách hàng", 180, Fill: true), new Col("SDT", "SĐT", 120),
            new Col("SoLuot", "Số lượt ở", 90, Right: true), new Col("SoDem", "Số đêm", 80, Right: true), new Col("ChiTieu", "Tổng chi tiêu", 140, "N0", Right: true));
        _tabs.TabPages.Add(Page("6.2  Khách hàng", _cKhach, GridCard("Khách hàng theo chi tiêu trong kỳ", _gKhach)));
        // 6.3
        _gDoanhThu.Columns(new Col("KyBaoCao", "Kỳ", 110), new Col("SoHoaDon", "Số HĐ", 80, Right: true), new Col("TienPhong", "Tiền phòng", 140, "N0", Right: true),
            new Col("TienDV", "Tiền dịch vụ", 130, "N0", Right: true), new Col("GiamGia", "Giảm giá", 110, "N0", Right: true), new Col("DoanhThu", "Doanh thu", 150, "N0", Right: true, Fill: true));
        _tabs.TabPages.Add(Page("6.3  Doanh thu", _cDoanhThu, Split(ChartCard("Biểu đồ doanh thu", _chart), GridCard("Chi tiết theo kỳ", _gDoanhThu), 0.58)));
        // 6.4
        _gBcLoai.Columns(new Col("TenLoai", "Loại phòng", 140, Fill: true), new Col("SoLuot", "Lượt ở", 80, Right: true), new Col("SoDem", "Đêm", 70, Right: true),
            new Col("DoanhThu", "Doanh thu phòng", 150, "N0", Right: true), new Col("TyTrong", "Tỷ trọng", 90, Right: true));
        _gBcDv.Columns(new Col("TenDV", "Dịch vụ", 160, Fill: true), new Col("DonViTinh", "ĐVT", 70), new Col("SoLuong", "Số lượng", 85, Right: true),
            new Col("DoanhThu", "Doanh thu", 130, "N0", Right: true), new Col("TyTrong", "Tỷ trọng", 90, Right: true));
        var bc = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Padding = new Padding(0, 14, 0, 0) };
        bc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        bc.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 14));
        bc.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        bc.Controls.Add(GridCard("Doanh thu theo loại phòng", _gBcLoai), 0, 0);
        bc.Controls.Add(GridCard("Doanh thu theo dịch vụ", _gBcDv), 2, 0);
        _tabs.TabPages.Add(Page("6.4  Báo cáo doanh thu", null, bc));

        Body.Controls.Add(_tabs);

        _preset.SelectedIndexChanged += (_, _) => ApplyPreset();
        _from.ValueChanged += (_, _) => { if (!_loading) _preset.SelectedItem = "Tùy chọn"; };
        _to.ValueChanged += (_, _) => { if (!_loading) _preset.SelectedItem = "Tùy chọn"; };
        view.Click += (_, _) => Reload();
        export.Click += (_, _) => Export();
        Reload();
    }

    private static FlowLayoutPanel Cards() => new() { Dock = DockStyle.Top, Height = 118, WrapContents = false, BackColor = Theme.Background, Padding = new Padding(0, 12, 0, 0) };

    private static TabPage Page(string title, Control? top, Control body)
    {
        var p = new TabPage(title) { BackColor = Theme.Background, Padding = new Padding(0) };
        p.Controls.Add(body);
        if (top != null) p.Controls.Add(top);
        return p;
    }

    private static Panel GridCard(string title, DataGridView g)
    {
        var c = UiKit.Card(padding: new Padding(1, 46, 1, 1));
        c.Dock = DockStyle.Fill;
        c.Margin = new Padding(0);
        var t = UiKit.Lbl(title, Theme.H2);
        t.Location = new Point(14, 12);
        c.Controls.Add(g);
        c.Controls.Add(t);
        return c;
    }

    private static Panel ChartCard(string title, BarChart chart)
    {
        var c = UiKit.Card(title, new Padding(12, 46, 12, 8));
        c.Dock = DockStyle.Fill;
        c.Margin = new Padding(0);
        c.Controls.Add(chart);
        return c;
    }

    private static Control Split(Control a, Control b, double ratio = 0.5)
    {
        var t = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1, Padding = new Padding(0, 2, 0, 0) };
        t.RowStyles.Add(new RowStyle(SizeType.Percent, (float)(ratio * 100)));
        t.RowStyles.Add(new RowStyle(SizeType.Absolute, 14));
        t.RowStyles.Add(new RowStyle(SizeType.Percent, (float)((1 - ratio) * 100)));
        t.Controls.Add(a, 0, 0);
        t.Controls.Add(b, 0, 2);
        return t;
    }

    private void ApplyPreset()
    {
        var today = DateTime.Today;
        var first = new DateTime(today.Year, today.Month, 1);
        (DateTime, DateTime)? r = (string)_preset.SelectedItem! switch
        {
            "7 ngày qua" => (today.AddDays(-6), today),
            "30 ngày qua" => (today.AddDays(-29), today),
            "Tháng này" => (first, today),
            "Tháng trước" => (first.AddMonths(-1), first.AddDays(-1)),
            "6 tháng qua" => (first.AddMonths(-5), today),
            _ => null,
        };
        if (r == null) return;
        _loading = true;
        (_from.Value, _to.Value) = r.Value;
        _loading = false;
        Reload();
    }

    public override void Reload()
    {
        var tu = _from.Value.Date;
        var den = _to.Value.Date;
        if (den < tu) { UiKit.Warn("Ngày kết thúc phải sau ngày bắt đầu."); return; }
        int soNgay = (den - tu).Days + 1;
        var hds = HotelService.HoaDonDaThu(tu, den).ToList();
        var lts = hds.Select(h => HotelService.LT(h.MaLuuTru)!).ToList();

        // ---- 6.1 Tình trạng phòng ----
        int demTrongKy(LuuTru l) => Math.Max(0, (Min(l.NgayTra.Date, den.AddDays(1)) - Max(l.NgayNhan.Date, tu)).Days);
        var luuTruTrongKy = LuuTrus.Where(l => l.NgayNhan.Date <= den && l.NgayTra.Date >= tu).ToList();
        var loaiRows = LoaiPhongs.Select(lp =>
        {
            var ps = Phongs.Where(p => p.MaLoai == lp.MaLoai).ToList();
            int c(string tt) => ps.Count(p => p.TrangThai == tt);
            var dem = luuTruTrongKy.Where(l => ps.Any(p => p.MaPhong == l.MaPhong)).Sum(demTrongKy);
            return new LoaiRow(lp.TenLoai, ps.Count, c(TrangThaiPhong.Trong), c(TrangThaiPhong.DaDat), c(TrangThaiPhong.DangO), c(TrangThaiPhong.DangDon), c(TrangThaiPhong.BaoTri),
                dem, ps.Count == 0 ? "-" : $"{Math.Min(1, (double)dem / (ps.Count * soNgay)):P1}");
        }).ToList();
        _gLoai.Bind(loaiRows);
        int tongDem = loaiRows.Sum(r => r.DemDaBan);
        SetCards(_cPhong,
            new StatCard("Tổng số phòng", Phongs.Count.ToString(), Theme.Gray, $"{Phongs.Select(p => p.Tang).Distinct().Count()} tầng"),
            new StatCard("Đang có khách", Phongs.Count(p => p.TrangThai == TrangThaiPhong.DangO).ToString(), Theme.Primary, "hiện tại"),
            new StatCard("Phòng trống", Phongs.Count(p => p.TrangThai == TrangThaiPhong.Trong).ToString(), Theme.Success, "hiện tại"),
            new StatCard("Đêm phòng đã bán", tongDem.ToString("N0"), Theme.Warning, $"trong {soNgay} ngày"),
            new StatCard("Công suất trung bình", $"{Math.Min(1, (double)tongDem / (Phongs.Count * soNgay)):P1}", Theme.Violet, "trong kỳ"));
        _chartPhong.Data = TrangThaiPhong.TatCa.Select(tt => (tt, new[] { (double)Phongs.Count(p => p.TrangThai == tt) })).ToList();
        _chartPhong.SeriesColors = TrangThaiPhong.TatCa.Select(Theme.StatusColor).ToArray();
        _chartPhong.Invalidate();

        // ---- 6.2 Khách hàng ----
        var khachKy = lts.Select(l => (lt: l, kh: HotelService.KhachChinh(l), hd: hds.First(h => h.MaLuuTru == l.MaLuuTru))).Where(x => x.kh != null).ToList();
        var khRows = khachKy.GroupBy(x => x.kh!.MaKH).Select(g =>
        {
            var k = g.First().kh!;
            return (k, luot: g.Count(), dem: g.Sum(x => HotelService.SoDem(x.lt.NgayNhan, x.lt.NgayTra)), chi: g.Sum(x => x.hd.TongTien));
        }).OrderByDescending(x => x.chi).Select((x, i) => new KhachRow(i + 1, x.k.MaKH, x.k.HoTen, x.k.SDT, x.luot, x.dem, x.chi)).ToList();
        _gKhach.Bind(khRows);
        var quayLai = khRows.Count(r => LuuTrus.Any(l => l.NgayNhan.Date < tu && HotelService.KhachChinh(l)?.MaKH == r.MaKH));
        var soKhach = lts.Sum(l => ChiTietLuuTrus.Count(c => c.MaLuuTru == l.MaLuuTru));
        SetCards(_cKhach,
            new StatCard("Lượt lưu trú", lts.Count.ToString(), Theme.Primary, "đã trả phòng trong kỳ"),
            new StatCard("Lượt khách", soKhach.ToString(), Theme.Info, "gồm khách đi cùng"),
            new StatCard("Khách hàng khác nhau", khRows.Count.ToString(), Theme.Success, "khách chính"),
            new StatCard("Khách quay lại", quayLai.ToString(), Theme.Warning, khRows.Count == 0 ? "" : $"{(double)quayLai / khRows.Count:P0} số khách"),
            new StatCard("Chi tiêu TB / lượt", DashboardView.Short(lts.Count == 0 ? 0 : hds.Sum(h => h.TongTien) / lts.Count), Theme.Violet, "đồng"));

        // ---- 6.3 Doanh thu ----
        bool theoThang = soNgay > 62;
        var groups = theoThang
            ? Enumerable.Range(0, (den.Year - tu.Year) * 12 + den.Month - tu.Month + 1).Select(i => new DateTime(tu.Year, tu.Month, 1).AddMonths(i))
                .Select(m => (label: m.ToString("MM/yyyy"), list: hds.Where(h => h.NgayLap.Year == m.Year && h.NgayLap.Month == m.Month).ToList()))
            : Enumerable.Range(0, soNgay).Select(i => tu.AddDays(i)).Select(d => (label: d.ToString("dd/MM"), list: hds.Where(h => h.NgayLap.Date == d).ToList()));
        var dtRows = groups.Select(g => new DoanhThuRow(g.label, g.list.Count, g.list.Sum(h => h.TienPhong), g.list.Sum(h => h.TienDV), g.list.Sum(h => h.GiamGia), g.list.Sum(h => h.TongTien))).ToList();
        _gDoanhThu.Bind(dtRows);
        _chart.Data = dtRows.Select(r => (r.KyBaoCao, new[] { (double)(r.TienPhong - r.GiamGia), (double)r.TienDV })).ToList();
        _chart.Invalidate();
        var tong = hds.Sum(h => h.TongTien);
        // so sánh với kỳ trước cùng độ dài
        var truoc = HotelService.HoaDonDaThu(tu.AddDays(-soNgay), tu.AddDays(-1)).Sum(h => h.TongTien);
        var tang = truoc == 0 ? "" : $"{(tong - truoc) / truoc:+0.0%;-0.0%} so với kỳ trước";
        SetCards(_cDoanhThu,
            new StatCard("Tổng doanh thu", DashboardView.Short(tong), Theme.Primary, tang),
            new StatCard("Tiền phòng", DashboardView.Short(hds.Sum(h => h.TienPhong)), Theme.Info, "trước giảm giá"),
            new StatCard("Tiền dịch vụ", DashboardView.Short(hds.Sum(h => h.TienDV)), Theme.Warning, tong == 0 ? "" : $"{hds.Sum(h => h.TienDV) / tong:P0} doanh thu"),
            new StatCard("Số hóa đơn", hds.Count.ToString(), Theme.Success, "đã thanh toán"),
            new StatCard("Doanh thu TB / ngày", DashboardView.Short(tong / soNgay), Theme.Violet, theoThang ? "biểu đồ theo tháng" : "biểu đồ theo ngày"));

        // ---- 6.4 Báo cáo doanh thu ----
        var tienPhongTong = hds.Sum(h => h.TienPhong);
        _gBcLoai.Bind(LoaiPhongs.Select(lp =>
        {
            var x = hds.Where(h => HotelService.PhongTheoMa(HotelService.LT(h.MaLuuTru)!.MaPhong)?.MaLoai == lp.MaLoai).ToList();
            var dt = x.Sum(h => h.TienPhong);
            return new LoaiDoanhThuRow(lp.TenLoai, x.Count, x.Sum(h => { var l = HotelService.LT(h.MaLuuTru)!; return HotelService.SoDem(l.NgayNhan, l.NgayTra); }),
                dt, tienPhongTong == 0 ? "-" : $"{dt / tienPhongTong:P1}");
        }).OrderByDescending(r => r.DoanhThu));
        var maLts = hds.Select(h => h.MaLuuTru).ToHashSet();
        var sd = SuDungDichVus.Where(s => maLts.Contains(s.MaLuuTru)).ToList();
        var dvTong = sd.Sum(s => s.ThanhTien);
        _gBcDv.Bind(sd.GroupBy(s => s.MaDV).Select(g =>
        {
            var dv = HotelService.DV(g.Key);
            var dt = g.Sum(s => s.ThanhTien);
            return new DichVuDoanhThuRow(dv?.TenDV ?? g.Key, dv?.DonViTinh ?? "", g.Sum(s => s.SoLuong), dt, dvTong == 0 ? "-" : $"{dt / dvTong:P1}");
        }).OrderByDescending(r => r.DoanhThu));
    }

    private static DateTime Min(DateTime a, DateTime b) => a < b ? a : b;
    private static DateTime Max(DateTime a, DateTime b) => a > b ? a : b;

    private static void SetCards(FlowLayoutPanel p, params StatCard[] cards)
    {
        var old = p.Controls.Cast<Control>().ToList();
        p.Controls.Clear();
        old.ForEach(c => c.Dispose());
        p.Controls.AddRange(cards);
    }

    /// <summary>6.5 Xuất báo cáo của tab đang xem.</summary>
    private void Export()
    {
        var ky = $"{_from.Value:dd/MM/yyyy} - {_to.Value:dd/MM/yyyy}";
        var file = $"{_from.Value:yyyyMMdd}_{_to.Value:yyyyMMdd}";
        switch (_tabs.SelectedIndex)
        {
            case 0: UiKit.ExportCsv($"ThongKe_TinhTrangPhong_{file}", $"THỐNG KÊ TÌNH TRẠNG PHÒNG ({ky})", _gLoai); break;
            case 1: UiKit.ExportCsv($"ThongKe_KhachHang_{file}", $"THỐNG KÊ KHÁCH HÀNG ({ky})", _gKhach); break;
            case 2: UiKit.ExportCsv($"ThongKe_DoanhThu_{file}", $"THỐNG KÊ DOANH THU ({ky})", _gDoanhThu); break;
            default: UiKit.ExportCsv($"BaoCao_DoanhThu_{file}", $"BÁO CÁO DOANH THU ({ky})", _gBcLoai, _gBcDv); break;
        }
    }
}
