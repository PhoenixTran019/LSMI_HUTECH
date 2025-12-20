// LmsMini.WebApp.ApiClients/IAssignmentApiClient.cs (Cập nhật)

using LmsMini.WebApp.ApiClients.Dto.Assignment;
using System.Collections.Generic;
using System.Threading.Tasks;
using LmsMini.WebApp.ApiClients.Dto.Student;

namespace LmsMini.WebApp.ApiClients
{
    // Cần đảm bảo AssignmentSummaryDto được truy cập
    // Ví dụ: bằng cách thêm using hoặc định nghĩa nó ở đây.

    public interface IAssignmentApiClient
    {
        // Phương thức cho Student Dashboard:
        Task<int> GetPendingAssignmentsCountForStudent(string studentId);

        // PHƯƠNG THỨC CẦN THÊM ĐỂ KHẮC PHỤC LỖI HIỆN TẠI
        Task<List<AssignmentSummaryDto>?> GetAllAssignmentsForStudent(string studentId); // <-- THÊM DÒNG NÀY

        // Phương thức cho Assignment Details:
        Task<AssignmentDetailDto?> GetAssignmentDetailsForStudent(string assignmentId, string studentId);

        // Phương thức quản lý (Staff/Lecturer)
        Task<(StaffAssignmentDetailDto? Detail, string? Error)> StaffGetAssignmentDetailAsync(string classroomId, string assignmentId);
        Task<(bool IsSuccess, string? Error, string? AssignmentId)> CreateAssignmentAsync(string classroomId, CreateAssignmentRequest dto);
        Task<(bool IsSuccess, string? Error)> UpdateAssignmentAsync(string classroomId, string assignmentId, UpdateAssignmentRequest dto);
        Task<(bool IsSuccess, string? Error)> DeleteAssignmentAsync(string classroomId, string assignmentId);
    }
}