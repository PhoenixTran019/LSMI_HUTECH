using LmsMini.WebApp.ApiClients.Dto.ProjectClassroom;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LmsMini.WebApp.ApiClients
{
    public interface IProjectClassroomApiClient
    {
        // 1. Tạo Project Classroom
        Task<(bool IsSuccess, string? Error, string? ProClassId)> CreateProjectClassroomAsync(
            CreateProjectClassroomRequest dto);

        // 2. Lấy danh sách lớp học của Staff/Lecturer
        Task<List<MyProjectClassroomItem>> GetMyClassroomsAsync();

        // 3. Thêm Giảng viên vào lớp
        Task<(bool IsSuccess, string? Error)> AddLecturerAsync(
            string proClassID,
            AddLecturerRequest dto);

        // 4. Tạo Nội dung/Bài tập (Hỗ trợ File)
        Task<(bool IsSuccess, string? Error)> CreateProjectContentAsync(
            string proClassID,
            CreateProjectContentRequest dto);

        // 5. Lấy Chi tiết Nội dung/Bài tập
        Task<(ProjectContentDetailDto? Detail, string? Error)> GetProjectContentDetailAsync(
            string proClassID,
            string contentId);

        // 6. Cập nhật Nội dung/Bài tập (Hỗ trợ File)
        Task<(bool IsSuccess, string? Error)> UpdateProjectContentAsync(
            string proClassID,
            string contentId,
            ProContentUpdateRequest dto);
    }
}