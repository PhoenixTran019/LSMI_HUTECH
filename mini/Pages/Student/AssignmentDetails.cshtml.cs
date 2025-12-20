using LmsMini.WebApp.ApiClients;
using LmsMini.WebApp.ApiClients.Dto;
using LmsMini.WebApp.ApiClients.Dto.Student;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LmsMini.WebApp.Pages.Student
{
    // Tăng giới hạn upload file cho trang này để tránh lỗi 400 (Bad Request)
    [RequestSizeLimit(200 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 200 * 1024 * 1024)]
    [Authorize(Roles = "Student")]
    public class AssignmentDetailsModel : PageModel
    {
        private readonly IClassroomApiClient _apiClient;

        [BindProperty]
        public List<IFormFile> UploadFiles { get; set; } = new();

        public StudentAssignmentDetailDto Assignment { get; set; } = new();

        public string Message { get; set; } = string.Empty;

        [TempData]
        public string SuccessMessage { get; set; }

        public AssignmentDetailsModel(IClassroomApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<IActionResult> OnGetAsync(string classroomId, string id)
        {
            if (string.IsNullOrEmpty(classroomId) || string.IsNullOrEmpty(id))
            {
                return RedirectToPage("/Student/Classrooms");
            }

            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Gọi API lấy chi tiết bài tập
            Assignment = await _apiClient.GetStudentAssignmentDetailAsync(classroomId, id, studentId);

            if (Assignment == null)
            {
                Message = "Không tìm thấy bài tập hoặc bạn không có quyền truy cập.";
                return Page(); // Hiển thị trang với thông báo lỗi
            }

            return Page();
        }

        // Handler nộp bài (POST)
        public async Task<IActionResult> OnPostSubmitAsync(string classroomId, string id)
        {
            // Validate file đầu vào
            if (UploadFiles == null || UploadFiles.Count == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn ít nhất một file để nộp.");
                // Load lại data để hiển thị lại trang
                return await OnGetAsync(classroomId, id);
            }

            // Chuyển đổi IFormFile sang DTO để gửi qua API Client
            var filesToSend = new List<BrowserFileDto>();
            foreach (var file in UploadFiles)
            {
                if (file.Length > 0)
                {
                    filesToSend.Add(new BrowserFileDto
                    {
                        FileName = file.FileName,
                        ContentType = file.ContentType,
                        Content = file.OpenReadStream()
                    });
                }
            }

            // Gọi API nộp bài
            var (success, msg) = await _apiClient.SubmitAssignmentAsync(classroomId, id, filesToSend);

            if (success)
            {
                SuccessMessage = "Nộp bài thành công!";
                // Redirect để tránh resubmit khi F5 và để load lại trạng thái mới
                return RedirectToPage(new { classroomId, id });
            }
            else
            {
                Message = msg ?? "Nộp bài thất bại. Vui lòng thử lại.";
                return await OnGetAsync(classroomId, id); // Load lại trang kèm thông báo lỗi
            }
        }

        // Handler tải file đề bài
        public async Task<IActionResult> OnGetDownloadFileAsync(string classroomId, string fileId)
        {
            var (stream, contentType, fileName) = await _apiClient.DownloadAssignmentFileAsync(classroomId, fileId);

            if (stream == null)
            {
                return NotFound("File không tồn tại trên máy chủ.");
            }

            return File(stream, contentType, fileName ?? "file_de_bai");
        }

        // Handler tải file bài làm (đã nộp)
        public async Task<IActionResult> OnGetDownloadSubmissionAsync(string classroomId, string fileId)
        {
            var (stream, contentType, fileName) = await _apiClient.DownloadSubmissionFileAsync(classroomId, fileId);

            if (stream == null)
            {
                return NotFound("File bài làm không tồn tại.");
            }

            return File(stream, contentType, fileName ?? "file_bai_lam");
        }
    }
}