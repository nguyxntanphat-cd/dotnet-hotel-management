using System.Globalization;
using HotelManagement.Data;
using HotelManagement.Forms;
using HotelManagement.UI;

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

        if (DataSource.LaSql)
        {
            try { Db.KiemTraKetNoi(); }
            catch (Exception ex)
            {
                UiKit.Warn($"Không kết nối được SQL Server (HOTEL_DATA=Sql).\n\n{ex.Message}\n\n" +
                           "Kiểm tra đã chạy Data/Sql/01_schema.sql, 02_seed.sql và biến HOTEL_DB.", "Lỗi kết nối");
                return;
            }
        }

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
