using HotelManagement.Data;
using HotelManagement.UI;

namespace HotelManagement.Forms;

/// <summary>Màn hình chính: menu bên trái theo sơ đồ BFD (lọc theo quyền), thanh trên và vùng nội dung.</summary>
public class MainForm : Form
{
    public static MainForm? Current { get; private set; }
    public bool DaDangXuat { get; private set; }

    private readonly Panel _content;
    private readonly Label _crumb;
    private readonly Dictionary<string, Button> _menuButtons = new();
    private string? _activeKey;

    public MainForm()
    {
        Current = this;
        Text = "Hệ thống quản lý khách sạn";
        Font = Theme.Base;
        MinimumSize = new Size(1200, 720);
        Size = new Size(1360, 820);
        StartPosition = FormStartPosition.CenterScreen;
        WindowState = FormWindowState.Maximized;
        BackColor = Theme.Background;

        _content = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Background };

        // ---- Thanh trên ----
        var top = new BorderPanel { Dock = DockStyle.Top, Height = 56, BackColor = Theme.Surface };
        _crumb = UiKit.Lbl("", Theme.Base, Theme.Muted);
        _crumb.Location = new Point(24, 19);
        var user = Session.NhanVien;
        var who = UiKit.Lbl($"{user?.HoTen ?? Session.TaiKhoan?.TenDangNhap}", Theme.Bold);
        var role = UiKit.Lbl(Session.VaiTro, Theme.Small, Theme.Muted);
        var logout = UiKit.Btn("Đăng xuất", BtnKind.Secondary, 100);
        logout.Height = 32;
        logout.Click += (_, _) =>
        {
            if (!UiKit.Confirm("Bạn có chắc muốn đăng xuất?")) return;
            Session.DangXuat();
            DaDangXuat = true;
            Close();
        };
        var clock = UiKit.Lbl(DateTime.Now.ToString("dddd, dd/MM/yyyy"), Theme.Base, Theme.Muted);
        top.Controls.AddRange(new Control[] { _crumb, who, role, logout, clock });
        top.Resize += (_, _) =>
        {
            logout.Location = new Point(top.Width - logout.Width - 20, 12);
            who.Location = new Point(logout.Left - 16 - Math.Max(who.Width, role.Width), 8);
            role.Location = new Point(who.Left, 28);
            clock.Location = new Point(who.Left - clock.Width - 32, 19);
        };

        // ---- Menu bên trái ----
        var side = new Panel { Dock = DockStyle.Left, Width = 250, BackColor = Theme.Sidebar };
        var brand = new Panel { Dock = DockStyle.Top, Height = 64, BackColor = Theme.Sidebar };
        var bName = UiKit.Lbl("🏨  " + Theme.HotelName, new Font(Theme.FontName, 14f, FontStyle.Bold), Color.White);
        bName.Location = new Point(20, 20);
        brand.Controls.Add(bName);

        var menu = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Theme.Sidebar,
            Padding = new Padding(10, 4, 0, 16),
        };
        string? nhom = null;
        foreach (var item in Menu.Items.Where(i => Session.CoQuyen(i.Key)))
        {
            if (item.Nhom != nhom && item.Nhom != "Tổng quan")
            {
                var g = UiKit.Lbl(item.Nhom.ToUpper(), new Font(Theme.FontName, 7.75f, FontStyle.Bold), Theme.SidebarMuted);
                g.Margin = new Padding(10, 14, 0, 4);
                menu.Controls.Add(g);
            }
            nhom = item.Nhom;
            var b = new Button
            {
                Text = "   " + item.Ten,
                TextAlign = ContentAlignment.MiddleLeft,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Theme.SidebarText,
                BackColor = Theme.Sidebar,
                Font = Theme.Base,
                Size = new Size(228, 36),
                Margin = new Padding(0, 1, 0, 1),
                Cursor = Cursors.Hand,
                Tag = item.Key,
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Theme.SidebarHover;
            b.Click += (_, _) => Navigate(item.Key);
            _menuButtons[item.Key] = b;
            menu.Controls.Add(b);
        }
        side.Controls.Add(menu);
        side.Controls.Add(brand);

        Controls.Add(_content);
        Controls.Add(top);
        Controls.Add(side);

        Load += (_, _) => Navigate(_menuButtons.Keys.First());
    }

    /// <summary>Mở một chức năng theo mã (dùng cả cho menu và cho các nút chuyển nhanh giữa màn hình).</summary>
    public void Navigate(string key)
    {
        var item = Menu.Items.FirstOrDefault(i => i.Key == key);
        if (item == null) return;
        if (!Session.CoQuyen(key))
        {
            UiKit.Warn("Tài khoản của bạn không có quyền sử dụng chức năng này.", "Không có quyền");
            return;
        }
        if (_activeKey != null && _menuButtons.TryGetValue(_activeKey, out var old))
        {
            old.BackColor = Theme.Sidebar;
            old.ForeColor = Theme.SidebarText;
            old.Font = Theme.Base;
        }
        if (_menuButtons.TryGetValue(key, out var btn))
        {
            btn.BackColor = Theme.Primary;
            btn.ForeColor = Color.White;
            btn.Font = Theme.Bold;
        }
        _activeKey = key;
        _crumb.Text = item.Nhom == "Tổng quan" ? "Tổng quan" : $"{item.Nhom}  ›  {item.Ten}";

        SuspendLayout();
        var cu = _content.Controls.Cast<Control>().ToList();
        _content.Controls.Clear();
        cu.ForEach(c => c.Dispose());
        _content.Controls.Add(item.TaoManHinh());
        ResumeLayout();
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        if (Current == this) Current = null;
        base.OnFormClosed(e);
    }
}
