using HotelManagement.Data;
using HotelManagement.UI;

namespace HotelManagement.Forms;

/// <summary>1.1 Đăng nhập.</summary>
public class LoginForm : Form
{
    private readonly TextBox _user;
    private readonly TextBox _pass;
    private readonly Label _error;

    public LoginForm()
    {
        Text = "Đăng nhập - Hệ thống quản lý khách sạn";
        Font = Theme.Base;
        ClientSize = new Size(900, 540);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Surface;

        // Bên trái: nhận diện thương hiệu
        var brand = new Panel { Dock = DockStyle.Left, Width = 400, BackColor = Theme.Sidebar };
        var logo = UiKit.Lbl("🏨", new Font("Segoe UI Emoji", 40f), Color.White);
        logo.Location = new Point(48, 110);
        var name = UiKit.Lbl(Theme.HotelName, new Font(Theme.FontName, 26f, FontStyle.Bold), Color.White);
        name.Location = new Point(46, 196);
        var slogan = UiKit.Lbl("Hệ thống quản lý tòa khách sạn", new Font(Theme.FontName, 12f), Theme.SidebarText);
        slogan.Location = new Point(50, 246);
        var desc = UiKit.Lbl("Đặt phòng • Nhận phòng • Lưu trú & dịch vụ\nTrả phòng • Hóa đơn • Thống kê doanh thu", Theme.Base, Theme.SidebarMuted);
        desc.Location = new Point(50, 290);
        var foot = UiKit.Lbl("Đồ án kết thúc môn học - Khoa CNTT, ĐH Sài Gòn", Theme.Small, Theme.SidebarMuted);
        foot.Location = new Point(50, 490);
        brand.Controls.AddRange(new Control[] { logo, name, slogan, desc, foot });

        // Bên phải: biểu mẫu đăng nhập
        var form = new Panel { Dock = DockStyle.Fill, BackColor = Theme.Surface };
        int x = 70;
        var title = UiKit.Lbl("Đăng nhập", Theme.H1);
        title.Location = new Point(x, 90);
        var sub = UiKit.Lbl("Vui lòng đăng nhập để tiếp tục", Theme.Base, Theme.Muted);
        sub.Location = new Point(x + 2, 128);

        var l1 = UiKit.Lbl("Tên đăng nhập", Theme.Bold);
        l1.Location = new Point(x, 176);
        _user = new TextBox { Location = new Point(x, 200), Width = 360, Font = new Font(Theme.FontName, 11f), Text = "admin" };
        var l2 = UiKit.Lbl("Mật khẩu", Theme.Bold);
        l2.Location = new Point(x, 244);
        _pass = new TextBox { Location = new Point(x, 268), Width = 360, Font = new Font(Theme.FontName, 11f), UseSystemPasswordChar = true, Text = "123" };
        var show = new CheckBox { Text = "Hiện mật khẩu", Location = new Point(x, 306), AutoSize = true, ForeColor = Theme.Muted };
        show.CheckedChanged += (_, _) => _pass.UseSystemPasswordChar = !show.Checked;

        _error = UiKit.Lbl("", Theme.Base, Theme.Danger);
        _error.Location = new Point(x, 336);

        var btn = UiKit.Btn("Đăng nhập", BtnKind.Primary, 360);
        btn.Height = 42;
        btn.Location = new Point(x, 364);
        btn.Click += (_, _) => DoLogin();
        AcceptButton = btn;

        var hint = UiKit.Lbl("Tài khoản demo (mật khẩu 123):\nadmin • quanly • letan • buongphong   (letan2 đang bị khóa)", Theme.Small, Theme.Muted);
        hint.Location = new Point(x, 424);

        form.Controls.AddRange(new Control[] { title, sub, l1, _user, l2, _pass, show, _error, btn, hint });
        Controls.Add(form);
        Controls.Add(brand);
    }

    private void DoLogin()
    {
        var err = Session.DangNhap(_user.Text, _pass.Text);
        if (err != null)
        {
            _error.Text = err;
            return;
        }
        DialogResult = DialogResult.OK;
    }
}
