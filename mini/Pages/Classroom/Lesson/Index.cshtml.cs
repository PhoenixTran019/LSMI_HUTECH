using LmsMini.WebApp.ApiClients;
using LmsMini.WebApp.ApiClients.Dto.Lesson;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace LmsMini.WebApp.Pages.Classroom.Lesson
{
    // Giả định quyền truy cập
    [Authorize(Roles = "Staff,Lecturer,Admin,Student")]
    public class ClassroomLessonModel : PageModel
    {
        private readonly ILessonApiClient _lessonApi;

        public ClassroomLessonModel(ILessonApiClient lessonApi)
        {
            _lessonApi = lessonApi;
        }

        [FromRoute]
        [BindProperty(SupportsGet = true)]
        public string ClassroomId { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string LessonId { get; set; } = string.Empty;

        // Dữ liệu hiển thị chi tiết
        public LessonDetailDto? LessonDetail { get; set; }

        // Form tạo mới
        [BindProperty]
        public CreateLessonRequest CreateInput { get; set; } = new();

        // Form cập nhật (Dùng cho modal Edit)
        [BindProperty]
        public UpdateLessonRequest UpdateInput { get; set; } = new();

        [TempData] public string? Message { get; set; }
        [TempData] public string? MessageType { get; set; }

        public bool IsEditMode => !string.IsNullOrEmpty(LessonId);

        // Giả định ClassName có sẵn (có thể từ db hoặc session)
        public string ClassName { get; set; } = "CNTT14A";

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(ClassroomId))
            {
                return NotFound("Classroom ID is required.");
            }

            if (IsEditMode)
            {
                var (detail, error) = await _lessonApi.GetLessonDetailAsync(ClassroomId, LessonId);

                if (detail == null)
                {
                    MessageType = "danger";
                    Message = error ?? "Không tìm thấy bài học.";
                    return RedirectToPage(new { ClassroomId }); // Quay lại danh sách
                }

                LessonDetail = detail;

                // Chuẩn bị UpdateInput nếu cần, hoặc dùng LessonDetail để hiển thị
                UpdateInput = new UpdateLessonRequest
                {
                    Title = detail.Title,
                    Content = detail.Content
                    // NewFiles và RemoveFileName sẽ được điền từ form
                };
            }

            // TODO: Cần có phương thức để Load danh sách Lesson nếu không ở chế độ chi tiết

            return Page();
        }

        // Xử lý tạo bài học mới
        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(ClassroomId))
            {
                MessageType = "danger";
                Message = "Lỗi xác thực hoặc thiếu Class ID.";
                return Page();
            }

            // Chuyển ClassName giả định vào API Call
            var (isSuccess, error, lessonId) = await _lessonApi.CreateLessonAsync(ClassroomId, CreateInput, ClassName);

            if (isSuccess)
            {
                MessageType = "success";
                Message = $"Tạo bài học '{CreateInput.LessonTitle}' thành công!";
                // Chuyển hướng đến trang chi tiết bài học vừa tạo
                return RedirectToPage(new { ClassroomId, LessonId = lessonId });
            }

            MessageType = "danger";
            Message = $"Tạo bài học thất bại: {error}";
            return Page();
        }

        // Xử lý cập nhật bài học
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(ClassroomId) || string.IsNullOrEmpty(LessonId))
            {
                MessageType = "danger";
                Message = "Lỗi xác thực hoặc thiếu ID.";
                return Page();
            }

            // Gán ClassName và LessonName hiện tại/mới vào DTO nếu cần (cho API Controller xử lý path)
            UpdateInput.Classname = ClassName;
            UpdateInput.LessonName = LessonDetail?.Title; // Dùng tên cũ nếu không đổi

            var (isSuccess, error) = await _lessonApi.UpdateLessonAsync(ClassroomId, LessonId, UpdateInput);

            if (isSuccess)
            {
                MessageType = "success";
                Message = "Cập nhật bài học thành công!";
                return RedirectToPage(new { ClassroomId, LessonId });
            }

            MessageType = "danger";
            Message = $"Cập nhật thất bại: {error}";
            return Page();
        }

        // Xử lý xóa bài học
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            if (string.IsNullOrEmpty(ClassroomId) || string.IsNullOrEmpty(LessonId))
            {
                MessageType = "danger";
                Message = "Thiếu ID để xóa.";
                return RedirectToPage(new { ClassroomId });
            }

            var (isSuccess, error) = await _lessonApi.DeleteLessonAsync(ClassroomId, LessonId);

            if (isSuccess)
            {
                MessageType = "success";
                Message = "Xóa bài học thành công.";
                return RedirectToPage(new { ClassroomId }); // Quay lại danh sách
            }

            MessageType = "danger";
            Message = $"Xóa thất bại: {error}";
            return Page();
        }
    }
}