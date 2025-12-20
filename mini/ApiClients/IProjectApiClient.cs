//using LmsMini.Application.DTOs.Project;
using LmsMini.WebApp.ApiClients.Dto.Project;

namespace LmsMini.WebApp.ApiClients
{
    public interface IProjectApiClient
    {
        // 1. Tạo Dự án
        Task<(bool IsSuccess, string? Error, string? ProjectId)> CreateProjectAsync(CreateProjectDto dto);

        // 2. Phê duyệt (Staff/Admin)
        Task<(bool IsSuccess, string? Error)> StaffApproveAsync(ProjectApprovalDto dto);

        // 3. Phê duyệt (Lecturer/Admin)
        Task<(bool IsSuccess, string? Error)> LeaderApproveAsync(ProjectApprovalDto dto);

        // TODO: Cần thêm GetProjectsAsync để lấy danh sách hiển thị
    }
}