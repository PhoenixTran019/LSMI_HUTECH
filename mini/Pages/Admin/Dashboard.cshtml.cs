using LmsMini.WebApp.ApiClients;
using LmsMini.WebApp.ApiClients.Dto.Students;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

using LmsMini.Application.DTOs.Classroom;
using ClassroomDashboardItemDto = LmsMini.WebApp.ApiClients.Dto.ClassroomDashboardItemDto;

namespace LmsMini.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        private readonly IStudentApiClient _studentApi;
        private readonly IClassroomApiClient _classroomApi;
        private readonly IProjectApiClient _projectApi;
        private readonly ICourseApiClient _courseApi;

        public DashboardModel(
            IStudentApiClient studentApi,
            IClassroomApiClient classroomApi,
            IProjectApiClient projectApi,
            ICourseApiClient courseApi)
        {
            _studentApi = studentApi;
            _classroomApi = classroomApi;
            _projectApi = projectApi;
            _courseApi = courseApi;
        }

        // ================= DASHBOARD STATS =================
        public int CoursesCount { get; set; }
        public int StudentsCount { get; set; }
        public int StaffCount { get; set; }
        public int ProjectsCount { get; set; }

        public List<string> ChartLabels { get; set; } = new();
        public List<int> ChartValues { get; set; } = new();
        public List<ActivityItem> RecentActivities { get; set; } = new();

        // ================= DROPDOWNS =================
        public List<SelectListItem> DepartmentOptions { get; set; } = new();
        public List<SelectListItem> ClassOptions { get; set; } = new();
        public List<SelectListItem> MajorOptions { get; set; } = new();

        // ✅ Không có endpoint Subject -> chỉ gợi ý "tên môn" từ dashboard-classrooms
        public List<SelectListItem> SubjectOptions { get; set; } = new();

        // ================= FORMS =================
        [BindProperty, ValidateNever]
        public CreateStudentRequestDto CreateStudentInput { get; set; } = new();

        [BindProperty, ValidateNever]
        public CreateClassroomInputModel CreateClassroomInput { get; set; } = new();

        // ================= TEMP DATA =================
        [TempData] public string? Message { get; set; }
        [TempData] public string? MessageType { get; set; }

        // ========= OPTIONAL CACHE =========
        private static DateTime _dropdownCacheUntil = DateTime.MinValue;
        private static List<SelectListItem>? _cachedClassOptions;
        private static List<SelectListItem>? _cachedDepartmentOptions;
        private static List<SelectListItem>? _cachedMajorOptions;
        private static List<SelectListItem>? _cachedSubjectOptions;

        public async Task OnGetAsync()
        {
            await LoadDashboardStatsAsync();
            await LoadDropdownsAsync();
            LoadMockActivities();
        }

        private Task LoadDashboardStatsAsync()
        {
            CoursesCount = 12;
            StudentsCount = 240;
            StaffCount = 18;
            ProjectsCount = 5;

            ChartLabels = new() { "T-6", "T-5", "T-4", "T-3", "T-2", "T-1", "Today" };
            ChartValues = new() { 3, 5, 2, 6, 4, 7, 5 };

            return Task.CompletedTask;
        }

        private void LoadMockActivities()
        {
            RecentActivities = new()
            {
                new ActivityItem("Tạo classroom CNTT01", "1 giờ trước"),
                new ActivityItem("Sinh viên SV001 đăng ký môn", "3 giờ trước"),
                new ActivityItem("Staff duyệt Dự án PRO002", "4 giờ trước")
            };
        }

        public async Task<IActionResult> OnPostCreateStudentAsync()
        {
            ModelState.Clear();
            TryValidateModel(CreateStudentInput, nameof(CreateStudentInput));

            if (!ModelState.IsValid)
            {
                await ReloadPageStateForReturnAsync(showStudentModal: true);
                MessageType = "danger";
                Message = "Vui lòng kiểm tra lại thông tin sinh viên.";
                TempData["ShowCreateStudentModal"] = "true";
                return Page();
            }

            var result = await _studentApi.CreateStudentAsync(CreateStudentInput);

            if (result.IsSuccess)
            {
                MessageType = "success";
                Message = "Tạo sinh viên thành công.";
                return RedirectToPage();
            }

            await ReloadPageStateForReturnAsync(showStudentModal: true);
            MessageType = "danger";
            Message = result.Error;
            TempData["ShowCreateStudentModal"] = "true";
            return Page();
        }

        public async Task<IActionResult> OnPostCreateClassroomAsync()
        {
            ModelState.Clear();
            TryValidateModel(CreateClassroomInput, nameof(CreateClassroomInput));

            if (!ModelState.IsValid)
            {
                await ReloadPageStateForReturnAsync(showClassroomModal: true);
                MessageType = "danger";
                Message = "Vui lòng kiểm tra lại thông tin classroom.";
                TempData["ShowCreateClassroomModal"] = "true";
                return Page();
            }

            // ✅ Swagger đã xác nhận ClassSub là "string" (tên môn)
            var dto = new CreateClassroomDto
            {
                ClassName = CreateClassroomInput.ClassName!,
                ClassSub = CreateClassroomInput.ClassSub!,     // <-- TÊN MÔN
                MainClass = CreateClassroomInput.MainClass!,
                Description = CreateClassroomInput.Description!,
                InviteCode = CreateClassroomInput.InviteCode!,
                ClassStatus = CreateClassroomInput.ClassStatus ?? "Inactive"
            };

            var result = await _classroomApi.CreateClassroomAsync(dto);

            if (result.IsSuccess)
            {
                MessageType = "success";
                Message = "Tạo classroom thành công.";
                return RedirectToPage();
            }

            await ReloadPageStateForReturnAsync(showClassroomModal: true);
            MessageType = "danger";
            Message = result.Error;
            TempData["ShowCreateClassroomModal"] = "true";
            return Page();
        }

        private async Task LoadDropdownsAsync()
        {
            if (DateTime.UtcNow <= _dropdownCacheUntil
                && _cachedClassOptions != null
                && _cachedDepartmentOptions != null
                && _cachedMajorOptions != null
                && _cachedSubjectOptions != null)
            {
                ClassOptions = _cachedClassOptions;
                DepartmentOptions = _cachedDepartmentOptions;
                MajorOptions = _cachedMajorOptions;
                SubjectOptions = _cachedSubjectOptions;
                return;
            }

            var classes = await _studentApi.GetClassesAsync();
            var depts = await _studentApi.GetDepartmentsAsync();
            var majors = await _studentApi.GetMajorsAsync();

            ClassOptions = classes.Select(c => new SelectListItem(c.ClassName, c.ClassId.ToString())).ToList();
            DepartmentOptions = depts.Select(d => new SelectListItem(d.DepartName, d.DepartId.ToString())).ToList();
            MajorOptions = majors.Select(m => new SelectListItem(m.MajorName, m.MajorId.ToString())).ToList();

            // ✅ Gợi ý "tên môn" từ các classroom đang có (dashboard-classrooms)
            SubjectOptions = new List<SelectListItem>();
            var subjectNames = await GetSubjectNameSuggestionsAsync();
            SubjectOptions.AddRange(subjectNames.Select(s => new SelectListItem(s, s)));

            _cachedClassOptions = ClassOptions;
            _cachedDepartmentOptions = DepartmentOptions;
            _cachedMajorOptions = MajorOptions;
            _cachedSubjectOptions = SubjectOptions;
            _dropdownCacheUntil = DateTime.UtcNow.AddMinutes(10);
        }

        private async Task<List<string>> GetSubjectNameSuggestionsAsync()
        {
            try
            {
                var items = await _classroomApi.GetDashboardClassroomsAsync(new ClassroomFilterDto());
                return items
                    .Select(x => x.ClassSub)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Distinct()
                    .OrderBy(s => s)
                    .ToList();
            }
            catch
            {
                return new();
            }
        }

        private async Task ReloadPageStateForReturnAsync(bool showStudentModal = false, bool showClassroomModal = false)
        {
            await LoadDashboardStatsAsync();
            await LoadDropdownsAsync();
            LoadMockActivities();

            if (showStudentModal) TempData["ShowCreateStudentModal"] = "true";
            if (showClassroomModal) TempData["ShowCreateClassroomModal"] = "true";
        }

        public record ActivityItem(string Message, string TimeAgo);
    }

    public class CreateClassroomInputModel
    {
        [Required(ErrorMessage = "Tên lớp là bắt buộc.")]
        public string? ClassName { get; set; }

        // ✅ Tên môn (string) theo Swagger
        [Required(ErrorMessage = "Môn học là bắt buộc.")]
        public string? ClassSub { get; set; }

        [Required(ErrorMessage = "Lớp chính là bắt buộc.")]
        public string? MainClass { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "InviteCode là bắt buộc.")]
        public string? InviteCode { get; set; }

        public string? ClassStatus { get; set; }
    }
}
