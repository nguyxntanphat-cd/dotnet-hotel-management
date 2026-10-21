using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace MyRazorApp.Pages
{
    public class AccountsModel : PageModel
    {
        // Khởi tạo danh sách tĩnh an toàn, không bao giờ bị null
        private static List<AccountItem> _staticAccountsList = new()
        {
            new() { Id = "TK001", Username = "admin", FullName = "Nguyễn Tấn Phát", Department = "Quản lý & Hành chính", Position = "Quản trị viên", Role = "Quản trị viên", Status = "Hoạt động" },
            new() { Id = "TK002", Username = "phamthid", FullName = "Phạm Thị D", Department = "Lễ tân", Position = "Trưởng bộ phận", Role = "Lễ tân", Status = "Hoạt động" },
            new() { Id = "TK003", Username = "hoangvane", FullName = "Hoàng Văn E", Department = "Buồng phòng", Position = "Trưởng bộ phận", Role = "Quản lý", Status = "Hoạt động" },
            new() { Id = "TK004", Username = "vuminhf", FullName = "Vũ Minh F", Department = "F&B (Ẩm thực)", Position = "Quản lý Nhà hàng", Role = "Lễ tân", Status = "Đã khóa" },
            new() { Id = "TK005", Username = "dovang", FullName = "Đỗ Văn G", Department = "Kỹ thuật", Position = "Kỹ sư trưởng", Role = "Nhân viên buồng phòng", Status = "Hoạt động" }
        };

        public List<AccountItem> AccountsList { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? RoleFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty]
        public AccountItem NewAccount { get; set; } = new();

        [BindProperty]
        public AccountItem EditAccount { get; set; } = new();

        public void OnGet()
        {
            // Kiểm tra chống null tuyệt đối cho danh sách nguồn
            if (_staticAccountsList == null)
            {
                _staticAccountsList = new List<AccountItem>();
            }

            var query = _staticAccountsList.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.ToLower();
                query = query.Where(x => (!string.IsNullOrEmpty(x.Id) && x.Id.ToLower().Contains(term)) ||
                                         (!string.IsNullOrEmpty(x.Username) && x.Username.ToLower().Contains(term)) ||
                                         (!string.IsNullOrEmpty(x.FullName) && x.FullName.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(RoleFilter))
            {
                query = query.Where(x => x.Role == RoleFilter);
            }

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                query = query.Where(x => x.Status == StatusFilter);
            }

            AccountsList = query.ToList();
        }

        public IActionResult OnPostCreate()
        {
            if (NewAccount != null)
            {
                NewAccount.Id = "TK00" + (_staticAccountsList.Count + 1);
                NewAccount.Department = "Lễ tân";
                NewAccount.Position = "Nhân viên";
                NewAccount.Status = "Hoạt động";
                _staticAccountsList.Add(NewAccount);
            }
            return RedirectToPage();
        }

        public IActionResult OnPostEdit()
        {
            var acc = _staticAccountsList.FirstOrDefault(x => x.Id == EditAccount.Id);
            if (acc != null)
            {
                acc.Role = EditAccount.Role;
                acc.Status = EditAccount.Status;
            }
            return RedirectToPage();
        }

        public IActionResult OnPostToggleStatus(string id)
        {
            var acc = _staticAccountsList.FirstOrDefault(x => x.Id == id);
            if (acc != null)
            {
                acc.Status = acc.Status == "Hoạt động" ? "Đã khóa" : "Hoạt động";
            }
            return RedirectToPage();
        }

        public IActionResult OnPostResetPassword(string id, string newPassword)
        {
            // Xử lý đổi mật khẩu ở đây
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(string id)
        {
            var acc = _staticAccountsList.FirstOrDefault(x => x.Id == id);
            if (acc != null)
            {
                _staticAccountsList.Remove(acc);
            }
            return RedirectToPage();
        }
    }

    public class AccountItem
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}