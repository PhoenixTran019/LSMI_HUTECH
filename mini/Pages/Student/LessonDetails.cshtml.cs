using LmsMini.WebApp.ApiClients;
using LmsMini.WebApp.ApiClients.Dto.Lesson;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace LmsMini.WebApp.Pages.Student
{
    [Authorize(Roles = "Student")]
    public class LessonDetailsModel : PageModel
    {
        private readonly IClassroomApiClient _apiClient;

        public LessonDetailDto Lesson { get; set; } = new();
        public string Message { get; set; } = string.Empty;

        public LessonDetailsModel(IClassroomApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> OnGetAsync(string classroomId, string lessonId)
        {
            if (string.IsNullOrEmpty(classroomId) || string.IsNullOrEmpty(lessonId))
            {
                return RedirectToPage("/Student/Classrooms");
            }

            Lesson = await _apiClient.GetLessonDetailAsync(classroomId, lessonId);

            if (Lesson == null)
            {
                Message = "Không tìm thấy bài giảng hoặc bạn không có quyền truy cập.";
                // Có thể redirect về trang danh sách lớp hoặc hiển thị thông báo
                return Page();
            }

            return Page();
        }

        public async Task<IActionResult> OnGetDownloadFileAsync(string classroomId, string fileId)
        {
            if (string.IsNullOrEmpty(fileId)) return NotFound("File ID is missing.");

            var (stream, contentType, fileName) = await _apiClient.DownloadLessonFileAsync(classroomId, fileId);

            if (stream == null)
            {
                return NotFound("Không thể tải file (File có thể đã bị xóa trên server).");
            }

            return File(stream, contentType, fileName ?? "lesson_file");
        }
    }
}