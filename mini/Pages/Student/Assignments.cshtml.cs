// Pages/Student/Assignments.cshtml.cs

using LmsMini.WebApp.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using LmsMini.WebApp.ApiClients.Dto.Student;

namespace LmsMini.WebApp.Pages.Student
{

    [Authorize(Roles = "Student")]
    public class AssignmentsModel : PageModel
    {
        private readonly IAssignmentApiClient _assignmentApiClient;

        public List<AssignmentSummaryDto> Assignments { get; set; } = new List<AssignmentSummaryDto>();

        [TempData]
        public string Message { get; set; }

        public AssignmentsModel(IAssignmentApiClient assignmentApiClient)
        {
            _assignmentApiClient = assignmentApiClient;
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
                // Khai báo biến cục bộ TƯỜNG MINH (List<AssignmentSummaryDto>?) 
                // để lưu kết quả từ API (có thể là null)
                List<AssignmentSummaryDto>? apiResult =
                    await _assignmentApiClient.GetAllAssignmentsForStudent(studentId);

                // Sử dụng toán tử ?? để đảm bảo Assignments luôn là một List không null
                Assignments = apiResult ?? new List<AssignmentSummaryDto>();

            }
            catch (Exception ex)
            {
                Message = $"Lỗi tải danh sách bài tập: {ex.Message}";
                // Trong trường hợp lỗi, Assignments vẫn giữ giá trị khởi tạo là new List<AssignmentSummaryDto>()
            }
        }
    }
}