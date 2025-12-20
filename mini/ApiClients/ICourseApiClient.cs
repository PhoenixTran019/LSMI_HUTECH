using LmsMini.WebApp.ApiClients.Dto.Course;

namespace LmsMini.WebApp.ApiClients
{
    public interface ICourseApiClient
    {
        // 1. Lấy chi tiết khóa học theo ID
        Task<(CourseDto? Course, string? Error)> GetCourseByIdAsync(Guid courseId);

        // 2. Lấy danh sách khóa học
        Task<List<CourseDto>> ListCoursesAsync();
    }
}