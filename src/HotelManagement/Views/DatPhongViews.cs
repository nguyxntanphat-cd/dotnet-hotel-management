using HotelManagement.Data;
using HotelManagement.Forms;
using HotelManagement.Models;
using HotelManagement.UI;
using static HotelManagement.Data.MockDb;

namespace HotelManagement.Views;

/// <summary>3.1 Quản lý khách hàng.</summary>
public class KhachHangView : BaseView
{
    private record LichSuRow(string MaLuuTru, string SoPhong, DateTime NgayNhan, DateTime NgayTra, string VaiTro, string TrangThai);

    private readonly DataGridView _grid = UiKit.Grid();
    private readonly DataGridView _history = UiKit.Grid();
    private readonly TextBox _search = UiKit.Search("Tìm theo tên, CCCD/hộ chiếu, SĐT...", 300);
    private readonly Label _histTitle = UiKit.Lbl("Lịch sử lưu trú", Theme.H2);

    public KhachHangView() : base("Quản lý khách hàng", "Thông tin khách hàng và lịch sử lưu trú tại khách sạn.")
    {
        var add = UiKit.Btn("+ Thêm khách hàng");
        var edit = UiKit.Btn("Sửa", BtnKind.Secondary);
        var del = UiKit.Btn("Xóa", BtnKind.Danger);
        Toolbar.Controls.Add(_search);
        Spacer();
        Toolbar.Controls.AddRange(new Control[] { add, edit, del });

        _grid.Columns(
            new Col("MaKH", "Mã KH", 80),
            new Col("HoTen", "Họ tên", 190, Fill: true),
            new Col("CCCD_HoChieu", "CCCD / Hộ chiếu", 150),
            new Col("SDT", "Số điện thoại", 130),
            new Col("Email", "Email", 220),
            new Col("DiaChi", "Địa chỉ", 180));
        _history.Columns(
            new Col("MaLuuTru", "Mã lưu trú", 100),
            new Col("SoPhong", "Phòng", 80),
            new Col("NgayNhan", "Ngày nhận", 140, "dd/MM/yyyy HH:mm"),
            new Col("NgayTra", "Ngày trả", 140, "dd/MM/yyyy HH:mm"),
            new Col("VaiTro", "Vai trò", 130),
            new Col("TrangThai", "Trạng thái", 130, Fill: true, Status: true));

        var top = UiKit.Card(padding: new Padding(1));
        top.Dock = DockStyle.Fill;
        top.Controls.Add(_grid);
        var bottom = UiKit.Card(padding: new Padding(1, 48, 1, 1));
        bottom.Dock = DockStyle.Bottom;
        bottom.Height = 220;
        _histTitle.Location = new Point(16, 14);
        bottom.Controls.Add(_history);
        bottom.Controls.Add(_histTitle);
        Body.Controls.Add(top);
        Body.Controls.Add(new Panel { Dock = DockStyle.Bottom, Height = 14 });
        Body.Controls.Add(bottom);

        _search.TextChanged += (_, _) => Reload();
        _grid.SelectionChanged += (_, _) => LoadHistory();
        add.Click += (_, _) => { if (EditDialog(this, null) != null) Reload(); };
        edit.Click += (_, _) => { if (_grid.Selected<KhachHang>() is { } k && EditDialog(this, k) != null) Reload(); };
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0 && _grid.Selected<KhachHang>() is { } k && EditDialog(this, k) != null) Reload(); };
        del.Click += (_, _) =>
        {
            if (_grid.Selected<KhachHang>() is not { } k) return;
            if (DatPhongs.Any(d => d.MaKH == k.MaKH) || ChiTietLuuTrus.Any(c => c.MaKH == k.MaKH))
            {
                UiKit.Warn("Khách hàng đã có lịch sử đặt phòng/lưu trú nên không thể xóa.");
                return;
            }
            if (!UiKit.Confirm($"Xóa khách hàng \"{k.HoTen}\"?")) return;
            KhachHangs.Remove(k);
            Reload();
        };
        Reload();
    }

    public override void Reload()
    {
        var q = _search.Text.Trim().ToLower();
        _grid.Bind(KhachHangs.Where(k => q == "" || k.HoTen.ToLower().Contains(q) || k.CCCD_HoChieu.ToLower().Contains(q) || k.SDT.Contains(q) || k.MaKH.ToLower().Contains(q)));
        LoadHistory();
    }

    private void LoadHistory()
    {
        var k = _grid.Selected<KhachHang>();
        _histTitle.Text = k == null ? "Lịch sử lưu trú" : $"Lịch sử lưu trú - {k.HoTen}";
        _history.Bind(ChiTietLuuTrus.Where(c => c.MaKH == k?.MaKH)
            .Select(c => (c, lt: HotelService.LT(c.MaLuuTru)!))
            .OrderByDescending(x => x.lt.NgayNhan)
            .Select(x => new LichSuRow(x.lt.MaLuuTru, HotelService.PhongTheoMa(x.lt.MaPhong)?.SoPhong ?? "", x.lt.NgayNhan, x.lt.NgayTra, x.c.VaiTro, x.lt.TrangThai)));
    }

    /// <summary>Hộp thoại thêm/sửa khách hàng, dùng lại ở màn hình đặt phòng và nhận phòng.</summary>
    public static KhachHang? EditDialog(IWin32Window owner, KhachHang? kh)
    {
        using var d = new FormDialog(kh == null ? "Thêm khách hàng" : "Sửa thông tin khách hàng");
        var ten = d.TextField("Họ tên", kh?.HoTen ?? "");
        var cccd = d.TextField("CCCD / Hộ chiếu", kh?.CCCD_HoChieu ?? "");
        var sdt = d.TextField("Số điện thoại", kh?.SDT ?? "");
        var email = d.TextField("Email", kh?.Email ?? "");
        var dc = d.TextField("Địa chỉ", kh?.DiaChi ?? "");
        d.Validator = () =>
        {
            if (string.IsNullOrWhiteSpace(ten.Text)) return "Họ tên không được để trống.";
            if (cccd.Text.Trim().Length < 6) return "CCCD / hộ chiếu không hợp lệ.";
            if (KhachHangs.Any(k => k != kh && k.CCCD_HoChieu == cccd.Text.Trim())) return "CCCD / hộ chiếu đã tồn tại trong hệ thống.";
            if (sdt.Text.Trim().Length < 9) return "Số điện thoại không hợp lệ.";
            if (email.Text.Length > 0 && !email.Text.Contains('@')) return "Email không hợp lệ.";
            return null;
        };
        if (d.ShowDialog(owner) != DialogResult.OK) return null;
        if (kh == null)
        {
            kh = new KhachHang { MaKH = NextId(KhachHangs, k => k.MaKH, "KH") };
            KhachHangs.Add(kh);
        }
        kh.HoTen = ten.Text.Trim();
        kh.CCCD_HoChieu = cccd.Text.Trim();
        kh.SDT = sdt.Text.Trim();
        kh.Email = email.Text.Trim();
        kh.DiaChi = dc.Text.Trim();
        return kh;
    }

    public static string KhLabel(KhachHang k) => $"{k.MaKH} - {k.HoTen} - {k.SDT}";
}

/// <summary>3.2 Tiếp nhận đặt phòng, 3.3 Sửa, 3.4 Hủy, 3.5 Tra cứu đặt phòng.</summary>
public class DatPhongView : BaseView
{
    private record Row(string MaDat, string KhachHang, string SDT, string SoPhong, string LoaiPhong, DateTime NgayNhan, DateTime NgayTra,
        int SoDem, decimal TamTinh, string TrangThai, DateTime NgayDat, string GhiChu, DatPhong E);

    private readonly DataGridView _grid = UiKit.Grid();
    private readonly TextBox _search = UiKit.Search("Mã đặt, tên khách, SĐT, số phòng...", 260);
    private readonly ComboBox _status = UiKit.Combo(new[] { "Tất cả trạng thái" }.Concat(TrangThaiDatPhong.TatCa), 160, TrangThaiDatPhong.ChoNhanPhong);
    private readonly DateTimePicker _from = UiKit.Date(DateTime.Today.AddDays(-30));
    private readonly DateTimePicker _to = UiKit.Date(DateTime.Today.AddDays(60));

    public DatPhongView() : base("Đặt phòng", "Tiếp nhận, sửa, hủy và tra cứu đặt phòng. Hệ thống tự kiểm tra để tránh trùng phòng.")
    {
        var add = UiKit.Btn("+ Đặt phòng mới");
        var edit = UiKit.Btn("Sửa", BtnKind.Secondary);
        var cancel = UiKit.Btn("Hủy đặt phòng", BtnKind.Danger);
        var checkin = UiKit.Btn("Nhận phòng →", BtnKind.Success);
        Label L(string s) { var l = UiKit.Lbl(s, Theme.Base, Theme.Muted); l.Margin = new Padding(0, 8, 6, 0); return l; }
        Toolbar.Controls.AddRange(new Control[] { _search, _status, L("Nhận từ"), _from, L("đến"), _to });
        Spacer(8);
        Toolbar.Controls.AddRange(new Control[] { add, edit, cancel, checkin });

        _grid.Columns(
            new Col("MaDat", "Mã đặt", 80),
            new Col("KhachHang", "Khách hàng", 170, Fill: true),
            new Col("SDT", "SĐT", 110),
            new Col("SoPhong", "Phòng", 70),
            new Col("LoaiPhong", "Loại", 90),
            new Col("NgayNhan", "Nhận phòng", 105, "dd/MM/yyyy"),
            new Col("NgayTra", "Trả phòng", 105, "dd/MM/yyyy"),
            new Col("SoDem", "Đêm", 60, Right: true),
            new Col("TamTinh", "Tạm tính", 120, "N0", Right: true),
            new Col("TrangThai", "Trạng thái", 140, Status: true),
            new Col("GhiChu", "Ghi chú", 160));
        var card = UiKit.Card(padding: new Padding(1));
        card.Dock = DockStyle.Fill;
        card.Controls.Add(_grid);
        Body.Controls.Add(card);

        _search.TextChanged += (_, _) => Reload();
        _status.SelectedIndexChanged += (_, _) => Reload();
        _from.ValueChanged += (_, _) => Reload();
        _to.ValueChanged += (_, _) => Reload();
        add.Click += (_, _) => Edit(null);
        edit.Click += (_, _) => { if (_grid.Selected<Row>() is { } r) Edit(r.E); };
        _grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0 && _grid.Selected<Row>() is { } r) Edit(r.E); };
        cancel.Click += (_, _) => Cancel();
        checkin.Click += (_, _) =>
        {
            if (_grid.Selected<Row>() is { } r && r.E.TrangThai == TrangThaiDatPhong.ChoNhanPhong) NhanPhongView.PreselectMaDat = r.MaDat;
            MainForm.Current?.Navigate("nhan_phong");
        };
        if (!Session.CoQuyen("nhan_phong")) checkin.Visible = false;
        Reload();
    }

    public override void Reload()
    {
        var q = _search.Text.Trim().ToLower();
        var st = _status.SelectedIndex > 0 ? (string)_status.SelectedItem! : null;
        _grid.Bind(DatPhongs
            .Where(d => (st == null || d.TrangThai == st) && d.NgayNhanDuKien.Date >= _from.Value.Date && d.NgayNhanDuKien.Date <= _to.Value.Date)
            .Select(ToRow)
            .Where(r => q == "" || r.MaDat.ToLower().Contains(q) || r.KhachHang.ToLower().Contains(q) || r.SDT.Contains(q) || r.SoPhong.Contains(q))
            .OrderBy(r => r.NgayNhan).ThenBy(r => r.SoPhong));
    }

    private static Row ToRow(DatPhong d)
    {
        var k = HotelService.Khach(d.MaKH);
        var p = HotelService.PhongTheoMa(d.MaPhong);
        var l = HotelService.Loai(p?.MaLoai);
        var dem = HotelService.SoDem(d.NgayNhanDuKien, d.NgayTraDuKien);
        return new Row(d.MaDat, k?.HoTen ?? "", k?.SDT ?? "", p?.SoPhong ?? "", l?.TenLoai ?? "", d.NgayNhanDuKien, d.NgayTraDuKien,
            dem, dem * HotelService.GiaPhong(p?.MaLoai ?? "", d.NgayNhanDuKien), d.TrangThai, d.NgayDat, d.GhiChu, d);
    }

    private void Cancel()
    {
        if (_grid.Selected<Row>() is not { } r) return;
        if (r.E.TrangThai != TrangThaiDatPhong.ChoNhanPhong) { UiKit.Warn("Chỉ hủy được đặt phòng đang ở trạng thái \"Chờ nhận phòng\"."); return; }
        if (!UiKit.Confirm($"Hủy đặt phòng {r.MaDat} của {r.KhachHang} (phòng {r.SoPhong}, {r.NgayNhan:dd/MM} - {r.NgayTra:dd/MM})?")) return;
        r.E.TrangThai = TrangThaiDatPhong.DaHuy;
        var p = HotelService.PhongTheoMa(r.E.MaPhong);
        if (p?.TrangThai == TrangThaiPhong.DaDat && !DatPhongs.Any(d => d.MaPhong == p.MaPhong && d.TrangThai == TrangThaiDatPhong.ChoNhanPhong && d.NgayNhanDuKien.Date <= DateTime.Today))
            p.TrangThai = TrangThaiPhong.Trong;
        Reload();
    }

    private void Edit(DatPhong? dp)
    {
        if (dp != null && dp.TrangThai != TrangThaiDatPhong.ChoNhanPhong)
        {
            UiKit.Warn($"Đặt phòng {dp.MaDat} đang \"{dp.TrangThai}\" nên không thể sửa.");
            return;
        }
        const string themMoi = "➕ Thêm khách hàng mới...";
        using var d = new FormDialog(dp == null ? "Tiếp nhận đặt phòng" : $"Sửa đặt phòng {dp.MaDat}");
        var khItems = new[] { themMoi }.Concat(KhachHangs.Select(KhachHangView.KhLabel)).ToList();
        var kh = d.ComboField("Khách hàng", khItems, khItems.FirstOrDefault(s => dp != null && s.StartsWith(dp.MaKH + " ")) ?? khItems[1]);
        var curLoai = HotelService.Loai(HotelService.PhongTheoMa(dp?.MaPhong)?.MaLoai);
        var loai = d.ComboField("Loại phòng", LoaiPhongs.Select(l => $"{l.TenLoai} ({l.SucChua} người)"), curLoai == null ? null : $"{curLoai.TenLoai} ({curLoai.SucChua} người)");
        var nhan = d.DateField("Ngày nhận", dp?.NgayNhanDuKien ?? DateTime.Today);
        var tra = d.DateField("Ngày trả", dp?.NgayTraDuKien ?? DateTime.Today.AddDays(1));
        var phong = d.ComboField("Phòng trống", Array.Empty<string>());
        var note = d.TextField("Ghi chú", dp?.GhiChu ?? "");
        var est = d.Note("", Theme.Primary);

        LoaiPhong SelLoai() => LoaiPhongs[loai.SelectedIndex];
        void refreshRooms()
        {
            var keep = phong.SelectedItem as string ?? (dp == null ? null : HotelService.PhongTheoMa(dp.MaPhong)?.SoPhong);
            phong.Items.Clear();
            if (tra.Value.Date > nhan.Value.Date)
                foreach (var p in HotelService.PhongTrong(nhan.Value, tra.Value, SelLoai().MaLoai, dp?.MaDat)) phong.Items.Add(p.SoPhong);
            if (keep != null && phong.Items.Contains(keep)) phong.SelectedItem = keep;
            else if (phong.Items.Count > 0) phong.SelectedIndex = 0;
            var dem = HotelService.SoDem(nhan.Value, tra.Value);
            var gia = HotelService.GiaPhong(SelLoai().MaLoai, nhan.Value);
            est.Text = phong.Items.Count == 0 ? "Không còn phòng trống phù hợp trong khoảng ngày này."
                : $"{phong.Items.Count} phòng trống • {dem} đêm × {gia:N0} đ = {dem * gia:N0} đ";
        }
        kh.SelectedIndexChanged += (_, _) =>
        {
            if (kh.SelectedIndex != 0) return;
            var moi = KhachHangView.EditDialog(d, null);
            if (moi == null) { kh.SelectedIndex = 1; return; }
            kh.Items.Add(KhachHangView.KhLabel(moi));
            kh.SelectedIndex = kh.Items.Count - 1;
        };
        loai.SelectedIndexChanged += (_, _) => { phong.SelectedIndex = -1; refreshRooms(); };
        nhan.ValueChanged += (_, _) => refreshRooms();
        tra.ValueChanged += (_, _) => refreshRooms();
        refreshRooms();

        d.Validator = () =>
        {
            if (kh.SelectedIndex <= 0) return "Vui lòng chọn khách hàng.";
            if (dp == null && nhan.Value.Date < DateTime.Today) return "Ngày nhận không được ở quá khứ.";
            if (tra.Value.Date <= nhan.Value.Date) return "Ngày trả phải sau ngày nhận ít nhất 1 đêm.";
            if (phong.SelectedItem == null) return "Không còn phòng trống phù hợp. Hãy đổi loại phòng hoặc ngày.";
            var p = Phongs.First(x => x.SoPhong == (string)phong.SelectedItem);
            if (HotelService.TrungLich(p.MaPhong, nhan.Value, tra.Value, dp?.MaDat)) return $"Phòng {p.SoPhong} đã có người đặt trong khoảng thời gian này.";
            return null;
        };
        if (d.ShowDialog(this) != DialogResult.OK) return;

        if (dp == null)
        {
            dp = new DatPhong { MaDat = NextId(DatPhongs, x => x.MaDat, "DP"), NgayDat = DateTime.Now };
            DatPhongs.Add(dp);
        }
        var oldRoom = HotelService.PhongTheoMa(dp.MaPhong);
        dp.MaKH = ((string)kh.SelectedItem!).Split(" - ")[0];
        dp.MaPhong = Phongs.First(x => x.SoPhong == (string)phong.SelectedItem!).MaPhong;
        dp.NgayNhanDuKien = nhan.Value.Date;
        dp.NgayTraDuKien = tra.Value.Date;
        dp.GhiChu = note.Text.Trim();
        if (oldRoom != null && oldRoom.MaPhong != dp.MaPhong && oldRoom.TrangThai == TrangThaiPhong.DaDat) oldRoom.TrangThai = TrangThaiPhong.Trong;
        var room = HotelService.PhongTheoMa(dp.MaPhong)!;
        if (dp.NgayNhanDuKien == DateTime.Today && room.TrangThai == TrangThaiPhong.Trong) room.TrangThai = TrangThaiPhong.DaDat;
        UiKit.Info($"Đã lưu đặt phòng {dp.MaDat}: phòng {room.SoPhong}, {dp.NgayNhanDuKien:dd/MM/yyyy} → {dp.NgayTraDuKien:dd/MM/yyyy}.");
        _status.SelectedItem = TrangThaiDatPhong.ChoNhanPhong;
        Reload();
    }
}
