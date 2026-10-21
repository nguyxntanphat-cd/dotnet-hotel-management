using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyRazorApp.Pages.Policies
{
    public class PoliciesModel : PageModel
    {
        public static readonly string[] Categories = { "Chính sách bảo mật", "Quy chế", "Quy định khách sạn" };

        private static List<PolicyItem> _staticPoliciesList = new()
        {
            // ---------- Chính sách bảo mật ----------
            new() { Id = "CS-1001", Title = "Thu thập thông tin cá nhân", Category = "Chính sách bảo mật", Content = "Khách sạn thu thập họ tên, số CCCD/hộ chiếu, số điện thoại, email và thông tin thanh toán khi khách đặt phòng hoặc nhận phòng.\nThông tin chỉ được thu thập phục vụ việc lưu trú, khai báo tạm trú và chăm sóc khách hàng.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" },
            new() { Id = "CS-1002", Title = "Sử dụng và chia sẻ thông tin", Category = "Chính sách bảo mật", Content = "Không bán, trao đổi hoặc chia sẻ thông tin khách hàng cho bên thứ ba vì mục đích thương mại.\nChỉ cung cấp thông tin cho cơ quan nhà nước có thẩm quyền khi có yêu cầu theo quy định pháp luật.\nThông tin chỉ dùng để gửi ưu đãi khi khách đồng ý nhận.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" },
            new() { Id = "CS-1003", Title = "Lưu trữ và bảo vệ dữ liệu", Category = "Chính sách bảo mật", Content = "Dữ liệu khách hàng được lưu trên hệ thống có phân quyền, chỉ nhân viên liên quan mới được truy cập.\nThông tin thẻ thanh toán không được lưu trên hệ thống của khách sạn.\nHồ sơ lưu trú được lưu tối đa 5 năm, sau đó sẽ được xóa hoặc ẩn danh.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" },
            new() { Id = "CS-1004", Title = "Quyền của khách hàng", Category = "Chính sách bảo mật", Content = "Khách có quyền yêu cầu xem, chỉnh sửa hoặc xóa thông tin cá nhân của mình.\nYêu cầu gửi về email privacy@aholahotel.vn, khách sạn phản hồi trong vòng 7 ngày làm việc.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" },
            new() { Id = "CS-1005", Title = "Camera an ninh", Category = "Chính sách bảo mật", Content = "Camera được lắp tại sảnh, hành lang, thang máy và bãi xe; tuyệt đối không lắp trong phòng nghỉ và nhà vệ sinh.\nDữ liệu camera được lưu 30 ngày và chỉ trích xuất khi có sự cố hoặc yêu cầu của cơ quan chức năng.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" },

            // ---------- Quy chế ----------
            new() { Id = "CS-1006", Title = "Quy chế đặt phòng và đặt cọc", Category = "Quy chế", Content = "Khách đặt phòng qua điện thoại, website hoặc trực tiếp tại quầy lễ tân.\nĐặt cọc 30% tổng tiền phòng để giữ phòng; dịp lễ, Tết đặt cọc 50%.\nĐặt phòng không cọc chỉ được giữ đến 18:00 ngày nhận phòng.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" },
            new() { Id = "CS-1007", Title = "Quy chế hủy phòng và hoàn tiền", Category = "Quy chế", Content = "Hủy trước 72 giờ: hoàn 100% tiền cọc.\nHủy từ 24 đến 72 giờ: hoàn 50% tiền cọc.\nHủy trong vòng 24 giờ hoặc không đến: không hoàn cọc.\nTiền hoàn được chuyển lại trong 5 - 7 ngày làm việc.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" },
            new() { Id = "CS-1008", Title = "Quy chế thanh toán", Category = "Quy chế", Content = "Chấp nhận tiền mặt (VNĐ), chuyển khoản, thẻ Visa/Master/JCB và ví điện tử.\nThanh toán thẻ tín dụng quốc tế phụ phí 2%.\nToàn bộ chi phí phòng và dịch vụ được thanh toán khi trả phòng; khách lưu trú trên 7 ngày thanh toán theo tuần.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" },
            new() { Id = "CS-1009", Title = "Quy chế bồi thường tài sản", Category = "Quy chế", Content = "Khách làm hư hỏng, mất mát tài sản của khách sạn phải bồi thường theo giá trị thực tế.\nKhách sạn chịu trách nhiệm với tài sản gửi tại quầy lễ tân có biên nhận; không chịu trách nhiệm với tài sản để trong phòng ngoài két sắt.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" },
            new() { Id = "CS-1010", Title = "Quy chế khách đoàn", Category = "Quy chế", Content = "Đoàn từ 10 phòng trở lên được giảm 10% giá phòng.\nTrưởng đoàn cung cấp danh sách khách trước ngày nhận phòng ít nhất 3 ngày.\nHủy đoàn phải báo trước 7 ngày để được hoàn cọc.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Ngưng áp dụng" },

            // ---------- Quy định khách sạn ----------
            new() { Id = "CS-1011", Title = "Giờ nhận phòng và trả phòng", Category = "Quy định khách sạn", Content = "Nhận phòng từ 14:00, trả phòng trước 12:00.\nTrả phòng trước 15:00: phụ thu 30% giá phòng; từ 15:00 đến 18:00: phụ thu 50%; sau 18:00: tính thêm 1 đêm.\nNhận phòng sớm trước 9:00 phụ thu 50% giá phòng.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" },
            new() { Id = "CS-1012", Title = "Giấy tờ tùy thân và khai báo tạm trú", Category = "Quy định khách sạn", Content = "Khách xuất trình CCCD/hộ chiếu còn hiệu lực khi nhận phòng để khai báo tạm trú.\nKhách nước ngoài xuất trình hộ chiếu và thị thực hợp lệ.\nKhách sạn không nhận khách không có giấy tờ tùy thân.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" },
            new() { Id = "CS-1013", Title = "Trẻ em và giường phụ", Category = "Quy định khách sạn", Content = "Trẻ dưới 6 tuổi ở miễn phí khi ngủ chung giường với bố mẹ.\nTrẻ từ 6 đến 12 tuổi phụ thu 150.000 VNĐ/đêm (bao gồm ăn sáng).\nGiường phụ: 300.000 VNĐ/đêm, tối đa 1 giường phụ mỗi phòng.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" },
            new() { Id = "CS-1014", Title = "Nội quy lưu trú", Category = "Quy định khách sạn", Content = "Không hút thuốc trong phòng, vi phạm phạt 1.000.000 VNĐ.\nGiữ trật tự sau 22:00, không tổ chức tiệc trong phòng.\nKhách đến thăm phải đăng ký tại lễ tân và rời khách sạn trước 22:00.\nKhông mang vũ khí, chất cháy nổ, chất cấm và hàng có mùi nặng (sầu riêng, mắm...) vào khách sạn.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" },
            new() { Id = "CS-1015", Title = "Thú cưng", Category = "Quy định khách sạn", Content = "Khách sạn không nhận thú cưng, trừ chó dẫn đường hỗ trợ người khuyết tật.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" },
            new() { Id = "CS-1016", Title = "An toàn phòng cháy chữa cháy", Category = "Quy định khách sạn", Content = "Không sử dụng bếp, bàn ủi hoặc thiết bị điện công suất lớn mang theo trong phòng.\nKhách đọc sơ đồ thoát hiểm phía sau cửa phòng.\nKhi có chuông báo cháy, di chuyển bằng thang bộ, không dùng thang máy.", EffectiveDate = new DateTime(2026, 1, 1), Status = "Đang áp dụng" }
        };

        public List<PolicyItem> PoliciesList { get; set; } = new();

        // Thống kê không phụ thuộc bộ lọc
        public int TotalCount => _staticPoliciesList.Count;
        public int ActiveCount => _staticPoliciesList.Count(x => x.Status == "Đang áp dụng");
        public int InactiveCount => _staticPoliciesList.Count(x => x.Status == "Ngưng áp dụng");
        public int CategoryCount => _staticPoliciesList.Select(x => x.Category).Distinct().Count();
        public int CountIn(string category) => _staticPoliciesList.Count(x => x.Category == category);

        // Các thuộc tính nhận giá trị từ form tìm kiếm & lọc
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CategoryFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty]
        public PolicyItem NewPolicy { get; set; } = new();

        [BindProperty]
        public PolicyItem EditPolicy { get; set; } = new();

        public void OnGet()
        {
            var query = _staticPoliciesList.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.ToLower();
                query = query.Where(x => x.Id.ToLower().Contains(term) ||
                                         x.Title.ToLower().Contains(term) ||
                                         x.Content.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                query = query.Where(x => x.Category == CategoryFilter);
            }

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                query = query.Where(x => x.Status == StatusFilter);
            }

            // Sắp theo thứ tự nhóm đã khai báo để hiển thị gom nhóm
            PoliciesList = query
                .OrderBy(x => Array.IndexOf(Categories, x.Category) < 0 ? int.MaxValue : Array.IndexOf(Categories, x.Category))
                .ThenBy(x => x.Id)
                .ToList();
        }

        // Sinh mã kế tiếp dựa trên mã lớn nhất (tránh trùng mã sau khi xóa)
        public static string NextId()
        {
            var max = _staticPoliciesList
                .Select(x => int.TryParse(x.Id.Replace("CS-", ""), out var n) ? n : 1000)
                .DefaultIfEmpty(1000)
                .Max();
            return "CS-" + (max + 1);
        }

        public IActionResult OnPostCreate()
        {
            if (NewPolicy != null && !string.IsNullOrWhiteSpace(NewPolicy.Title))
            {
                NewPolicy.Id = NextId();
                NewPolicy.UpdatedAt = DateTime.Now;
                _staticPoliciesList.Add(NewPolicy);
            }
            return RedirectToPage();
        }

        public IActionResult OnPostEdit()
        {
            var policy = _staticPoliciesList.FirstOrDefault(x => x.Id == EditPolicy.Id);
            if (policy != null)
            {
                policy.Title = EditPolicy.Title;
                policy.Category = EditPolicy.Category;
                policy.Content = EditPolicy.Content;
                policy.EffectiveDate = EditPolicy.EffectiveDate;
                policy.Status = EditPolicy.Status;
                policy.UpdatedAt = DateTime.Now;
            }
            return RedirectToPage();
        }

        public IActionResult OnPostToggleStatus(string id)
        {
            var policy = _staticPoliciesList.FirstOrDefault(x => x.Id == id);
            if (policy != null)
            {
                policy.Status = policy.Status == "Đang áp dụng" ? "Ngưng áp dụng" : "Đang áp dụng";
                policy.UpdatedAt = DateTime.Now;
            }
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(string id)
        {
            var policy = _staticPoliciesList.FirstOrDefault(x => x.Id == id);
            if (policy != null)
            {
                _staticPoliciesList.Remove(policy);
            }
            return RedirectToPage();
        }
    }

    public class PolicyItem
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime EffectiveDate { get; set; } = DateTime.Today;
        public string Status { get; set; } = "Đang áp dụng";
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
