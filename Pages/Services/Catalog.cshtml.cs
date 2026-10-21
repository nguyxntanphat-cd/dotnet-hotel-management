using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace MyRazorApp.Pages.Services
{
    public class CatalogModel : PageModel
    {
        public static readonly string[] Categories = { "Ăn uống", "Giặt ủi", "Spa & Làm đẹp", "Đưa đón", "Giải trí", "Khác" };
        public static readonly string[] Units = { "Lần", "Suất", "Kg", "Giờ", "Chuyến", "Chai", "Ngày" };

        private static List<ServiceItem> _staticServicesList = new()
        {
            new() { Id = "DV-1001", Name = "Buffet sáng", Category = "Ăn uống", Price = 150000m, Unit = "Suất", Description = "Phục vụ 6:00 - 10:00 tại nhà hàng tầng 1", Status = "Đang cung cấp" },
            new() { Id = "DV-1002", Name = "Nước suối minibar", Category = "Ăn uống", Price = 20000m, Unit = "Chai", Description = "Nước suối 500ml trong minibar", Status = "Đang cung cấp" },
            new() { Id = "DV-1003", Name = "Giặt ủi thường", Category = "Giặt ủi", Price = 50000m, Unit = "Kg", Description = "Trả đồ trong vòng 24 giờ", Status = "Đang cung cấp" },
            new() { Id = "DV-1004", Name = "Massage body 60 phút", Category = "Spa & Làm đẹp", Price = 450000m, Unit = "Lần", Description = "Đặt trước tại quầy lễ tân", Status = "Đang cung cấp" },
            new() { Id = "DV-1005", Name = "Đưa đón sân bay", Category = "Đưa đón", Price = 300000m, Unit = "Chuyến", Description = "Xe 4 chỗ, một chiều", Status = "Đang cung cấp" },
            new() { Id = "DV-1006", Name = "Thuê xe máy", Category = "Đưa đón", Price = 150000m, Unit = "Ngày", Description = "Bao gồm 2 mũ bảo hiểm", Status = "Tạm ngưng" }
        };

        public List<ServiceItem> ServicesList { get; set; } = new();

        // Tổng số dịch vụ (không phụ thuộc bộ lọc) để hiển thị thống kê
        public int TotalCount => _staticServicesList.Count;
        public int ActiveCount => _staticServicesList.Count(x => x.Status == "Đang cung cấp");
        public int PausedCount => _staticServicesList.Count(x => x.Status == "Tạm ngưng");
        public int CategoryCount => _staticServicesList.Select(x => x.Category).Distinct().Count();

        // Các thuộc tính nhận giá trị từ form tìm kiếm & lọc
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CategoryFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty]
        public ServiceItem NewService { get; set; } = new();

        [BindProperty]
        public ServiceItem EditService { get; set; } = new();

        public void OnGet()
        {
            var query = _staticServicesList.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.ToLower();
                query = query.Where(x => x.Id.ToLower().Contains(term) ||
                                         x.Name.ToLower().Contains(term) ||
                                         (x.Description != null && x.Description.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                query = query.Where(x => x.Category == CategoryFilter);
            }

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                query = query.Where(x => x.Status == StatusFilter);
            }

            ServicesList = query.ToList();
        }

        // Sinh mã kế tiếp dựa trên mã lớn nhất (tránh trùng mã sau khi xóa)
        public static string NextId()
        {
            var max = _staticServicesList
                .Select(x => int.TryParse(x.Id.Replace("DV-", ""), out var n) ? n : 1000)
                .DefaultIfEmpty(1000)
                .Max();
            return "DV-" + (max + 1);
        }

        public IActionResult OnPostCreate()
        {
            if (NewService != null && !string.IsNullOrWhiteSpace(NewService.Name))
            {
                NewService.Id = NextId();
                _staticServicesList.Add(NewService);
            }
            return RedirectToPage();
        }

        public IActionResult OnPostEdit()
        {
            var service = _staticServicesList.FirstOrDefault(x => x.Id == EditService.Id);
            if (service != null)
            {
                service.Name = EditService.Name;
                service.Category = EditService.Category;
                service.Price = EditService.Price;
                service.Unit = EditService.Unit;
                service.Description = EditService.Description;
                service.Status = EditService.Status;
            }
            return RedirectToPage();
        }

        public IActionResult OnPostToggleStatus(string id)
        {
            var service = _staticServicesList.FirstOrDefault(x => x.Id == id);
            if (service != null)
            {
                service.Status = service.Status == "Đang cung cấp" ? "Tạm ngưng" : "Đang cung cấp";
            }
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(string id)
        {
            var service = _staticServicesList.FirstOrDefault(x => x.Id == id);
            if (service != null)
            {
                _staticServicesList.Remove(service);
            }
            return RedirectToPage();
        }
    }

    public class ServiceItem
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = "Đang cung cấp";

        public string PriceText => $"{Price:N0} VNĐ";
    }
}
