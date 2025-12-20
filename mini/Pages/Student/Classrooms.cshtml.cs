// Pages/Student/Classrooms.cshtml.cs (ĐÃ SỬA VÀ HOÀN CHỈNH)

using LmsMini.WebApp.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using LmsMini.WebApp.ApiClients.Dto.Student;

// THÊM DÒNG USING NÀY (Đảm bảo ClassDetailsDto và ClassroomListDto nằm trong namespace này)
// hoặc chính xác hơn: using LmsMini.WebApp.ApiClients.Dto.Classroom; 
// Tạm thời dùng LmsMini.WebApp.ApiClients vì DTO được đặt trong đó trong ví dụ trước.


namespace LmsMini.WebApp.Pages.Student
{
    // DTO đã được xóa ở đây và được truy cập qua using ở trên.

    [Authorize(Roles = "Student")]
    public class ClassroomsModel : PageModel
    {
        private readonly IClassroomApiClient _classroomApiClient;

        // Bây giờ nó tham chiếu đến DTO từ namespace ApiClients
        public List<ClassroomListDto> Classrooms { get; set; } = new List<ClassroomListDto>();

        [TempData]
        public string Message { get; set; }

        public ClassroomsModel(IClassroomApiClient classroomApiClient)
        {
            _classroomApiClient = classroomApiClient;
        }

        public async Task OnGetAsync()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
            {
                Message = "Không tìm thấy thông tin sinh viên.";
                return;
            }

            try
            {
                // Khai báo kiểu TƯỜNG MINH cho kết quả API (có thể null)
                List<ClassroomListDto>? apiResult =
                    await _classroomApiClient.GetClassroomsForStudentAsync(studentId);

                // Bây giờ ?? hoạt động đúng
                Classrooms = apiResult ?? new List<ClassroomListDto>();
            }
            catch (Exception ex)
            {
                Message = $"Lỗi tải danh sách lớp học: {ex.Message}";
            }
        }
    }
}