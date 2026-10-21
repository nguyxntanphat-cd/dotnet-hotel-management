using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace MyRazorApp.Pages.Permissions
{
    public class PermissionsModel : PageModel
    {
        public List<PermissionGroupModel> PermissionGroups { get; set; } = new();

        public void OnGet()
        {
            PermissionGroups = new List<PermissionGroupModel>
            {
                new() {
                    GroupName = "Tổng quan",
                    Icon = "bi bi-speedometer2",
                    Permissions = new List<PermissionItem>
                    {
                        new() { Code = "1.1", Name = "Bảng điều khiển (Dashboard)", IsGranted = true }
                    }
                },
                new() {
                    GroupName = "Quản lý hệ thống",
                    Icon = "bi bi-gear-wide-connected",
                    Permissions = new List<PermissionItem>
                    {
                        new() { Code = "1.3", Name = "Tài khoản", IsGranted = true },
                        new() { Code = "1.4", Name = "Nhân viên", IsGranted = true },
                        new() { Code = "1.5", Name = "Phân quyền", IsGranted = true }
                    }
                },
                new() {
                    GroupName = "Quản lý phòng",
                    Icon = "bi bi-door-open",
                    Permissions = new List<PermissionItem>
                    {
                        new() { Code = "2.1", Name = "Loại phòng & bảng giá", IsGranted = true },
                        new() { Code = "2.2", Name = "Sơ đồ phòng khách sạn", IsGranted = true }
                    }
                },
                new() {
                    GroupName = "Khách hàng & đặt phòng",
                    Icon = "bi bi-calendar-check",
                    Permissions = new List<PermissionItem>
                    {
                        new() { Code = "3.1", Name = "Danh sách khách hàng", IsGranted = true },
                        new() { Code = "3.2", Name = "Quản lý đặt phòng", IsGranted = true }
                    }
                },
                new() {
                    GroupName = "Lưu trú & dịch vụ",
                    Icon = "bi bi-box-seam",
                    Permissions = new List<PermissionItem>
                    {
                        new() { Code = "4.1", Name = "Nhận phòng (Check-in)", IsGranted = true },
                        new() { Code = "4.2", Name = "Khách đang lưu trú", IsGranted = true },
                        new() { Code = "4.3", Name = "Danh mục dịch vụ", IsGranted = true },
                        new() { Code = "4.4", Name = "Ghi nhận dịch vụ phát sinh", IsGranted = true }
                    }
                },
                new() {
                    GroupName = "Thanh toán & Báo cáo",
                    Icon = "bi bi-receipt",
                    Permissions = new List<PermissionItem>
                    {
                        new() { Code = "5.1", Name = "Trả phòng & thanh toán", IsGranted = true },
                        new() { Code = "5.6", Name = "Tra cứu hóa đơn", IsGranted = true },
                        new() { Code = "6.1", Name = "Thống kê doanh thu & báo cáo", IsGranted = true }
                    }
                }
            };
        }
    }

    public class PermissionGroupModel
    {
        public string GroupName { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public List<PermissionItem> Permissions { get; set; } = new();
    }

    public class PermissionItem
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsGranted { get; set; }
    }
}