
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRazorApp.Pages
{
    public class IndexModel : PageModel
    {
        public List<ChartBarModel> ChartData { get; set; } = new();

        public void OnGet()
        {
            ChartData = new List<ChartBarModel>
            {
                new() { Date = "27-09", RoomRevenue = 15.0, ServiceRevenue = 4.9 },
                new() { Date = "28-09", RoomRevenue = 14.0, ServiceRevenue = 4.4 },
                new() { Date = "29-09", RoomRevenue = 2.0,  ServiceRevenue = 1.1 },
                new() { Date = "30-09", RoomRevenue = 12.0, ServiceRevenue = 4.7 },
                new() { Date = "01-10", RoomRevenue = 13.0, ServiceRevenue = 4.2 },
                new() { Date = "02-10", RoomRevenue = 4.5,  ServiceRevenue = 1.8 },
                new() { Date = "03-10", RoomRevenue = 8.0,  ServiceRevenue = 2.6 },
                new() { Date = "04-10", RoomRevenue = 8.0,  ServiceRevenue = 2.5 },
                new() { Date = "05-10", RoomRevenue = 14.0, ServiceRevenue = 5.3 },
                new() { Date = "06-10", RoomRevenue = 18.0, ServiceRevenue = 7.3 },
                new() { Date = "07-10", RoomRevenue = 13.0, ServiceRevenue = 5.1 },
                new() { Date = "08-10", RoomRevenue = 21.0, ServiceRevenue = 7.3 },
                new() { Date = "09-10", RoomRevenue = 12.0, ServiceRevenue = 5.1 },
                new() { Date = "10-10", RoomRevenue = 16.0, ServiceRevenue = 6.6 }
            };

            // Tính chiều cao cột dựa trên doanh thu lớn nhất
            double maxRevenue = ChartData.Max(x =>
                x.RoomRevenue + x.ServiceRevenue);

            foreach (var item in ChartData)
            {
                double total = item.RoomRevenue + item.ServiceRevenue;

                // Chiều cao tối đa 180px
                item.Height = Math.Max(
                    6,
                    (int)Math.Round(total / maxRevenue * 180));

                item.RoomHeight = (int)Math.Round(
                    item.RoomRevenue / maxRevenue * 180);

                item.ServiceHeight = item.Height - item.RoomHeight;

                // Chuỗi doanh thu hiển thị phía trên cột
                item.Value = total.ToString("0.0") + "tr";
            }
        }
    }

    public class ChartBarModel
    {
        public string Date { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public double RoomRevenue { get; set; }

        public double ServiceRevenue { get; set; }

        public int Height { get; set; }

        public int RoomHeight { get; set; }

        public int ServiceHeight { get; set; }
    }
}
