// LmsMini.WebApp.Pages.Student/JoinClassroom.cshtml.cs (CẦN CẬP NHẬT)

using LmsMini.WebApp.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LmsMini.WebApp.Pages.Student
{
    // Cấu trúc Input Model (Giả định)
    public class JoinClassroomInputModel
    {
        [Required(ErrorMessage = "Mã mời là bắt buộc.")]
        [StringLength(10, MinimumLength = 5, ErrorMessage = "Mã mời phải có từ 5 đến 10 ký tự.")]
        [Display(Name = "Mã mời")]
        public string InviteCode { get; set; } = string.Empty;
    }

    [Authorize(Roles = "Student")]
    public class JoinClassroomModel : PageModel
    {
        private readonly IClassroomApiClient _classroomApiClient;

        [BindProperty]
        public JoinClassroomInputModel Input { get; set; } = new JoinClassroomInputModel();

        [TempData]
        public string Message { get; set; } = string.Empty;

        public JoinClassroomModel(IClassroomApiClient classroomApiClient)
        {
            _classroomApiClient = classroomApiClient;
        }

        public void OnGet()
        {
            // Reset message khi người dùng truy cập trang
            Message = TempData["SuccessMessage"] as string ?? "";
        }

        // Luôn sử dụng OnPostAsync để xử lý form submit
        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Kiểm tra Validation phía Client (Server-side validation)
            if (!ModelState.IsValid)
            {
                // Nếu validation thất bại, giữ nguyên trang và hiển thị lỗi
                return Page();
            }

            // 2. Lấy Student ID từ Token (để truyền vào Client API)
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(studentId))
            {
                Message = "Lỗi xác thực: Không tìm thấy ID người dùng.";
                return RedirectToPage("/Account/Login");
            }

            // 3. Gọi API Backend
            var (isSuccess, error, classroomId) = await _classroomApiClient.JoinClassroomAsync(studentId, Input.InviteCode.Trim());

            if (isSuccess && !string.IsNullOrEmpty(classroomId))
            {
                // THÀNH CÔNG: Chuyển hướng đến trang chi tiết lớp học
                TempData["SuccessMessage"] = $"Tham gia lớp học thành công! Mã lớp: {Input.InviteCode}";
                return RedirectToPage("/Student/ClassDetails", new { id = classroomId });
            }
            else
            {
                // THẤT BẠI: Hiển thị lỗi từ API hoặc lỗi mặc định
                string finalError = error ?? "Tham gia lớp học thất bại, vui lòng kiểm tra mã mời và thử lại.";
                ModelState.AddModelError(string.Empty, finalError);
                Message = finalError;
                return Page(); // <-- Luôn trả về Page() khi xử lý POST thất bại
            }
        }
    }
}