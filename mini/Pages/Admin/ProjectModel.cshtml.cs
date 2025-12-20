using LmsMini.WebApp.ApiClients;
using LmsMini.WebApp.ApiClients.Dto.Project; // GIỮ LẠI DÒNG NÀY (CHO Web App DTOs)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic; // Cần cho List<SelectListItem>
using System.Linq;
using System.Threading.Tasks;
// Dòng 'using LmsMini.Application.DTOs.Project;' đã được XÓA để tránh xung đột

namespace LmsMini.WebApp.Pages.Admin
{
    [Authorize(Roles = "Staff,Admin")]
    public class ProjectModel : PageModel
    {
        private readonly IProjectApiClient _projectApi;
        private readonly IStudentApiClient _studentApi;

        // --- Data Input ---
        [BindProperty]
        public CreateProjectDto CreateProjectInput { get; set; } = new CreateProjectDto();

        [BindProperty]
        public ProjectApprovalDto ApprovalInput { get; set; } = new ProjectApprovalDto();

        // --- Dropdown Options (ĐÃ BỔ SUNG) ---
        public List<SelectListItem> MajorOptions { get; set; } = new();

        // --- Status Messages (ĐÃ BỔ SUNG) ---
        [TempData]
        public string Message { get; set; } = "";

        [TempData]
        public string MessageType { get; set; } = "";

        public ProjectModel(IProjectApiClient projectApi, IStudentApiClient studentApi)
        {
            _projectApi = projectApi;
            _studentApi = studentApi;
        }

        public async Task OnGetAsync()
        {
            await LoadDropdownData();
        }

        private async Task LoadDropdownData()
        {
            try
            {
                var majors = await _studentApi.GetMajorsAsync();

                MajorOptions = majors.Select(m =>
                    new SelectListItem(
                        m.MajorName ?? "",
                        m.MajorId.ToString()
                    ))
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR LoadDropdownData] Lỗi tải dropdown Majors: {ex.Message}");
                MajorOptions = new List<SelectListItem>();
            }
        }

        public async Task<IActionResult> OnPostCreateProjectAsync()
        {
            await LoadDropdownData();
            if (!ModelState.IsValid)
            {
                MessageType = "danger";
                Message = "Vui lòng kiểm tra lại thông tin nhập cho Dự án.";
                TempData["ShowCreateProjectModal"] = "true";
                return Page();
            }
            var (isSuccess, error, projectId) = await _projectApi.CreateProjectAsync(CreateProjectInput);
            if (isSuccess)
            {
                MessageType = "success";
                Message = $"Tạo Dự án thành công! ID: {projectId}";
                return RedirectToPage();
            }
            else
            {
                MessageType = "danger";
                Message = $"Lỗi khi tạo Dự án: {error ?? "Lỗi không xác định"}";
                TempData["ShowCreateProjectModal"] = "true";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostStaffApproveAsync()
        {
            await LoadDropdownData();
            if (string.IsNullOrEmpty(ApprovalInput.AssignID) || string.IsNullOrEmpty(ApprovalInput.Decicion))
            {
                MessageType = "danger";
                Message = "Thiếu thông tin Project ID hoặc Quyết định phê duyệt.";
                return Page();
            }
            var (isSuccess, error) = await _projectApi.StaffApproveAsync(ApprovalInput);
            if (isSuccess)
            {
                MessageType = "success";
                Message = $"Đã xử lý phê duyệt (Staff) cho Project {ApprovalInput.AssignID}.";
            }
            else
            {
                MessageType = "danger";
                Message = $"Phê duyệt (Staff) thất bại: {error}";
            }
            return RedirectToPage();
        }

        [Authorize(Roles = "Staff, Admin, Lecturer")]
        public async Task<IActionResult> OnPostLeaderApproveAsync()
        {
            await LoadDropdownData();
            if (string.IsNullOrEmpty(ApprovalInput.AssignID) || string.IsNullOrEmpty(ApprovalInput.Decicion))
            {
                MessageType = "danger";
                Message = "Thiếu thông tin Project ID hoặc Quyết định phê duyệt.";
                return Page();
            }
            var (isSuccess, error) = await _projectApi.LeaderApproveAsync(ApprovalInput);
            if (isSuccess)
            {
                MessageType = "success";
                Message = $"Đã xử lý phê duyệt (Leader) cho Project {ApprovalInput.AssignID}.";
            }
            else
            {
                MessageType = "danger";
                Message = $"Phê duyệt (Leader) thất bại: {error}";
            }
            return RedirectToPage();
        }
    }
}