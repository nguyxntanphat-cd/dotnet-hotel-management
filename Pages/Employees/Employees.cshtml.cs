using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace MyRazorApp.Pages.Employees
{
    public class EmployeesModel : PageModel
    {
        private static List<EmployeeItem> _staticEmployeesList = new()
        {
            new() { Id = "NV-1001", FullName = "Phạm Thị D", Email = "phamthid@aholahotel.vn", Phone = "0901 234 567", Ext = "101", Department = "Lễ tân", Position = "Trưởng bộ phận", JoinDate = "15/03/2022", Status = "Đang làm việc" },
            new() { Id = "NV-1002", FullName = "Hoàng Văn E", Email = "hoangvane@aholahotel.vn", Phone = "0912 345 678", Ext = "102", Department = "Buồng phòng", Position = "Trưởng bộ phận", JoinDate = "10/08/2021", Status = "Đang làm việc" },
            new() { Id = "NV-1003", FullName = "Vũ Minh F", Email = "vuminhf@aholahotel.vn", Phone = "0923 456 789", Ext = "103", Department = "F&B (Ẩm thực)", Position = "Quản lý Nhà hàng", JoinDate = "01/12/2023", Status = "Đang nghỉ phép" },
            new() { Id = "NV-1004", FullName = "Đỗ Văn G", Email = "dovang@aholahotel.vn", Phone = "0934 567 890", Ext = "104", Department = "Kỹ thuật", Position = "Kỹ sư trưởng", JoinDate = "05/05/2020", Status = "Đang làm việc" }
        };

        public List<EmployeeItem> EmployeesList { get; set; } = new();

        // Các thuộc tính nhận giá trị từ form tìm kiếm & lọc
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DepartmentFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty]
        public EmployeeItem NewEmp { get; set; } = new();

        [BindProperty]
        public EmployeeItem EditEmp { get; set; } = new();

        public void OnGet()
        {
            if (_staticEmployeesList == null)
            {
                _staticEmployeesList = new List<EmployeeItem>();
            }

            var query = _staticEmployeesList.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                var term = SearchTerm.ToLower();
                query = query.Where(x => (!string.IsNullOrEmpty(x.Id) && x.Id.ToLower().Contains(term)) ||
                                         (!string.IsNullOrEmpty(x.FullName) && x.FullName.ToLower().Contains(term)) ||
                                         (!string.IsNullOrEmpty(x.Email) && x.Email.ToLower().Contains(term)) ||
                                         (!string.IsNullOrEmpty(x.Phone) && x.Phone.Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(DepartmentFilter))
            {
                query = query.Where(x => x.Department == DepartmentFilter);
            }

            if (!string.IsNullOrWhiteSpace(StatusFilter))
            {
                query = query.Where(x => x.Status == StatusFilter);
            }

            EmployeesList = query.ToList();
        }

        public IActionResult OnPostCreate()
        {
            if (NewEmp != null)
            {
                NewEmp.Id = "NV-100" + (_staticEmployeesList.Count + 1);
                _staticEmployeesList.Add(NewEmp);
            }
            return RedirectToPage();
        }

        public IActionResult OnPostEdit()
        {
            var emp = _staticEmployeesList.FirstOrDefault(x => x.Id == EditEmp.Id);
            if (emp != null)
            {
                emp.FullName = EditEmp.FullName;
                emp.Email = EditEmp.Email;
                emp.Phone = EditEmp.Phone;
                emp.Ext = EditEmp.Ext;
                emp.Department = EditEmp.Department;
                emp.Position = EditEmp.Position;
                emp.JoinDate = EditEmp.JoinDate;
                emp.Status = EditEmp.Status;
            }
            return RedirectToPage();
        }

        public IActionResult OnPostToggleStatus(string id)
        {
            var emp = _staticEmployeesList.FirstOrDefault(x => x.Id == id);
            if (emp != null)
            {
                emp.Status = emp.Status == "Đang làm việc" ? "Đang nghỉ phép" : "Đang làm việc";
            }
            return RedirectToPage();
        }

        public IActionResult OnPostDelete(string id)
        {
            var emp = _staticEmployeesList.FirstOrDefault(x => x.Id == id);
            if (emp != null)
            {
                _staticEmployeesList.Remove(emp);
            }
            return RedirectToPage();
        }
    }

    public class EmployeeItem
    {
        public string Id { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Ext { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string JoinDate { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}