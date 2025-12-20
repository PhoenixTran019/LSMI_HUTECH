using LmsMini.WebApp.ApiClients;
using LmsMini.WebApp.ApiClients.Dto.Students;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace LmsMini.WebApp.Pages.Admin.Student
{
    [Authorize(Roles = "Staff,Admin")]
    public class IndexModel : PageModel
    {
        private readonly IStudentApiClient _studentApi;

        public IndexModel(IStudentApiClient studentApi)
        {
            _studentApi = studentApi;
        }

        public List<SelectListItem> Classes { get; set; } = new();
        public List<SelectListItem> Departments { get; set; } = new();
        public List<SelectListItem> Majors { get; set; } = new();

        [BindProperty]
        public CreateStudentRequestDto NewStudent { get; set; } = new();

        public IList<StudentDto> Students { get; set; } = new List<StudentDto>();

        [TempData] public string Message { get; set; } = "";
        [TempData] public string MessageType { get; set; } = "";

        // OPTIONAL CACHE (10 phút)
        private static DateTime _cacheUntil = DateTime.MinValue;
        private static List<SelectListItem>? _cacheClasses;
        private static List<SelectListItem>? _cacheDepartments;
        private static List<SelectListItem>? _cacheMajors;

        private async Task LoadDropdownDataAsync()
        {
            if (DateTime.UtcNow <= _cacheUntil
                && _cacheClasses != null
                && _cacheDepartments != null
                && _cacheMajors != null)
            {
                Classes = _cacheClasses;
                Departments = _cacheDepartments;
                Majors = _cacheMajors;
                return;
            }

            var classesDto = await _studentApi.GetClassesAsync();
            Classes = classesDto.Select(c => new SelectListItem(c.ClassName, c.ClassId)).ToList();

            var departsDto = await _studentApi.GetDepartmentsAsync();
            Departments = departsDto.Select(d => new SelectListItem(d.DepartName, d.DepartId)).ToList();

            var majorsDto = await _studentApi.GetMajorsAsync();
            Majors = majorsDto.Select(m => new SelectListItem(m.MajorName, m.MajorId)).ToList();

            _cacheClasses = Classes;
            _cacheDepartments = Departments;
            _cacheMajors = Majors;
            _cacheUntil = DateTime.UtcNow.AddMinutes(10);
        }

        public async Task OnGetAsync()
        {
            await LoadDropdownDataAsync();

            if (string.IsNullOrEmpty(Message))
            {
                MessageType = "warning";
                Message = "Tính năng xem danh sách sinh viên hiện không khả dụng (API endpoint /students bị thiếu).";
            }

            Students = new List<StudentDto>();
        }

        public async Task<IActionResult> OnPostCreateStudentAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownDataAsync();
                MessageType = "danger";
                Message = "Lỗi xác thực dữ liệu. Vui lòng kiểm tra lại form.";
                TempData["ShowCreateStudentModal"] = "true";
                return Page();
            }

            var (isSuccess, error) = await _studentApi.CreateStudentAsync(NewStudent);

            if (isSuccess)
            {
                MessageType = "success";
                Message = "Tạo sinh viên và tài khoản thành công!";
                return RedirectToPage();
            }

            // ✅ Nếu API fail mà bạn muốn thấy lỗi ngay + giữ modal mở
            await LoadDropdownDataAsync();
            MessageType = "danger";
            Message = $"Tạo sinh viên thất bại. Lỗi: {error}";
            TempData["ShowCreateStudentModal"] = "true";
            return Page();
        }

    }
}
