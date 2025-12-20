using LmsMini.WebApp.ApiClients;
using LmsMini.WebApp.ApiClients.Dto.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace LmsMini.WebApp.Pages.Student
{
    [Authorize(Roles = "Student")]
    public class ClassDetailsModel : PageModel
    {
        private readonly IClassroomApiClient _classroomApiClient;

        // Khởi tạo mặc định để tránh lỗi NullReference trên View
        public ClassDetailsDto ClassDetails { get; set; } = new ClassDetailsDto();

        [TempData]
        public string Message { get; set; } = string.Empty;

        public ClassDetailsModel(IClassroomApiClient classroomApiClient)
        {
            _classroomApiClient = classroomApiClient;
        }

        public async Task<IActionResult> OnGetAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return RedirectToPage("/Student/Classrooms");
            }

            try
            {
                var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(studentId))
                {
                    return RedirectToPage("/Account/Login");
                }

                var data = await _classroomApiClient.GetClassDetailsForStudent(id, studentId);

                if (data == null)
                {
                    Message = "Không tìm thấy lớp học hoặc bạn chưa tham gia lớp này.";
                    return RedirectToPage("/Student/Classrooms");
                }

                ClassDetails = data;

                // Đảm bảo ClassroomId trong DTO khớp với ID trên URL (đề phòng API trả về thiếu)
                if (string.IsNullOrEmpty(ClassDetails.ClassroomId))
                {
                    ClassDetails.ClassroomId = id;
                }
            }
            catch (Exception ex)
            {
                Message = $"Lỗi tải chi tiết lớp học: {ex.Message}";
                return RedirectToPage("/Student/Classrooms");
            }

            return Page();
        }
    }
}