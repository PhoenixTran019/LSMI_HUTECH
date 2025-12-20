// Pages/Student/Dashboard.cshtml.cs (PHIÊN BẢN ĐÃ LOẠI BỎ CÁC LỆNH GỌI API BỊ THIẾU)

using LmsMini.WebApp.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using System;
using LmsMini.WebApp.ApiClients.Dto.Students;
using LmsMini.WebApp.ApiClients.Dto.Student;
using System.Threading.Tasks;

namespace LmsMini.WebApp.Pages.Student
{
    public class StudentDashboardSummaryDto
    {
        public string StudentId { get; set; } = "N/A";
        public string FullName { get; set; } = "Người dùng ẩn danh";
        public int TotalClassrooms { get; set; } = 0;      // <--- Gán giá trị mặc định 0
        public int PendingAssignments { get; set; } = 0;   // <--- Gán giá trị mặc định 0
    }

    [Authorize(Roles = "Student")]
    public class DashboardModel : PageModel
    {
        // Vẫn giữ các Dependency Injection, nhưng sẽ không sử dụng chúng
        private readonly IStudentApiClient _studentApiClient;
        private readonly IClassroomApiClient _classroomApiClient;
        private readonly IAssignmentApiClient _assignmentApiClient;

        public StudentDashboardSummaryDto DashboardData { get; set; } = new StudentDashboardSummaryDto();

        [TempData]
        public string Message { get; set; } = string.Empty;

        public DashboardModel(IStudentApiClient studentApiClient,
                              IClassroomApiClient classroomApiClient,
                              IAssignmentApiClient assignmentApiClient)
        {
            _studentApiClient = studentApiClient;
            _classroomApiClient = classroomApiClient;
            _assignmentApiClient = assignmentApiClient;
        }

        public async Task OnGetAsync()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var studentNameClaim = User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(studentId))
            {
                Message = "Không tìm thấy thông tin định danh người dùng.";
                return;
            }

            DashboardData.StudentId = studentId;

            // Nếu bạn không có API Profile, chúng ta sử dụng Claim Name (Mã số SV) cho FullName
            DashboardData.FullName = string.IsNullOrEmpty(studentNameClaim) ? "Sinh viên" : studentNameClaim;

           
        }
    }
}