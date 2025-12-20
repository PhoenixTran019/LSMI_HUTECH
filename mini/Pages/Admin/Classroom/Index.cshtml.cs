using LmsMini.WebApp.ApiClients;
using LmsMini.WebApp.ApiClients.Dto.Assignment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Threading.Tasks;

namespace LmsMini.WebApp.Pages.Classroom.Assignment
{
    [Authorize(Roles = "Staff,Lecturer,Admin")]
    public class AssignmentIndexModel : PageModel
    {
        private readonly IAssignmentApiClient _assignmentApi;

        public AssignmentIndexModel(IAssignmentApiClient assignmentApi)
        {
            _assignmentApi = assignmentApi;
        }

        [FromRoute]
        [BindProperty(SupportsGet = true)]
        public string ClassroomId { get; set; } = string.Empty;

        [FromRoute]
        [BindProperty(SupportsGet = true)]
        public string AssignmentId { get; set; } = string.Empty;

        // Dữ liệu hiển thị chi tiết bài tập (cho Staff/Lecturer)
        public StaffAssignmentDetailDto? AssignmentDetail { get; set; }

        // Form tạo mới
        [BindProperty]
        public CreateAssignmentRequest CreateInput { get; set; } = new();

        // Form cập nhật
        [BindProperty]
        public UpdateAssignmentRequest UpdateInput { get; set; } = new();

        [TempData] public string? Message { get; set; }
        [TempData] public string? MessageType { get; set; }

        public bool IsEditMode => !string.IsNullOrEmpty(AssignmentId);

        // Giả định ClassName có thể lấy được (từ DB hoặc tham số)
        public string ClassName { get; set; } = "UnknownClass";

        public async Task<IActionResult> OnGetAsync()
        {
            if (string.IsNullOrEmpty(ClassroomId))
            {
                MessageType = "danger";
                Message = "Thiếu ID Lớp học.";
                return RedirectToPage("/Admin/Dashboard"); // Chuyển hướng về trang tổng quan nếu thiếu ID
            }

            // Nếu có AssignmentId, tải chi tiết bài tập
            // Nếu có AssignmentId, tải chi tiết bài tập
            if (IsEditMode)
            {
                // Thay thế 'var' bằng định nghĩa kiểu tường minh của tuple
                (StaffAssignmentDetailDto? detail, string? error) =
                    await _assignmentApi.StaffGetAssignmentDetailAsync(ClassroomId, AssignmentId);
                // ^ Bằng cách này, trình biên dịch không cần phải "suy luận" kiểu trả về 
                //   mà bạn đã định nghĩa kiểu rõ ràng.

                if (detail == null)
                {
                    MessageType = "danger";
                    Message = error ?? "Không tìm thấy bài tập.";
                    return RedirectToPage(new { ClassroomId });
                }

                AssignmentDetail = detail;

                // Chuẩn bị form cập nhật
                UpdateInput = new UpdateAssignmentRequest
                {
                    Title = detail.Title,
                    Description = detail.Description,
                    Deadline = detail.Deadline,
                    HomeworkStatus = detail.HomeworkStatus
                    // Các field file sẽ được xử lý riêng
                };
            }

            // TODO: Cần có phương thức để Load danh sách Assignments nếu không ở chế độ chi tiết

            return Page();
        }

        // Xử lý tạo bài tập mới
        public async Task<IActionResult> OnPostCreateAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(ClassroomId))
            {
                MessageType = "danger";
                Message = "Lỗi xác thực hoặc thiếu Class ID.";
                return Page();
            }

            // Gán ClassName trước khi gọi API
            CreateInput.ClassName = this.ClassName;

            var (isSuccess, error, assignmentId) = await _assignmentApi.CreateAssignmentAsync(ClassroomId, CreateInput);

            if (isSuccess)
            {
                MessageType = "success";
                Message = $"Tạo bài tập '{CreateInput.Title}' thành công!";
                // Chuyển hướng đến trang chi tiết bài tập vừa tạo
                return RedirectToPage(new { ClassroomId, AssignmentId = assignmentId });
            }

            MessageType = "danger";
            Message = $"Tạo bài tập thất bại: {error}";
            return Page();
        }

        // Xử lý cập nhật bài tập
        public async Task<IActionResult> OnPostUpdateAsync()
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(ClassroomId) || string.IsNullOrEmpty(AssignmentId))
            {
                MessageType = "danger";
                Message = "Lỗi xác thực hoặc thiếu ID.";
                return Page();
            }

            // Cần gán ClassName và AssignmentName (Tên cũ/mới) cho API xử lý đường dẫn file
            UpdateInput.ClassName = this.ClassName;
            UpdateInput.AssigmentName = AssignmentDetail?.Title; // Sử dụng tên cũ

            var (isSuccess, error) = await _assignmentApi.UpdateAssignmentAsync(ClassroomId, AssignmentId, UpdateInput);

            if (isSuccess)
            {
                MessageType = "success";
                Message = "Cập nhật bài tập thành công!";
                return RedirectToPage(new { ClassroomId, AssignmentId });
            }

            MessageType = "danger";
            Message = $"Cập nhật thất bại: {error}";
            return Page();
        }

        // Xử lý xóa bài tập
        public async Task<IActionResult> OnPostDeleteAsync()
        {
            if (string.IsNullOrEmpty(ClassroomId) || string.IsNullOrEmpty(AssignmentId))
            {
                MessageType = "danger";
                Message = "Thiếu ID để xóa.";
                return RedirectToPage(new { ClassroomId });
            }

            var (isSuccess, error) = await _assignmentApi.DeleteAssignmentAsync(ClassroomId, AssignmentId);

            if (isSuccess)
            {
                MessageType = "success";
                Message = "Xóa bài tập thành công.";
                return RedirectToPage(new { ClassroomId }); // Quay lại danh sách
            }

            MessageType = "danger";
            Message = $"Xóa thất bại: {error}";
            return Page();
        }
    }
}