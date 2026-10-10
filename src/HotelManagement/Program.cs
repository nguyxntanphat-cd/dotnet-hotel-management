using System.Globalization;
using HotelManagement.Forms;

namespace HotelManagement;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        var vi = new CultureInfo("vi-VN");
        CultureInfo.DefaultThreadCurrentCulture = vi;
        CultureInfo.DefaultThreadCurrentUICulture = vi;
        Thread.CurrentThread.CurrentCulture = vi;
        Thread.CurrentThread.CurrentUICulture = vi;

        ApplicationConfiguration.Initialize();

        // Vòng lặp đăng nhập → màn hình chính → đăng xuất → đăng nhập lại.
        while (true)
        {
            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK) break;

            using var main = new MainForm();
            Application.Run(main);
            if (!main.DaDangXuat) break;
        }
    }
}
